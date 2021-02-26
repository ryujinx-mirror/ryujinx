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

using Ryujinx.Audio.Common;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Integration
{
    public class HardwareDeviceImpl : IHardwareDevice
    {
        private IHardwareDeviceSession _session;
        private uint _channelCount;
        private uint _sampleRate;
        private uint _currentBufferTag;

        private byte[] _buffer;

        public HardwareDeviceImpl(IHardwareDeviceDriver deviceDriver, uint channelCount, uint sampleRate)
        {
            _session = deviceDriver.OpenDeviceSession(IHardwareDeviceDriver.Direction.Output, null, SampleFormat.PcmInt16, sampleRate, channelCount);
            _channelCount = channelCount;
            _sampleRate = sampleRate;
            _currentBufferTag = 0;

            _buffer = new byte[Constants.TargetSampleCount * channelCount * sizeof(ushort)];

            _session.Start();
        }

        public void AppendBuffer(ReadOnlySpan<short> data, uint channelCount)
        {
            data.CopyTo(MemoryMarshal.Cast<byte, short>(_buffer));

            _session.QueueBuffer(new AudioBuffer
            {
                DataPointer = _currentBufferTag++,
                Data        = _buffer,
                DataSize    = (ulong)_buffer.Length,
            });

            _currentBufferTag = _currentBufferTag % 4;
        }

        public uint GetChannelCount()
        {
            return _channelCount;
        }

        public uint GetSampleRate()
        {
            return _sampleRate;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _session.Dispose();
            }
        }
    }
}
