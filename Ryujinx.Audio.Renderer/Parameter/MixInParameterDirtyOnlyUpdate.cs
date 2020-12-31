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
    /// Input information header for mix updates on REV7 and later
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MixInParameterDirtyOnlyUpdate
    {
        /// <summary>
        /// Magic of the header
        /// </summary>
        /// <remarks>Never checked on hardware.</remarks>
        public uint Magic;

        /// <summary>
        /// The count of <see cref="MixParameter"/> following this header.
        /// </summary>
        public uint MixCount;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private unsafe fixed byte _reserved[24];
    }
}
