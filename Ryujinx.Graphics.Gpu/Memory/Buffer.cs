using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Buffer, used to store vertex and index data, uniform and storage buffers, and others.
    /// </summary>
    class Buffer : IRange, IDisposable
    {
        private readonly GpuContext _context;

        /// <summary>
        /// Host buffer object.
        /// </summary>
        public IBuffer HostBuffer { get; }

        /// <summary>
        /// Start address of the buffer in guest memory.
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// Size of the buffer in bytes.
        /// </summary>
        public ulong Size { get; }

        /// <summary>
        /// End address of the buffer in guest memory.
        /// </summary>
        public ulong EndAddress => Address + Size;

        private int[] _sequenceNumbers;

        /// <summary>
        /// Creates a new instance of the buffer.
        /// </summary>
        /// <param name="context">GPU context that the buffer belongs to</param>
        /// <param name="address">Start address of the buffer</param>
        /// <param name="size">Size of the buffer in bytes</param>
        public Buffer(GpuContext context, ulong address, ulong size)
        {
            _context = context;
            Address  = address;
            Size     = size;

            HostBuffer = context.Renderer.CreateBuffer((int)size);

            _sequenceNumbers = new int[size / MemoryManager.PageSize];

            Invalidate();
        }

        /// <summary>
        /// Gets a sub-range from the buffer.
        /// </summary>
        /// <remarks>
        /// This can be used to bind and use sub-ranges of the buffer on the host API.
        /// </remarks>
        /// <param name="address">Start address of the sub-range, must be greater than or equal to the buffer address</param>
        /// <param name="size">Size in bytes of the sub-range, must be less than or equal to the buffer size</param>
        /// <returns>The buffer sub-range</returns>
        public BufferRange GetRange(ulong address, ulong size)
        {
            int offset = (int)(address - Address);

            return new BufferRange(HostBuffer, offset, (int)size);
        }

        /// <summary>
        /// Checks if a given range overlaps with the buffer.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size in bytes of the range</param>
        /// <returns>True if the range overlaps, false otherwise</returns>
        public bool OverlapsWith(ulong address, ulong size)
        {
            return Address < address + size && address < EndAddress;
        }

        /// <summary>
        /// Performs guest to host memory synchronization of the buffer data.
        /// </summary>
        /// <remarks>
        /// This causes the buffer data to be overwritten if a write was detected from the CPU,
        /// since the last call to this method.
        /// </remarks>
        /// <param name="address">Start address of the range to synchronize</param>
        /// <param name="size">Size in bytes of the range to synchronize</param>
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

            (ulong, ulong)[] modifiedRanges = _context.PhysicalMemory.GetModifiedRanges(address, size, ResourceName.Buffer);

            for (int index = 0; index < modifiedRanges.Length; index++)
            {
                (ulong mAddress, ulong mSize) = modifiedRanges[index];

                int offset = (int)(mAddress - Address);

                HostBuffer.SetData(offset, _context.PhysicalMemory.Read(mAddress, mSize));
            }
        }

        /// <summary>
        /// Performs copy of all the buffer data from one buffer to another.
        /// </summary>
        /// <param name="destination">The destination buffer to copy the data into</param>
        /// <param name="dstOffset">The offset of the destination buffer to copy into</param>
        public void CopyTo(Buffer destination, int dstOffset)
        {
            HostBuffer.CopyTo(destination.HostBuffer, 0, dstOffset, (int)Size);
        }

        /// <summary>
        /// Flushes a range of the buffer.
        /// This writes the range data back into guest memory.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size in bytes of the range</param>
        public void Flush(ulong address, ulong size)
        {
            int offset = (int)(address - Address);

            byte[] data = HostBuffer.GetData(offset, (int)size);

            _context.PhysicalMemory.Write(address, data);
        }

        /// <summary>
        /// Invalidates all the buffer data, causing it to be read from guest memory.
        /// </summary>
        public void Invalidate()
        {
            HostBuffer.SetData(0, _context.PhysicalMemory.Read(Address, Size));
        }

        /// <summary>
        /// Disposes the host buffer.
        /// </summary>
        public void Dispose()
        {
            HostBuffer.Dispose();
        }
    }
}