using Ryujinx.Audio.Common;
using Ryujinx.Audio.Renderer.Common;
using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;
using CpuAddress = System.UInt64;

namespace Ryujinx.Audio.Renderer.Parameter.Sink
{
    /// <summary>
    /// <see cref="SinkInParameter.SpecificData"/> for <see cref="SinkType.CircularBuffer"/>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CircularBufferParameter
    {
        /// <summary>
        /// The CPU address of the user circular buffer.
        /// </summary>
        public CpuAddress BufferAddress;

        /// <summary>
        /// The size of the user circular buffer.
        /// </summary>
        public uint BufferSize;

        /// <summary>
        /// The total count of channels to output to the circular buffer.
        /// </summary>
        public uint InputCount;

        /// <summary>
        /// The target sample count to output per update in the circular buffer.
        /// </summary>
        public uint SampleCount;

        /// <summary>
        /// Last read offset on the CPU side.
        /// </summary>
        public uint LastReadOffset;

        /// <summary>
        /// The target <see cref="SampleFormat"/>.
        /// </summary>
        /// <remarks>Only <see cref="Audio.Common.SampleFormat.PcmInt16"/> is supported.</remarks>
        public SampleFormat SampleFormat;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private unsafe fixed byte _reserved1[3];

        /// <summary>
        /// The input channels index that will be used.
        /// </summary>
        public Array6<byte> Input;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private readonly ushort _reserved2;
    }
}
