//
// Copyright (c) 2019-2021 Ryujinx
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

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
using Ryujinx.Common.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Ryujinx.Audio.Renderer.Common.BehaviourParameter;

namespace Ryujinx.Audio.Renderer.Server
{
    public class StateUpdater
    {
        private readonly ReadOnlyMemory<byte> _inputOrigin;
        private ReadOnlyMemory <byte> _outputOrigin;
        private ReadOnlyMemory<byte> _input;

        private Memory<byte> _output;
        private uint _processHandle;
        private BehaviourContext _behaviourContext;

        private UpdateDataHeader _inputHeader;
        private Memory<UpdateDataHeader> _outputHeader;

        private ref UpdateDataHeader OutputHeader => ref _outputHeader.Span[0];

        public StateUpdater(ReadOnlyMemory<byte> input, Memory<byte> output, uint processHandle, BehaviourContext behaviourContext)
        {
            _input = input;
            _inputOrigin = _input;
            _output = output;
            _outputOrigin = _output;
            _processHandle = processHandle;
            _behaviourContext = behaviourContext;

            _inputHeader = SpanIOHelper.Read<UpdateDataHeader>(ref _input);

            _outputHeader = SpanMemoryManager<UpdateDataHeader>.Cast(_output.Slice(0, Unsafe.SizeOf<UpdateDataHeader>()));
            OutputHeader.Initialize(_behaviourContext.UserRevision);
            _output = _output.Slice(Unsafe.SizeOf<UpdateDataHeader>());
        }

        public ResultCode UpdateBehaviourContext()
        {
            BehaviourParameter parameter = SpanIOHelper.Read<BehaviourParameter>(ref _input);

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
            PoolMapper mapper = new PoolMapper(_processHandle, _behaviourContext.IsMemoryPoolForceMappingEnabled());

            if (memoryPools.Length * Unsafe.SizeOf<MemoryPoolInParameter>() != _inputHeader.MemoryPoolsSize)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            foreach (ref MemoryPoolState memoryPool in memoryPools)
            {
                MemoryPoolInParameter parameter = SpanIOHelper.Read<MemoryPoolInParameter>(ref _input);

                ref MemoryPoolOutStatus outStatus = ref SpanIOHelper.GetWriteRef<MemoryPoolOutStatus>(ref _output)[0];

                PoolMapper.UpdateResult updateResult = mapper.Update(ref memoryPool, ref parameter, ref outStatus);

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
                VoiceChannelResourceInParameter parameter = SpanIOHelper.Read<VoiceChannelResourceInParameter>(ref _input);

                ref VoiceChannelResource resource = ref context.GetChannelResource(i);

                resource.Id = parameter.Id;
                parameter.Mix.ToSpan().CopyTo(resource.Mix.ToSpan());
                resource.IsUsed = parameter.IsUsed;
            }

            return ResultCode.Success;
        }

        public ResultCode UpdateVoices(VoiceContext context, Memory<MemoryPoolState> memoryPools)
        {
            if (context.GetCount() * Unsafe.SizeOf<VoiceInParameter>() != _inputHeader.VoicesSize)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            int initialOutputSize = _output.Length;

            ReadOnlySpan<VoiceInParameter> parameters = MemoryMarshal.Cast<byte, VoiceInParameter>(_input.Slice(0, (int)_inputHeader.VoicesSize).Span);

            _input = _input.Slice((int)_inputHeader.VoicesSize);

            PoolMapper mapper = new PoolMapper(_processHandle, memoryPools, _behaviourContext.IsMemoryPoolForceMappingEnabled());

            // First make everything not in use.
            for (int i = 0; i < context.GetCount(); i++)
            {
                ref VoiceState state = ref context.GetState(i);

                state.InUse = false;
            }

            // Start processing
            for (int i = 0; i < context.GetCount(); i++)
            {
                VoiceInParameter parameter = parameters[i];

                Memory<VoiceUpdateState>[] voiceUpdateStates = new Memory<VoiceUpdateState>[Constants.VoiceChannelCountMax];

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

                    currentVoiceState.UpdateParameters(out ErrorInfo updateParameterError, ref parameter, ref mapper, ref _behaviourContext);

                    if (updateParameterError.ErrorCode != ResultCode.Success)
                    {
                        _behaviourContext.AppendError(ref updateParameterError);
                    }

                    currentVoiceState.UpdateWaveBuffers(out ErrorInfo[] waveBufferUpdateErrorInfos, ref parameter, voiceUpdateStates, ref mapper, ref _behaviourContext);

                    foreach (ref ErrorInfo errorInfo in waveBufferUpdateErrorInfos.AsSpan())
                    {
                        if (errorInfo.ErrorCode != ResultCode.Success)
                        {
                            _behaviourContext.AppendError(ref errorInfo);
                        }
                    }

                    currentVoiceState.WriteOutStatus(ref outStatus, ref parameter, voiceUpdateStates);
                }
            }

