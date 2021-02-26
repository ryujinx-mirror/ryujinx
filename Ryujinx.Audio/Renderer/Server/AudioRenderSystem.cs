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

using Ryujinx.Audio.Integration;
using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Dsp.Command;
using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Server.Effect;
using Ryujinx.Audio.Renderer.Server.MemoryPool;
using Ryujinx.Audio.Renderer.Server.Mix;
using Ryujinx.Audio.Renderer.Server.Performance;
using Ryujinx.Audio.Renderer.Server.Sink;
using Ryujinx.Audio.Renderer.Server.Splitter;
using Ryujinx.Audio.Renderer.Server.Types;
using Ryujinx.Audio.Renderer.Server.Upsampler;
using Ryujinx.Audio.Renderer.Server.Voice;
using Ryujinx.Audio.Renderer.Utils;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Memory;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading;

using CpuAddress = System.UInt64;

namespace Ryujinx.Audio.Renderer.Server
{
    public class AudioRenderSystem : IDisposable
    {
        private object _lock = new object();

        private AudioRendererExecutionMode _executionMode;
        private IWritableEvent             _systemEvent;
        private ManualResetEvent           _terminationEvent;
        private MemoryPoolState            _dspMemoryPoolState;
        private VoiceContext               _voiceContext;
        private MixContext                 _mixContext;
        private SinkContext                _sinkContext;
        private SplitterContext            _splitterContext;
        private EffectContext              _effectContext;
        private PerformanceManager         _performanceManager;
        private UpsamplerManager           _upsamplerManager;
        private bool                       _isActive;
        private BehaviourContext           _behaviourContext;
        private ulong                      _totalElapsedTicksUpdating;
        private ulong                      _totalElapsedTicks;
        private int                        _sessionId;
        private Memory<MemoryPoolState>    _memoryPools;

        private uint  _sampleRate;
        private uint  _sampleCount;
        private uint  _mixBufferCount;
        private uint  _voiceChannelCountMax;
        private uint  _upsamplerCount;
        private uint  _memoryPoolCount;
        private uint  _processHandle;
        private ulong _appletResourceId;

        private WritableRegion _workBufferRegion;
        private MemoryHandle   _workBufferMemoryPin;

        private Memory<float> _mixBuffer;
        private Memory<float> _depopBuffer;

        private uint _renderingTimeLimitPercent;
        private bool _voiceDropEnabled;
        private uint _voiceDropCount;
        private bool _isDspRunningBehind;

        private ICommandProcessingTimeEstimator _commandProcessingTimeEstimator;

        private Memory<byte> _performanceBuffer;

        public IVirtualMemoryManager MemoryManager { get; private set; }

        private ulong _elapsedFrameCount;
        private ulong _renderingStartTick;

        private AudioRendererManager _manager;

        public AudioRenderSystem(AudioRendererManager manager, IWritableEvent systemEvent)
        {
            _manager            = manager;
            _terminationEvent   = new ManualResetEvent(false);
            _dspMemoryPoolState = MemoryPoolState.Create(MemoryPoolState.LocationType.Dsp);
            _voiceContext       = new VoiceContext();
            _mixContext         = new MixContext();
            _sinkContext        = new SinkContext();
            _splitterContext    = new SplitterContext();
            _effectContext      = new EffectContext();

            _commandProcessingTimeEstimator = null;
            _systemEvent = systemEvent;
            _behaviourContext = new BehaviourContext();

            _totalElapsedTicksUpdating = 0;
            _sessionId                 = 0;
        }

