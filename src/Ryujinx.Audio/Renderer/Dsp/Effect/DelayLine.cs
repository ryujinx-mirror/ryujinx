using System;

namespace Ryujinx.Audio.Renderer.Dsp.Effect
{
    public class DelayLine : IDelayLine
    {
        private readonly float[] _workBuffer;
        private readonly uint _sampleRate;
        private uint _currentSampleIndex;
        private uint _lastSampleIndex;

        public uint CurrentSampleCount { get; private set; }
        public uint SampleCountMax { get; private set; }

        public DelayLine(uint sampleRate, float delayTimeMax)
        {
            _sampleRate = sampleRate;
            SampleCountMax = IDelayLine.GetSampleCount(_sampleRate, delayTimeMax);
            _workBuffer = new float[SampleCountMax + 1];

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
