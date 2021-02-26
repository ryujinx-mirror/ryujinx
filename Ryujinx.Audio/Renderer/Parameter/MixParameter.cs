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

using Ryujinx.Common.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Input information for a mix.
    /// </summary>
    /// <remarks>Also used on the client side for mix tracking.</remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MixParameter
    {
        /// <summary>
        /// Base volume of the mix.
        /// </summary>
        public float Volume;

        /// <summary>
        /// Target sample rate of the mix.
        /// </summary>
        public uint SampleRate;

        /// <summary>
        /// Target buffer count.
        /// </summary>
        public uint BufferCount;

        /// <summary>
        /// Set to true if in use.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsUsed;

        /// <summary>
        /// Set to true if it was changed.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsDirty;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private ushort _reserved1;

        /// <summary>
        /// The id of the mix.
        /// </summary>
        public int MixId;

        /// <summary>
        /// The effect count. (client side)
        /// </summary>
        public uint EffectCount;

        /// <summary>
        /// The mix node id.
        /// </summary>
        public int NodeId;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private ulong _reserved2;

        /// <summary>
        /// Mix buffer volumes storage.
        /// </summary>
        private MixVolumeArray _mixBufferVolumeArray;

        /// <summary>
        /// The mix to output the result of this mix.
        /// </summary>
        public int DestinationMixId;

        /// <summary>
        /// The splitter to output the result of this mix.
        /// </summary>
        public uint DestinationSplitterId;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private uint _reserved3;

        [StructLayout(LayoutKind.Sequential, Size = 4 * Constants.MixBufferCountMax * Constants.MixBufferCountMax, Pack = 1)]
        private struct MixVolumeArray { }

        /// <summary>
        /// Mix buffer volumes.
        /// </summary>
        /// <remarks>Used when no splitter id is specified.</remarks>
        public Span<float> MixBufferVolume => SpanHelpers.AsSpan<MixVolumeArray, float>(ref _mixBufferVolumeArray);
    }
}