        public ResultCode Initialize(ref AudioRendererConfiguration parameter, uint processHandle, CpuAddress workBuffer, ulong workBufferSize, int sessionId, ulong appletResourceId, IVirtualMemoryManager memoryManager)
        {
            if (!BehaviourContext.CheckValidRevision(parameter.Revision))
            {
                return ResultCode.OperationFailed;
            }

            if (GetWorkBufferSize(ref parameter) > workBufferSize)
            {
                return ResultCode.WorkBufferTooSmall;
            }

            Debug.Assert(parameter.RenderingDevice == AudioRendererRenderingDevice.Dsp && parameter.ExecutionMode == AudioRendererExecutionMode.Auto);

            Logger.Info?.Print(LogClass.AudioRenderer, $"Initializing with REV{BehaviourContext.GetRevisionNumber(parameter.Revision)}");

            _behaviourContext.SetUserRevision(parameter.Revision);

            _sampleRate  = parameter.SampleRate;
            _sampleCount = parameter.SampleCount;
            _mixBufferCount = parameter.MixBufferCount;
            _voiceChannelCountMax = Constants.VoiceChannelCountMax;
            _upsamplerCount = parameter.SinkCount + parameter.SubMixBufferCount;
            _appletResourceId = appletResourceId;
            _memoryPoolCount = parameter.EffectCount + parameter.VoiceCount * Constants.VoiceWaveBufferCount;
            _executionMode = parameter.ExecutionMode;
            _sessionId = sessionId;
            MemoryManager = memoryManager;

            WorkBufferAllocator workBufferAllocator;

            _workBufferRegion = MemoryManager.GetWritableRegion(workBuffer, (int)workBufferSize);
            _workBufferRegion.Memory.Span.Fill(0);
            _workBufferMemoryPin = _workBufferRegion.Memory.Pin();

            workBufferAllocator = new WorkBufferAllocator(_workBufferRegion.Memory);

            PoolMapper poolMapper = new PoolMapper(processHandle, false);
            poolMapper.InitializeSystemPool(ref _dspMemoryPoolState, workBuffer, workBufferSize);

            _mixBuffer = workBufferAllocator.Allocate<float>(_sampleCount * (_voiceChannelCountMax + _mixBufferCount), 0x10);

            if (_mixBuffer.IsEmpty)
            {
                return ResultCode.WorkBufferTooSmall;
            }

            Memory<float> upSamplerWorkBuffer = workBufferAllocator.Allocate<float>(Constants.TargetSampleCount * (_voiceChannelCountMax + _mixBufferCount) * _upsamplerCount, 0x10);

            if (upSamplerWorkBuffer.IsEmpty)
            {
                return ResultCode.WorkBufferTooSmall;
            }

            _depopBuffer = workBufferAllocator.Allocate<float>((ulong)BitUtils.AlignUp(parameter.MixBufferCount, Constants.BufferAlignment), Constants.BufferAlignment);

            if (_depopBuffer.IsEmpty)
            {
                return ResultCode.WorkBufferTooSmall;
            }

            // Invalidate DSP cache on what was currently allocated with workBuffer.
            AudioProcessorMemoryManager.InvalidateDspCache(_dspMemoryPoolState.Translate(workBuffer, workBufferAllocator.Offset), workBufferAllocator.Offset);

            Debug.Assert((workBufferAllocator.Offset % Constants.BufferAlignment) == 0);

            Memory<VoiceState> voices = workBufferAllocator.Allocate<VoiceState>(parameter.VoiceCount, VoiceState.Alignment);

            if (voices.IsEmpty)
            {
                return ResultCode.WorkBufferTooSmall;
            }

            foreach (ref VoiceState voice in voices.Span)
            {
                voice.Initialize();
            }

            // A pain to handle as we can't have VoiceState*, use indices to be a bit more safe
            Memory<int> sortedVoices = workBufferAllocator.Allocate<int>(parameter.VoiceCount, 0x10);

            if (sortedVoices.IsEmpty)
            {
                return ResultCode.WorkBufferTooSmall;
            }

            // Clear memory (use -1 as it's an invalid index)
            sortedVoices.Span.Fill(-1);

            Memory<VoiceChannelResource> voiceChannelResources = workBufferAllocator.Allocate<VoiceChannelResource>(parameter.VoiceCount, VoiceChannelResource.Alignment);

            if (voiceChannelResources.IsEmpty)
            {
                return ResultCode.WorkBufferTooSmall;
            }

            for (uint id = 0; id < voiceChannelResources.Length; id++)
            {
                ref VoiceChannelResource voiceChannelResource = ref voiceChannelResources.Span[(int)id];

                voiceChannelResource.Id     = id;
                voiceChannelResource.IsUsed = false;
            }

            Memory<VoiceUpdateState> voiceUpdateStates = workBufferAllocator.Allocate<VoiceUpdateState>(parameter.VoiceCount, VoiceUpdateState.Align);

            if (voiceUpdateStates.IsEmpty)
            {
                return ResultCode.WorkBufferTooSmall;
            }

            uint mixesCount = parameter.SubMixBufferCount + 1;

            Memory<MixState> mixes = workBufferAllocator.Allocate<MixState>(mixesCount, MixState.Alignment);

            if (mixes.IsEmpty)
            {
                return ResultCode.WorkBufferTooSmall;
            }

            if (parameter.EffectCount == 0)
            {
                foreach (ref MixState mix in mixes.Span)
                {
                    mix = new MixState(Memory<int>.Empty, ref _behaviourContext);
                }
            }
            else
            {
                Memory<int> effectProcessingOrderArray = workBufferAllocator.Allocate<int>(parameter.EffectCount * mixesCount, 0x10);

                foreach (ref MixState mix in mixes.Span)
                {
                    mix = new MixState(effectProcessingOrderArray.Slice(0, (int)parameter.EffectCount), ref _behaviourContext);

                    effectProcessingOrderArray = effectProcessingOrderArray.Slice((int)parameter.EffectCount);
                }
            }

            // Initialize the final mix id
            mixes.Span[0].MixId = Constants.FinalMixId;

            Memory<int> sortedMixesState = workBufferAllocator.Allocate<int>(mixesCount, 0x10);

            if (sortedMixesState.IsEmpty)
            {
                return ResultCode.WorkBufferTooSmall;
            }

            // Clear memory (use -1 as it's an invalid index)
            sortedMixesState.Span.Fill(-1);

            Memory<byte> nodeStatesWorkBuffer = Memory<byte>.Empty;
            Memory<byte> edgeMatrixWorkBuffer = Memory<byte>.Empty;

            if (_behaviourContext.IsSplitterSupported())
            {
                nodeStatesWorkBuffer = workBufferAllocator.Allocate((uint)NodeStates.GetWorkBufferSize((int)mixesCount), 1);
                edgeMatrixWorkBuffer = workBufferAllocator.Allocate((uint)EdgeMatrix.GetWorkBufferSize((int)mixesCount), 1);

                if (nodeStatesWorkBuffer.IsEmpty || edgeMatrixWorkBuffer.IsEmpty)
                {
                    return ResultCode.WorkBufferTooSmall;
                }
            }

            _mixContext.Initialize(sortedMixesState, mixes, nodeStatesWorkBuffer, edgeMatrixWorkBuffer);

            _memoryPools = workBufferAllocator.Allocate<MemoryPoolState>(_memoryPoolCount, MemoryPoolState.Alignment);

            if (_memoryPools.IsEmpty)
            {
                return ResultCode.WorkBufferTooSmall;
            }

            foreach (ref MemoryPoolState state in _memoryPools.Span)
            {
                state = MemoryPoolState.Create(MemoryPoolState.LocationType.Cpu);
            }

            if (!_splitterContext.Initialize(ref _behaviourContext, ref parameter, workBufferAllocator))
            {
                return ResultCode.WorkBufferTooSmall;
            }

            _processHandle = processHandle;

            _upsamplerManager = new UpsamplerManager(upSamplerWorkBuffer, _upsamplerCount);

            _effectContext.Initialize(parameter.EffectCount);
            _sinkContext.Initialize(parameter.SinkCount);

            Memory<VoiceUpdateState> voiceUpdateStatesDsp = workBufferAllocator.Allocate<VoiceUpdateState>(parameter.VoiceCount, VoiceUpdateState.Align);

            if (voiceUpdateStatesDsp.IsEmpty)
            {
                return ResultCode.WorkBufferTooSmall;
            }

            _voiceContext.Initialize(sortedVoices, voices, voiceChannelResources, voiceUpdateStates, voiceUpdateStatesDsp, parameter.VoiceCount);

            if (parameter.PerformanceMetricFramesCount > 0)
            {
                ulong performanceBufferSize = PerformanceManager.GetRequiredBufferSizeForPerformanceMetricsPerFrame(ref parameter, ref _behaviourContext) * (parameter.PerformanceMetricFramesCount + 1) + 0xC;

                _performanceBuffer = workBufferAllocator.Allocate(performanceBufferSize, Constants.BufferAlignment);

                if (_performanceBuffer.IsEmpty)
                {
                    return ResultCode.WorkBufferTooSmall;
                }

                _performanceManager = PerformanceManager.Create(_performanceBuffer, ref parameter, _behaviourContext);
            }
            else
            {
                _performanceManager = null;
            }

            _totalElapsedTicksUpdating = 0;
            _totalElapsedTicks = 0;
            _renderingTimeLimitPercent = 100;
            _voiceDropEnabled = parameter.VoiceDropEnabled && _executionMode == AudioRendererExecutionMode.Auto;

            AudioProcessorMemoryManager.InvalidateDataCache(workBuffer, workBufferSize);

            _processHandle = processHandle;
            _elapsedFrameCount = 0;

            switch (_behaviourContext.GetCommandProcessingTimeEstimatorVersion())
            {
                case 1:
                    _commandProcessingTimeEstimator = new CommandProcessingTimeEstimatorVersion1(_sampleCount, _mixBufferCount);
                    break;
                case 2:
                    _commandProcessingTimeEstimator = new CommandProcessingTimeEstimatorVersion2(_sampleCount, _mixBufferCount);
                    break;
                case 3:
                    _commandProcessingTimeEstimator = new CommandProcessingTimeEstimatorVersion3(_sampleCount, _mixBufferCount);
                    break;
                default:
                    throw new NotImplementedException($"Unsupported processing time estimator version {_behaviourContext.GetCommandProcessingTimeEstimatorVersion()}.");
            }

            return ResultCode.Success;
        }

