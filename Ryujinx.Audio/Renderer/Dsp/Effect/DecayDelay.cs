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

namespace Ryujinx.Audio.Renderer.Dsp.Effect
{
    public class DecayDelay : IDelayLine
    {
        private readonly IDelayLine _delayLine;

        public uint CurrentSampleCount => _delayLine.CurrentSampleCount;

        public uint SampleCountMax => _delayLine.SampleCountMax;

        private float _decayRate;

        public DecayDelay(IDelayLine delayLine)
        {
            _decayRate = 0.0f;
            _delayLine = delayLine;
        }

        public void SetDecayRate(float decayRate)
        {
            _decayRate = decayRate;
        }

        public float Update(float value)
        {
            float delayLineValue = _delayLine.Read();
            float processedValue = value - (_decayRate * delayLineValue);

            return _delayLine.Update(processedValue) + processedValue * _decayRate;
        }

        public void SetDelay(float delayTime)
        {
            _delayLine.SetDelay(delayTime);
        }

        public float Read()
        {
            return _delayLine.Read();
        }

        public float TapUnsafe(uint sampleIndex, int offset)
        {
            return _delayLine.TapUnsafe(sampleIndex, offset);
        }

        public float Tap(uint sampleIndex)
        {
            return _delayLine.Tap(sampleIndex);
        }
    }
}
