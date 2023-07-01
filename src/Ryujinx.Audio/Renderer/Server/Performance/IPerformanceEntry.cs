using Ryujinx.Audio.Renderer.Common;

namespace Ryujinx.Audio.Renderer.Server.Performance
{
    /// <summary>
    /// Represents an entry in a performance frame.
    /// </summary>
    public interface IPerformanceEntry
    {
        /// <summary>
        /// Get the start time of this entry event (in microseconds).
        /// </summary>
        /// <returns>The start time of this entry event (in microseconds).</returns>
        int GetStartTime();

        /// <summary>
        /// Get the start time offset in this structure.
        /// </summary>
        /// <returns>The start time offset in this structure.</returns>
        int GetStartTimeOffset();

        /// <summary>
        /// Get the processing time of this entry event (in microseconds).
        /// </summary>
        /// <returns>The processing time of this entry event (in microseconds).</returns>
        int GetProcessingTime();

        /// <summary>
        /// Get the processing time offset in this structure.
        /// </summary>
        /// <returns>The processing time offset in this structure.</returns>
        int GetProcessingTimeOffset();

        /// <summary>
        /// Set the <paramref name="nodeId"/> of this entry.
        /// </summary>
        /// <param name="nodeId">The node id of this entry.</param>
        void SetNodeId(int nodeId);

        /// <summary>
        /// Set the <see cref="PerformanceEntryType"/> of this entry.
        /// </summary>
        /// <param name="type">The type to use.</param>
        void SetEntryType(PerformanceEntryType type);
    }
}
