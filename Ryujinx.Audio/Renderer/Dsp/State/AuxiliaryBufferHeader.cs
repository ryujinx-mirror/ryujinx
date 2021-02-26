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

using Ryujinx.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Dsp.State
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x40)]
    public struct AuxiliaryBufferHeader
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0xC)]
        public struct AuxiliaryBufferInfo
        {
            private const uint ReadOffsetPosition = 0x0;
            private const uint WriteOffsetPosition = 0x4;

            public uint ReadOffset;
            public uint WriteOffset;
            private uint _reserved;

            public static uint GetReadOffset(IVirtualMemoryManager manager, ulong bufferAddress)
            {
                return manager.Read<uint>(bufferAddress + ReadOffsetPosition);
            }

            public static uint GetWriteOffset(IVirtualMemoryManager manager, ulong bufferAddress)
            {
                return manager.Read<uint>(bufferAddress + WriteOffsetPosition);
            }

            public static void SetReadOffset(IVirtualMemoryManager manager, ulong bufferAddress, uint value)
            {
                manager.Write(bufferAddress + ReadOffsetPosition, value);
            }

            public static void SetWriteOffset(IVirtualMemoryManager manager, ulong bufferAddress, uint value)
            {
                manager.Write(bufferAddress + WriteOffsetPosition, value);
            }
        }

        public AuxiliaryBufferInfo BufferInfo;
        public unsafe fixed uint Unknown[13];
    }
}
