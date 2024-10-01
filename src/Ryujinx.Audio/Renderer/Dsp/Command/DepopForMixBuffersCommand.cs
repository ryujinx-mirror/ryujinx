using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp.Command
{
    public class DepopForMixBuffersCommand : ICommand
    {
        public bool Enabled { get; set; }

        public int NodeId { get; }

        public CommandType CommandType => CommandType.DepopForMixBuffers;

        public uint EstimatedProcessingTime { get; set; }

        public uint MixBufferOffset { get; }

        public uint MixBufferCount { get; }

        public float Decay { get; }

        public Memory<float> DepopBuffer { get; }

        public DepopForMixBuffersCommand(Memory<float> depopBuffer, uint bufferOffset, uint mixBufferCount, int nodeId, uint sampleRate)
        {
            Enabled = true;
            NodeId = nodeId;
            MixBufferOffset = bufferOffset;
            MixBufferCount = mixBufferCount;
            DepopBuffer = depopBuffer;

            if (sampleRate == 48000)
            {
                Decay = 0.962189f;
            }
            else // if (sampleRate == 32000)
            {
                Decay = 0.943695f;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe float ProcessDepopMix(float* buffer, float depopValue, uint sampleCount)
        {
            if (depopValue < 0)
            {
                depopValue = -depopValue;

                for (int i = 0; i < sampleCount; i++)
                {
                    depopValue = FloatingPointHelper.MultiplyRoundDown(Decay, depopValue);

                    buffer[i] -= depopValue;
                }

                return -depopValue;
            }

            for (int i = 0; i < sampleCount; i++)
            {
                depopValue = FloatingPointHelper.MultiplyRoundDown(Decay, depopValue);

                buffer[i] += depopValue;
            }

            return depopValue;
        }

        public void Process(CommandList context)
        {
            Span<float> depopBuffer = DepopBuffer.Span;

            uint bufferCount = Math.Min(MixBufferOffset + MixBufferCount, context.BufferCount);

            for (int i = (int)MixBufferOffset; i < bufferCount; i++)
            {
                float depopValue = depopBuffer[i];
                if (depopValue != 0)
                {
                    unsafe
                    {
                        float* buffer = (float*)context.GetBufferPointer(i);

                        depopBuffer[i] = ProcessDepopMix(buffer, depopValue, context.SampleCount);
                    }
                }
            }
        }
    }
}
