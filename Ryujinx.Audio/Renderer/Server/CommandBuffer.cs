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
using Ryujinx.Audio.Renderer.Dsp.Command;
using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Parameter.Effect;
using Ryujinx.Audio.Renderer.Server.Performance;
using Ryujinx.Audio.Renderer.Server.Sink;
using Ryujinx.Audio.Renderer.Server.Upsampler;
using Ryujinx.Audio.Renderer.Server.Voice;
using Ryujinx.Common.Memory;
using System;
using CpuAddress = System.UInt64;

namespace Ryujinx.Audio.Renderer.Server
{
    /// <summary>
    /// An API to generate commands and aggregate them into a <see cref="CommandList"/>.
    /// </summary>
    public class CommandBuffer
    {
        /// <summary>
        /// The command processing time estimator in use.
        /// </summary>
        private ICommandProcessingTimeEstimator _commandProcessingTimeEstimator;

        /// <summary>
        /// The estimated total processing time.
        /// </summary>
        public ulong EstimatedProcessingTime { get; set; }

        /// <summary>
        /// The command list that is populated by the <see cref="CommandBuffer"/>.
        /// </summary>
        public CommandList CommandList { get; }

        /// <summary>
        /// Create a new <see cref="CommandBuffer"/>.
        /// </summary>
        /// <param name="commandList">The command list that will store the generated commands.</param>
        /// <param name="commandProcessingTimeEstimator">The command processing time estimator to use.</param>
        public CommandBuffer(CommandList commandList, ICommandProcessingTimeEstimator commandProcessingTimeEstimator)
        {
            CommandList = commandList;
            EstimatedProcessingTime = 0;
            _commandProcessingTimeEstimator = commandProcessingTimeEstimator;
        }

        /// <summary>
        /// Add a new generated command to the <see cref="CommandList"/>.
        /// </summary>
        /// <param name="command">The command to add.</param>
        private void AddCommand(ICommand command)
        {
            EstimatedProcessingTime += command.EstimatedProcessingTime;

            CommandList.AddCommand(command);
        }

