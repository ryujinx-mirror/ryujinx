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

using Ryujinx.Audio.Renderer.Utils;
using Ryujinx.Common;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Common
{
    public class WorkBufferAllocator
    {
        public Memory<byte> BackingMemory { get; }

        public ulong Offset { get; private set; }

        public WorkBufferAllocator(Memory<byte> backingMemory)
        {
            BackingMemory = backingMemory;
        }

        public Memory<byte> Allocate(ulong size, int align)
        {
            Debug.Assert(align != 0);

            if (size != 0)
            {
                ulong alignedOffset = BitUtils.AlignUp(Offset, align);

                if (alignedOffset + size <= (ulong)BackingMemory.Length)
                {
                    Memory<byte> result = BackingMemory.Slice((int)alignedOffset, (int)size);

                    Offset = alignedOffset + size;

                    // Clear the memory to be sure that is does not contain any garbage.
                    result.Span.Fill(0);

                    return result;
                }
            }

            return Memory<byte>.Empty;
        }

        public Memory<T> Allocate<T>(ulong count, int align) where T: unmanaged
        {
            Memory<byte> allocatedMemory = Allocate((ulong)Unsafe.SizeOf<T>() * count, align);

            if (allocatedMemory.IsEmpty)
            {
                return Memory<T>.Empty;
            }

            return SpanMemoryManager<T>.Cast(allocatedMemory);
        }

        public static ulong GetTargetSize<T>(ulong currentSize, ulong count, int align) where T: unmanaged
        {
            return BitUtils.AlignUp(currentSize, align) + (ulong)Unsafe.SizeOf<T>() * count;
        }
    }
}
