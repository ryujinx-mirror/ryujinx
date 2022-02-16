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