        /// <summary>
        /// Generate a new <see cref="ClearMixBufferCommand"/>.
        /// </summary>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateClearMixBuffer(int nodeId)
        {
            ClearMixBufferCommand command = new ClearMixBufferCommand(nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Generate a new <see cref="DepopPrepareCommand"/>.
        /// </summary>
        /// <param name="state">The voice state associated.</param>
        /// <param name="depopBuffer">The depop buffer.</param>
        /// <param name="bufferCount">The buffer count.</param>
        /// <param name="bufferOffset">The target buffer offset.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        /// <param name="wasPlaying">Set to true if the voice was playing previously.</param>
        public void GenerateDepopPrepare(Memory<VoiceUpdateState> state, Memory<float> depopBuffer, uint bufferCount, uint bufferOffset, int nodeId, bool wasPlaying)
        {
            DepopPrepareCommand command = new DepopPrepareCommand(state, depopBuffer, bufferCount, bufferOffset, nodeId, wasPlaying);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Generate a new <see cref="PerformanceCommand"/>.
        /// </summary>
        /// <param name="performanceEntryAddresses">The <see cref="PerformanceEntryAddresses"/>.</param>
        /// <param name="type">The performance operation to perform.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GeneratePerformance(ref PerformanceEntryAddresses performanceEntryAddresses, PerformanceCommand.Type type, int nodeId)
        {
            PerformanceCommand command = new PerformanceCommand(ref performanceEntryAddresses, type, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Create a new <see cref="VolumeRampCommand"/>.
        /// </summary>
        /// <param name="previousVolume">The previous volume.</param>
        /// <param name="volume">The new volume.</param>
        /// <param name="bufferIndex">The index of the mix buffer to use.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateVolumeRamp(float previousVolume, float volume, uint bufferIndex, int nodeId)
        {
            VolumeRampCommand command = new VolumeRampCommand(previousVolume, volume, bufferIndex, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Create a new <see cref="DataSourceVersion2Command"/>.
        /// </summary>
        /// <param name="voiceState">The <see cref="VoiceState"/> to generate the command from.</param>
        /// <param name="state">The <see cref="VoiceUpdateState"/> to generate the command from.</param>
        /// <param name="outputBufferIndex">The output buffer index to use.</param>
        /// <param name="channelIndex">The target channel index.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateDataSourceVersion2(ref VoiceState voiceState, Memory<VoiceUpdateState> state, ushort outputBufferIndex, ushort channelIndex, int nodeId)
        {
            DataSourceVersion2Command command = new DataSourceVersion2Command(ref voiceState, state, outputBufferIndex, channelIndex, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Create a new <see cref="PcmInt16DataSourceCommandVersion1"/>.
        /// </summary>
        /// <param name="voiceState">The <see cref="VoiceState"/> to generate the command from.</param>
        /// <param name="state">The <see cref="VoiceUpdateState"/> to generate the command from.</param>
        /// <param name="outputBufferIndex">The output buffer index to use.</param>
        /// <param name="channelIndex">The target channel index.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GeneratePcmInt16DataSourceVersion1(ref VoiceState voiceState, Memory<VoiceUpdateState> state, ushort outputBufferIndex, ushort channelIndex, int nodeId)
        {
            PcmInt16DataSourceCommandVersion1 command = new PcmInt16DataSourceCommandVersion1(ref voiceState, state, outputBufferIndex, channelIndex, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Create a new <see cref="PcmFloatDataSourceCommandVersion1"/>.
        /// </summary>
        /// <param name="voiceState">The <see cref="VoiceState"/> to generate the command from.</param>
        /// <param name="state">The <see cref="VoiceUpdateState"/> to generate the command from.</param>
        /// <param name="outputBufferIndex">The output buffer index to use.</param>
        /// <param name="channelIndex">The target channel index.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GeneratePcmFloatDataSourceVersion1(ref VoiceState voiceState, Memory<VoiceUpdateState> state, ushort outputBufferIndex, ushort channelIndex, int nodeId)
        {
            PcmFloatDataSourceCommandVersion1 command = new PcmFloatDataSourceCommandVersion1(ref voiceState, state, outputBufferIndex, channelIndex, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Create a new <see cref="AdpcmDataSourceCommandVersion1"/>.
        /// </summary>
        /// <param name="voiceState">The <see cref="VoiceState"/> to generate the command from.</param>
        /// <param name="state">The <see cref="VoiceUpdateState"/> to generate the command from.</param>
        /// <param name="outputBufferIndex">The output buffer index to use.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateAdpcmDataSourceVersion1(ref VoiceState voiceState, Memory<VoiceUpdateState> state, ushort outputBufferIndex, int nodeId)
        {
            AdpcmDataSourceCommandVersion1 command = new AdpcmDataSourceCommandVersion1(ref voiceState, state, outputBufferIndex, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Create a new <see cref="BiquadFilterCommand"/>.
        /// </summary>
        /// <param name="baseIndex">The base index of the input and output buffer.</param>
        /// <param name="filter">The biquad filter parameter.</param>
        /// <param name="biquadFilterStateMemory">The biquad state.</param>
        /// <param name="inputBufferOffset">The input buffer offset.</param>
        /// <param name="outputBufferOffset">The output buffer offset.</param>
        /// <param name="needInitialization">Set to true if the biquad filter state needs to be initialized.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateBiquadFilter(int baseIndex, ref BiquadFilterParameter filter, Memory<BiquadFilterState> biquadFilterStateMemory, int inputBufferOffset, int outputBufferOffset, bool needInitialization, int nodeId)
        {
            BiquadFilterCommand command = new BiquadFilterCommand(baseIndex, ref filter, biquadFilterStateMemory, inputBufferOffset, outputBufferOffset, needInitialization, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Create a new <see cref="GroupedBiquadFilterCommand"/>.
        /// </summary>
        /// <param name="baseIndex">The base index of the input and output buffer.</param>
        /// <param name="filters">The biquad filter parameters.</param>
        /// <param name="biquadFilterStatesMemory">The biquad states.</param>
        /// <param name="inputBufferOffset">The input buffer offset.</param>
        /// <param name="outputBufferOffset">The output buffer offset.</param>
        /// <param name="isInitialized">Set to true if the biquad filter state is initialized.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateGroupedBiquadFilter(int baseIndex, ReadOnlySpan<BiquadFilterParameter> filters, Memory<BiquadFilterState> biquadFilterStatesMemory, int inputBufferOffset, int outputBufferOffset, ReadOnlySpan<bool> isInitialized, int nodeId)
        {
            GroupedBiquadFilterCommand command = new GroupedBiquadFilterCommand(baseIndex, filters, biquadFilterStatesMemory, inputBufferOffset, outputBufferOffset, isInitialized, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Generate a new <see cref="MixRampGroupedCommand"/>.
        /// </summary>
        /// <param name="mixBufferCount">The mix buffer count.</param>
        /// <param name="inputBufferIndex">The base input index.</param>
        /// <param name="outputBufferIndex">The base output index.</param>
        /// <param name="previousVolume">The previous volume.</param>
        /// <param name="volume">The new volume.</param>
        /// <param name="state">The <see cref="VoiceUpdateState"/> to generate the command from.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateMixRampGrouped(uint mixBufferCount, uint inputBufferIndex, uint outputBufferIndex, Span<float> previousVolume, Span<float> volume, Memory<VoiceUpdateState> state, int nodeId)
        {
            MixRampGroupedCommand command = new MixRampGroupedCommand(mixBufferCount, inputBufferIndex, outputBufferIndex, previousVolume, volume, state, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Generate a new <see cref="MixRampCommand"/>.
        /// </summary>
        /// <param name="previousVolume">The previous volume.</param>
        /// <param name="volume">The new volume.</param>
        /// <param name="inputBufferIndex">The input buffer index.</param>
        /// <param name="outputBufferIndex">The output buffer index.</param>
        /// <param name="lastSampleIndex">The index in the <see cref="VoiceUpdateState.LastSamples"/> array to store the ramped sample.</param>
        /// <param name="state">The <see cref="VoiceUpdateState"/> to generate the command from.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateMixRamp(float previousVolume, float volume, uint inputBufferIndex, uint outputBufferIndex, int lastSampleIndex, Memory<VoiceUpdateState> state, int nodeId)
        {
            MixRampCommand command = new MixRampCommand(previousVolume, volume, inputBufferIndex, outputBufferIndex, lastSampleIndex, state, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Generate a new <see cref="DepopForMixBuffersCommand"/>.
        /// </summary>
        /// <param name="depopBuffer">The depop buffer.</param>
        /// <param name="bufferOffset">The target buffer offset.</param>
        /// <param name="bufferCount">The buffer count.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        /// <param name="sampleRate">The target sample rate in use.</param>
        public void GenerateDepopForMixBuffersCommand(Memory<float> depopBuffer, uint bufferOffset, uint bufferCount, int nodeId, uint sampleRate)
        {
            DepopForMixBuffersCommand command = new DepopForMixBuffersCommand(depopBuffer, bufferOffset, bufferCount, nodeId, sampleRate);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Generate a new <see cref="CopyMixBufferCommand"/>.
        /// </summary>
        /// <param name="inputBufferIndex">The input buffer index.</param>
        /// <param name="outputBufferIndex">The output buffer index.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateCopyMixBuffer(uint inputBufferIndex, uint outputBufferIndex, int nodeId)
        {
            CopyMixBufferCommand command = new CopyMixBufferCommand(inputBufferIndex, outputBufferIndex, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Generate a new <see cref="MixCommand"/>.
        /// </summary>
        /// <param name="inputBufferIndex">The input buffer index.</param>
        /// <param name="outputBufferIndex">The output buffer index.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        /// <param name="volume">The mix volume.</param>
        public void GenerateMix(uint inputBufferIndex, uint outputBufferIndex, int nodeId, float volume)
        {
            MixCommand command = new MixCommand(inputBufferIndex, outputBufferIndex, nodeId, volume);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Generate a new <see cref="ReverbCommand"/>.
        /// </summary>
        /// <param name="bufferOffset">The target buffer offset.</param>
        /// <param name="parameter">The reverb parameter.</param>
        /// <param name="state">The reverb state.</param>
        /// <param name="isEnabled">Set to true if the effect should be active.</param>
        /// <param name="workBuffer">The work buffer to use for processing.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        /// <param name="isLongSizePreDelaySupported">If set to true, the long size pre-delay is supported.</param>
        /// <param name="newEffectChannelMappingSupported">If set to true, the new effect channel mapping for 5.1 is supported.</param>
        public void GenerateReverbEffect(uint bufferOffset, ReverbParameter parameter, Memory<ReverbState> state, bool isEnabled, CpuAddress workBuffer, int nodeId, bool isLongSizePreDelaySupported, bool newEffectChannelMappingSupported)
        {
            if (parameter.IsChannelCountValid())
            {
                ReverbCommand command = new ReverbCommand(bufferOffset, parameter, state, isEnabled, workBuffer, nodeId, isLongSizePreDelaySupported, newEffectChannelMappingSupported);

                command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

                AddCommand(command);
            }
        }

        /// <summary>
        /// Generate a new <see cref="Reverb3dCommand"/>.
        /// </summary>
        /// <param name="bufferOffset">The target buffer offset.</param>
        /// <param name="parameter">The reverb 3d parameter.</param>
        /// <param name="state">The reverb 3d state.</param>
        /// <param name="isEnabled">Set to true if the effect should be active.</param>
        /// <param name="workBuffer">The work buffer to use for processing.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        /// <param name="newEffectChannelMappingSupported">If set to true, the new effect channel mapping for 5.1 is supported.</param>
        public void GenerateReverb3dEffect(uint bufferOffset, Reverb3dParameter parameter, Memory<Reverb3dState> state, bool isEnabled, CpuAddress workBuffer, int nodeId, bool newEffectChannelMappingSupported)
        {
            if (parameter.IsChannelCountValid())
            {
                Reverb3dCommand command = new Reverb3dCommand(bufferOffset, parameter, state, isEnabled, workBuffer, nodeId, newEffectChannelMappingSupported);

                command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

                AddCommand(command);
            }
        }


        /// <summary>
        /// Generate a new <see cref="DelayCommand"/>.
        /// </summary>
        /// <param name="bufferOffset">The target buffer offset.</param>
        /// <param name="parameter">The delay parameter.</param>
        /// <param name="state">The delay state.</param>
        /// <param name="isEnabled">Set to true if the effect should be active.</param>
        /// <param name="workBuffer">The work buffer to use for processing.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        /// <param name="newEffectChannelMappingSupported">If set to true, the new effect channel mapping for 5.1 is supported.</param>
        public void GenerateDelayEffect(uint bufferOffset, DelayParameter parameter, Memory<DelayState> state, bool isEnabled, CpuAddress workBuffer, int nodeId, bool newEffectChannelMappingSupported)
        {
            if (parameter.IsChannelCountValid())
            {
                DelayCommand command = new DelayCommand(bufferOffset, parameter, state, isEnabled, workBuffer, nodeId, newEffectChannelMappingSupported);

                command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

                AddCommand(command);
            }
        }

        /// <summary>
        /// Generate a new <see cref="LimiterCommandVersion1"/>.
        /// </summary>
        /// <param name="bufferOffset">The target buffer offset.</param>
        /// <param name="parameter">The limiter parameter.</param>
        /// <param name="state">The limiter state.</param>
        /// <param name="isEnabled">Set to true if the effect should be active.</param>
        /// <param name="workBuffer">The work buffer to use for processing.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateLimiterEffectVersion1(uint bufferOffset, LimiterParameter parameter, Memory<LimiterState> state, bool isEnabled, ulong workBuffer, int nodeId)
        {
            if (parameter.IsChannelCountValid())
            {
                LimiterCommandVersion1 command = new LimiterCommandVersion1(bufferOffset, parameter, state, isEnabled, workBuffer, nodeId);

                command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

                AddCommand(command);
            }
        }

        /// <summary>
        /// Generate a new <see cref="LimiterCommandVersion2"/>.
        /// </summary>
        /// <param name="bufferOffset">The target buffer offset.</param>
        /// <param name="parameter">The limiter parameter.</param>
        /// <param name="state">The limiter state.</param>
        /// <param name="effectResultState">The DSP effect result state.</param>
        /// <param name="isEnabled">Set to true if the effect should be active.</param>
        /// <param name="workBuffer">The work buffer to use for processing.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateLimiterEffectVersion2(uint bufferOffset, LimiterParameter parameter, Memory<LimiterState> state, Memory<EffectResultState> effectResultState, bool isEnabled, ulong workBuffer, int nodeId)
        {
            if (parameter.IsChannelCountValid())
            {
                LimiterCommandVersion2 command = new LimiterCommandVersion2(bufferOffset, parameter, state, effectResultState, isEnabled, workBuffer, nodeId);

                command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

                AddCommand(command);
            }
        }

        /// <summary>
        /// Generate a new <see cref="AuxiliaryBufferCommand"/>.
        /// </summary>
        /// <param name="bufferOffset">The target buffer offset.</param>
        /// <param name="inputBufferOffset">The input buffer offset.</param>
        /// <param name="outputBufferOffset">The output buffer offset.</param>
        /// <param name="state">The aux state.</param>
        /// <param name="isEnabled">Set to true if the effect should be active.</param>
        /// <param name="countMax">The limit of the circular buffer.</param>
        /// <param name="outputBuffer">The guest address of the output buffer.</param>
        /// <param name="inputBuffer">The guest address of the input buffer.</param>
        /// <param name="updateCount">The count to add on the offset after write/read operations.</param>
        /// <param name="writeOffset">The write offset.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateAuxEffect(uint bufferOffset, byte inputBufferOffset, byte outputBufferOffset, ref AuxiliaryBufferAddresses state, bool isEnabled, uint countMax, CpuAddress outputBuffer, CpuAddress inputBuffer, uint updateCount, uint writeOffset, int nodeId)
        {
            if (state.SendBufferInfoBase != 0 && state.ReturnBufferInfoBase != 0)
            {
                AuxiliaryBufferCommand command = new AuxiliaryBufferCommand(bufferOffset, inputBufferOffset, outputBufferOffset, ref state, isEnabled, countMax, outputBuffer, inputBuffer, updateCount, writeOffset, nodeId);

                command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

                AddCommand(command);
            }
        }

        /// <summary>
        /// Generate a new <see cref="CaptureBufferCommand"/>.
        /// </summary>
        /// <param name="bufferOffset">The target buffer offset.</param>
        /// <param name="inputBufferOffset">The input buffer offset.</param>
        /// <param name="sendBufferInfo">The capture state.</param>
        /// <param name="isEnabled">Set to true if the effect should be active.</param>
        /// <param name="countMax">The limit of the circular buffer.</param>
        /// <param name="outputBuffer">The guest address of the output buffer.</param>
        /// <param name="updateCount">The count to add on the offset after write operations.</param>
        /// <param name="writeOffset">The write offset.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateCaptureEffect(uint bufferOffset, byte inputBufferOffset, ulong sendBufferInfo, bool isEnabled, uint countMax, CpuAddress outputBuffer, uint updateCount, uint writeOffset, int nodeId)
        {
            if (sendBufferInfo != 0)
            {
                CaptureBufferCommand command = new CaptureBufferCommand(bufferOffset, inputBufferOffset, sendBufferInfo, isEnabled, countMax, outputBuffer, updateCount, writeOffset, nodeId);

                command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

                AddCommand(command);
            }
        }

        /// <summary>
        /// Generate a new <see cref="VolumeCommand"/>.
        /// </summary>
        /// <param name="volume">The target volume to apply.</param>
        /// <param name="bufferOffset">The offset of the mix buffer.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateVolume(float volume, uint bufferOffset, int nodeId)
        {
            VolumeCommand command = new VolumeCommand(volume, bufferOffset, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Create a new <see cref="CircularBufferSinkCommand"/>.
        /// </summary>
        /// <param name="bufferOffset">The offset of the mix buffer.</param>
        /// <param name="sink">The <see cref="BaseSink"/> of the circular buffer.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateCircularBuffer(uint bufferOffset, CircularBufferSink sink, int nodeId)
        {
            CircularBufferSinkCommand command = new CircularBufferSinkCommand(bufferOffset, ref sink.Parameter, ref sink.CircularBufferAddressInfo, sink.CurrentWriteOffset, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Create a new <see cref="DownMixSurroundToStereoCommand"/>.
        /// </summary>
        /// <param name="bufferOffset">The offset of the mix buffer.</param>
        /// <param name="inputBufferOffset">The input buffer offset.</param>
        /// <param name="outputBufferOffset">The output buffer offset.</param>
        /// <param name="downMixParameter">The downmixer parameters to use.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateDownMixSurroundToStereo(uint bufferOffset, Span<byte> inputBufferOffset, Span<byte> outputBufferOffset, float[] downMixParameter, int nodeId)
        {
            DownMixSurroundToStereoCommand command = new DownMixSurroundToStereoCommand(bufferOffset, inputBufferOffset, outputBufferOffset, downMixParameter, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Create a new <see cref="UpsampleCommand"/>.
        /// </summary>
        /// <param name="bufferOffset">The offset of the mix buffer.</param>
        /// <param name="upsampler">The <see cref="UpsamplerState"/> associated.</param>
        /// <param name="inputCount">The total input count.</param>
        /// <param name="inputBufferOffset">The input buffer mix offset.</param>
        /// <param name="bufferCountPerSample">The buffer count per sample.</param>
        /// <param name="sampleCount">The source sample count.</param>
        /// <param name="sampleRate">The source sample rate.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateUpsample(uint bufferOffset, UpsamplerState upsampler, uint inputCount, Span<byte> inputBufferOffset, uint bufferCountPerSample, uint sampleCount, uint sampleRate, int nodeId)
        {
            UpsampleCommand command = new UpsampleCommand(bufferOffset, upsampler, inputCount, inputBufferOffset, bufferCountPerSample, sampleCount, sampleRate, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }

        /// <summary>
        /// Create a new <see cref="DeviceSinkCommand"/>.
        /// </summary>
        /// <param name="bufferOffset">The offset of the mix buffer.</param>
        /// <param name="sink">The <see cref="BaseSink"/> of the device sink.</param>
        /// <param name="sessionId">The current audio renderer session id.</param>
        /// <param name="buffer">The mix buffer in use.</param>
        /// <param name="nodeId">The node id associated to this command.</param>
        public void GenerateDeviceSink(uint bufferOffset, DeviceSink sink, int sessionId, Memory<float> buffer, int nodeId)
        {
            DeviceSinkCommand command = new DeviceSinkCommand(bufferOffset, sink, sessionId, buffer, nodeId);

            command.EstimatedProcessingTime = _commandProcessingTimeEstimator.Estimate(command);

            AddCommand(command);
        }
    }
}
