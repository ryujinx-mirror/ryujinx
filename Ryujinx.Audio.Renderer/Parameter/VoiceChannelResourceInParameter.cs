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

using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Input information for a voice channel resources.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0x70, Pack = 1)]
    public struct VoiceChannelResourceInParameter
    {
        /// <summary>
        /// The id of the voice channel resource.
        /// </summary>
        public uint Id;

        /// <summary>
        /// Mix volumes for the voice channel resource.
        /// </summary>
        public Array24<float> Mix;

        /// <summary>
        /// Indicate if the voice channel resource is used.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsUsed;
    }
}
