using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class VolumeRampCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.VolumeRamp;

        public uint EstimatedProcessingTime { get; set; }

        public ushort InputBufferIndex { get; }
        public ushort OutputBufferIndex { get; }

        public float Volume0 { get; }
        public float Volume1 { get; }

        public VolumeRampCommand(float volume0, float volume1, uint bufferIndex, int nodeId)
        {
            Enabled = true;
            NodeId = nodeId;

            InputBufferIndex = (ushort)bufferIndex;
            OutputBufferIndex = (ushort)bufferIndex;

            Volume0 = volume0;
            Volume1 = volume1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProcessVolumeRamp(Span<float> outputBuffer, ReadOnlySpan<float> inputBuffer, int sampleCount)
        {
            float ramp = (Volume1 - Volume0) / sampleCount;

            float volume = Volume0;

            for (int i = 0; i < sampleCount; i++)
            {
                outputBuffer[i] = FloatingPointHelper.MultiplyRoundUp(inputBuffer[i], volume);
                volume += ramp;
            }
        }

        public void Process(CommandList context)
        {
            ReadOnlySpan<float> inputBuffer = context.GetBuffer(InputBufferIndex);
            Span<float> outputBuffer = context.GetBuffer(OutputBufferIndex);

            ProcessVolumeRamp(outputBuffer, inputBuffer, (int)context.SampleCount);
        }
    }
}
