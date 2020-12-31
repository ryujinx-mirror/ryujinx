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

using System.Diagnostics;

namespace Ryujinx.Audio.Renderer.Device
{
    /// <summary>
    /// Represents a virtual device used by IAudioDevice.
    /// </summary>
    public class VirtualDevice
    {
        /// <summary>
        /// All the defined virtual devices.
        /// </summary>
        public static readonly VirtualDevice[] Devices = new VirtualDevice[4]
        {
            new VirtualDevice("AudioStereoJackOutput", 2),
            new VirtualDevice("AudioBuiltInSpeakerOutput", 2),
            new VirtualDevice("AudioTvOutput", 6),
            new VirtualDevice("AudioUsbDeviceOutput", 2),
        };

        /// <summary>
        /// The name of the <see cref="VirtualDevice"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The count of channels supported by the <see cref="VirtualDevice"/>.
        /// </summary>
        public uint ChannelCount { get; }

        /// <summary>
        /// The system master volume of the <see cref="VirtualDevice"/>.
        /// </summary>
        public float MasterVolume { get; private set; }

        /// <summary>
        /// Create a new <see cref="VirtualDevice"/> instance.
        /// </summary>
        /// <param name="name">The name of the <see cref="VirtualDevice"/>.</param>
        /// <param name="channelCount">The count of channels supported by the <see cref="VirtualDevice"/>.</param>
        private VirtualDevice(string name, uint channelCount)
        {
            Name = name;
            ChannelCount = channelCount;
        }

        /// <summary>
        /// Update the master volume of the <see cref="VirtualDevice"/>.
        /// </summary>
        /// <param name="volume">The new master volume.</param>
        public void UpdateMasterVolume(float volume)
        {
            Debug.Assert(volume >= 0.0f && volume <= 1.0f);

            MasterVolume = volume;
        }

        /// <summary>
        /// Check if the <see cref="VirtualDevice"/> is a usb device.
        /// </summary>
        /// <returns>Returns true if the <see cref="VirtualDevice"/> is a usb device.</returns>
        public bool IsUsbDevice()
        {
            return Name.Equals("AudioUsbDeviceOutput");
        }
    }
}
