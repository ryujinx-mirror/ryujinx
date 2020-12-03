using Ryujinx.Cpu.Tracking;
using Ryujinx.Graphics.GAL;
using Ryujinx.Memory.Range;
using System;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Buffer, used to store vertex and index data, uniform and storage buffers, and others.
    /// </summary>
    class Buffer : IRange, IDisposable
    {
        private static ulong GranularBufferThreshold = 4096;

        private readonly GpuContext _context;

        /// <summary>
        /// Host buffer handle.
        /// </summary>
        public BufferHandle Handle { get; }

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

        private CpuMultiRegionHandle _memoryTrackingGranular;
        private CpuRegionHandle _memoryTracking;
        private readonly Action<ulong, ulong> _modifiedDelegate;
        private int _sequenceNumber;

        private bool _useGranular;

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

            Handle = context.Renderer.CreateBuffer((int)size);

            _useGranular = size > GranularBufferThreshold;

            if (_useGranular)
            {
                _memoryTrackingGranular = context.PhysicalMemory.BeginGranularTracking(address, size);
            }
            else
            {
                _memoryTracking = context.PhysicalMemory.BeginTracking(address, size);
            }

            _modifiedDelegate = new Action<ulong, ulong>(RegionModified);
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

            return new BufferRange(Handle, offset, (int)size);
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
            if (_useGranular)
            {
                _memoryTrackingGranular.QueryModified(address, size, _modifiedDelegate, _context.SequenceNumber);
            }
            else
            {
                if (_memoryTracking.Dirty && _context.SequenceNumber != _sequenceNumber)
                {
                    _memoryTracking.Reprotect();
                    _context.Renderer.SetBufferData(Handle, 0, _context.PhysicalMemory.GetSpan(Address, (int)Size));
                    _sequenceNumber = _context.SequenceNumber;
                }
            }
        }

        /// <summary>
        /// Indicate that a region of the buffer was modified, and must be loaded from memory.
        /// </summary>
        /// <param name="mAddress">Start address of the modified region</param>
        /// <param name="mSize">Size of the modified region</param>
        private void RegionModified(ulong mAddress, ulong mSize)
        {
            if (mAddress < Address)
            {
                mAddress = Address;
            }

            ulong maxSize = Address + Size - mAddress;

            if (mSize > maxSize)
            {
                mSize = maxSize;
            }

            int offset = (int)(mAddress - Address);

            _context.Renderer.SetBufferData(Handle, offset, _context.PhysicalMemory.GetSpan(mAddress, (int)mSize));
        }

        /// <summary>
        /// Performs copy of all the buffer data from one buffer to another.
        /// </summary>
        /// <param name="destination">The destination buffer to copy the data into</param>
        /// <param name="dstOffset">The offset of the destination buffer to copy into</param>
        public void CopyTo(Buffer destination, int dstOffset)
        {
            _context.Renderer.Pipeline.CopyBuffer(Handle, destination.Handle, 0, dstOffset, (int)Size);
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

            byte[] data = _context.Renderer.GetBufferData(Handle, offset, (int)size);

            // TODO: When write tracking shaders, they will need to be aware of changes in overlapping buffers.
            _context.PhysicalMemory.WriteUntracked(address, data);
        }

        /// <summary>
        /// Disposes the host buffer.
        /// </summary>
        public void Dispose()
        {
            _context.Renderer.DeleteBuffer(Handle);

            _memoryTrackingGranular?.Dispose();
            _memoryTracking?.Dispose();
        }
    }
}