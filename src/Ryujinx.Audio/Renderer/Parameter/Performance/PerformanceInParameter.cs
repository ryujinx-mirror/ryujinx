using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Parameter.Performance
{
    /// <summary>
    /// Input information for performance monitoring.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PerformanceInParameter
    {
        /// <summary>
        /// The target node id to monitor performance on.
        /// </summary>
        public int TargetNodeId;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private unsafe fixed uint _reserved[3];
    }
}
