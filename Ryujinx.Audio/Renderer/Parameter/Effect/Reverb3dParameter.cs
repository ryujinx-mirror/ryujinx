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

using Ryujinx.Audio.Renderer.Server.Effect;
using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter.Effect
{
    /// <summary>
    /// <see cref="EffectInParameter.SpecificData"/> for <see cref="Common.EffectType.Reverb3d"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Reverb3dParameter
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
        /// Reserved/unused.
        /// </summary>
        private uint _reserved;

        /// <summary>
        /// The target sample rate.
        /// </summary>
        /// <remarks>This is in kHz.</remarks>
        public uint SampleRate;

        /// <summary>
        /// Gain of the room high-frequency effect.
        /// </summary>
        public float RoomHf;

        /// <summary>
        /// Reference high frequency.
        /// </summary>
        public float HfReference;

        /// <summary>
        /// Reverberation decay time at low frequencies.
        /// </summary>
        public float DecayTime;

        /// <summary>
        /// Ratio of the decay time at high frequencies to the decay time at low frequencies.
        /// </summary>
        public float HfDecayRatio;

        /// <summary>
        /// Gain of the room effect.
        /// </summary>
        public float RoomGain;

        /// <summary>
        /// Gain of the early reflections relative to <see cref="RoomGain"/>.
        /// </summary>
        public float ReflectionsGain;

        /// <summary>
        /// Gain of the late reverberation relative to <see cref="RoomGain"/>.
        /// </summary>
        public float ReverbGain;

        /// <summary>
        /// Echo density in the late reverberation decay.
        /// </summary>
        public float Diffusion;

        /// <summary>
        /// Modal density in the late reverberation decay.
        /// </summary>
        public float ReflectionDelay;

        /// <summary>
        /// Time limit between the early reflections and the late reverberation relative to the time of the first reflection.
        /// </summary>
        public float ReverbDelayTime;

        /// <summary>
        /// Modal density in the late reverberation decay.
        /// </summary>
        public float Density;

        /// <summary>
        /// The dry gain.
        /// </summary>
        public float DryGain;

        /// <summary>
        /// The current usage status of the effect on the client side.
        /// </summary>
        public UsageState ParameterStatus;

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
