using Ryujinx.Graphics.GAL;
using Ryujinx.Memory.Range;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Buffer cache.
    /// </summary>
    class BufferCache : IDisposable
    {
        private const int OverlapsBufferInitialCapacity = 10;
        private const int OverlapsBufferMaxCapacity     = 10000;

        private const ulong BufferAlignmentSize = 0x1000;
        private const ulong BufferAlignmentMask = BufferAlignmentSize - 1;

        private const ulong MaxDynamicGrowthSize = 0x100000;

        private readonly GpuContext _context;
        private readonly PhysicalMemory _physicalMemory;

        private readonly RangeList<Buffer> _buffers;

        private Buffer[] _bufferOverlaps;

        private readonly Dictionary<ulong, BufferCacheEntry> _dirtyCache;

        public event Action NotifyBuffersModified;

        /// <summary>
        /// Creates a new instance of the buffer manager.
        /// </summary>
        /// <param name="context">The GPU context that the buffer manager belongs to</param>
        /// <param name="physicalMemory">Physical memory where the cached buffers are mapped</param>
        public BufferCache(GpuContext context, PhysicalMemory physicalMemory)
        {
            _context = context;
            _physicalMemory = physicalMemory;

            _buffers = new RangeList<Buffer>();

            _bufferOverlaps = new Buffer[OverlapsBufferInitialCapacity];

            _dirtyCache = new Dictionary<ulong, BufferCacheEntry>();
        }

        /// <summary>
        /// Handles removal of buffers written to a memory region being unmapped.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        public void MemoryUnmappedHandler(object sender, UnmapEventArgs e)
        {
            Buffer[] overlaps = new Buffer[10];
            int overlapCount;

            ulong address = ((MemoryManager)sender).Translate(e.Address);
            ulong size = e.Size;

            lock (_buffers)
            {
                overlapCount = _buffers.FindOverlaps(address, size, ref overlaps);
            }

            for (int i = 0; i < overlapCount; i++)
            {
                overlaps[i].Unmapped(address, size);
            }
        }

        /// <summary>
        /// Performs address translation of the GPU virtual address, and creates a
        /// new buffer, if needed, for the specified range.
        /// </summary>
        /// <param name="memoryManager">GPU memory manager where the buffer is mapped</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the buffer</param>
        /// <returns>CPU virtual address of the buffer, after address translation</returns>
        public ulong TranslateAndCreateBuffer(MemoryManager memoryManager, ulong gpuVa, ulong size)
        {
            if (gpuVa == 0)
            {
                return 0;
            }

            ulong address = memoryManager.Translate(gpuVa);

            if (address == MemoryManager.PteUnmapped)
            {
                return 0;
            }

            CreateBuffer(address, size);

            return address;
        }

        /// <summary>
        /// Creates a new buffer for the specified range, if it does not yet exist.
        /// This can be used to ensure the existance of a buffer.
        /// </summary>
        /// <param name="address">Address of the buffer in memory</param>
        /// <param name="size">Size of the buffer in bytes</param>
        public void CreateBuffer(ulong address, ulong size)
        {
            ulong endAddress = address + size;

            ulong alignedAddress = address & ~BufferAlignmentMask;

            ulong alignedEndAddress = (endAddress + BufferAlignmentMask) & ~BufferAlignmentMask;

            // The buffer must have the size of at least one page.
            if (alignedEndAddress == alignedAddress)
            {
                alignedEndAddress += BufferAlignmentSize;
            }

            CreateBufferAligned(alignedAddress, alignedEndAddress - alignedAddress);
        }

        /// <summary>
        /// Performs address translation of the GPU virtual address, and attempts to force
        /// the buffer in the region as dirty.
        /// The buffer lookup for this function is cached in a dictionary for quick access, which
        /// accelerates common UBO updates.
        /// </summary>
        /// <param name="memoryManager">GPU memory manager where the buffer is mapped</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the buffer</param>
        public void ForceDirty(MemoryManager memoryManager, ulong gpuVa, ulong size)
        {
            if (!_dirtyCache.TryGetValue(gpuVa, out BufferCacheEntry result) ||
                result.EndGpuAddress < gpuVa + size ||
                result.UnmappedSequence != result.Buffer.UnmappedSequence)
            {
                ulong address = TranslateAndCreateBuffer(memoryManager, gpuVa, size);
                result = new BufferCacheEntry(address, gpuVa, GetBuffer(address, size));

                _dirtyCache[gpuVa] = result;
            }

            result.Buffer.ForceDirty(result.Address, size);
        }

        /// <summary>
        /// Creates a new buffer for the specified range, if needed.
        /// If a buffer where this range can be fully contained already exists,
        /// then the creation of a new buffer is not necessary.
        /// </summary>
        /// <param name="address">Address of the buffer in guest memory</param>
        /// <param name="size">Size in bytes of the buffer</param>
        private void CreateBufferAligned(ulong address, ulong size)
        {
            int overlapsCount;

            lock (_buffers)
            {
                overlapsCount = _buffers.FindOverlapsNonOverlapping(address, size, ref _bufferOverlaps);
            }

            if (overlapsCount != 0)
            {
                // The buffer already exists. We can just return the existing buffer
                // if the buffer we need is fully contained inside the overlapping buffer.
                // Otherwise, we must delete the overlapping buffers and create a bigger buffer
                // that fits all the data we need. We also need to copy the contents from the
                // old buffer(s) to the new buffer.

                ulong endAddress = address + size;

                if (_bufferOverlaps[0].Address > address || _bufferOverlaps[0].EndAddress < endAddress)
                {
                    // Check if the following conditions are met:
                    // - We have a single overlap.
                    // - The overlap starts at or before the requested range. That is, the overlap happens at the end.
                    // - The size delta between the new, merged buffer and the old one is of at most 2 pages.
                    // In this case, we attempt to extend the buffer further than the requested range,
                    // this can potentially avoid future resizes if the application keeps using overlapping
                    // sequential memory.
                    // Allowing for 2 pages (rather than just one) is necessary to catch cases where the
                    // range crosses a page, and after alignment, ends having a size of 2 pages.
                    if (overlapsCount == 1 &&
                        address >= _bufferOverlaps[0].Address &&
                        endAddress - _bufferOverlaps[0].EndAddress <= BufferAlignmentSize * 2)
                    {
                        // Try to grow the buffer by 1.5x of its current size.
                        // This improves performance in the cases where the buffer is resized often by small amounts.
                        ulong existingSize = _bufferOverlaps[0].Size;
                        ulong growthSize = (existingSize + Math.Min(existingSize >> 1, MaxDynamicGrowthSize)) & ~BufferAlignmentMask;

                        size = Math.Max(size, growthSize);
                        endAddress = address + size;

                        overlapsCount = _buffers.FindOverlapsNonOverlapping(address, size, ref _bufferOverlaps);
                    }

                    for (int index = 0; index < overlapsCount; index++)
                    {
                        Buffer buffer = _bufferOverlaps[index];

                        address    = Math.Min(address,    buffer.Address);
                        endAddress = Math.Max(endAddress, buffer.EndAddress);

                        lock (_buffers)
                        {
                            _buffers.Remove(buffer);
                        }
                    }

                    ulong newSize = endAddress - address;

                    Buffer newBuffer = new Buffer(_context, _physicalMemory, address, newSize, _bufferOverlaps.Take(overlapsCount));

                    lock (_buffers)
                    {
                        _buffers.Add(newBuffer);
                    }

                    for (int index = 0; index < overlapsCount; index++)
                    {
                        Buffer buffer = _bufferOverlaps[index];

                        int dstOffset = (int)(buffer.Address - newBuffer.Address);

                        buffer.CopyTo(newBuffer, dstOffset);
                        newBuffer.InheritModifiedRanges(buffer);

                        buffer.DisposeData();
                    }

                    newBuffer.SynchronizeMemory(address, newSize);

                    // Existing buffers were modified, we need to rebind everything.
                    NotifyBuffersModified?.Invoke();
                }
            }
            else
            {
                // No overlap, just create a new buffer.
                Buffer buffer = new Buffer(_context, _physicalMemory, address, size);

                lock (_buffers)
                {
                    _buffers.Add(buffer);
                }
            }

            ShrinkOverlapsBufferIfNeeded();
        }

        /// <summary>
        /// Resizes the temporary buffer used for range list intersection results, if it has grown too much.
        /// </summary>
        private void ShrinkOverlapsBufferIfNeeded()
        {
            if (_bufferOverlaps.Length > OverlapsBufferMaxCapacity)
            {
                Array.Resize(ref _bufferOverlaps, OverlapsBufferMaxCapacity);
            }
        }

        /// <summary>
        /// Copy a buffer data from a given address to another.
        /// </summary>
        /// <remarks>
        /// This does a GPU side copy.
        /// </remarks>
        /// <param name="memoryManager">GPU memory manager where the buffer is mapped</param>
        /// <param name="srcVa">GPU virtual address of the copy source</param>
        /// <param name="dstVa">GPU virtual address of the copy destination</param>
        /// <param name="size">Size in bytes of the copy</param>
        public void CopyBuffer(MemoryManager memoryManager, ulong srcVa, ulong dstVa, ulong size)
        {
            ulong srcAddress = TranslateAndCreateBuffer(memoryManager, srcVa, size);
            ulong dstAddress = TranslateAndCreateBuffer(memoryManager, dstVa, size);

            Buffer srcBuffer = GetBuffer(srcAddress, size);
            Buffer dstBuffer = GetBuffer(dstAddress, size);

            int srcOffset = (int)(srcAddress - srcBuffer.Address);
            int dstOffset = (int)(dstAddress - dstBuffer.Address);

            _context.Renderer.Pipeline.CopyBuffer(
                srcBuffer.Handle,
                dstBuffer.Handle,
                srcOffset,
                dstOffset,
                (int)size);

            if (srcBuffer.IsModified(srcAddress, size))
            {
                dstBuffer.SignalModified(dstAddress, size);
            }
            else
            {
                // Optimization: If the data being copied is already in memory, then copy it directly instead of flushing from GPU.

                dstBuffer.ClearModified(dstAddress, size);
                memoryManager.Physical.WriteUntracked(dstAddress, memoryManager.Physical.GetSpan(srcAddress, (int)size));
            }
        }

        /// <summary>
        /// Clears a buffer at a given address with the specified value.
        /// </summary>
        /// <remarks>
        /// Both the address and size must be aligned to 4 bytes.
        /// </remarks>
        /// <param name="memoryManager">GPU memory manager where the buffer is mapped</param>
        /// <param name="gpuVa">GPU virtual address of the region to clear</param>
        /// <param name="size">Number of bytes to clear</param>
        /// <param name="value">Value to be written into the buffer</param>
        public void ClearBuffer(MemoryManager memoryManager, ulong gpuVa, ulong size, uint value)
        {
            ulong address = TranslateAndCreateBuffer(memoryManager, gpuVa, size);

            Buffer buffer = GetBuffer(address, size);

            int offset = (int)(address - buffer.Address);

            _context.Renderer.Pipeline.ClearBuffer(buffer.Handle, offset, (int)size, value);

            buffer.SignalModified(address, size);
        }

        /// <summary>
        /// Gets a buffer sub-range for a given GPU memory range.
        /// </summary>
        /// <param name="memoryManager">GPU memory manager where the buffer is mapped</param>
        /// <param name="gpuVa">Start GPU virtual address of the buffer</param>
        /// <param name="size">Size in bytes of the buffer</param>
        /// <returns>The buffer sub-range for the given range</returns>
        public BufferRange GetGpuBufferRange(MemoryManager memoryManager, ulong gpuVa, ulong size)
        {
            return GetBufferRange(TranslateAndCreateBuffer(memoryManager, gpuVa, size), size);
        }

        /// <summary>
        /// Gets a buffer sub-range starting at a given memory address.
        /// </summary>
        /// <param name="address">Start address of the memory range</param>
        /// <param name="size">Size in bytes of the memory range</param>
        /// <param name="write">Whether the buffer will be written to by this use</param>
        /// <returns>The buffer sub-range starting at the given memory address</returns>
        public BufferRange GetBufferRangeTillEnd(ulong address, ulong size, bool write = false)
        {
            return GetBuffer(address, size, write).GetRange(address);
        }

        /// <summary>
        /// Gets a buffer sub-range for a given memory range.
        /// </summary>
        /// <param name="address">Start address of the memory range</param>
        /// <param name="size">Size in bytes of the memory range</param>
        /// <param name="write">Whether the buffer will be written to by this use</param>
        /// <returns>The buffer sub-range for the given range</returns>
        public BufferRange GetBufferRange(ulong address, ulong size, bool write = false)
        {
            return GetBuffer(address, size, write).GetRange(address, size);
        }

        /// <summary>
        /// Gets a buffer for a given memory range.
        /// A buffer overlapping with the specified range is assumed to already exist on the cache.
        /// </summary>
        /// <param name="address">Start address of the memory range</param>
        /// <param name="size">Size in bytes of the memory range</param>
        /// <param name="write">Whether the buffer will be written to by this use</param>
        /// <returns>The buffer where the range is fully contained</returns>
        private Buffer GetBuffer(ulong address, ulong size, bool write = false)
        {
            Buffer buffer;

            if (size != 0)
            {
                lock (_buffers)
                {
                    buffer = _buffers.FindFirstOverlap(address, size);
                }

                buffer.SynchronizeMemory(address, size);

                if (write)
                {
                    buffer.SignalModified(address, size);
                }
            }
            else
            {
                lock (_buffers)
                {
                    buffer = _buffers.FindFirstOverlap(address, 1);
                }
            }

            return buffer;
        }

        /// <summary>
        /// Performs guest to host memory synchronization of a given memory range.
        /// </summary>
        /// <param name="address">Start address of the memory range</param>
        /// <param name="size">Size in bytes of the memory range</param>
        public void SynchronizeBufferRange(ulong address, ulong size)
        {
            if (size != 0)
            {
                Buffer buffer;

                lock (_buffers)
                {
                    buffer = _buffers.FindFirstOverlap(address, size);
                }

                buffer.SynchronizeMemory(address, size);
            }
        }

        /// <summary>
        /// Disposes all buffers in the cache.
        /// It's an error to use the buffer manager after disposal.
        /// </summary>
        public void Dispose()
        {
            lock (_buffers)
            {
                foreach (Buffer buffer in _buffers)
                {
                    buffer.Dispose();
                }
            }
        }
    }
}