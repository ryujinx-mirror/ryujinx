//
// Copyright (c) 2019-2021 Ryujinx
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Performance
{
    /// <summary>
    /// Implementation of <see cref="IPerformanceHeader"/> for performance metrics version 2.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x30)]
    public struct PerformanceFrameHeaderVersion2 : IPerformanceHeader
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

        /// <summary>
        /// The start ticks of the <see cref="Dsp.AudioProcessor"/>. (before sending commands)
        /// </summary>
        public ulong StartRenderingTicks;

        /// <summary>
        /// The index of this performance frame.
        /// </summary>
        public uint Index;

        /// <summary>
        /// If set to true, the DSP is running behind.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool IsDspRunningBehind;

        public int GetEntryCount()
        {
            return EntryCount;
        }

        public int GetEntryCountOffset()
        {
            return 4;
        }

        public int GetEntryDetailCount()
        {
            return EntryDetailCount;
        }

        public void SetDspRunningBehind(bool isRunningBehind)
        {
            IsDspRunningBehind = isRunningBehind;
        }

        public void SetEntryCount(int entryCount)
        {
            EntryCount = entryCount;
        }

        public void SetEntryDetailCount(int entryDetailCount)
        {
            EntryDetailCount = entryDetailCount;
        }

        public void SetIndex(uint index)
        {
            Index = index;
        }

        public void SetMagic(uint magic)
        {
            Magic = magic;
        }

        public void SetNextOffset(int nextOffset)
        {
            NextOffset = nextOffset;
        }

        public void SetStartRenderingTicks(ulong startTicks)
        {
            StartRenderingTicks = startTicks;
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
