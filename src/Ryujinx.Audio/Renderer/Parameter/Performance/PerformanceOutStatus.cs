using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter.Performance
{
    /// <summary>
    /// Output information for performance monitoring.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PerformanceOutStatus
    {
        /// <summary>
        /// Indicates the total size output to the performance buffer.
        /// </summary>
        public uint HistorySize;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private unsafe fixed uint _reserved[3];
    }
}
