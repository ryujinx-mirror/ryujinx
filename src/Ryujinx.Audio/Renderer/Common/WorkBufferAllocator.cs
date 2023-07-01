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
                ulong alignedOffset = BitUtils.AlignUp(Offset, (ulong)align);

                if (alignedOffset + size <= (ulong)BackingMemory.Length)
                {
                    Memory<byte> result = BackingMemory.Slice((int)alignedOffset, (int)size);

                    Offset = alignedOffset + size;

                    // Clear the memory to be sure that is does not contain any garbage.
                    result.Span.Clear();

                    return result;
                }
            }

            return Memory<byte>.Empty;
        }

        public Memory<T> Allocate<T>(ulong count, int align) where T : unmanaged
        {
            Memory<byte> allocatedMemory = Allocate((ulong)Unsafe.SizeOf<T>() * count, align);

            if (allocatedMemory.IsEmpty)
            {
                return Memory<T>.Empty;
            }

            return SpanMemoryManager<T>.Cast(allocatedMemory);
        }

        public static ulong GetTargetSize<T>(ulong currentSize, ulong count, int align) where T : unmanaged
        {
            return BitUtils.AlignUp(currentSize, (ulong)align) + (ulong)Unsafe.SizeOf<T>() * count;
        }
    }
}
