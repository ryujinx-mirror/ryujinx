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

namespace Ryujinx.Audio.Renderer.Server.Upsampler
{
    /// <summary>
    /// Upsampler manager.
    /// </summary>
    public class UpsamplerManager
    {
        /// <summary>
        /// Work buffer for upsampler.
        /// </summary>
        private Memory<float> _upSamplerWorkBuffer;

        /// <summary>
        /// Global lock of the object.
        /// </summary>
        private object Lock = new object();

        /// <summary>
        /// The upsamplers instances.
        /// </summary>
        private UpsamplerState[] _upsamplers;

        /// <summary>
        /// The count of upsamplers.
        /// </summary>
        private uint _count;

        /// <summary>
        /// Create a new <see cref="UpsamplerManager"/>.
        /// </summary>
        /// <param name="upSamplerWorkBuffer">Work buffer for upsampler.</param>
        /// <param name="count">The count of upsamplers.</param>
        public UpsamplerManager(Memory<float> upSamplerWorkBuffer, uint count)
        {
            _upSamplerWorkBuffer = upSamplerWorkBuffer;
            _count = count;

            _upsamplers = new UpsamplerState[_count];
        }

        /// <summary>
        /// Allocate a new <see cref="UpsamplerState"/>.
        /// </summary>
        /// <returns>A new <see cref="UpsamplerState"/> or null if out of memory.</returns>
        public UpsamplerState Allocate()
        {
            int workBufferOffset = 0;

            lock (Lock)
            {
                for (int i = 0; i < _count; i++)
                {
                    if (_upsamplers[i] == null)
                    {
                        _upsamplers[i] = new UpsamplerState(this, i, _upSamplerWorkBuffer.Slice(workBufferOffset, Constants.UpSampleEntrySize), Constants.TargetSampleCount);

                        return _upsamplers[i];
                    }

                    workBufferOffset += Constants.UpSampleEntrySize;
                }
            }

            return null;
        }

        /// <summary>
        /// Free a <see cref="UpsamplerState"/> at the given index.
        /// </summary>
        /// <param name="index">The index of the <see cref="UpsamplerState"/> to free.</param>
        public void Free(int index)
        {
            lock (Lock)
            {
                Debug.Assert(_upsamplers[index] != null);

                _upsamplers[index] = null;
            }
        }
    }
}
