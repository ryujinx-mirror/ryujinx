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

using Ryujinx.Audio.Integration;

namespace Ryujinx.Audio.Common
{
    /// <summary>
    /// Represent an audio buffer that will be used by an <see cref="IHardwareDeviceSession"/>.
    /// </summary>
    public class AudioBuffer
    {
        /// <summary>
        /// Unique tag of this buffer.
        /// </summary>
        /// <remarks>Unique per session</remarks>
        public ulong BufferTag;

        /// <summary>
        /// Pointer to the user samples.
        /// </summary>
        public ulong DataPointer;

        /// <summary>
        /// Size of the user samples region.
        /// </summary>
        public ulong DataSize;

        /// <summary>
        ///  The timestamp at which the buffer was played.
        /// </summary>
        /// <remarks>Not used but useful for debugging</remarks>
        public ulong PlayedTimestamp;

        /// <summary>
        /// The user samples.
        /// </summary>
        public byte[] Data;
    }
}
