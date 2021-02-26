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

using Ryujinx.Audio.Renderer.Dsp.Effect;
using Ryujinx.Audio.Renderer.Parameter.Effect;

namespace Ryujinx.Audio.Renderer.Dsp.State
{
    public class DelayState
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
                LowPassZ[0] = 0;
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
    }
}
