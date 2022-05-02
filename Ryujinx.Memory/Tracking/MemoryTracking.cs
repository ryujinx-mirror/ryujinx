using Ryujinx.Common.Pools;
using Ryujinx.Memory.Range;
using System;
using System.Collections.Generic;

namespace Ryujinx.Memory.Tracking
{
    /// <summary>
    /// Manages memory tracking for a given virutal/physical memory block.
    /// </summary>
    public class MemoryTracking
    {
        private readonly IVirtualMemoryManager _memoryManager;
        private readonly InvalidAccessHandler _invalidAccessHandler;

        // Only use these from within the lock.
        private readonly NonOverlappingRangeList<VirtualRegion> _virtualRegions;

        private readonly int _pageSize;

        /// <summary>
        /// This lock must be obtained when traversing or updating the region-handle hierarchy.
        /// It is not required when reading dirty flags.
        /// </summary>
        internal object TrackingLock = new object();

        /// <summary>
        /// Create a new tracking structure for the given "physical" memory block,
        /// with a given "virtual" memory manager that will provide mappings and virtual memory protection.
        /// </summary>
        /// <param name="memoryManager">Virtual memory manager</param>
        /// <param name="block">Physical memory block</param>
        /// <param name="pageSize">Page size of the virtual memory space</param>
        public MemoryTracking(IVirtualMemoryManager memoryManager, int pageSize, InvalidAccessHandler invalidAccessHandler = null)
        {
            _memoryManager = memoryManager;
            _pageSize = pageSize;
            _invalidAccessHandler = invalidAccessHandler;

            _virtualRegions = new NonOverlappingRangeList<VirtualRegion>();
        }

        private (ulong address, ulong size) PageAlign(ulong address, ulong size)
        {
            ulong pageMask = (ulong)_pageSize - 1;
            ulong rA = address & ~pageMask;
            ulong rS = ((address + size + pageMask) & ~pageMask) - rA;
            return (rA, rS);
        }

        /// <summary>
        /// Indicate that a virtual region has been mapped, and which physical region it has been mapped to.
        /// Should be called after the mapping is complete.
        /// </summary>
        /// <param name="va">Virtual memory address</param>
        /// <param name="size">Size to be mapped</param>
        public void Map(ulong va, ulong size)
        {
            // A mapping may mean we need to re-evaluate each VirtualRegion's affected area.
            // Find all handles that overlap with the range, we need to recalculate their physical regions

            lock (TrackingLock)
            {
                ref var overlaps = ref ThreadStaticArray<VirtualRegion>.Get();

                int count = _virtualRegions.FindOverlapsNonOverlapping(va, size, ref overlaps);

                for (int i = 0; i < count; i++)
                {
                    VirtualRegion region = overlaps[i];

                    // If the region has been fully remapped, signal that it has been mapped again.
                    bool remapped = _memoryManager.IsRangeMapped(region.Address, region.Size);
                    if (remapped)
                    {
                        region.SignalMappingChanged(true);
                    }

                    region.UpdateProtection();
                }
            }
        }

        /// <summary>
        /// Indicate that a virtual region has been unmapped.
        /// Should be called before the unmapping is complete.
        /// </summary>
        /// <param name="va">Virtual memory address</param>
        /// <param name="size">Size to be unmapped</param>
        public void Unmap(ulong va, ulong size)
        {
            // An unmapping may mean we need to re-evaluate each VirtualRegion's affected area.
            // Find all handles that overlap with the range, we need to notify them that the region was unmapped.

            lock (TrackingLock)
            {
                ref var overlaps = ref ThreadStaticArray<VirtualRegion>.Get();

                int count = _virtualRegions.FindOverlapsNonOverlapping(va, size, ref overlaps);

                for (int i = 0; i < count; i++)
                {
                    VirtualRegion region = overlaps[i];

                    region.SignalMappingChanged(false);
                }
            }
        }

        /// <summary>
        /// Get a list of virtual regions that a handle covers.
        /// </summary>
        /// <param name="va">Starting virtual memory address of the handle</param>
        /// <param name="size">Size of the handle's memory region</param>
        /// <returns>A list of virtual regions within the given range</returns>
        internal List<VirtualRegion> GetVirtualRegionsForHandle(ulong va, ulong size)
        {
            List<VirtualRegion> result = new List<VirtualRegion>();
            _virtualRegions.GetOrAddRegions(result, va, size, (va, size) => new VirtualRegion(this, va, size));

            return result;
        }

        /// <summary>
        /// Remove a virtual region from the range list. This assumes that the lock has been acquired.
        /// </summary>
        /// <param name="region">Region to remove</param>
        internal void RemoveVirtual(VirtualRegion region)
        {
            _virtualRegions.Remove(region);
        }

        /// <summary>
        /// Obtains a memory tracking handle for the given virtual region, with a specified granularity. This should be disposed when finished with.
        /// </summary>
        /// <param name="address">CPU virtual address of the region</param>
        /// <param name="size">Size of the region</param>
        /// <param name="handles">Handles to inherit state from or reuse. When none are present, provide null</param>
        /// <param name="granularity">Desired granularity of write tracking</param>
        /// <returns>The memory tracking handle</returns>
        public MultiRegionHandle BeginGranularTracking(ulong address, ulong size, IEnumerable<IRegionHandle> handles, ulong granularity)
        {
            (address, size) = PageAlign(address, size);

            return new MultiRegionHandle(this, address, size, handles, granularity);
        }

