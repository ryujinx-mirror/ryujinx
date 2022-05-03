using Ryujinx.Common;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    abstract class KPageTableBase
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

        protected readonly KernelContext Context;

        public ulong AddrSpaceStart { get; private set; }
        public ulong AddrSpaceEnd { get; private set; }

        public ulong CodeRegionStart { get; private set; }
        public ulong CodeRegionEnd { get; private set; }

        public ulong HeapRegionStart { get; private set; }
        public ulong HeapRegionEnd { get; private set; }

        private ulong _currentHeapAddr;

        public ulong AliasRegionStart { get; private set; }
        public ulong AliasRegionEnd { get; private set; }

        public ulong StackRegionStart { get; private set; }
        public ulong StackRegionEnd { get; private set; }

        public ulong TlsIoRegionStart { get; private set; }
        public ulong TlsIoRegionEnd { get; private set; }

        private ulong _heapCapacity;

        public ulong PhysicalMemoryUsage { get; private set; }

        private readonly KMemoryBlockManager _blockManager;

        private MemoryRegion _memRegion;

        private bool _aslrDisabled;

        public int AddrSpaceWidth { get; private set; }

        private bool _isKernel;

        private bool _aslrEnabled;

        private KMemoryBlockSlabManager _slabManager;

        private int _contextId;

        private MersenneTwister _randomNumberGenerator;

        private MemoryFillValue _heapFillValue;
        private MemoryFillValue _ipcFillValue;

        public KPageTableBase(KernelContext context)
        {
            Context = context;

            _blockManager = new KMemoryBlockManager();

            _isKernel = false;

            _heapFillValue = MemoryFillValue.Zero;
            _ipcFillValue = MemoryFillValue.Zero;
        }

        private static readonly int[] AddrSpaceSizes = new int[] { 32, 36, 32, 39 };

        public KernelResult InitializeForProcess(
            AddressSpaceType addrSpaceType,
            bool aslrEnabled,
            bool aslrDisabled,
            MemoryRegion memRegion,
            ulong address,
            ulong size,
            KMemoryBlockSlabManager slabManager)
        {
            if ((uint)addrSpaceType > (uint)AddressSpaceType.Addr39Bits)
            {
                throw new ArgumentException(nameof(addrSpaceType));
            }

            _contextId = Context.ContextIdManager.GetId();

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
                slabManager);

            if (result != KernelResult.Success)
            {
                Context.ContextIdManager.PutId(_contextId);
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
            AddressSpaceType addrSpaceType,
            bool aslrEnabled,
            bool aslrDisabled,
            ulong addrSpaceStart,
            ulong addrSpaceEnd,
            MemoryRegion memRegion,
            ulong address,
            ulong size,
            KMemoryBlockSlabManager slabManager)
        {
            ulong endAddr = address + size;

            Region aliasRegion = new Region();
            Region heapRegion = new Region();
            Region stackRegion = new Region();
            Region tlsIoRegion = new Region();

            ulong codeRegionSize;
            ulong stackAndTlsIoStart;
            ulong stackAndTlsIoEnd;
            ulong baseAddress;

            switch (addrSpaceType)
            {
                case AddressSpaceType.Addr32Bits:
                    aliasRegion.Size = 0x40000000;
                    heapRegion.Size = 0x40000000;
                    stackRegion.Size = 0;
                    tlsIoRegion.Size = 0;
                    CodeRegionStart = 0x200000;
                    codeRegionSize = 0x3fe00000;
                    stackAndTlsIoStart = 0x200000;
                    stackAndTlsIoEnd = 0x40000000;
                    baseAddress = 0x200000;
                    AddrSpaceWidth = 32;
                    break;

                case AddressSpaceType.Addr36Bits:
                    aliasRegion.Size = 0x180000000;
                    heapRegion.Size = 0x180000000;
                    stackRegion.Size = 0;
                    tlsIoRegion.Size = 0;
                    CodeRegionStart = 0x8000000;
                    codeRegionSize = 0x78000000;
                    stackAndTlsIoStart = 0x8000000;
                    stackAndTlsIoEnd = 0x80000000;
                    baseAddress = 0x8000000;
                    AddrSpaceWidth = 36;
                    break;

                case AddressSpaceType.Addr32BitsNoMap:
                    aliasRegion.Size = 0;
                    heapRegion.Size = 0x80000000;
                    stackRegion.Size = 0;
                    tlsIoRegion.Size = 0;
                    CodeRegionStart = 0x200000;
                    codeRegionSize = 0x3fe00000;
                    stackAndTlsIoStart = 0x200000;
                    stackAndTlsIoEnd = 0x40000000;
                    baseAddress = 0x200000;
                    AddrSpaceWidth = 32;
                    break;

                case AddressSpaceType.Addr39Bits:
                    aliasRegion.Size = 0x1000000000;
                    heapRegion.Size = 0x180000000;
                    stackRegion.Size = 0x80000000;
                    tlsIoRegion.Size = 0x1000000000;
                    CodeRegionStart = BitUtils.AlignDown(address, 0x200000);
                    codeRegionSize = BitUtils.AlignUp(endAddr, 0x200000) - CodeRegionStart;
                    stackAndTlsIoStart = 0;
                    stackAndTlsIoEnd = 0;
                    baseAddress = 0x8000000;
                    AddrSpaceWidth = 39;
                    break;

                default: throw new ArgumentException(nameof(addrSpaceType));
            }

            CodeRegionEnd = CodeRegionStart + codeRegionSize;

            ulong mapBaseAddress;
            ulong mapAvailableSize;

            if (CodeRegionStart - baseAddress >= addrSpaceEnd - CodeRegionEnd)
            {
                // Has more space before the start of the code region.
                mapBaseAddress = baseAddress;
                mapAvailableSize = CodeRegionStart - baseAddress;
            }
            else
            {
                // Has more space after the end of the code region.
                mapBaseAddress = CodeRegionEnd;
                mapAvailableSize = addrSpaceEnd - CodeRegionEnd;
            }

            ulong mapTotalSize = aliasRegion.Size + heapRegion.Size + stackRegion.Size + tlsIoRegion.Size;

            ulong aslrMaxOffset = mapAvailableSize - mapTotalSize;

            _aslrEnabled = aslrEnabled;

            AddrSpaceStart = addrSpaceStart;
            AddrSpaceEnd = addrSpaceEnd;

            _slabManager = slabManager;

            if (mapAvailableSize < mapTotalSize)
            {
                return KernelResult.OutOfMemory;
            }

            if (aslrEnabled)
            {
                aliasRegion.AslrOffset = GetRandomValue(0, aslrMaxOffset >> 21) << 21;
                heapRegion.AslrOffset = GetRandomValue(0, aslrMaxOffset >> 21) << 21;
                stackRegion.AslrOffset = GetRandomValue(0, aslrMaxOffset >> 21) << 21;
                tlsIoRegion.AslrOffset = GetRandomValue(0, aslrMaxOffset >> 21) << 21;
            }

            // Regions are sorted based on ASLR offset.
            // When ASLR is disabled, the order is Map, Heap, NewMap and TlsIo.
            aliasRegion.Start = mapBaseAddress + aliasRegion.AslrOffset;
            aliasRegion.End = aliasRegion.Start + aliasRegion.Size;
            heapRegion.Start = mapBaseAddress + heapRegion.AslrOffset;
            heapRegion.End = heapRegion.Start + heapRegion.Size;
            stackRegion.Start = mapBaseAddress + stackRegion.AslrOffset;
            stackRegion.End = stackRegion.Start + stackRegion.Size;
            tlsIoRegion.Start = mapBaseAddress + tlsIoRegion.AslrOffset;
            tlsIoRegion.End = tlsIoRegion.Start + tlsIoRegion.Size;

            SortRegion(heapRegion, aliasRegion);

            if (stackRegion.Size != 0)
            {
                SortRegion(stackRegion, aliasRegion);
                SortRegion(stackRegion, heapRegion);
            }
            else
            {
                stackRegion.Start = stackAndTlsIoStart;
                stackRegion.End = stackAndTlsIoEnd;
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
                tlsIoRegion.End = stackAndTlsIoEnd;
            }

            AliasRegionStart = aliasRegion.Start;
            AliasRegionEnd = aliasRegion.End;
            HeapRegionStart = heapRegion.Start;
            HeapRegionEnd = heapRegion.End;
            StackRegionStart = stackRegion.Start;
            StackRegionEnd = stackRegion.End;
            TlsIoRegionStart = tlsIoRegion.Start;
            TlsIoRegionEnd = tlsIoRegion.End;

            // TODO: Check kernel configuration via secure monitor call when implemented to set memory fill values.

            _currentHeapAddr = HeapRegionStart;
            _heapCapacity = 0;
            PhysicalMemoryUsage = 0;

            _memRegion = memRegion;
            _aslrDisabled = aslrDisabled;

            return _blockManager.Initialize(addrSpaceStart, addrSpaceEnd, slabManager);
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
                rhs.End += lhs.Size;
            }
            else
            {
                lhs.Start += rhs.Size;
                lhs.End += rhs.Size;
            }
        }

        public KernelResult MapPages(ulong address, KPageList pageList, MemoryState state, KMemoryPermission permission)
        {
            ulong pagesCount = pageList.GetPagesCount();

            ulong size = pagesCount * PageSize;

            if (!CanContain(address, size, state))
            {
                return KernelResult.InvalidMemState;
            }

            lock (_blockManager)
            {
                if (!IsUnmapped(address, pagesCount * PageSize))
                {
                    return KernelResult.InvalidMemState;
                }

                if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                KernelResult result = MapPages(address, pageList, permission);

                if (result == KernelResult.Success)
                {
                    _blockManager.InsertBlock(address, pagesCount, state, permission);
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

            lock (_blockManager)
            {
                KPageList currentPageList = new KPageList();

                GetPhysicalRegions(address, size, currentPageList);

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
                    if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    KernelResult result = Unmap(address, pagesCount);

                    if (result == KernelResult.Success)
                    {
                        _blockManager.InsertBlock(address, pagesCount, MemoryState.Unmapped);
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

        public KernelResult MapPages(
            ulong pagesCount,
            int alignment,
            ulong srcPa,
            bool paIsValid,
            ulong regionStart,
            ulong regionPagesCount,
            MemoryState state,
            KMemoryPermission permission,
            out ulong address)
        {
            address = 0;

            ulong regionSize = regionPagesCount * PageSize;

            if (!CanContain(regionStart, regionSize, state))
            {
                return KernelResult.InvalidMemState;
            }

            if (regionPagesCount <= pagesCount)
            {
                return KernelResult.OutOfMemory;
            }

            lock (_blockManager)
            {
                address = AllocateVa(regionStart, regionPagesCount, pagesCount, alignment);

                if (address == 0)
                {
                    return KernelResult.OutOfMemory;
                }

                if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                KernelResult result;

                if (paIsValid)
                {
                    result = MapPages(address, pagesCount, srcPa, permission);
                }
                else
                {
                    result = AllocateAndMapPages(address, pagesCount, permission);
                }

                if (result != KernelResult.Success)
                {
                    return result;
                }

                _blockManager.InsertBlock(address, pagesCount, state, permission);
            }

            return KernelResult.Success;
        }

        public KernelResult MapPages(ulong address, ulong pagesCount, MemoryState state, KMemoryPermission permission)
        {
            ulong size = pagesCount * PageSize;

            if (!CanContain(address, size, state))
            {
                return KernelResult.InvalidMemState;
            }

            lock (_blockManager)
            {
                if (!IsUnmapped(address, size))
                {
                    return KernelResult.InvalidMemState;
                }

                if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                KernelResult result = AllocateAndMapPages(address, pagesCount, permission);

                if (result == KernelResult.Success)
                {
                    _blockManager.InsertBlock(address, pagesCount, state, permission);
                }

                return result;
            }
        }

        private KernelResult AllocateAndMapPages(ulong address, ulong pagesCount, KMemoryPermission permission)
        {
            KMemoryRegionManager region = GetMemoryRegionManager();

            KernelResult result = region.AllocatePages(pagesCount, _aslrDisabled, out KPageList pageList);

            if (result != KernelResult.Success)
            {
                return result;
            }

            using var _ = new OnScopeExit(() => pageList.DecrementPagesReferenceCount(Context.MemoryManager));

            return MapPages(address, pageList, permission);
        }

        public KernelResult MapProcessCodeMemory(ulong dst, ulong src, ulong size)
        {
            lock (_blockManager)
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
                    out MemoryState state,
                    out KMemoryPermission permission,
                    out _);

                success &= IsUnmapped(dst, size);

                if (success)
                {
                    if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion * 2))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong pagesCount = size / PageSize;

                    KernelResult result = MapMemory(src, dst, pagesCount, permission, KMemoryPermission.None);

                    _blockManager.InsertBlock(src, pagesCount, state, KMemoryPermission.None, MemoryAttribute.Borrowed);
                    _blockManager.InsertBlock(dst, pagesCount, MemoryState.ModCodeStatic);

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
            lock (_blockManager)
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
                    ulong pagesCount = size / PageSize;

                    KernelResult result = Unmap(dst, pagesCount);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    // TODO: Missing some checks here.

                    if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion * 2))
                    {
                        return KernelResult.OutOfResource;
                    }

                    _blockManager.InsertBlock(dst, pagesCount, MemoryState.Unmapped);
                    _blockManager.InsertBlock(src, pagesCount, MemoryState.Heap, KMemoryPermission.ReadAndWrite);

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

            lock (_blockManager)
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

                    using var _ = new OnScopeExit(() => pageList.DecrementPagesReferenceCount(Context.MemoryManager));

                    void CleanUpForError()
                    {
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

                    if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        CleanUpForError();

                        return KernelResult.OutOfResource;
                    }

                    if (!IsUnmapped(_currentHeapAddr, sizeDelta))
                    {
                        CleanUpForError();

                        return KernelResult.InvalidMemState;
                    }

                    result = MapPages(_currentHeapAddr, pageList, KMemoryPermission.ReadAndWrite, true, (byte)_heapFillValue);

                    if (result != KernelResult.Success)
                    {
                        CleanUpForError();

                        return result;
                    }

                    _blockManager.InsertBlock(_currentHeapAddr, pagesCount, MemoryState.Heap, KMemoryPermission.ReadAndWrite);
                }
                else
                {
                    // Shrink.
                    ulong freeAddr = HeapRegionStart + size;
                    ulong sizeDelta = currentHeapSize - size;

                    if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
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

                    KernelResult result = Unmap(freeAddr, pagesCount);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    currentProcess.ResourceLimit?.Release(LimitableResource.Memory, sizeDelta);

                    _blockManager.InsertBlock(freeAddr, pagesCount, MemoryState.Unmapped);
                }

                _currentHeapAddr = HeapRegionStart + size;
            }

            address = HeapRegionStart;

            return KernelResult.Success;
        }

        public KernelResult SetMemoryPermission(ulong address, ulong size, KMemoryPermission permission)
        {
            lock (_blockManager)
            {
                if (CheckRange(
                    address,
                    size,
                    MemoryState.PermissionChangeAllowed,
                    MemoryState.PermissionChangeAllowed,
                    KMemoryPermission.None,
                    KMemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState oldState,
                    out KMemoryPermission oldPermission,
                    out _))
                {
                    if (permission != oldPermission)
                    {
                        if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                        {
                            return KernelResult.OutOfResource;
                        }

                        ulong pagesCount = size / PageSize;

                        KernelResult result = Reprotect(address, pagesCount, permission);

                        if (result != KernelResult.Success)
                        {
                            return result;
                        }

                        _blockManager.InsertBlock(address, pagesCount, oldState, permission);
                    }

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public ulong GetTotalHeapSize()
        {
            lock (_blockManager)
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
            lock (_blockManager)
            {
                _heapCapacity = capacity;
            }

            return KernelResult.Success;
        }

        public KernelResult SetMemoryAttribute(
            ulong address,
            ulong size,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeValue)
        {
            lock (_blockManager)
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
                    out MemoryState state,
                    out KMemoryPermission permission,
                    out MemoryAttribute attribute))
                {
                    if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong pagesCount = size / PageSize;

                    attribute &= ~attributeMask;
                    attribute |= attributeMask & attributeValue;

                    _blockManager.InsertBlock(address, pagesCount, state, permission, attribute);

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
                address < AddrSpaceEnd)
            {
                lock (_blockManager)
                {
                    return _blockManager.FindBlock(address).GetInfo();
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

            lock (_blockManager)
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
                    if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion * 2))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong pagesCount = size / PageSize;

                    KernelResult result = MapMemory(src, dst, pagesCount, KMemoryPermission.ReadAndWrite, KMemoryPermission.ReadAndWrite);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    _blockManager.InsertBlock(src, pagesCount, srcState, KMemoryPermission.None, MemoryAttribute.Borrowed);
                    _blockManager.InsertBlock(dst, pagesCount, MemoryState.Stack, KMemoryPermission.ReadAndWrite);

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

            lock (_blockManager)
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
                    if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    KernelResult result = Unmap(address, pagesCount);

                    if (result == KernelResult.Success)
                    {
                        _blockManager.InsertBlock(address, pagesCount, MemoryState.Unmapped);
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

            lock (_blockManager)
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
                    if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion * 2))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong pagesCount = size / PageSize;

                    KernelResult result = UnmapMemory(dst, src, pagesCount, dstPermission, KMemoryPermission.ReadAndWrite);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    _blockManager.InsertBlock(src, pagesCount, srcState, KMemoryPermission.ReadAndWrite);
                    _blockManager.InsertBlock(dst, pagesCount, MemoryState.Unmapped);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult UnmapProcessMemory(ulong dst, ulong size, KPageTableBase srcPageTable, ulong src)
        {
            lock (_blockManager)
            {
                lock (srcPageTable._blockManager)
                {
                    bool success = CheckRange(
                        dst,
                        size,
                        MemoryState.Mask,
                        MemoryState.ProcessMemory,
                        KMemoryPermission.ReadAndWrite,
                        KMemoryPermission.ReadAndWrite,
                        MemoryAttribute.Mask,
                        MemoryAttribute.None,
                        MemoryAttribute.IpcAndDeviceMapped,
                        out _,
                        out _,
                        out _);

                    success &= srcPageTable.CheckRange(
                        src,
                        size,
                        MemoryState.MapProcessAllowed,
                        MemoryState.MapProcessAllowed,
                        KMemoryPermission.None,
                        KMemoryPermission.None,
                        MemoryAttribute.Mask,
                        MemoryAttribute.None,
                        MemoryAttribute.IpcAndDeviceMapped,
                        out _,
                        out _,
                        out _);

                    if (!success)
                    {
                        return KernelResult.InvalidMemState;
                    }

                    KPageList srcPageList = new KPageList();
                    KPageList dstPageList = new KPageList();

                    srcPageTable.GetPhysicalRegions(src, size, srcPageList);
                    GetPhysicalRegions(dst, size, dstPageList);

                    if (!dstPageList.IsEqual(srcPageList))
                    {
                        return KernelResult.InvalidMemRange;
                    }
                }

                if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                ulong pagesCount = size / PageSize;

                KernelResult result = Unmap(dst, pagesCount);

                if (result != KernelResult.Success)
                {
                    return result;
                }

                _blockManager.InsertBlock(dst, pagesCount, MemoryState.Unmapped);

                return KernelResult.Success;
            }
        }

        public KernelResult SetProcessMemoryPermission(ulong address, ulong size, KMemoryPermission permission)
        {
            lock (_blockManager)
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
                    out MemoryState oldState,
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
                        if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                        {
                            return KernelResult.OutOfResource;
                        }

                        ulong pagesCount = size / PageSize;

                        KernelResult result;

                        if ((oldPermission & KMemoryPermission.Execute) != 0)
                        {
                            result = ReprotectWithAttributes(address, pagesCount, permission);
                        }
                        else
                        {
                            result = Reprotect(address, pagesCount, permission);
                        }

                        if (result != KernelResult.Success)
                        {
                            return result;
                        }

                        _blockManager.InsertBlock(address, pagesCount, newState, permission);
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

            lock (_blockManager)
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

                using var _ = new OnScopeExit(() => pageList.DecrementPagesReferenceCount(Context.MemoryManager));

                void CleanUpForError()
                {
                    currentProcess.ResourceLimit?.Release(LimitableResource.Memory, remainingSize);
                }

                if (result != KernelResult.Success)
                {
                    CleanUpForError();

                    return result;
                }

                if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    CleanUpForError();

                    return KernelResult.OutOfResource;
                }

                LinkedListNode<KPageNode> pageListNode = pageList.Nodes.First;

                KPageNode pageNode = pageListNode.Value;

                ulong srcPa = pageNode.Address;
                ulong srcPaPages = pageNode.PagesCount;

                foreach (KMemoryInfo info in IterateOverRange(address, endAddr))
                {
                    if (info.State != MemoryState.Unmapped)
                    {
                        continue;
                    }

                    ulong blockSize = GetSizeInRange(info, address, endAddr);

                    ulong dstVaPages = blockSize / PageSize;

                    ulong dstVa = GetAddrInRange(info, address);

                    while (dstVaPages > 0)
                    {
                        if (srcPaPages == 0)
                        {
                            pageListNode = pageListNode.Next;

                            pageNode = pageListNode.Value;

                            srcPa = pageNode.Address;
                            srcPaPages = pageNode.PagesCount;
                        }

                        ulong currentPagesCount = Math.Min(srcPaPages, dstVaPages);

                        MapPages(dstVa, currentPagesCount, srcPa, KMemoryPermission.ReadAndWrite);

                        dstVa += currentPagesCount * PageSize;
                        srcPa += currentPagesCount * PageSize;
                        srcPaPages -= currentPagesCount;
                        dstVaPages -= currentPagesCount;
                    }
                }

                PhysicalMemoryUsage += remainingSize;

                ulong pagesCount = size / PageSize;

                _blockManager.InsertBlock(
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

            lock (_blockManager)
            {
                // Scan, ensure that the region can be unmapped (all blocks are heap or
                // already unmapped), fill pages list for freeing memory.
                ulong heapMappedSize = 0;

                foreach (KMemoryInfo info in IterateOverRange(address, endAddr))
                {
                    if (info.State == MemoryState.Heap)
                    {
                        if (info.Attribute != MemoryAttribute.None)
                        {
                            return KernelResult.InvalidMemState;
                        }

                        ulong blockSize = GetSizeInRange(info, address, endAddr);

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

                if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                // Try to unmap all the heap mapped memory inside range.
                KernelResult result = KernelResult.Success;

                foreach (KMemoryInfo info in IterateOverRange(address, endAddr))
                {
                    if (info.State == MemoryState.Heap)
                    {
                        ulong blockSize = GetSizeInRange(info, address, endAddr);
                        ulong blockAddress = GetAddrInRange(info, address);

                        ulong blockPagesCount = blockSize / PageSize;

                        result = Unmap(blockAddress, blockPagesCount);

                        // The kernel would attempt to remap if this fails, but we don't because:
                        // - The implementation may not support remapping if memory aliasing is not supported on the platform.
                        // - Unmap can't ever fail here anyway.
                        Debug.Assert(result == KernelResult.Success);
                    }
                }

                if (result == KernelResult.Success)
                {
                    PhysicalMemoryUsage -= heapMappedSize;

                    KProcess currentProcess = KernelStatic.GetCurrentProcess();

                    currentProcess.ResourceLimit?.Release(LimitableResource.Memory, heapMappedSize);

                    ulong pagesCount = size / PageSize;

                    _blockManager.InsertBlock(address, pagesCount, MemoryState.Unmapped);
                }

                return result;
            }
        }

        public KernelResult CopyDataToCurrentProcess(
            ulong dst,
            ulong size,
            ulong src,
            MemoryState stateMask,
            MemoryState stateExpected,
            KMemoryPermission permission,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeExpected)
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
            ulong dst,
            ulong size,
            MemoryState stateMask,
            MemoryState stateExpected,
            KMemoryPermission permission,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeExpected,
            ulong src)
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
            ulong size,
            ulong clientAddress,
            ulong serverAddress,
            MemoryState stateMask,
            MemoryState stateExpected,
            KMemoryPermission permission,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeExpected,
            bool toServer)
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

            lock (_blockManager)
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
                        ulong copySize = 0x100000; // Copy chunck size. Any value will do, moderate sizes are recommended.

                        if (copySize > size)
                        {
                            copySize = size;
                        }

                        if (toServer)
                        {
                            currentProcess.CpuMemory.Write(serverAddress, GetSpan(clientAddress, (int)copySize));
                        }
                        else
                        {
                            Write(clientAddress, currentProcess.CpuMemory.GetSpan(serverAddress, (int)copySize));
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
            ulong size,
            ulong src,
            KPageTableBase srcPageTable,
            KMemoryPermission permission,
            MemoryState state,
            bool send,
            out ulong dst)
        {
            dst = 0;

            lock (srcPageTable._blockManager)
            {
                lock (_blockManager)
                {
                    KernelResult result = srcPageTable.ReprotectClientProcess(
                        src,
                        size,
                        permission,
                        state,
                        out int blocksNeeded);

                    if (result != KernelResult.Success)
                    {
                        return result;
                    }

                    if (!srcPageTable._slabManager.CanAllocate(blocksNeeded))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong srcMapAddress = BitUtils.AlignUp(src, PageSize);
                    ulong srcMapEndAddr = BitUtils.AlignDown(src + size, PageSize);
                    ulong srcMapSize = srcMapEndAddr - srcMapAddress;

                    result = MapPagesFromClientProcess(size, src, permission, state, srcPageTable, send, out ulong va);

                    if (result != KernelResult.Success)
                    {
                        if (srcMapEndAddr > srcMapAddress)
                        {
                            srcPageTable.UnmapIpcRestorePermission(src, size, state);
                        }

                        return result;
                    }

                    if (srcMapAddress < srcMapEndAddr)
                    {
                        KMemoryPermission permissionMask = permission == KMemoryPermission.ReadAndWrite
                            ? KMemoryPermission.None
                            : KMemoryPermission.Read;

                        srcPageTable._blockManager.InsertBlock(srcMapAddress, srcMapSize / PageSize, SetIpcMappingPermissions, permissionMask);
                    }

                    dst = va;
                }
            }

            return KernelResult.Success;
        }

        private KernelResult ReprotectClientProcess(
            ulong address,
            ulong size,
            KMemoryPermission permission,
            MemoryState state,
            out int blocksNeeded)
        {
            blocksNeeded = 0;

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

            ulong addressRounded = BitUtils.AlignUp(address, PageSize);
            ulong addressTruncated = BitUtils.AlignDown(address, PageSize);
            ulong endAddrRounded = BitUtils.AlignUp(endAddr, PageSize);
            ulong endAddrTruncated = BitUtils.AlignDown(endAddr, PageSize);

            if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
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
                        ulong blockSize = GetSizeInRange(info, addressRounded, endAddrVisited);

                        ulong blockPagesCount = blockSize / PageSize;

                        KernelResult reprotectResult = Reprotect(blockAddress, blockPagesCount, info.Permission);
                        Debug.Assert(reprotectResult == KernelResult.Success);
                    }
                }
            }

            // Signal a read for any resources tracking reads in the region, as the other process is likely to use their data.
            SignalMemoryTracking(addressTruncated, endAddrRounded - addressTruncated, false);

            // Reprotect the aligned pages range on the client to make them inaccessible from the client process.
            KernelResult result;

            if (addressRounded < endAddrTruncated)
            {
                foreach (KMemoryInfo info in IterateOverRange(addressRounded, endAddrTruncated))
                {
                    // Check if the block state matches what we expect.
                    if ((info.State & stateMask) != stateMask ||
                        (info.Permission & permission) != permission ||
                        (info.Attribute & attributeMask) != MemoryAttribute.None)
                    {
                        CleanUpForError();

                        return KernelResult.InvalidMemState;
                    }

                    ulong blockAddress = GetAddrInRange(info, addressRounded);
                    ulong blockSize = GetSizeInRange(info, addressRounded, endAddrTruncated);

                    ulong blockPagesCount = blockSize / PageSize;

                    // If the first block starts before the aligned range, it will need to be split.
                    if (info.Address < addressRounded)
                    {
                        blocksNeeded++;
                    }

                    // If the last block ends after the aligned range, it will need to be split.
                    if (endAddrTruncated - 1 < info.Address + info.Size - 1)
                    {
                        blocksNeeded++;
                    }

                    if ((info.Permission & KMemoryPermission.ReadAndWrite) != permissionMask && info.IpcRefCount == 0)
                    {
                        result = Reprotect(blockAddress, blockPagesCount, permissionMask);

                        if (result != KernelResult.Success)
                        {
                            CleanUpForError();

                            return result;
                        }
                    }

                    visitedSize += blockSize;
                }
            }

            return KernelResult.Success;
        }

        private KernelResult MapPagesFromClientProcess(
            ulong size,
            ulong address,
            KMemoryPermission permission,
            MemoryState state,
            KPageTableBase srcPageTable,
            bool send,
            out ulong dst)
        {
            dst = 0;

            if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
            {
                return KernelResult.OutOfResource;
            }

            ulong endAddr = address + size;

            ulong addressTruncated = BitUtils.AlignDown(address, PageSize);
            ulong addressRounded = BitUtils.AlignUp(address, PageSize);
            ulong endAddrTruncated = BitUtils.AlignDown(endAddr, PageSize);
            ulong endAddrRounded = BitUtils.AlignUp(endAddr, PageSize);

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

            ulong dstFirstPagePa = 0;
            ulong dstLastPagePa = 0;
            ulong currentVa = va;

            using var _ = new OnScopeExit(() =>
            {
                if (dstFirstPagePa != 0)
                {
                    Context.MemoryManager.DecrementPagesReferenceCount(dstFirstPagePa, 1);
                }

                if (dstLastPagePa != 0)
                {
                    Context.MemoryManager.DecrementPagesReferenceCount(dstLastPagePa, 1);
                }
            });

            void CleanUpForError()
            {
                if (currentVa != va)
                {
                    Unmap(va, (currentVa - va) / PageSize);
                }
            }

            // Is the first page address aligned?
            // If not, allocate a new page and copy the unaligned chunck.
            if (addressTruncated < addressRounded)
            {
                dstFirstPagePa = GetMemoryRegionManager().AllocatePagesContiguous(Context, 1, _aslrDisabled);

                if (dstFirstPagePa == 0)
                {
                    CleanUpForError();

                    return KernelResult.OutOfMemory;
                }
            }

            // Is the last page end address aligned?
            // If not, allocate a new page and copy the unaligned chunck.
            if (endAddrTruncated < endAddrRounded && (addressTruncated == addressRounded || addressTruncated < endAddrTruncated))
            {
                dstLastPagePa = GetMemoryRegionManager().AllocatePagesContiguous(Context, 1, _aslrDisabled);

                if (dstLastPagePa == 0)
                {
                    CleanUpForError();

                    return KernelResult.OutOfMemory;
                }
            }

            if (dstFirstPagePa != 0)
            {
                ulong firstPageFillAddress = dstFirstPagePa;
                ulong unusedSizeAfter;

                if (send)
                {
                    ulong unusedSizeBefore = address - addressTruncated;

                    Context.Memory.Fill(GetDramAddressFromPa(dstFirstPagePa), unusedSizeBefore, (byte)_ipcFillValue);

                    ulong copySize = addressRounded <= endAddr ? addressRounded - address : size;
                    var data = srcPageTable.GetSpan(addressTruncated + unusedSizeBefore, (int)copySize);

                    Context.Memory.Write(GetDramAddressFromPa(dstFirstPagePa + unusedSizeBefore), data);

                    firstPageFillAddress += unusedSizeBefore + copySize;

                    unusedSizeAfter = addressRounded > endAddr ? addressRounded - endAddr : 0;
                }
                else
                {
                    unusedSizeAfter = PageSize;
                }

                if (unusedSizeAfter != 0)
                {
                    Context.Memory.Fill(GetDramAddressFromPa(firstPageFillAddress), unusedSizeAfter, (byte)_ipcFillValue);
                }

                KernelResult result = MapPages(currentVa, 1, dstFirstPagePa, permission);

                if (result != KernelResult.Success)
                {
                    CleanUpForError();

                    return result;
                }

                currentVa += PageSize;
            }

            if (endAddrTruncated > addressRounded)
            {
                ulong alignedSize = endAddrTruncated - addressRounded;

                KPageList pageList = new KPageList();
                srcPageTable.GetPhysicalRegions(addressRounded, alignedSize, pageList);

                KernelResult result = MapPages(currentVa, pageList, permission);

                if (result != KernelResult.Success)
                {
                    CleanUpForError();

                    return result;
                }

                currentVa += alignedSize;
            }

            if (dstLastPagePa != 0)
            {
                ulong lastPageFillAddr = dstLastPagePa;
                ulong unusedSizeAfter;

                if (send)
                {
                    ulong copySize = endAddr - endAddrTruncated;
                    var data = srcPageTable.GetSpan(endAddrTruncated, (int)copySize);

                    Context.Memory.Write(GetDramAddressFromPa(dstLastPagePa), data);

                    lastPageFillAddr += copySize;

                    unusedSizeAfter = PageSize - copySize;
                }
                else
                {
                    unusedSizeAfter = PageSize;
                }

                Context.Memory.Fill(GetDramAddressFromPa(lastPageFillAddr), unusedSizeAfter, (byte)_ipcFillValue);

                KernelResult result = MapPages(currentVa, 1, dstLastPagePa, permission);

                if (result != KernelResult.Success)
                {
                    CleanUpForError();

                    return result;
                }
            }

            _blockManager.InsertBlock(va, neededPagesCount, state, permission);

            dst = va + (address - addressTruncated);

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

            lock (_blockManager)
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
                    if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong addressTruncated = BitUtils.AlignDown(address, PageSize);
                    ulong addressRounded = BitUtils.AlignUp(address, PageSize);
                    ulong endAddrTruncated = BitUtils.AlignDown(endAddr, PageSize);
                    ulong endAddrRounded = BitUtils.AlignUp(endAddr, PageSize);

                    ulong pagesCount = (endAddrRounded - addressTruncated) / PageSize;

                    KernelResult result = Unmap(addressTruncated, pagesCount);

                    if (result == KernelResult.Success)
                    {
                        _blockManager.InsertBlock(addressTruncated, pagesCount, MemoryState.Unmapped);
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

            ulong addressRounded = BitUtils.AlignUp(address, PageSize);
            ulong addressTruncated = BitUtils.AlignDown(address, PageSize);
            ulong endAddrRounded = BitUtils.AlignUp(endAddr, PageSize);
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
                MemoryAttribute.Borrowed |
                MemoryAttribute.IpcMapped |
                MemoryAttribute.Uncached;

            if (state == MemoryState.IpcBuffer0)
            {
                attributeMask |= MemoryAttribute.DeviceMapped;
            }

            if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
            {
                return KernelResult.OutOfResource;
            }

            // Anything on the client side should see this memory as modified.
            SignalMemoryTracking(addressTruncated, endAddrRounded - addressTruncated, true);

            lock (_blockManager)
            {
                foreach (KMemoryInfo info in IterateOverRange(addressRounded, endAddrTruncated))
                {
                    // Check if the block state matches what we expect.
                    if ((info.State & stateMask) != stateMask ||
                        (info.Attribute & attributeMask) != MemoryAttribute.IpcMapped)
                    {
                        return KernelResult.InvalidMemState;
                    }

                    if (info.Permission != info.SourcePermission && info.IpcRefCount == 1)
                    {
                        ulong blockAddress = GetAddrInRange(info, addressRounded);
                        ulong blockSize = GetSizeInRange(info, addressRounded, endAddrTruncated);

                        ulong blockPagesCount = blockSize / PageSize;

                        KernelResult result = Reprotect(blockAddress, blockPagesCount, info.SourcePermission);

                        if (result != KernelResult.Success)
                        {
                            return result;
                        }
                    }
                }

                _blockManager.InsertBlock(addressRounded, pagesCount, RestoreIpcMappingPermissions);

                return KernelResult.Success;
            }
        }

        private static void SetIpcMappingPermissions(KMemoryBlock block, KMemoryPermission permission)
        {
            block.SetIpcMappingPermission(permission);
        }

        private static void RestoreIpcMappingPermissions(KMemoryBlock block, KMemoryPermission permission)
        {
            block.RestoreIpcMappingPermission();
        }

        public KernelResult GetPagesIfStateEquals(
            ulong address,
            ulong size,
            MemoryState stateMask,
            MemoryState stateExpected,
            KMemoryPermission permissionMask,
            KMemoryPermission permissionExpected,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeExpected,
            KPageList pageList)
        {
            if (!InsideAddrSpace(address, size))
            {
                return KernelResult.InvalidMemState;
            }

            lock (_blockManager)
            {
                if (CheckRange(
                    address,
                    size,
                    stateMask | MemoryState.IsPoolAllocated,
                    stateExpected | MemoryState.IsPoolAllocated,
                    permissionMask,
                    permissionExpected,
                    attributeMask,
                    attributeExpected,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out _,
                    out _,
                    out _))
                {
                    GetPhysicalRegions(address, size, pageList);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
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

        public KernelResult BorrowCodeMemory(KPageList pageList, ulong address, ulong size)
        {
            return SetAttributesAndChangePermission(
                address,
                size,
                MemoryState.CodeMemoryAllowed,
                MemoryState.CodeMemoryAllowed,
                KMemoryPermission.Mask,
                KMemoryPermission.ReadAndWrite,
                MemoryAttribute.Mask,
                MemoryAttribute.None,
                KMemoryPermission.None,
                MemoryAttribute.Borrowed,
                pageList);
        }

        private KernelResult SetAttributesAndChangePermission(
            ulong address,
            ulong size,
            MemoryState stateMask,
            MemoryState stateExpected,
            KMemoryPermission permissionMask,
            KMemoryPermission permissionExpected,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeExpected,
            KMemoryPermission newPermission,
            MemoryAttribute attributeSetMask,
            KPageList pageList = null)
        {
            if (address + size <= address || !InsideAddrSpace(address, size))
            {
                return KernelResult.InvalidMemState;
            }

            lock (_blockManager)
            {
                if (CheckRange(
                    address,
                    size,
                    stateMask | MemoryState.IsPoolAllocated,
                    stateExpected | MemoryState.IsPoolAllocated,
                    permissionMask,
                    permissionExpected,
                    attributeMask,
                    attributeExpected,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState oldState,
                    out KMemoryPermission oldPermission,
                    out MemoryAttribute oldAttribute))
                {
                    ulong pagesCount = size / PageSize;

                    if (pageList != null)
                    {
                        GetPhysicalRegions(address, pagesCount * PageSize, pageList);
                    }

                    if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    if (newPermission == KMemoryPermission.None)
                    {
                        newPermission = oldPermission;
                    }

                    if (newPermission != oldPermission)
                    {
                        KernelResult result = Reprotect(address, pagesCount, newPermission);

                        if (result != KernelResult.Success)
                        {
                            return result;
                        }
                    }

                    MemoryAttribute newAttribute = oldAttribute | attributeSetMask;

                    _blockManager.InsertBlock(address, pagesCount, oldState, newPermission, newAttribute);

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

        public KernelResult UnborrowCodeMemory(ulong address, ulong size, KPageList pageList)
        {
            return ClearAttributesAndChangePermission(
                address,
                size,
                MemoryState.CodeMemoryAllowed,
                MemoryState.CodeMemoryAllowed,
                KMemoryPermission.None,
                KMemoryPermission.None,
                MemoryAttribute.Mask,
                MemoryAttribute.Borrowed,
                KMemoryPermission.ReadAndWrite,
                MemoryAttribute.Borrowed,
                pageList);
        }

        private KernelResult ClearAttributesAndChangePermission(
            ulong address,
            ulong size,
            MemoryState stateMask,
            MemoryState stateExpected,
            KMemoryPermission permissionMask,
            KMemoryPermission permissionExpected,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeExpected,
            KMemoryPermission newPermission,
            MemoryAttribute attributeClearMask,
            KPageList pageList = null)
        {
            if (address + size <= address || !InsideAddrSpace(address, size))
            {
                return KernelResult.InvalidMemState;
            }

            lock (_blockManager)
            {
                if (CheckRange(
                    address,
                    size,
                    stateMask | MemoryState.IsPoolAllocated,
                    stateExpected | MemoryState.IsPoolAllocated,
                    permissionMask,
                    permissionExpected,
                    attributeMask,
                    attributeExpected,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState oldState,
                    out KMemoryPermission oldPermission,
                    out MemoryAttribute oldAttribute))
                {
                    ulong pagesCount = size / PageSize;

                    if (pageList != null)
                    {
                        KPageList currentPageList = new KPageList();

                        GetPhysicalRegions(address, pagesCount * PageSize, currentPageList);

                        if (!currentPageList.IsEqual(pageList))
                        {
                            return KernelResult.InvalidMemRange;
                        }
                    }

                    if (!_slabManager.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    if (newPermission == KMemoryPermission.None)
                    {
                        newPermission = oldPermission;
                    }

                    if (newPermission != oldPermission)
                    {
                        KernelResult result = Reprotect(address, pagesCount, newPermission);

                        if (result != KernelResult.Success)
                        {
                            return result;
                        }
                    }

                    MemoryAttribute newAttribute = oldAttribute & ~attributeClearMask;

                    _blockManager.InsertBlock(address, pagesCount, oldState, newPermission, newAttribute);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
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
            ulong size = info.Size;

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
            ulong address,
            ulong size,
            MemoryState stateMask,
            MemoryState stateExpected,
            KMemoryPermission permissionMask,
            KMemoryPermission permissionExpected,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeExpected,
            MemoryAttribute attributeIgnoreMask,
            out MemoryState outState,
            out KMemoryPermission outPermission,
            out MemoryAttribute outAttribute)
        {
            ulong endAddr = address + size;

            LinkedListNode<KMemoryBlock> node = _blockManager.FindBlockNode(address);

            KMemoryInfo info = node.Value.GetInfo();

            MemoryState firstState = info.State;
            KMemoryPermission firstPermission = info.Permission;
            MemoryAttribute firstAttribute = info.Attribute;

            do
            {
                info = node.Value.GetInfo();

                // Check if the block state matches what we expect.
                if (firstState != info.State ||
                     firstPermission != info.Permission ||
                    (info.Attribute & attributeMask) != attributeExpected ||
                    (firstAttribute | attributeIgnoreMask) != (info.Attribute | attributeIgnoreMask) ||
                    (firstState & stateMask) != stateExpected ||
                    (firstPermission & permissionMask) != permissionExpected)
                {
                    outState = MemoryState.Unmapped;
                    outPermission = KMemoryPermission.None;
                    outAttribute = MemoryAttribute.None;

                    return false;
                }
            }
            while (info.Address + info.Size - 1 < endAddr - 1 && (node = node.Next) != null);

            outState = firstState;
            outPermission = firstPermission;
            outAttribute = firstAttribute & ~attributeIgnoreMask;

            return true;
        }

        private bool CheckRange(
            ulong address,
            ulong size,
            MemoryState stateMask,
            MemoryState stateExpected,
            KMemoryPermission permissionMask,
            KMemoryPermission permissionExpected,
            MemoryAttribute attributeMask,
            MemoryAttribute attributeExpected)
        {
            foreach (KMemoryInfo info in IterateOverRange(address, address + size))
            {
                // Check if the block state matches what we expect.
                if ((info.State & stateMask) != stateExpected ||
                    (info.Permission & permissionMask) != permissionExpected ||
                    (info.Attribute & attributeMask) != attributeExpected)
                {
                    return false;
                }
            }

            return true;
        }

        private IEnumerable<KMemoryInfo> IterateOverRange(ulong start, ulong end)
        {
            LinkedListNode<KMemoryBlock> node = _blockManager.FindBlockNode(start);

            KMemoryInfo info;

            do
            {
                info = node.Value.GetInfo();

                yield return info;
            }
            while (info.Address + info.Size - 1 < end - 1 && (node = node.Next) != null);
        }

        private ulong AllocateVa(ulong regionStart, ulong regionPagesCount, ulong neededPagesCount, int alignment)
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

                    KMemoryInfo info = _blockManager.FindBlock(address).GetInfo();

                    if (info.State != MemoryState.Unmapped)
                    {
                        continue;
                    }

                    ulong currBaseAddr = info.Address + reservedPagesCount * PageSize;
                    ulong currEndAddr = info.Address + info.Size;

                    if (address >= regionStart &&
                        address >= currBaseAddr &&
                        endAddr - 1 <= regionEndAddr - 1 &&
                        endAddr - 1 <= currEndAddr - 1)
                    {
                        break;
                    }
                }

                if (address == 0)
                {
                    ulong aslrPage = GetRandomValue(0, aslrMaxOffset);

                    address = FindFirstFit(
                        regionStart + aslrPage * PageSize,
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
            int alignment,
            ulong reservedStart,
            ulong reservedPagesCount)
        {
            ulong reservedSize = reservedPagesCount * PageSize;

            ulong totalNeededSize = reservedSize + neededPagesCount * PageSize;

            ulong regionEndAddr = regionStart + regionPagesCount * PageSize;

            LinkedListNode<KMemoryBlock> node = _blockManager.FindBlockNode(regionStart);

            KMemoryInfo info = node.Value.GetInfo();

            while (regionEndAddr >= info.Address)
            {
                if (info.State == MemoryState.Unmapped)
                {
                    ulong currBaseAddr = info.Address <= regionStart ? regionStart : info.Address;
                    ulong currEndAddr = info.Address + info.Size - 1;

                    currBaseAddr += reservedSize;

                    ulong address = BitUtils.AlignDown(currBaseAddr, alignment) + reservedStart;

                    if (currBaseAddr > address)
                    {
                        address += (ulong)alignment;
                    }

                    ulong allocationEndAddr = address + totalNeededSize - 1;

                    if (info.Address <= address &&
                        address < allocationEndAddr &&
                        allocationEndAddr <= regionEndAddr &&
                        allocationEndAddr <= currEndAddr)
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

        public bool CanContain(ulong address, ulong size, MemoryState state)
        {
            ulong endAddr = address + size;

            ulong regionBaseAddr = GetBaseAddress(state);
            ulong regionEndAddr = regionBaseAddr + GetSize(state);

            bool InsideRegion()
            {
                return regionBaseAddr <= address &&
                       endAddr > address &&
                       endAddr - 1 <= regionEndAddr - 1;
            }

            bool OutsideHeapRegion()
            {
                return endAddr <= HeapRegionStart || address >= HeapRegionEnd;
            }

            bool OutsideAliasRegion()
            {
                return endAddr <= AliasRegionStart || address >= AliasRegionEnd;
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

        private static ulong GetDramAddressFromPa(ulong pa)
        {
            return pa - DramMemoryMap.DramBase;
        }

        protected KMemoryRegionManager GetMemoryRegionManager()
        {
            return Context.MemoryManager.MemoryRegions[(int)_memRegion];
        }

        public ulong GetMmUsedPages()
        {
            lock (_blockManager)
            {
                return BitUtils.DivRoundUp(GetMmUsedSize(), PageSize);
            }
        }

        private ulong GetMmUsedSize()
        {
            return (ulong)(_blockManager.BlocksCount * KMemoryBlockSize);
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

        /// <summary>
        /// Gets the physical regions that make up the given virtual address region.
        /// If any part of the virtual region is unmapped, null is returned.
        /// </summary>
        /// <param name="va">Virtual address of the range</param>
        /// <param name="size">Size of the range</param>
        /// <param name="pageList">Page list where the ranges will be added</param>
        protected abstract void GetPhysicalRegions(ulong va, ulong size, KPageList pageList);

        /// <summary>
        /// Gets a read-only span of data from CPU mapped memory.
        /// </summary>
        /// <remarks>
        /// This may perform a allocation if the data is not contiguous in memory.
        /// For this reason, the span is read-only, you can't modify the data.
        /// </remarks>
        /// <param name="va">Virtual address of the data</param>
        /// <param name="size">Size of the data</param>
        /// <param name="tracked">True if read tracking is triggered on the span</param>
        /// <returns>A read-only span of the data</returns>
        /// <exception cref="Ryujinx.Memory.InvalidMemoryRegionException">Throw for unhandled invalid or unmapped memory accesses</exception>
        protected abstract ReadOnlySpan<byte> GetSpan(ulong va, int size);

        /// <summary>
        /// Maps a new memory region with the contents of a existing memory region.
        /// </summary>
        /// <param name="src">Source memory region where the data will be taken from</param>
        /// <param name="dst">Destination memory region to map</param>
        /// <param name="pagesCount">Number of pages to map</param>
        /// <param name="oldSrcPermission">Current protection of the source memory region</param>
        /// <param name="newDstPermission">Desired protection for the destination memory region</param>
        /// <returns>Result of the mapping operation</returns>
        protected abstract KernelResult MapMemory(ulong src, ulong dst, ulong pagesCount, KMemoryPermission oldSrcPermission, KMemoryPermission newDstPermission);

        /// <summary>
        /// Unmaps a region of memory that was previously mapped with <see cref="MapMemory"/>.
        /// </summary>
        /// <param name="dst">Destination memory region to be unmapped</param>
        /// <param name="src">Source memory region that was originally remapped</param>
        /// <param name="pagesCount">Number of pages to unmap</param>
        /// <param name="oldDstPermission">Current protection of the destination memory region</param>
        /// <param name="newSrcPermission">Desired protection of the source memory region</param>
        /// <returns>Result of the unmapping operation</returns>
        protected abstract KernelResult UnmapMemory(ulong dst, ulong src, ulong pagesCount, KMemoryPermission oldDstPermission, KMemoryPermission newSrcPermission);

        /// <summary>
        /// Maps a region of memory into the specified physical memory region.
        /// </summary>
        /// <param name="dstVa">Destination virtual address that should be mapped</param>
        /// <param name="pagesCount">Number of pages to map</param>
        /// <param name="srcPa">Physical address where the pages should be mapped. May be ignored if aliasing is not supported</param>
        /// <param name="permission">Permission of the region to be mapped</param>
        /// <param name="shouldFillPages">Indicate if the pages should be filled with the <paramref name="fillValue"/> value</param>
        /// <param name="fillValue">The value used to fill pages when <paramref name="shouldFillPages"/> is set to true</param>
        /// <returns>Result of the mapping operation</returns>
        protected abstract KernelResult MapPages(ulong dstVa, ulong pagesCount, ulong srcPa, KMemoryPermission permission, bool shouldFillPages = false, byte fillValue = 0);

        /// <summary>
        /// Maps a region of memory into the specified physical memory region.
        /// </summary>
        /// <param name="address">Destination virtual address that should be mapped</param>
        /// <param name="pageList">List of physical memory pages where the pages should be mapped. May be ignored if aliasing is not supported</param>
        /// <param name="permission">Permission of the region to be mapped</param>
        /// <param name="shouldFillPages">Indicate if the pages should be filled with the <paramref name="fillValue"/> value</param>
        /// <param name="fillValue">The value used to fill pages when <paramref name="shouldFillPages"/> is set to true</param>
        /// <returns>Result of the mapping operation</returns>
        protected abstract KernelResult MapPages(ulong address, KPageList pageList, KMemoryPermission permission, bool shouldFillPages = false, byte fillValue = 0);

        /// <summary>
        /// Unmaps a region of memory that was previously mapped with one of the page mapping methods.
        /// </summary>
        /// <param name="address">Virtual address of the region to unmap</param>
        /// <param name="pagesCount">Number of pages to unmap</param>
        /// <returns>Result of the unmapping operation</returns>
        protected abstract KernelResult Unmap(ulong address, ulong pagesCount);

        /// <summary>
        /// Changes the permissions of a given virtual memory region.
        /// </summary>
        /// <param name="address">Virtual address of the region to have the permission changes</param>
        /// <param name="pagesCount">Number of pages to have their permissions changed</param>
        /// <param name="permission">New permission</param>
        /// <returns>Result of the permission change operation</returns>
        protected abstract KernelResult Reprotect(ulong address, ulong pagesCount, KMemoryPermission permission);

        /// <summary>
        /// Changes the permissions of a given virtual memory region.
        /// </summary>
        /// <param name="address">Virtual address of the region to have the permission changes</param>
        /// <param name="pagesCount">Number of pages to have their permissions changed</param>
        /// <param name="permission">New permission</param>
        /// <returns>Result of the permission change operation</returns>
        protected abstract KernelResult ReprotectWithAttributes(ulong address, ulong pagesCount, KMemoryPermission permission);

        /// <summary>
        /// Alerts the memory tracking that a given region has been read from or written to.
        /// This should be called before read/write is performed.
        /// </summary>
        /// <param name="va">Virtual address of the region</param>
        /// <param name="size">Size of the region</param>
        protected abstract void SignalMemoryTracking(ulong va, ulong size, bool write);

        /// <summary>
        /// Writes data to CPU mapped memory, with write tracking.
        /// </summary>
        /// <param name="va">Virtual address to write the data into</param>
        /// <param name="data">Data to be written</param>
        /// <exception cref="Ryujinx.Memory.InvalidMemoryRegionException">Throw for unhandled invalid or unmapped memory accesses</exception>
        protected abstract void Write(ulong va, ReadOnlySpan<byte> data);
    }
}