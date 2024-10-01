using Ryujinx.Audio.Common;
using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Audio.Renderer.Parameter;
using Ryujinx.Audio.Renderer.Server.MemoryPool;
using Ryujinx.Common.Memory;
using Ryujinx.Common.Utilities;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static Ryujinx.Audio.Renderer.Common.BehaviourParameter;
using static Ryujinx.Audio.Renderer.Parameter.VoiceInParameter;
using PlayState = Ryujinx.Audio.Renderer.Server.Types.PlayState;

namespace Ryujinx.Audio.Renderer.Server.Voice
{
    [StructLayout(LayoutKind.Sequential, Pack = Alignment)]
    public struct VoiceState
    {
        public const int Alignment = 0x10;

        /// <summary>
        /// Set to true if the voice is used.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool InUse;

        /// <summary>
        /// Set to true if the voice is new.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsNew;

        [MarshalAs(UnmanagedType.I1)]
        public bool WasPlaying;

        /// <summary>
        /// The <see cref="SampleFormat"/> of the voice.
        /// </summary>
        public SampleFormat SampleFormat;

        /// <summary>
        /// The sample rate of the voice.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// The total channel count used.
        /// </summary>
        public uint ChannelsCount;

        /// <summary>
        /// Id of the voice.
        /// </summary>
        public int Id;

        /// <summary>
        /// Node id of the voice.
        /// </summary>
        public int NodeId;

        /// <summary>
        /// The target mix id of the voice.
        /// </summary>
        public int MixId;

        /// <summary>
        /// The current voice <see cref="Types.PlayState"/>.
        /// </summary>
        public PlayState PlayState;

        /// <summary>
        /// The previous voice <see cref="Types.PlayState"/>.
        /// </summary>
        public PlayState PreviousPlayState;

        /// <summary>
        /// The priority of the voice.
        /// </summary>
        public uint Priority;

        /// <summary>
        /// Target sorting position of the voice. (used to sort voice with the same <see cref="Priority"/>)
        /// </summary>
        public uint SortingOrder;

        /// <summary>
        /// The pitch used on the voice.
        /// </summary>
        public float Pitch;

        /// <summary>
        /// The output volume of the voice.
        /// </summary>
        public float Volume;

        /// <summary>
        /// The previous output volume of the voice.
        /// </summary>
        public float PreviousVolume;

        /// <summary>
        /// Biquad filters to apply to the output of the voice.
        /// </summary>
        public Array2<BiquadFilterParameter> BiquadFilters;

        /// <summary>
        /// Total count of <see cref="WaveBufferInternal"/> of the voice.
        /// </summary>
        public uint WaveBuffersCount;

        /// <summary>
        /// Current playing <see cref="WaveBufferInternal"/> of the voice.
        /// </summary>
        public uint WaveBuffersIndex;

        /// <summary>
        /// Change the behaviour of the voice.
        /// </summary>
        /// <remarks>This was added on REV5.</remarks>
        public DecodingBehaviour DecodingBehaviour;

        /// <summary>
        /// User state <see cref="AddressInfo"/> required by the data source.
        /// </summary>
        /// <remarks>Only used for <see cref="SampleFormat.Adpcm"/> as the GC-ADPCM coefficients.</remarks>
        public AddressInfo DataSourceStateAddressInfo;

        /// <summary>
        /// The wavebuffers of this voice.
        /// </summary>
        public Array4<WaveBuffer> WaveBuffers;

        /// <summary>
        /// The channel resource ids associated to the voice.
        /// </summary>
        public Array6<int> ChannelResourceIds;

        /// <summary>
        /// The target splitter id of the voice.
        /// </summary>
        public uint SplitterId;

        /// <summary>
        /// Change the Sample Rate Conversion (SRC) quality of the voice.
        /// </summary>
        /// <remarks>This was added on REV8.</remarks>
        public SampleRateConversionQuality SrcQuality;

        /// <summary>
        /// If set to true, the voice was dropped.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool VoiceDropFlag;

        /// <summary>
        /// Set to true if the data source state work buffer wasn't mapped.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool DataSourceStateUnmapped;

        /// <summary>
        /// Set to true if any of the <see cref="WaveBuffer.BufferAddressInfo"/> work buffer wasn't mapped.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool BufferInfoUnmapped;