        public void Start()
        {
            Logger.Info?.Print(LogClass.AudioRenderer, $"Starting renderer id {_sessionId}");

            lock (_lock)
            {
                _elapsedFrameCount = 0;
                _isActive = true;
            }
        }

        public void Stop()
        {
            Logger.Info?.Print(LogClass.AudioRenderer, $"Stopping renderer id {_sessionId}");

            lock (_lock)
            {
                _isActive = false;
            }

            if (_executionMode == AudioRendererExecutionMode.Auto)
            {
                _terminationEvent.WaitOne();
            }

            Logger.Info?.Print(LogClass.AudioRenderer, $"Stopped renderer id {_sessionId}");
        }

        public ResultCode Update(Memory<byte> output, Memory<byte> performanceOutput, ReadOnlyMemory<byte> input)
        {
            lock (_lock)
            {
                ulong updateStartTicks = GetSystemTicks();

                output.Span.Fill(0);

                StateUpdater stateUpdater = new StateUpdater(input, output, _processHandle, _behaviourContext);

                ResultCode result;

                result = stateUpdater.UpdateBehaviourContext();

                if (result != ResultCode.Success)
                {
                    return result;
                }

                result = stateUpdater.UpdateMemoryPools(_memoryPools.Span);

                if (result != ResultCode.Success)
                {
                    return result;
                }

                result = stateUpdater.UpdateVoiceChannelResources(_voiceContext);

                if (result != ResultCode.Success)
                {
                    return result;
                }

                result = stateUpdater.UpdateVoices(_voiceContext, _memoryPools);

                if (result != ResultCode.Success)
                {
                    return result;
                }

                result = stateUpdater.UpdateEffects(_effectContext, _isActive, _memoryPools);

                if (result != ResultCode.Success)
                {
                    return result;
                }

                if (_behaviourContext.IsSplitterSupported())
                {
                    result = stateUpdater.UpdateSplitter(_splitterContext);

                    if (result != ResultCode.Success)
                    {
                        return result;
                    }
                }

                result = stateUpdater.UpdateMixes(_mixContext, GetMixBufferCount(), _effectContext, _splitterContext);

                if (result != ResultCode.Success)
                {
                    return result;
                }

                result = stateUpdater.UpdateSinks(_sinkContext, _memoryPools);

                if (result != ResultCode.Success)
                {
                    return result;
                }

                result = stateUpdater.UpdatePerformanceBuffer(_performanceManager, performanceOutput.Span);

                if (result != ResultCode.Success)
                {
                    return result;
                }

                result = stateUpdater.UpdateErrorInfo();

                if (result != ResultCode.Success)
                {
                    return result;
                }

                if (_behaviourContext.IsElapsedFrameCountSupported())
                {
                    result = stateUpdater.UpdateRendererInfo(_elapsedFrameCount);

                    if (result != ResultCode.Success)
                    {
                        return result;
                    }
                }

                result = stateUpdater.CheckConsumedSize();

                if (result != ResultCode.Success)
                {
                    return result;
                }

                _systemEvent.Clear();

                ulong updateEndTicks = GetSystemTicks();

                _totalElapsedTicksUpdating += (updateEndTicks - updateStartTicks);

                return result;
            }
        }

