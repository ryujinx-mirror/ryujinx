using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading;

namespace Ryujinx.Memory.WindowsShared
{
    /// <summary>
    /// Windows memory placeholder manager.
    /// </summary>
    [SupportedOSPlatform("windows")]
    class PlaceholderManager
    {
        private const ulong MinimumPageSize = 0x1000;

        [ThreadStatic]
        private static int _threadLocalPartialUnmapsCount;

        private readonly IntervalTree<ulong, ulong> _mappings;
        private readonly IntervalTree<ulong, MemoryPermission> _protections;
        private readonly ReaderWriterLock _partialUnmapLock;
        private int _partialUnmapsCount;

        /// <summary>
        /// Creates a new instance of the Windows memory placeholder manager.
        /// </summary>
        public PlaceholderManager()
        {
            _mappings = new IntervalTree<ulong, ulong>();
            _protections = new IntervalTree<ulong, MemoryPermission>();
            _partialUnmapLock = new ReaderWriterLock();
        }

        /// <summary>
        /// Reserves a range of the address space to be later mapped as shared memory views.
        /// </summary>
        /// <param name="address">Start address of the region to reserve</param>
        /// <param name="size">Size in bytes of the region to reserve</param>
        public void ReserveRange(ulong address, ulong size)
        {
            lock (_mappings)
            {
                _mappings.Add(address, address + size, ulong.MaxValue);
            }
        }

        /// <summary>
        /// Unreserves a range of memory that has been previously reserved with <see cref="ReserveRange"/>.
        /// </summary>
        /// <param name="address">Start address of the region to unreserve</param>
        /// <param name="size">Size in bytes of the region to unreserve</param>
        /// <exception cref="WindowsApiException">Thrown when the Windows API returns an error unreserving the memory</exception>
        public void UnreserveRange(ulong address, ulong size)
        {
            ulong endAddress = address + size;

            var overlaps = Array.Empty<IntervalTreeNode<ulong, ulong>>();
            int count;

            lock (_mappings)
            {
                count = _mappings.Get(address, endAddress, ref overlaps);

                for (int index = 0; index < count; index++)
                {
                    var overlap = overlaps[index];

                    if (IsMapped(overlap.Value))
                    {
                        if (!WindowsApi.UnmapViewOfFile2(WindowsApi.CurrentProcessHandle, (IntPtr)overlap.Start, 2))
                        {
                            throw new WindowsApiException("UnmapViewOfFile2");
                        }
                    }

                    _mappings.Remove(overlap);
                }
            }

            if (count > 1)
            {
                CheckFreeResult(WindowsApi.VirtualFree(
                    (IntPtr)address,
                    (IntPtr)size,
                    AllocationType.Release | AllocationType.CoalescePlaceholders));
            }

            RemoveProtection(address, size);
        }