        /// <summary>
        /// The biquad filter initialization state storage.
        /// </summary>
        private BiquadFilterNeedInitializationArrayStruct _biquadFilterNeedInitialization;

        /// <summary>
        /// Flush the amount of wavebuffer specified. This will result in the wavebuffer being skipped and marked played.
        /// </summary>
        /// <remarks>This was added on REV5.</remarks>
        public byte FlushWaveBufferCount;

        [StructLayout(LayoutKind.Sequential, Size = Constants.VoiceBiquadFilterCount)]
        private struct BiquadFilterNeedInitializationArrayStruct { }

        /// <summary>
        /// The biquad filter initialization state array.
        /// </summary>
        public Span<bool> BiquadFilterNeedInitialization => SpanHelpers.AsSpan<BiquadFilterNeedInitializationArrayStruct, bool>(ref _biquadFilterNeedInitialization);

        /// <summary>
        /// Initialize the <see cref="VoiceState"/>.
        /// </summary>
        public void Initialize()
        {
            IsNew = false;
            VoiceDropFlag = false;
            DataSourceStateUnmapped = false;
            BufferInfoUnmapped = false;
            FlushWaveBufferCount = 0;
            PlayState = PlayState.Stopped;
            Priority = Constants.VoiceLowestPriority;
            Id = 0;
            NodeId = 0;
            SampleRate = 0;
            SampleFormat = SampleFormat.Invalid;
            ChannelsCount = 0;
            Pitch = 0.0f;
            Volume = 0.0f;
            PreviousVolume = 0.0f;
            BiquadFilters.AsSpan().Clear();
            WaveBuffersCount = 0;
            WaveBuffersIndex = 0;
            MixId = Constants.UnusedMixId;
            SplitterId = Constants.UnusedSplitterId;
            DataSourceStateAddressInfo.Setup(0, 0);

            InitializeWaveBuffers();
        }

        /// <summary>
        /// Initialize the <see cref="WaveBuffer"/> in this <see cref="VoiceState"/>.
        /// </summary>
        private void InitializeWaveBuffers()
        {
            for (int i = 0; i < WaveBuffers.Length; i++)
            {
                WaveBuffers[i].StartSampleOffset = 0;
                WaveBuffers[i].EndSampleOffset = 0;
                WaveBuffers[i].ShouldLoop = false;
                WaveBuffers[i].IsEndOfStream = false;
                WaveBuffers[i].BufferAddressInfo.Setup(0, 0);
                WaveBuffers[i].ContextAddressInfo.Setup(0, 0);
                WaveBuffers[i].IsSendToAudioProcessor = true;
            }
        }

        /// <summary>
        /// Check if the voice needs to be skipped.
        /// </summary>
        /// <returns>Returns true if the voice needs to be skipped.</returns>
        public readonly bool ShouldSkip()
        {
            return !InUse || WaveBuffersCount == 0 || DataSourceStateUnmapped || BufferInfoUnmapped || VoiceDropFlag;
        }

        /// <summary>
        /// Return true if the mix has any destinations.
        /// </summary>
        /// <returns>True if the mix has any destinations.</returns>
        public readonly bool HasAnyDestination()
        {
            return MixId != Constants.UnusedMixId || SplitterId != Constants.UnusedSplitterId;
        }

        /// <summary>
        /// Indicate if the server voice information needs to be updated.
        /// </summary>
        /// <param name="parameter">The user parameter.</param>
        /// <returns>Return true, if the server voice information needs to be updated.</returns>
        private readonly bool ShouldUpdateParameters(in VoiceInParameter parameter)
        {
            if (DataSourceStateAddressInfo.CpuAddress == parameter.DataSourceStateAddress)
            {
                return DataSourceStateAddressInfo.Size != parameter.DataSourceStateSize;
            }

            return DataSourceStateAddressInfo.CpuAddress != parameter.DataSourceStateAddress ||
                   DataSourceStateAddressInfo.Size != parameter.DataSourceStateSize ||
                   DataSourceStateUnmapped;
        }

