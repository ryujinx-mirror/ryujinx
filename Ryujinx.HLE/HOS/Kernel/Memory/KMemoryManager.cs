using Ryujinx.Common;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KMemoryManager
    {
        private static readonly int[] MappingUnitSizes = new int[]
        {
            0x1000,
            0x10000,
            0x200000,
            0x400000,
            0x2000000,
            0x40000000
        };

        public const int PageSize = 0x1000;

        private const int KMemoryBlockSize = 0x40;

        // We need 2 blocks for the case where a big block
        // needs to be split in 2, plus one block that will be the new one inserted.
        private const int MaxBlocksNeededForInsertion = 2;

        private readonly LinkedList<KMemoryBlock> _blocks;

        private readonly IVirtualMemoryManager _cpuMemory;

        private readonly KernelContext _context;

        public ulong AddrSpaceStart { get; private set; }
        public ulong AddrSpaceEnd   { get; private set; }

        public ulong CodeRegionStart { get; private set; }
        public ulong CodeRegionEnd   { get; private set; }

        public ulong HeapRegionStart { get; private set; }
        public ulong HeapRegionEnd   { get; private set; }

        private ulong _currentHeapAddr;

        public ulong AliasRegionStart { get; private set; }
        public ulong AliasRegionEnd   { get; private set; }

        public ulong StackRegionStart { get; private set; }
        public ulong StackRegionEnd   { get; private set; }

        public ulong TlsIoRegionStart { get; private set; }
        public ulong TlsIoRegionEnd   { get; private set; }

        private ulong _heapCapacity;

        public ulong PhysicalMemoryUsage { get; private set; }

        private MemoryRegion _memRegion;

        private bool _aslrDisabled;

        public int AddrSpaceWidth { get; private set; }

        private bool _isKernel;

        private bool _aslrEnabled;

        private KMemoryBlockAllocator _blockAllocator;

        private int _contextId;

        private MersenneTwister _randomNumberGenerator;

        public KMemoryManager(KernelContext context, IVirtualMemoryManager cpuMemory)
        {
            _context   = context;
            _cpuMemory = cpuMemory;

            _blocks = new LinkedList<KMemoryBlock>();

            _isKernel = false;
        }

        private static readonly int[] AddrSpaceSizes = new int[] { 32, 36, 32, 39 };

        public KernelResult InitializeForProcess(
            AddressSpaceType      addrSpaceType,
            bool                  aslrEnabled,
            bool                  aslrDisabled,
            MemoryRegion          memRegion,
            ulong                 address,
            ulong                 size,
            KMemoryBlockAllocator blockAllocator)
        {
            if ((uint)addrSpaceType > (uint)AddressSpaceType.Addr39Bits)
            {
                throw new ArgumentException(nameof(addrSpaceType));
            }

            _contextId = _context.ContextIdManager.GetId();

            ulong addrSpaceBase = 0;
            ulong addrSpaceSize = 1UL << AddrSpaceSizes[(int)addrSpaceType];

            KernelResult result = CreateUserAddressSpace(
                addrSpaceType,
                aslrEnabled,
                aslrDisabled,
                addrSpaceBase,
                addrSpaceSize,
                memRegion,
                address,
                size,
                blockAllocator);

            if (result != KernelResult.Success)
            {
                _context.ContextIdManager.PutId(_contextId);
            }

            return result;
        }

        private class Region
        {
            public ulong Start;
            public ulong End;
            public ulong Size;
            public ulong AslrOffset;
        }

        private KernelResult CreateUserAddressSpace(
            AddressSpaceType      addrSpaceType,
            bool                  aslrEnabled,
            bool                  aslrDisabled,
            ulong                 addrSpaceStart,
            ulong                 addrSpaceEnd,
            MemoryRegion          memRegion,
            ulong                 address,
            ulong                 size,
            KMemoryBlockAllocator blockAllocator)
        {
            ulong endAddr = address + size;

            Region aliasRegion = new Region();
            Region heapRegion  = new Region();
            Region stackRegion = new Region();
            Region tlsIoRegion = new Region();

            ulong codeRegionSize;
            ulong stackAndTlsIoStart;
            ulong stackAndTlsIoEnd;
            ulong baseAddress;

            switch (addrSpaceType)
            {
                case AddressSpaceType.Addr32Bits:
                    aliasRegion.Size   = 0x40000000;
                    heapRegion.Size    = 0x40000000;
                    stackRegion.Size   = 0;
                    tlsIoRegion.Size   = 0;
                    CodeRegionStart    = 0x200000;
                    codeRegionSize     = 0x3fe00000;
                    stackAndTlsIoStart = 0x200000;
                    stackAndTlsIoEnd   = 0x40000000;
                    baseAddress        = 0x200000;
                    AddrSpaceWidth     = 32;
                    break;

                case AddressSpaceType.Addr36Bits:
                    aliasRegion.Size   = 0x180000000;
                    heapRegion.Size    = 0x180000000;
                    stackRegion.Size   = 0;
                    tlsIoRegion.Size   = 0;
                    CodeRegionStart    = 0x8000000;
                    codeRegionSize     = 0x78000000;
                    stackAndTlsIoStart = 0x8000000;
                    stackAndTlsIoEnd   = 0x80000000;
                    baseAddress        = 0x8000000;
                    AddrSpaceWidth     = 36;
                    break;

                case AddressSpaceType.Addr32BitsNoMap:
                    aliasRegion.Size   = 0;
                    heapRegion.Size    = 0x80000000;
                    stackRegion.Size   = 0;
                    tlsIoRegion.Size   = 0;
                    CodeRegionStart    = 0x200000;
                    codeRegionSize     = 0x3fe00000;
                    stackAndTlsIoStart = 0x200000;
                    stackAndTlsIoEnd   = 0x40000000;
                    baseAddress        = 0x200000;
                    AddrSpaceWidth     = 32;
                    break;

                case AddressSpaceType.Addr39Bits:
                    aliasRegion.Size   = 0x1000000000;
                    heapRegion.Size    = 0x180000000;
                    stackRegion.Size   = 0x80000000;
                    tlsIoRegion.Size   = 0x1000000000;
                    CodeRegionStart    = BitUtils.AlignDown(address, 0x200000);
                    codeRegionSize     = BitUtils.AlignUp  (endAddr, 0x200000) - CodeRegionStart;
                    stackAndTlsIoStart = 0;
                    stackAndTlsIoEnd   = 0;
                    baseAddress        = 0x8000000;
                    AddrSpaceWidth     = 39;
                    break;

                default: throw new ArgumentException(nameof(addrSpaceType));
            }

            CodeRegionEnd = CodeRegionStart + codeRegionSize;

            ulong mapBaseAddress;
            ulong mapAvailableSize;

            if (CodeRegionStart - baseAddress >= addrSpaceEnd - CodeRegionEnd)
            {
                // Has more space before the start of the code region.
                mapBaseAddress   = baseAddress;
                mapAvailableSize = CodeRegionStart - baseAddress;
            }
            else
            {
                // Has more space after the end of the code region.
                mapBaseAddress   = CodeRegionEnd;
                mapAvailableSize = addrSpaceEnd - CodeRegionEnd;
            }

            ulong mapTotalSize = aliasRegion.Size + heapRegion.Size + stackRegion.Size + tlsIoRegion.Size;

            ulong aslrMaxOffset = mapAvailableSize - mapTotalSize;

            _aslrEnabled = aslrEnabled;

            AddrSpaceStart = addrSpaceStart;
            AddrSpaceEnd   = addrSpaceEnd;

            _blockAllocator = blockAllocator;

            if (mapAvailableSize < mapTotalSize)
            {
                return KernelResult.OutOfMemory;
            }

            if (aslrEnabled)
            {
                aliasRegion.AslrOffset = GetRandomValue(0, aslrMaxOffset >> 21) << 21;
                heapRegion.AslrOffset  = GetRandomValue(0, aslrMaxOffset >> 21) << 21;
                stackRegion.AslrOffset = GetRandomValue(0, aslrMaxOffset >> 21) << 21;
                tlsIoRegion.AslrOffset = GetRandomValue(0, aslrMaxOffset >> 21) << 21;
            }

            // Regions are sorted based on ASLR offset.
            // When ASLR is disabled, the order is Map, Heap, NewMap and TlsIo.
            aliasRegion.Start = mapBaseAddress    + aliasRegion.AslrOffset;
            aliasRegion.End   = aliasRegion.Start + aliasRegion.Size;
            heapRegion.Start  = mapBaseAddress    + heapRegion.AslrOffset;
            heapRegion.End    = heapRegion.Start  + heapRegion.Size;
            stackRegion.Start = mapBaseAddress    + stackRegion.AslrOffset;
            stackRegion.End   = stackRegion.Start + stackRegion.Size;
            tlsIoRegion.Start = mapBaseAddress    + tlsIoRegion.AslrOffset;
            tlsIoRegion.End   = tlsIoRegion.Start + tlsIoRegion.Size;

            SortRegion(heapRegion, aliasRegion);

            if (stackRegion.Size != 0)
            {
                SortRegion(stackRegion, aliasRegion);
                SortRegion(stackRegion, heapRegion);
            }
            else
            {
                stackRegion.Start = stackAndTlsIoStart;
                stackRegion.End   = stackAndTlsIoEnd;
            }

            if (tlsIoRegion.Size != 0)
            {
                SortRegion(tlsIoRegion, aliasRegion);
                SortRegion(tlsIoRegion, heapRegion);
                SortRegion(tlsIoRegion, stackRegion);
            }
            else
            {
                tlsIoRegion.Start = stackAndTlsIoStart;
                tlsIoRegion.End   = stackAndTlsIoEnd;
            }

            AliasRegionStart = aliasRegion.Start;
            AliasRegionEnd   = aliasRegion.End;
            HeapRegionStart  = heapRegion.Start;
            HeapRegionEnd    = heapRegion.End;
            StackRegionStart = stackRegion.Start;
            StackRegionEnd   = stackRegion.End;
            TlsIoRegionStart = tlsIoRegion.Start;
            TlsIoRegionEnd   = tlsIoRegion.End;

            _currentHeapAddr    = HeapRegionStart;
            _heapCapacity       = 0;
            PhysicalMemoryUsage = 0;

            _memRegion    = memRegion;
            _aslrDisabled = aslrDisabled;

            return InitializeBlocks(addrSpaceStart, addrSpaceEnd);
        }

        private ulong GetRandomValue(ulong min, ulong max)
        {
            return (ulong)GetRandomValue((long)min, (long)max);
        }

        private long GetRandomValue(long min, long max)
        {
            if (_randomNumberGenerator == null)
            {
                _randomNumberGenerator = new MersenneTwister(0);
            }

            return _randomNumberGenerator.GenRandomNumber(min, max);
        }

        private static void SortRegion(Region lhs, Region rhs)
        {
            if (lhs.AslrOffset < rhs.AslrOffset)
            {
                rhs.Start += lhs.Size;
                rhs.End   += lhs.Size;
            }
            else
            {
                lhs.Start += rhs.Size;
                lhs.End   += rhs.Size;
            }
        }

        private KernelResult InitializeBlocks(ulong addrSpaceStart, ulong addrSpaceEnd)
        {
            // First insertion will always need only a single block,
            // because there's nothing else to split.
            if (!_blockAllocator.CanAllocate(1))
            {
                return KernelResult.OutOfResource;
            }

            ulong addrSpacePagesCount = (addrSpaceEnd - addrSpaceStart) / PageSize;

            _blocks.AddFirst(new KMemoryBlock(
                addrSpaceStart,
                addrSpacePagesCount,
                MemoryState.Unmapped,
                KMemoryPermission.None,
                MemoryAttribute.None));

            return KernelResult.Success;
        }

        public KernelResult MapPages(
            ulong            address,
            KPageList        pageList,
            MemoryState      state,
            KMemoryPermission permission)
        {
            ulong pagesCount = pageList.GetPagesCount();

            ulong size = pagesCount * PageSize;

            if (!CanContain(address, size, state))
            {
                return KernelResult.InvalidMemState;
            }

            lock (_blocks)
            {
                if (!IsUnmapped(address, pagesCount * PageSize))
                {
                    return KernelResult.InvalidMemState;
                }

                if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                KernelResult result = MapPages(address, pageList, permission);

                if (result == KernelResult.Success)
                {
                    InsertBlock(address, pagesCount, state, permission);
                }

                return result;
            }
        }

        public KernelResult UnmapPages(ulong address, KPageList pageList, MemoryState stateExpected)
        {
            ulong pagesCount = pageList.GetPagesCount();

            ulong size = pagesCount * PageSize;

            ulong endAddr = address + size;

            ulong addrSpacePagesCount = (AddrSpaceEnd - AddrSpaceStart) / PageSize;

            if (AddrSpaceStart > address)
            {
                return KernelResult.InvalidMemState;
            }

            if (addrSpacePagesCount < pagesCount)
            {
                return KernelResult.InvalidMemState;
            }

            if (endAddr - 1 > AddrSpaceEnd - 1)
            {
                return KernelResult.InvalidMemState;
            }

            lock (_blocks)
            {
                KPageList currentPageList = new KPageList();

                AddVaRangeToPageList(currentPageList, address, pagesCount);

                if (!currentPageList.IsEqual(pageList))
                {
                    return KernelResult.InvalidMemRange;
                }

                if (CheckRange(
                    address,
                    size,
                    MemoryState.Mask,
                    stateExpected,
                    KMemoryPermission.None,
                    KMemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState state,
                    out _,
                    out _))
                {
                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    KernelResult result = MmuUnmap(address, pagesCount);

                    if (result == KernelResult.Success)
                    {
                        InsertBlock(address, pagesCount, MemoryState.Unmapped);
                    }

                    return result;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult MapNormalMemory(long address, long size, KMemoryPermission permission)
        {
            // TODO.
            return KernelResult.Success;
        }

        public KernelResult MapIoMemory(long address, long size, KMemoryPermission permission)
        {
            // TODO.
            return KernelResult.Success;
        }

        public KernelResult AllocateOrMapPa(
            ulong            neededPagesCount,
            int              alignment,
            ulong            srcPa,
            bool             map,
            ulong            regionStart,
            ulong            regionPagesCount,
            MemoryState      state,
            KMemoryPermission permission,
            out ulong        address)
        {
            address = 0;

            ulong regionSize = regionPagesCount * PageSize;

            ulong regionEndAddr = regionStart + regionSize;

            if (!CanContain(regionStart, regionSize, state))
            {
                return KernelResult.InvalidMemState;
            }

            if (regionPagesCount <= neededPagesCount)
            {
                return KernelResult.OutOfMemory;
            }

            lock (_blocks)
            {
                address = AllocateVa(regionStart, regionPagesCount, neededPagesCount, alignment);

                if (address == 0)
                {
                    return KernelResult.OutOfMemory;
                }

                if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                MemoryOperation operation = map
                    ? MemoryOperation.MapPa
                    : MemoryOperation.Allocate;

                KernelResult result = DoMmuOperation(
                    address,
                    neededPagesCount,
                    srcPa,
                    map,
                    permission,
                    operation);

                if (result != KernelResult.Success)
                {
                    return result;
                }

                InsertBlock(address, neededPagesCount, state, permission);
            }

            return KernelResult.Success;
        }

        public KernelResult MapNewProcessCode(
            ulong            address,
            ulong            pagesCount,
            MemoryState      state,
            KMemoryPermission permission)
        {
            ulong size = pagesCount * PageSize;

            if (!CanContain(address, size, state))
            {
                return KernelResult.InvalidMemState;
            }

            lock (_blocks)
            {
                if (!IsUnmapped(address, size))
                {
                    return KernelResult.InvalidMemState;
                }

                if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                KernelResult result = DoMmuOperation(
                    address,
                    pagesCount,
                    0,
                    false,
                    permission,
                    MemoryOperation.Allocate);

                if (result == KernelResult.Success)
                {
                    InsertBlock(address, pagesCount, state, permission);
                }

                return result;
            }
        }

        public KernelResult MapProcessCodeMemory(ulong dst, ulong src, ulong size)
        {
            ulong pagesCount = size / PageSize;

            lock (_blocks)
            {
                bool success = CheckRange(
                    src,
                    size,
                    MemoryState.Mask,
                    MemoryState.Heap,
                    KMemoryPermission.Mask,
                    KMemoryPermission.ReadAndWrite,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState      state,
                    out KMemoryPermission permission,
                    out _);

                success &= IsUnmapped(dst, size);

                if (success)
                {
                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion * 2))
                    {
                        return KernelResult.OutOfResource;
                    }

                    KPageList pageList = new KPageList();

                    AddVaRangeToPageList(pageList, src, pagesCount);

                    KernelResult result = MmuChangePermission(src, pagesCount, KMemoryPermission.None);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    result = MapPages(dst, pageList, KMemoryPermission.None);

                    if (result != KernelResult.Success)
                    {
                        MmuChangePermission(src, pagesCount, permission);

                        return result;
                    }

                    InsertBlock(src, pagesCount, state, KMemoryPermission.None, MemoryAttribute.Borrowed);
                    InsertBlock(dst, pagesCount, MemoryState.ModCodeStatic);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult UnmapProcessCodeMemory(ulong dst, ulong src, ulong size)
        {
            ulong pagesCount = size / PageSize;

            lock (_blocks)
            {
                bool success = CheckRange(
                    src,
                    size,
                    MemoryState.Mask,
                    MemoryState.Heap,
                    KMemoryPermission.None,
                    KMemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.Borrowed,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out _,
                    out _,
                    out _);

                success &= CheckRange(
                    dst,
                    PageSize,
                    MemoryState.UnmapProcessCodeMemoryAllowed,
                    MemoryState.UnmapProcessCodeMemoryAllowed,
                    KMemoryPermission.None,
                    KMemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState state,
                    out _,
                    out _);

                success &= CheckRange(
                    dst,
                    size,
                    MemoryState.Mask,
                    state,
                    KMemoryPermission.None,
                    KMemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None);

                if (success)
                {
                    KernelResult result = MmuUnmap(dst, pagesCount);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    // TODO: Missing some checks here.

                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion * 2))
                    {
                        return KernelResult.OutOfResource;
                    }

                    InsertBlock(dst, pagesCount, MemoryState.Unmapped);
                    InsertBlock(src, pagesCount, MemoryState.Heap, KMemoryPermission.ReadAndWrite);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult SetHeapSize(ulong size, out ulong address)
        {
            address = 0;

            if (size > HeapRegionEnd - HeapRegionStart)
            {
                return KernelResult.OutOfMemory;
            }

            KProcess currentProcess = KernelStatic.GetCurrentProcess();

            lock (_blocks)
            {
                ulong currentHeapSize = GetHeapSize();

                if (currentHeapSize <= size)
                {
                    // Expand.
                    ulong sizeDelta = size - currentHeapSize;

                    if (currentProcess.ResourceLimit != null && sizeDelta != 0 &&
                        !currentProcess.ResourceLimit.Reserve(LimitableResource.Memory, sizeDelta))
                    {
                        return KernelResult.ResLimitExceeded;
                    }

                    ulong pagesCount = sizeDelta / PageSize;

                    KMemoryRegionManager region = GetMemoryRegionManager();

                    KernelResult result = region.AllocatePages(pagesCount, _aslrDisabled, out KPageList pageList);

                    void CleanUpForError()
                    {
                        if (pageList != null)
                        {
                            region.FreePages(pageList);
                        }

                        if (currentProcess.ResourceLimit != null && sizeDelta != 0)
                        {
                            currentProcess.ResourceLimit.Release(LimitableResource.Memory, sizeDelta);
                        }
                    }

                    if (result != KernelResult.Success)
                    {
                        CleanUpForError();

                        return result;
                    }

                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        CleanUpForError();

                        return KernelResult.OutOfResource;
                    }

                    if (!IsUnmapped(_currentHeapAddr, sizeDelta))
                    {
                        CleanUpForError();

                        return KernelResult.InvalidMemState;
                    }

                    result = DoMmuOperation(
                        _currentHeapAddr,
                        pagesCount,
                        pageList,
                        KMemoryPermission.ReadAndWrite,
                        MemoryOperation.MapVa);

                    if (result != KernelResult.Success)
                    {
                        CleanUpForError();

                        return result;
                    }

                    InsertBlock(_currentHeapAddr, pagesCount, MemoryState.Heap, KMemoryPermission.ReadAndWrite);
                }
                else
                {
                    // Shrink.
                    ulong freeAddr = HeapRegionStart + size;
                    ulong sizeDelta = currentHeapSize - size;

                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    if (!CheckRange(
                        freeAddr,
                        sizeDelta,
                        MemoryState.Mask,
                        MemoryState.Heap,
                        KMemoryPermission.Mask,
                        KMemoryPermission.ReadAndWrite,
                        MemoryAttribute.Mask,
                        MemoryAttribute.None,
                        MemoryAttribute.IpcAndDeviceMapped,
                        out _,
                        out _,
                        out _))
                    {
                        return KernelResult.InvalidMemState;
                    }

                    ulong pagesCount = sizeDelta / PageSize;

                    KernelResult result = MmuUnmap(freeAddr, pagesCount);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    currentProcess.ResourceLimit?.Release(LimitableResource.Memory, sizeDelta);

                    InsertBlock(freeAddr, pagesCount, MemoryState.Unmapped);
                }

                _currentHeapAddr = HeapRegionStart + size;
            }

            address = HeapRegionStart;

            return KernelResult.Success;
        }

        public ulong GetTotalHeapSize()
        {
            lock (_blocks)
            {
                return GetHeapSize() + PhysicalMemoryUsage;
            }
        }

        private ulong GetHeapSize()
        {
            return _currentHeapAddr - HeapRegionStart;
        }

        public KernelResult SetHeapCapacity(ulong capacity)
        {
            lock (_blocks)
            {
                _heapCapacity = capacity;
            }

            return KernelResult.Success;
        }

        public KernelResult SetMemoryAttribute(
            ulong           address,
            ulong           size,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeValue)
        {
            lock (_blocks)
            {
                if (CheckRange(
                    address,
                    size,
                    MemoryState.AttributeChangeAllowed,
                    MemoryState.AttributeChangeAllowed,
                    KMemoryPermission.None,
                    KMemoryPermission.None,
                    MemoryAttribute.BorrowedAndIpcMapped,
                    MemoryAttribute.None,
                    MemoryAttribute.DeviceMappedAndUncached,
                    out MemoryState      state,
                    out KMemoryPermission permission,
                    out MemoryAttribute  attribute))
                {
                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong pagesCount = size / PageSize;

                    attribute &= ~attributeMask;
                    attribute |=  attributeMask & attributeValue;

                    InsertBlock(address, pagesCount, state, permission, attribute);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KMemoryInfo QueryMemory(ulong address)
        {
            if (address >= AddrSpaceStart &&
                address <  AddrSpaceEnd)
            {
                lock (_blocks)
                {
                    return FindBlock(address).GetInfo();
                }
            }
            else
            {
                return new KMemoryInfo(
                    AddrSpaceEnd,
                    ~AddrSpaceEnd + 1,
                    MemoryState.Reserved,
                    KMemoryPermission.None,
                    MemoryAttribute.None,
                    KMemoryPermission.None,
                    0,
                    0);
            }
        }

        public KernelResult Map(ulong dst, ulong src, ulong size)
        {
            bool success;

            lock (_blocks)
            {
                success = CheckRange(
                    src,
                    size,
                    MemoryState.MapAllowed,
                    MemoryState.MapAllowed,
                    KMemoryPermission.Mask,
                    KMemoryPermission.ReadAndWrite,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState srcState,
                    out _,
                    out _);

                success &= IsUnmapped(dst, size);

                if (success)
                {
                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion * 2))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong pagesCount = size / PageSize;

                    KPageList pageList = new KPageList();

                    AddVaRangeToPageList(pageList, src, pagesCount);

                    KernelResult result = MmuChangePermission(src, pagesCount, KMemoryPermission.None);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    result = MapPages(dst, pageList, KMemoryPermission.ReadAndWrite);

                    if (result != KernelResult.Success)
                    {
                        if (MmuChangePermission(src, pagesCount, KMemoryPermission.ReadAndWrite) != KernelResult.Success)
                        {
                            throw new InvalidOperationException("Unexpected failure reverting memory permission.");
                        }

                        return result;
                    }

                    InsertBlock(src, pagesCount, srcState, KMemoryPermission.None, MemoryAttribute.Borrowed);
                    InsertBlock(dst, pagesCount, MemoryState.Stack, KMemoryPermission.ReadAndWrite);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult UnmapForKernel(ulong address, ulong pagesCount, MemoryState stateExpected)
        {
            ulong size = pagesCount * PageSize;

            lock (_blocks)
            {
                if (CheckRange(
                    address,
                    size,
                    MemoryState.Mask,
                    stateExpected,
                    KMemoryPermission.None,
                    KMemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out _,
                    out _,
                    out _))
                {
                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    KernelResult result = MmuUnmap(address, pagesCount);

                    if (result == KernelResult.Success)
                    {
                        InsertBlock(address, pagesCount, MemoryState.Unmapped);
                    }

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult Unmap(ulong dst, ulong src, ulong size)
        {
            bool success;

            lock (_blocks)
            {
                success = CheckRange(
                    src,
                    size,
                    MemoryState.MapAllowed,
                    MemoryState.MapAllowed,
                    KMemoryPermission.Mask,
                    KMemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.Borrowed,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState srcState,
                    out _,
                    out _);

                success &= CheckRange(
                    dst,
                    size,
                    MemoryState.Mask,
                    MemoryState.Stack,
                    KMemoryPermission.None,
                    KMemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out _,
                    out KMemoryPermission dstPermission,
                    out _);

                if (success)
                {
                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion * 2))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong pagesCount = size / PageSize;

                    KPageList srcPageList = new KPageList();
                    KPageList dstPageList = new KPageList();

                    AddVaRangeToPageList(srcPageList, src, pagesCount);
                    AddVaRangeToPageList(dstPageList, dst, pagesCount);

                    if (!dstPageList.IsEqual(srcPageList))
                    {
                        return KernelResult.InvalidMemRange;
                    }

                    KernelResult result = MmuUnmap(dst, pagesCount);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    result = MmuChangePermission(src, pagesCount, KMemoryPermission.ReadAndWrite);

                    if (result != KernelResult.Success)
                    {
                        MapPages(dst, dstPageList, dstPermission);

                        return result;
                    }

                    InsertBlock(src, pagesCount, srcState, KMemoryPermission.ReadAndWrite);
                    InsertBlock(dst, pagesCount, MemoryState.Unmapped);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult SetProcessMemoryPermission(ulong address, ulong size, KMemoryPermission permission)
        {
            lock (_blocks)
            {
                if (CheckRange(
                    address,
                    size,
                    MemoryState.ProcessPermissionChangeAllowed,
                    MemoryState.ProcessPermissionChangeAllowed,
                    KMemoryPermission.None,
                    KMemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState      oldState,
                    out KMemoryPermission oldPermission,
                    out _))
                {
                    MemoryState newState = oldState;

                    // If writing into the code region is allowed, then we need
                    // to change it to mutable.
                    if ((permission & KMemoryPermission.Write) != 0)
                    {
                        if (oldState == MemoryState.CodeStatic)
                        {
                            newState = MemoryState.CodeMutable;
                        }
                        else if (oldState == MemoryState.ModCodeStatic)
                        {
                            newState = MemoryState.ModCodeMutable;
                        }
                        else
                        {
                            throw new InvalidOperationException($"Memory state \"{oldState}\" not valid for this operation.");
                        }
                    }

                    if (newState != oldState || permission != oldPermission)
                    {
                        if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                        {
                            return KernelResult.OutOfResource;
                        }

                        ulong pagesCount = size / PageSize;

                        MemoryOperation operation = (permission & KMemoryPermission.Execute) != 0
                            ? MemoryOperation.ChangePermsAndAttributes
                            : MemoryOperation.ChangePermRw;

                        KernelResult result = DoMmuOperation(address, pagesCount, 0, false, permission, operation);

                        if (result != KernelResult.Success)
                        {
                            return result;
                        }

                        InsertBlock(address, pagesCount, newState, permission);
                    }

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult MapPhysicalMemory(ulong address, ulong size)
        {
            ulong endAddr = address + size;

            lock (_blocks)
            {
                ulong mappedSize = 0;

                foreach (KMemoryInfo info in IterateOverRange(address, endAddr))
                {
                    if (info.State != MemoryState.Unmapped)
                    {
                        mappedSize += GetSizeInRange(info, address, endAddr);
                    }
                }

                if (mappedSize == size)
                {
                    return KernelResult.Success;
                }

                ulong remainingSize = size - mappedSize;

                ulong remainingPages = remainingSize / PageSize;

                KProcess currentProcess = KernelStatic.GetCurrentProcess();

                if (currentProcess.ResourceLimit != null &&
                   !currentProcess.ResourceLimit.Reserve(LimitableResource.Memory, remainingSize))
                {
                    return KernelResult.ResLimitExceeded;
                }

                KMemoryRegionManager region = GetMemoryRegionManager();

                KernelResult result = region.AllocatePages(remainingPages, _aslrDisabled, out KPageList pageList);

                void CleanUpForError()
                {
                    if (pageList != null)
                    {
                        region.FreePages(pageList);
                    }

                    currentProcess.ResourceLimit?.Release(LimitableResource.Memory, remainingSize);
                }

                if (result != KernelResult.Success)
                {
                    CleanUpForError();

                    return result;
                }

                if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    CleanUpForError();

                    return KernelResult.OutOfResource;
                }

                MapPhysicalMemory(pageList, address, endAddr);

                PhysicalMemoryUsage += remainingSize;

                ulong pagesCount = size / PageSize;

                InsertBlock(
                    address,
                    pagesCount,
                    MemoryState.Unmapped,
                    KMemoryPermission.None,
                    MemoryAttribute.None,
                    MemoryState.Heap,
                    KMemoryPermission.ReadAndWrite,
                    MemoryAttribute.None);
            }

            return KernelResult.Success;
        }

        public KernelResult UnmapPhysicalMemory(ulong address, ulong size)
        {
            ulong endAddr = address + size;

            lock (_blocks)
            {
                // Scan, ensure that the region can be unmapped (all blocks are heap or
                // already unmapped), fill pages list for freeing memory.
                ulong heapMappedSize = 0;

                KPageList pageList = new KPageList();

                foreach (KMemoryInfo info in IterateOverRange(address, endAddr))
                {
                    if (info.State == MemoryState.Heap)
                    {
                        if (info.Attribute != MemoryAttribute.None)
                        {
                            return KernelResult.InvalidMemState;
                        }

                        ulong blockSize    = GetSizeInRange(info, address, endAddr);
                        ulong blockAddress = GetAddrInRange(info, address);

                        AddVaRangeToPageList(pageList, blockAddress, blockSize / PageSize);

                        heapMappedSize += blockSize;
                    }
                    else if (info.State != MemoryState.Unmapped)
                    {
                        return KernelResult.InvalidMemState;
                    }
                }

                if (heapMappedSize == 0)
                {
                    return KernelResult.Success;
                }

                if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                // Try to unmap all the heap mapped memory inside range.
                KernelResult result = KernelResult.Success;

                foreach (KMemoryInfo info in IterateOverRange(address, endAddr))
                {
                    if (info.State == MemoryState.Heap)
                    {
                        ulong blockSize    = GetSizeInRange(info, address, endAddr);
                        ulong blockAddress = GetAddrInRange(info, address);

                        ulong blockPagesCount = blockSize / PageSize;

                        result = MmuUnmap(blockAddress, blockPagesCount);

                        if (result != KernelResult.Success)
                        {
                            // If we failed to unmap, we need to remap everything back again.
                            MapPhysicalMemory(pageList, address, blockAddress + blockSize);

                            break;
                        }
                    }
                }

                if (result == KernelResult.Success)
                {
                    GetMemoryRegionManager().FreePages(pageList);

                    PhysicalMemoryUsage -= heapMappedSize;

                    KProcess currentProcess = KernelStatic.GetCurrentProcess();

                    currentProcess.ResourceLimit?.Release(LimitableResource.Memory, heapMappedSize);

                    ulong pagesCount = size / PageSize;

                    InsertBlock(address, pagesCount, MemoryState.Unmapped);
                }

                return result;
            }
        }

        private void MapPhysicalMemory(KPageList pageList, ulong address, ulong endAddr)
        {
            LinkedListNode<KPageNode> pageListNode = pageList.Nodes.First;

            KPageNode pageNode = pageListNode.Value;

            ulong srcPa      = pageNode.Address;
            ulong srcPaPages = pageNode.PagesCount;

            foreach (KMemoryInfo info in IterateOverRange(address, endAddr))
            {
                if (info.State == MemoryState.Unmapped)
                {
                    ulong blockSize = GetSizeInRange(info, address, endAddr);

                    ulong dstVaPages = blockSize / PageSize;

                    ulong dstVa = GetAddrInRange(info, address);

                    while (dstVaPages > 0)
                    {
                        if (srcPaPages == 0)
                        {
                            pageListNode = pageListNode.Next;

                            pageNode = pageListNode.Value;

                            srcPa      = pageNode.Address;
                            srcPaPages = pageNode.PagesCount;
                        }

                        ulong pagesCount = srcPaPages;

                        if (pagesCount > dstVaPages)
                        {
                            pagesCount = dstVaPages;
                        }

                        DoMmuOperation(
                            dstVa,
                            pagesCount,
                            srcPa,
                            true,
                            KMemoryPermission.ReadAndWrite,
                            MemoryOperation.MapPa);

                        dstVa      += pagesCount * PageSize;
                        srcPa      += pagesCount * PageSize;
                        srcPaPages -= pagesCount;
                        dstVaPages -= pagesCount;
                    }
                }
            }
        }

        public KernelResult CopyDataToCurrentProcess(
            ulong            dst,
            ulong            size,
            ulong            src,
            MemoryState      stateMask,
            MemoryState      stateExpected,
            KMemoryPermission permission,
            MemoryAttribute  attributeMask,
            MemoryAttribute  attributeExpected)
        {
            // Client -> server.
            return CopyDataFromOrToCurrentProcess(
                size,
                src,
                dst,
                stateMask,
                stateExpected,
                permission,
                attributeMask,
                attributeExpected,
                toServer: true);
        }

        public KernelResult CopyDataFromCurrentProcess(
            ulong            dst,
            ulong            size,
            MemoryState      stateMask,
            MemoryState      stateExpected,
            KMemoryPermission permission,
            MemoryAttribute  attributeMask,
            MemoryAttribute  attributeExpected,
            ulong            src)
        {
            // Server -> client.
            return CopyDataFromOrToCurrentProcess(
                size,
                dst,
                src,
                stateMask,
                stateExpected,
                permission,
                attributeMask,
                attributeExpected,
                toServer: false);
        }

        private KernelResult CopyDataFromOrToCurrentProcess(
            ulong            size,
            ulong            clientAddress,
            ulong            serverAddress,
            MemoryState      stateMask,
            MemoryState      stateExpected,
            KMemoryPermission permission,
            MemoryAttribute  attributeMask,
            MemoryAttribute  attributeExpected,
            bool             toServer)
        {
            if (AddrSpaceStart > clientAddress)
            {
                return KernelResult.InvalidMemState;
            }

            ulong srcEndAddr = clientAddress + size;

            if (srcEndAddr <= clientAddress || srcEndAddr - 1 > AddrSpaceEnd - 1)
            {
                return KernelResult.InvalidMemState;
            }

            lock (_blocks)
            {
                if (CheckRange(
                    clientAddress,
                    size,
                    stateMask,
                    stateExpected,
                    permission,
                    permission,
                    attributeMask | MemoryAttribute.Uncached,
                    attributeExpected))
                {
                    KProcess currentProcess = KernelStatic.GetCurrentProcess();

                    while (size > 0)
                    {
                        ulong copySize = Math.Min(PageSize - (serverAddress & (PageSize - 1)), PageSize - (clientAddress & (PageSize - 1)));

                        if (copySize > size)
                        {
                            copySize = size;
                        }

                        ulong serverDramAddr = currentProcess.MemoryManager.GetDramAddressFromVa(serverAddress);
                        ulong clientDramAddr = GetDramAddressFromVa(clientAddress);

                        if (serverDramAddr != clientDramAddr)
                        {
                            if (toServer)
                            {
                                _context.Memory.Copy(serverDramAddr, clientDramAddr, copySize);
                            }
                            else
                            {
                                _context.Memory.Copy(clientDramAddr, serverDramAddr, copySize);
                            }
                        }

                        serverAddress += copySize;
                        clientAddress += copySize;
                        size -= copySize;
                    }

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult MapBufferFromClientProcess(
            ulong            size,
            ulong            src,
            KMemoryManager   sourceMemMgr,
            KMemoryPermission permission,
            MemoryState      state,
            bool             copyData,
            out ulong        dst)
        {
            dst = 0;

            KernelResult result = sourceMemMgr.GetPagesForMappingIntoAnotherProcess(
                src,
                size,
                permission,
                state,
                copyData,
                _aslrDisabled,
                _memRegion,
                out KPageList pageList);

            if (result != KernelResult.Success)
            {
                return result;
            }

            result = MapPagesFromAnotherProcess(size, src, permission, state, pageList, out ulong va);

            if (result != KernelResult.Success)
            {
                sourceMemMgr.UnmapIpcRestorePermission(src, size, state);
            }
            else
            {
                dst = va;
            }

            return result;
        }

        private KernelResult GetPagesForMappingIntoAnotherProcess(
            ulong            address,
            ulong            size,
            KMemoryPermission permission,
            MemoryState      state,
            bool             copyData,
            bool             aslrDisabled,
            MemoryRegion     region,
            out KPageList    pageList)
        {
            pageList = null;

            if (AddrSpaceStart > address)
            {
                return KernelResult.InvalidMemState;
            }

            ulong endAddr = address + size;

            if (endAddr <= address || endAddr - 1 > AddrSpaceEnd - 1)
            {
                return KernelResult.InvalidMemState;
            }

            MemoryState stateMask;

            switch (state)
            {
                case MemoryState.IpcBuffer0: stateMask = MemoryState.IpcSendAllowedType0; break;
                case MemoryState.IpcBuffer1: stateMask = MemoryState.IpcSendAllowedType1; break;
                case MemoryState.IpcBuffer3: stateMask = MemoryState.IpcSendAllowedType3; break;

                default: return KernelResult.InvalidCombination;
            }

            KMemoryPermission permissionMask = permission == KMemoryPermission.ReadAndWrite
                ? KMemoryPermission.None
                : KMemoryPermission.Read;

            MemoryAttribute attributeMask = MemoryAttribute.Borrowed | MemoryAttribute.Uncached;

            if (state == MemoryState.IpcBuffer0)
            {
                attributeMask |= MemoryAttribute.DeviceMapped;
            }

            ulong addressRounded   = BitUtils.AlignUp  (address, PageSize);
            ulong addressTruncated = BitUtils.AlignDown(address, PageSize);
            ulong endAddrRounded   = BitUtils.AlignUp  (endAddr, PageSize);
            ulong endAddrTruncated = BitUtils.AlignDown(endAddr, PageSize);

            if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
            {
                return KernelResult.OutOfResource;
            }

            ulong visitedSize = 0;

            void CleanUpForError()
            {
                if (visitedSize == 0)
                {
                    return;
                }

                ulong endAddrVisited = address + visitedSize;

                foreach (KMemoryInfo info in IterateOverRange(addressRounded, endAddrVisited))
                {
                    if ((info.Permission & KMemoryPermission.ReadAndWrite) != permissionMask && info.IpcRefCount == 0)
                    {
                        ulong blockAddress = GetAddrInRange(info, addressRounded);
                        ulong blockSize    = GetSizeInRange(info, addressRounded, endAddrVisited);

                        ulong blockPagesCount = blockSize / PageSize;

                        if (DoMmuOperation(
                            blockAddress,
                            blockPagesCount,
                            0,
                            false,
                            info.Permission,
                            MemoryOperation.ChangePermRw) != KernelResult.Success)
                        {
                            throw new InvalidOperationException("Unexpected failure trying to restore permission.");
                        }
                    }
                }
            }

            // Signal a read for any resources tracking reads in the region, as the other process is likely to use their data.
            _cpuMemory.SignalMemoryTracking(addressTruncated, endAddrRounded - addressTruncated, false);

            lock (_blocks)
            {
                KernelResult result;

                if (addressRounded < endAddrTruncated)
                {
                    foreach (KMemoryInfo info in IterateOverRange(addressRounded, endAddrTruncated))
                    {
                        // Check if the block state matches what we expect.
                        if ((info.State      & stateMask)     != stateMask  ||
                            (info.Permission & permission)    != permission ||
                            (info.Attribute  & attributeMask) != MemoryAttribute.None)
                        {
                            CleanUpForError();

                            return KernelResult.InvalidMemState;
                        }

                        ulong blockAddress = GetAddrInRange(info, addressRounded);
                        ulong blockSize    = GetSizeInRange(info, addressRounded, endAddrTruncated);

                        ulong blockPagesCount = blockSize / PageSize;

                        if ((info.Permission & KMemoryPermission.ReadAndWrite) != permissionMask && info.IpcRefCount == 0)
                        {
                            result = DoMmuOperation(
                                blockAddress,
                                blockPagesCount,
                                0,
                                false,
                                permissionMask,
                                MemoryOperation.ChangePermRw);

                            if (result != KernelResult.Success)
                            {
                                CleanUpForError();

                                return result;
                            }
                        }

                        visitedSize += blockSize;
                    }
                }

                result = GetPagesForIpcTransfer(address, size, copyData, aslrDisabled, region, out pageList);

                if (result != KernelResult.Success)
                {
                    CleanUpForError();

                    return result;
                }

                if (visitedSize != 0)
                {
                    InsertBlock(addressRounded, visitedSize / PageSize, SetIpcMappingPermissions, permissionMask);
                }
            }

            return KernelResult.Success;
        }

        private KernelResult GetPagesForIpcTransfer(
            ulong         address,
            ulong         size,
            bool          copyData,
            bool          aslrDisabled,
            MemoryRegion  region,
            out KPageList pageList)
        {
            // When the start address is unaligned, we can't safely map the
            // first page as it would expose other undesirable information on the
            // target process. So, instead we allocate new pages, copy the data
            // inside the range, and then clear the remaining space.
            // The same also holds for the last page, if the end address
            // (address + size) is also not aligned.

            pageList = null;

            KPageList pages = new KPageList();

            ulong addressTruncated = BitUtils.AlignDown(address, PageSize);
            ulong addressRounded   = BitUtils.AlignUp  (address, PageSize);

            ulong endAddr = address + size;

            ulong dstFirstPagePa = 0;
            ulong dstLastPagePa  = 0;

            void CleanUpForError()
            {
                if (dstFirstPagePa != 0)
                {
                    FreeSinglePage(region, dstFirstPagePa);
                }

                if (dstLastPagePa != 0)
                {
                    FreeSinglePage(region, dstLastPagePa);
                }
            }

            // Is the first page address aligned?
            // If not, allocate a new page and copy the unaligned chunck.
            if (addressTruncated < addressRounded)
            {
                dstFirstPagePa = AllocateSinglePage(region, aslrDisabled);

                if (dstFirstPagePa == 0)
                {
                    return KernelResult.OutOfMemory;
                }

                ulong firstPageFillAddress = dstFirstPagePa;

                if (!TryConvertVaToPa(addressTruncated, out ulong srcFirstPagePa))
                {
                    CleanUpForError();

                    return KernelResult.InvalidMemState;
                }

                ulong unusedSizeAfter;

                if (copyData)
                {
                    ulong unusedSizeBefore = address - addressTruncated;

                    _context.Memory.ZeroFill(GetDramAddressFromPa(dstFirstPagePa), unusedSizeBefore);

                    ulong copySize = addressRounded <= endAddr ? addressRounded - address : size;

                    _context.Memory.Copy(
                        GetDramAddressFromPa(dstFirstPagePa + unusedSizeBefore),
                        GetDramAddressFromPa(srcFirstPagePa + unusedSizeBefore), copySize);

                    firstPageFillAddress += unusedSizeBefore + copySize;

                    unusedSizeAfter = addressRounded > endAddr ? addressRounded - endAddr : 0;
                }
                else
                {
                    unusedSizeAfter = PageSize;
                }

                if (unusedSizeAfter != 0)
                {
                    _context.Memory.ZeroFill(GetDramAddressFromPa(firstPageFillAddress), unusedSizeAfter);
                }

                if (pages.AddRange(dstFirstPagePa, 1) != KernelResult.Success)
                {
                    CleanUpForError();

                    return KernelResult.OutOfResource;
                }
            }

            ulong endAddrTruncated = BitUtils.AlignDown(endAddr, PageSize);
            ulong endAddrRounded   = BitUtils.AlignUp  (endAddr, PageSize);

            if (endAddrTruncated > addressRounded)
            {
                ulong alignedPagesCount = (endAddrTruncated - addressRounded) / PageSize;

                AddVaRangeToPageList(pages, addressRounded, alignedPagesCount);
            }

            // Is the last page end address aligned?
            // If not, allocate a new page and copy the unaligned chunck.
            if (endAddrTruncated < endAddrRounded && (addressTruncated == addressRounded || addressTruncated < endAddrTruncated))
            {
                dstLastPagePa = AllocateSinglePage(region, aslrDisabled);

                if (dstLastPagePa == 0)
                {
                    CleanUpForError();

                    return KernelResult.OutOfMemory;
                }

                ulong lastPageFillAddr = dstLastPagePa;

                if (!TryConvertVaToPa(endAddrTruncated, out ulong srcLastPagePa))
                {
                    CleanUpForError();

                    return KernelResult.InvalidMemState;
                }

                ulong unusedSizeAfter;

                if (copyData)
                {
                    ulong copySize = endAddr - endAddrTruncated;

                    _context.Memory.Copy(
                        GetDramAddressFromPa(dstLastPagePa),
                        GetDramAddressFromPa(srcLastPagePa), copySize);

                    lastPageFillAddr += copySize;

                    unusedSizeAfter = PageSize - copySize;
                }
                else
                {
                    unusedSizeAfter = PageSize;
                }

                _context.Memory.ZeroFill(GetDramAddressFromPa(lastPageFillAddr), unusedSizeAfter);

                if (pages.AddRange(dstLastPagePa, 1) != KernelResult.Success)
                {
                    CleanUpForError();

                    return KernelResult.OutOfResource;
                }
            }

            pageList = pages;

            return KernelResult.Success;
        }

        private ulong AllocateSinglePage(MemoryRegion region, bool aslrDisabled)
        {
            KMemoryRegionManager regionMgr = _context.MemoryRegions[(int)region];

            return regionMgr.AllocatePagesContiguous(1, aslrDisabled);
        }

        private void FreeSinglePage(MemoryRegion region, ulong address)
        {
            KMemoryRegionManager regionMgr = _context.MemoryRegions[(int)region];

            regionMgr.FreePage(address);
        }

        private KernelResult MapPagesFromAnotherProcess(
            ulong            size,
            ulong            address,
            KMemoryPermission permission,
            MemoryState      state,
            KPageList        pageList,
            out ulong        dst)
        {
            dst = 0;

            lock (_blocks)
            {
                if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                ulong endAddr = address + size;

                ulong addressTruncated = BitUtils.AlignDown(address, PageSize);
                ulong endAddrRounded   = BitUtils.AlignUp  (endAddr, PageSize);

                ulong neededSize = endAddrRounded - addressTruncated;

                ulong neededPagesCount = neededSize / PageSize;

                ulong regionPagesCount = (AliasRegionEnd - AliasRegionStart) / PageSize;

                ulong va = 0;

                for (int unit = MappingUnitSizes.Length - 1; unit >= 0 && va == 0; unit--)
                {
                    int alignment = MappingUnitSizes[unit];

                    va = AllocateVa(AliasRegionStart, regionPagesCount, neededPagesCount, alignment);
                }

                if (va == 0)
                {
                    return KernelResult.OutOfVaSpace;
                }

                if (pageList.Nodes.Count != 0)
                {
                    KernelResult result = MapPages(va, pageList, permission);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }
                }

                InsertBlock(va, neededPagesCount, state, permission);

                dst = va + (address - addressTruncated);
            }

            return KernelResult.Success;
        }

        public KernelResult UnmapNoAttributeIfStateEquals(ulong address, ulong size, MemoryState state)
        {
            if (AddrSpaceStart > address)
            {
                return KernelResult.InvalidMemState;
            }

            ulong endAddr = address + size;

            if (endAddr <= address || endAddr - 1 > AddrSpaceEnd - 1)
            {
                return KernelResult.InvalidMemState;
            }

            lock (_blocks)
            {
                if (CheckRange(
                    address,
                    size,
                    MemoryState.Mask,
                    state,
                    KMemoryPermission.Read,
                    KMemoryPermission.Read,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out _,
                    out _,
                    out _))
                {
                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong addressTruncated = BitUtils.AlignDown(address, PageSize);
                    ulong addressRounded   = BitUtils.AlignUp  (address, PageSize);
                    ulong endAddrTruncated = BitUtils.AlignDown(endAddr, PageSize);
                    ulong endAddrRounded   = BitUtils.AlignUp  (endAddr, PageSize);

                    ulong pagesCount = (endAddrRounded - addressTruncated) / PageSize;

                    // Free pages we had to create on-demand, if any of the buffer was not page aligned.
                    // Real kernel has page ref counting, so this is done as part of the unmap operation.
                    if (addressTruncated != addressRounded)
                    {
                        FreeSinglePage(_memRegion, ConvertVaToPa(addressTruncated));
                    }

                    if (endAddrTruncated < endAddrRounded && (addressTruncated == addressRounded || addressTruncated < endAddrTruncated))
                    {
                        FreeSinglePage(_memRegion, ConvertVaToPa(endAddrTruncated));
                    }

                    KernelResult result = DoMmuOperation(
                        addressTruncated,
                        pagesCount,
                        0,
                        false,
                        KMemoryPermission.None,
                        MemoryOperation.Unmap);

                    if (result == KernelResult.Success)
                    {
                        InsertBlock(addressTruncated, pagesCount, MemoryState.Unmapped);
                    }

                    return result;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult UnmapIpcRestorePermission(ulong address, ulong size, MemoryState state)
        {
            ulong endAddr = address + size;

            ulong addressRounded   = BitUtils.AlignUp  (address, PageSize);
            ulong addressTruncated = BitUtils.AlignDown(address, PageSize);
            ulong endAddrRounded   = BitUtils.AlignUp  (endAddr, PageSize);
            ulong endAddrTruncated = BitUtils.AlignDown(endAddr, PageSize);

            ulong pagesCount = addressRounded < endAddrTruncated ? (endAddrTruncated - addressRounded) / PageSize : 0;

            if (pagesCount == 0)
            {
                return KernelResult.Success;
            }

            MemoryState stateMask;

            switch (state)
            {
                case MemoryState.IpcBuffer0: stateMask = MemoryState.IpcSendAllowedType0; break;
                case MemoryState.IpcBuffer1: stateMask = MemoryState.IpcSendAllowedType1; break;
                case MemoryState.IpcBuffer3: stateMask = MemoryState.IpcSendAllowedType3; break;

                default: return KernelResult.InvalidCombination;
            }

            MemoryAttribute attributeMask =
                MemoryAttribute.Borrowed  |
                MemoryAttribute.IpcMapped |
                MemoryAttribute.Uncached;

            if (state == MemoryState.IpcBuffer0)
            {
                attributeMask |= MemoryAttribute.DeviceMapped;
            }

            if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
            {
                return KernelResult.OutOfResource;
            }

            // Anything on the client side should see this memory as modified.
            _cpuMemory.SignalMemoryTracking(addressTruncated, endAddrRounded - addressTruncated, true);

            lock (_blocks)
            {
                foreach (KMemoryInfo info in IterateOverRange(addressRounded, endAddrTruncated))
                {
                    // Check if the block state matches what we expect.
                    if ((info.State      & stateMask)     != stateMask ||
                        (info.Attribute  & attributeMask) != MemoryAttribute.IpcMapped)
                    {
                        return KernelResult.InvalidMemState;
                    }

                    if (info.Permission != info.SourcePermission && info.IpcRefCount == 1)
                    {
                        ulong blockAddress = GetAddrInRange(info, addressRounded);
                        ulong blockSize    = GetSizeInRange(info, addressRounded, endAddrTruncated);

                        ulong blockPagesCount = blockSize / PageSize;

                        KernelResult result = DoMmuOperation(
                            blockAddress,
                            blockPagesCount,
                            0,
                            false,
                            info.SourcePermission,
                            MemoryOperation.ChangePermRw);

                        if (result != KernelResult.Success)
                        {
                            return result;
                        }
                    }
                }

                InsertBlock(addressRounded, pagesCount, RestoreIpcMappingPermissions);

                return KernelResult.Success;
            }
        }

        public KernelResult BorrowIpcBuffer(ulong address, ulong size)
        {
            return SetAttributesAndChangePermission(
                address,
                size,
                MemoryState.IpcBufferAllowed,
                MemoryState.IpcBufferAllowed,
                KMemoryPermission.Mask,
                KMemoryPermission.ReadAndWrite,
                MemoryAttribute.Mask,
                MemoryAttribute.None,
                KMemoryPermission.None,
                MemoryAttribute.Borrowed);
        }

        public KernelResult BorrowTransferMemory(KPageList pageList, ulong address, ulong size, KMemoryPermission permission)
        {
            return SetAttributesAndChangePermission(
                address,
                size,
                MemoryState.TransferMemoryAllowed,
                MemoryState.TransferMemoryAllowed,
                KMemoryPermission.Mask,
                KMemoryPermission.ReadAndWrite,
                MemoryAttribute.Mask,
                MemoryAttribute.None,
                permission,
                MemoryAttribute.Borrowed,
                pageList);
        }

        private KernelResult SetAttributesAndChangePermission(
            ulong            address,
            ulong            size,
            MemoryState      stateMask,
            MemoryState      stateExpected,
            KMemoryPermission permissionMask,
            KMemoryPermission permissionExpected,
            MemoryAttribute  attributeMask,
            MemoryAttribute  attributeExpected,
            KMemoryPermission newPermission,
            MemoryAttribute  attributeSetMask,
            KPageList        pageList = null)
        {
            if (address + size <= address || !InsideAddrSpace(address, size))
            {
                return KernelResult.InvalidMemState;
            }

            lock (_blocks)
            {
                if (CheckRange(
                    address,
                    size,
                    stateMask     | MemoryState.IsPoolAllocated,
                    stateExpected | MemoryState.IsPoolAllocated,
                    permissionMask,
                    permissionExpected,
                    attributeMask,
                    attributeExpected,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState      oldState,
                    out KMemoryPermission oldPermission,
                    out MemoryAttribute  oldAttribute))
                {
                    ulong pagesCount = size / PageSize;

                    if (pageList != null)
                    {
                        AddVaRangeToPageList(pageList, address, pagesCount);
                    }

                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    if (newPermission == KMemoryPermission.None)
                    {
                        newPermission = oldPermission;
                    }

                    if (newPermission != oldPermission)
                    {
                        KernelResult result = DoMmuOperation(
                            address,
                            pagesCount,
                            0,
                            false,
                            newPermission,
                            MemoryOperation.ChangePermRw);

                        if (result != KernelResult.Success)
                        {
                            return result;
                        }
                    }

                    MemoryAttribute newAttribute = oldAttribute | attributeSetMask;

                    InsertBlock(address, pagesCount, oldState, newPermission, newAttribute);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult UnborrowIpcBuffer(ulong address, ulong size)
        {
            return ClearAttributesAndChangePermission(
                address,
                size,
                MemoryState.IpcBufferAllowed,
                MemoryState.IpcBufferAllowed,
                KMemoryPermission.None,
                KMemoryPermission.None,
                MemoryAttribute.Mask,
                MemoryAttribute.Borrowed,
                KMemoryPermission.ReadAndWrite,
                MemoryAttribute.Borrowed);
        }

        public KernelResult UnborrowTransferMemory(ulong address, ulong size, KPageList pageList)
        {
            return ClearAttributesAndChangePermission(
                address,
                size,
                MemoryState.TransferMemoryAllowed,
                MemoryState.TransferMemoryAllowed,
                KMemoryPermission.None,
                KMemoryPermission.None,
                MemoryAttribute.Mask,
                MemoryAttribute.Borrowed,
                KMemoryPermission.ReadAndWrite,
                MemoryAttribute.Borrowed,
                pageList);
        }

        private KernelResult ClearAttributesAndChangePermission(
            ulong            address,
            ulong            size,
            MemoryState      stateMask,
            MemoryState      stateExpected,
            KMemoryPermission permissionMask,
            KMemoryPermission permissionExpected,
            MemoryAttribute  attributeMask,
            MemoryAttribute  attributeExpected,
            KMemoryPermission newPermission,
            MemoryAttribute  attributeClearMask,
            KPageList        pageList = null)
        {
            if (address + size <= address || !InsideAddrSpace(address, size))
            {
                return KernelResult.InvalidMemState;
            }

            lock (_blocks)
            {
                if (CheckRange(
                    address,
                    size,
                    stateMask     | MemoryState.IsPoolAllocated,
                    stateExpected | MemoryState.IsPoolAllocated,
                    permissionMask,
                    permissionExpected,
                    attributeMask,
                    attributeExpected,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState      oldState,
                    out KMemoryPermission oldPermission,
                    out MemoryAttribute  oldAttribute))
                {
                    ulong pagesCount = size / PageSize;

                    if (pageList != null)
                    {
                        KPageList currPageList = new KPageList();

                        AddVaRangeToPageList(currPageList, address, pagesCount);

                        if (!currPageList.IsEqual(pageList))
                        {
                            return KernelResult.InvalidMemRange;
                        }
                    }

                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    if (newPermission == KMemoryPermission.None)
                    {
                        newPermission = oldPermission;
                    }

                    if (newPermission != oldPermission)
                    {
                        KernelResult result = DoMmuOperation(
                            address,
                            pagesCount,
                            0,
                            false,
                            newPermission,
                            MemoryOperation.ChangePermRw);

                        if (result != KernelResult.Success)
                        {
                            return result;
                        }
                    }

                    MemoryAttribute newAttribute = oldAttribute & ~attributeClearMask;

                    InsertBlock(address, pagesCount, oldState, newPermission, newAttribute);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        private void AddVaRangeToPageList(KPageList pageList, ulong start, ulong pagesCount)
        {
            ulong address = start;

            while (address < start + pagesCount * PageSize)
            {
                if (!TryConvertVaToPa(address, out ulong pa))
                {
                    throw new InvalidOperationException("Unexpected failure translating virtual address.");
                }

                pageList.AddRange(pa, 1);

                address += PageSize;
            }
        }

        private static ulong GetAddrInRange(KMemoryInfo info, ulong start)
        {
            if (info.Address < start)
            {
                return start;
            }

            return info.Address;
        }

        private static ulong GetSizeInRange(KMemoryInfo info, ulong start, ulong end)
        {
            ulong endAddr = info.Size + info.Address;
            ulong size    = info.Size;

            if (info.Address < start)
            {
                size -= start - info.Address;
            }

            if (endAddr > end)
            {
                size -= endAddr - end;
            }

            return size;
        }

        private bool IsUnmapped(ulong address, ulong size)
        {
            return CheckRange(
                address,
                size,
                MemoryState.Mask,
                MemoryState.Unmapped,
                KMemoryPermission.Mask,
                KMemoryPermission.None,
                MemoryAttribute.Mask,
                MemoryAttribute.None,
                MemoryAttribute.IpcAndDeviceMapped,
                out _,
                out _,
                out _);
        }

        private bool CheckRange(
            ulong                address,
            ulong                size,
            MemoryState          stateMask,
            MemoryState          stateExpected,
            KMemoryPermission     permissionMask,
            KMemoryPermission     permissionExpected,
            MemoryAttribute      attributeMask,
            MemoryAttribute      attributeExpected,
            MemoryAttribute      attributeIgnoreMask,
            out MemoryState      outState,
            out KMemoryPermission outPermission,
            out MemoryAttribute  outAttribute)
        {
            ulong endAddr = address + size;

            LinkedListNode<KMemoryBlock> node = FindBlockNode(address);

            KMemoryInfo info = node.Value.GetInfo();

            MemoryState      firstState      = info.State;
            KMemoryPermission firstPermission = info.Permission;
            MemoryAttribute  firstAttribute  = info.Attribute;

            do
            {
                info = node.Value.GetInfo();

                // Check if the block state matches what we expect.
                if ( firstState                             != info.State                             ||
                     firstPermission                        != info.Permission                        ||
                    (info.Attribute  & attributeMask)       != attributeExpected                      ||
                    (firstAttribute  | attributeIgnoreMask) != (info.Attribute | attributeIgnoreMask) ||
                    (firstState      & stateMask)           != stateExpected                          ||
                    (firstPermission & permissionMask)      != permissionExpected)
                {
                    outState      = MemoryState.Unmapped;
                    outPermission = KMemoryPermission.None;
                    outAttribute  = MemoryAttribute.None;

                    return false;
                }
            }
            while (info.Address + info.Size - 1 < endAddr - 1 && (node = node.Next) != null);

            outState      = firstState;
            outPermission = firstPermission;
            outAttribute  = firstAttribute & ~attributeIgnoreMask;

            return true;
        }

        private bool CheckRange(
            ulong            address,
            ulong            size,
            MemoryState      stateMask,
            MemoryState      stateExpected,
            KMemoryPermission permissionMask,
            KMemoryPermission permissionExpected,
            MemoryAttribute  attributeMask,
            MemoryAttribute  attributeExpected)
        {
            foreach (KMemoryInfo info in IterateOverRange(address, address + size))
            {
                // Check if the block state matches what we expect.
                if ((info.State      & stateMask)      != stateExpected      ||
                    (info.Permission & permissionMask) != permissionExpected ||
                    (info.Attribute  & attributeMask)  != attributeExpected)
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerable<KMemoryInfo> IterateOverRange(ulong start, ulong end)
        {
            LinkedListNode<KMemoryBlock> node = FindBlockNode(start);

            KMemoryInfo info;

            do
            {
                info = node.Value.GetInfo();

                yield return info;
            }
            while (info.Address + info.Size - 1 < end - 1 && (node = node.Next) != null);
        }

        private void InsertBlock(
            ulong            baseAddress,
            ulong            pagesCount,
            MemoryState      oldState,
            KMemoryPermission oldPermission,
            MemoryAttribute  oldAttribute,
            MemoryState      newState,
            KMemoryPermission newPermission,
            MemoryAttribute  newAttribute)
        {
            // Insert new block on the list only on areas where the state
            // of the block matches the state specified on the old* state
            // arguments, otherwise leave it as is.
            int oldCount = _blocks.Count;

            oldAttribute |= MemoryAttribute.IpcAndDeviceMapped;

            ulong endAddr = baseAddress + pagesCount * PageSize;

            LinkedListNode<KMemoryBlock> node = _blocks.First;

            while (node != null)
            {
                LinkedListNode<KMemoryBlock> newNode = node;

                KMemoryBlock currBlock = node.Value;

                ulong currBaseAddr = currBlock.BaseAddress;
                ulong currEndAddr  = currBlock.PagesCount * PageSize + currBaseAddr;

                if (baseAddress < currEndAddr && currBaseAddr < endAddr)
                {
                    MemoryAttribute currBlockAttr = currBlock.Attribute | MemoryAttribute.IpcAndDeviceMapped;

                    if (currBlock.State      != oldState      ||
                        currBlock.Permission != oldPermission ||
                        currBlockAttr        != oldAttribute)
                    {
                        node = node.Next;

                        continue;
                    }

                    if (baseAddress > currBaseAddr)
                    {
                        _blocks.AddBefore(node, currBlock.SplitRightAtAddress(baseAddress));
                    }

                    if (endAddr < currEndAddr)
                    {
                        newNode = _blocks.AddBefore(node, currBlock.SplitRightAtAddress(endAddr));
                    }

                    newNode.Value.SetState(newPermission, newState, newAttribute);

                    newNode = MergeEqualStateNeighbors(newNode);
                }

                if (currEndAddr - 1 >= endAddr - 1)
                {
                    break;
                }

                node = newNode.Next;
            }

            _blockAllocator.Count += _blocks.Count - oldCount;

            ValidateInternalState();
        }

        private void InsertBlock(
            ulong            baseAddress,
            ulong            pagesCount,
            MemoryState      state,
            KMemoryPermission permission = KMemoryPermission.None,
            MemoryAttribute  attribute  = MemoryAttribute.None)
        {
            // Inserts new block at the list, replacing and splitting
            // existing blocks as needed.
            int oldCount = _blocks.Count;

            ulong endAddr = baseAddress + pagesCount * PageSize;

            LinkedListNode<KMemoryBlock> node = _blocks.First;

            while (node != null)
            {
                LinkedListNode<KMemoryBlock> newNode = node;

                KMemoryBlock currBlock = node.Value;

                ulong currBaseAddr = currBlock.BaseAddress;
                ulong currEndAddr  = currBlock.PagesCount * PageSize + currBaseAddr;

                if (baseAddress < currEndAddr && currBaseAddr < endAddr)
                {
                    if (baseAddress > currBaseAddr)
                    {
                        _blocks.AddBefore(node, currBlock.SplitRightAtAddress(baseAddress));
                    }

                    if (endAddr < currEndAddr)
                    {
                        newNode = _blocks.AddBefore(node, currBlock.SplitRightAtAddress(endAddr));
                    }

                    newNode.Value.SetState(permission, state, attribute);

                    newNode = MergeEqualStateNeighbors(newNode);
                }

                if (currEndAddr - 1 >= endAddr - 1)
                {
                    break;
                }

                node = newNode.Next;
            }

            _blockAllocator.Count += _blocks.Count - oldCount;

            ValidateInternalState();
        }

        private static void SetIpcMappingPermissions(KMemoryBlock block, KMemoryPermission permission)
        {
            block.SetIpcMappingPermission(permission);
        }

        private static void RestoreIpcMappingPermissions(KMemoryBlock block, KMemoryPermission permission)
        {
            block.RestoreIpcMappingPermission();
        }

        private delegate void BlockMutator(KMemoryBlock block, KMemoryPermission newPerm);

        private void InsertBlock(
            ulong            baseAddress,
            ulong            pagesCount,
            BlockMutator     blockMutate,
            KMemoryPermission permission = KMemoryPermission.None)
        {
            // Inserts new block at the list, replacing and splitting
            // existing blocks as needed, then calling the callback
            // function on the new block.
            int oldCount = _blocks.Count;

            ulong endAddr = baseAddress + pagesCount * PageSize;

            LinkedListNode<KMemoryBlock> node = _blocks.First;

            while (node != null)
            {
                LinkedListNode<KMemoryBlock> newNode = node;

                KMemoryBlock currBlock = node.Value;

                ulong currBaseAddr = currBlock.BaseAddress;
                ulong currEndAddr  = currBlock.PagesCount * PageSize + currBaseAddr;

                if (baseAddress < currEndAddr && currBaseAddr < endAddr)
                {
                    if (baseAddress > currBaseAddr)
                    {
                        _blocks.AddBefore(node, currBlock.SplitRightAtAddress(baseAddress));
                    }

                    if (endAddr < currEndAddr)
                    {
                        newNode = _blocks.AddBefore(node, currBlock.SplitRightAtAddress(endAddr));
                    }

                    KMemoryBlock newBlock = newNode.Value;

                    blockMutate(newBlock, permission);

                    newNode = MergeEqualStateNeighbors(newNode);
                }

                if (currEndAddr - 1 >= endAddr - 1)
                {
                    break;
                }

                node = newNode.Next;
            }

            _blockAllocator.Count += _blocks.Count - oldCount;

            ValidateInternalState();
        }

        [Conditional("DEBUG")]
        private void ValidateInternalState()
        {
            ulong expectedAddress = 0;

            LinkedListNode<KMemoryBlock> node = _blocks.First;

            while (node != null)
            {
                LinkedListNode<KMemoryBlock> newNode = node;

                KMemoryBlock currBlock = node.Value;

                Debug.Assert(currBlock.BaseAddress == expectedAddress);

                expectedAddress = currBlock.BaseAddress + currBlock.PagesCount * PageSize;

                node = newNode.Next;
            }

            Debug.Assert(expectedAddress == AddrSpaceEnd);
        }

        private LinkedListNode<KMemoryBlock> MergeEqualStateNeighbors(LinkedListNode<KMemoryBlock> node)
        {
            KMemoryBlock block = node.Value;

            if (node.Previous != null)
            {
                KMemoryBlock previousBlock = node.Previous.Value;

                if (BlockStateEquals(block, previousBlock))
                {
                    LinkedListNode<KMemoryBlock> previousNode = node.Previous;

                    _blocks.Remove(node);

                    previousBlock.AddPages(block.PagesCount);

                    node  = previousNode;
                    block = previousBlock;
                }
            }

            if (node.Next != null)
            {
                KMemoryBlock nextBlock = node.Next.Value;

                if (BlockStateEquals(block, nextBlock))
                {
                    _blocks.Remove(node.Next);

                    block.AddPages(nextBlock.PagesCount);
                }
            }

            return node;
        }

        private static bool BlockStateEquals(KMemoryBlock lhs, KMemoryBlock rhs)
        {
            return lhs.State            == rhs.State            &&
                   lhs.Permission       == rhs.Permission       &&
                   lhs.Attribute        == rhs.Attribute        &&
                   lhs.SourcePermission == rhs.SourcePermission &&
                   lhs.DeviceRefCount   == rhs.DeviceRefCount   &&
                   lhs.IpcRefCount      == rhs.IpcRefCount;
        }

        private ulong AllocateVa(
            ulong regionStart,
            ulong regionPagesCount,
            ulong neededPagesCount,
            int   alignment)
        {
            ulong address = 0;

            ulong regionEndAddr = regionStart + regionPagesCount * PageSize;

            ulong reservedPagesCount = _isKernel ? 1UL : 4UL;

            if (_aslrEnabled)
            {
                ulong totalNeededSize = (reservedPagesCount + neededPagesCount) * PageSize;

                ulong remainingPages = regionPagesCount - neededPagesCount;

                ulong aslrMaxOffset = ((remainingPages + reservedPagesCount) * PageSize) / (ulong)alignment;

                for (int attempt = 0; attempt < 8; attempt++)
                {
                    address = BitUtils.AlignDown(regionStart + GetRandomValue(0, aslrMaxOffset) * (ulong)alignment, alignment);

                    ulong endAddr = address + totalNeededSize;

                    KMemoryInfo info = FindBlock(address).GetInfo();

                    if (info.State != MemoryState.Unmapped)
                    {
                        continue;
                    }

                    ulong currBaseAddr = info.Address + reservedPagesCount * PageSize;
                    ulong currEndAddr  = info.Address + info.Size;

                    if (address     >= regionStart       &&
                        address     >= currBaseAddr      &&
                        endAddr - 1 <= regionEndAddr - 1 &&
                        endAddr - 1 <= currEndAddr   - 1)
                    {
                        break;
                    }
                }

                if (address == 0)
                {
                    ulong aslrPage = GetRandomValue(0, aslrMaxOffset);

                    address = FindFirstFit(
                        regionStart      + aslrPage * PageSize,
                        regionPagesCount - aslrPage,
                        neededPagesCount,
                        alignment,
                        0,
                        reservedPagesCount);
                }
            }

            if (address == 0)
            {
                address = FindFirstFit(
                    regionStart,
                    regionPagesCount,
                    neededPagesCount,
                    alignment,
                    0,
                    reservedPagesCount);
            }

            return address;
        }

        private ulong FindFirstFit(
            ulong regionStart,
            ulong regionPagesCount,
            ulong neededPagesCount,
            int   alignment,
            ulong reservedStart,
            ulong reservedPagesCount)
        {
            ulong reservedSize = reservedPagesCount * PageSize;

            ulong totalNeededSize = reservedSize + neededPagesCount * PageSize;

            ulong regionEndAddr = regionStart + regionPagesCount * PageSize;

            LinkedListNode<KMemoryBlock> node = FindBlockNode(regionStart);

            KMemoryInfo info = node.Value.GetInfo();

            while (regionEndAddr >= info.Address)
            {
                if (info.State == MemoryState.Unmapped)
                {
                    ulong currBaseAddr = info.Address + reservedSize;
                    ulong currEndAddr  = info.Address + info.Size - 1;

                    ulong address = BitUtils.AlignDown(currBaseAddr, alignment) + reservedStart;

                    if (currBaseAddr > address)
                    {
                        address += (ulong)alignment;
                    }

                    ulong allocationEndAddr = address + totalNeededSize - 1;

                    if (allocationEndAddr <= regionEndAddr &&
                        allocationEndAddr <= currEndAddr   &&
                        address           <  allocationEndAddr)
                    {
                        return address;
                    }
                }

                node = node.Next;

                if (node == null)
                {
                    break;
                }

                info = node.Value.GetInfo();
            }

            return 0;
        }

        private KMemoryBlock FindBlock(ulong address)
        {
            return FindBlockNode(address)?.Value;
        }

        private LinkedListNode<KMemoryBlock> FindBlockNode(ulong address)
        {
            lock (_blocks)
            {
                LinkedListNode<KMemoryBlock> node = _blocks.First;

                while (node != null)
                {
                    KMemoryBlock block = node.Value;

                    ulong currEndAddr = block.PagesCount * PageSize + block.BaseAddress;

                    if (block.BaseAddress <= address && currEndAddr - 1 >= address)
                    {
                        return node;
                    }

                    node = node.Next;
                }
            }

            return null;
        }

        public bool CanContain(ulong address, ulong size, MemoryState state)
        {
            ulong endAddr = address + size;

            ulong regionBaseAddr = GetBaseAddress(state);
            ulong regionEndAddr = regionBaseAddr + GetSize(state);

            bool InsideRegion()
            {
                return regionBaseAddr <= address &&
                       endAddr        >  address &&
                       endAddr - 1    <= regionEndAddr - 1;
            }

            bool OutsideHeapRegion() => endAddr <= HeapRegionStart ||  address >= HeapRegionEnd;
            bool OutsideAliasRegion() => endAddr <= AliasRegionStart || address >= AliasRegionEnd;

            switch (state)
            {
                case MemoryState.Io:
                case MemoryState.Normal:
                case MemoryState.CodeStatic:
                case MemoryState.CodeMutable:
                case MemoryState.SharedMemory:
                case MemoryState.ModCodeStatic:
                case MemoryState.ModCodeMutable:
                case MemoryState.Stack:
                case MemoryState.ThreadLocal:
                case MemoryState.TransferMemoryIsolated:
                case MemoryState.TransferMemory:
                case MemoryState.ProcessMemory:
                case MemoryState.CodeReadOnly:
                case MemoryState.CodeWritable:
                    return InsideRegion() && OutsideHeapRegion() && OutsideAliasRegion();

                case MemoryState.Heap:
                    return InsideRegion() && OutsideAliasRegion();

                case MemoryState.IpcBuffer0:
                case MemoryState.IpcBuffer1:
                case MemoryState.IpcBuffer3:
                    return InsideRegion() && OutsideHeapRegion();

                case MemoryState.KernelStack:
                    return InsideRegion();
            }

            throw new ArgumentException($"Invalid state value \"{state}\".");
        }

        private ulong GetBaseAddress(MemoryState state)
        {
            switch (state)
            {
                case MemoryState.Io:
                case MemoryState.Normal:
                case MemoryState.ThreadLocal:
                    return TlsIoRegionStart;

                case MemoryState.CodeStatic:
                case MemoryState.CodeMutable:
                case MemoryState.SharedMemory:
                case MemoryState.ModCodeStatic:
                case MemoryState.ModCodeMutable:
                case MemoryState.TransferMemoryIsolated:
                case MemoryState.TransferMemory:
                case MemoryState.ProcessMemory:
                case MemoryState.CodeReadOnly:
                case MemoryState.CodeWritable:
                    return GetAddrSpaceBaseAddr();

                case MemoryState.Heap:
                    return HeapRegionStart;

                case MemoryState.IpcBuffer0:
                case MemoryState.IpcBuffer1:
                case MemoryState.IpcBuffer3:
                    return AliasRegionStart;

                case MemoryState.Stack:
                    return StackRegionStart;

                case MemoryState.KernelStack:
                    return AddrSpaceStart;
            }

            throw new ArgumentException($"Invalid state value \"{state}\".");
        }

        private ulong GetSize(MemoryState state)
        {
            switch (state)
            {
                case MemoryState.Io:
                case MemoryState.Normal:
                case MemoryState.ThreadLocal:
                    return TlsIoRegionEnd - TlsIoRegionStart;

                case MemoryState.CodeStatic:
                case MemoryState.CodeMutable:
                case MemoryState.SharedMemory:
                case MemoryState.ModCodeStatic:
                case MemoryState.ModCodeMutable:
                case MemoryState.TransferMemoryIsolated:
                case MemoryState.TransferMemory:
                case MemoryState.ProcessMemory:
                case MemoryState.CodeReadOnly:
                case MemoryState.CodeWritable:
                    return GetAddrSpaceSize();

                case MemoryState.Heap:
                    return HeapRegionEnd - HeapRegionStart;

                case MemoryState.IpcBuffer0:
                case MemoryState.IpcBuffer1:
                case MemoryState.IpcBuffer3:
                    return AliasRegionEnd - AliasRegionStart;

                case MemoryState.Stack:
                    return StackRegionEnd - StackRegionStart;

                case MemoryState.KernelStack:
                    return AddrSpaceEnd - AddrSpaceStart;
            }

            throw new ArgumentException($"Invalid state value \"{state}\".");
        }

        public ulong GetAddrSpaceBaseAddr()
        {
            if (AddrSpaceWidth == 36 || AddrSpaceWidth == 39)
            {
                return 0x8000000;
            }
            else if (AddrSpaceWidth == 32)
            {
                return 0x200000;
            }
            else
            {
                throw new InvalidOperationException("Invalid address space width!");
            }
        }

        public ulong GetAddrSpaceSize()
        {
            if (AddrSpaceWidth == 36)
            {
                return 0xff8000000;
            }
            else if (AddrSpaceWidth == 39)
            {
                return 0x7ff8000000;
            }
            else if (AddrSpaceWidth == 32)
            {
                return 0xffe00000;
            }
            else
            {
                throw new InvalidOperationException("Invalid address space width!");
            }
        }

        private KernelResult MapPages(ulong address, KPageList pageList, KMemoryPermission permission)
        {
            ulong currAddr = address;

            KernelResult result = KernelResult.Success;

            foreach (KPageNode pageNode in pageList)
            {
                result = DoMmuOperation(
                    currAddr,
                    pageNode.PagesCount,
                    pageNode.Address,
                    true,
                    permission,
                    MemoryOperation.MapPa);

                if (result != KernelResult.Success)
                {
                    KMemoryInfo info = FindBlock(currAddr).GetInfo();

                    ulong pagesCount = (address - currAddr) / PageSize;

                    result = MmuUnmap(address, pagesCount);

                    break;
                }

                currAddr += pageNode.PagesCount * PageSize;
            }

            return result;
        }

        private KernelResult MmuUnmap(ulong address, ulong pagesCount)
        {
            return DoMmuOperation(
                address,
                pagesCount,
                0,
                false,
                KMemoryPermission.None,
                MemoryOperation.Unmap);
        }

        private KernelResult MmuChangePermission(ulong address, ulong pagesCount, KMemoryPermission permission)
        {
            return DoMmuOperation(
                address,
                pagesCount,
                0,
                false,
                permission,
                MemoryOperation.ChangePermRw);
        }

        private KernelResult DoMmuOperation(
            ulong            dstVa,
            ulong            pagesCount,
            ulong            srcPa,
            bool             map,
            KMemoryPermission permission,
            MemoryOperation  operation)
        {
            if (map != (operation == MemoryOperation.MapPa))
            {
                throw new ArgumentException(nameof(map) + " value is invalid for this operation.");
            }

            KernelResult result;

            switch (operation)
            {
                case MemoryOperation.MapPa:
                {
                    ulong size = pagesCount * PageSize;

                    _cpuMemory.Map(dstVa, srcPa - DramMemoryMap.DramBase, size);

                    result = KernelResult.Success;

                    break;
                }

                case MemoryOperation.Allocate:
                {
                    KMemoryRegionManager region = GetMemoryRegionManager();

                    result = region.AllocatePages(pagesCount, _aslrDisabled, out KPageList pageList);

                    if (result == KernelResult.Success)
                    {
                        result = MmuMapPages(dstVa, pageList);
                    }

                    break;
                }

                case MemoryOperation.Unmap:
                {
                    ulong size = pagesCount * PageSize;

                    _cpuMemory.Unmap(dstVa, size);

                    result = KernelResult.Success;

                    break;
                }

                case MemoryOperation.ChangePermRw:             result = KernelResult.Success; break;
                case MemoryOperation.ChangePermsAndAttributes: result = KernelResult.Success; break;

                default: throw new ArgumentException($"Invalid operation \"{operation}\".");
            }

            return result;
        }

        private KernelResult DoMmuOperation(
            ulong            address,
            ulong            pagesCount,
            KPageList        pageList,
            KMemoryPermission permission,
            MemoryOperation  operation)
        {
            if (operation != MemoryOperation.MapVa)
            {
                throw new ArgumentException($"Invalid memory operation \"{operation}\" specified.");
            }

            return MmuMapPages(address, pageList);
        }

        private KMemoryRegionManager GetMemoryRegionManager()
        {
            return _context.MemoryRegions[(int)_memRegion];
        }

        private KernelResult MmuMapPages(ulong address, KPageList pageList)
        {
            foreach (KPageNode pageNode in pageList)
            {
                ulong size = pageNode.PagesCount * PageSize;

                _cpuMemory.Map(address, pageNode.Address - DramMemoryMap.DramBase, size);

                address += size;
            }

            return KernelResult.Success;
        }

        public ulong GetDramAddressFromVa(ulong va)
        {
            return _cpuMemory.GetPhysicalAddress(va);
        }

        public ulong ConvertVaToPa(ulong va)
        {
            if (!TryConvertVaToPa(va, out ulong pa))
            {
                throw new ArgumentException($"Invalid virtual address 0x{va:X} specified.");
            }

            return pa;
        }

        public bool TryConvertVaToPa(ulong va, out ulong pa)
        {
            pa = DramMemoryMap.DramBase + _cpuMemory.GetPhysicalAddress(va);

            return true;
        }

        public static ulong GetDramAddressFromPa(ulong pa)
        {
            return pa - DramMemoryMap.DramBase;
        }

        public long GetMmUsedPages()
        {
            lock (_blocks)
            {
                return BitUtils.DivRoundUp(GetMmUsedSize(), PageSize);
            }
        }

        private long GetMmUsedSize()
        {
            return _blocks.Count * KMemoryBlockSize;
        }

        public bool IsInvalidRegion(ulong address, ulong size)
        {
            return address + size - 1 > GetAddrSpaceBaseAddr() + GetAddrSpaceSize() - 1;
        }

        public bool InsideAddrSpace(ulong address, ulong size)
        {
            return AddrSpaceStart <= address && address + size - 1 <= AddrSpaceEnd - 1;
        }

        public bool InsideAliasRegion(ulong address, ulong size)
        {
            return address + size > AliasRegionStart && AliasRegionEnd > address;
        }

        public bool InsideHeapRegion(ulong address, ulong size)
        {
            return address + size > HeapRegionStart && HeapRegionEnd > address;
        }

        public bool InsideStackRegion(ulong address, ulong size)
        {
            return address + size > StackRegionStart && StackRegionEnd > address;
        }

        public bool OutsideAliasRegion(ulong address, ulong size)
        {
            return AliasRegionStart > address || address + size - 1 > AliasRegionEnd - 1;
        }

        public bool OutsideAddrSpace(ulong address, ulong size)
        {
            return AddrSpaceStart > address || address + size - 1 > AddrSpaceEnd - 1;
        }

        public bool OutsideStackRegion(ulong address, ulong size)
        {
            return StackRegionStart > address || address + size - 1 > StackRegionEnd - 1;
        }
    }
}