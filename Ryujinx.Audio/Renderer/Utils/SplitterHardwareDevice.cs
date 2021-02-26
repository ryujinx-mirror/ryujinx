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

using Ryujinx.Audio.Integration;
using System;

namespace Ryujinx.Audio.Renderer.Utils
{
    public class SplitterHardwareDevice : IHardwareDevice
    {
        private IHardwareDevice _baseDevice;
        private IHardwareDevice _secondaryDevice;

        public SplitterHardwareDevice(IHardwareDevice baseDevice, IHardwareDevice secondaryDevice)
        {
            _baseDevice = baseDevice;
            _secondaryDevice = secondaryDevice;
        }

        public void AppendBuffer(ReadOnlySpan<short> data, uint channelCount)
        {
            _baseDevice.AppendBuffer(data, channelCount);
            _secondaryDevice?.AppendBuffer(data, channelCount);
        }

        public uint GetChannelCount()
        {
            return _baseDevice.GetChannelCount();
        }

        public uint GetSampleRate()
        {
            return _baseDevice.GetSampleRate();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _baseDevice.Dispose();
                _secondaryDevice?.Dispose();
            }
        }
    }
}
