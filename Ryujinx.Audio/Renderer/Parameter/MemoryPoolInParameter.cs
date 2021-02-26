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
using CpuAddress = System.UInt64;

namespace Ryujinx.Audio.Renderer.Parameter
{
    /// <summary>
    /// Input information for a memory pool.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MemoryPoolInParameter
    {
        /// <summary>
        /// The CPU address used by the memory pool.
        /// </summary>
        public CpuAddress CpuAddress;

        /// <summary>
        /// The size used by the memory pool.
        /// </summary>
        public ulong Size;

        /// <summary>
        /// The target state the user wants.
        /// </summary>
        public MemoryPoolUserState State;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        private unsafe fixed uint _reserved[3];
    }
}
