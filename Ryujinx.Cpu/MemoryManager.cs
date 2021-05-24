using ARMeilleure.Memory;
using Ryujinx.Cpu.Tracking;
using Ryujinx.Memory;
using Ryujinx.Memory.Range;
using Ryujinx.Memory.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ryujinx.Cpu
{
    /// <summary>
    /// Represents a CPU memory manager.
    /// </summary>
    public sealed class MemoryManager : MemoryManagerBase, IMemoryManager, IVirtualMemoryManagerTracked, IWritableBlock
    {
        public const int PageBits = 12;
        public const int PageSize = 1 << PageBits;
        public const int PageMask = PageSize - 1;

        private const int PteSize = 8;

        private const int PointerTagBit = 62;

        private readonly InvalidAccessHandler _invalidAccessHandler;

        /// <summary>
        /// Address space width in bits.
        /// </summary>
        public int AddressSpaceBits { get; }

        private readonly ulong _addressSpaceSize;

        private readonly MemoryBlock _pageTable;

        /// <summary>
        /// Page table base pointer.
        /// </summary>
        public IntPtr PageTablePointer => _pageTable.Pointer;

        public MemoryManagerType Type => MemoryManagerType.SoftwarePageTable;

        public MemoryTracking Tracking { get; }

        public event Action<ulong, ulong> UnmapEvent;

        /// <summary>
        /// Creates a new instance of the memory manager.
        /// </summary>
        /// <param name="addressSpaceSize">Size of the address space</param>
        /// <param name="invalidAccessHandler">Optional function to handle invalid memory accesses</param>
        public MemoryManager(ulong addressSpaceSize, InvalidAccessHandler invalidAccessHandler = null)
        {
            _invalidAccessHandler = invalidAccessHandler;

            ulong asSize = PageSize;
            int asBits = PageBits;

            while (asSize < addressSpaceSize)
            {
                asSize <<= 1;
                asBits++;
            }

            AddressSpaceBits = asBits;
            _addressSpaceSize = asSize;
            _pageTable = new MemoryBlock((asSize / PageSize) * PteSize);

            Tracking = new MemoryTracking(this, PageSize);
        }

        /// <inheritdoc/>
        public void Map(ulong va, nuint hostAddress, ulong size)
        {
            AssertValidAddressAndSize(va, size);

            ulong remainingSize = size;
            ulong oVa = va;
            while (remainingSize != 0)
            {
                _pageTable.Write((va / PageSize) * PteSize, hostAddress);

                va += PageSize;
                hostAddress += PageSize;
                remainingSize -= PageSize;
            }
            Tracking.Map(oVa, size);
        }

        /// <inheritdoc/>
        public void Unmap(ulong va, ulong size)
        {
            // If size is 0, there's nothing to unmap, just exit early.
            if (size == 0)
            {
                return;
            }

            AssertValidAddressAndSize(va, size);

            UnmapEvent?.Invoke(va, size);
            Tracking.Unmap(va, size);

            ulong remainingSize = size;
            while (remainingSize != 0)
            {
                _pageTable.Write((va / PageSize) * PteSize, (nuint)0);

                va += PageSize;
                remainingSize -= PageSize;
            }
        }

        /// <inheritdoc/>
        public T Read<T>(ulong va) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(GetSpan(va, Unsafe.SizeOf<T>(), true))[0];
        }

        /// <inheritdoc/>
        public T ReadTracked<T>(ulong va) where T : unmanaged
        {
            SignalMemoryTracking(va, (ulong)Unsafe.SizeOf<T>(), false);
            return MemoryMarshal.Cast<byte, T>(GetSpan(va, Unsafe.SizeOf<T>()))[0];
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

        /// <summary>
        /// Writes data to CPU mapped memory.
        /// </summary>
        /// <param name="va">Virtual address to write the data into</param>
        /// <param name="data">Data to be written</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteImpl(ulong va, ReadOnlySpan<byte> data)
        {
            try
            {
                AssertValidAddressAndSize(va, (ulong)data.Length);

                if (IsContiguousAndMapped(va, data.Length))
                {
                    data.CopyTo(GetHostSpanContiguous(va, data.Length));
                }
                else
                {
                    int offset = 0, size;

                    if ((va & PageMask) != 0)
                    {
                        size = Math.Min(data.Length, PageSize - (int)(va & PageMask));

                        data.Slice(0, size).CopyTo(GetHostSpanContiguous(va, size));

                        offset += size;
                    }

                    for (; offset < data.Length; offset += size)
                    {
                        size = Math.Min(data.Length - offset, PageSize);

                        data.Slice(offset, size).CopyTo(GetHostSpanContiguous(va + (ulong)offset, size));
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
                return GetHostSpanContiguous(va, size);
            }
            else
            {
                Span<byte> data = new byte[size];

                ReadImpl(va, data);

                return data;
            }
        }

        /// <inheritdoc/>
        public unsafe WritableRegion GetWritableRegion(ulong va, int size)
        {
            if (size == 0)
            {
                return new WritableRegion(null, va, Memory<byte>.Empty);
            }

            if (IsContiguousAndMapped(va, size))
            {
                return new WritableRegion(null, va, new NativeMemoryManager<byte>((byte*)GetHostAddress(va), size).Memory);
            }
            else
            {
                Memory<byte> memory = new byte[size];

                GetSpan(va, size).CopyTo(memory.Span);

                return new WritableRegion(this, va, memory);
            }
        }

        /// <inheritdoc/>
        public unsafe ref T GetRef<T>(ulong va) where T : unmanaged
        {
            if (!IsContiguous(va, Unsafe.SizeOf<T>()))
            {
                ThrowMemoryNotContiguous();
            }

            SignalMemoryTracking(va, (ulong)Unsafe.SizeOf<T>(), true);

            return ref *(T*)GetHostAddress(va);
        }

        /// <summary>
        /// Computes the number of pages in a virtual address range.
        /// </summary>
        /// <param name="va">Virtual address of the range</param>
        /// <param name="size">Size of the range</param>
        /// <param name="startVa">The virtual address of the beginning of the first page</param>
        /// <remarks>This function does not differentiate between allocated and unallocated pages.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetPagesCount(ulong va, uint size, out ulong startVa)
        {
            // WARNING: Always check if ulong does not overflow during the operations.
            startVa = va & ~(ulong)PageMask;
            ulong vaSpan = (va - startVa + size + PageMask) & ~(ulong)PageMask;

            return (int)(vaSpan / PageSize);
        }

        private void ThrowMemoryNotContiguous() => throw new MemoryNotContiguousException();

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

                if (GetHostAddress(va) + PageSize != GetHostAddress(va + PageSize))
                {
                    return false;
                }

                va += PageSize;
            }

            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<HostMemoryRange> GetPhysicalRegions(ulong va, ulong size)
        {
            if (size == 0)
            {
                return Enumerable.Empty<HostMemoryRange>();
            }

            if (!ValidateAddress(va) || !ValidateAddressAndSize(va, size))
            {
                return null;
            }

            int pages = GetPagesCount(va, (uint)size, out va);

            var regions = new List<HostMemoryRange>();

            nuint regionStart = GetHostAddress(va);
            ulong regionSize = PageSize;

            for (int page = 0; page < pages - 1; page++)
            {
                if (!ValidateAddress(va + PageSize))
                {
                    return null;
                }

                nuint newHostAddress = GetHostAddress(va + PageSize);

                if (GetHostAddress(va) + PageSize != newHostAddress)
                {
                    regions.Add(new HostMemoryRange(regionStart, regionSize));
                    regionStart = newHostAddress;
                    regionSize = 0;
                }

                va += PageSize;
                regionSize += PageSize;
            }

            regions.Add(new HostMemoryRange(regionStart, regionSize));

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
                    size = Math.Min(data.Length, PageSize - (int)(va & PageMask));

                    GetHostSpanContiguous(va, size).CopyTo(data.Slice(0, size));

                    offset += size;
                }

                for (; offset < data.Length; offset += size)
                {
                    size = Math.Min(data.Length - offset, PageSize);

                    GetHostSpanContiguous(va + (ulong)offset, size).CopyTo(data.Slice(offset, size));
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
        public bool IsRangeMapped(ulong va, ulong size)
        {
            if (size == 0UL)
            {
                return true;
            }

            if (!ValidateAddressAndSize(va, size))
            {
                return false;
            }

            int pages = GetPagesCount(va, (uint)size, out va);

            for (int page = 0; page < pages; page++)
            {
                if (!IsMapped(va))
                {
                    return false;
                }

                va += PageSize;
            }

            return true;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMapped(ulong va)
        {
            if (!ValidateAddress(va))
            {
                return false;
            }

            return _pageTable.Read<nuint>((va / PageSize) * PteSize) != 0;
        }

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
        /// Get a span representing the given virtual address and size range in host memory.
        /// This function assumes that the requested virtual memory region is contiguous.
        /// </summary>
        /// <param name="va">Virtual address of the range</param>
        /// <param name="size">Size of the range in bytes</param>
        /// <returns>A span representing the given virtual range in host memory</returns>
        /// <exception cref="InvalidMemoryRegionException">Throw when the base virtual address is not mapped</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe Span<byte> GetHostSpanContiguous(ulong va, int size)
        {
            return new Span<byte>((void*)GetHostAddress(va), size);
        }

        /// <summary>
        /// Get the host address for a given virtual address, using the page table.
        /// </summary>
        /// <param name="va">Virtual address</param>
        /// <returns>The corresponding host address for the given virtual address</returns>
        /// <exception cref="InvalidMemoryRegionException">Throw when the virtual address is not mapped</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private nuint GetHostAddress(ulong va)
        {
            nuint pageBase = _pageTable.Read<nuint>((va / PageSize) * PteSize) & unchecked((nuint)0xffff_ffff_ffffUL);

            if (pageBase == 0)
            {
                ThrowInvalidMemoryRegionException($"Not mapped: va=0x{va:X16}");
            }

            return pageBase + (nuint)(va & PageMask);
        }

        /// <inheritdoc/>
        public void TrackingReprotect(ulong va, ulong size, MemoryPermission protection)
        {
            AssertValidAddressAndSize(va, size);

            // Protection is inverted on software pages, since the default value is 0.
            protection = (~protection) & MemoryPermission.ReadAndWrite;

            long tag = protection switch
            {
                MemoryPermission.None => 0L,
                MemoryPermission.Write => 2L << PointerTagBit,
                _ => 3L << PointerTagBit
            };

            int pages = GetPagesCount(va, (uint)size, out va);
            ulong pageStart = va >> PageBits;
            long invTagMask = ~(0xffffL << 48);

            for (int page = 0; page < pages; page++)
            {
                ref long pageRef = ref _pageTable.GetRef<long>(pageStart * PteSize);

                long pte;

                do
                {
                    pte = Volatile.Read(ref pageRef);
                }
                while (pte != 0 && Interlocked.CompareExchange(ref pageRef, (pte & invTagMask) | tag, pte) != pte);

                pageStart++;
            }
        }

        /// <inheritdoc/>
        public CpuRegionHandle BeginTracking(ulong address, ulong size)
        {
            return new CpuRegionHandle(Tracking.BeginTracking(address, size));
        }

        /// <inheritdoc/>
        public CpuMultiRegionHandle BeginGranularTracking(ulong address, ulong size, ulong granularity)
        {
            return new CpuMultiRegionHandle(Tracking.BeginGranularTracking(address, size, granularity));
        }

        /// <inheritdoc/>
        public CpuSmartMultiRegionHandle BeginSmartGranularTracking(ulong address, ulong size, ulong granularity)
        {
            return new CpuSmartMultiRegionHandle(Tracking.BeginSmartGranularTracking(address, size, granularity));
        }

        /// <inheritdoc/>
        public void SignalMemoryTracking(ulong va, ulong size, bool write)
        {
            AssertValidAddressAndSize(va, size);

            // We emulate guard pages for software memory access. This makes for an easy transition to
            // tracking using host guard pages in future, but also supporting platforms where this is not possible.

            // Write tag includes read protection, since we don't have any read actions that aren't performed before write too.
            long tag = (write ? 3L : 1L) << PointerTagBit;

            int pages = GetPagesCount(va, (uint)size, out _);
            ulong pageStart = va >> PageBits;

            for (int page = 0; page < pages; page++)
            {
                ref long pageRef = ref _pageTable.GetRef<long>(pageStart * PteSize);

                long pte;

                pte = Volatile.Read(ref pageRef);

                if ((pte & tag) != 0)
                {
                    Tracking.VirtualMemoryEvent(va, size, write);
                    break;
                }

                pageStart++;
            }
        }

        /// <summary>
        /// Disposes of resources used by the memory manager.
        /// </summary>
        protected override void Destroy() => _pageTable.Dispose();

        private void ThrowInvalidMemoryRegionException(string message) => throw new InvalidMemoryRegionException(message);
    }
}
