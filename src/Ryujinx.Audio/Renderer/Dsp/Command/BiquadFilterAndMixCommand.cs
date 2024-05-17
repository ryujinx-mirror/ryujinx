using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Audio.Renderer.Parameter;
using System;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class BiquadFilterAndMixCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.BiquadFilterAndMix;

        public uint EstimatedProcessingTime { get; set; }

        public ushort InputBufferIndex { get; }
        public ushort OutputBufferIndex { get; }

        private BiquadFilterParameter _parameter;

        public Memory<BiquadFilterState> BiquadFilterState { get; }
        public Memory<BiquadFilterState> PreviousBiquadFilterState { get; }

        public Memory<VoiceUpdateState> State { get; }

        public int LastSampleIndex { get; }

        public float Volume0 { get; }
        public float Volume1 { get; }

        public bool NeedInitialization { get; }
        public bool HasVolumeRamp { get; }
        public bool IsFirstMixBuffer { get; }

        public BiquadFilterAndMixCommand(
            float volume0,
            float volume1,
            uint inputBufferIndex,
            uint outputBufferIndex,
            int lastSampleIndex,
            Memory<VoiceUpdateState> state,
            ref BiquadFilterParameter filter,
            Memory<BiquadFilterState> biquadFilterState,
            Memory<BiquadFilterState> previousBiquadFilterState,
            bool needInitialization,
            bool hasVolumeRamp,
            bool isFirstMixBuffer,
            int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;

            InputBufferIndex = (ushort)inputBufferIndex;
            OutputBufferIndex = (ushort)outputBufferIndex;

            _parameter = filter;
            BiquadFilterState = biquadFilterState;
            PreviousBiquadFilterState = previousBiquadFilterState;

            State = state;
            LastSampleIndex = lastSampleIndex;

            Volume0 = volume0;
            Volume1 = volume1;

            NeedInitialization = needInitialization;
            HasVolumeRamp = hasVolumeRamp;
            IsFirstMixBuffer = isFirstMixBuffer;
        }

        public void Process(CommandList context)
        {
            ReadOnlySpan<float> inputBuffer = context.GetBuffer(InputBufferIndex);
            Span<float> outputBuffer = context.GetBuffer(OutputBufferIndex);

            if (NeedInitialization)
            {
                // If there is no previous state, initialize to zero.

                BiquadFilterState.Span[0] = new BiquadFilterState();
            }
            else if (IsFirstMixBuffer)
            {
                // This is the first buffer, set previous state to current state.

                PreviousBiquadFilterState.Span[0] = BiquadFilterState.Span[0];
            }
            else
            {
                // Rewind the current state by copying back the previous state.

                BiquadFilterState.Span[0] = PreviousBiquadFilterState.Span[0];
            }

            if (HasVolumeRamp)
            {
                float volume = Volume0;
                float ramp = (Volume1 - Volume0) / (int)context.SampleCount;

                State.Span[0].LastSamples[LastSampleIndex] = BiquadFilterHelper.ProcessBiquadFilterAndMixRamp(
                    ref _parameter,
                    ref BiquadFilterState.Span[0],
                    outputBuffer,
                    inputBuffer,
                    context.SampleCount,
                    volume,
                    ramp);
            }
            else
            {
                BiquadFilterHelper.ProcessBiquadFilterAndMix(
                    ref _parameter,
                    ref BiquadFilterState.Span[0],
                    outputBuffer,
                    inputBuffer,
                    context.SampleCount,
                    Volume1);
            }
        }
    }
}
