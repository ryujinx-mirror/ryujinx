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

namespace Ryujinx.Audio.Renderer.Dsp.Effect
{
    public class DelayLine : IDelayLine
    {
        private float[] _workBuffer;
        private uint _sampleRate;
        private uint _currentSampleIndex;
        private uint _lastSampleIndex;

        public uint CurrentSampleCount { get; private set; }
        public uint SampleCountMax { get; private set; }

        public DelayLine(uint sampleRate, float delayTimeMax)
        {
            _sampleRate = sampleRate;
            SampleCountMax = IDelayLine.GetSampleCount(_sampleRate, delayTimeMax);
            _workBuffer = new float[SampleCountMax];

            SetDelay(delayTimeMax);
        }

        private void ConfigureDelay(uint targetSampleCount)
        {
            CurrentSampleCount = Math.Min(SampleCountMax, targetSampleCount);
            _currentSampleIndex = 0;

            if (CurrentSampleCount == 0)
            {
                _lastSampleIndex = 0;
            }
            else
            {
                _lastSampleIndex = CurrentSampleCount - 1;
            }
        }

        public void SetDelay(float delayTime)
        {
            ConfigureDelay(IDelayLine.GetSampleCount(_sampleRate, delayTime));
        }

        public float Read()
        {
            return _workBuffer[_currentSampleIndex];
        }

        public float Update(float value)
        {
            float output = Read();

            _workBuffer[_currentSampleIndex++] = value;

            if (_currentSampleIndex >= _lastSampleIndex)
            {
                _currentSampleIndex = 0;
            }

            return output;
        }

        public float TapUnsafe(uint sampleIndex, int offset)
        {
            return IDelayLine.Tap(_workBuffer, (int)_currentSampleIndex, (int)sampleIndex + offset, (int)CurrentSampleCount);
        }

        public float Tap(uint sampleIndex)
        {
            if (sampleIndex >= CurrentSampleCount)
            {
                sampleIndex = CurrentSampleCount - 1;
            }

            return TapUnsafe(sampleIndex, -1);
        }
    }
}
