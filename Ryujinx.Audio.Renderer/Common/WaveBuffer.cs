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

using DspAddr = System.UInt64;

namespace Ryujinx.Audio.Renderer.Common
{
    /// <summary>
    /// A wavebuffer used for data source commands.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WaveBuffer
    {
        /// <summary>
        /// The DSP address of the sample data of the wavebuffer.
        /// </summary>
        public DspAddr Buffer;

        /// <summary>
        /// The DSP address of the context of the wavebuffer.
        /// </summary>
        /// <remarks>Only used by <see cref="SampleFormat.Adpcm"/>.</remarks>
        public DspAddr Context;

        /// <summary>
        /// The size of the sample buffer data.
        /// </summary>
        public uint BufferSize;

        /// <summary>
        /// The size of the context buffer.
        /// </summary>
        public uint ContextSize;

        /// <summary>
        /// First sample to play on the wavebuffer.
        /// </summary>
        public uint StartSampleOffset;

        /// <summary>
        /// Last sample to play on the wavebuffer.
        /// </summary>
        public uint EndSampleOffset;

        /// <summary>
        /// First sample to play when looping the wavebuffer.
        /// </summary>
        /// <remarks>
        /// If <see cref="LoopStartSampleOffset"/> or <see cref="LoopEndSampleOffset"/> is equal to zero,, it will default to <see cref="StartSampleOffset"/> and <see cref="EndSampleOffset"/>.
        /// </remarks>
        public uint LoopStartSampleOffset;

        /// <summary>
        /// Last sample to play when looping the wavebuffer.
        /// </summary>
        /// <remarks>
        /// If <see cref="LoopStartSampleOffset"/> or <see cref="LoopEndSampleOffset"/> is equal to zero, it will default to <see cref="StartSampleOffset"/> and <see cref="EndSampleOffset"/>.
        /// </remarks>
        public uint LoopEndSampleOffset;

        /// <summary>
        /// The max loop count.
        /// </summary>
        public int LoopCount;

        /// <summary>
        /// Set to true if the wavebuffer is looping.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool Looping;

        /// <summary>
        /// Set to true if the wavebuffer is the end of stream.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsEndOfStream;

        /// <summary>
        /// Padding/Reserved.
        /// </summary>
        private ushort _padding;
    }
}
