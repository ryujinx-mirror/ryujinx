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
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Server.Performance
{
    /// <summary>
    /// Implementation of <see cref="IPerformanceEntry"/> for performance metrics version 1.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    public struct PerformanceEntryVersion1 : IPerformanceEntry
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

        public int GetProcessingTime()
        {
            return ProcessingTime;
        }

        public int GetProcessingTimeOffset()
        {
            return 8;
        }

        public int GetStartTime()
        {
            return StartTime;
        }

        public int GetStartTimeOffset()
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
