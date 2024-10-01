using System;

namespace Ryujinx.Audio.Renderer.Server.Performance
{
    /// <summary>
    /// Information used by the performance command to store informations in the performance entry.
    /// </summary>
    public class PerformanceEntryAddresses
    {
        /// <summary>
        /// The memory storing the performance entry.
        /// </summary>
        public Memory<int> BaseMemory;

        /// <summary>
        /// The offset to the start time field.
        /// </summary>
        public uint StartTimeOffset;

        /// <summary>
        /// The offset to the entry count field.
        /// </summary>
        public uint EntryCountOffset;

        /// <summary>
        /// The offset to the processing time field.
        /// </summary>
        public uint ProcessingTimeOffset;

        /// <summary>
        /// Increment the entry count.
        /// </summary>
        public void IncrementEntryCount()
        {
            BaseMemory.Span[(int)EntryCountOffset / 4]++;
        }

        /// <summary>
        /// Set the start time in the entry.
        /// </summary>
        /// <param name="startTimeNano">The start time in nanoseconds.</param>
        public void SetStartTime(ulong startTimeNano)
        {
            BaseMemory.Span[(int)StartTimeOffset / 4] = (int)(startTimeNano / 1000);
        }

        /// <summary>
        /// Set the processing time in the entry.
        /// </summary>
        /// <param name="endTimeNano">The end time in nanoseconds.</param>
        public void SetProcessingTime(ulong endTimeNano)
        {
            BaseMemory.Span[(int)ProcessingTimeOffset / 4] = (int)(endTimeNano / 1000) - BaseMemory.Span[(int)StartTimeOffset / 4];
        }
    }
}
