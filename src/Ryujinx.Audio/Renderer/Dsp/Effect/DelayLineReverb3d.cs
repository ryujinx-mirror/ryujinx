using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Dsp.Effect
{
    public class DelayLine3d : IDelayLine
    {
        private readonly float[] _workBuffer;
        private readonly uint _sampleRate;
        private uint _currentSampleIndex;
        private uint _lastSampleIndex;

        public uint CurrentSampleCount { get; private set; }
        public uint SampleCountMax { get; private set; }

        public DelayLine3d(uint sampleRate, float delayTimeMax)
        {
            _sampleRate = sampleRate;
            SampleCountMax = IDelayLine.GetSampleCount(_sampleRate, delayTimeMax);
            _workBuffer = new float[SampleCountMax + 1];

            SetDelay(delayTimeMax);
        }

        private void ConfigureDelay(uint targetSampleCount)
        {
            if (SampleCountMax >= targetSampleCount)
            {
                CurrentSampleCount = targetSampleCount;
                _lastSampleIndex = (_currentSampleIndex + targetSampleCount) % (SampleCountMax + 1);
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
            Debug.Assert(!float.IsNaN(value) && !float.IsInfinity(value));

            _workBuffer[_lastSampleIndex++] = value;

            float output = Read();

            _currentSampleIndex++;

            if (_currentSampleIndex >= SampleCountMax)
            {
                _currentSampleIndex = 0;
            }

            if (_lastSampleIndex >= SampleCountMax)
            {
                _lastSampleIndex = 0;
            }

            return output;
        }

        public float TapUnsafe(uint sampleIndex, int offset)
        {
            return IDelayLine.Tap(_workBuffer, (int)_lastSampleIndex, (int)sampleIndex + offset, (int)SampleCountMax + 1);
        }

        public float Tap(uint sampleIndex)
        {
            return TapUnsafe(sampleIndex, -1);
        }
    }
}
