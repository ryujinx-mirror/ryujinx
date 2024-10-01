using Ryujinx.Audio.Renderer.Dsp.Effect;
using Ryujinx.Audio.Renderer.Parameter.Effect;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Dsp.State
{
    public struct DelayState
    {
        public DelayLine[] DelayLines { get; }
        public float[] LowPassZ { get; set; }
        public float FeedbackGain { get; private set; }
        public float DelayFeedbackBaseGain { get; private set; }
        public float DelayFeedbackCrossGain { get; private set; }
        public float LowPassFeedbackGain { get; private set; }
        public float LowPassBaseGain { get; private set; }

        private const int FixedPointPrecision = 14;

        public DelayState(ref DelayParameter parameter, ulong workBuffer)
        {
            DelayLines = new DelayLine[parameter.ChannelCount];
            LowPassZ = new float[parameter.ChannelCount];

            uint sampleRate = (uint)FixedPointHelper.ToInt(parameter.SampleRate, FixedPointPrecision) / 1000;

            for (int i = 0; i < DelayLines.Length; i++)
            {
                DelayLines[i] = new DelayLine(sampleRate, parameter.DelayTimeMax);
                DelayLines[i].SetDelay(parameter.DelayTime);
            }

            UpdateParameter(ref parameter);
        }

        public void UpdateParameter(ref DelayParameter parameter)
        {
            FeedbackGain = FixedPointHelper.ToFloat(parameter.FeedbackGain, FixedPointPrecision) * 0.98f;

            float channelSpread = FixedPointHelper.ToFloat(parameter.ChannelSpread, FixedPointPrecision);

            DelayFeedbackBaseGain = (1.0f - channelSpread) * FeedbackGain;

            if (parameter.ChannelCount == 4 || parameter.ChannelCount == 6)
            {
                DelayFeedbackCrossGain = channelSpread * 0.5f * FeedbackGain;
            }
            else
            {
                DelayFeedbackCrossGain = channelSpread * FeedbackGain;
            }

            LowPassFeedbackGain = 0.95f * FixedPointHelper.ToFloat(parameter.LowPassAmount, FixedPointPrecision);
            LowPassBaseGain = 1.0f - LowPassFeedbackGain;
        }

        public readonly void UpdateLowPassFilter(ref float tempRawRef, uint channelCount)
        {
            for (int i = 0; i < channelCount; i++)
            {
                float lowPassResult = LowPassFeedbackGain * LowPassZ[i] + Unsafe.Add(ref tempRawRef, i) * LowPassBaseGain;

                LowPassZ[i] = lowPassResult;
                DelayLines[i].Update(lowPassResult);
            }
        }
    }
}
