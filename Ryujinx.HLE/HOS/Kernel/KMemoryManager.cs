using ChocolArm64.Memory;
using Ryujinx.Common;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KMemoryManager
    {
        public const int PageSize = 0x1000;

        private const int KMemoryBlockSize = 0x40;

        //We need 2 blocks for the case where a big block
        //needs to be split in 2, plus one block that will be the new one inserted.
        private const int MaxBlocksNeededForInsertion = 2;

        private LinkedList<KMemoryBlock> _blocks;

        private MemoryManager _cpuMemory;

        private Horizon _system;

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

        public KMemoryManager(Horizon system, MemoryManager cpuMemory)
        {
            _system    = system;
            _cpuMemory = cpuMemory;

            _blocks = new LinkedList<KMemoryBlock>();
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

            _contextId = _system.ContextIdManager.GetId();

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
                _system.ContextIdManager.PutId(_contextId);
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
                //Has more space before the start of the code region.
                mapBaseAddress   = baseAddress;
                mapAvailableSize = CodeRegionStart - baseAddress;
            }
            else
            {
                //Has more space after the end of the code region.
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

            //Regions are sorted based on ASLR offset.
            //When ASLR is disabled, the order is Map, Heap, NewMap and TlsIo.
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
            //First insertion will always need only a single block,
            //because there's nothing else to split.
            if (!_blockAllocator.CanAllocate(1))
            {
                return KernelResult.OutOfResource;
            }

            ulong addrSpacePagesCount = (addrSpaceEnd - addrSpaceStart) / PageSize;

            InsertBlock(addrSpaceStart, addrSpacePagesCount, MemoryState.Unmapped);

            return KernelResult.Success;
        }

        public KernelResult MapPages(
            ulong            address,
            KPageList        pageList,
            MemoryState      state,
            MemoryPermission permission)
        {
            ulong pagesCount = pageList.GetPagesCount();

            ulong size = pagesCount * PageSize;

            if (!ValidateRegionForState(address, size, state))
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
                    MemoryPermission.None,
                    MemoryPermission.None,
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

        public KernelResult MapNormalMemory(long address, long size, MemoryPermission permission)
        {
            //TODO.
            return KernelResult.Success;
        }

        public KernelResult MapIoMemory(long address, long size, MemoryPermission permission)
        {
            //TODO.
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
            MemoryPermission permission,
            out ulong        address)
        {
            address = 0;

            ulong regionSize = regionPagesCount * PageSize;

            ulong regionEndAddr = regionStart + regionSize;

            if (!ValidateRegionForState(regionStart, regionSize, state))
            {
                return KernelResult.InvalidMemState;
            }

            if (regionPagesCount <= neededPagesCount)
            {
                return KernelResult.OutOfMemory;
            }

            ulong reservedPagesCount = _isKernel ? 1UL : 4UL;

            lock (_blocks)
            {
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
            MemoryPermission permission)
        {
            ulong size = pagesCount * PageSize;

            if (!ValidateRegionForState(address, size, state))
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
                    MemoryPermission.Mask,
                    MemoryPermission.ReadAndWrite,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState      state,
                    out MemoryPermission permission,
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

                    KernelResult result = MmuChangePermission(src, pagesCount, MemoryPermission.None);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    result = MapPages(dst, pageList, MemoryPermission.None);

                    if (result != KernelResult.Success)
                    {
                        MmuChangePermission(src, pagesCount, permission);

                        return result;
                    }

                    InsertBlock(src, pagesCount, state, MemoryPermission.None, MemoryAttribute.Borrowed);
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
                    MemoryPermission.None,
                    MemoryPermission.None,
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
                    MemoryPermission.None,
                    MemoryPermission.None,
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
                    MemoryPermission.None,
                    MemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None);

                if (success)
                {
                    KernelResult result = MmuUnmap(dst, pagesCount);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    //TODO: Missing some checks here.

                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion * 2))
                    {
                        return KernelResult.OutOfResource;
                    }

                    InsertBlock(dst, pagesCount, MemoryState.Unmapped);
                    InsertBlock(src, pagesCount, MemoryState.Heap, MemoryPermission.ReadAndWrite);

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

            KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

            ulong currentHeapSize = GetHeapSize();

            if (currentHeapSize <= size)
            {
                //Expand.
                ulong diffSize = size - currentHeapSize;

                lock (_blocks)
                {
                    if (currentProcess.ResourceLimit != null && diffSize != 0 &&
                       !currentProcess.ResourceLimit.Reserve(LimitableResource.Memory, diffSize))
                    {
                        return KernelResult.ResLimitExceeded;
                    }

                    ulong pagesCount = diffSize / PageSize;

                    KMemoryRegionManager region = GetMemoryRegionManager();

                    KernelResult result = region.AllocatePages(pagesCount, _aslrDisabled, out KPageList pageList);

                    void CleanUpForError()
                    {
                        if (pageList != null)
                        {
                            region.FreePages(pageList);
                        }

                        if (currentProcess.ResourceLimit != null && diffSize != 0)
                        {
                            currentProcess.ResourceLimit.Release(LimitableResource.Memory, diffSize);
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

                    if (!IsUnmapped(_currentHeapAddr, diffSize))
                    {
                        CleanUpForError();

                        return KernelResult.InvalidMemState;
                    }

                    result = DoMmuOperation(
                        _currentHeapAddr,
                        pagesCount,
                        pageList,
                        MemoryPermission.ReadAndWrite,
                        MemoryOperation.MapVa);

                    if (result != KernelResult.Success)
                    {
                        CleanUpForError();

                        return result;
                    }

                    InsertBlock(_currentHeapAddr, pagesCount, MemoryState.Heap, MemoryPermission.ReadAndWrite);
                }
            }
            else
            {
                //Shrink.
                ulong freeAddr = HeapRegionStart + size;
                ulong diffSize = currentHeapSize - size;

                lock (_blocks)
                {
                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    if (!CheckRange(
                        freeAddr,
                        diffSize,
                        MemoryState.Mask,
                        MemoryState.Heap,
                        MemoryPermission.Mask,
                        MemoryPermission.ReadAndWrite,
                        MemoryAttribute.Mask,
                        MemoryAttribute.None,
                        MemoryAttribute.IpcAndDeviceMapped,
                        out _,
                        out _,
                        out _))
                    {
                        return KernelResult.InvalidMemState;
                    }

                    ulong pagesCount = diffSize / PageSize;

                    KernelResult result = MmuUnmap(freeAddr, pagesCount);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    currentProcess.ResourceLimit?.Release(LimitableResource.Memory, BitUtils.AlignDown(diffSize, PageSize));

                    InsertBlock(freeAddr, pagesCount, MemoryState.Unmapped);
                }
            }

            _currentHeapAddr = HeapRegionStart + size;

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
                    MemoryPermission.None,
                    MemoryPermission.None,
                    MemoryAttribute.BorrowedAndIpcMapped,
                    MemoryAttribute.None,
                    MemoryAttribute.DeviceMappedAndUncached,
                    out MemoryState      state,
                    out MemoryPermission permission,
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
                    MemoryPermission.None,
                    MemoryAttribute.None,
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
                    MemoryPermission.Mask,
                    MemoryPermission.ReadAndWrite,
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

                    KernelResult result = MmuChangePermission(src, pagesCount, MemoryPermission.None);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    result = MapPages(dst, pageList, MemoryPermission.ReadAndWrite);

                    if (result != KernelResult.Success)
                    {
                        if (MmuChangePermission(src, pagesCount, MemoryPermission.ReadAndWrite) != KernelResult.Success)
                        {
                            throw new InvalidOperationException("Unexpected failure reverting memory permission.");
                        }

                        return result;
                    }

                    InsertBlock(src, pagesCount, srcState, MemoryPermission.None, MemoryAttribute.Borrowed);
                    InsertBlock(dst, pagesCount, MemoryState.Stack, MemoryPermission.ReadAndWrite);

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
                    MemoryPermission.None,
                    MemoryPermission.None,
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
                    MemoryPermission.Mask,
                    MemoryPermission.None,
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
                    MemoryPermission.None,
                    MemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out _,
                    out MemoryPermission dstPermission,
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

                    result = MmuChangePermission(src, pagesCount, MemoryPermission.ReadAndWrite);

                    if (result != KernelResult.Success)
                    {
                        MapPages(dst, dstPageList, dstPermission);

                        return result;
                    }

                    InsertBlock(src, pagesCount, srcState, MemoryPermission.ReadAndWrite);
                    InsertBlock(dst, pagesCount, MemoryState.Unmapped);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult ReserveTransferMemory(ulong address, ulong size, MemoryPermission permission)
        {
            lock (_blocks)
            {
                if (CheckRange(
                    address,
                    size,
                    MemoryState.TransferMemoryAllowed | MemoryState.IsPoolAllocated,
                    MemoryState.TransferMemoryAllowed | MemoryState.IsPoolAllocated,
                    MemoryPermission.Mask,
                    MemoryPermission.ReadAndWrite,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState state,
                    out _,
                    out MemoryAttribute attribute))
                {
                    //TODO: Missing checks.

                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong pagesCount = size / PageSize;

                    attribute |= MemoryAttribute.Borrowed;

                    InsertBlock(address, pagesCount, state, permission, attribute);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult ResetTransferMemory(ulong address, ulong size)
        {
            lock (_blocks)
            {
                if (CheckRange(
                    address,
                    size,
                    MemoryState.TransferMemoryAllowed | MemoryState.IsPoolAllocated,
                    MemoryState.TransferMemoryAllowed | MemoryState.IsPoolAllocated,
                    MemoryPermission.None,
                    MemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.Borrowed,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState state,
                    out _,
                    out _))
                {
                    if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong pagesCount = size / PageSize;

                    InsertBlock(address, pagesCount, state, MemoryPermission.ReadAndWrite);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult SetProcessMemoryPermission(ulong address, ulong size, MemoryPermission permission)
        {
            lock (_blocks)
            {
                if (CheckRange(
                    address,
                    size,
                    MemoryState.ProcessPermissionChangeAllowed,
                    MemoryState.ProcessPermissionChangeAllowed,
                    MemoryPermission.None,
                    MemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState      oldState,
                    out MemoryPermission oldPermission,
                    out _))
                {
                    MemoryState newState = oldState;

                    //If writing into the code region is allowed, then we need
                    //to change it to mutable.
                    if ((permission & MemoryPermission.Write) != 0)
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

                        MemoryOperation operation = (permission & MemoryPermission.Execute) != 0
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

                KMemoryInfo info;

                LinkedListNode<KMemoryBlock> node = FindBlockNode(address);

                do
                {
                    info = node.Value.GetInfo();

                    if (info.State != MemoryState.Unmapped)
                    {
                        mappedSize += GetSizeInRange(info, address, endAddr);
                    }

                    node = node.Next;
                }
                while (info.Address + info.Size < endAddr && node != null);

                if (mappedSize == size)
                {
                    return KernelResult.Success;
                }

                ulong remainingSize = size - mappedSize;

                ulong remainingPages = remainingSize / PageSize;

                KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

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
                    MemoryPermission.None,
                    MemoryAttribute.None,
                    MemoryState.Heap,
                    MemoryPermission.ReadAndWrite,
                    MemoryAttribute.None);
            }

            return KernelResult.Success;
        }

        public KernelResult UnmapPhysicalMemory(ulong address, ulong size)
        {
            ulong endAddr = address + size;

            lock (_blocks)
            {
                //Scan, ensure that the region can be unmapped (all blocks are heap or
                //already unmapped), fill pages list for freeing memory.
                ulong heapMappedSize = 0;

                KPageList pageList = new KPageList();

                KMemoryInfo info;

                LinkedListNode<KMemoryBlock> baseNode = FindBlockNode(address);

                LinkedListNode<KMemoryBlock> node = baseNode;

                do
                {
                    info = node.Value.GetInfo();

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

                    node = node.Next;
                }
                while (info.Address + info.Size < endAddr && node != null);

                if (heapMappedSize == 0)
                {
                    return KernelResult.Success;
                }

                if (!_blockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                //Try to unmap all the heap mapped memory inside range.
                KernelResult result = KernelResult.Success;

                node = baseNode;

                do
                {
                    info = node.Value.GetInfo();

                    if (info.State == MemoryState.Heap)
                    {
                        ulong blockSize    = GetSizeInRange(info, address, endAddr);
                        ulong blockAddress = GetAddrInRange(info, address);

                        ulong blockPagesCount = blockSize / PageSize;

                        result = MmuUnmap(blockAddress, blockPagesCount);

                        if (result != KernelResult.Success)
                        {
                            //If we failed to unmap, we need to remap everything back again.
                            MapPhysicalMemory(pageList, address, blockAddress + blockSize);

                            break;
                        }
                    }

                    node = node.Next;
                }
                while (info.Address + info.Size < endAddr && node != null);

                if (result == KernelResult.Success)
                {
                    GetMemoryRegionManager().FreePages(pageList);

                    PhysicalMemoryUsage -= heapMappedSize;

                    KProcess currentProcess = _system.Scheduler.GetCurrentProcess();

                    currentProcess.ResourceLimit?.Release(LimitableResource.Memory, heapMappedSize);

                    ulong pagesCount = size / PageSize;

                    InsertBlock(address, pagesCount, MemoryState.Unmapped);
                }

                return result;
            }
        }

        private void MapPhysicalMemory(KPageList pageList, ulong address, ulong endAddr)
        {
            KMemoryInfo info;

            LinkedListNode<KMemoryBlock> node = FindBlockNode(address);

            LinkedListNode<KPageNode> pageListNode = pageList.Nodes.First;

            KPageNode pageNode = pageListNode.Value;

            ulong srcPa      = pageNode.Address;
            ulong srcPaPages = pageNode.PagesCount;

            do
            {
                info = node.Value.GetInfo();

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
                            MemoryPermission.ReadAndWrite,
                            MemoryOperation.MapPa);

                        dstVa      += pagesCount * PageSize;
                        srcPa      += pagesCount * PageSize;
                        srcPaPages -= pagesCount;
                        dstVaPages -= pagesCount;
                    }
                }

                node = node.Next;
            }
            while (info.Address + info.Size < endAddr && node != null);
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

        private static ulong GetAddrInRange(KMemoryInfo info, ulong start)
        {
            if (info.Address < start)
            {
                return start;
            }

            return info.Address;
        }

        private void AddVaRangeToPageList(KPageList pageList, ulong start, ulong pagesCount)
        {
            ulong address = start;

            while (address < start + pagesCount * PageSize)
            {
                KernelResult result = ConvertVaToPa(address, out ulong pa);

                if (result != KernelResult.Success)
                {
                    throw new InvalidOperationException("Unexpected failure translating virtual address.");
                }

                pageList.AddRange(pa, 1);

                address += PageSize;
            }
        }

        private bool IsUnmapped(ulong address, ulong size)
        {
            return CheckRange(
                address,
                size,
                MemoryState.Mask,
                MemoryState.Unmapped,
                MemoryPermission.Mask,
                MemoryPermission.None,
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
            MemoryPermission     permissionMask,
            MemoryPermission     permissionExpected,
            MemoryAttribute      attributeMask,
            MemoryAttribute      attributeExpected,
            MemoryAttribute      attributeIgnoreMask,
            out MemoryState      outState,
            out MemoryPermission outPermission,
            out MemoryAttribute  outAttribute)
        {
            ulong endAddr = address + size - 1;

            LinkedListNode<KMemoryBlock> node = FindBlockNode(address);

            KMemoryInfo info = node.Value.GetInfo();

            MemoryState      firstState      = info.State;
            MemoryPermission firstPermission = info.Permission;
            MemoryAttribute  firstAttribute  = info.Attribute;

            do
            {
                info = node.Value.GetInfo();

                //Check if the block state matches what we expect.
                if ( firstState                             != info.State                             ||
                     firstPermission                        != info.Permission                        ||
                    (info.Attribute  & attributeMask)       != attributeExpected                      ||
                    (firstAttribute  | attributeIgnoreMask) != (info.Attribute | attributeIgnoreMask) ||
                    (firstState      & stateMask)           != stateExpected                          ||
                    (firstPermission & permissionMask)      != permissionExpected)
                {
                    break;
                }

                //Check if this is the last block on the range, if so return success.
                if (endAddr <= info.Address + info.Size - 1)
                {
                    outState      = firstState;
                    outPermission = firstPermission;
                    outAttribute  = firstAttribute & ~attributeIgnoreMask;

                    return true;
                }

                node = node.Next;
            }
            while (node != null);

            outState      = MemoryState.Unmapped;
            outPermission = MemoryPermission.None;
            outAttribute  = MemoryAttribute.None;

            return false;
        }

        private bool CheckRange(
            ulong            address,
            ulong            size,
            MemoryState      stateMask,
            MemoryState      stateExpected,
            MemoryPermission permissionMask,
            MemoryPermission permissionExpected,
            MemoryAttribute  attributeMask,
            MemoryAttribute  attributeExpected)
        {
            ulong endAddr = address + size - 1;

            LinkedListNode<KMemoryBlock> node = FindBlockNode(address);

            do
            {
                KMemoryInfo info = node.Value.GetInfo();

                //Check if the block state matches what we expect.
                if ((info.State      & stateMask)      != stateExpected      ||
                    (info.Permission & permissionMask) != permissionExpected ||
                    (info.Attribute  & attributeMask)  != attributeExpected)
                {
                    break;
                }

                //Check if this is the last block on the range, if so return success.
                if (endAddr <= info.Address + info.Size - 1)
                {
                    return true;
                }

                node = node.Next;
            }
            while (node != null);

            return false;
        }

        private void InsertBlock(
            ulong            baseAddress,
            ulong            pagesCount,
            MemoryState      oldState,
            MemoryPermission oldPermission,
            MemoryAttribute  oldAttribute,
            MemoryState      newState,
            MemoryPermission newPermission,
            MemoryAttribute  newAttribute)
        {
            //Insert new block on the list only on areas where the state
            //of the block matches the state specified on the Old* state
            //arguments, otherwise leave it as is.
            int oldCount = _blocks.Count;

            oldAttribute |= MemoryAttribute.IpcAndDeviceMapped;

            ulong endAddr = pagesCount * PageSize + baseAddress;

            LinkedListNode<KMemoryBlock> node = _blocks.First;

            while (node != null)
            {
                LinkedListNode<KMemoryBlock> newNode  = node;
                LinkedListNode<KMemoryBlock> nextNode = node.Next;

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
                        node = nextNode;

                        continue;
                    }

                    if (currBaseAddr >= baseAddress && currEndAddr <= endAddr)
                    {
                        currBlock.State      = newState;
                        currBlock.Permission = newPermission;
                        currBlock.Attribute &= ~MemoryAttribute.IpcAndDeviceMapped;
                        currBlock.Attribute |= newAttribute;
                    }
                    else if (currBaseAddr >= baseAddress)
                    {
                        currBlock.BaseAddress = endAddr;

                        currBlock.PagesCount = (currEndAddr - endAddr) / PageSize;

                        ulong newPagesCount = (endAddr - currBaseAddr) / PageSize;

                        newNode = _blocks.AddBefore(node, new KMemoryBlock(
                            currBaseAddr,
                            newPagesCount,
                            newState,
                            newPermission,
                            newAttribute));
                    }
                    else if (currEndAddr <= endAddr)
                    {
                        currBlock.PagesCount = (baseAddress - currBaseAddr) / PageSize;

                        ulong newPagesCount = (currEndAddr - baseAddress) / PageSize;

                        newNode = _blocks.AddAfter(node, new KMemoryBlock(
                            baseAddress,
                            newPagesCount,
                            newState,
                            newPermission,
                            newAttribute));
                    }
                    else
                    {
                        currBlock.PagesCount = (baseAddress - currBaseAddr) / PageSize;

                        ulong nextPagesCount = (currEndAddr - endAddr) / PageSize;

                        newNode = _blocks.AddAfter(node, new KMemoryBlock(
                            baseAddress,
                            pagesCount,
                            newState,
                            newPermission,
                            newAttribute));

                        _blocks.AddAfter(newNode, new KMemoryBlock(
                            endAddr,
                            nextPagesCount,
                            currBlock.State,
                            currBlock.Permission,
                            currBlock.Attribute));

                        nextNode = null;
                    }

                    MergeEqualStateNeighbours(newNode);
                }

                node = nextNode;
            }

            _blockAllocator.Count += _blocks.Count - oldCount;
        }

        private void InsertBlock(
            ulong            baseAddress,
            ulong            pagesCount,
            MemoryState      state,
            MemoryPermission permission = MemoryPermission.None,
            MemoryAttribute  attribute  = MemoryAttribute.None)
        {
            //Inserts new block at the list, replacing and spliting
            //existing blocks as needed.
            KMemoryBlock block = new KMemoryBlock(baseAddress, pagesCount, state, permission, attribute);

            int oldCount = _blocks.Count;

            ulong endAddr = pagesCount * PageSize + baseAddress;

            LinkedListNode<KMemoryBlock> newNode = null;

            LinkedListNode<KMemoryBlock> node = _blocks.First;

            while (node != null)
            {
                KMemoryBlock currBlock = node.Value;

                LinkedListNode<KMemoryBlock> nextNode = node.Next;

                ulong currBaseAddr = currBlock.BaseAddress;
                ulong currEndAddr  = currBlock.PagesCount * PageSize + currBaseAddr;

                if (baseAddress < currEndAddr && currBaseAddr < endAddr)
                {
                    if (baseAddress >= currBaseAddr && endAddr <= currEndAddr)
                    {
                        block.Attribute |= currBlock.Attribute & MemoryAttribute.IpcAndDeviceMapped;
                    }

                    if (baseAddress > currBaseAddr && endAddr < currEndAddr)
                    {
                        currBlock.PagesCount = (baseAddress - currBaseAddr) / PageSize;

                        ulong nextPagesCount = (currEndAddr - endAddr) / PageSize;

                        newNode = _blocks.AddAfter(node, block);

                        _blocks.AddAfter(newNode, new KMemoryBlock(
                            endAddr,
                            nextPagesCount,
                            currBlock.State,
                            currBlock.Permission,
                            currBlock.Attribute));

                        break;
                    }
                    else if (baseAddress <= currBaseAddr && endAddr < currEndAddr)
                    {
                        currBlock.BaseAddress = endAddr;

                        currBlock.PagesCount = (currEndAddr - endAddr) / PageSize;

                        if (newNode == null)
                        {
                            newNode = _blocks.AddBefore(node, block);
                        }
                    }
                    else if (baseAddress > currBaseAddr && endAddr >= currEndAddr)
                    {
                        currBlock.PagesCount = (baseAddress - currBaseAddr) / PageSize;

                        if (newNode == null)
                        {
                            newNode = _blocks.AddAfter(node, block);
                        }
                    }
                    else
                    {
                        if (newNode == null)
                        {
                            newNode = _blocks.AddBefore(node, block);
                        }

                        _blocks.Remove(node);
                    }
                }

                node = nextNode;
            }

            if (newNode == null)
            {
                newNode = _blocks.AddFirst(block);
            }

            MergeEqualStateNeighbours(newNode);

            _blockAllocator.Count += _blocks.Count - oldCount;
        }

        private void MergeEqualStateNeighbours(LinkedListNode<KMemoryBlock> node)
        {
            KMemoryBlock block = node.Value;

            ulong endAddr = block.PagesCount * PageSize + block.BaseAddress;

            if (node.Previous != null)
            {
                KMemoryBlock previous = node.Previous.Value;

                if (BlockStateEquals(block, previous))
                {
                    _blocks.Remove(node.Previous);

                    block.BaseAddress = previous.BaseAddress;
                }
            }

            if (node.Next != null)
            {
                KMemoryBlock next = node.Next.Value;

                if (BlockStateEquals(block, next))
                {
                    _blocks.Remove(node.Next);

                    endAddr = next.BaseAddress + next.PagesCount * PageSize;
                }
            }

            block.PagesCount = (endAddr - block.BaseAddress) / PageSize;
        }

        private static bool BlockStateEquals(KMemoryBlock lhs, KMemoryBlock rhs)
        {
            return lhs.State          == rhs.State          &&
                   lhs.Permission     == rhs.Permission     &&
                   lhs.Attribute      == rhs.Attribute      &&
                   lhs.DeviceRefCount == rhs.DeviceRefCount &&
                   lhs.IpcRefCount    == rhs.IpcRefCount;
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

        private bool ValidateRegionForState(ulong address, ulong size, MemoryState state)
        {
            ulong endAddr = address + size;

            ulong regionBaseAddr = GetBaseAddrForState(state);

            ulong regionEndAddr = regionBaseAddr + GetSizeForState(state);

            bool InsideRegion()
            {
                return regionBaseAddr <= address &&
                       endAddr        >  address &&
                       endAddr - 1    <= regionEndAddr - 1;
            }

            bool OutsideHeapRegion()
            {
                return endAddr <= HeapRegionStart ||
                       address >= HeapRegionEnd;
            }

            bool OutsideMapRegion()
            {
                return endAddr <= AliasRegionStart ||
                       address >= AliasRegionEnd;
            }

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
                    return InsideRegion() && OutsideHeapRegion() && OutsideMapRegion();

                case MemoryState.Heap:
                    return InsideRegion() && OutsideMapRegion();

                case MemoryState.IpcBuffer0:
                case MemoryState.IpcBuffer1:
                case MemoryState.IpcBuffer3:
                    return InsideRegion() && OutsideHeapRegion();

                case MemoryState.KernelStack:
                    return InsideRegion();
            }

            throw new ArgumentException($"Invalid state value \"{state}\".");
        }

        private ulong GetBaseAddrForState(MemoryState state)
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

        private ulong GetSizeForState(MemoryState state)
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

        private KernelResult MapPages(ulong address, KPageList pageList, MemoryPermission permission)
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
                MemoryPermission.None,
                MemoryOperation.Unmap);
        }

        private KernelResult MmuChangePermission(ulong address, ulong pagesCount, MemoryPermission permission)
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
            MemoryPermission permission,
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

                    _cpuMemory.Map((long)dstVa, (long)(srcPa - DramMemoryMap.DramBase), (long)size);

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

                    _cpuMemory.Unmap((long)dstVa, (long)size);

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
            MemoryPermission permission,
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
            return _system.MemoryRegions[(int)_memRegion];
        }

        private KernelResult MmuMapPages(ulong address, KPageList pageList)
        {
            foreach (KPageNode pageNode in pageList)
            {
                ulong size = pageNode.PagesCount * PageSize;

                _cpuMemory.Map((long)address, (long)(pageNode.Address - DramMemoryMap.DramBase), (long)size);

                address += size;
            }

            return KernelResult.Success;
        }

        public KernelResult ConvertVaToPa(ulong va, out ulong pa)
        {
            pa = DramMemoryMap.DramBase + (ulong)_cpuMemory.GetPhysicalAddress((long)va);

            return KernelResult.Success;
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