        private ulong GetSystemTicks()
        {
            double ticks = ARMeilleure.State.ExecutionContext.ElapsedTicks * ARMeilleure.State.ExecutionContext.TickFrequency;

            return (ulong)(ticks * Constants.TargetTimerFrequency);
        }

        private uint ComputeVoiceDrop(CommandBuffer commandBuffer, long voicesEstimatedTime, long deltaTimeDsp)
        {
            int i;

            for (i = 0; i < commandBuffer.CommandList.Commands.Count; i++)
            {
                ICommand command = commandBuffer.CommandList.Commands[i];

                CommandType commandType = command.CommandType;

                if (commandType == CommandType.AdpcmDataSourceVersion1 ||
                    commandType == CommandType.AdpcmDataSourceVersion2 ||
                    commandType == CommandType.PcmInt16DataSourceVersion1 ||
                    commandType == CommandType.PcmInt16DataSourceVersion2 ||
                    commandType == CommandType.PcmFloatDataSourceVersion1 ||
                    commandType == CommandType.PcmFloatDataSourceVersion2 ||
                    commandType == CommandType.Performance)
                {
                    break;
                }
            }

            uint voiceDropped = 0;

            for (; i < commandBuffer.CommandList.Commands.Count; i++)
            {
                ICommand targetCommand = commandBuffer.CommandList.Commands[i];

                int targetNodeId = targetCommand.NodeId;

                if (voicesEstimatedTime <= deltaTimeDsp || NodeIdHelper.GetType(targetNodeId) != NodeIdType.Voice)
                {
                    break;
                }

                ref VoiceState voice = ref _voiceContext.GetState(NodeIdHelper.GetBase(targetNodeId));

                if (voice.Priority == Constants.VoiceHighestPriority)
                {
                    break;
                }

                // We can safely drop this voice, disable all associated commands while activating depop preparation commands.
                voiceDropped++;
                voice.VoiceDropFlag = true;

                Logger.Warning?.Print(LogClass.AudioRenderer, $"Dropping voice {voice.NodeId}");

                for (; i < commandBuffer.CommandList.Commands.Count; i++)
                {
                    ICommand command = commandBuffer.CommandList.Commands[i];

                    if (command.NodeId != targetNodeId)
                    {
                        break;
                    }

                    if (command.CommandType == CommandType.DepopPrepare)
                    {
                        command.Enabled = true;
                    }
                    else if (command.CommandType == CommandType.Performance || !command.Enabled)
                    {
                        continue;
                    }
                    else
                    {
                        command.Enabled = false;

                        voicesEstimatedTime -= (long)command.EstimatedProcessingTime;
                    }
                }
            }

            return voiceDropped;
        }

