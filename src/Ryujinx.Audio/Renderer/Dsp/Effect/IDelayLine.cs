using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp.Effect
{
    public interface IDelayLine
    {
        uint CurrentSampleCount { get; }
        uint SampleCountMax { get; }

        void SetDelay(float delayTime);
        float Read();
        float Update(float value);

        float TapUnsafe(uint sampleIndex, int offset);
        float Tap(uint sampleIndex);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Tap(Span<float> workBuffer, int baseIndex, int sampleIndex, int delaySampleCount)
        {
            int targetIndex = baseIndex - sampleIndex;

            if (targetIndex < 0)
            {
                targetIndex += delaySampleCount;
            }

            return workBuffer[targetIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetSampleCount(uint sampleRate, float delayTime)
        {
            return (uint)MathF.Round(sampleRate * delayTime);
        }
    }
}