            int currentOutputSize = _output.Length;

            OutputHeader.VoicesSize = (uint)(Unsafe.SizeOf<VoiceOutStatus>() * context.GetCount());
            OutputHeader.TotalSize += OutputHeader.VoicesSize;

            Debug.Assert((initialOutputSize - currentOutputSize) == OutputHeader.VoicesSize);

            return ResultCode.Success;
        }

        private static void ResetEffect(ref BaseEffect effect, ref EffectInParameter parameter, PoolMapper mapper)
        {
            effect.ForceUnmapBuffers(mapper);

            switch (parameter.Type)
            {
                case EffectType.Invalid:
                    effect = new BaseEffect();
                    break;
                case EffectType.BufferMix:
                    effect = new BufferMixEffect();
                    break;
                case EffectType.AuxiliaryBuffer:
                    effect = new AuxiliaryBufferEffect();
                    break;
                case EffectType.Delay:
                    effect = new DelayEffect();
                    break;
                case EffectType.Reverb:
                    effect = new ReverbEffect();
                    break;
                case EffectType.Reverb3d:
                    effect = new Reverb3dEffect();
                    break;
                case EffectType.BiquadFilter:
                    effect = new BiquadFilterEffect();
                    break;
                default:
                    throw new NotImplementedException($"EffectType {parameter.Type} not implemented!");
            }
        }

        public ResultCode UpdateEffects(EffectContext context, bool isAudioRendererActive, Memory<MemoryPoolState> memoryPools)
        {
            if (context.GetCount() * Unsafe.SizeOf<EffectInParameter>() != _inputHeader.EffectsSize)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            int initialOutputSize = _output.Length;

            ReadOnlySpan<EffectInParameter> parameters = MemoryMarshal.Cast<byte, EffectInParameter>(_input.Slice(0, (int)_inputHeader.EffectsSize).Span);

            _input = _input.Slice((int)_inputHeader.EffectsSize);

            PoolMapper mapper = new PoolMapper(_processHandle, memoryPools, _behaviourContext.IsMemoryPoolForceMappingEnabled());

            for (int i = 0; i < context.GetCount(); i++)
            {
                EffectInParameter parameter = parameters[i];

                ref EffectOutStatus outStatus = ref SpanIOHelper.GetWriteRef<EffectOutStatus>(ref _output)[0];

                ref BaseEffect effect = ref context.GetEffect(i);

                if (!effect.IsTypeValid(ref parameter))
                {
                    ResetEffect(ref effect, ref parameter, mapper);
                }

                effect.Update(out ErrorInfo updateErrorInfo, ref parameter, mapper);

                if (updateErrorInfo.ErrorCode != ResultCode.Success)
                {
                    _behaviourContext.AppendError(ref updateErrorInfo);
                }

                effect.StoreStatus(ref outStatus, isAudioRendererActive);
            }

            int currentOutputSize = _output.Length;

            OutputHeader.EffectsSize = (uint)(Unsafe.SizeOf<EffectOutStatus>() * context.GetCount());
            OutputHeader.TotalSize += OutputHeader.EffectsSize;

            Debug.Assert((initialOutputSize - currentOutputSize) == OutputHeader.EffectsSize);

            return ResultCode.Success;
        }

        public ResultCode UpdateSplitter(SplitterContext context)
        {
            if (context.Update(_input.Span, out int consumedSize))
            {
                _input = _input.Slice(consumedSize);

                return ResultCode.Success;
            }
            else
            {
                return ResultCode.InvalidUpdateInfo;
            }
        }

