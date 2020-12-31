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
    public static class FloatingPointHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MultiplyRoundDown(float a, float b)
        {
            return RoundDown(a * b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RoundDown(float a)
        {
            return MathF.Round(a, 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float RoundUp(float a)
        {
            return MathF.Round(a);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MultiplyRoundUp(float a, float b)
        {
            return RoundUp(a * b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pow10(float x)
        {
            // NOTE: Nintendo implementation uses Q15 and a LUT for this, we don't.
            // As such, we support the same ranges as Nintendo to avoid unexpected behaviours.
            if (x >= 0.0f)
            {
                return 1.0f;
            }
            else if (x <= -5.3f)
            {
                return 0.0f;
            }

            return MathF.Pow(10, x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DegreesToRadians(float degrees)
        {
            return degrees * MathF.PI / 180.0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(float value)
        {
            return MathF.Cos(DegreesToRadians(value));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(float value)
        {
            return MathF.Sin(DegreesToRadians(value));
        }
    }
}
