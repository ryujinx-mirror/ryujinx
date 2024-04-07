using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Parameter.Performance;
using Ryujinx.Audio.Renderer.Server.Effect;
using Ryujinx.Audio.Renderer.Server.MemoryPool;
using Ryujinx.Audio.Renderer.Server.Mix;
using Ryujinx.Audio.Renderer.Server.Performance;
using Ryujinx.Audio.Renderer.Server.Sink;
using Ryujinx.Audio.Renderer.Server.Splitter;
using Ryujinx.Audio.Renderer.Server.Voice;
using Ryujinx.Audio.Renderer.Utils;
using Ryujinx.Common.Extensions;
using Ryujinx.Common.Logging;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Ryujinx.Audio.Renderer.Common.BehaviourParameter;

namespace Ryujinx.Audio.Renderer.Server
{
    public ref struct StateUpdater
    {
        private SequenceReader<byte> _inputReader;

        private readonly ReadOnlyMemory<byte> _outputOrigin;

        private Memory<byte> _output;
        private readonly uint _processHandle;
        private BehaviourContext _behaviourContext;

        private readonly ref readonly UpdateDataHeader _inputHeader;
        private readonly Memory<UpdateDataHeader> _outputHeader;

        private readonly ref UpdateDataHeader OutputHeader => ref _outputHeader.Span[0];

        public StateUpdater(ReadOnlySequence<byte> input, Memory<byte> output, uint processHandle, BehaviourContext behaviourContext)
        {
            _inputReader = new SequenceReader<byte>(input);
            _output = output;
            _outputOrigin = _output;
            _processHandle = processHandle;
            _behaviourContext = behaviourContext;

            _inputHeader = ref _inputReader.GetRefOrRefToCopy<UpdateDataHeader>(out _);

            _outputHeader = SpanMemoryManager<UpdateDataHeader>.Cast(_output[..Unsafe.SizeOf<UpdateDataHeader>()]);
            OutputHeader.Initialize(_behaviourContext.UserRevision);
            _output = _output[Unsafe.SizeOf<UpdateDataHeader>()..];
        }

        public ResultCode UpdateBehaviourContext()
        {
            ref readonly BehaviourParameter parameter = ref _inputReader.GetRefOrRefToCopy<BehaviourParameter>(out _);

            if (!BehaviourContext.CheckValidRevision(parameter.UserRevision) || parameter.UserRevision != _behaviourContext.UserRevision)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            _behaviourContext.ClearError();
            _behaviourContext.UpdateFlags(parameter.Flags);

            if (_inputHeader.BehaviourSize != Unsafe.SizeOf<BehaviourParameter>())
            {
                return ResultCode.InvalidUpdateInfo;
            }

            return ResultCode.Success;
        }

        public ResultCode UpdateMemoryPools(Span<MemoryPoolState> memoryPools)
        {
            PoolMapper mapper = new(_processHandle, _behaviourContext.IsMemoryPoolForceMappingEnabled());

            if (memoryPools.Length * Unsafe.SizeOf<MemoryPoolInParameter>() != _inputHeader.MemoryPoolsSize)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            foreach (ref MemoryPoolState memoryPool in memoryPools)
            {
                ref readonly MemoryPoolInParameter parameter = ref _inputReader.GetRefOrRefToCopy<MemoryPoolInParameter>(out _);

                ref MemoryPoolOutStatus outStatus = ref SpanIOHelper.GetWriteRef<MemoryPoolOutStatus>(ref _output)[0];

                PoolMapper.UpdateResult updateResult = mapper.Update(ref memoryPool, in parameter, ref outStatus);

                if (updateResult != PoolMapper.UpdateResult.Success &&
                    updateResult != PoolMapper.UpdateResult.MapError &&
                    updateResult != PoolMapper.UpdateResult.UnmapError)
                {
                    if (updateResult != PoolMapper.UpdateResult.InvalidParameter)
                    {
                        throw new InvalidOperationException($"{updateResult}");
                    }

                    return ResultCode.InvalidUpdateInfo;
                }
            }

            OutputHeader.MemoryPoolsSize = (uint)(Unsafe.SizeOf<MemoryPoolOutStatus>() * memoryPools.Length);
            OutputHeader.TotalSize += OutputHeader.MemoryPoolsSize;

            return ResultCode.Success;
        }

        public ResultCode UpdateVoiceChannelResources(VoiceContext context)
        {
            if (context.GetCount() * Unsafe.SizeOf<VoiceChannelResourceInParameter>() != _inputHeader.VoiceResourcesSize)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            for (int i = 0; i < context.GetCount(); i++)
            {
                ref readonly VoiceChannelResourceInParameter parameter = ref _inputReader.GetRefOrRefToCopy<VoiceChannelResourceInParameter>(out _);

                ref VoiceChannelResource resource = ref context.GetChannelResource(i);

                resource.Id = parameter.Id;
                parameter.Mix.AsSpan().CopyTo(resource.Mix.AsSpan());
                resource.IsUsed = parameter.IsUsed;
            }

            return ResultCode.Success;
        }

        public ResultCode UpdateVoices(VoiceContext context, PoolMapper mapper)
        {
            if (context.GetCount() * Unsafe.SizeOf<VoiceInParameter>() != _inputHeader.VoicesSize)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            int initialOutputSize = _output.Length;

            long initialInputConsumed = _inputReader.Consumed;

            // First make everything not in use.
            for (int i = 0; i < context.GetCount(); i++)
            {
                ref VoiceState state = ref context.GetState(i);

                state.InUse = false;
            }

            Memory<VoiceUpdateState>[] voiceUpdateStatesArray = ArrayPool<Memory<VoiceUpdateState>>.Shared.Rent(Constants.VoiceChannelCountMax);

            Span<Memory<VoiceUpdateState>> voiceUpdateStates = voiceUpdateStatesArray.AsSpan(0, Constants.VoiceChannelCountMax);

            // Start processing
            for (int i = 0; i < context.GetCount(); i++)
            {
                ref readonly VoiceInParameter parameter = ref _inputReader.GetRefOrRefToCopy<VoiceInParameter>(out _);

                voiceUpdateStates.Fill(Memory<VoiceUpdateState>.Empty);

                ref VoiceOutStatus outStatus = ref SpanIOHelper.GetWriteRef<VoiceOutStatus>(ref _output)[0];

                if (parameter.InUse)
                {
                    ref VoiceState currentVoiceState = ref context.GetState(i);

                    for (int channelResourceIndex = 0; channelResourceIndex < parameter.ChannelCount; channelResourceIndex++)
                    {
                        int channelId = parameter.ChannelResourceIds[channelResourceIndex];

                        Debug.Assert(channelId >= 0 && channelId < context.GetCount());

                        voiceUpdateStates[channelResourceIndex] = context.GetUpdateStateForCpu(channelId);
                    }

                    if (parameter.IsNew)
                    {
                        currentVoiceState.Initialize();
                    }

                    currentVoiceState.UpdateParameters(out ErrorInfo updateParameterError, in parameter, mapper, ref _behaviourContext);

                    if (updateParameterError.ErrorCode != ResultCode.Success)
                    {
                        _behaviourContext.AppendError(ref updateParameterError);
                    }

                    currentVoiceState.UpdateWaveBuffers(out ErrorInfo[] waveBufferUpdateErrorInfos, in parameter, voiceUpdateStates, mapper, ref _behaviourContext);

                    foreach (ref ErrorInfo errorInfo in waveBufferUpdateErrorInfos.AsSpan())
                    {
                        if (errorInfo.ErrorCode != ResultCode.Success)
                        {
                            _behaviourContext.AppendError(ref errorInfo);
                        }
                    }

                    currentVoiceState.WriteOutStatus(ref outStatus, in parameter, voiceUpdateStates);
                }
            }

            ArrayPool<Memory<VoiceUpdateState>>.Shared.Return(voiceUpdateStatesArray);

            int currentOutputSize = _output.Length;

            OutputHeader.VoicesSize = (uint)(Unsafe.SizeOf<VoiceOutStatus>() * context.GetCount());
            OutputHeader.TotalSize += OutputHeader.VoicesSize;

            Debug.Assert((initialOutputSize - currentOutputSize) == OutputHeader.VoicesSize);

            _inputReader.SetConsumed(initialInputConsumed + _inputHeader.VoicesSize);

            return ResultCode.Success;
        }

        private static void ResetEffect<T>(ref BaseEffect effect, in T parameter, PoolMapper mapper) where T : unmanaged, IEffectInParameter
        {
            effect.ForceUnmapBuffers(mapper);

            effect = parameter.Type switch
            {
                EffectType.Invalid => new BaseEffect(),
                EffectType.BufferMix => new BufferMixEffect(),
                EffectType.AuxiliaryBuffer => new AuxiliaryBufferEffect(),
                EffectType.Delay => new DelayEffect(),
                EffectType.Reverb => new ReverbEffect(),
                EffectType.Reverb3d => new Reverb3dEffect(),
                EffectType.BiquadFilter => new BiquadFilterEffect(),
                EffectType.Limiter => new LimiterEffect(),
                EffectType.CaptureBuffer => new CaptureBufferEffect(),
                EffectType.Compressor => new CompressorEffect(),
                _ => throw new NotImplementedException($"EffectType {parameter.Type} not implemented!"),
            };
        }

        public ResultCode UpdateEffects(EffectContext context, bool isAudioRendererActive, PoolMapper mapper)
        {
            if (_behaviourContext.IsEffectInfoVersion2Supported())
            {
                return UpdateEffectsVersion2(context, isAudioRendererActive, mapper);
            }

            return UpdateEffectsVersion1(context, isAudioRendererActive, mapper);
        }

        public ResultCode UpdateEffectsVersion2(EffectContext context, bool isAudioRendererActive, PoolMapper mapper)
        {
            if (context.GetCount() * Unsafe.SizeOf<EffectInParameterVersion2>() != _inputHeader.EffectsSize)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            int initialOutputSize = _output.Length;

            long initialInputConsumed = _inputReader.Consumed;

            for (int i = 0; i < context.GetCount(); i++)
            {
                ref readonly EffectInParameterVersion2 parameter = ref _inputReader.GetRefOrRefToCopy<EffectInParameterVersion2>(out _);

                ref EffectOutStatusVersion2 outStatus = ref SpanIOHelper.GetWriteRef<EffectOutStatusVersion2>(ref _output)[0];

                ref BaseEffect effect = ref context.GetEffect(i);

                if (!effect.IsTypeValid(in parameter))
                {
                    ResetEffect(ref effect, in parameter, mapper);
                }

                effect.Update(out ErrorInfo updateErrorInfo, in parameter, mapper);

                if (updateErrorInfo.ErrorCode != ResultCode.Success)
                {
                    _behaviourContext.AppendError(ref updateErrorInfo);
                }

                effect.StoreStatus(ref outStatus, isAudioRendererActive);

                if (parameter.IsNew)
                {
                    effect.InitializeResultState(ref context.GetDspState(i));
                    effect.InitializeResultState(ref context.GetState(i));
                }

                effect.UpdateResultState(ref outStatus.ResultState, ref context.GetState(i));
            }

            int currentOutputSize = _output.Length;

            OutputHeader.EffectsSize = (uint)(Unsafe.SizeOf<EffectOutStatusVersion2>() * context.GetCount());
            OutputHeader.TotalSize += OutputHeader.EffectsSize;

            Debug.Assert((initialOutputSize - currentOutputSize) == OutputHeader.EffectsSize);

            _inputReader.SetConsumed(initialInputConsumed + _inputHeader.EffectsSize);

            return ResultCode.Success;
        }

        public ResultCode UpdateEffectsVersion1(EffectContext context, bool isAudioRendererActive, PoolMapper mapper)
        {
            if (context.GetCount() * Unsafe.SizeOf<EffectInParameterVersion1>() != _inputHeader.EffectsSize)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            int initialOutputSize = _output.Length;

            long initialInputConsumed = _inputReader.Consumed;

            for (int i = 0; i < context.GetCount(); i++)
            {
                ref readonly EffectInParameterVersion1 parameter = ref _inputReader.GetRefOrRefToCopy<EffectInParameterVersion1>(out _);

                ref EffectOutStatusVersion1 outStatus = ref SpanIOHelper.GetWriteRef<EffectOutStatusVersion1>(ref _output)[0];

                ref BaseEffect effect = ref context.GetEffect(i);

                if (!effect.IsTypeValid(in parameter))
                {
                    ResetEffect(ref effect, in parameter, mapper);
                }

                effect.Update(out ErrorInfo updateErrorInfo, in parameter, mapper);

                if (updateErrorInfo.ErrorCode != ResultCode.Success)
                {
                    _behaviourContext.AppendError(ref updateErrorInfo);
                }

                effect.StoreStatus(ref outStatus, isAudioRendererActive);
            }

            int currentOutputSize = _output.Length;

            OutputHeader.EffectsSize = (uint)(Unsafe.SizeOf<EffectOutStatusVersion1>() * context.GetCount());
            OutputHeader.TotalSize += OutputHeader.EffectsSize;

            Debug.Assert((initialOutputSize - currentOutputSize) == OutputHeader.EffectsSize);

            _inputReader.SetConsumed(initialInputConsumed + _inputHeader.EffectsSize);

            return ResultCode.Success;
        }

        public ResultCode UpdateSplitter(SplitterContext context)
        {
            if (context.Update(ref _inputReader))
            {
                return ResultCode.Success;
            }

            return ResultCode.InvalidUpdateInfo;
        }

        private static bool CheckMixParametersValidity(MixContext mixContext, uint mixBufferCount, uint inputMixCount, SequenceReader<byte> parameters)
        {
            uint maxMixStateCount = mixContext.GetCount();
            uint totalRequiredMixBufferCount = 0;

            for (int i = 0; i < inputMixCount; i++)
            {
                ref readonly MixParameter parameter = ref parameters.GetRefOrRefToCopy<MixParameter>(out _);

                if (parameter.IsUsed)
                {
                    if (parameter.DestinationMixId != Constants.UnusedMixId &&
                        parameter.DestinationMixId > maxMixStateCount &&
                        parameter.MixId != Constants.FinalMixId)
                    {
                        return true;
                    }

                    totalRequiredMixBufferCount += parameter.BufferCount;
                }
            }

            return totalRequiredMixBufferCount > mixBufferCount;
        }

        public ResultCode UpdateMixes(MixContext mixContext, uint mixBufferCount, EffectContext effectContext, SplitterContext splitterContext)
        {
            uint mixCount;
            uint inputMixSize;
            uint inputSize = 0;

            if (_behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported())
            {
                ref readonly MixInParameterDirtyOnlyUpdate parameter = ref _inputReader.GetRefOrRefToCopy<MixInParameterDirtyOnlyUpdate>(out _);

                mixCount = parameter.MixCount;

                inputSize += (uint)Unsafe.SizeOf<MixInParameterDirtyOnlyUpdate>();
            }
            else
            {
                mixCount = mixContext.GetCount();
            }

            inputMixSize = mixCount * (uint)Unsafe.SizeOf<MixParameter>();

            inputSize += inputMixSize;

            if (inputSize != _inputHeader.MixesSize)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            long initialInputConsumed = _inputReader.Consumed;

            int parameterCount = (int)inputMixSize / Unsafe.SizeOf<MixParameter>();

            if (CheckMixParametersValidity(mixContext, mixBufferCount, mixCount, _inputReader))
            {
                return ResultCode.InvalidUpdateInfo;
            }

            bool isMixContextDirty = false;

            for (int i = 0; i < parameterCount; i++)
            {
                ref readonly MixParameter parameter = ref _inputReader.GetRefOrRefToCopy<MixParameter>(out _);

                int mixId = i;

                if (_behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported())
                {
                    mixId = parameter.MixId;
                }

                ref MixState mix = ref mixContext.GetState(mixId);

                if (parameter.IsUsed != mix.IsUsed)
                {
                    mix.IsUsed = parameter.IsUsed;

                    if (parameter.IsUsed)
                    {
                        mix.ClearEffectProcessingOrder();
                    }

                    isMixContextDirty = true;
                }

                if (mix.IsUsed)
                {
                    isMixContextDirty |= mix.Update(mixContext.EdgeMatrix, in parameter, effectContext, splitterContext, _behaviourContext);
                }
            }

            if (isMixContextDirty)
            {
                if (_behaviourContext.IsSplitterSupported() && splitterContext.UsingSplitter())
                {
                    if (!mixContext.Sort(splitterContext))
                    {
                        return ResultCode.InvalidMixSorting;
                    }
                }
                else
                {
                    mixContext.Sort();
                }
            }

            _inputReader.SetConsumed(initialInputConsumed + inputMixSize);

            return ResultCode.Success;
        }

        private static void ResetSink(ref BaseSink sink, in SinkInParameter parameter)
        {
            sink.CleanUp();

            sink = parameter.Type switch
            {
                SinkType.Invalid => new BaseSink(),
                SinkType.CircularBuffer => new CircularBufferSink(),
                SinkType.Device => new DeviceSink(),
                _ => throw new NotImplementedException($"SinkType {parameter.Type} not implemented!"),
            };
        }

        public ResultCode UpdateSinks(SinkContext context, PoolMapper mapper)
        {
            if (context.GetCount() * Unsafe.SizeOf<SinkInParameter>() != _inputHeader.SinksSize)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            int initialOutputSize = _output.Length;

            long initialInputConsumed = _inputReader.Consumed;

            for (int i = 0; i < context.GetCount(); i++)
            {
                ref readonly SinkInParameter parameter = ref _inputReader.GetRefOrRefToCopy<SinkInParameter>(out _);
                ref SinkOutStatus outStatus = ref SpanIOHelper.GetWriteRef<SinkOutStatus>(ref _output)[0];
                ref BaseSink sink = ref context.GetSink(i);

                if (!sink.IsTypeValid(in parameter))
                {
                    ResetSink(ref sink, in parameter);
                }

                sink.Update(out ErrorInfo updateErrorInfo, in parameter, ref outStatus, mapper);

                if (updateErrorInfo.ErrorCode != ResultCode.Success)
                {
                    _behaviourContext.AppendError(ref updateErrorInfo);
                }
            }

            int currentOutputSize = _output.Length;

            OutputHeader.SinksSize = (uint)(Unsafe.SizeOf<SinkOutStatus>() * context.GetCount());
            OutputHeader.TotalSize += OutputHeader.SinksSize;

            Debug.Assert((initialOutputSize - currentOutputSize) == OutputHeader.SinksSize);

            _inputReader.SetConsumed(initialInputConsumed + _inputHeader.SinksSize);

            return ResultCode.Success;
        }

        public ResultCode UpdatePerformanceBuffer(PerformanceManager manager, Span<byte> performanceOutput)
        {
            if (Unsafe.SizeOf<PerformanceInParameter>() != _inputHeader.PerformanceBufferSize)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            ref readonly PerformanceInParameter parameter = ref _inputReader.GetRefOrRefToCopy<PerformanceInParameter>(out _);

            ref PerformanceOutStatus outStatus = ref SpanIOHelper.GetWriteRef<PerformanceOutStatus>(ref _output)[0];

            if (manager != null)
            {
                outStatus.HistorySize = manager.CopyHistories(performanceOutput);

                manager.SetTargetNodeId(parameter.TargetNodeId);
            }
            else
            {
                outStatus.HistorySize = 0;
            }

            OutputHeader.PerformanceBufferSize = (uint)Unsafe.SizeOf<PerformanceOutStatus>();
            OutputHeader.TotalSize += OutputHeader.PerformanceBufferSize;

            return ResultCode.Success;
        }

        public ResultCode UpdateErrorInfo()
        {
            ref BehaviourErrorInfoOutStatus outStatus = ref SpanIOHelper.GetWriteRef<BehaviourErrorInfoOutStatus>(ref _output)[0];

            _behaviourContext.CopyErrorInfo(outStatus.ErrorInfos.AsSpan(), out outStatus.ErrorInfosCount);

            OutputHeader.BehaviourSize = (uint)Unsafe.SizeOf<BehaviourErrorInfoOutStatus>();
            OutputHeader.TotalSize += OutputHeader.BehaviourSize;

            return ResultCode.Success;
        }

        public ResultCode UpdateRendererInfo(ulong elapsedFrameCount)
        {
            ref RendererInfoOutStatus outStatus = ref SpanIOHelper.GetWriteRef<RendererInfoOutStatus>(ref _output)[0];

            outStatus.ElapsedFrameCount = elapsedFrameCount;

            OutputHeader.RenderInfoSize = (uint)Unsafe.SizeOf<RendererInfoOutStatus>();
            OutputHeader.TotalSize += OutputHeader.RenderInfoSize;

            return ResultCode.Success;
        }

        public readonly ResultCode CheckConsumedSize()
        {
            long consumedInputSize = _inputReader.Consumed;
            int consumedOutputSize = _outputOrigin.Length - _output.Length;

            if (consumedInputSize != _inputHeader.TotalSize)
            {
                Logger.Error?.Print(LogClass.AudioRenderer, $"Consumed input size mismatch (got {consumedInputSize} expected {_inputHeader.TotalSize})");

                return ResultCode.InvalidUpdateInfo;
            }

            if (consumedOutputSize != OutputHeader.TotalSize)
            {
                Logger.Error?.Print(LogClass.AudioRenderer, $"Consumed output size mismatch (got {consumedOutputSize} expected {OutputHeader.TotalSize})");

                return ResultCode.InvalidUpdateInfo;
            }

            return ResultCode.Success;
        }
    }
}
