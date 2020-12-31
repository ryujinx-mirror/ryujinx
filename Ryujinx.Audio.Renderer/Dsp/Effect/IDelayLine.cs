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
