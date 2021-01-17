using Ryujinx.Memory.Range;
using System;
using System.Linq;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// A range within a buffer that has been modified by the GPU.
    /// </summary>
    class BufferModifiedRange : IRange
    {
        /// <summary>
        /// Start address of the range in guest memory.
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// Size of the range in bytes.
        /// </summary>
        public ulong Size { get; }

        /// <summary>
        /// End address of the range in guest memory.
        /// </summary>
        public ulong EndAddress => Address + Size;

        /// <summary>
        /// The GPU sync number at the time of the last modification.
        /// </summary>
        public ulong SyncNumber { get; internal set; }

        /// <summary>
        /// Creates a new instance of a modified range.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size of the range in bytes</param>
        /// <param name="syncNumber">The GPU sync number at the time of creation</param>
        public BufferModifiedRange(ulong address, ulong size, ulong syncNumber)
        {
            Address = address;
            Size = size;
            SyncNumber = syncNumber;
        }

        /// <summary>
        /// Checks if a given range overlaps with the modified range.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size in bytes of the range</param>
        /// <returns>True if the range overlaps, false otherwise</returns>
        public bool OverlapsWith(ulong address, ulong size)
        {
            return Address < address + size && address < EndAddress;
        }
    }

    /// <summary>
    /// A structure used to track GPU modified ranges within a buffer.
    /// </summary>
    class BufferModifiedRangeList : RangeList<BufferModifiedRange>
    {
        private GpuContext _context;

        private object _lock = new object();

        // The list can be accessed from both the GPU thread, and a background thread.
        private BufferModifiedRange[] _foregroundOverlaps = new BufferModifiedRange[1];
        private BufferModifiedRange[] _backgroundOverlaps = new BufferModifiedRange[1];

        /// <summary>
        /// Creates a new instance of a modified range list.
        /// </summary>
        /// <param name="context">GPU context that the buffer range list belongs to</param>
        public BufferModifiedRangeList(GpuContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Given an input range, calls the given action with sub-ranges which exclude any of the modified regions.
        /// </summary>
        /// <param name="address">Start address of the query range</param>
        /// <param name="size">Size of the query range in bytes</param>
        /// <param name="action">Action to perform for each remaining sub-range of the input range</param>
        public void ExcludeModifiedRegions(ulong address, ulong size, Action<ulong, ulong> action)
        {
            lock (_lock)
            {
                // Slices a given region using the modified regions in the list. Calls the action for the new slices.
                int count = FindOverlapsNonOverlapping(address, size, ref _foregroundOverlaps);

                for (int i = 0; i < count; i++)
                {
                    BufferModifiedRange overlap = _foregroundOverlaps[i];
                    
                    if (overlap.Address > address)
                    {
                        // The start of the remaining region is uncovered by this overlap. Call the action for it.
                        action(address, overlap.Address - address);
                    }

                    // Remaining region is after this overlap.
                    size -= overlap.EndAddress - address;
                    address = overlap.EndAddress;
                }

                if ((long)size > 0)
                {
                    // If there is any region left after removing the overlaps, signal it.
                    action(address, size);
                }
            }
        }

        /// <summary>
        /// Signal that a region of the buffer has been modified, and add the new region to the range list.
        /// Any overlapping ranges will be (partially) removed.
        /// </summary>
        /// <param name="address">Start address of the modified region</param>
        /// <param name="size">Size of the modified region in bytes</param>
        public void SignalModified(ulong address, ulong size)
        {
            // Must lock, as this can affect flushes from the background thread.
            lock (_lock)
            {
                // We may overlap with some existing modified regions. They must be cut into by the new entry.
                int count = FindOverlapsNonOverlapping(address, size, ref _foregroundOverlaps);

                ulong endAddress = address + size;
                ulong syncNumber = _context.SyncNumber;

                for (int i = 0; i < count; i++)
                {
                    // The overlaps must be removed or split.

                    BufferModifiedRange overlap = _foregroundOverlaps[i];

                    if (overlap.Address == address && overlap.Size == size)
                    {
                        // Region already exists. Just update the existing sync number.
                        overlap.SyncNumber = syncNumber;

                        return;
                    }

                    Remove(overlap);

                    if (overlap.Address < address && overlap.EndAddress > address)
                    {
                        // A split item must be created behind this overlap.

                        Add(new BufferModifiedRange(overlap.Address, address - overlap.Address, overlap.SyncNumber));
                    }

                    if (overlap.Address < endAddress && overlap.EndAddress > endAddress)
                    {
                        // A split item must be created after this overlap.

                        Add(new BufferModifiedRange(endAddress, overlap.EndAddress - endAddress, overlap.SyncNumber));
                    }
                }

                Add(new BufferModifiedRange(address, size, syncNumber));
            }
        }

        /// <summary>
        /// Gets modified ranges within the specified region, and then fires the given action for each range individually.
        /// </summary>
        /// <param name="address">Start address to query</param>
        /// <param name="size">Size to query</param>
        /// <param name="rangeAction">The action to call for each modified range</param>
        public void GetRanges(ulong address, ulong size, Action<ulong, ulong> rangeAction)
        {
            int count = 0;

            // Range list must be consistent for this operation.
            lock (_lock)
            {
                count = FindOverlapsNonOverlapping(address, size, ref _foregroundOverlaps);
            }

            for (int i = 0; i < count; i++)
            {
                BufferModifiedRange overlap = _foregroundOverlaps[i];
                rangeAction(overlap.Address, overlap.Size);
            }
        }

        /// <summary>
        /// Queries if a range exists within the specified region.
        /// </summary>
        /// <param name="address">Start address to query</param>
        /// <param name="size">Size to query</param>
        /// <returns>True if a range exists in the specified region, false otherwise</returns>
        public bool HasRange(ulong address, ulong size)
        {
            // Range list must be consistent for this operation.
            lock (_lock)
            {
                return FindOverlapsNonOverlapping(address, size, ref _foregroundOverlaps) > 0;
            }
        }

        /// <summary>
        /// Gets modified ranges within the specified region, waits on ones from a previous sync number,
        /// and then fires the given action for each range individually.
        /// </summary>
        /// <remarks>
        /// This function assumes it is called from the background thread.
        /// Modifications from the current sync number are ignored because the guest should not expect them to be available yet.
        /// They will remain reserved, so that any data sync prioritizes the data in the GPU.
        /// </remarks>
        /// <param name="address">Start address to query</param>
        /// <param name="size">Size to query</param>
        /// <param name="rangeAction">The action to call for each modified range</param>
        public void WaitForAndGetRanges(ulong address, ulong size, Action<ulong, ulong> rangeAction)
        {
            ulong endAddress = address + size;
            ulong currentSync = _context.SyncNumber;

            int rangeCount = 0;

            // Range list must be consistent for this operation
            lock (_lock)
            {
                rangeCount = FindOverlapsNonOverlapping(address, size, ref _backgroundOverlaps);
            }

            if (rangeCount == 0)
            {
                return;
            }

            // First, determine which syncpoint to wait on.
            // This is the latest syncpoint that is not equal to the current sync.

            long highestDiff = long.MinValue;

            for (int i = 0; i < rangeCount; i++)
            {
                BufferModifiedRange overlap = _backgroundOverlaps[i];

                long diff = (long)(overlap.SyncNumber - currentSync);

                if (diff < 0 && diff > highestDiff)
                {
                    highestDiff = diff;
                }
            }

            if (highestDiff == long.MinValue)
            {
                return;
            }

            // Wait for the syncpoint.
            _context.Renderer.WaitSync(currentSync + (ulong)highestDiff);

            // Flush and remove all regions with the older syncpoint.
            lock (_lock)
            {
                for (int i = 0; i < rangeCount; i++)
                {
                    BufferModifiedRange overlap = _backgroundOverlaps[i];

                    long diff = (long)(overlap.SyncNumber - currentSync);

                    if (diff <= highestDiff)
                    {
                        ulong clampAddress = Math.Max(address, overlap.Address);
                        ulong clampEnd = Math.Min(endAddress, overlap.EndAddress);

                        ClearPart(overlap, clampAddress, clampEnd);

                        rangeAction(clampAddress, clampEnd - clampAddress);
                    }
                }
            }
        }

        /// <summary>
        /// Inherit ranges from another modified range list.
        /// </summary>
        /// <param name="ranges">The range list to inherit from</param>
        /// <param name="rangeAction">The action to call for each modified range</param>
        public void InheritRanges(BufferModifiedRangeList ranges, Action<ulong, ulong> rangeAction)
        {
            BufferModifiedRange[] inheritRanges;

            lock (ranges._lock)
            {
                inheritRanges = ranges.ToArray();
            }

            lock (_lock)
            {
                foreach (BufferModifiedRange range in inheritRanges)
                {
                    Add(range);
                }
            }

            ulong currentSync = _context.SyncNumber;
            foreach (BufferModifiedRange range in inheritRanges)
            {
                if (range.SyncNumber != currentSync)
                {
                    rangeAction(range.Address, range.Size);
                }
            }
        }

        private void ClearPart(BufferModifiedRange overlap, ulong address, ulong endAddress)
        {
            Remove(overlap);

            // If the overlap extends outside of the clear range, make sure those parts still exist.

            if (overlap.Address < address)
            {
                Add(new BufferModifiedRange(overlap.Address, address - overlap.Address, overlap.SyncNumber));
            }

            if (overlap.EndAddress > endAddress)
            {
                Add(new BufferModifiedRange(endAddress, overlap.EndAddress - endAddress, overlap.SyncNumber));
            }
        }

        /// <summary>
        /// Clear modified ranges within the specified area.
        /// </summary>
        /// <param name="address">Start address to clear</param>
        /// <param name="size">Size to clear</param>
        public void Clear(ulong address, ulong size)
        {
            lock (_lock)
            {
                // This function can be called from any thread, so it cannot use the arrays for background or foreground.
                BufferModifiedRange[] toClear = new BufferModifiedRange[1];

                int rangeCount = FindOverlapsNonOverlapping(address, size, ref toClear);

                ulong endAddress = address + size;

                for (int i = 0; i < rangeCount; i++)
                {
                    BufferModifiedRange overlap = toClear[i];

                    ClearPart(overlap, address, endAddress);
                }
            }
        }

        /// <summary>
        /// Clear all modified ranges.
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                Items.Clear();
            }
        }
    }
}