        /// <summary>
        /// Maps a shared memory view on a previously reserved memory region.
        /// </summary>
        /// <param name="sharedMemory">Shared memory that will be the backing storage for the view</param>
        /// <param name="srcOffset">Offset in the shared memory to map</param>
        /// <param name="location">Address to map the view into</param>
        /// <param name="size">Size of the view in bytes</param>
        /// <param name="owner">Memory block that owns the mapping</param>
        public void MapView(IntPtr sharedMemory, ulong srcOffset, IntPtr location, IntPtr size, MemoryBlock owner)
        {
            _partialUnmapLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                UnmapViewInternal(sharedMemory, location, size, owner);
                MapViewInternal(sharedMemory, srcOffset, location, size);
            }
            finally
            {
                _partialUnmapLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Maps a shared memory view on a previously reserved memory region.
        /// </summary>
        /// <param name="sharedMemory">Shared memory that will be the backing storage for the view</param>
        /// <param name="srcOffset">Offset in the shared memory to map</param>
        /// <param name="location">Address to map the view into</param>
        /// <param name="size">Size of the view in bytes</param>
        /// <exception cref="WindowsApiException">Thrown when the Windows API returns an error mapping the memory</exception>
        private void MapViewInternal(IntPtr sharedMemory, ulong srcOffset, IntPtr location, IntPtr size)
        {
            SplitForMap((ulong)location, (ulong)size, srcOffset);

            var ptr = WindowsApi.MapViewOfFile3(
                sharedMemory,
                WindowsApi.CurrentProcessHandle,
                location,
                srcOffset,
                size,
                0x4000,
                MemoryProtection.ReadWrite,
                IntPtr.Zero,
                0);

            if (ptr == IntPtr.Zero)
            {
                throw new WindowsApiException("MapViewOfFile3");
            }
        }

        /// <summary>
        /// Splits a larger placeholder, slicing at the start and end address, for a new memory mapping.
        /// </summary>
        /// <param name="address">Address to split</param>
        /// <param name="size">Size of the new region</param>
        /// <param name="backingOffset">Offset in the shared memory that will be mapped</param>
        private void SplitForMap(ulong address, ulong size, ulong backingOffset)
        {
            ulong endAddress = address + size;

            var overlaps = Array.Empty<IntervalTreeNode<ulong, ulong>>();

            lock (_mappings)
            {
                int count = _mappings.Get(address, endAddress, ref overlaps);

                Debug.Assert(count == 1);
                Debug.Assert(!IsMapped(overlaps[0].Value));

                var overlap = overlaps[0];

                // Tree operations might modify the node start/end values, so save a copy before we modify the tree.
                ulong overlapStart = overlap.Start;
                ulong overlapEnd = overlap.End;
                ulong overlapValue = overlap.Value;

                _mappings.Remove(overlap);

                bool overlapStartsBefore = overlapStart < address;
                bool overlapEndsAfter = overlapEnd > endAddress;

                if (overlapStartsBefore && overlapEndsAfter)
                {
                    CheckFreeResult(WindowsApi.VirtualFree(
                        (IntPtr)address,
                        (IntPtr)size,
                        AllocationType.Release | AllocationType.PreservePlaceholder));

                    _mappings.Add(overlapStart, address, overlapValue);
                    _mappings.Add(endAddress, overlapEnd, AddBackingOffset(overlapValue, endAddress - overlapStart));
                }
                else if (overlapStartsBefore)
                {
                    ulong overlappedSize = overlapEnd - address;

                    CheckFreeResult(WindowsApi.VirtualFree(
                        (IntPtr)address,
                        (IntPtr)overlappedSize,
                        AllocationType.Release | AllocationType.PreservePlaceholder));

                    _mappings.Add(overlapStart, address, overlapValue);
                }
                else if (overlapEndsAfter)
                {
                    ulong overlappedSize = endAddress - overlapStart;

                    CheckFreeResult(WindowsApi.VirtualFree(
                        (IntPtr)overlapStart,
                        (IntPtr)overlappedSize,
                        AllocationType.Release | AllocationType.PreservePlaceholder));

                    _mappings.Add(endAddress, overlapEnd, AddBackingOffset(overlapValue, overlappedSize));
                }

                _mappings.Add(address, endAddress, backingOffset);
            }
        }

        /// <summary>
        /// Unmaps a view that has been previously mapped with <see cref="MapView"/>.
        /// </summary>
        /// <remarks>
        /// For "partial unmaps" (when not the entire mapped range is being unmapped), it might be
        /// necessary to unmap the whole range and then remap the sub-ranges that should remain mapped.
        /// </remarks>
        /// <param name="sharedMemory">Shared memory that the view being unmapped belongs to</param>
        /// <param name="location">Address to unmap</param>
        /// <param name="size">Size of the region to unmap in bytes</param>
        /// <param name="owner">Memory block that owns the mapping</param>
        public void UnmapView(IntPtr sharedMemory, IntPtr location, IntPtr size, MemoryBlock owner)
        {
            _partialUnmapLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                UnmapViewInternal(sharedMemory, location, size, owner);
            }
            finally
            {
                _partialUnmapLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Unmaps a view that has been previously mapped with <see cref="MapView"/>.
        /// </summary>
        /// <remarks>
        /// For "partial unmaps" (when not the entire mapped range is being unmapped), it might be
        /// necessary to unmap the whole range and then remap the sub-ranges that should remain mapped.
        /// </remarks>
        /// <param name="sharedMemory">Shared memory that the view being unmapped belongs to</param>
        /// <param name="location">Address to unmap</param>
        /// <param name="size">Size of the region to unmap in bytes</param>
        /// <param name="owner">Memory block that owns the mapping</param>
        /// <exception cref="WindowsApiException">Thrown when the Windows API returns an error unmapping or remapping the memory</exception>
        private void UnmapViewInternal(IntPtr sharedMemory, IntPtr location, IntPtr size, MemoryBlock owner)
        {
            ulong startAddress = (ulong)location;
            ulong unmapSize = (ulong)size;
            ulong endAddress = startAddress + unmapSize;

            var overlaps = Array.Empty<IntervalTreeNode<ulong, ulong>>();
            int count;

            lock (_mappings)
            {
                count = _mappings.Get(startAddress, endAddress, ref overlaps);
            }

            for (int index = 0; index < count; index++)
            {
                var overlap = overlaps[index];

                if (IsMapped(overlap.Value))
                {
                    if (!WindowsApi.UnmapViewOfFile2(WindowsApi.CurrentProcessHandle, (IntPtr)overlap.Start, 2))
                    {
                        throw new WindowsApiException("UnmapViewOfFile2");
                    }

                    // Tree operations might modify the node start/end values, so save a copy before we modify the tree.
                    ulong overlapStart = overlap.Start;
                    ulong overlapEnd = overlap.End;
                    ulong overlapValue = overlap.Value;

                    lock (_mappings)
                    {
                        _mappings.Remove(overlap);
                        _mappings.Add(overlapStart, overlapEnd, ulong.MaxValue);
                    }

                    bool overlapStartsBefore = overlapStart < startAddress;
                    bool overlapEndsAfter = overlapEnd > endAddress;

                    if (overlapStartsBefore || overlapEndsAfter)
                    {
                        // If the overlap extends beyond the region we are unmapping,
                        // then we need to re-map the regions that are supposed to remain mapped.
                        // This is necessary because Windows does not support partial view unmaps.
                        // That is, you can only fully unmap a view that was previously mapped, you can't just unmap a chunck of it.

                        LockCookie lockCookie = _partialUnmapLock.UpgradeToWriterLock(Timeout.Infinite);

                        _partialUnmapsCount++;

                        if (overlapStartsBefore)
                        {
                            ulong remapSize = startAddress - overlapStart;

                            MapViewInternal(sharedMemory, overlapValue, (IntPtr)overlapStart, (IntPtr)remapSize);
                            RestoreRangeProtection(overlapStart, remapSize);
                        }

                        if (overlapEndsAfter)
                        {
                            ulong overlappedSize = endAddress - overlapStart;
                            ulong remapBackingOffset = overlapValue + overlappedSize;
                            ulong remapAddress = overlapStart + overlappedSize;
                            ulong remapSize = overlapEnd - endAddress;

                            MapViewInternal(sharedMemory, remapBackingOffset, (IntPtr)remapAddress, (IntPtr)remapSize);
                            RestoreRangeProtection(remapAddress, remapSize);
                        }

                        _partialUnmapLock.DowngradeFromWriterLock(ref lockCookie);
                    }
                }
            }

            CoalesceForUnmap(startAddress, unmapSize, owner);
            RemoveProtection(startAddress, unmapSize);
        }

        /// <summary>
        /// Coalesces adjacent placeholders after unmap.
        /// </summary>
        /// <param name="address">Address of the region that was unmapped</param>
        /// <param name="size">Size of the region that was unmapped in bytes</param>
        /// <param name="owner">Memory block that owns the mapping</param>
        private void CoalesceForUnmap(ulong address, ulong size, MemoryBlock owner)
        {
            ulong endAddress = address + size;
            ulong blockAddress = (ulong)owner.Pointer;
            ulong blockEnd = blockAddress + owner.Size;
            var overlaps = Array.Empty<IntervalTreeNode<ulong, ulong>>();
            int unmappedCount = 0;

            lock (_mappings)
            {
                int count = _mappings.Get(
                    Math.Max(address - MinimumPageSize, blockAddress),
                    Math.Min(endAddress + MinimumPageSize, blockEnd), ref overlaps);

                if (count < 2)
                {
                    // Nothing to coalesce if we only have 1 or no overlaps.
                    return;
                }

                for (int index = 0; index < count; index++)
                {
                    var overlap = overlaps[index];

                    if (!IsMapped(overlap.Value))
                    {
                        if (address > overlap.Start)
                        {
                            address = overlap.Start;
                        }

                        if (endAddress < overlap.End)
                        {
                            endAddress = overlap.End;
                        }

                        _mappings.Remove(overlap);

                        unmappedCount++;
                    }
                }

                _mappings.Add(address, endAddress, ulong.MaxValue);
            }

            if (unmappedCount > 1)
            {
                size = endAddress - address;

                CheckFreeResult(WindowsApi.VirtualFree(
                    (IntPtr)address,
                    (IntPtr)size,
                    AllocationType.Release | AllocationType.CoalescePlaceholders));
            }
        }

        /// <summary>
        /// Reprotects a region of memory that has been mapped.
        /// </summary>
        /// <param name="address">Address of the region to reprotect</param>
        /// <param name="size">Size of the region to reprotect in bytes</param>
        /// <param name="permission">New permissions</param>
        /// <returns>True if the reprotection was successful, false otherwise</returns>
        public bool ReprotectView(IntPtr address, IntPtr size, MemoryPermission permission)
        {
            _partialUnmapLock.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return ReprotectViewInternal(address, size, permission, false);
            }
            finally
            {
                _partialUnmapLock.ReleaseReaderLock();
            }
        }

        /// <summary>
        /// Reprotects a region of memory that has been mapped.
        /// </summary>
        /// <param name="address">Address of the region to reprotect</param>
        /// <param name="size">Size of the region to reprotect in bytes</param>
        /// <param name="permission">New permissions</param>
        /// <param name="throwOnError">Throw an exception instead of returning an error if the operation fails</param>
        /// <returns>True if the reprotection was successful or if <paramref name="throwOnError"/> is true, false otherwise</returns>
        /// <exception cref="WindowsApiException">If <paramref name="throwOnError"/> is true, it is thrown when the Windows API returns an error reprotecting the memory</exception>
        private bool ReprotectViewInternal(IntPtr address, IntPtr size, MemoryPermission permission, bool throwOnError)
        {
            ulong reprotectAddress = (ulong)address;
            ulong reprotectSize = (ulong)size;
            ulong endAddress = reprotectAddress + reprotectSize;

            var overlaps = Array.Empty<IntervalTreeNode<ulong, ulong>>();
            int count;

            lock (_mappings)
            {
                count = _mappings.Get(reprotectAddress, endAddress, ref overlaps);
            }

            bool success = true;

            for (int index = 0; index < count; index++)
            {
                var overlap = overlaps[index];

                ulong mappedAddress = overlap.Start;
                ulong mappedSize = overlap.End - overlap.Start;

                if (mappedAddress < reprotectAddress)
                {
                    ulong delta = reprotectAddress - mappedAddress;
                    mappedAddress = reprotectAddress;
                    mappedSize -= delta;
                }

                ulong mappedEndAddress = mappedAddress + mappedSize;

                if (mappedEndAddress > endAddress)
                {
                    ulong delta = mappedEndAddress - endAddress;
                    mappedSize -= delta;
                }

                if (!WindowsApi.VirtualProtect((IntPtr)mappedAddress, (IntPtr)mappedSize, WindowsApi.GetProtection(permission), out _))
                {
                    if (throwOnError)
                    {
                        throw new WindowsApiException("VirtualProtect");
                    }

                    success = false;
                }

                // We only keep track of "non-standard" protections,
                // that is, everything that is not just RW (which is the default when views are mapped).
                if (permission == MemoryPermission.ReadAndWrite)
                {
                    RemoveProtection(mappedAddress, mappedSize);
                }
                else
                {
                    AddProtection(mappedAddress, mappedSize, permission);
                }
            }

            return success;
        }

        /// <summary>
        /// Checks the result of a VirtualFree operation, throwing if needed.
        /// </summary>
        /// <param name="success">Operation result</param>
        /// <exception cref="WindowsApiException">Thrown if <paramref name="success"/> is false</exception>
        private static void CheckFreeResult(bool success)
        {
            if (!success)
            {
                throw new WindowsApiException("VirtualFree");
            }
        }

        /// <summary>
        /// Adds an offset to a backing offset. This will do nothing if the backing offset is the special "unmapped" value.
        /// </summary>
        /// <param name="backingOffset">Backing offset</param>
        /// <param name="offset">Offset to be added</param>
        /// <returns>Added offset or just <paramref name="backingOffset"/> if the region is unmapped</returns>
        private static ulong AddBackingOffset(ulong backingOffset, ulong offset)
        {
            if (backingOffset == ulong.MaxValue)
            {
                return backingOffset;
            }

            return backingOffset + offset;
        }

        /// <summary>
        /// Checks if a region is unmapped.
        /// </summary>
        /// <param name="backingOffset">Backing offset to check</param>
        /// <returns>True if the backing offset is the special "unmapped" value, false otherwise</returns>
        private static bool IsMapped(ulong backingOffset)
        {
            return backingOffset != ulong.MaxValue;
        }

        /// <summary>
        /// Adds a protection to the list of protections.
        /// </summary>
        /// <param name="address">Address of the protected region</param>
        /// <param name="size">Size of the protected region in bytes</param>
        /// <param name="permission">Memory permissions of the region</param>
        private void AddProtection(ulong address, ulong size, MemoryPermission permission)
        {
            ulong endAddress = address + size;
            var overlaps = Array.Empty<IntervalTreeNode<ulong, MemoryPermission>>();
            int count;

            lock (_protections)
            {
                count = _protections.Get(address, endAddress, ref overlaps);

                if (count == 1 &&
                    overlaps[0].Start <= address &&
                    overlaps[0].End >= endAddress &&
                    overlaps[0].Value == permission)
                {
                    return;
                }

                ulong startAddress = address;

                for (int index = 0; index < count; index++)
                {
                    var protection = overlaps[index];

                    ulong protAddress = protection.Start;
                    ulong protEndAddress = protection.End;
                    MemoryPermission protPermission = protection.Value;

                    _protections.Remove(protection);

                    if (protection.Value == permission)
                    {
                        if (startAddress > protAddress)
                        {
                            startAddress = protAddress;
                        }

                        if (endAddress < protEndAddress)
                        {
                            endAddress = protEndAddress;
                        }
                    }
                    else
                    {
                        if (startAddress > protAddress)
                        {
                            _protections.Add(protAddress, startAddress, protPermission);
                        }

                        if (endAddress < protEndAddress)
                        {
                            _protections.Add(endAddress, protEndAddress, protPermission);
                        }
                    }
                }

                _protections.Add(startAddress, endAddress, permission);
            }
        }

        /// <summary>
        /// Removes protection from the list of protections.
        /// </summary>
        /// <param name="address">Address of the protected region</param>
        /// <param name="size">Size of the protected region in bytes</param>
        private void RemoveProtection(ulong address, ulong size)
        {
            ulong endAddress = address + size;
            var overlaps = Array.Empty<IntervalTreeNode<ulong, MemoryPermission>>();
            int count;

            lock (_protections)
            {
                count = _protections.Get(address, endAddress, ref overlaps);

                for (int index = 0; index < count; index++)
                {
                    var protection = overlaps[index];

                    ulong protAddress = protection.Start;
                    ulong protEndAddress = protection.End;
                    MemoryPermission protPermission = protection.Value;

                    _protections.Remove(protection);

                    if (address > protAddress)
                    {
                        _protections.Add(protAddress, address, protPermission);
                    }

                    if (endAddress < protEndAddress)
                    {
                        _protections.Add(endAddress, protEndAddress, protPermission);
                    }
                }
            }
        }

        /// <summary>
        /// Restores the protection of a given memory region that was remapped, using the protections list.
        /// </summary>
        /// <param name="address">Address of the remapped region</param>
        /// <param name="size">Size of the remapped region in bytes</param>
        private void RestoreRangeProtection(ulong address, ulong size)
        {
            ulong endAddress = address + size;
            var overlaps = Array.Empty<IntervalTreeNode<ulong, MemoryPermission>>();
            int count;

            lock (_protections)
            {
                count = _protections.Get(address, endAddress, ref overlaps);
            }

            ulong startAddress = address;

            for (int index = 0; index < count; index++)
            {
                var protection = overlaps[index];

                ulong protAddress = protection.Start;
                ulong protEndAddress = protection.End;

                if (protAddress < address)
                {
                    protAddress = address;
                }

                if (protEndAddress > endAddress)
                {
                    protEndAddress = endAddress;
                }

                ReprotectViewInternal((IntPtr)protAddress, (IntPtr)(protEndAddress - protAddress), protection.Value, true);
            }
        }

        /// <summary>
        /// Checks if an access violation handler should retry execution due to a fault caused by partial unmap.
        /// </summary>
        /// <remarks>
        /// Due to Windows limitations, <see cref="UnmapView"/> might need to unmap more memory than requested.
        /// The additional memory that was unmapped is later remapped, however this leaves a time gap where the
        /// memory might be accessed but is unmapped. Users of the API must compensate for that by catching the
        /// access violation and retrying if it happened between the unmap and remap operation.
        /// This method can be used to decide if retrying in such cases is necessary or not.
        /// </remarks>
        /// <returns>True if execution should be retried, false otherwise</returns>
        public bool RetryFromAccessViolation()
        {
            _partialUnmapLock.AcquireReaderLock(Timeout.Infinite);

            bool retry = _threadLocalPartialUnmapsCount != _partialUnmapsCount;
            if (retry)
            {
                _threadLocalPartialUnmapsCount = _partialUnmapsCount;
            }

            _partialUnmapLock.ReleaseReaderLock();

            return retry;
        }
    }
}