        /// <summary>
        /// Update the internal state from a user parameter.
        /// </summary>
        /// <param name="outErrorInfo">The possible <see cref="ErrorInfo"/> that was generated.</param>
        /// <param name="parameter">The user parameter.</param>
        /// <param name="poolMapper">The mapper to use.</param>
        /// <param name="behaviourContext">The behaviour context.</param>
        public void UpdateParameters(out ErrorInfo outErrorInfo, in VoiceInParameter parameter, PoolMapper poolMapper, ref BehaviourContext behaviourContext)
        {
            InUse = parameter.InUse;
            Id = parameter.Id;
            NodeId = parameter.NodeId;

            UpdatePlayState(parameter.PlayState);

            SrcQuality = parameter.SrcQuality;

            Priority = parameter.Priority;
            SortingOrder = parameter.SortingOrder;
            SampleRate = parameter.SampleRate;
            SampleFormat = parameter.SampleFormat;
            ChannelsCount = parameter.ChannelCount;
            Pitch = parameter.Pitch;
            Volume = parameter.Volume;
            parameter.BiquadFilters.AsSpan().CopyTo(BiquadFilters.AsSpan());
            WaveBuffersCount = parameter.WaveBuffersCount;
            WaveBuffersIndex = parameter.WaveBuffersIndex;

            if (behaviourContext.IsFlushVoiceWaveBuffersSupported())
            {
                FlushWaveBufferCount += parameter.FlushWaveBufferCount;
            }

            MixId = parameter.MixId;

            if (behaviourContext.IsSplitterSupported())
            {
                SplitterId = parameter.SplitterId;
            }
            else
            {
                SplitterId = Constants.UnusedSplitterId;
            }

            parameter.ChannelResourceIds.AsSpan().CopyTo(ChannelResourceIds.AsSpan());

            DecodingBehaviour behaviour = DecodingBehaviour.Default;

            if (behaviourContext.IsDecodingBehaviourFlagSupported())
            {
                behaviour = parameter.DecodingBehaviourFlags;
            }

            DecodingBehaviour = behaviour;

            if (parameter.ResetVoiceDropFlag)
            {
                VoiceDropFlag = false;
            }

            if (ShouldUpdateParameters(in parameter))
            {
                DataSourceStateUnmapped = !poolMapper.TryAttachBuffer(out outErrorInfo, ref DataSourceStateAddressInfo, parameter.DataSourceStateAddress, parameter.DataSourceStateSize);
            }
            else
            {
                outErrorInfo = new ErrorInfo();
            }
        }

        /// <summary>
        /// Update the internal play state from user play state.
        /// </summary>
        /// <param name="userPlayState">The target user play state.</param>
        public void UpdatePlayState(Common.PlayState userPlayState)
        {
            PlayState oldServerPlayState = PlayState;

            PreviousPlayState = oldServerPlayState;

            PlayState newServerPlayState;

            switch (userPlayState)
            {
                case Common.PlayState.Start:
                    newServerPlayState = PlayState.Started;
                    break;

                case Common.PlayState.Stop:
                    if (oldServerPlayState == PlayState.Stopped)
                    {
                        return;
                    }

                    newServerPlayState = PlayState.Stopping;
                    break;

                case Common.PlayState.Pause:
                    newServerPlayState = PlayState.Paused;
                    break;

                default:
                    throw new NotImplementedException($"Unhandled PlayState.{userPlayState}");
            }

            PlayState = newServerPlayState;
        }

        /// <summary>
        /// Write the status of the voice to the given user output.
        /// </summary>
        /// <param name="outStatus">The given user output.</param>
        /// <param name="parameter">The user parameter.</param>
        /// <param name="voiceUpdateStates">The voice states associated to the <see cref="VoiceState"/>.</param>
        public void WriteOutStatus(ref VoiceOutStatus outStatus, in VoiceInParameter parameter, ReadOnlySpan<Memory<VoiceUpdateState>> voiceUpdateStates)
        {
#if DEBUG
            // Sanity check in debug mode of the internal state
            if (!parameter.IsNew && !IsNew)
            {
                for (int i = 1; i < ChannelsCount; i++)
                {
                    ref VoiceUpdateState stateA = ref voiceUpdateStates[i - 1].Span[0];
                    ref VoiceUpdateState stateB = ref voiceUpdateStates[i].Span[0];

                    Debug.Assert(stateA.WaveBufferConsumed == stateB.WaveBufferConsumed);
                    Debug.Assert(stateA.PlayedSampleCount == stateB.PlayedSampleCount);
                    Debug.Assert(stateA.Offset == stateB.Offset);
                    Debug.Assert(stateA.WaveBufferIndex == stateB.WaveBufferIndex);
                    Debug.Assert(stateA.Fraction == stateB.Fraction);
                    Debug.Assert(stateA.IsWaveBufferValid.SequenceEqual(stateB.IsWaveBufferValid));
                }
            }
#endif
            if (parameter.IsNew || IsNew)
            {
                IsNew = true;

                outStatus.VoiceDropFlag = false;
                outStatus.PlayedWaveBuffersCount = 0;
                outStatus.PlayedSampleCount = 0;
            }
            else
            {
                ref VoiceUpdateState state = ref voiceUpdateStates[0].Span[0];

                outStatus.VoiceDropFlag = VoiceDropFlag;
                outStatus.PlayedWaveBuffersCount = state.WaveBufferConsumed;
                outStatus.PlayedSampleCount = state.PlayedSampleCount;
            }
        }

