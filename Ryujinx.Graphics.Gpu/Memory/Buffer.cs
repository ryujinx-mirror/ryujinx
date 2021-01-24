using Ryujinx.Cpu.Tracking;
using Ryujinx.Graphics.GAL;
using Ryujinx.Memory.Range;
using Ryujinx.Memory.Tracking;
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

        /// <summary>
        /// Ranges of the buffer that have been modified on the GPU.
        /// Ranges defined here cannot be updated from CPU until a CPU waiting sync point is reached.
        /// Then, write tracking will signal, wait for GPU sync (generated at the syncpoint) and flush these regions.
        /// </summary>
        /// <remarks>
        /// This is null until at least one modification occurs.
        /// </remarks>
        private BufferModifiedRangeList _modifiedRanges = null;

        private CpuMultiRegionHandle _memoryTrackingGranular;

        private CpuRegionHandle _memoryTracking;

        private readonly RegionSignal _externalFlushDelegate;
        private readonly Action<ulong, ulong> _loadDelegate;
        private readonly Action<ulong, ulong> _modifiedDelegate;

        private int _sequenceNumber;

        private bool _useGranular;
        private bool _syncActionRegistered;

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

            _externalFlushDelegate = new RegionSignal(ExternalFlush);
            _loadDelegate = new Action<ulong, ulong>(LoadRegion);
            _modifiedDelegate = new Action<ulong, ulong>(RegionModified);
        }

        /// <summary>
        /// Gets a sub-range from the buffer, from a start address till the end of the buffer.
        /// </summary>
        /// <remarks>
        /// This can be used to bind and use sub-ranges of the buffer on the host API.
        /// </remarks>
        /// <param name="address">Start address of the sub-range, must be greater than or equal to the buffer address</param>
        /// <returns>The buffer sub-range</returns>
        public BufferRange GetRange(ulong address)
        {
            ulong offset = address - Address;

            return new BufferRange(Handle, (int)offset, (int)(Size - offset));
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

                    if (_modifiedRanges != null)
                    {
                        _modifiedRanges.ExcludeModifiedRegions(Address, Size, _loadDelegate);
                    }
                    else
                    {
                        _context.Renderer.SetBufferData(Handle, 0, _context.PhysicalMemory.GetSpan(Address, (int)Size));
                    }
                    
                    _sequenceNumber = _context.SequenceNumber;
                }
            }
        }

        /// <summary>
        /// Ensure that the modified range list exists.
        /// </summary>
        private void EnsureRangeList()
        {
            if (_modifiedRanges == null)
            {
                _modifiedRanges = new BufferModifiedRangeList(_context);
            }
        }

        /// <summary>
        /// Signal that the given region of the buffer has been modified.
        /// </summary>
        /// <param name="address">The start address of the modified region</param>
        /// <param name="size">The size of the modified region</param>
        public void SignalModified(ulong address, ulong size)
        {
            EnsureRangeList();

            _modifiedRanges.SignalModified(address, size);

            if (!_syncActionRegistered)
            {
                _context.RegisterSyncAction(SyncAction);
                _syncActionRegistered = true;
            }
        }

        /// <summary>
        /// Indicate that mofifications in a given region of this buffer have been overwritten.
        /// </summary>
        /// <param name="address">The start address of the region</param>
        /// <param name="size">The size of the region</param>
        public void ClearModified(ulong address, ulong size)
        {
            if (_modifiedRanges != null)
            {
                _modifiedRanges.Clear(address, size);
            }
        }

        /// <summary>
        /// Action to be performed when a syncpoint is reached after modification.
        /// This will register read/write tracking to flush the buffer from GPU when its memory is used.
        /// </summary>
        private void SyncAction()
        {
            _syncActionRegistered = false;

            if (_useGranular)
            {
                _modifiedRanges.GetRanges(Address, Size, (address, size) =>
                {
                    _memoryTrackingGranular.RegisterAction(address, size, _externalFlushDelegate);
                    SynchronizeMemory(address, size);
                });
            }
            else
            {
                _memoryTracking.RegisterAction(_externalFlushDelegate);
                SynchronizeMemory(Address, Size);
            }
        }

        /// <summary>
        /// Inherit modified ranges from another buffer.
        /// </summary>
        /// <param name="from">The buffer to inherit from</param>
        public void InheritModifiedRanges(Buffer from)
        {
            if (from._modifiedRanges != null)
            {
                if (from._syncActionRegistered && !_syncActionRegistered)
                {
                    _context.RegisterSyncAction(SyncAction);
                    _syncActionRegistered = true;
                }

                EnsureRangeList();
                _modifiedRanges.InheritRanges(from._modifiedRanges, (ulong address, ulong size) =>
                {
                    if (_useGranular)
                    {
                        _memoryTrackingGranular.RegisterAction(address, size, _externalFlushDelegate);
                    }
                    else
                    {
                        _memoryTracking.RegisterAction(_externalFlushDelegate);
                    }
                });
            }
        }

        /// <summary>
        /// Determine if a given region of the buffer has been modified, and must be flushed.
        /// </summary>
        /// <param name="address">The start address of the region</param>
        /// <param name="size">The size of the region</param>
        /// <returns></returns>
        public bool IsModified(ulong address, ulong size)
        {
            if (_modifiedRanges != null)
            {
                return _modifiedRanges.HasRange(address, size);
            }

            return false;
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

            if (_modifiedRanges != null)
            {
                _modifiedRanges.ExcludeModifiedRegions(mAddress, mSize, _loadDelegate);
            }
            else
            {
                LoadRegion(mAddress, mSize);
            }
        }

        /// <summary>
        /// Load a region of the buffer from memory.
        /// </summary>
        /// <param name="mAddress">Start address of the modified region</param>
        /// <param name="mSize">Size of the modified region</param>
        private void LoadRegion(ulong mAddress, ulong mSize)
        {
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
        /// Align a given address and size region to page boundaries.
        /// </summary>
        /// <param name="address">The start address of the region</param>
        /// <param name="size">The size of the region</param>
        /// <returns>The page aligned address and size</returns>
        private static (ulong address, ulong size) PageAlign(ulong address, ulong size)
        {
            ulong pageMask = MemoryManager.PageMask;
            ulong rA = address & ~pageMask;
            ulong rS = ((address + size + pageMask) & ~pageMask) - rA;
            return (rA, rS);
        }

        /// <summary>
        /// Flush modified ranges of the buffer from another thread.
        /// This will flush all modifications made before the active SyncNumber was set, and may block to wait for GPU sync.
        /// </summary>
        /// <param name="address">Address of the memory action</param>
        /// <param name="size">Size in bytes</param>
        public void ExternalFlush(ulong address, ulong size)
        {
            _context.Renderer.BackgroundContextAction(() =>
            {
                var ranges = _modifiedRanges;

                if (ranges != null)
                {
                    (address, size) = PageAlign(address, size);
                    ranges.WaitForAndGetRanges(address, size, Flush);
                }
            });
        }

        /// <summary>
        /// Called when part of the memory for this buffer has been unmapped.
        /// Calls are from non-GPU threads.
        /// </summary>
        /// <param name="address">Start address of the unmapped region</param>
        /// <param name="size">Size of the unmapped region</param>
        public void Unmapped(ulong address, ulong size)
        {
            _modifiedRanges?.Clear(address, size);
        }

        /// <summary>
        /// Disposes the host buffer.
        /// </summary>
        public void Dispose()
        {
            _modifiedRanges?.Clear();

            _memoryTrackingGranular?.Dispose();
            _memoryTracking?.Dispose();

            _context.Renderer.DeleteBuffer(Handle);
        }
    }
}