        private bool CheckMixParametersValidity(MixContext mixContext, uint mixBufferCount, uint inputMixCount, ReadOnlySpan<MixParameter> parameters)
        {
            uint maxMixStateCount = mixContext.GetCount();
            uint totalRequiredMixBufferCount = 0;

            for (int i = 0; i < inputMixCount; i++)
            {
                if (parameters[i].IsUsed)
                {
                    if (parameters[i].DestinationMixId != Constants.UnusedMixId &&
                        parameters[i].DestinationMixId > maxMixStateCount &&
                        parameters[i].MixId != Constants.FinalMixId)
                    {
                        return true;
                    }

                    totalRequiredMixBufferCount += parameters[i].BufferCount;
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
                MixInParameterDirtyOnlyUpdate parameter = MemoryMarshal.Cast<byte, MixInParameterDirtyOnlyUpdate>(_input.Span)[0];

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

            if (_behaviourContext.IsMixInParameterDirtyOnlyUpdateSupported())
            {
                _input = _input.Slice(Unsafe.SizeOf<MixInParameterDirtyOnlyUpdate>());
            }

            ReadOnlySpan<MixParameter> parameters = MemoryMarshal.Cast<byte, MixParameter>(_input.Span.Slice(0, (int)inputMixSize));

            _input = _input.Slice((int)inputMixSize);

            if (CheckMixParametersValidity(mixContext, mixBufferCount, mixCount, parameters))
            {
                return ResultCode.InvalidUpdateInfo;
            }

            bool isMixContextDirty = false;

            for (int i = 0; i < parameters.Length; i++)
            {
                MixParameter parameter = parameters[i];

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
                    isMixContextDirty |= mix.Update(mixContext.EdgeMatrix, ref parameter, effectContext, splitterContext, _behaviourContext);
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

            return ResultCode.Success;
        }

        private static void ResetSink(ref BaseSink sink, ref SinkInParameter parameter)
        {
            sink.CleanUp();

            switch (parameter.Type)
            {
                case SinkType.Invalid:
                    sink = new BaseSink();
                    break;
                case SinkType.CircularBuffer:
                    sink = new CircularBufferSink();
                    break;
                case SinkType.Device:
                    sink = new DeviceSink();
                    break;
                default:
                    throw new NotImplementedException($"SinkType {parameter.Type} not implemented!");
            }
        }

        public ResultCode UpdateSinks(SinkContext context, Memory<MemoryPoolState> memoryPools)
        {
            PoolMapper mapper = new PoolMapper(_processHandle, memoryPools, _behaviourContext.IsMemoryPoolForceMappingEnabled());

            if (context.GetCount() * Unsafe.SizeOf<SinkInParameter>() != _inputHeader.SinksSize)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            int initialOutputSize = _output.Length;

            ReadOnlySpan<SinkInParameter> parameters = MemoryMarshal.Cast<byte, SinkInParameter>(_input.Slice(0, (int)_inputHeader.SinksSize).Span);

            _input = _input.Slice((int)_inputHeader.SinksSize);

            for (int i = 0; i < context.GetCount(); i++)
            {
                SinkInParameter parameter = parameters[i];
                ref SinkOutStatus outStatus = ref SpanIOHelper.GetWriteRef<SinkOutStatus>(ref _output)[0];
                ref BaseSink sink = ref context.GetSink(i);

                if (!sink.IsTypeValid(ref parameter))
                {
                    ResetSink(ref sink, ref parameter);
                }

                sink.Update(out ErrorInfo updateErrorInfo, ref parameter, ref outStatus, mapper);

                if (updateErrorInfo.ErrorCode != ResultCode.Success)
                {
                    _behaviourContext.AppendError(ref updateErrorInfo);
                }
            }

            int currentOutputSize = _output.Length;

            OutputHeader.SinksSize = (uint)(Unsafe.SizeOf<SinkOutStatus>() * context.GetCount());
            OutputHeader.TotalSize += OutputHeader.SinksSize;

            Debug.Assert((initialOutputSize - currentOutputSize) == OutputHeader.SinksSize);

            return ResultCode.Success;
        }

        public ResultCode UpdatePerformanceBuffer(PerformanceManager manager, Span<byte> performanceOutput)
        {
            if (Unsafe.SizeOf<PerformanceInParameter>() != _inputHeader.PerformanceBufferSize)
            {
                return ResultCode.InvalidUpdateInfo;
            }

            PerformanceInParameter parameter = SpanIOHelper.Read<PerformanceInParameter>(ref _input);

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

            _behaviourContext.CopyErrorInfo(outStatus.ErrorInfos.ToSpan(), out outStatus.ErrorInfosCount);

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

        public ResultCode CheckConsumedSize()
        {
            int consumedInputSize = _inputOrigin.Length - _input.Length;
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
