using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp
{
    public static class PcmHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCountToDecode(int startSampleOffset, int endSampleOffset, int offset, int count)
        {
            return Math.Min(count, endSampleOffset - startSampleOffset - offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong GetBufferOffset<T>(int startSampleOffset, int offset, int channelCount) where T : unmanaged
        {
            return (ulong)(Unsafe.SizeOf<T>() * channelCount * (startSampleOffset + offset));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetBufferSize<T>(int startSampleOffset, int endSampleOffset, int offset, int count) where T : unmanaged
        {
            if (endSampleOffset < startSampleOffset)
            {
                return 0;
            }

            return GetCountToDecode(startSampleOffset, endSampleOffset, offset, count) * Unsafe.SizeOf<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ConvertSampleToPcmFloat(short sample)
        {
            return (float)sample / short.MaxValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ConvertSampleToPcmInt16(float sample)
        {
            return Saturate(sample * short.MaxValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertSampleToPcm8(Span<sbyte> output, ReadOnlySpan<short> input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                // Output most significant byte
                output[i] = (sbyte)(input[i] >> 8);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertSampleToPcm24(Span<byte> output, ReadOnlySpan<short> input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                output[i * 3 + 2] = (byte)(input[i] >> 8);
                output[i * 3 + 1] = (byte)(input[i] & 0xff);
                output[i * 3 + 0] = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertSampleToPcm32(Span<int> output, ReadOnlySpan<short> input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = input[i] << 16;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConvertSampleToPcmFloat(Span<float> output, ReadOnlySpan<short> input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                output[i] = ConvertSampleToPcmFloat(input[i]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Decode(Span<short> output, ReadOnlySpan<short> input, int startSampleOffset, int endSampleOffset, int channelIndex, int channelCount)
        {
            if (input.IsEmpty || endSampleOffset < startSampleOffset)
            {
                return 0;
            }

            int decodedCount = input.Length / channelCount;

            for (int i = 0; i < decodedCount; i++)
            {
                output[i] = input[i * channelCount + channelIndex];
            }

            return decodedCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Decode(Span<short> output, ReadOnlySpan<float> input, int startSampleOffset, int endSampleOffset, int channelIndex, int channelCount)
        {
            if (input.IsEmpty || endSampleOffset < startSampleOffset)
            {
                return 0;
            }

            int decodedCount = input.Length / channelCount;

            for (int i = 0; i < decodedCount; i++)
            {
                output[i] = ConvertSampleToPcmInt16(input[i * channelCount + channelIndex]);
            }

            return decodedCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short Saturate(float value)
        {
            if (value > short.MaxValue)
            {
                return short.MaxValue;
            }

            if (value < short.MinValue)
            {
                return short.MinValue;
            }

            return (short)value;
        }
    }
}
