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
