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

using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Audio.Renderer.Server.Effect;
using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter.Effect
{
    /// <summary>
    /// <see cref="EffectInParameter.SpecificData"/> for <see cref="Common.EffectType.Reverb"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ReverbParameter
    {
        /// <summary>
        /// The input channel indices that will be used by the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        public Array6<byte> Input;

        /// <summary>
        /// The output channel indices that will be used by the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        public Array6<byte> Output;

        /// <summary>
        /// The maximum number of channels supported.
        /// </summary>
        public ushort ChannelCountMax;

        /// <summary>
        /// The total channel count used.
        /// </summary>
        public ushort ChannelCount;

        /// <summary>
        /// The target sample rate. (Q15)
        /// </summary>
        /// <remarks>This is in kHz.</remarks>
        public int SampleRate;

        /// <summary>
        /// The early mode to use.
        /// </summary>
        public ReverbEarlyMode EarlyMode;

        /// <summary>
        /// The gain to apply to the result of the early reflection. (Q15)
        /// </summary>
        public int EarlyGain;

        /// <summary>
        /// The pre-delay time in milliseconds. (Q15)
        /// </summary>
        public int PreDelayTime;

        /// <summary>
        /// The late mode to use.
        /// </summary>
        public ReverbLateMode LateMode;

        /// <summary>
        /// The gain to apply to the result of the late reflection. (Q15)
        /// </summary>
        public int LateGain;

        /// <summary>
        /// The decay time. (Q15)
        /// </summary>
        public int DecayTime;

        /// <summary>
        /// The high frequency decay ratio. (Q15)
        /// </summary>
        /// <remarks>If <see cref="HighFrequencyDecayRatio"/> >= 0.995f, it is considered disabled.</remarks>
        public int HighFrequencyDecayRatio;

        /// <summary>
        /// The coloration of the decay. (Q15)
        /// </summary>
        public int Coloration;

        /// <summary>
        /// The reverb gain. (Q15)
        /// </summary>
        public int ReverbGain;

        /// <summary>
        /// The output gain. (Q15)
        /// </summary>
        public int OutGain;

        /// <summary>
        /// The dry gain. (Q15)
        /// </summary>
        public int DryGain;

        /// <summary>
        /// The current usage status of the effect on the client side.
        /// </summary>
        public UsageState Status;

        /// <summary>
        /// Check if the <see cref="ChannelCount"/> is valid.
        /// </summary>
        /// <returns>Returns true if the <see cref="ChannelCount"/> is valid.</returns>
        public bool IsChannelCountValid()
        {
            return EffectInParameter.IsChannelCountValid(ChannelCount);
        }

        /// <summary>
        /// Check if the <see cref="ChannelCountMax"/> is valid.
        /// </summary>
        /// <returns>Returns true if the <see cref="ChannelCountMax"/> is valid.</returns>
        public bool IsChannelCountMaxValid()
        {
            return EffectInParameter.IsChannelCountValid(ChannelCountMax);
        }
    }
}
