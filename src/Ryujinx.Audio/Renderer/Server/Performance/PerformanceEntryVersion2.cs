using Ryujinx.Audio.Renderer.Common;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Performance
{
    /// <summary>
    /// Implementation of <see cref="IPerformanceEntry"/> for performance metrics version 2.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x18)]
    public struct PerformanceEntryVersion2 : IPerformanceEntry
    {
        /// <summary>
        /// The node id associated to this entry.
        /// </summary>
        public int NodeId;

        /// <summary>
        /// The start time (in microseconds) associated to this entry.
        /// </summary>
        public int StartTime;

        /// <summary>
        /// The processing time (in microseconds) associated to this entry.
        /// </summary>
        public int ProcessingTime;

        /// <summary>
        /// The entry type associated to this entry.
        /// </summary>
        public PerformanceEntryType EntryType;

        public readonly int GetProcessingTime()
        {
            return ProcessingTime;
        }

        public readonly int GetProcessingTimeOffset()
        {
            return 8;
        }

        public readonly int GetStartTime()
        {
            return StartTime;
        }

        public readonly int GetStartTimeOffset()
        {
            return 4;
        }

        public void SetEntryType(PerformanceEntryType type)
        {
            EntryType = type;
        }

        public void SetNodeId(int nodeId)
        {
            NodeId = nodeId;
        }
    }
}
