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

namespace Ryujinx.Audio.Renderer.Server.Upsampler
{
    /// <summary>
    /// Server state for a upsampling.
    /// </summary>
    public class UpsamplerState
    {
        /// <summary>
        /// The output buffer containing the target samples.
        /// </summary>
        public Memory<float> OutputBuffer { get; }

        /// <summary>
        /// The target sample count.
        /// </summary>
        public uint SampleCount { get; }

        /// <summary>
        /// The index of the <see cref="UpsamplerState"/>. (used to free it)
        /// </summary>
        private int _index;

        /// <summary>
        /// The <see cref="UpsamplerManager"/>.
        /// </summary>
        private UpsamplerManager _manager;

        /// <summary>
        /// The source sample count.
        /// </summary>
        public uint SourceSampleCount;

        /// <summary>
        /// The input buffer indices of the buffers holding the samples that need upsampling.
        /// </summary>
        public ushort[] InputBufferIndices;

        /// <summary>
        /// Create a new <see cref="UpsamplerState"/>.
        /// </summary>
        /// <param name="manager">The upsampler manager.</param>
        /// <param name="index">The index of the <see cref="UpsamplerState"/>. (used to free it)</param>
        /// <param name="outputBuffer">The output buffer used to contain the target samples.</param>
        /// <param name="sampleCount">The target sample count.</param>
        public UpsamplerState(UpsamplerManager manager, int index, Memory<float> outputBuffer, uint sampleCount)
        {
            _manager = manager;
            _index = index;
            OutputBuffer = outputBuffer;
            SampleCount = sampleCount;
        }

        /// <summary>
        /// Release the <see cref="UpsamplerState"/>.
        /// </summary>
        public void Release()
        {
            _manager.Free(_index);
        }
    }
}