        /// <summary>
        /// Update the internal state of all the <see cref="WaveBuffer"/> of the <see cref="VoiceState"/>.
        /// </summary>
        /// <param name="errorInfos">An array of <see cref="ErrorInfo"/> used to report errors when mapping any of the <see cref="WaveBuffer"/>.</param>
        /// <param name="parameter">The user parameter.</param>
        /// <param name="voiceUpdateStates">The voice states associated to the <see cref="VoiceState"/>.</param>
        /// <param name="mapper">The mapper to use.</param>
        /// <param name="behaviourContext">The behaviour context.</param>
        public void UpdateWaveBuffers(
            out ErrorInfo[] errorInfos,
            in VoiceInParameter parameter,
            ReadOnlySpan<Memory<VoiceUpdateState>> voiceUpdateStates,
            PoolMapper mapper,
            ref BehaviourContext behaviourContext)
        {
            errorInfos = new ErrorInfo[Constants.VoiceWaveBufferCount * 2];

            if (parameter.IsNew)
            {
                InitializeWaveBuffers();

                for (int i = 0; i < parameter.ChannelCount; i++)
                {
                    voiceUpdateStates[i].Span[0].IsWaveBufferValid.Clear();
                }
            }

            ref VoiceUpdateState voiceUpdateState = ref voiceUpdateStates[0].Span[0];

            for (int i = 0; i < Constants.VoiceWaveBufferCount; i++)
            {
                UpdateWaveBuffer(errorInfos.AsSpan(i * 2, 2), ref WaveBuffers[i], ref parameter.WaveBuffers[i], parameter.SampleFormat, voiceUpdateState.IsWaveBufferValid[i], mapper, ref behaviourContext);
            }
        }

