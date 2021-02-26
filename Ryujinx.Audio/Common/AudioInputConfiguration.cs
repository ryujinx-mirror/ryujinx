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
    /// Audio user input configuration.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AudioInputConfiguration
    {
        /// <summary>
        /// The target sample rate of the user.
        /// </summary>
        /// <remarks>Only 48000Hz is considered valid, other sample rates will be refused.</remarks>
        public uint SampleRate;

        /// <summary>
        /// The target channel count of the user.
        /// </summary>
        /// <remarks>Only Stereo and Surround are considered valid, other configurations will be refused.</remarks>
        /// <remarks>Not used in audin.</remarks>
        public ushort ChannelCount;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private ushort _reserved;
    }
}