        private void GenerateCommandList(out CommandList commandList)
        {
            Debug.Assert(_executionMode == AudioRendererExecutionMode.Auto);

            PoolMapper.ClearUsageState(_memoryPools);

            ulong startTicks = GetSystemTicks();

            commandList = new CommandList(this);

            if (_performanceManager != null)
            {
                _performanceManager.TapFrame(_isDspRunningBehind, _voiceDropCount, _renderingStartTick);

                _isDspRunningBehind = false;
                _voiceDropCount = 0;
                _renderingStartTick = 0;
            }

            CommandBuffer commandBuffer = new CommandBuffer(commandList, _commandProcessingTimeEstimator);

            CommandGenerator commandGenerator = new CommandGenerator(commandBuffer, GetContext(), _voiceContext, _mixContext, _effectContext, _sinkContext, _splitterContext, _performanceManager);

            _voiceContext.Sort();
            commandGenerator.GenerateVoices();

            long voicesEstimatedTime = (long)commandBuffer.EstimatedProcessingTime;

            commandGenerator.GenerateSubMixes();
            commandGenerator.GenerateFinalMixes();
            commandGenerator.GenerateSinks();

            long totalEstimatedTime = (long)commandBuffer.EstimatedProcessingTime;

            if (_voiceDropEnabled)
            {
                long maxDspTime = GetMaxAllocatedTimeForDsp();

                long restEstimateTime = totalEstimatedTime - voicesEstimatedTime;

                long deltaTimeDsp = Math.Max(maxDspTime - restEstimateTime, 0);

                _voiceDropCount = ComputeVoiceDrop(commandBuffer, voicesEstimatedTime, deltaTimeDsp);
            }

            _voiceContext.UpdateForCommandGeneration();

            ulong endTicks = GetSystemTicks();

            _totalElapsedTicks = endTicks - startTicks;

            _renderingStartTick = GetSystemTicks();
            _elapsedFrameCount++;
        }

