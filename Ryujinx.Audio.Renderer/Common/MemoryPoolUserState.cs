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
    /// Represents the state of a memory pool.
    /// </summary>
    public enum MemoryPoolUserState : uint
    {
        /// <summary>
        /// Invalid state.
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// The memory pool is new. (client side only)
        /// </summary>
        New = 1,

        /// <summary>
        /// The user asked to detach the memory pool from the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        RequestDetach = 2,

        /// <summary>
        /// The memory pool is detached from the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        Detached = 3,

        /// <summary>
        /// The user asked to attach the memory pool to the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        RequestAttach = 4,

        /// <summary>
        /// The memory pool is attached to the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        Attached = 5,

        /// <summary>
        /// The memory pool is released. (client side only)
        /// </summary>
        Released = 6
    }
}
