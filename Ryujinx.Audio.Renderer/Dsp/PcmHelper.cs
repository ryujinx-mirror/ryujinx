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
            return GetCountToDecode(startSampleOffset, endSampleOffset, offset, count) * Unsafe.SizeOf<T>();
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
                output[i] = (short)(input[i * channelCount + channelIndex] * short.MaxValue);
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
