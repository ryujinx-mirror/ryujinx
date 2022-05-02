using ARMeilleure.Memory;
using Ryujinx.Cpu.Tracking;
using Ryujinx.Memory;
using Ryujinx.Memory.Range;
using Ryujinx.Memory.Tracking;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Ryujinx.Cpu
{
    /// <summary>
    /// Represents a CPU memory manager which maps guest virtual memory directly onto a host virtual region.
    /// </summary>
    public class MemoryManagerHostMapped : MemoryManagerBase, IMemoryManager, IVirtualMemoryManagerTracked, IWritableBlock
    {
        public const int PageBits = 12;
        public const int PageSize = 1 << PageBits;
        public const int PageMask = PageSize - 1;

        public const int PageToPteShift = 5; // 32 pages (2 bits each) in one ulong page table entry.
        public const ulong BlockMappedMask = 0x5555555555555555; // First bit of each table entry set.

        private enum HostMappedPtBits : ulong
        {
            Unmapped = 0,
            Mapped,
            WriteTracked,
            ReadWriteTracked,

            MappedReplicated = 0x5555555555555555,
            WriteTrackedReplicated = 0xaaaaaaaaaaaaaaaa,
            ReadWriteTrackedReplicated = ulong.MaxValue
        }

        private readonly InvalidAccessHandler _invalidAccessHandler;
        private readonly bool _unsafeMode;

        private readonly MemoryBlock _addressSpace;
        private readonly MemoryBlock _addressSpaceMirror;
        private readonly ulong _addressSpaceSize;

        private readonly MemoryBlock _backingMemory;
        private readonly PageTable<ulong> _pageTable;

        private readonly MemoryEhMeilleure _memoryEh;

        private readonly ulong[] _pageBitmap;

        public int AddressSpaceBits { get; }

        public IntPtr PageTablePointer => _addressSpace.Pointer;

        public MemoryManagerType Type => _unsafeMode ? MemoryManagerType.HostMappedUnsafe : MemoryManagerType.HostMapped;

        public MemoryTracking Tracking { get; }

        public event Action<ulong, ulong> UnmapEvent;

        /// <summary>
        /// Creates a new instance of the host mapped memory manager.
        /// </summary>
        /// <param name="backingMemory">Physical backing memory where virtual memory will be mapped to</param>
        /// <param name="addressSpaceSize">Size of the address space</param>
        /// <param name="unsafeMode">True if unmanaged access should not be masked (unsafe), false otherwise.</param>
        /// <param name="invalidAccessHandler">Optional function to handle invalid memory accesses</param>
        public MemoryManagerHostMapped(MemoryBlock backingMemory, ulong addressSpaceSize, bool unsafeMode, InvalidAccessHandler invalidAccessHandler = null)
        {
            _backingMemory = backingMemory;
            _pageTable = new PageTable<ulong>();
            _invalidAccessHandler = invalidAccessHandler;
            _unsafeMode = unsafeMode;
            _addressSpaceSize = addressSpaceSize;

            ulong asSize = PageSize;
            int asBits = PageBits;

            while (asSize < addressSpaceSize)
            {
                asSize <<= 1;
                asBits++;
            }

            AddressSpaceBits = asBits;

            _pageBitmap = new ulong[1 << (AddressSpaceBits - (PageBits + PageToPteShift))];

            MemoryAllocationFlags asFlags = MemoryAllocationFlags.Reserve | MemoryAllocationFlags.ViewCompatible;

            _addressSpace = new MemoryBlock(asSize, asFlags);
            _addressSpaceMirror = new MemoryBlock(asSize, asFlags | MemoryAllocationFlags.ForceWindows4KBViewMapping);

            Tracking = new MemoryTracking(this, PageSize, invalidAccessHandler);
            _memoryEh = new MemoryEhMeilleure(_addressSpace, Tracking);
        }

        /// <summary>
        /// Checks if the virtual address is part of the addressable space.
        /// </summary>
        /// <param name="va">Virtual address</param>
        /// <returns>True if the virtual address is part of the addressable space</returns>
        private bool ValidateAddress(ulong va)
        {
            return va < _addressSpaceSize;
        }

        /// <summary>
        /// Checks if the combination of virtual address and size is part of the addressable space.
        /// </summary>
        /// <param name="va">Virtual address of the range</param>
        /// <param name="size">Size of the range in bytes</param>
        /// <returns>True if the combination of virtual address and size is part of the addressable space</returns>
        private bool ValidateAddressAndSize(ulong va, ulong size)
        {
            ulong endVa = va + size;
            return endVa >= va && endVa >= size && endVa <= _addressSpaceSize;
        }

        /// <summary>
        /// Ensures the combination of virtual address and size is part of the addressable space.
        /// </summary>
        /// <param name="va">Virtual address of the range</param>
        /// <param name="size">Size of the range in bytes</param>
        /// <exception cref="InvalidMemoryRegionException">Throw when the memory region specified outside the addressable space</exception>
        private void AssertValidAddressAndSize(ulong va, ulong size)
        {
            if (!ValidateAddressAndSize(va, size))
            {
                throw new InvalidMemoryRegionException($"va=0x{va:X16}, size=0x{size:X16}");
            }
        }

        /// <summary>
        /// Ensures the combination of virtual address and size is part of the addressable space and fully mapped.
        /// </summary>
        /// <param name="va">Virtual address of the range</param>
        /// <param name="size">Size of the range in bytes</param>
        private void AssertMapped(ulong va, ulong size)
        {
            if (!ValidateAddressAndSize(va, size) || !IsRangeMappedImpl(va, size))
            {
                throw new InvalidMemoryRegionException($"Not mapped: va=0x{va:X16}, size=0x{size:X16}");
            }
        }

        /// <inheritdoc/>
        public void Map(ulong va, ulong pa, ulong size)
        {
            AssertValidAddressAndSize(va, size);

            _addressSpace.MapView(_backingMemory, pa, va, size);
            _addressSpaceMirror.MapView(_backingMemory, pa, va, size);
            AddMapping(va, size);
            PtMap(va, pa, size);

            Tracking.Map(va, size);
        }

        /// <inheritdoc/>
        public void Unmap(ulong va, ulong size)
        {
            AssertValidAddressAndSize(va, size);

            UnmapEvent?.Invoke(va, size);
            Tracking.Unmap(va, size);

            RemoveMapping(va, size);
            PtUnmap(va, size);
            _addressSpace.UnmapView(_backingMemory, va, size);
            _addressSpaceMirror.UnmapView(_backingMemory, va, size);
        }

        private void PtMap(ulong va, ulong pa, ulong size)
        {
            while (size != 0)
            {
                _pageTable.Map(va, pa);

                va += PageSize;
                pa += PageSize;
                size -= PageSize;
            }
        }

        private void PtUnmap(ulong va, ulong size)
        {
            while (size != 0)
            {
                _pageTable.Unmap(va);

                va += PageSize;
                size -= PageSize;
            }
        }

        /// <inheritdoc/>
        public T Read<T>(ulong va) where T : unmanaged
        {
            try
            {
                AssertMapped(va, (ulong)Unsafe.SizeOf<T>());

                return _addressSpaceMirror.Read<T>(va);
            }
            catch (InvalidMemoryRegionException)
            {
                if (_invalidAccessHandler == null || !_invalidAccessHandler(va))
                {
                    throw;
                }

                return default;
            }
        }

        /// <inheritdoc/>
        public T ReadTracked<T>(ulong va) where T : unmanaged
        {
            try
            {
                SignalMemoryTracking(va, (ulong)Unsafe.SizeOf<T>(), false);

                return Read<T>(va);
            }
            catch (InvalidMemoryRegionException)
            {
                if (_invalidAccessHandler == null || !_invalidAccessHandler(va))
                {
                    throw;
                }

                return default;
            }
        }

        /// <inheritdoc/>
        public void Read(ulong va, Span<byte> data)
        {
            try
            {
                AssertMapped(va, (ulong)data.Length);

                _addressSpaceMirror.Read(va, data);
            }
            catch (InvalidMemoryRegionException)
            {
                if (_invalidAccessHandler == null || !_invalidAccessHandler(va))
                {
                    throw;
                }
            }
        }


        /// <inheritdoc/>
        public void Write<T>(ulong va, T value) where T : unmanaged
        {
            try
            {
                SignalMemoryTracking(va, (ulong)Unsafe.SizeOf<T>(), write: true);

                _addressSpaceMirror.Write(va, value);
            }
            catch (InvalidMemoryRegionException)
            {
                if (_invalidAccessHandler == null || !_invalidAccessHandler(va))
                {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public void Write(ulong va, ReadOnlySpan<byte> data)
        {
            try {
                SignalMemoryTracking(va, (ulong)data.Length, write: true);

                _addressSpaceMirror.Write(va, data);
            }
            catch (InvalidMemoryRegionException)
            {
                if (_invalidAccessHandler == null || !_invalidAccessHandler(va))
                {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public void WriteUntracked(ulong va, ReadOnlySpan<byte> data)
        {
            try
            {
                AssertMapped(va, (ulong)data.Length);

                _addressSpaceMirror.Write(va, data);
            }
            catch (InvalidMemoryRegionException)
            {
                if (_invalidAccessHandler == null || !_invalidAccessHandler(va))
                {
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public ReadOnlySpan<byte> GetSpan(ulong va, int size, bool tracked = false)
        {
            if (tracked)
            {
                SignalMemoryTracking(va, (ulong)size, write: false);
            }
            else
            {
                AssertMapped(va, (ulong)size);
            }

            return _addressSpaceMirror.GetSpan(va, size);
        }

        /// <inheritdoc/>
        public WritableRegion GetWritableRegion(ulong va, int size, bool tracked = false)
        {
            if (tracked)
            {
                SignalMemoryTracking(va, (ulong)size, true);
            }
            else
            {
                AssertMapped(va, (ulong)size);
            }

            return _addressSpaceMirror.GetWritableRegion(va, size);
        }

        /// <inheritdoc/>
        public ref T GetRef<T>(ulong va) where T : unmanaged
        {
            SignalMemoryTracking(va, (ulong)Unsafe.SizeOf<T>(), true);

            return ref _addressSpaceMirror.GetRef<T>(va);
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMapped(ulong va)
        {
            return ValidateAddress(va) && IsMappedImpl(va);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsMappedImpl(ulong va)
        {
            ulong page = va >> PageBits;

            int bit = (int)((page & 31) << 1);

            int pageIndex = (int)(page >> PageToPteShift);
            ref ulong pageRef = ref _pageBitmap[pageIndex];

            ulong pte = Volatile.Read(ref pageRef);

            return ((pte >> bit) & 3) != 0;
        }

        /// <inheritdoc/>
        public bool IsRangeMapped(ulong va, ulong size)
        {
            AssertValidAddressAndSize(va, size);

            return IsRangeMappedImpl(va, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetPageBlockRange(ulong pageStart, ulong pageEnd, out ulong startMask, out ulong endMask, out int pageIndex, out int pageEndIndex)
        {
            startMask = ulong.MaxValue << ((int)(pageStart & 31) << 1);
            endMask = ulong.MaxValue >> (64 - ((int)(pageEnd & 31) << 1));

            pageIndex = (int)(pageStart >> PageToPteShift);
            pageEndIndex = (int)((pageEnd - 1) >> PageToPteShift);
        }

        private bool IsRangeMappedImpl(ulong va, ulong size)
        {
            int pages = GetPagesCount(va, size, out _);

            if (pages == 1)
            {
                return IsMappedImpl(va);
            }

            ulong pageStart = va >> PageBits;
            ulong pageEnd = pageStart + (ulong)pages;

            GetPageBlockRange(pageStart, pageEnd, out ulong startMask, out ulong endMask, out int pageIndex, out int pageEndIndex);

            // Check if either bit in each 2 bit page entry is set.
            // OR the block with itself shifted down by 1, and check the first bit of each entry.

            ulong mask = BlockMappedMask & startMask;

            while (pageIndex <= pageEndIndex)
            {
                if (pageIndex == pageEndIndex)
                {
                    mask &= endMask;
                }

                ref ulong pageRef = ref _pageBitmap[pageIndex++];
                ulong pte = Volatile.Read(ref pageRef);

                pte |= pte >> 1;
                if ((pte & mask) != mask)
                {
                    return false;
                }

                mask = BlockMappedMask;
            }

            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<MemoryRange> GetPhysicalRegions(ulong va, ulong size)
        {
            int pages = GetPagesCount(va, (uint)size, out va);

            var regions = new List<MemoryRange>();

            ulong regionStart = GetPhysicalAddressChecked(va);
            ulong regionSize = PageSize;

            for (int page = 0; page < pages - 1; page++)
            {
                if (!ValidateAddress(va + PageSize))
                {
                    return null;
                }

                ulong newPa = GetPhysicalAddressChecked(va + PageSize);

                if (GetPhysicalAddressChecked(va) + PageSize != newPa)
                {
                    regions.Add(new MemoryRange(regionStart, regionSize));
                    regionStart = newPa;
                    regionSize = 0;
                }

                va += PageSize;
                regionSize += PageSize;
            }

            regions.Add(new MemoryRange(regionStart, regionSize));

            return regions;
        }

        private ulong GetPhysicalAddressChecked(ulong va)
        {
            if (!IsMapped(va))
            {
                ThrowInvalidMemoryRegionException($"Not mapped: va=0x{va:X16}");
            }

            return GetPhysicalAddressInternal(va);
        }

        private ulong GetPhysicalAddressInternal(ulong va)
        {
            return _pageTable.Read(va) + (va & PageMask);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This function also validates that the given range is both valid and mapped, and will throw if it is not.
        /// </remarks>
        public void SignalMemoryTracking(ulong va, ulong size, bool write, bool precise = false)
        {
            AssertValidAddressAndSize(va, size);

            if (precise)
            {
                Tracking.VirtualMemoryEvent(va, size, write, precise: true);
                return;
            }

            // Software table, used for managed memory tracking.

            int pages = GetPagesCount(va, size, out _);
            ulong pageStart = va >> PageBits;

            if (pages == 1)
            {
                ulong tag = (ulong)(write ? HostMappedPtBits.WriteTracked : HostMappedPtBits.ReadWriteTracked);

                int bit = (int)((pageStart & 31) << 1);

                int pageIndex = (int)(pageStart >> PageToPteShift);
                ref ulong pageRef = ref _pageBitmap[pageIndex];

                ulong pte = Volatile.Read(ref pageRef);
                ulong state = ((pte >> bit) & 3);

                if (state >= tag)
                {
                    Tracking.VirtualMemoryEvent(va, size, write);
                    return;
                }
                else if (state == 0)
                {
                    ThrowInvalidMemoryRegionException($"Not mapped: va=0x{va:X16}, size=0x{size:X16}");
                }
            }
            else
            {
                ulong pageEnd = pageStart + (ulong)pages;

                GetPageBlockRange(pageStart, pageEnd, out ulong startMask, out ulong endMask, out int pageIndex, out int pageEndIndex);

                ulong mask = startMask;

                ulong anyTrackingTag = (ulong)HostMappedPtBits.WriteTrackedReplicated;

                while (pageIndex <= pageEndIndex)
                {
                    if (pageIndex == pageEndIndex)
                    {
                        mask &= endMask;
                    }

                    ref ulong pageRef = ref _pageBitmap[pageIndex++];

                    ulong pte = Volatile.Read(ref pageRef);
                    ulong mappedMask = mask & BlockMappedMask;

                    ulong mappedPte = pte | (pte >> 1);
                    if ((mappedPte & mappedMask) != mappedMask)
                    {
                        ThrowInvalidMemoryRegionException($"Not mapped: va=0x{va:X16}, size=0x{size:X16}");
                    }

                    pte &= mask;
                    if ((pte & anyTrackingTag) != 0) // Search for any tracking.
                    {
                        // Writes trigger any tracking.
                        // Only trigger tracking from reads if both bits are set on any page.
                        if (write || (pte & (pte >> 1) & BlockMappedMask) != 0)
                        {
                            Tracking.VirtualMemoryEvent(va, size, write);
                            break;
                        }
                    }

                    mask = ulong.MaxValue;
                }
            }
        }

        /// <summary>
        /// Computes the number of pages in a virtual address range.
        /// </summary>
        /// <param name="va">Virtual address of the range</param>
        /// <param name="size">Size of the range</param>
        /// <param name="startVa">The virtual address of the beginning of the first page</param>
        /// <remarks>This function does not differentiate between allocated and unallocated pages.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetPagesCount(ulong va, ulong size, out ulong startVa)
        {
            // WARNING: Always check if ulong does not overflow during the operations.
            startVa = va & ~(ulong)PageMask;
            ulong vaSpan = (va - startVa + size + PageMask) & ~(ulong)PageMask;

            return (int)(vaSpan / PageSize);
        }

        /// <inheritdoc/>
        public void TrackingReprotect(ulong va, ulong size, MemoryPermission protection)
        {
            // Protection is inverted on software pages, since the default value is 0.
            protection = (~protection) & MemoryPermission.ReadAndWrite;

            int pages = GetPagesCount(va, size, out va);
            ulong pageStart = va >> PageBits;

            if (pages == 1)
            {
                ulong protTag = protection switch
                {
                    MemoryPermission.None => (ulong)HostMappedPtBits.Mapped,
                    MemoryPermission.Write => (ulong)HostMappedPtBits.WriteTracked,
                    _ => (ulong)HostMappedPtBits.ReadWriteTracked,
                };

                int bit = (int)((pageStart & 31) << 1);

                ulong tagMask = 3UL << bit;
                ulong invTagMask = ~tagMask;

                ulong tag = protTag << bit;

                int pageIndex = (int)(pageStart >> PageToPteShift);
                ref ulong pageRef = ref _pageBitmap[pageIndex];

                ulong pte;

                do
                {
                    pte = Volatile.Read(ref pageRef);
                }
                while ((pte & tagMask) != 0 && Interlocked.CompareExchange(ref pageRef, (pte & invTagMask) | tag, pte) != pte);
            }
            else
            {
                ulong pageEnd = pageStart + (ulong)pages;

                GetPageBlockRange(pageStart, pageEnd, out ulong startMask, out ulong endMask, out int pageIndex, out int pageEndIndex);

                ulong mask = startMask;

                ulong protTag = protection switch
                {
                    MemoryPermission.None => (ulong)HostMappedPtBits.MappedReplicated,
                    MemoryPermission.Write => (ulong)HostMappedPtBits.WriteTrackedReplicated,
                    _ => (ulong)HostMappedPtBits.ReadWriteTrackedReplicated,
                };

                while (pageIndex <= pageEndIndex)
                {
                    if (pageIndex == pageEndIndex)
                    {
                        mask &= endMask;
                    }

                    ref ulong pageRef = ref _pageBitmap[pageIndex++];

                    ulong pte;
                    ulong mappedMask;

                    // Change the protection of all 2 bit entries that are mapped.
                    do
                    {
                        pte = Volatile.Read(ref pageRef);

                        mappedMask = pte | (pte >> 1);
                        mappedMask |= (mappedMask & BlockMappedMask) << 1;
                        mappedMask &= mask; // Only update mapped pages within the given range.
                    }
                    while (Interlocked.CompareExchange(ref pageRef, (pte & (~mappedMask)) | (protTag & mappedMask), pte) != pte);

                    mask = ulong.MaxValue;
                }
            }

            protection = protection switch
            {
                MemoryPermission.None => MemoryPermission.ReadAndWrite,
                MemoryPermission.Write => MemoryPermission.Read,
                _ => MemoryPermission.None
            };

            _addressSpace.Reprotect(va, size, protection, false);
        }

        /// <inheritdoc/>
        public CpuRegionHandle BeginTracking(ulong address, ulong size)
        {
            return new CpuRegionHandle(Tracking.BeginTracking(address, size));
        }

        /// <inheritdoc/>
        public CpuMultiRegionHandle BeginGranularTracking(ulong address, ulong size, IEnumerable<IRegionHandle> handles, ulong granularity)
        {
            return new CpuMultiRegionHandle(Tracking.BeginGranularTracking(address, size, handles, granularity));
        }

        /// <inheritdoc/>
        public CpuSmartMultiRegionHandle BeginSmartGranularTracking(ulong address, ulong size, ulong granularity)
        {
            return new CpuSmartMultiRegionHandle(Tracking.BeginSmartGranularTracking(address, size, granularity));
        }

        /// <summary>
        /// Adds the given address mapping to the page table.
        /// </summary>
        /// <param name="va">Virtual memory address</param>
        /// <param name="size">Size to be mapped</param>
        private void AddMapping(ulong va, ulong size)
        {
            int pages = GetPagesCount(va, size, out _);
            ulong pageStart = va >> PageBits;
            ulong pageEnd = pageStart + (ulong)pages;

            GetPageBlockRange(pageStart, pageEnd, out ulong startMask, out ulong endMask, out int pageIndex, out int pageEndIndex);

            ulong mask = startMask;

            while (pageIndex <= pageEndIndex)
            {
                if (pageIndex == pageEndIndex)
                {
                    mask &= endMask;
                }

                ref ulong pageRef = ref _pageBitmap[pageIndex++];

                ulong pte;
                ulong mappedMask;

                // Map all 2-bit entries that are unmapped.
                do
                {
                    pte = Volatile.Read(ref pageRef);

                    mappedMask = pte | (pte >> 1);
                    mappedMask |= (mappedMask & BlockMappedMask) << 1;
                    mappedMask |= ~mask; // Treat everything outside the range as mapped, thus unchanged.
                }
                while (Interlocked.CompareExchange(ref pageRef, (pte & mappedMask) | (BlockMappedMask & (~mappedMask)), pte) != pte);

                mask = ulong.MaxValue;
            }
        }

        /// <summary>
        /// Removes the given address mapping from the page table.
        /// </summary>
        /// <param name="va">Virtual memory address</param>
        /// <param name="size">Size to be unmapped</param>
        private void RemoveMapping(ulong va, ulong size)
        {
            int pages = GetPagesCount(va, size, out _);
            ulong pageStart = va >> PageBits;
            ulong pageEnd = pageStart + (ulong)pages;

            GetPageBlockRange(pageStart, pageEnd, out ulong startMask, out ulong endMask, out int pageIndex, out int pageEndIndex);

            startMask = ~startMask;
            endMask = ~endMask;

            ulong mask = startMask;

            while (pageIndex <= pageEndIndex)
            {
                if (pageIndex == pageEndIndex)
                {
                    mask |= endMask;
                }

                ref ulong pageRef = ref _pageBitmap[pageIndex++];
                ulong pte;

                do
                {
                    pte = Volatile.Read(ref pageRef);
                }
                while (Interlocked.CompareExchange(ref pageRef, pte & mask, pte) != pte);

                mask = 0;
            }
        }

        /// <summary>
        /// Disposes of resources used by the memory manager.
        /// </summary>
        protected override void Destroy()
        {
            _addressSpace.Dispose();
            _addressSpaceMirror.Dispose();
            _memoryEh.Dispose();
        }

        private static void ThrowInvalidMemoryRegionException(string message) => throw new InvalidMemoryRegionException(message);
    }
}
