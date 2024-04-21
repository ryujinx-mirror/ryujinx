using System;
using System.Buffers;

namespace Ryujinx.Memory
{
    public interface IWritableBlock
    {
        /// <summary>
        /// Writes data to CPU mapped memory, with write tracking.
        /// </summary>
        /// <param name="va">Virtual address to write the data into</param>
        /// <param name="data">Data to be written</param>
        /// <exception cref="InvalidMemoryRegionException">Throw for unhandled invalid or unmapped memory accesses</exception>
        void Write(ulong va, ReadOnlySequence<byte> data)
        {
            foreach (ReadOnlyMemory<byte> segment in data)
            {
                Write(va, segment.Span);
                va += (ulong)segment.Length;
            }
        }

        void Write(ulong va, ReadOnlySpan<byte> data);

        void WriteUntracked(ulong va, ReadOnlySpan<byte> data) => Write(va, data);
    }
}
