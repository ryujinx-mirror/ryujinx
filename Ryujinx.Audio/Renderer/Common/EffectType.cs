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

namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// The type of an effect.
    /// </summary>
    public enum EffectType : byte
    {
        /// <summary>
        /// Invalid effect.
        /// </summary>
        Invalid,

        /// <summary>
        /// Effect applying additional mixing capability.
        /// </summary>
        BufferMix,

        /// <summary>
        /// Effect applying custom user effect (via auxiliary buffers).
        /// </summary>
        AuxiliaryBuffer,

        /// <summary>
        /// Effect applying a delay.
        /// </summary>
        Delay,

        /// <summary>
        /// Effect applying a reverberation effect via a given preset.
        /// </summary>
        Reverb,

        /// <summary>
        /// Effect applying a 3D reverberation effect via a given preset.
        /// </summary>
        Reverb3d,

        /// <summary>
        /// Effect applying a biquad filter.
        /// </summary>
        BiquadFilter
    }
}
