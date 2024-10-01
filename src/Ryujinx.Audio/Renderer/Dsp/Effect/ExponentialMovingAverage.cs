namespace Ryujinx.Audio.Renderer.Dsp.Effect
{
    public struct ExponentialMovingAverage
    {
        private float _mean;

        public ExponentialMovingAverage(float mean)
        {
            _mean = mean;
        }

        public readonly float Read()
        {
            return _mean;
        }

        public float Update(float value, float alpha)
        {
            _mean += alpha * (value - _mean);

            return _mean;
        }
    }
}
