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

using Ryujinx.Common;
using System;

namespace Ryujinx.Audio.Backends.Common
{
    /// <summary>
    /// A ring buffer that grow if data written to it is too big to fit.
    /// </summary>
    public class DynamicRingBuffer
    {
        private const int RingBufferAlignment = 2048;

        private object _lock = new object();

        private byte[] _buffer;
        private int _size;
        private int _headOffset;
        private int _tailOffset;

        public int Length => _size;

        public DynamicRingBuffer(int initialCapacity = RingBufferAlignment)
        {
            _buffer = new byte[initialCapacity];
        }

        public void Clear()
        {
            _size = 0;
            _headOffset = 0;
            _tailOffset = 0;
        }

        public void Clear(int size)
        {
            lock (_lock)
            {
                if (size > _size)
                {
                    size = _size;
                }

                if (size == 0)
                {
                    return;
                }

                _headOffset = (_headOffset + size) % _buffer.Length;
                _size -= size;

                if (_size == 0)
                {
                    _headOffset = 0;
                    _tailOffset = 0;
                }
            }
        }

        private void SetCapacityLocked(int capacity)
        {
            byte[] buffer = new byte[capacity];

            if (_size > 0)
            {
                if (_headOffset < _tailOffset)
                {
                    Buffer.BlockCopy(_buffer, _headOffset, buffer, 0, _size);
                }
                else
                {
                    Buffer.BlockCopy(_buffer, _headOffset, buffer, 0, _buffer.Length - _headOffset);
                    Buffer.BlockCopy(_buffer, 0, buffer, _buffer.Length - _headOffset, _tailOffset);
                }
            }

            _buffer = buffer;
            _headOffset = 0;
            _tailOffset = _size;
        }


        public void Write<T>(T[] buffer, int index, int count)
        {
            if (count == 0)
            {
                return;
            }

            lock (_lock)
            {
                if ((_size + count) > _buffer.Length)
                {
                    SetCapacityLocked(BitUtils.AlignUp(_size + count, RingBufferAlignment));
                }

                if (_headOffset < _tailOffset)
                {
                    int tailLength = _buffer.Length - _tailOffset;

                    if (tailLength >= count)
                    {
                        Buffer.BlockCopy(buffer, index, _buffer, _tailOffset, count);
                    }
                    else
                    {
                        Buffer.BlockCopy(buffer, index, _buffer, _tailOffset, tailLength);
                        Buffer.BlockCopy(buffer, index + tailLength, _buffer, 0, count - tailLength);
                    }
                }
                else
                {
                    Buffer.BlockCopy(buffer, index, _buffer, _tailOffset, count);
                }

                _size += count;
                _tailOffset = (_tailOffset + count) % _buffer.Length;
            }
        }

        public int Read<T>(T[] buffer, int index, int count)
        {
            lock (_lock)
            {
                if (count > _size)
                {
                    count = _size;
                }

                if (count == 0)
                {
                    return 0;
                }

                if (_headOffset < _tailOffset)
                {
                    Buffer.BlockCopy(_buffer, _headOffset, buffer, index, count);
                }
                else
                {
                    int tailLength = _buffer.Length - _headOffset;

                    if (tailLength >= count)
                    {
                        Buffer.BlockCopy(_buffer, _headOffset, buffer, index, count);
                    }
                    else
                    {
                        Buffer.BlockCopy(_buffer, _headOffset, buffer, index, tailLength);
                        Buffer.BlockCopy(_buffer, 0, buffer, index + tailLength, count - tailLength);
                    }
                }

                _size -= count;
                _headOffset = (_headOffset + count) % _buffer.Length;

                if (_size == 0)
                {
                    _headOffset = 0;
                    _tailOffset = 0;
                }

                return count;
            }
        }
    }
}
