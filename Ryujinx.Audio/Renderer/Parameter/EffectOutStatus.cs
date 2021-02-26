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

using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Output information for an effect.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EffectOutStatus
    {
        /// <summary>
        /// The state of an effect.
        /// </summary>
        public enum EffectState : byte
        {
            /// <summary>
            /// The effect is enabled.
            /// </summary>
            Enabled = 3,

            /// <summary>
            /// The effect is disabled.
            /// </summary>
            Disabled = 4
        }

        /// <summary>
        /// Current effect state.
        /// </summary>
        public EffectState State;

        /// <summary>
        /// Unused/Reserved.
        /// </summary>
        private unsafe fixed byte _reserved[15];
    }
}