        /// <summary>
        /// Update the internal state of one of the <see cref="WaveBuffer"/> of the <see cref="VoiceState"/>.
        /// </summary>
        /// <param name="errorInfos">A <see cref="Span{ErrorInfo}"/> used to report errors when mapping the <see cref="WaveBuffer"/>.</param>
        /// <param name="waveBuffer">The <see cref="WaveBuffer"/> to update.</param>
        /// <param name="inputWaveBuffer">The <see cref="WaveBufferInternal"/> from the user input.</param>
        /// <param name="sampleFormat">The <see cref="SampleFormat"/> from the user input.</param>
        /// <param name="isValid">If set to true, the server side wavebuffer is considered valid.</param>
        /// <param name="mapper">The mapper to use.</param>
        /// <param name="behaviourContext">The behaviour context.</param>
        private void UpdateWaveBuffer(
            Span<ErrorInfo> errorInfos,
            ref WaveBuffer waveBuffer,
            ref WaveBufferInternal inputWaveBuffer,
            SampleFormat sampleFormat,
            bool isValid,
            PoolMapper mapper,
            ref BehaviourContext behaviourContext)
        {
            if (!isValid && waveBuffer.IsSendToAudioProcessor && waveBuffer.BufferAddressInfo.CpuAddress != 0)
            {
                mapper.ForceUnmap(ref waveBuffer.BufferAddressInfo);
                waveBuffer.BufferAddressInfo.Setup(0, 0);
            }

            if (!inputWaveBuffer.SentToServer || BufferInfoUnmapped)
            {
                if (inputWaveBuffer.IsSampleOffsetValid(sampleFormat))
                {
                    Debug.Assert(waveBuffer.IsSendToAudioProcessor);

                    waveBuffer.IsSendToAudioProcessor = false;
                    waveBuffer.StartSampleOffset = inputWaveBuffer.StartSampleOffset;
                    waveBuffer.EndSampleOffset = inputWaveBuffer.EndSampleOffset;
                    waveBuffer.ShouldLoop = inputWaveBuffer.ShouldLoop;
                    waveBuffer.IsEndOfStream = inputWaveBuffer.IsEndOfStream;
                    waveBuffer.LoopStartSampleOffset = inputWaveBuffer.LoopFirstSampleOffset;
                    waveBuffer.LoopEndSampleOffset = inputWaveBuffer.LoopLastSampleOffset;
                    waveBuffer.LoopCount = inputWaveBuffer.LoopCount;

                    BufferInfoUnmapped = !mapper.TryAttachBuffer(out ErrorInfo bufferInfoError, ref waveBuffer.BufferAddressInfo, inputWaveBuffer.Address, inputWaveBuffer.Size);

                    errorInfos[0] = bufferInfoError;

                    if (sampleFormat == SampleFormat.Adpcm && behaviourContext.IsAdpcmLoopContextBugFixed() && inputWaveBuffer.ContextAddress != 0)
                    {
                        bool adpcmLoopContextMapped = mapper.TryAttachBuffer(out ErrorInfo adpcmLoopContextInfoError,
                                                                             ref waveBuffer.ContextAddressInfo,
                                                                             inputWaveBuffer.ContextAddress,
                                                                             inputWaveBuffer.ContextSize);

                        errorInfos[1] = adpcmLoopContextInfoError;

                        if (adpcmLoopContextMapped)
                        {
                            BufferInfoUnmapped = DataSourceStateUnmapped;
                        }
                        else
                        {
                            BufferInfoUnmapped = true;
                        }
                    }
                    else
                    {
                        waveBuffer.ContextAddressInfo.Setup(0, 0);
                    }
                }
                else
                {
                    errorInfos[0].ErrorCode = ResultCode.InvalidAddressInfo;
                    errorInfos[0].ExtraErrorInfo = inputWaveBuffer.Address;
                }
            }
        }

        /// <summary>
        /// Reset the resources associated to this <see cref="VoiceState"/>.
        /// </summary>
        /// <param name="context">The voice context.</param>
        private void ResetResources(VoiceContext context)
        {
            for (int i = 0; i < ChannelsCount; i++)
            {
                int channelResourceId = ChannelResourceIds[i];

                ref VoiceChannelResource voiceChannelResource = ref context.GetChannelResource(channelResourceId);

                Debug.Assert(voiceChannelResource.IsUsed);

                Memory<VoiceUpdateState> dspSharedState = context.GetUpdateStateForDsp(channelResourceId);

                MemoryMarshal.Cast<VoiceUpdateState, byte>(dspSharedState.Span).Clear();

                voiceChannelResource.UpdateState();
            }
        }

        /// <summary>
        /// Flush a certain amount of <see cref="WaveBuffer"/>.
        /// </summary>
        /// <param name="waveBufferCount">The amount of wavebuffer to flush.</param>
        /// <param name="voiceUpdateStates">The voice states associated to the <see cref="VoiceState"/>.</param>
        /// <param name="channelCount">The channel count from user input.</param>
        private void FlushWaveBuffers(uint waveBufferCount, Memory<VoiceUpdateState>[] voiceUpdateStates, uint channelCount)
        {
            uint waveBufferIndex = WaveBuffersIndex;

            for (int i = 0; i < waveBufferCount; i++)
            {
                WaveBuffers[(int)waveBufferIndex].IsSendToAudioProcessor = true;

                for (int j = 0; j < channelCount; j++)
                {
                    ref VoiceUpdateState voiceUpdateState = ref voiceUpdateStates[j].Span[0];

                    voiceUpdateState.WaveBufferIndex = (voiceUpdateState.WaveBufferIndex + 1) % Constants.VoiceWaveBufferCount;
                    voiceUpdateState.WaveBufferConsumed++;
                    voiceUpdateState.IsWaveBufferValid[(int)waveBufferIndex] = false;
                }

                waveBufferIndex = (waveBufferIndex + 1) % Constants.VoiceWaveBufferCount;
            }
        }

