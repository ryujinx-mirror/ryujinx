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
    /// Input header for a splitter state update.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SplitterInParameter
    {
        /// <summary>
        /// Magic of the input header.
        /// </summary>
        public uint Magic;

        /// <summary>
        /// Target splitter id.
        /// </summary>
        public int Id;

        /// <summary>
        /// Target sample rate to use on the splitter.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// Count of splitter destinations.
        /// </summary>
        /// <remarks>Splitter destination ids are defined right after this header.</remarks>
        public int DestinationCount;

        /// <summary>
        /// The expected constant of any input header.
        /// </summary>
        private const uint ValidMagic = 0x49444E53;

        /// <summary>
        /// Check if the magic is valid.
        /// </summary>
        /// <returns>Returns true if the magic is valid.</returns>
        public bool IsMagicValid()
        {
            return Magic == ValidMagic;
        }
    }
}
