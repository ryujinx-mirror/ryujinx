using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Output information for a sink.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SinkOutStatus
    {
        /// <summary>
        /// Last written offset if the sink type is <see cref="Common.SinkType.CircularBuffer"/>.
        /// </summary>
        public uint LastWrittenOffset;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private readonly uint _padding;

        /// <summary>
        /// Reserved/padding.
        /// </summary>
        private unsafe fixed ulong _reserved[3];
    }
}
