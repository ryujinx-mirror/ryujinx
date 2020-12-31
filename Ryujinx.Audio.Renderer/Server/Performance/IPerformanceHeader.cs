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

namespace Ryujinx.Audio.Renderer.Server.Performance
{
    /// <summary>
    /// The header of a performance frame.
    /// </summary>
    public interface IPerformanceHeader
    {
        /// <summary>
        /// Get the entry count offset in this structure.
        /// </summary>
        /// <returns>The entry count offset in this structure.</returns>
        int GetEntryCountOffset();

        /// <summary>
        /// Set the DSP running behind flag.
        /// </summary>
        /// <param name="isRunningBehind">The flag.</param>
        void SetDspRunningBehind(bool isRunningBehind);

        /// <summary>
        /// Set the count of voices that were dropped.
        /// </summary>
        /// <param name="voiceCount">The count of voices that were dropped.</param>
        void SetVoiceDropCount(uint voiceCount);

        /// <summary>
        /// Set the start ticks of the <see cref="Dsp.AudioProcessor"/>. (before sending commands)
        /// </summary>
        /// <param name="startTicks">The start ticks of the <see cref="Dsp.AudioProcessor"/>. (before sending commands)</param>
        void SetStartRenderingTicks(ulong startTicks);

        /// <summary>
        /// Set the header magic.
        /// </summary>
        /// <param name="magic">The header magic.</param>
        void SetMagic(uint magic);

        /// <summary>
        /// Set the offset of the next performance header.
        /// </summary>
        /// <param name="nextOffset">The offset of the next performance header.</param>
        void SetNextOffset(int nextOffset);

        /// <summary>
        /// Set the total time taken by all the commands profiled.
        /// </summary>
        /// <param name="totalProcessingTime">The total time taken by all the commands profiled.</param>
        void SetTotalProcessingTime(int totalProcessingTime);

        /// <summary>
        /// Set the index of this performance frame.
        /// </summary>
        /// <param name="index">The index of this performance frame.</param>
        void SetIndex(uint index);

        /// <summary>
        /// Get the total count of entries in this frame.
        /// </summary>
        /// <returns>The total count of entries in this frame.</returns>
        int GetEntryCount();

        /// <summary>
        /// Get the total count of detailed entries in this frame.
        /// </summary>
        /// <returns>The total count of detailed entries in this frame.</returns>
        int GetEntryDetailCount();

        /// <summary>
        /// Set the total count of entries in this frame.
        /// </summary>
        /// <param name="entryCount">The total count of entries in this frame.</param>
        void SetEntryCount(int entryCount);

        /// <summary>
        /// Set the total count of detailed entries in this frame.
        /// </summary>
        /// <param name="entryDetailCount">The total count of detailed entries in this frame.</param>
        void SetEntryDetailCount(int entryDetailCount);
    }
}
