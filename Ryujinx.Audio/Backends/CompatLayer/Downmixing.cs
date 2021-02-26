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
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Backends.CompatLayer
{
    public static class Downmixing
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Channel51FormatPCM16
        {
            public short FrontLeft;
            public short FrontRight;
            public short FrontCenter;
            public short LowFrequency;
            public short BackLeft;
            public short BackRight;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ChannelStereoFormatPCM16
        {
            public short Left;
            public short Right;
        }

        private const int Q15Bits        = 16;
        private const int RawQ15One      = 1 << Q15Bits;
        private const int RawQ15HalfOne  = (int)(0.5f * RawQ15One);
        private const int Minus3dBInQ15  = (int)(0.707f * RawQ15One);
        private const int Minus6dBInQ15  = (int)(0.501f * RawQ15One);
        private const int Minus12dBInQ15 = (int)(0.251f * RawQ15One);

        private static readonly int[] DefaultSurroundToStereoCoefficients = new int[4]
        {
            RawQ15One,
            Minus3dBInQ15,
            Minus12dBInQ15,
            Minus3dBInQ15
        };

        private static readonly int[] DefaultStereoToMonoCoefficients = new int[2]
        {
            Minus6dBInQ15,
            Minus6dBInQ15
        };

        private const int SurroundChannelCount = 6;
        private const int StereoChannelCount   = 2;
        private const int MonoChannelCount     = 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<Channel51FormatPCM16> GetSurroundBuffer(ReadOnlySpan<short> data)
        {
            return MemoryMarshal.Cast<short, Channel51FormatPCM16>(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ReadOnlySpan<ChannelStereoFormatPCM16> GetStereoBuffer(ReadOnlySpan<short> data)
        {
            return MemoryMarshal.Cast<short, ChannelStereoFormatPCM16>(data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static short DownMixStereoToMono(ReadOnlySpan<int> coefficients, short left, short right)
        {
            return (short)((left * coefficients[0] + right * coefficients[1]) >> Q15Bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static short DownMixSurroundToStereo(ReadOnlySpan<int> coefficients, short back, short lfe, short center, short front)
        {
            return (short)((coefficients[3] * back + coefficients[2] * lfe + coefficients[1] * center + coefficients[0] * front + RawQ15HalfOne) >> Q15Bits);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static short[] DownMixSurroundToStereo(ReadOnlySpan<int> coefficients, ReadOnlySpan<short> data)
        {
            int samplePerChannelCount = data.Length / SurroundChannelCount;

            short[] downmixedBuffer = new short[samplePerChannelCount * StereoChannelCount];

            ReadOnlySpan<Channel51FormatPCM16> channels = GetSurroundBuffer(data);

            for (int i = 0; i < samplePerChannelCount; i++)
            {
                Channel51FormatPCM16 channel = channels[i];

                downmixedBuffer[i * 2]     = DownMixSurroundToStereo(coefficients, channel.BackLeft, channel.LowFrequency, channel.FrontCenter, channel.FrontLeft);
                downmixedBuffer[i * 2 + 1] = DownMixSurroundToStereo(coefficients, channel.BackRight, channel.LowFrequency, channel.FrontCenter, channel.FrontRight);
            }

            return downmixedBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static short[] DownMixStereoToMono(ReadOnlySpan<int> coefficients, ReadOnlySpan<short> data)
        {
            int samplePerChannelCount = data.Length / StereoChannelCount;

            short[] downmixedBuffer = new short[samplePerChannelCount * MonoChannelCount];

            ReadOnlySpan<ChannelStereoFormatPCM16> channels = GetStereoBuffer(data);

            for (int i = 0; i < samplePerChannelCount; i++)
            {
                ChannelStereoFormatPCM16 channel = channels[i];

                downmixedBuffer[i] = DownMixStereoToMono(coefficients, channel.Left, channel.Right);
            }

            return downmixedBuffer;
        }

        public static short[] DownMixStereoToMono(ReadOnlySpan<short> data)
        {
            return DownMixStereoToMono(DefaultStereoToMonoCoefficients, data);
        }

        public static short[] DownMixSurroundToStereo(ReadOnlySpan<short> data)
        {
            return DownMixSurroundToStereo(DefaultSurroundToStereoCoefficients, data);
        }
    }
}
