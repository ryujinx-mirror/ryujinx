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

namespace Ryujinx.Audio.Common
{
    /// <summary>
    /// Audio user buffer.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AudioUserBuffer
    {
        /// <summary>
        /// Pointer to the next buffer (ignored).
        /// </summary>
        public ulong NextBuffer;

        /// <summary>
        /// Pointer to the user samples.
        /// </summary>
        public ulong Data;

        /// <summary>
        /// Capacity of the buffer (unused).
        /// </summary>
        public ulong Capacity;

        /// <summary>
        /// Size of the user samples region.
        /// </summary>
        public ulong DataSize;

        /// <summary>
        /// Offset in the user samples region (unused).
        /// </summary>
        public ulong DataOffset;
    }
}
