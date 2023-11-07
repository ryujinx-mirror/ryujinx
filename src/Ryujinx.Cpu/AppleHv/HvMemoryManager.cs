using ARMeilleure.Memory;
using Ryujinx.Memory;
using Ryujinx.Memory.Range;
using Ryujinx.Memory.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace Ryujinx.Cpu.AppleHv
{
    /// <summary>
    /// Represents a CPU memory manager which maps guest virtual memory directly onto the Hypervisor page table.
    /// </summary>
    [SupportedOSPlatform("macos")]
    public class HvMemoryManager : MemoryManagerBase, IMemoryManager, IVirtualMemoryManagerTracked, IWritableBlock
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
            ReadWriteTrackedReplicated = ulong.MaxValue,
        }

        private readonly InvalidAccessHandler _invalidAccessHandler;

        private readonly ulong _addressSpaceSize;

        private readonly HvAddressSpace _addressSpace;

        internal HvAddressSpace AddressSpace => _addressSpace;

        private readonly MemoryBlock _backingMemory;
        private readonly PageTable<ulong> _pageTable;

        private readonly ulong[] _pageBitmap;

        public bool Supports4KBPages => true;

        public int AddressSpaceBits { get; }

        public IntPtr PageTablePointer => IntPtr.Zero;

        public MemoryManagerType Type => MemoryManagerType.SoftwarePageTable;

        public MemoryTracking Tracking { get; }

        public event Action<ulong, ulong> UnmapEvent;

        /// <summary>
        /// Creates a new instance of the Hypervisor memory manager.
        /// </summary>
        /// <param name="backingMemory">Physical backing memory where virtual memory will be mapped to</param>
        /// <param name="addressSpaceSize">Size of the address space</param>
        /// <param name="invalidAccessHandler">Optional function to handle invalid memory accesses</param>
        public HvMemoryManager(MemoryBlock backingMemory, ulong addressSpaceSize, InvalidAccessHandler invalidAccessHandler = null)
        {
            _backingMemory = backingMemory;
            _pageTable = new PageTable<ulong>();
            _invalidAccessHandler = invalidAccessHandler;
            _addressSpaceSize = addressSpaceSize;

            ulong asSize = PageSize;
            int asBits = PageBits;

            while (asSize < addressSpaceSize)
            {
                asSize <<= 1;
                asBits++;
            }

            _addressSpace = new HvAddressSpace(backingMemory, asSize);

            AddressSpaceBits = asBits;

            _pageBitmap = new ulong[1 << (AddressSpaceBits - (PageBits + PageToPteShift))];
            Tracking = new MemoryTracking(this, PageSize, invalidAccessHandler);
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

        /// <inheritdoc/>
        public void Map(ulong va, ulong pa, ulong size, MemoryMapFlags flags)
        {
            AssertValidAddressAndSize(va, size);

            PtMap(va, pa, size);
            _addressSpace.MapUser(va, pa, size, MemoryPermission.ReadWriteExecute);
            AddMapping(va, size);

            Tracking.Map(va, size);
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

        /// <inheritdoc/>
        public void MapForeign(ulong va, nuint hostPointer, ulong size)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public void Unmap(ulong va, ulong size)
        {
            AssertValidAddressAndSize(va, size);

            UnmapEvent?.Invoke(va, size);
            Tracking.Unmap(va, size);

            RemoveMapping(va, size);
            _addressSpace.UnmapUser(va, size);
            PtUnmap(va, size);
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
            return MemoryMarshal.Cast<byte, T>(GetSpan(va, Unsafe.SizeOf<T>()))[0];
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
            ReadImpl(va, data);
        }

        /// <inheritdoc/>
        public void Write<T>(ulong va, T value) where T : unmanaged
        {
            Write(va, MemoryMarshal.Cast<T, byte>(MemoryMarshal.CreateSpan(ref value, 1)));
        }

        /// <inheritdoc/>
        public void Write(ulong va, ReadOnlySpan<byte> data)
        {
            if (data.Length == 0)
            {
                return;
            }

            SignalMemoryTracking(va, (ulong)data.Length, true);

            WriteImpl(va, data);
        }

        /// <inheritdoc/>
        public void WriteUntracked(ulong va, ReadOnlySpan<byte> data)
        {
            if (data.Length == 0)
            {
                return;
            }

            WriteImpl(va, data);
        }

        /// <inheritdoc/>
        public bool WriteWithRedundancyCheck(ulong va, ReadOnlySpan<byte> data)
        {
            if (data.Length == 0)
            {
                return false;
            }

            SignalMemoryTracking(va, (ulong)data.Length, false);

            if (IsContiguousAndMapped(va, data.Length))
            {
                var target = _backingMemory.GetSpan(GetPhysicalAddressInternal(va), data.Length);

                bool changed = !data.SequenceEqual(target);

                if (changed)
                {
                    data.CopyTo(target);
                }

                return changed;
            }
            else
            {
                WriteImpl(va, data);

                return true;
            }
        }

        private void WriteImpl(ulong va, ReadOnlySpan<byte> data)
        {
            try
            {
                AssertValidAddressAndSize(va, (ulong)data.Length);

                if (IsContiguousAndMapped(va, data.Length))
                {
                    data.CopyTo(_backingMemory.GetSpan(GetPhysicalAddressInternal(va), data.Length));
                }
                else
                {
                    int offset = 0, size;

                    if ((va & PageMask) != 0)
                    {
                        ulong pa = GetPhysicalAddressChecked(va);

                        size = Math.Min(data.Length, PageSize - (int)(va & PageMask));

                        data[..size].CopyTo(_backingMemory.GetSpan(pa, size));

                        offset += size;
                    }

                    for (; offset < data.Length; offset += size)
                    {
                        ulong pa = GetPhysicalAddressChecked(va + (ulong)offset);

                        size = Math.Min(data.Length - offset, PageSize);

                        data.Slice(offset, size).CopyTo(_backingMemory.GetSpan(pa, size));
                    }
                }
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
            if (size == 0)
            {
                return ReadOnlySpan<byte>.Empty;
            }

            if (tracked)
            {
                SignalMemoryTracking(va, (ulong)size, false);
            }

            if (IsContiguousAndMapped(va, size))
            {
                return _backingMemory.GetSpan(GetPhysicalAddressInternal(va), size);
            }
            else
            {
                Span<byte> data = new byte[size];

                ReadImpl(va, data);

                return data;
            }
        }

        /// <inheritdoc/>
        public WritableRegion GetWritableRegion(ulong va, int size, bool tracked = false)
        {
            if (size == 0)
            {
                return new WritableRegion(null, va, Memory<byte>.Empty);
            }

            if (tracked)
            {
                SignalMemoryTracking(va, (ulong)size, true);
            }

            if (IsContiguousAndMapped(va, size))
            {
                return new WritableRegion(null, va, _backingMemory.GetMemory(GetPhysicalAddressInternal(va), size));
            }
            else
            {
                Memory<byte> memory = new byte[size];

                ReadImpl(va, memory.Span);

                return new WritableRegion(this, va, memory);
            }
        }

        /// <inheritdoc/>
        public ref T GetRef<T>(ulong va) where T : unmanaged
        {
            if (!IsContiguous(va, Unsafe.SizeOf<T>()))
            {
                ThrowMemoryNotContiguous();
            }

            SignalMemoryTracking(va, (ulong)Unsafe.SizeOf<T>(), true);

            return ref _backingMemory.GetRef<T>(GetPhysicalAddressChecked(va));
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
        private static void GetPageBlockRange(ulong pageStart, ulong pageEnd, out ulong startMask, out ulong endMask, out int pageIndex, out int pageEndIndex)
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

        private static void ThrowMemoryNotContiguous() => throw new MemoryNotContiguousException();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsContiguousAndMapped(ulong va, int size) => IsContiguous(va, size) && IsMapped(va);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsContiguous(ulong va, int size)
        {
            if (!ValidateAddress(va) || !ValidateAddressAndSize(va, (ulong)size))
            {
                return false;
            }

            int pages = GetPagesCount(va, (uint)size, out va);

            for (int page = 0; page < pages - 1; page++)
            {
                if (!ValidateAddress(va + PageSize))
                {
                    return false;
                }

                if (GetPhysicalAddressInternal(va) + PageSize != GetPhysicalAddressInternal(va + PageSize))
                {
                    return false;
                }

                va += PageSize;
            }

            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<HostMemoryRange> GetHostRegions(ulong va, ulong size)
        {
            if (size == 0)
            {
                return Enumerable.Empty<HostMemoryRange>();
            }

            var guestRegions = GetPhysicalRegionsImpl(va, size);
            if (guestRegions == null)
            {
                return null;
            }

            var regions = new HostMemoryRange[guestRegions.Count];

            for (int i = 0; i < regions.Length; i++)
            {
                var guestRegion = guestRegions[i];
                IntPtr pointer = _backingMemory.GetPointer(guestRegion.Address, guestRegion.Size);
                regions[i] = new HostMemoryRange((nuint)(ulong)pointer, guestRegion.Size);
            }

            return regions;
        }

        /// <inheritdoc/>
        public IEnumerable<MemoryRange> GetPhysicalRegions(ulong va, ulong size)
        {
            if (size == 0)
            {
                return Enumerable.Empty<MemoryRange>();
            }

            return GetPhysicalRegionsImpl(va, size);
        }

        private List<MemoryRange> GetPhysicalRegionsImpl(ulong va, ulong size)
        {
            if (!ValidateAddress(va) || !ValidateAddressAndSize(va, size))
            {
                return null;
            }

            int pages = GetPagesCount(va, (uint)size, out va);

            var regions = new List<MemoryRange>();

            ulong regionStart = GetPhysicalAddressInternal(va);
            ulong regionSize = PageSize;

            for (int page = 0; page < pages - 1; page++)
            {
                if (!ValidateAddress(va + PageSize))
                {
                    return null;
                }

                ulong newPa = GetPhysicalAddressInternal(va + PageSize);

                if (GetPhysicalAddressInternal(va) + PageSize != newPa)
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

        private void ReadImpl(ulong va, Span<byte> data)
        {
            if (data.Length == 0)
            {
                return;
            }

            try
            {
                AssertValidAddressAndSize(va, (ulong)data.Length);

                int offset = 0, size;

                if ((va & PageMask) != 0)
                {
                    ulong pa = GetPhysicalAddressChecked(va);

                    size = Math.Min(data.Length, PageSize - (int)(va & PageMask));

                    _backingMemory.GetSpan(pa, size).CopyTo(data[..size]);

                    offset += size;
                }

                for (; offset < data.Length; offset += size)
                {
                    ulong pa = GetPhysicalAddressChecked(va + (ulong)offset);

                    size = Math.Min(data.Length - offset, PageSize);

                    _backingMemory.GetSpan(pa, size).CopyTo(data.Slice(offset, size));
                }
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
        /// <remarks>
        /// This function also validates that the given range is both valid and mapped, and will throw if it is not.
        /// </remarks>
        public void SignalMemoryTracking(ulong va, ulong size, bool write, bool precise = false, int? exemptId = null)
        {
            AssertValidAddressAndSize(va, size);

            if (precise)
            {
                Tracking.VirtualMemoryEvent(va, size, write, precise: true, exemptId);
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
                    Tracking.VirtualMemoryEvent(va, size, write, precise: false, exemptId);
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
                            Tracking.VirtualMemoryEvent(va, size, write, precise: false, exemptId);
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
        private static int GetPagesCount(ulong va, ulong size, out ulong startVa)
        {
            // WARNING: Always check if ulong does not overflow during the operations.
            startVa = va & ~(ulong)PageMask;
            ulong vaSpan = (va - startVa + size + PageMask) & ~(ulong)PageMask;

            return (int)(vaSpan / PageSize);
        }

        /// <inheritdoc/>
        public void Reprotect(ulong va, ulong size, MemoryPermission protection)
        {
            if (protection.HasFlag(MemoryPermission.Execute))
            {
                // Some applications use unordered exclusive memory access instructions
                // where it is not valid to do so, leading to memory re-ordering that
                // makes the code behave incorrectly on some CPUs.
                // To work around this, we force all such accesses to be ordered.

                using WritableRegion writableRegion = GetWritableRegion(va, (int)size);

                HvCodePatcher.RewriteUnorderedExclusiveInstructions(writableRegion.Memory.Span);
            }

            // TODO
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
                _ => MemoryPermission.None,
            };

            _addressSpace.ReprotectUser(va, size, protection);
        }

        /// <inheritdoc/>
        public RegionHandle BeginTracking(ulong address, ulong size, int id)
        {
            return Tracking.BeginTracking(address, size, id);
        }

        /// <inheritdoc/>
        public MultiRegionHandle BeginGranularTracking(ulong address, ulong size, IEnumerable<IRegionHandle> handles, ulong granularity, int id)
        {
            return Tracking.BeginGranularTracking(address, size, handles, granularity, id);
        }

        /// <inheritdoc/>
        public SmartMultiRegionHandle BeginSmartGranularTracking(ulong address, ulong size, ulong granularity, int id)
        {
            return Tracking.BeginSmartGranularTracking(address, size, granularity, id);
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

        /// <summary>
        /// Disposes of resources used by the memory manager.
        /// </summary>
        protected override void Destroy()
        {
            _addressSpace.Dispose();
        }

        private static void ThrowInvalidMemoryRegionException(string message) => throw new InvalidMemoryRegionException(message);
    }
}