        /// <summary>
        /// Obtains a smart memory tracking handle for the given virtual region, with a specified granularity. This should be disposed when finished with.
        /// </summary>
        /// <param name="address">CPU virtual address of the region</param>
        /// <param name="size">Size of the region</param>
        /// <param name="granularity">Desired granularity of write tracking</param>
        /// <returns>The memory tracking handle</returns>
        public SmartMultiRegionHandle BeginSmartGranularTracking(ulong address, ulong size, ulong granularity)
        {
            (address, size) = PageAlign(address, size);

            return new SmartMultiRegionHandle(this, address, size, granularity);
        }

        /// <summary>
        /// Obtains a memory tracking handle for the given virtual region. This should be disposed when finished with.
        /// </summary>
        /// <param name="address">CPU virtual address of the region</param>
        /// <param name="size">Size of the region</param>
        /// <returns>The memory tracking handle</returns>
        public RegionHandle BeginTracking(ulong address, ulong size)
        {
            (address, size) = PageAlign(address, size);

            lock (TrackingLock)
            {
                RegionHandle handle = new RegionHandle(this, address, size, _memoryManager.IsRangeMapped(address, size));

                return handle;
            }
        }

        /// <summary>
        /// Signal that a virtual memory event happened at the given location (one byte).
        /// </summary>
        /// <param name="address">Virtual address accessed</param>
        /// <param name="write">Whether the address was written to or read</param>
        /// <returns>True if the event triggered any tracking regions, false otherwise</returns>
        public bool VirtualMemoryEventTracking(ulong address, bool write)
        {
            return VirtualMemoryEvent(address, 1, write);
        }

        /// <summary>
        /// Signal that a virtual memory event happened at the given location.
        /// This is similar VirtualMemoryEvent, but on Windows, it might also return true after a partial unmap.
        /// This should only be called from the exception handler.
        /// </summary>
        /// <param name="address">Virtual address accessed</param>
        /// <param name="size">Size of the region affected in bytes</param>
        /// <param name="write">Whether the region was written to or read</param>
        /// <param name="precise">True if the access is precise, false otherwise</param>
        /// <returns>True if the event triggered any tracking regions, false otherwise</returns>
        public bool VirtualMemoryEventEh(ulong address, ulong size, bool write, bool precise = false)
        {
            // Windows has a limitation, it can't do partial unmaps.
            // For this reason, we need to unmap the whole range and then remap the sub-ranges.
            // When this happens, we might have caused a undesirable access violation from the time that the range was unmapped.
            // In this case, try again as the memory might be mapped now.
            if (OperatingSystem.IsWindows() && MemoryManagementWindows.RetryFromAccessViolation())
            {
                return true;
            }

            return VirtualMemoryEvent(address, size, write, precise);
        }

        /// <summary>
        /// Signal that a virtual memory event happened at the given location.
        /// This can be flagged as a precise event, which will avoid reprotection and call special handlers if possible.
        /// A precise event has an exact address and size, rather than triggering on page granularity.
        /// </summary>
        /// <param name="address">Virtual address accessed</param>
        /// <param name="size">Size of the region affected in bytes</param>
        /// <param name="write">Whether the region was written to or read</param>
        /// <param name="precise">True if the access is precise, false otherwise</param>
        /// <returns>True if the event triggered any tracking regions, false otherwise</returns>
        public bool VirtualMemoryEvent(ulong address, ulong size, bool write, bool precise = false)
        {
            // Look up the virtual region using the region list.
            // Signal up the chain to relevant handles.

            lock (TrackingLock)
            {
                ref var overlaps = ref ThreadStaticArray<VirtualRegion>.Get();

                int count = _virtualRegions.FindOverlapsNonOverlapping(address, size, ref overlaps);

                if (count == 0 && !precise)
                {
                    if (!_memoryManager.IsMapped(address))
                    {
                        _invalidAccessHandler?.Invoke(address);

                        // We can't continue - it's impossible to remove protection from the page.
                        // Even if the access handler wants us to continue, we wouldn't be able to.
                        throw new InvalidMemoryRegionException();
                    }

                    _memoryManager.TrackingReprotect(address & ~(ulong)(_pageSize - 1), (ulong)_pageSize, MemoryPermission.ReadAndWrite);
                    return false; // We can't handle this - it's probably a real invalid access.
                }

                for (int i = 0; i < count; i++)
                {
                    VirtualRegion region = overlaps[i];

                    if (precise)
                    {
                        region.SignalPrecise(address, size, write);
                    }
                    else
                    {
                        region.Signal(address, size, write);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Reprotect a given virtual region. The virtual memory manager will handle this.
        /// </summary>
        /// <param name="region">Region to reprotect</param>
        /// <param name="permission">Memory permission to protect with</param>
        internal void ProtectVirtualRegion(VirtualRegion region, MemoryPermission permission)
        {
            _memoryManager.TrackingReprotect(region.Address, region.Size, permission);
        }

        /// <summary>
        /// Returns the number of virtual regions currently being tracked.
        /// Useful for tests and metrics.
        /// </summary>
        /// <returns>The number of virtual regions</returns>
        public int GetRegionCount()
        {
            lock (TrackingLock)
            {
                return _virtualRegions.Count;
            }
        }
    }
}
