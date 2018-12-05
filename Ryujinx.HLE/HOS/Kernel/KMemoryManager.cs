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

        private LinkedList<KMemoryBlock> Blocks;

        private MemoryManager CpuMemory;

        private Horizon System;

        public ulong AddrSpaceStart { get; private set; }
        public ulong AddrSpaceEnd   { get; private set; }

        public ulong CodeRegionStart { get; private set; }
        public ulong CodeRegionEnd   { get; private set; }

        public ulong HeapRegionStart { get; private set; }
        public ulong HeapRegionEnd   { get; private set; }

        private ulong CurrentHeapAddr;

        public ulong AliasRegionStart { get; private set; }
        public ulong AliasRegionEnd   { get; private set; }

        public ulong StackRegionStart { get; private set; }
        public ulong StackRegionEnd   { get; private set; }

        public ulong TlsIoRegionStart { get; private set; }
        public ulong TlsIoRegionEnd   { get; private set; }

        private ulong HeapCapacity;

        public ulong PhysicalMemoryUsage { get; private set; }

        private MemoryRegion MemRegion;

        private bool AslrDisabled;

        public int AddrSpaceWidth { get; private set; }

        private bool IsKernel;
        private bool AslrEnabled;

        private KMemoryBlockAllocator BlockAllocator;

        private int ContextId;

        private MersenneTwister RandomNumberGenerator;

        public KMemoryManager(Horizon System, MemoryManager CpuMemory)
        {
            this.System    = System;
            this.CpuMemory = CpuMemory;

            Blocks = new LinkedList<KMemoryBlock>();
        }

        private static readonly int[] AddrSpaceSizes = new int[] { 32, 36, 32, 39 };

        public KernelResult InitializeForProcess(
            AddressSpaceType      AddrSpaceType,
            bool                  AslrEnabled,
            bool                  AslrDisabled,
            MemoryRegion          MemRegion,
            ulong                 Address,
            ulong                 Size,
            KMemoryBlockAllocator BlockAllocator)
        {
            if ((uint)AddrSpaceType > (uint)AddressSpaceType.Addr39Bits)
            {
                throw new ArgumentException(nameof(AddrSpaceType));
            }

            ContextId = System.ContextIdManager.GetId();

            ulong AddrSpaceBase = 0;
            ulong AddrSpaceSize = 1UL << AddrSpaceSizes[(int)AddrSpaceType];

            KernelResult Result = CreateUserAddressSpace(
                AddrSpaceType,
                AslrEnabled,
                AslrDisabled,
                AddrSpaceBase,
                AddrSpaceSize,
                MemRegion,
                Address,
                Size,
                BlockAllocator);

            if (Result != KernelResult.Success)
            {
                System.ContextIdManager.PutId(ContextId);
            }

            return Result;
        }

        private class Region
        {
            public ulong Start;
            public ulong End;
            public ulong Size;
            public ulong AslrOffset;
        }

        private KernelResult CreateUserAddressSpace(
            AddressSpaceType      AddrSpaceType,
            bool                  AslrEnabled,
            bool                  AslrDisabled,
            ulong                 AddrSpaceStart,
            ulong                 AddrSpaceEnd,
            MemoryRegion          MemRegion,
            ulong                 Address,
            ulong                 Size,
            KMemoryBlockAllocator BlockAllocator)
        {
            ulong EndAddr = Address + Size;

            Region AliasRegion = new Region();
            Region HeapRegion  = new Region();
            Region StackRegion = new Region();
            Region TlsIoRegion = new Region();

            ulong CodeRegionSize;
            ulong StackAndTlsIoStart;
            ulong StackAndTlsIoEnd;
            ulong BaseAddress;

            switch (AddrSpaceType)
            {
                case AddressSpaceType.Addr32Bits:
                    AliasRegion.Size   = 0x40000000;
                    HeapRegion.Size    = 0x40000000;
                    StackRegion.Size   = 0;
                    TlsIoRegion.Size   = 0;
                    CodeRegionStart    = 0x200000;
                    CodeRegionSize     = 0x3fe00000;
                    StackAndTlsIoStart = 0x200000;
                    StackAndTlsIoEnd   = 0x40000000;
                    BaseAddress        = 0x200000;
                    AddrSpaceWidth     = 32;
                    break;

                case AddressSpaceType.Addr36Bits:
                    AliasRegion.Size   = 0x180000000;
                    HeapRegion.Size    = 0x180000000;
                    StackRegion.Size   = 0;
                    TlsIoRegion.Size   = 0;
                    CodeRegionStart    = 0x8000000;
                    CodeRegionSize     = 0x78000000;
                    StackAndTlsIoStart = 0x8000000;
                    StackAndTlsIoEnd   = 0x80000000;
                    BaseAddress        = 0x8000000;
                    AddrSpaceWidth     = 36;
                    break;

                case AddressSpaceType.Addr32BitsNoMap:
                    AliasRegion.Size   = 0;
                    HeapRegion.Size    = 0x80000000;
                    StackRegion.Size   = 0;
                    TlsIoRegion.Size   = 0;
                    CodeRegionStart    = 0x200000;
                    CodeRegionSize     = 0x3fe00000;
                    StackAndTlsIoStart = 0x200000;
                    StackAndTlsIoEnd   = 0x40000000;
                    BaseAddress        = 0x200000;
                    AddrSpaceWidth     = 32;
                    break;

                case AddressSpaceType.Addr39Bits:
                    AliasRegion.Size   = 0x1000000000;
                    HeapRegion.Size    = 0x180000000;
                    StackRegion.Size   = 0x80000000;
                    TlsIoRegion.Size   = 0x1000000000;
                    CodeRegionStart    = BitUtils.AlignDown(Address, 0x200000);
                    CodeRegionSize     = BitUtils.AlignUp  (EndAddr, 0x200000) - CodeRegionStart;
                    StackAndTlsIoStart = 0;
                    StackAndTlsIoEnd   = 0;
                    BaseAddress        = 0x8000000;
                    AddrSpaceWidth     = 39;
                    break;

                default: throw new ArgumentException(nameof(AddrSpaceType));
            }

            CodeRegionEnd = CodeRegionStart + CodeRegionSize;

            ulong MapBaseAddress;
            ulong MapAvailableSize;

            if (CodeRegionStart - BaseAddress >= AddrSpaceEnd - CodeRegionEnd)
            {
                //Has more space before the start of the code region.
                MapBaseAddress   = BaseAddress;
                MapAvailableSize = CodeRegionStart - BaseAddress;
            }
            else
            {
                //Has more space after the end of the code region.
                MapBaseAddress   = CodeRegionEnd;
                MapAvailableSize = AddrSpaceEnd - CodeRegionEnd;
            }

            ulong MapTotalSize = AliasRegion.Size + HeapRegion.Size + StackRegion.Size + TlsIoRegion.Size;

            ulong AslrMaxOffset = MapAvailableSize - MapTotalSize;

            this.AslrEnabled = AslrEnabled;

            this.AddrSpaceStart = AddrSpaceStart;
            this.AddrSpaceEnd   = AddrSpaceEnd;

            this.BlockAllocator = BlockAllocator;

            if (MapAvailableSize < MapTotalSize)
            {
                return KernelResult.OutOfMemory;
            }

            if (AslrEnabled)
            {
                AliasRegion.AslrOffset = GetRandomValue(0, AslrMaxOffset >> 21) << 21;
                HeapRegion.AslrOffset  = GetRandomValue(0, AslrMaxOffset >> 21) << 21;
                StackRegion.AslrOffset = GetRandomValue(0, AslrMaxOffset >> 21) << 21;
                TlsIoRegion.AslrOffset = GetRandomValue(0, AslrMaxOffset >> 21) << 21;
            }

            //Regions are sorted based on ASLR offset.
            //When ASLR is disabled, the order is Map, Heap, NewMap and TlsIo.
            AliasRegion.Start = MapBaseAddress    + AliasRegion.AslrOffset;
            AliasRegion.End   = AliasRegion.Start + AliasRegion.Size;
            HeapRegion.Start  = MapBaseAddress    + HeapRegion.AslrOffset;
            HeapRegion.End    = HeapRegion.Start  + HeapRegion.Size;
            StackRegion.Start = MapBaseAddress    + StackRegion.AslrOffset;
            StackRegion.End   = StackRegion.Start + StackRegion.Size;
            TlsIoRegion.Start = MapBaseAddress    + TlsIoRegion.AslrOffset;
            TlsIoRegion.End   = TlsIoRegion.Start + TlsIoRegion.Size;

            SortRegion(HeapRegion, AliasRegion);

            if (StackRegion.Size != 0)
            {
                SortRegion(StackRegion, AliasRegion);
                SortRegion(StackRegion, HeapRegion);
            }
            else
            {
                StackRegion.Start = StackAndTlsIoStart;
                StackRegion.End   = StackAndTlsIoEnd;
            }

            if (TlsIoRegion.Size != 0)
            {
                SortRegion(TlsIoRegion, AliasRegion);
                SortRegion(TlsIoRegion, HeapRegion);
                SortRegion(TlsIoRegion, StackRegion);
            }
            else
            {
                TlsIoRegion.Start = StackAndTlsIoStart;
                TlsIoRegion.End   = StackAndTlsIoEnd;
            }

            AliasRegionStart = AliasRegion.Start;
            AliasRegionEnd   = AliasRegion.End;
            HeapRegionStart  = HeapRegion.Start;
            HeapRegionEnd    = HeapRegion.End;
            StackRegionStart = StackRegion.Start;
            StackRegionEnd   = StackRegion.End;
            TlsIoRegionStart = TlsIoRegion.Start;
            TlsIoRegionEnd   = TlsIoRegion.End;

            CurrentHeapAddr     = HeapRegionStart;
            HeapCapacity        = 0;
            PhysicalMemoryUsage = 0;

            this.MemRegion    = MemRegion;
            this.AslrDisabled = AslrDisabled;

            return InitializeBlocks(AddrSpaceStart, AddrSpaceEnd);
        }

        private ulong GetRandomValue(ulong Min, ulong Max)
        {
            return (ulong)GetRandomValue((long)Min, (long)Max);
        }

        private long GetRandomValue(long Min, long Max)
        {
            if (RandomNumberGenerator == null)
            {
                RandomNumberGenerator = new MersenneTwister(0);
            }

            return RandomNumberGenerator.GenRandomNumber(Min, Max);
        }

        private static void SortRegion(Region Lhs, Region Rhs)
        {
            if (Lhs.AslrOffset < Rhs.AslrOffset)
            {
                Rhs.Start += Lhs.Size;
                Rhs.End   += Lhs.Size;
            }
            else
            {
                Lhs.Start += Rhs.Size;
                Lhs.End   += Rhs.Size;
            }
        }

        private KernelResult InitializeBlocks(ulong AddrSpaceStart, ulong AddrSpaceEnd)
        {
            //First insertion will always need only a single block,
            //because there's nothing else to split.
            if (!BlockAllocator.CanAllocate(1))
            {
                return KernelResult.OutOfResource;
            }

            ulong AddrSpacePagesCount = (AddrSpaceEnd - AddrSpaceStart) / PageSize;

            InsertBlock(AddrSpaceStart, AddrSpacePagesCount, MemoryState.Unmapped);

            return KernelResult.Success;
        }

        public KernelResult MapPages(
            ulong            Address,
            KPageList        PageList,
            MemoryState      State,
            MemoryPermission Permission)
        {
            ulong PagesCount = PageList.GetPagesCount();

            ulong Size = PagesCount * PageSize;

            if (!ValidateRegionForState(Address, Size, State))
            {
                return KernelResult.InvalidMemState;
            }

            lock (Blocks)
            {
                if (!IsUnmapped(Address, PagesCount * PageSize))
                {
                    return KernelResult.InvalidMemState;
                }

                if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                KernelResult Result = MapPages(Address, PageList, Permission);

                if (Result == KernelResult.Success)
                {
                    InsertBlock(Address, PagesCount, State, Permission);
                }

                return Result;
            }
        }

        public KernelResult UnmapPages(ulong Address, KPageList PageList, MemoryState StateExpected)
        {
            ulong PagesCount = PageList.GetPagesCount();

            ulong Size = PagesCount * PageSize;

            ulong EndAddr = Address + Size;

            ulong AddrSpacePagesCount = (AddrSpaceEnd - AddrSpaceStart) / PageSize;

            if (AddrSpaceStart > Address)
            {
                return KernelResult.InvalidMemState;
            }

            if (AddrSpacePagesCount < PagesCount)
            {
                return KernelResult.InvalidMemState;
            }

            if (EndAddr - 1 > AddrSpaceEnd - 1)
            {
                return KernelResult.InvalidMemState;
            }

            lock (Blocks)
            {
                KPageList CurrentPageList = new KPageList();

                AddVaRangeToPageList(CurrentPageList, Address, PagesCount);

                if (!CurrentPageList.IsEqual(PageList))
                {
                    return KernelResult.InvalidMemRange;
                }

                if (CheckRange(
                    Address,
                    Size,
                    MemoryState.Mask,
                    StateExpected,
                    MemoryPermission.None,
                    MemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState State,
                    out _,
                    out _))
                {
                    if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    KernelResult Result = MmuUnmap(Address, PagesCount);

                    if (Result == KernelResult.Success)
                    {
                        InsertBlock(Address, PagesCount, MemoryState.Unmapped);
                    }

                    return Result;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult MapNormalMemory(long Address, long Size, MemoryPermission Permission)
        {
            //TODO.
            return KernelResult.Success;
        }

        public KernelResult MapIoMemory(long Address, long Size, MemoryPermission Permission)
        {
            //TODO.
            return KernelResult.Success;
        }

        public KernelResult AllocateOrMapPa(
            ulong            NeededPagesCount,
            int              Alignment,
            ulong            SrcPa,
            bool             Map,
            ulong            RegionStart,
            ulong            RegionPagesCount,
            MemoryState      State,
            MemoryPermission Permission,
            out ulong        Address)
        {
            Address = 0;

            ulong RegionSize = RegionPagesCount * PageSize;

            ulong RegionEndAddr = RegionStart + RegionSize;

            if (!ValidateRegionForState(RegionStart, RegionSize, State))
            {
                return KernelResult.InvalidMemState;
            }

            if (RegionPagesCount <= NeededPagesCount)
            {
                return KernelResult.OutOfMemory;
            }

            ulong ReservedPagesCount = IsKernel ? 1UL : 4UL;

            lock (Blocks)
            {
                if (AslrEnabled)
                {
                    ulong TotalNeededSize = (ReservedPagesCount + NeededPagesCount) * PageSize;

                    ulong RemainingPages = RegionPagesCount - NeededPagesCount;

                    ulong AslrMaxOffset = ((RemainingPages + ReservedPagesCount) * PageSize) / (ulong)Alignment;

                    for (int Attempt = 0; Attempt < 8; Attempt++)
                    {
                        Address = BitUtils.AlignDown(RegionStart + GetRandomValue(0, AslrMaxOffset) * (ulong)Alignment, Alignment);

                        ulong EndAddr = Address + TotalNeededSize;

                        KMemoryInfo Info = FindBlock(Address).GetInfo();

                        if (Info.State != MemoryState.Unmapped)
                        {
                            continue;
                        }

                        ulong CurrBaseAddr = Info.Address + ReservedPagesCount * PageSize;
                        ulong CurrEndAddr  = Info.Address + Info.Size;

                        if (Address     >= RegionStart       &&
                            Address     >= CurrBaseAddr      &&
                            EndAddr - 1 <= RegionEndAddr - 1 &&
                            EndAddr - 1 <= CurrEndAddr   - 1)
                        {
                            break;
                        }
                    }

                    if (Address == 0)
                    {
                        ulong AslrPage = GetRandomValue(0, AslrMaxOffset);

                        Address = FindFirstFit(
                            RegionStart      + AslrPage * PageSize,
                            RegionPagesCount - AslrPage,
                            NeededPagesCount,
                            Alignment,
                            0,
                            ReservedPagesCount);
                    }
                }

                if (Address == 0)
                {
                    Address = FindFirstFit(
                        RegionStart,
                        RegionPagesCount,
                        NeededPagesCount,
                        Alignment,
                        0,
                        ReservedPagesCount);
                }

                if (Address == 0)
                {
                    return KernelResult.OutOfMemory;
                }

                if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                MemoryOperation Operation = Map
                    ? MemoryOperation.MapPa
                    : MemoryOperation.Allocate;

                KernelResult Result = DoMmuOperation(
                    Address,
                    NeededPagesCount,
                    SrcPa,
                    Map,
                    Permission,
                    Operation);

                if (Result != KernelResult.Success)
                {
                    return Result;
                }

                InsertBlock(Address, NeededPagesCount, State, Permission);
            }

            return KernelResult.Success;
        }

        public KernelResult MapNewProcessCode(
            ulong            Address,
            ulong            PagesCount,
            MemoryState      State,
            MemoryPermission Permission)
        {
            ulong Size = PagesCount * PageSize;

            if (!ValidateRegionForState(Address, Size, State))
            {
                return KernelResult.InvalidMemState;
            }

            lock (Blocks)
            {
                if (!IsUnmapped(Address, Size))
                {
                    return KernelResult.InvalidMemState;
                }

                if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                KernelResult Result = DoMmuOperation(
                    Address,
                    PagesCount,
                    0,
                    false,
                    Permission,
                    MemoryOperation.Allocate);

                if (Result == KernelResult.Success)
                {
                    InsertBlock(Address, PagesCount, State, Permission);
                }

                return Result;
            }
        }

        public KernelResult MapProcessCodeMemory(ulong Dst, ulong Src, ulong Size)
        {
            ulong PagesCount = Size / PageSize;

            lock (Blocks)
            {
                bool Success = CheckRange(
                    Src,
                    Size,
                    MemoryState.Mask,
                    MemoryState.Heap,
                    MemoryPermission.Mask,
                    MemoryPermission.ReadAndWrite,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState      State,
                    out MemoryPermission Permission,
                    out _);

                Success &= IsUnmapped(Dst, Size);

                if (Success)
                {
                    if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion * 2))
                    {
                        return KernelResult.OutOfResource;
                    }

                    KPageList PageList = new KPageList();

                    AddVaRangeToPageList(PageList, Src, PagesCount);

                    KernelResult Result = MmuChangePermission(Src, PagesCount, MemoryPermission.None);

                    if (Result != KernelResult.Success)
                    {
                        return Result;
                    }

                    Result = MapPages(Dst, PageList, MemoryPermission.None);

                    if (Result != KernelResult.Success)
                    {
                        MmuChangePermission(Src, PagesCount, Permission);

                        return Result;
                    }

                    InsertBlock(Src, PagesCount, State, MemoryPermission.None, MemoryAttribute.Borrowed);
                    InsertBlock(Dst, PagesCount, MemoryState.ModCodeStatic);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult UnmapProcessCodeMemory(ulong Dst, ulong Src, ulong Size)
        {
            ulong PagesCount = Size / PageSize;

            lock (Blocks)
            {
                bool Success = CheckRange(
                    Src,
                    Size,
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

                Success &= CheckRange(
                    Dst,
                    PageSize,
                    MemoryState.UnmapProcessCodeMemoryAllowed,
                    MemoryState.UnmapProcessCodeMemoryAllowed,
                    MemoryPermission.None,
                    MemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState State,
                    out _,
                    out _);

                Success &= CheckRange(
                    Dst,
                    Size,
                    MemoryState.Mask,
                    State,
                    MemoryPermission.None,
                    MemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None);

                if (Success)
                {
                    KernelResult Result = MmuUnmap(Dst, PagesCount);

                    if (Result != KernelResult.Success)
                    {
                        return Result;
                    }

                    //TODO: Missing some checks here.

                    if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion * 2))
                    {
                        return KernelResult.OutOfResource;
                    }

                    InsertBlock(Dst, PagesCount, MemoryState.Unmapped);
                    InsertBlock(Src, PagesCount, MemoryState.Heap, MemoryPermission.ReadAndWrite);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult SetHeapSize(ulong Size, out ulong Address)
        {
            Address = 0;

            if (Size > HeapRegionEnd - HeapRegionStart)
            {
                return KernelResult.OutOfMemory;
            }

            KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

            ulong CurrentHeapSize = GetHeapSize();

            if (CurrentHeapSize <= Size)
            {
                //Expand.
                ulong DiffSize = Size - CurrentHeapSize;

                lock (Blocks)
                {
                    if (CurrentProcess.ResourceLimit != null && DiffSize != 0 &&
                       !CurrentProcess.ResourceLimit.Reserve(LimitableResource.Memory, DiffSize))
                    {
                        return KernelResult.ResLimitExceeded;
                    }

                    ulong PagesCount = DiffSize / PageSize;

                    KMemoryRegionManager Region = GetMemoryRegionManager();

                    KernelResult Result = Region.AllocatePages(PagesCount, AslrDisabled, out KPageList PageList);

                    void CleanUpForError()
                    {
                        if (PageList != null)
                        {
                            Region.FreePages(PageList);
                        }

                        if (CurrentProcess.ResourceLimit != null && DiffSize != 0)
                        {
                            CurrentProcess.ResourceLimit.Release(LimitableResource.Memory, DiffSize);
                        }
                    }

                    if (Result != KernelResult.Success)
                    {
                        CleanUpForError();

                        return Result;
                    }

                    if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        CleanUpForError();

                        return KernelResult.OutOfResource;
                    }

                    if (!IsUnmapped(CurrentHeapAddr, DiffSize))
                    {
                        CleanUpForError();

                        return KernelResult.InvalidMemState;
                    }

                    Result = DoMmuOperation(
                        CurrentHeapAddr,
                        PagesCount,
                        PageList,
                        MemoryPermission.ReadAndWrite,
                        MemoryOperation.MapVa);

                    if (Result != KernelResult.Success)
                    {
                        CleanUpForError();

                        return Result;
                    }

                    InsertBlock(CurrentHeapAddr, PagesCount, MemoryState.Heap, MemoryPermission.ReadAndWrite);
                }
            }
            else
            {
                //Shrink.
                ulong FreeAddr = HeapRegionStart + Size;
                ulong DiffSize = CurrentHeapSize - Size;

                lock (Blocks)
                {
                    if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    if (!CheckRange(
                        FreeAddr,
                        DiffSize,
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

                    ulong PagesCount = DiffSize / PageSize;

                    KernelResult Result = MmuUnmap(FreeAddr, PagesCount);

                    if (Result != KernelResult.Success)
                    {
                        return Result;
                    }

                    CurrentProcess.ResourceLimit?.Release(LimitableResource.Memory, BitUtils.AlignDown(DiffSize, PageSize));

                    InsertBlock(FreeAddr, PagesCount, MemoryState.Unmapped);
                }
            }

            CurrentHeapAddr = HeapRegionStart + Size;

            Address = HeapRegionStart;

            return KernelResult.Success;
        }

        public ulong GetTotalHeapSize()
        {
            lock (Blocks)
            {
                return GetHeapSize() + PhysicalMemoryUsage;
            }
        }

        private ulong GetHeapSize()
        {
            return CurrentHeapAddr - HeapRegionStart;
        }

        public KernelResult SetHeapCapacity(ulong Capacity)
        {
            lock (Blocks)
            {
                HeapCapacity = Capacity;
            }

            return KernelResult.Success;
        }

        public KernelResult SetMemoryAttribute(
            ulong           Address,
            ulong           Size,
            MemoryAttribute AttributeMask,
            MemoryAttribute AttributeValue)
        {
            lock (Blocks)
            {
                if (CheckRange(
                    Address,
                    Size,
                    MemoryState.AttributeChangeAllowed,
                    MemoryState.AttributeChangeAllowed,
                    MemoryPermission.None,
                    MemoryPermission.None,
                    MemoryAttribute.BorrowedAndIpcMapped,
                    MemoryAttribute.None,
                    MemoryAttribute.DeviceMappedAndUncached,
                    out MemoryState      State,
                    out MemoryPermission Permission,
                    out MemoryAttribute  Attribute))
                {
                    if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong PagesCount = Size / PageSize;

                    Attribute &= ~AttributeMask;
                    Attribute |=  AttributeMask & AttributeValue;

                    InsertBlock(Address, PagesCount, State, Permission, Attribute);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KMemoryInfo QueryMemory(ulong Address)
        {
            if (Address >= AddrSpaceStart &&
                Address <  AddrSpaceEnd)
            {
                lock (Blocks)
                {
                    return FindBlock(Address).GetInfo();
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

        public KernelResult Map(ulong Dst, ulong Src, ulong Size)
        {
            bool Success;

            lock (Blocks)
            {
                Success = CheckRange(
                    Src,
                    Size,
                    MemoryState.MapAllowed,
                    MemoryState.MapAllowed,
                    MemoryPermission.Mask,
                    MemoryPermission.ReadAndWrite,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState SrcState,
                    out _,
                    out _);

                Success &= IsUnmapped(Dst, Size);

                if (Success)
                {
                    if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion * 2))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong PagesCount = Size / PageSize;

                    KPageList PageList = new KPageList();

                    AddVaRangeToPageList(PageList, Src, PagesCount);

                    KernelResult Result = MmuChangePermission(Src, PagesCount, MemoryPermission.None);

                    if (Result != KernelResult.Success)
                    {
                        return Result;
                    }

                    Result = MapPages(Dst, PageList, MemoryPermission.ReadAndWrite);

                    if (Result != KernelResult.Success)
                    {
                        if (MmuChangePermission(Src, PagesCount, MemoryPermission.ReadAndWrite) != KernelResult.Success)
                        {
                            throw new InvalidOperationException("Unexpected failure reverting memory permission.");
                        }

                        return Result;
                    }

                    InsertBlock(Src, PagesCount, SrcState, MemoryPermission.None, MemoryAttribute.Borrowed);
                    InsertBlock(Dst, PagesCount, MemoryState.Stack, MemoryPermission.ReadAndWrite);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult UnmapForKernel(ulong Address, ulong PagesCount, MemoryState StateExpected)
        {
            ulong Size = PagesCount * PageSize;

            lock (Blocks)
            {
                if (CheckRange(
                    Address,
                    Size,
                    MemoryState.Mask,
                    StateExpected,
                    MemoryPermission.None,
                    MemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out _,
                    out _,
                    out _))
                {
                    if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    KernelResult Result = MmuUnmap(Address, PagesCount);

                    if (Result == KernelResult.Success)
                    {
                        InsertBlock(Address, PagesCount, MemoryState.Unmapped);
                    }

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult Unmap(ulong Dst, ulong Src, ulong Size)
        {
            bool Success;

            lock (Blocks)
            {
                Success = CheckRange(
                    Src,
                    Size,
                    MemoryState.MapAllowed,
                    MemoryState.MapAllowed,
                    MemoryPermission.Mask,
                    MemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.Borrowed,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState SrcState,
                    out _,
                    out _);

                Success &= CheckRange(
                    Dst,
                    Size,
                    MemoryState.Mask,
                    MemoryState.Stack,
                    MemoryPermission.None,
                    MemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out _,
                    out MemoryPermission DstPermission,
                    out _);

                if (Success)
                {
                    if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion * 2))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong PagesCount = Size / PageSize;

                    KPageList SrcPageList = new KPageList();
                    KPageList DstPageList = new KPageList();

                    AddVaRangeToPageList(SrcPageList, Src, PagesCount);
                    AddVaRangeToPageList(DstPageList, Dst, PagesCount);

                    if (!DstPageList.IsEqual(SrcPageList))
                    {
                        return KernelResult.InvalidMemRange;
                    }

                    KernelResult Result = MmuUnmap(Dst, PagesCount);

                    if (Result != KernelResult.Success)
                    {
                        return Result;
                    }

                    Result = MmuChangePermission(Src, PagesCount, MemoryPermission.ReadAndWrite);

                    if (Result != KernelResult.Success)
                    {
                        MapPages(Dst, DstPageList, DstPermission);

                        return Result;
                    }

                    InsertBlock(Src, PagesCount, SrcState, MemoryPermission.ReadAndWrite);
                    InsertBlock(Dst, PagesCount, MemoryState.Unmapped);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult ReserveTransferMemory(ulong Address, ulong Size, MemoryPermission Permission)
        {
            lock (Blocks)
            {
                if (CheckRange(
                    Address,
                    Size,
                    MemoryState.TransferMemoryAllowed | MemoryState.IsPoolAllocated,
                    MemoryState.TransferMemoryAllowed | MemoryState.IsPoolAllocated,
                    MemoryPermission.Mask,
                    MemoryPermission.ReadAndWrite,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState State,
                    out _,
                    out MemoryAttribute Attribute))
                {
                    //TODO: Missing checks.

                    if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong PagesCount = Size / PageSize;

                    Attribute |= MemoryAttribute.Borrowed;

                    InsertBlock(Address, PagesCount, State, Permission, Attribute);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult ResetTransferMemory(ulong Address, ulong Size)
        {
            lock (Blocks)
            {
                if (CheckRange(
                    Address,
                    Size,
                    MemoryState.TransferMemoryAllowed | MemoryState.IsPoolAllocated,
                    MemoryState.TransferMemoryAllowed | MemoryState.IsPoolAllocated,
                    MemoryPermission.None,
                    MemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.Borrowed,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState State,
                    out _,
                    out _))
                {
                    if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                    {
                        return KernelResult.OutOfResource;
                    }

                    ulong PagesCount = Size / PageSize;

                    InsertBlock(Address, PagesCount, State, MemoryPermission.ReadAndWrite);

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult SetProcessMemoryPermission(ulong Address, ulong Size, MemoryPermission Permission)
        {
            lock (Blocks)
            {
                if (CheckRange(
                    Address,
                    Size,
                    MemoryState.ProcessPermissionChangeAllowed,
                    MemoryState.ProcessPermissionChangeAllowed,
                    MemoryPermission.None,
                    MemoryPermission.None,
                    MemoryAttribute.Mask,
                    MemoryAttribute.None,
                    MemoryAttribute.IpcAndDeviceMapped,
                    out MemoryState      OldState,
                    out MemoryPermission OldPermission,
                    out _))
                {
                    MemoryState NewState = OldState;

                    //If writing into the code region is allowed, then we need
                    //to change it to mutable.
                    if ((Permission & MemoryPermission.Write) != 0)
                    {
                        if (OldState == MemoryState.CodeStatic)
                        {
                            NewState = MemoryState.CodeMutable;
                        }
                        else if (OldState == MemoryState.ModCodeStatic)
                        {
                            NewState = MemoryState.ModCodeMutable;
                        }
                        else
                        {
                            throw new InvalidOperationException($"Memory state \"{OldState}\" not valid for this operation.");
                        }
                    }

                    if (NewState != OldState || Permission != OldPermission)
                    {
                        if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                        {
                            return KernelResult.OutOfResource;
                        }

                        ulong PagesCount = Size / PageSize;

                        MemoryOperation Operation = (Permission & MemoryPermission.Execute) != 0
                            ? MemoryOperation.ChangePermsAndAttributes
                            : MemoryOperation.ChangePermRw;

                        KernelResult Result = DoMmuOperation(Address, PagesCount, 0, false, Permission, Operation);

                        if (Result != KernelResult.Success)
                        {
                            return Result;
                        }

                        InsertBlock(Address, PagesCount, NewState, Permission);
                    }

                    return KernelResult.Success;
                }
                else
                {
                    return KernelResult.InvalidMemState;
                }
            }
        }

        public KernelResult MapPhysicalMemory(ulong Address, ulong Size)
        {
            ulong EndAddr = Address + Size;

            lock (Blocks)
            {
                ulong MappedSize = 0;

                KMemoryInfo Info;

                LinkedListNode<KMemoryBlock> Node = FindBlockNode(Address);

                do
                {
                    Info = Node.Value.GetInfo();

                    if (Info.State != MemoryState.Unmapped)
                    {
                        MappedSize += GetSizeInRange(Info, Address, EndAddr);
                    }

                    Node = Node.Next;
                }
                while (Info.Address + Info.Size < EndAddr && Node != null);

                if (MappedSize == Size)
                {
                    return KernelResult.Success;
                }

                ulong RemainingSize = Size - MappedSize;

                ulong RemainingPages = RemainingSize / PageSize;

                KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

                if (CurrentProcess.ResourceLimit != null &&
                   !CurrentProcess.ResourceLimit.Reserve(LimitableResource.Memory, RemainingSize))
                {
                    return KernelResult.ResLimitExceeded;
                }

                KMemoryRegionManager Region = GetMemoryRegionManager();

                KernelResult Result = Region.AllocatePages(RemainingPages, AslrDisabled, out KPageList PageList);

                void CleanUpForError()
                {
                    if (PageList != null)
                    {
                        Region.FreePages(PageList);
                    }

                    CurrentProcess.ResourceLimit?.Release(LimitableResource.Memory, RemainingSize);
                }

                if (Result != KernelResult.Success)
                {
                    CleanUpForError();

                    return Result;
                }

                if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    CleanUpForError();

                    return KernelResult.OutOfResource;
                }

                MapPhysicalMemory(PageList, Address, EndAddr);

                PhysicalMemoryUsage += RemainingSize;

                ulong PagesCount = Size / PageSize;

                InsertBlock(
                    Address,
                    PagesCount,
                    MemoryState.Unmapped,
                    MemoryPermission.None,
                    MemoryAttribute.None,
                    MemoryState.Heap,
                    MemoryPermission.ReadAndWrite,
                    MemoryAttribute.None);
            }

            return KernelResult.Success;
        }

        public KernelResult UnmapPhysicalMemory(ulong Address, ulong Size)
        {
            ulong EndAddr = Address + Size;

            lock (Blocks)
            {
                //Scan, ensure that the region can be unmapped (all blocks are heap or
                //already unmapped), fill pages list for freeing memory.
                ulong HeapMappedSize = 0;

                KPageList PageList = new KPageList();

                KMemoryInfo Info;

                LinkedListNode<KMemoryBlock> BaseNode = FindBlockNode(Address);

                LinkedListNode<KMemoryBlock> Node = BaseNode;

                do
                {
                    Info = Node.Value.GetInfo();

                    if (Info.State == MemoryState.Heap)
                    {
                        if (Info.Attribute != MemoryAttribute.None)
                        {
                            return KernelResult.InvalidMemState;
                        }

                        ulong BlockSize    = GetSizeInRange(Info, Address, EndAddr);
                        ulong BlockAddress = GetAddrInRange(Info, Address);

                        AddVaRangeToPageList(PageList, BlockAddress, BlockSize / PageSize);

                        HeapMappedSize += BlockSize;
                    }
                    else if (Info.State != MemoryState.Unmapped)
                    {
                        return KernelResult.InvalidMemState;
                    }

                    Node = Node.Next;
                }
                while (Info.Address + Info.Size < EndAddr && Node != null);

                if (HeapMappedSize == 0)
                {
                    return KernelResult.Success;
                }

                if (!BlockAllocator.CanAllocate(MaxBlocksNeededForInsertion))
                {
                    return KernelResult.OutOfResource;
                }

                //Try to unmap all the heap mapped memory inside range.
                KernelResult Result = KernelResult.Success;

                Node = BaseNode;

                do
                {
                    Info = Node.Value.GetInfo();

                    if (Info.State == MemoryState.Heap)
                    {
                        ulong BlockSize    = GetSizeInRange(Info, Address, EndAddr);
                        ulong BlockAddress = GetAddrInRange(Info, Address);

                        ulong BlockPagesCount = BlockSize / PageSize;

                        Result = MmuUnmap(BlockAddress, BlockPagesCount);

                        if (Result != KernelResult.Success)
                        {
                            //If we failed to unmap, we need to remap everything back again.
                            MapPhysicalMemory(PageList, Address, BlockAddress + BlockSize);

                            break;
                        }
                    }

                    Node = Node.Next;
                }
                while (Info.Address + Info.Size < EndAddr && Node != null);

                if (Result == KernelResult.Success)
                {
                    GetMemoryRegionManager().FreePages(PageList);

                    PhysicalMemoryUsage -= HeapMappedSize;

                    KProcess CurrentProcess = System.Scheduler.GetCurrentProcess();

                    CurrentProcess.ResourceLimit?.Release(LimitableResource.Memory, HeapMappedSize);

                    ulong PagesCount = Size / PageSize;

                    InsertBlock(Address, PagesCount, MemoryState.Unmapped);
                }

                return Result;
            }
        }

        private void MapPhysicalMemory(KPageList PageList, ulong Address, ulong EndAddr)
        {
            KMemoryInfo Info;

            LinkedListNode<KMemoryBlock> Node = FindBlockNode(Address);

            LinkedListNode<KPageNode> PageListNode = PageList.Nodes.First;

            KPageNode PageNode = PageListNode.Value;

            ulong SrcPa      = PageNode.Address;
            ulong SrcPaPages = PageNode.PagesCount;

            do
            {
                Info = Node.Value.GetInfo();

                if (Info.State == MemoryState.Unmapped)
                {
                    ulong BlockSize = GetSizeInRange(Info, Address, EndAddr);

                    ulong DstVaPages = BlockSize / PageSize;

                    ulong DstVa = GetAddrInRange(Info, Address);

                    while (DstVaPages > 0)
                    {
                        if (SrcPaPages == 0)
                        {
                            PageListNode = PageListNode.Next;

                            PageNode = PageListNode.Value;

                            SrcPa      = PageNode.Address;
                            SrcPaPages = PageNode.PagesCount;
                        }

                        ulong PagesCount = SrcPaPages;

                        if (PagesCount > DstVaPages)
                        {
                            PagesCount = DstVaPages;
                        }

                        DoMmuOperation(
                            DstVa,
                            PagesCount,
                            SrcPa,
                            true,
                            MemoryPermission.ReadAndWrite,
                            MemoryOperation.MapPa);

                        DstVa      += PagesCount * PageSize;
                        SrcPa      += PagesCount * PageSize;
                        SrcPaPages -= PagesCount;
                        DstVaPages -= PagesCount;
                    }
                }

                Node = Node.Next;
            }
            while (Info.Address + Info.Size < EndAddr && Node != null);
        }

        private static ulong GetSizeInRange(KMemoryInfo Info, ulong Start, ulong End)
        {
            ulong EndAddr = Info.Size + Info.Address;
            ulong Size    = Info.Size;

            if (Info.Address < Start)
            {
                Size -= Start - Info.Address;
            }

            if (EndAddr > End)
            {
                Size -= EndAddr - End;
            }

            return Size;
        }

        private static ulong GetAddrInRange(KMemoryInfo Info, ulong Start)
        {
            if (Info.Address < Start)
            {
                return Start;
            }

            return Info.Address;
        }

        private void AddVaRangeToPageList(KPageList PageList, ulong Start, ulong PagesCount)
        {
            ulong Address = Start;

            while (Address < Start + PagesCount * PageSize)
            {
                KernelResult Result = ConvertVaToPa(Address, out ulong Pa);

                if (Result != KernelResult.Success)
                {
                    throw new InvalidOperationException("Unexpected failure translating virtual address.");
                }

                PageList.AddRange(Pa, 1);

                Address += PageSize;
            }
        }

        private bool IsUnmapped(ulong Address, ulong Size)
        {
            return CheckRange(
                Address,
                Size,
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
            ulong                Address,
            ulong                Size,
            MemoryState          StateMask,
            MemoryState          StateExpected,
            MemoryPermission     PermissionMask,
            MemoryPermission     PermissionExpected,
            MemoryAttribute      AttributeMask,
            MemoryAttribute      AttributeExpected,
            MemoryAttribute      AttributeIgnoreMask,
            out MemoryState      OutState,
            out MemoryPermission OutPermission,
            out MemoryAttribute  OutAttribute)
        {
            ulong EndAddr = Address + Size - 1;

            LinkedListNode<KMemoryBlock> Node = FindBlockNode(Address);

            KMemoryInfo Info = Node.Value.GetInfo();

            MemoryState      FirstState      = Info.State;
            MemoryPermission FirstPermission = Info.Permission;
            MemoryAttribute  FirstAttribute  = Info.Attribute;

            do
            {
                Info = Node.Value.GetInfo();

                //Check if the block state matches what we expect.
                if ( FirstState                             != Info.State                             ||
                     FirstPermission                        != Info.Permission                        ||
                    (Info.Attribute  & AttributeMask)       != AttributeExpected                      ||
                    (FirstAttribute  | AttributeIgnoreMask) != (Info.Attribute | AttributeIgnoreMask) ||
                    (FirstState      & StateMask)           != StateExpected                          ||
                    (FirstPermission & PermissionMask)      != PermissionExpected)
                {
                    break;
                }

                //Check if this is the last block on the range, if so return success.
                if (EndAddr <= Info.Address + Info.Size - 1)
                {
                    OutState      = FirstState;
                    OutPermission = FirstPermission;
                    OutAttribute  = FirstAttribute & ~AttributeIgnoreMask;

                    return true;
                }

                Node = Node.Next;
            }
            while (Node != null);

            OutState      = MemoryState.Unmapped;
            OutPermission = MemoryPermission.None;
            OutAttribute  = MemoryAttribute.None;

            return false;
        }

        private bool CheckRange(
            ulong            Address,
            ulong            Size,
            MemoryState      StateMask,
            MemoryState      StateExpected,
            MemoryPermission PermissionMask,
            MemoryPermission PermissionExpected,
            MemoryAttribute  AttributeMask,
            MemoryAttribute  AttributeExpected)
        {
            ulong EndAddr = Address + Size - 1;

            LinkedListNode<KMemoryBlock> Node = FindBlockNode(Address);

            do
            {
                KMemoryInfo Info = Node.Value.GetInfo();

                //Check if the block state matches what we expect.
                if ((Info.State      & StateMask)      != StateExpected      ||
                    (Info.Permission & PermissionMask) != PermissionExpected ||
                    (Info.Attribute  & AttributeMask)  != AttributeExpected)
                {
                    break;
                }

                //Check if this is the last block on the range, if so return success.
                if (EndAddr <= Info.Address + Info.Size - 1)
                {
                    return true;
                }

                Node = Node.Next;
            }
            while (Node != null);

            return false;
        }

        private void InsertBlock(
            ulong            BaseAddress,
            ulong            PagesCount,
            MemoryState      OldState,
            MemoryPermission OldPermission,
            MemoryAttribute  OldAttribute,
            MemoryState      NewState,
            MemoryPermission NewPermission,
            MemoryAttribute  NewAttribute)
        {
            //Insert new block on the list only on areas where the state
            //of the block matches the state specified on the Old* state
            //arguments, otherwise leave it as is.
            int OldCount = Blocks.Count;

            OldAttribute |= MemoryAttribute.IpcAndDeviceMapped;

            ulong EndAddr = PagesCount * PageSize + BaseAddress;

            LinkedListNode<KMemoryBlock> Node = Blocks.First;

            while (Node != null)
            {
                LinkedListNode<KMemoryBlock> NewNode  = Node;
                LinkedListNode<KMemoryBlock> NextNode = Node.Next;

                KMemoryBlock CurrBlock = Node.Value;

                ulong CurrBaseAddr = CurrBlock.BaseAddress;
                ulong CurrEndAddr  = CurrBlock.PagesCount * PageSize + CurrBaseAddr;

                if (BaseAddress < CurrEndAddr && CurrBaseAddr < EndAddr)
                {
                    MemoryAttribute CurrBlockAttr = CurrBlock.Attribute | MemoryAttribute.IpcAndDeviceMapped;

                    if (CurrBlock.State      != OldState      ||
                        CurrBlock.Permission != OldPermission ||
                        CurrBlockAttr        != OldAttribute)
                    {
                        Node = NextNode;

                        continue;
                    }

                    if (CurrBaseAddr >= BaseAddress && CurrEndAddr <= EndAddr)
                    {
                        CurrBlock.State      = NewState;
                        CurrBlock.Permission = NewPermission;
                        CurrBlock.Attribute &= ~MemoryAttribute.IpcAndDeviceMapped;
                        CurrBlock.Attribute |= NewAttribute;
                    }
                    else if (CurrBaseAddr >= BaseAddress)
                    {
                        CurrBlock.BaseAddress = EndAddr;

                        CurrBlock.PagesCount = (CurrEndAddr - EndAddr) / PageSize;

                        ulong NewPagesCount = (EndAddr - CurrBaseAddr) / PageSize;

                        NewNode = Blocks.AddBefore(Node, new KMemoryBlock(
                            CurrBaseAddr,
                            NewPagesCount,
                            NewState,
                            NewPermission,
                            NewAttribute));
                    }
                    else if (CurrEndAddr <= EndAddr)
                    {
                        CurrBlock.PagesCount = (BaseAddress - CurrBaseAddr) / PageSize;

                        ulong NewPagesCount = (CurrEndAddr - BaseAddress) / PageSize;

                        NewNode = Blocks.AddAfter(Node, new KMemoryBlock(
                            BaseAddress,
                            NewPagesCount,
                            NewState,
                            NewPermission,
                            NewAttribute));
                    }
                    else
                    {
                        CurrBlock.PagesCount = (BaseAddress - CurrBaseAddr) / PageSize;

                        ulong NextPagesCount = (CurrEndAddr - EndAddr) / PageSize;

                        NewNode = Blocks.AddAfter(Node, new KMemoryBlock(
                            BaseAddress,
                            PagesCount,
                            NewState,
                            NewPermission,
                            NewAttribute));

                        Blocks.AddAfter(NewNode, new KMemoryBlock(
                            EndAddr,
                            NextPagesCount,
                            CurrBlock.State,
                            CurrBlock.Permission,
                            CurrBlock.Attribute));

                        NextNode = null;
                    }

                    MergeEqualStateNeighbours(NewNode);
                }

                Node = NextNode;
            }

            BlockAllocator.Count += Blocks.Count - OldCount;
        }

        private void InsertBlock(
            ulong            BaseAddress,
            ulong            PagesCount,
            MemoryState      State,
            MemoryPermission Permission = MemoryPermission.None,
            MemoryAttribute  Attribute  = MemoryAttribute.None)
        {
            //Inserts new block at the list, replacing and spliting
            //existing blocks as needed.
            KMemoryBlock Block = new KMemoryBlock(BaseAddress, PagesCount, State, Permission, Attribute);

            int OldCount = Blocks.Count;

            ulong EndAddr = PagesCount * PageSize + BaseAddress;

            LinkedListNode<KMemoryBlock> NewNode = null;

            LinkedListNode<KMemoryBlock> Node = Blocks.First;

            while (Node != null)
            {
                KMemoryBlock CurrBlock = Node.Value;

                LinkedListNode<KMemoryBlock> NextNode = Node.Next;

                ulong CurrBaseAddr = CurrBlock.BaseAddress;
                ulong CurrEndAddr  = CurrBlock.PagesCount * PageSize + CurrBaseAddr;

                if (BaseAddress < CurrEndAddr && CurrBaseAddr < EndAddr)
                {
                    if (BaseAddress >= CurrBaseAddr && EndAddr <= CurrEndAddr)
                    {
                        Block.Attribute |= CurrBlock.Attribute & MemoryAttribute.IpcAndDeviceMapped;
                    }

                    if (BaseAddress > CurrBaseAddr && EndAddr < CurrEndAddr)
                    {
                        CurrBlock.PagesCount = (BaseAddress - CurrBaseAddr) / PageSize;

                        ulong NextPagesCount = (CurrEndAddr - EndAddr) / PageSize;

                        NewNode = Blocks.AddAfter(Node, Block);

                        Blocks.AddAfter(NewNode, new KMemoryBlock(
                            EndAddr,
                            NextPagesCount,
                            CurrBlock.State,
                            CurrBlock.Permission,
                            CurrBlock.Attribute));

                        break;
                    }
                    else if (BaseAddress <= CurrBaseAddr && EndAddr < CurrEndAddr)
                    {
                        CurrBlock.BaseAddress = EndAddr;

                        CurrBlock.PagesCount = (CurrEndAddr - EndAddr) / PageSize;

                        if (NewNode == null)
                        {
                            NewNode = Blocks.AddBefore(Node, Block);
                        }
                    }
                    else if (BaseAddress > CurrBaseAddr && EndAddr >= CurrEndAddr)
                    {
                        CurrBlock.PagesCount = (BaseAddress - CurrBaseAddr) / PageSize;

                        if (NewNode == null)
                        {
                            NewNode = Blocks.AddAfter(Node, Block);
                        }
                    }
                    else
                    {
                        if (NewNode == null)
                        {
                            NewNode = Blocks.AddBefore(Node, Block);
                        }

                        Blocks.Remove(Node);
                    }
                }

                Node = NextNode;
            }

            if (NewNode == null)
            {
                NewNode = Blocks.AddFirst(Block);
            }

            MergeEqualStateNeighbours(NewNode);

            BlockAllocator.Count += Blocks.Count - OldCount;
        }

        private void MergeEqualStateNeighbours(LinkedListNode<KMemoryBlock> Node)
        {
            KMemoryBlock Block = Node.Value;

            ulong EndAddr = Block.PagesCount * PageSize + Block.BaseAddress;

            if (Node.Previous != null)
            {
                KMemoryBlock Previous = Node.Previous.Value;

                if (BlockStateEquals(Block, Previous))
                {
                    Blocks.Remove(Node.Previous);

                    Block.BaseAddress = Previous.BaseAddress;
                }
            }

            if (Node.Next != null)
            {
                KMemoryBlock Next = Node.Next.Value;

                if (BlockStateEquals(Block, Next))
                {
                    Blocks.Remove(Node.Next);

                    EndAddr = Next.BaseAddress + Next.PagesCount * PageSize;
                }
            }

            Block.PagesCount = (EndAddr - Block.BaseAddress) / PageSize;
        }

        private static bool BlockStateEquals(KMemoryBlock Lhs, KMemoryBlock Rhs)
        {
            return Lhs.State          == Rhs.State          &&
                   Lhs.Permission     == Rhs.Permission     &&
                   Lhs.Attribute      == Rhs.Attribute      &&
                   Lhs.DeviceRefCount == Rhs.DeviceRefCount &&
                   Lhs.IpcRefCount    == Rhs.IpcRefCount;
        }

        private ulong FindFirstFit(
            ulong RegionStart,
            ulong RegionPagesCount,
            ulong NeededPagesCount,
            int   Alignment,
            ulong ReservedStart,
            ulong ReservedPagesCount)
        {
            ulong ReservedSize = ReservedPagesCount * PageSize;

            ulong TotalNeededSize = ReservedSize + NeededPagesCount * PageSize;

            ulong RegionEndAddr = RegionStart + RegionPagesCount * PageSize;

            LinkedListNode<KMemoryBlock> Node = FindBlockNode(RegionStart);

            KMemoryInfo Info = Node.Value.GetInfo();

            while (RegionEndAddr >= Info.Address)
            {
                if (Info.State == MemoryState.Unmapped)
                {
                    ulong CurrBaseAddr = Info.Address + ReservedSize;
                    ulong CurrEndAddr  = Info.Address + Info.Size - 1;

                    ulong Address = BitUtils.AlignDown(CurrBaseAddr, Alignment) + ReservedStart;

                    if (CurrBaseAddr > Address)
                    {
                        Address += (ulong)Alignment;
                    }

                    ulong AllocationEndAddr = Address + TotalNeededSize - 1;

                    if (AllocationEndAddr <= RegionEndAddr &&
                        AllocationEndAddr <= CurrEndAddr   &&
                        Address           <  AllocationEndAddr)
                    {
                        return Address;
                    }
                }

                Node = Node.Next;

                if (Node == null)
                {
                    break;
                }

                Info = Node.Value.GetInfo();
            }

            return 0;
        }

        private KMemoryBlock FindBlock(ulong Address)
        {
            return FindBlockNode(Address)?.Value;
        }

        private LinkedListNode<KMemoryBlock> FindBlockNode(ulong Address)
        {
            lock (Blocks)
            {
                LinkedListNode<KMemoryBlock> Node = Blocks.First;

                while (Node != null)
                {
                    KMemoryBlock Block = Node.Value;

                    ulong CurrEndAddr = Block.PagesCount * PageSize + Block.BaseAddress;

                    if (Block.BaseAddress <= Address && CurrEndAddr - 1 >= Address)
                    {
                        return Node;
                    }

                    Node = Node.Next;
                }
            }

            return null;
        }

        private bool ValidateRegionForState(ulong Address, ulong Size, MemoryState State)
        {
            ulong EndAddr = Address + Size;

            ulong RegionBaseAddr = GetBaseAddrForState(State);

            ulong RegionEndAddr = RegionBaseAddr + GetSizeForState(State);

            bool InsideRegion()
            {
                return RegionBaseAddr <= Address &&
                       EndAddr        >  Address &&
                       EndAddr - 1    <= RegionEndAddr - 1;
            }

            bool OutsideHeapRegion()
            {
                return EndAddr <= HeapRegionStart ||
                       Address >= HeapRegionEnd;
            }

            bool OutsideMapRegion()
            {
                return EndAddr <= AliasRegionStart ||
                       Address >= AliasRegionEnd;
            }

            switch (State)
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

            throw new ArgumentException($"Invalid state value \"{State}\".");
        }

        private ulong GetBaseAddrForState(MemoryState State)
        {
            switch (State)
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

            throw new ArgumentException($"Invalid state value \"{State}\".");
        }

        private ulong GetSizeForState(MemoryState State)
        {
            switch (State)
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

            throw new ArgumentException($"Invalid state value \"{State}\".");
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

        private KernelResult MapPages(ulong Address, KPageList PageList, MemoryPermission Permission)
        {
            ulong CurrAddr = Address;

            KernelResult Result = KernelResult.Success;

            foreach (KPageNode PageNode in PageList)
            {
                Result = DoMmuOperation(
                    CurrAddr,
                    PageNode.PagesCount,
                    PageNode.Address,
                    true,
                    Permission,
                    MemoryOperation.MapPa);

                if (Result != KernelResult.Success)
                {
                    KMemoryInfo Info = FindBlock(CurrAddr).GetInfo();

                    ulong PagesCount = (Address - CurrAddr) / PageSize;

                    Result = MmuUnmap(Address, PagesCount);

                    break;
                }

                CurrAddr += PageNode.PagesCount * PageSize;
            }

            return Result;
        }

        private KernelResult MmuUnmap(ulong Address, ulong PagesCount)
        {
            return DoMmuOperation(
                Address,
                PagesCount,
                0,
                false,
                MemoryPermission.None,
                MemoryOperation.Unmap);
        }

        private KernelResult MmuChangePermission(ulong Address, ulong PagesCount, MemoryPermission Permission)
        {
            return DoMmuOperation(
                Address,
                PagesCount,
                0,
                false,
                Permission,
                MemoryOperation.ChangePermRw);
        }

        private KernelResult DoMmuOperation(
            ulong            DstVa,
            ulong            PagesCount,
            ulong            SrcPa,
            bool             Map,
            MemoryPermission Permission,
            MemoryOperation  Operation)
        {
            if (Map != (Operation == MemoryOperation.MapPa))
            {
                throw new ArgumentException(nameof(Map) + " value is invalid for this operation.");
            }

            KernelResult Result;

            switch (Operation)
            {
                case MemoryOperation.MapPa:
                {
                    ulong Size = PagesCount * PageSize;

                    CpuMemory.Map((long)DstVa, (long)(SrcPa - DramMemoryMap.DramBase), (long)Size);

                    Result = KernelResult.Success;

                    break;
                }

                case MemoryOperation.Allocate:
                {
                    KMemoryRegionManager Region = GetMemoryRegionManager();

                    Result = Region.AllocatePages(PagesCount, AslrDisabled, out KPageList PageList);

                    if (Result == KernelResult.Success)
                    {
                        Result = MmuMapPages(DstVa, PageList);
                    }

                    break;
                }

                case MemoryOperation.Unmap:
                {
                    ulong Size = PagesCount * PageSize;

                    CpuMemory.Unmap((long)DstVa, (long)Size);

                    Result = KernelResult.Success;

                    break;
                }

                case MemoryOperation.ChangePermRw:             Result = KernelResult.Success; break;
                case MemoryOperation.ChangePermsAndAttributes: Result = KernelResult.Success; break;

                default: throw new ArgumentException($"Invalid operation \"{Operation}\".");
            }

            return Result;
        }

        private KernelResult DoMmuOperation(
            ulong            Address,
            ulong            PagesCount,
            KPageList        PageList,
            MemoryPermission Permission,
            MemoryOperation  Operation)
        {
            if (Operation != MemoryOperation.MapVa)
            {
                throw new ArgumentException($"Invalid memory operation \"{Operation}\" specified.");
            }

            return MmuMapPages(Address, PageList);
        }

        private KMemoryRegionManager GetMemoryRegionManager()
        {
            return System.MemoryRegions[(int)MemRegion];
        }

        private KernelResult MmuMapPages(ulong Address, KPageList PageList)
        {
            foreach (KPageNode PageNode in PageList)
            {
                ulong Size = PageNode.PagesCount * PageSize;

                CpuMemory.Map((long)Address, (long)(PageNode.Address - DramMemoryMap.DramBase), (long)Size);

                Address += Size;
            }

            return KernelResult.Success;
        }

        public KernelResult ConvertVaToPa(ulong Va, out ulong Pa)
        {
            Pa = DramMemoryMap.DramBase + (ulong)CpuMemory.GetPhysicalAddress((long)Va);

            return KernelResult.Success;
        }

        public long GetMmUsedPages()
        {
            lock (Blocks)
            {
                return BitUtils.DivRoundUp(GetMmUsedSize(), PageSize);
            }
        }

        private long GetMmUsedSize()
        {
            return Blocks.Count * KMemoryBlockSize;
        }

        public bool IsInvalidRegion(ulong Address, ulong Size)
        {
            return Address + Size - 1 > GetAddrSpaceBaseAddr() + GetAddrSpaceSize() - 1;
        }

        public bool InsideAddrSpace(ulong Address, ulong Size)
        {
            return AddrSpaceStart <= Address && Address + Size - 1 <= AddrSpaceEnd - 1;
        }

        public bool InsideAliasRegion(ulong Address, ulong Size)
        {
            return Address + Size > AliasRegionStart && AliasRegionEnd > Address;
        }

        public bool InsideHeapRegion(ulong Address, ulong Size)
        {
            return Address + Size > HeapRegionStart && HeapRegionEnd > Address;
        }

        public bool InsideStackRegion(ulong Address, ulong Size)
        {
            return Address + Size > StackRegionStart && StackRegionEnd > Address;
        }

        public bool OutsideAliasRegion(ulong Address, ulong Size)
        {
            return AliasRegionStart > Address || Address + Size - 1 > AliasRegionEnd - 1;
        }

        public bool OutsideAddrSpace(ulong Address, ulong Size)
        {
            return AddrSpaceStart > Address || Address + Size - 1 > AddrSpaceEnd - 1;
        }

        public bool OutsideStackRegion(ulong Address, ulong Size)
        {
            return StackRegionStart > Address || Address + Size - 1 > StackRegionEnd - 1;
        }
    }
}