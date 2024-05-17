using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Dsp.State;
using Ryujinx.Audio.Renderer.Parameter;
using System;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class MultiTapBiquadFilterAndMixCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.MultiTapBiquadFilterAndMix;

        public uint EstimatedProcessingTime { get; set; }

        public ushort InputBufferIndex { get; }
        public ushort OutputBufferIndex { get; }

        private BiquadFilterParameter _parameter0;
        private BiquadFilterParameter _parameter1;

        public Memory<BiquadFilterState> BiquadFilterState0 { get; }
        public Memory<BiquadFilterState> BiquadFilterState1 { get; }
        public Memory<BiquadFilterState> PreviousBiquadFilterState0 { get; }
        public Memory<BiquadFilterState> PreviousBiquadFilterState1 { get; }

        public Memory<VoiceUpdateState> State { get; }

        public int LastSampleIndex { get; }

        public float Volume0 { get; }
        public float Volume1 { get; }

        public bool NeedInitialization0 { get; }
        public bool NeedInitialization1 { get; }
        public bool HasVolumeRamp { get; }
        public bool IsFirstMixBuffer { get; }

        public MultiTapBiquadFilterAndMixCommand(
            float volume0,
            float volume1,
            uint inputBufferIndex,
            uint outputBufferIndex,
            int lastSampleIndex,
            Memory<VoiceUpdateState> state,
            ref BiquadFilterParameter filter0,
            ref BiquadFilterParameter filter1,
            Memory<BiquadFilterState> biquadFilterState0,
            Memory<BiquadFilterState> biquadFilterState1,
            Memory<BiquadFilterState> previousBiquadFilterState0,
            Memory<BiquadFilterState> previousBiquadFilterState1,
            bool needInitialization0,
            bool needInitialization1,
            bool hasVolumeRamp,
            bool isFirstMixBuffer,
            int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;

            InputBufferIndex = (ushort)inputBufferIndex;
            OutputBufferIndex = (ushort)outputBufferIndex;

            _parameter0 = filter0;
            _parameter1 = filter1;
            BiquadFilterState0 = biquadFilterState0;
            BiquadFilterState1 = biquadFilterState1;
            PreviousBiquadFilterState0 = previousBiquadFilterState0;
            PreviousBiquadFilterState1 = previousBiquadFilterState1;

            State = state;
            LastSampleIndex = lastSampleIndex;

            Volume0 = volume0;
            Volume1 = volume1;

            NeedInitialization0 = needInitialization0;
            NeedInitialization1 = needInitialization1;
            HasVolumeRamp = hasVolumeRamp;
            IsFirstMixBuffer = isFirstMixBuffer;
        }

        private void UpdateState(Memory<BiquadFilterState> state, Memory<BiquadFilterState> previousState, bool needInitialization)
        {
            if (needInitialization)
            {
                // If there is no previous state, initialize to zero.

                state.Span[0] = new BiquadFilterState();
            }
            else if (IsFirstMixBuffer)
            {
                // This is the first buffer, set previous state to current state.

                previousState.Span[0] = state.Span[0];
            }
            else
            {
                // Rewind the current state by copying back the previous state.

                state.Span[0] = previousState.Span[0];
            }
        }

        public void Process(CommandList context)
        {
            ReadOnlySpan<float> inputBuffer = context.GetBuffer(InputBufferIndex);
            Span<float> outputBuffer = context.GetBuffer(OutputBufferIndex);

            UpdateState(BiquadFilterState0, PreviousBiquadFilterState0, NeedInitialization0);
            UpdateState(BiquadFilterState1, PreviousBiquadFilterState1, NeedInitialization1);

            if (HasVolumeRamp)
            {
                float volume = Volume0;
                float ramp = (Volume1 - Volume0) / (int)context.SampleCount;

                State.Span[0].LastSamples[LastSampleIndex] = BiquadFilterHelper.ProcessDoubleBiquadFilterAndMixRamp(
                    ref _parameter0,
                    ref _parameter1,
                    ref BiquadFilterState0.Span[0],
                    ref BiquadFilterState1.Span[0],
                    outputBuffer,
                    inputBuffer,
                    context.SampleCount,
                    volume,
                    ramp);
            }
            else
            {
                BiquadFilterHelper.ProcessDoubleBiquadFilterAndMix(
                    ref _parameter0,
                    ref _parameter1,
                    ref BiquadFilterState0.Span[0],
                    ref BiquadFilterState1.Span[0],
                    outputBuffer,
                    inputBuffer,
                    context.SampleCount,
                    Volume1);
            }
        }
    }
}
