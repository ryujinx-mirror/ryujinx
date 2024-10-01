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
        private readonly ushort _reserved1;

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
