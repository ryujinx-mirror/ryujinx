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

using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Common.Utilities;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Input information for a sink.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SinkInParameter
    {
        /// <summary>
        /// Type of the sink.
        /// </summary>
        public SinkType Type;

        /// <summary>
        /// Set to true if the sink is used.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsUsed;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private ushort _reserved1;

        /// <summary>
        /// The node id of the sink.
        /// </summary>
        public int NodeId;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private unsafe fixed ulong _reserved2[3];

        /// <summary>
        /// Specific data storage.
        /// </summary>
        private SpecificDataStruct _specificDataStart;

        [StructLayout(LayoutKind.Sequential, Size = 0x120, Pack = 1)]
        private struct SpecificDataStruct { }

        /// <summary>
        /// Specific data changing depending of the <see cref="Type"/>. See also the <see cref="Sink"/> namespace.
        /// </summary>
        public Span<byte> SpecificData => SpanHelpers.AsSpan<SpecificDataStruct, byte>(ref _specificDataStart);
    }
}