        private int GetMaxAllocatedTimeForDsp()
        {
            return (int)(Constants.AudioProcessorMaxUpdateTimePerSessions * _behaviourContext.GetAudioRendererProcessingTimeLimit() * (GetRenderingTimeLimit() / 100.0f));
        }

        public void SendCommands()
        {
            lock (_lock)
            {
                if (_isActive)
                {
                    _terminationEvent.Reset();

                    GenerateCommandList(out CommandList commands);

                    _manager.Processor.Send(_sessionId,
                                            commands,
                                            GetMaxAllocatedTimeForDsp(),
                                            _appletResourceId);

                    _systemEvent.Signal();
                }
                else
                {
                    _terminationEvent.Set();
                }
            }
        }

        public uint GetMixBufferCount()
        {
            return _mixBufferCount;
        }

        public void SetRenderingTimeLimitPercent(uint percent)
        {
            Debug.Assert(percent <= 100);

            _renderingTimeLimitPercent = percent;
        }

        public uint GetRenderingTimeLimit()
        {
            return _renderingTimeLimitPercent;
        }

        public Memory<float> GetMixBuffer()
        {
            return _mixBuffer;
        }

        public uint GetSampleCount()
        {
            return _sampleCount;
        }

        public uint GetSampleRate()
        {
            return _sampleRate;
        }

        public uint GetVoiceChannelCountMax()
        {
            return _voiceChannelCountMax;
        }

        public bool IsActive()
        {
            return _isActive;
        }

        private RendererSystemContext GetContext()
        {
            return new RendererSystemContext
            {
                ChannelCount = _manager.Processor.OutputDevices[_sessionId].GetChannelCount(),
                BehaviourContext = _behaviourContext,
                DepopBuffer = _depopBuffer,
                MixBufferCount = GetMixBufferCount(),
                SessionId = _sessionId,
                UpsamplerManager = _upsamplerManager
            };
        }

        public int GetSessionId()
        {
            return _sessionId;
        }

