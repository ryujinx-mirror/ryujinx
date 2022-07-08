using Ryujinx.Audio.Renderer.Parameter.Effect;
using System;

namespace Ryujinx.Audio.Renderer.Dsp.State
{
    public class LimiterState
    {
        public float[] DectectorAverage;
        public float[] CompressionGain;
        public float[] DelayedSampleBuffer;
        public int[] DelayedSampleBufferPosition;

        public LimiterState(ref LimiterParameter parameter, ulong workBuffer)
        {
            DectectorAverage = new float[parameter.ChannelCount];
            CompressionGain = new float[parameter.ChannelCount];
            DelayedSampleBuffer = new float[parameter.ChannelCount * parameter.DelayBufferSampleCountMax];
            DelayedSampleBufferPosition = new int[parameter.ChannelCount];

            DectectorAverage.AsSpan().Fill(0.0f);
            CompressionGain.AsSpan().Fill(1.0f);
            DelayedSampleBufferPosition.AsSpan().Fill(0);
            DelayedSampleBuffer.AsSpan().Fill(0.0f);

            UpdateParameter(ref parameter);
        }

        public void UpdateParameter(ref LimiterParameter parameter) {}
    }
}
