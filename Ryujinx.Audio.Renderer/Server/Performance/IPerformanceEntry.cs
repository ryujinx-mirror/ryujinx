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
