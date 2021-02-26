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

using System;
using System.Diagnostics;

namespace Ryujinx.Audio.Integration
{
    /// <summary>
    /// Represent an hardware device used in <see cref="Renderer.Dsp.Command.DeviceSinkCommand"/>
    /// </summary>
    public interface IHardwareDevice : IDisposable
    {
        /// <summary>
        /// Get the supported sample rate of this device.
        /// </summary>
        /// <returns>The supported sample rate of this device.</returns>
        uint GetSampleRate();

        /// <summary>
        /// Get the channel count supported by this device.
        /// </summary>
        /// <returns>The channel count supported by this device.</returns>
        uint GetChannelCount();

        /// <summary>
        /// Appends new PCM16 samples to the device.
        /// </summary>
        /// <param name="data">The new PCM16 samples.</param>
        /// <param name="channelCount">The number of channels.</param>
        void AppendBuffer(ReadOnlySpan<short> data, uint channelCount);

        /// <summary>
        /// Check if the audio renderer needs to perform downmixing.
        /// </summary>
        /// <returns>True if downmixing is needed.</returns>
        public bool NeedDownmixing()
        {
            uint channelCount = GetChannelCount();

            Debug.Assert(channelCount > 0 && channelCount <= Constants.ChannelCountMax);

            return channelCount != Constants.ChannelCountMax;
        }
    }
}
