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

using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter.Effect
{
    /// <summary>
    /// <see cref="EffectInParameter.SpecificData"/> for <see cref="Common.EffectType.AuxiliaryBuffer"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AuxiliaryBufferParameter
    {
        /// <summary>
        /// The input channel indices that will be used by the <see cref="Dsp.AudioProcessor"/> to write data to <see cref="SendBufferInfoAddress"/>.
        /// </summary>
        public Array24<byte> Input;

        /// <summary>
        /// The output channel indices that will be used by the <see cref="Dsp.AudioProcessor"/> to read data from <see cref="ReturnBufferInfoAddress"/>.
        /// </summary>
        public Array24<byte> Output;

        /// <summary>
        /// The total channel count used.
        /// </summary>
        public uint ChannelCount;

        /// <summary>
        /// The target sample rate.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// The buffer storage total size.
        /// </summary>
        public uint BufferStorageSize;

        /// <summary>
        /// The maximum number of channels supported.
        /// </summary>
        /// <remarks>This is unused.</remarks>
        public uint ChannelCountMax;

        /// <summary>
        /// The address of the start of the region containing two <see cref="Dsp.State.AuxiliaryBufferHeader"/> followed by the data that will be written by the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        public ulong SendBufferInfoAddress;

        /// <summary>
        /// The address of the start of the region containling data that will be written by the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        /// <remarks>This is unused.</remarks>
        public ulong SendBufferStorageAddress;

        /// <summary>
        /// The address of the start of the region containing two <see cref="Dsp.State.AuxiliaryBufferHeader"/> followed by the data that will be read by the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        public ulong ReturnBufferInfoAddress;

        /// <summary>
        /// The address of the start of the region containling data that will be read by the <see cref="Dsp.AudioProcessor"/>.
        /// </summary>
        /// <remarks>This is unused.</remarks>
        public ulong ReturnBufferStorageAddress;

        /// <summary>
        /// Size of a sample of the mix buffer.
        /// </summary>
        /// <remarks>This is unused.</remarks>
        public uint MixBufferSampleSize;

        /// <summary>
        /// The total count of sample that can be stored.
        /// </summary>
        /// <remarks>This is unused.</remarks>
        public uint TotalSampleCount;

        /// <summary>
        /// The count of sample of the mix buffer.
        /// </summary>
        /// <remarks>This is unused.</remarks>
        public uint MixBufferSampleCount;
    }
}
