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