        /// <summary>
        /// Update the internal parameters for command generation.
        /// </summary>
        /// <param name="voiceUpdateStates">The voice states associated to the <see cref="VoiceState"/>.</param>
        /// <returns>Return true if this voice should be played.</returns>
        public bool UpdateParametersForCommandGeneration(Memory<VoiceUpdateState>[] voiceUpdateStates)
        {
            if (FlushWaveBufferCount != 0)
            {
                FlushWaveBuffers(FlushWaveBufferCount, voiceUpdateStates, ChannelsCount);

                FlushWaveBufferCount = 0;
            }

            switch (PlayState)
            {
                case PlayState.Started:
                    for (int i = 0; i < WaveBuffers.Length; i++)
                    {
                        ref WaveBuffer wavebuffer = ref WaveBuffers[i];

                        if (!wavebuffer.IsSendToAudioProcessor)
                        {
                            for (int y = 0; y < ChannelsCount; y++)
                            {
                                Debug.Assert(!voiceUpdateStates[y].Span[0].IsWaveBufferValid[i]);

                                voiceUpdateStates[y].Span[0].IsWaveBufferValid[i] = true;
                            }

                            wavebuffer.IsSendToAudioProcessor = true;
                        }
                    }

                    WasPlaying = false;

                    ref VoiceUpdateState primaryVoiceUpdateState = ref voiceUpdateStates[0].Span[0];

                    for (int i = 0; i < primaryVoiceUpdateState.IsWaveBufferValid.Length; i++)
                    {
                        if (primaryVoiceUpdateState.IsWaveBufferValid[i])
                        {
                            return true;
                        }
                    }

                    return false;

                case PlayState.Stopping:
                    for (int i = 0; i < WaveBuffers.Length; i++)
                    {
                        ref WaveBuffer wavebuffer = ref WaveBuffers[i];

                        wavebuffer.IsSendToAudioProcessor = true;

                        for (int j = 0; j < ChannelsCount; j++)
                        {
                            ref VoiceUpdateState voiceUpdateState = ref voiceUpdateStates[j].Span[0];

                            if (voiceUpdateState.IsWaveBufferValid[i])
                            {
                                voiceUpdateState.WaveBufferIndex = (voiceUpdateState.WaveBufferIndex + 1) % Constants.VoiceWaveBufferCount;
                                voiceUpdateState.WaveBufferConsumed++;
                            }

                            voiceUpdateState.IsWaveBufferValid[i] = false;
                        }
                    }

                    for (int i = 0; i < ChannelsCount; i++)
                    {
                        ref VoiceUpdateState voiceUpdateState = ref voiceUpdateStates[i].Span[0];

                        voiceUpdateState.Offset = 0;
                        voiceUpdateState.PlayedSampleCount = 0;
                        voiceUpdateState.Pitch.AsSpan().Clear();
                        voiceUpdateState.Fraction = 0;
                        voiceUpdateState.LoopContext = new AdpcmLoopContext();
                    }

                    PlayState = PlayState.Stopped;
                    WasPlaying = PreviousPlayState == PlayState.Started;

                    return WasPlaying;

                case PlayState.Stopped:
                case PlayState.Paused:
                    foreach (ref WaveBuffer wavebuffer in WaveBuffers.AsSpan())
                    {
                        wavebuffer.BufferAddressInfo.GetReference(true);
                        wavebuffer.ContextAddressInfo.GetReference(true);
                    }

                    if (SampleFormat == SampleFormat.Adpcm)
                    {
                        if (DataSourceStateAddressInfo.CpuAddress != 0)
                        {
                            DataSourceStateAddressInfo.GetReference(true);
                        }
                    }

                    WasPlaying = PreviousPlayState == PlayState.Started;

                    return WasPlaying;
                default:
                    throw new NotImplementedException($"{PlayState}");
            }
        }

        /// <summary>
        /// Update the internal state for command generation.
        /// </summary>
        /// <param name="context">The voice context.</param>
        /// <returns>Return true if this voice should be played.</returns>
        public bool UpdateForCommandGeneration(VoiceContext context)
        {
            if (IsNew)
            {
                ResetResources(context);
                PreviousVolume = Volume;
                IsNew = false;
            }

            Memory<VoiceUpdateState>[] voiceUpdateStates = new Memory<VoiceUpdateState>[Constants.VoiceChannelCountMax];

            for (int i = 0; i < ChannelsCount; i++)
            {
                voiceUpdateStates[i] = context.GetUpdateStateForDsp(ChannelResourceIds[i]);
            }

            return UpdateParametersForCommandGeneration(voiceUpdateStates);
        }
    }
}
