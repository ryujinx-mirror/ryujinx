using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.Gpu.Memory
{
    class Buffer : IRange<Buffer>, IDisposable
    {
        private GpuContext _context;

        private IBuffer _buffer;

        public ulong Address { get; }
        public ulong Size    { get; }

        public ulong EndAddress => Address + Size;

        private int[] _sequenceNumbers;

        public Buffer(GpuContext context, ulong address, ulong size)
        {
            _context = context;
            Address  = address;
            Size     = size;

            _buffer = context.Renderer.CreateBuffer((int)size);

            _sequenceNumbers = new int[size / MemoryManager.PageSize];

            Invalidate();
        }

        public BufferRange GetRange(ulong address, ulong size)
        {
            int offset = (int)(address - Address);

            return new BufferRange(_buffer, offset, (int)size);
        }

        public bool OverlapsWith(ulong address, ulong size)
        {
            return Address < address + size && address < EndAddress;
        }

        public void SynchronizeMemory(ulong address, ulong size)
        {
            int currentSequenceNumber = _context.SequenceNumber;

            bool needsSync = false;

            ulong buffOffset = address - Address;

            ulong buffEndOffset = (buffOffset + size + MemoryManager.PageMask) & ~MemoryManager.PageMask;

            int startIndex = (int)(buffOffset    / MemoryManager.PageSize);
            int endIndex   = (int)(buffEndOffset / MemoryManager.PageSize);

            for (int index = startIndex; index < endIndex; index++)
            {
                if (_sequenceNumbers[index] != currentSequenceNumber)
                {
                    _sequenceNumbers[index] = currentSequenceNumber;

                    needsSync = true;
                }
            }

            if (!needsSync)
            {
                return;
            }

            (ulong, ulong)[] modifiedRanges = _context.PhysicalMemory.GetModifiedRanges(address, size);

            for (int index = 0; index < modifiedRanges.Length; index++)
            {
                (ulong mAddress, ulong mSize) = modifiedRanges[index];

                int offset = (int)(mAddress - Address);

                _buffer.SetData(offset, _context.PhysicalMemory.Read(mAddress, mSize));
            }
        }

        public void CopyTo(Buffer destination, int dstOffset)
        {
            _buffer.CopyTo(destination._buffer, 0, dstOffset, (int)Size);
        }

        public void Invalidate()
        {
            _buffer.SetData(0, _context.PhysicalMemory.Read(Address, Size));
        }

        public void Dispose()
        {
            _buffer.Dispose();
        }
    }
}