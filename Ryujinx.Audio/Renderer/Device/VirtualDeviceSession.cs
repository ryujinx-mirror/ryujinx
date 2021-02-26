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

namespace Ryujinx.Audio.Renderer.Device
{
    /// <summary>
    /// Represents a virtual device session used by IAudioDevice.
    /// </summary>
    public class VirtualDeviceSession
    {
        /// <summary>
        /// The <see cref="VirtualDevice"/> associated to this session.
        /// </summary>
        public VirtualDevice Device { get; }

        /// <summary>
        /// The user volume of this session.
        /// </summary>
        public float Volume { get; set; }

        /// <summary>
        /// Create a new <see cref="VirtualDeviceSession"/> instance.
        /// </summary>
        /// <param name="virtualDevice">The <see cref="VirtualDevice"/> associated to this session.</param>
        public VirtualDeviceSession(VirtualDevice virtualDevice)
        {
            Device = virtualDevice;
        }
    }
}
