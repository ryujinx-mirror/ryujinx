using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Performance
{
    /// <summary>
    /// Implementation of <see cref="IPerformanceHeader"/> for performance metrics version 1.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x18)]
    public struct PerformanceFrameHeaderVersion1 : IPerformanceHeader
    {
        /// <summary>
        /// The magic of the performance header.
        /// </summary>
        public uint Magic;

        /// <summary>
        /// The total count of entries in this frame.
        /// </summary>
        public int EntryCount;

        /// <summary>
        /// The total count of detailed entries in this frame.
        /// </summary>
        public int EntryDetailCount;

        /// <summary>
        /// The offset of the next performance header.
        /// </summary>
        public int NextOffset;

        /// <summary>
        /// The total time taken by all the commands profiled.
        /// </summary>
        public int TotalProcessingTime;

        /// <summary>
        /// The count of voices that were dropped.
        /// </summary>
        public uint VoiceDropCount;

        public readonly int GetEntryCount()
        {
            return EntryCount;
        }

        public readonly int GetEntryCountOffset()
        {
            return 4;
        }

        public readonly int GetEntryDetailCount()
        {
            return EntryDetailCount;
        }

        public readonly void SetDspRunningBehind(bool isRunningBehind)
        {
            // NOTE: Not present in version 1
        }

        public void SetEntryCount(int entryCount)
        {
            EntryCount = entryCount;
        }

        public void SetEntryDetailCount(int entryDetailCount)
        {
            EntryDetailCount = entryDetailCount;
        }

        public readonly void SetIndex(uint index)
        {
            // NOTE: Not present in version 1
        }

        public void SetMagic(uint magic)
        {
            Magic = magic;
        }

        public void SetNextOffset(int nextOffset)
        {
            NextOffset = nextOffset;
        }

        public readonly void SetStartRenderingTicks(ulong startTicks)
        {
            // NOTE: not present in version 1
        }

        public void SetTotalProcessingTime(int totalProcessingTime)
        {
            TotalProcessingTime = totalProcessingTime;
        }

        public void SetVoiceDropCount(uint voiceCount)
        {
            VoiceDropCount = voiceCount;
        }
    }
}