        public static ulong GetWorkBufferSize(ref AudioRendererConfiguration parameter)
        {
            BehaviourContext behaviourContext = new BehaviourContext();

            behaviourContext.SetUserRevision(parameter.Revision);

            uint mixesCount = parameter.SubMixBufferCount + 1;

            uint memoryPoolCount = parameter.EffectCount + parameter.VoiceCount * Constants.VoiceWaveBufferCount;

            ulong size = 0;

            // Mix Buffers
            size = WorkBufferAllocator.GetTargetSize<float>(size, parameter.SampleCount * (Constants.VoiceChannelCountMax + parameter.MixBufferCount), 0x10);

            // Upsampler workbuffer
            size = WorkBufferAllocator.GetTargetSize<float>(size, Constants.TargetSampleCount * (Constants.VoiceChannelCountMax + parameter.MixBufferCount) * (parameter.SinkCount + parameter.SubMixBufferCount), 0x10);

            // Depop buffer
            size = WorkBufferAllocator.GetTargetSize<float>(size, (ulong)BitUtils.AlignUp(parameter.MixBufferCount, Constants.BufferAlignment), Constants.BufferAlignment);

            // Voice
            size = WorkBufferAllocator.GetTargetSize<VoiceState>(size, parameter.VoiceCount, VoiceState.Alignment);
            size = WorkBufferAllocator.GetTargetSize<int>(size, parameter.VoiceCount, 0x10);
            size = WorkBufferAllocator.GetTargetSize<VoiceChannelResource>(size, parameter.VoiceCount, VoiceChannelResource.Alignment);
            size = WorkBufferAllocator.GetTargetSize<VoiceUpdateState>(size, parameter.VoiceCount, VoiceUpdateState.Align);

            // Mix
            size = WorkBufferAllocator.GetTargetSize<MixState>(size, mixesCount, MixState.Alignment);
            size = WorkBufferAllocator.GetTargetSize<int>(size, parameter.EffectCount * mixesCount, 0x10);
            size = WorkBufferAllocator.GetTargetSize<int>(size, mixesCount, 0x10);

            if (behaviourContext.IsSplitterSupported())
            {
                size += (ulong)BitUtils.AlignUp(NodeStates.GetWorkBufferSize((int)mixesCount) + EdgeMatrix.GetWorkBufferSize((int)mixesCount), 0x10);
            }

            // Memory Pool
            size = WorkBufferAllocator.GetTargetSize<MemoryPoolState>(size, memoryPoolCount, MemoryPoolState.Alignment);

            // Splitter
            size = SplitterContext.GetWorkBufferSize(size, ref behaviourContext, ref parameter);

            // DSP Voice
            size = WorkBufferAllocator.GetTargetSize<VoiceUpdateState>(size, parameter.VoiceCount, VoiceUpdateState.Align);

            // Performance
            if (parameter.PerformanceMetricFramesCount > 0)
            {
                ulong performanceMetricsPerFramesSize = PerformanceManager.GetRequiredBufferSizeForPerformanceMetricsPerFrame(ref parameter, ref behaviourContext) * (parameter.PerformanceMetricFramesCount + 1) + 0xC;

                size += BitUtils.AlignUp(performanceMetricsPerFramesSize, Constants.PerformanceMetricsPerFramesSizeAlignment);
            }

            return BitUtils.AlignUp(size, Constants.WorkBufferAlignment);
        }

        public ResultCode QuerySystemEvent(out IWritableEvent systemEvent)
        {
            systemEvent = default;

            if (_executionMode == AudioRendererExecutionMode.Manual)
            {
                return ResultCode.UnsupportedOperation;
            }

            systemEvent = _systemEvent;

            return ResultCode.Success;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_isActive)
                {
                    Stop();
                }

                PoolMapper mapper = new PoolMapper(_processHandle, false);
                mapper.Unmap(ref _dspMemoryPoolState);

                PoolMapper.ClearUsageState(_memoryPools);

                for (int i = 0; i < _memoryPoolCount; i++)
                {
                    ref MemoryPoolState memoryPool = ref _memoryPools.Span[i];

                    if (memoryPool.IsMapped())
                    {
                        mapper.Unmap(ref memoryPool);
                    }
                }

                _manager.Unregister(this);
                _terminationEvent.Dispose();
                _workBufferMemoryPin.Dispose();
                _workBufferRegion.Dispose();
            }
        }
    }
}
