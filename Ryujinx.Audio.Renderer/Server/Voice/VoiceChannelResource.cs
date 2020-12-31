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

namespace Ryujinx.Audio.Renderer.Server.Voice
{
    /// <summary>
    /// Server state for a voice channel resource.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0xD0, Pack = Alignment)]
    public struct VoiceChannelResource
    {
        public const int Alignment = 0x10;

        /// <summary>
        /// Mix volumes for the resource.
        /// </summary>
        public Array24<float> Mix;

        /// <summary>
        /// Previous mix volumes for resource.
        /// </summary>
        public Array24<float> PreviousMix;

        /// <summary>
        /// The id of the resource.
        /// </summary>
        public uint Id;

        /// <summary>
        /// Indicate if the resource is used.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsUsed;

        public void UpdateState()
        {
            Mix.ToSpan().CopyTo(PreviousMix.ToSpan());
        }
    }
}
