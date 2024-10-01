using Ryujinx.Audio.Renderer.Common;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Performance
{
    /// <summary>
    /// Implementation of <see cref="IPerformanceDetailEntry"/> for performance metrics version 1.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    public struct PerformanceDetailVersion1 : IPerformanceDetailEntry
    {
        /// <summary>
        /// The node id associated to this detailed entry.
        /// </summary>
        public int NodeId;

        /// <summary>
        /// The start time (in microseconds) associated to this detailed entry.
        /// </summary>
        public int StartTime;

        /// <summary>
        /// The processing time (in microseconds) associated to this detailed entry.
        /// </summary>
        public int ProcessingTime;

        /// <summary>
        /// The detailed entry type associated to this detailed entry.
        /// </summary>
        public PerformanceDetailType DetailType;

        /// <summary>
        /// The entry type associated to this detailed entry.
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

        public void SetDetailType(PerformanceDetailType detailType)
        {
            DetailType = detailType;
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
