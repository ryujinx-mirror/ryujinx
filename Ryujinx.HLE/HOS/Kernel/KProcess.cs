using ChocolArm64;
using ChocolArm64.Events;
using ChocolArm64.Memory;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KProcess : KSynchronizationObject
    {
        public const int KernelVersionMajor    = 10;
        public const int KernelVersionMinor    = 4;
        public const int KernelVersionRevision = 0;

        public const int KernelVersionPacked =
            (KernelVersionMajor    << 19) |
            (KernelVersionMinor    << 15) |
            (KernelVersionRevision << 0);

        public KMemoryManager MemoryManager { get; private set; }

        private SortedDictionary<ulong, KTlsPageInfo> FullTlsPages;
        private SortedDictionary<ulong, KTlsPageInfo> FreeTlsPages;

        public int DefaultCpuCore { get; private set; }

        public bool Debug { get; private set; }

        public KResourceLimit ResourceLimit { get; private set; }

        public ulong PersonalMmHeapPagesCount { get; private set; }

        private ProcessState State;

        private object ProcessLock;
        private object ThreadingLock;

        public KAddressArbiter AddressArbiter { get; private set; }

        public long[] RandomEntropy { get; private set; }

        private bool Signaled;
        private bool UseSystemMemBlocks;

        public string Name { get; private set; }

        private int ThreadCount;

        public int MmuFlags { get; private set; }

        private MemoryRegion MemRegion;

        public KProcessCapabilities Capabilities { get; private set; }

        public long TitleId { get; private set; }
        public long Pid     { get; private set; }

        private long  CreationTimestamp;
        private ulong Entrypoint;
        private ulong ImageSize;
        private ulong MainThreadStackSize;
        private ulong MemoryUsageCapacity;
        private int   Category;

        public KHandleTable HandleTable { get; private set; }

        public ulong UserExceptionContextAddress { get; private set; }

        private LinkedList<KThread> Threads;

        public bool IsPaused { get; private set; }

        public Translator Translator { get; private set; }

        public MemoryManager CpuMemory { get; private set; }

        private SvcHandler SvcHandler;

        public HleProcessDebugger Debugger { get; private set; }

        public KProcess(Horizon System) : base(System)
        {
            ProcessLock   = new object();
            ThreadingLock = new object();

            CpuMemory = new MemoryManager(System.Device.Memory.RamPointer);

            CpuMemory.InvalidAccess += InvalidAccessHandler;

            AddressArbiter = new KAddressArbiter(System);

            MemoryManager = new KMemoryManager(System, CpuMemory);

            FullTlsPages = new SortedDictionary<ulong, KTlsPageInfo>();
            FreeTlsPages = new SortedDictionary<ulong, KTlsPageInfo>();

            Capabilities = new KProcessCapabilities();

            RandomEntropy = new long[KScheduler.CpuCoresCount];

            Threads = new LinkedList<KThread>();

            Translator = new Translator();

            Translator.CpuTrace += CpuTraceHandler;

            SvcHandler = new SvcHandler(System.Device, this);

            Debugger = new HleProcessDebugger(this);
        }

        public KernelResult InitializeKip(
            ProcessCreationInfo CreationInfo,
            int[]               Caps,
            KPageList           PageList,
            KResourceLimit      ResourceLimit,
            MemoryRegion        MemRegion)
        {
            this.ResourceLimit = ResourceLimit;
            this.MemRegion     = MemRegion;

            AddressSpaceType AddrSpaceType = (AddressSpaceType)((CreationInfo.MmuFlags >> 1) & 7);

            bool AslrEnabled = ((CreationInfo.MmuFlags >> 5) & 1) != 0;

            ulong CodeAddress = CreationInfo.CodeAddress;

            ulong CodeSize = (ulong)CreationInfo.CodePagesCount * KMemoryManager.PageSize;

            KMemoryBlockAllocator MemoryBlockAllocator = (MmuFlags & 0x40) != 0
                ? System.LargeMemoryBlockAllocator
                : System.SmallMemoryBlockAllocator;

            KernelResult Result = MemoryManager.InitializeForProcess(
                AddrSpaceType,
                AslrEnabled,
                !AslrEnabled,
                MemRegion,
                CodeAddress,
                CodeSize,
                MemoryBlockAllocator);

            if (Result != KernelResult.Success)
            {
                return Result;
            }

            if (!ValidateCodeAddressAndSize(CodeAddress, CodeSize))
            {
                return KernelResult.InvalidMemRange;
            }

            Result = MemoryManager.MapPages(
                CodeAddress,
                PageList,
                MemoryState.CodeStatic,
                MemoryPermission.None);

            if (Result != KernelResult.Success)
            {
                return Result;
            }

            Result = Capabilities.InitializeForKernel(Caps, MemoryManager);

            if (Result != KernelResult.Success)
            {
                return Result;
            }

            Pid = System.GetKipId();

            if (Pid == 0 || (ulong)Pid >= Horizon.InitialProcessId)
            {
                throw new InvalidOperationException($"Invalid KIP Id {Pid}.");
            }

            Result = ParseProcessInfo(CreationInfo);

            return Result;
        }

        public KernelResult Initialize(
            ProcessCreationInfo CreationInfo,
            int[]               Caps,
            KResourceLimit      ResourceLimit,
            MemoryRegion        MemRegion)
        {
            this.ResourceLimit = ResourceLimit;
            this.MemRegion     = MemRegion;

            ulong PersonalMmHeapSize = GetPersonalMmHeapSize((ulong)CreationInfo.PersonalMmHeapPagesCount, MemRegion);

            ulong CodePagesCount = (ulong)CreationInfo.CodePagesCount;

            ulong NeededSizeForProcess = PersonalMmHeapSize + CodePagesCount * KMemoryManager.PageSize;

            if (NeededSizeForProcess != 0 && ResourceLimit != null)
            {
                if (!ResourceLimit.Reserve(LimitableResource.Memory, NeededSizeForProcess))
                {
                    return KernelResult.ResLimitExceeded;
                }
            }

            void CleanUpForError()
            {
                if (NeededSizeForProcess != 0 && ResourceLimit != null)
                {
                    ResourceLimit.Release(LimitableResource.Memory, NeededSizeForProcess);
                }
            }

            PersonalMmHeapPagesCount = (ulong)CreationInfo.PersonalMmHeapPagesCount;

            KMemoryBlockAllocator MemoryBlockAllocator;

            if (PersonalMmHeapPagesCount != 0)
            {
                MemoryBlockAllocator = new KMemoryBlockAllocator(PersonalMmHeapPagesCount * KMemoryManager.PageSize);
            }
            else
            {
                MemoryBlockAllocator = (MmuFlags & 0x40) != 0
                    ? System.LargeMemoryBlockAllocator
                    : System.SmallMemoryBlockAllocator;
            }

            AddressSpaceType AddrSpaceType = (AddressSpaceType)((CreationInfo.MmuFlags >> 1) & 7);

            bool AslrEnabled = ((CreationInfo.MmuFlags >> 5) & 1) != 0;

            ulong CodeAddress = CreationInfo.CodeAddress;

            ulong CodeSize = CodePagesCount * KMemoryManager.PageSize;

            KernelResult Result = MemoryManager.InitializeForProcess(
                AddrSpaceType,
                AslrEnabled,
                !AslrEnabled,
                MemRegion,
                CodeAddress,
                CodeSize,
                MemoryBlockAllocator);

            if (Result != KernelResult.Success)
            {
                CleanUpForError();

                return Result;
            }

            if (!ValidateCodeAddressAndSize(CodeAddress, CodeSize))
            {
                CleanUpForError();

                return KernelResult.InvalidMemRange;
            }

            Result = MemoryManager.MapNewProcessCode(
                CodeAddress,
                CodePagesCount,
                MemoryState.CodeStatic,
                MemoryPermission.None);

            if (Result != KernelResult.Success)
            {
                CleanUpForError();

                return Result;
            }

            Result = Capabilities.InitializeForUser(Caps, MemoryManager);

            if (Result != KernelResult.Success)
            {
                CleanUpForError();

                return Result;
            }

            Pid = System.GetProcessId();

            if (Pid == -1 || (ulong)Pid < Horizon.InitialProcessId)
            {
                throw new InvalidOperationException($"Invalid Process Id {Pid}.");
            }

            Result = ParseProcessInfo(CreationInfo);

            if (Result != KernelResult.Success)
            {
                CleanUpForError();
            }

            return Result;
        }

        private bool ValidateCodeAddressAndSize(ulong Address, ulong Size)
        {
            ulong CodeRegionStart;
            ulong CodeRegionSize;

            switch (MemoryManager.AddrSpaceWidth)
            {
                case 32:
                    CodeRegionStart = 0x200000;
                    CodeRegionSize  = 0x3fe00000;
                    break;

                case 36:
                    CodeRegionStart = 0x8000000;
                    CodeRegionSize  = 0x78000000;
                    break;

                case 39:
                    CodeRegionStart = 0x8000000;
                    CodeRegionSize  = 0x7ff8000000;
                    break;

                default: throw new InvalidOperationException("Invalid address space width on memory manager.");
            }

            ulong EndAddr = Address + Size;

            ulong CodeRegionEnd = CodeRegionStart + CodeRegionSize;

            if (EndAddr     <= Address ||
                EndAddr - 1 >  CodeRegionEnd - 1)
            {
                return false;
            }

            if (MemoryManager.InsideHeapRegion (Address, Size) ||
                MemoryManager.InsideAliasRegion(Address, Size))
            {
                return false;
            }

            return true;
        }

        private KernelResult ParseProcessInfo(ProcessCreationInfo CreationInfo)
        {
            //Ensure that the current kernel version is equal or above to the minimum required.
            uint RequiredKernelVersionMajor =  (uint)Capabilities.KernelReleaseVersion >> 19;
            uint RequiredKernelVersionMinor = ((uint)Capabilities.KernelReleaseVersion >> 15) & 0xf;

            if (System.EnableVersionChecks)
            {
                if (RequiredKernelVersionMajor > KernelVersionMajor)
                {
                    return KernelResult.InvalidCombination;
                }

                if (RequiredKernelVersionMajor != KernelVersionMajor && RequiredKernelVersionMajor < 3)
                {
                    return KernelResult.InvalidCombination;
                }

                if (RequiredKernelVersionMinor > KernelVersionMinor)
                {
                    return KernelResult.InvalidCombination;
                }
            }

            KernelResult Result = AllocateThreadLocalStorage(out ulong UserExceptionContextAddress);

            if (Result != KernelResult.Success)
            {
                return Result;
            }

            this.UserExceptionContextAddress = UserExceptionContextAddress;

            MemoryHelper.FillWithZeros(CpuMemory, (long)UserExceptionContextAddress, KTlsPageInfo.TlsEntrySize);

            Name = CreationInfo.Name;

            State = ProcessState.Created;

            CreationTimestamp = PerformanceCounter.ElapsedMilliseconds;

            MmuFlags   = CreationInfo.MmuFlags;
            Category   = CreationInfo.Category;
            TitleId    = CreationInfo.TitleId;
            Entrypoint = CreationInfo.CodeAddress;
            ImageSize  = (ulong)CreationInfo.CodePagesCount * KMemoryManager.PageSize;

            UseSystemMemBlocks = ((MmuFlags >> 6) & 1) != 0;

            switch ((AddressSpaceType)((MmuFlags >> 1) & 7))
            {
                case AddressSpaceType.Addr32Bits:
                case AddressSpaceType.Addr36Bits:
                case AddressSpaceType.Addr39Bits:
                    MemoryUsageCapacity = MemoryManager.HeapRegionEnd -
                                          MemoryManager.HeapRegionStart;
                    break;

                case AddressSpaceType.Addr32BitsNoMap:
                    MemoryUsageCapacity = MemoryManager.HeapRegionEnd -
                                          MemoryManager.HeapRegionStart +
                                          MemoryManager.AliasRegionEnd -
                                          MemoryManager.AliasRegionStart;
                    break;

                default: throw new InvalidOperationException($"Invalid MMU flags value 0x{MmuFlags:x2}.");
            }

            GenerateRandomEntropy();

            return KernelResult.Success;
        }

        public KernelResult AllocateThreadLocalStorage(out ulong Address)
        {
            System.CriticalSection.Enter();

            KernelResult Result;

            if (FreeTlsPages.Count > 0)
            {
                //If we have free TLS pages available, just use the first one.
                KTlsPageInfo PageInfo = FreeTlsPages.Values.First();

                if (!PageInfo.TryGetFreePage(out Address))
                {
                    throw new InvalidOperationException("Unexpected failure getting free TLS page!");
                }

                if (PageInfo.IsFull())
                {
                    FreeTlsPages.Remove(PageInfo.PageAddr);

                    FullTlsPages.Add(PageInfo.PageAddr, PageInfo);
                }

                Result = KernelResult.Success;
            }
            else
            {
                //Otherwise, we need to create a new one.
                Result = AllocateTlsPage(out KTlsPageInfo PageInfo);

                if (Result == KernelResult.Success)
                {
                    if (!PageInfo.TryGetFreePage(out Address))
                    {
                        throw new InvalidOperationException("Unexpected failure getting free TLS page!");
                    }

                    FreeTlsPages.Add(PageInfo.PageAddr, PageInfo);
                }
                else
                {
                    Address = 0;
                }
            }

            System.CriticalSection.Leave();

            return Result;
        }

        private KernelResult AllocateTlsPage(out KTlsPageInfo PageInfo)
        {
            PageInfo = default(KTlsPageInfo);

            if (!System.UserSlabHeapPages.TryGetItem(out ulong TlsPagePa))
            {
                return KernelResult.OutOfMemory;
            }

            ulong RegionStart = MemoryManager.TlsIoRegionStart;
            ulong RegionSize  = MemoryManager.TlsIoRegionEnd - RegionStart;

            ulong RegionPagesCount = RegionSize / KMemoryManager.PageSize;

            KernelResult Result = MemoryManager.AllocateOrMapPa(
                1,
                KMemoryManager.PageSize,
                TlsPagePa,
                true,
                RegionStart,
                RegionPagesCount,
                MemoryState.ThreadLocal,
                MemoryPermission.ReadAndWrite,
                out ulong TlsPageVa);

            if (Result != KernelResult.Success)
            {
                System.UserSlabHeapPages.Free(TlsPagePa);
            }
            else
            {
                PageInfo = new KTlsPageInfo(TlsPageVa);

                MemoryHelper.FillWithZeros(CpuMemory, (long)TlsPageVa, KMemoryManager.PageSize);
            }

            return Result;
        }

        public KernelResult FreeThreadLocalStorage(ulong TlsSlotAddr)
        {
            ulong TlsPageAddr = BitUtils.AlignDown(TlsSlotAddr, KMemoryManager.PageSize);

            System.CriticalSection.Enter();

            KernelResult Result = KernelResult.Success;

            KTlsPageInfo PageInfo = null;

            if (FullTlsPages.TryGetValue(TlsPageAddr, out PageInfo))
            {
                //TLS page was full, free slot and move to free pages tree.
                FullTlsPages.Remove(TlsPageAddr);

                FreeTlsPages.Add(TlsPageAddr, PageInfo);
            }
            else if (!FreeTlsPages.TryGetValue(TlsPageAddr, out PageInfo))
            {
                Result = KernelResult.InvalidAddress;
            }

            if (PageInfo != null)
            {
                PageInfo.FreeTlsSlot(TlsSlotAddr);

                if (PageInfo.IsEmpty())
                {
                    //TLS page is now empty, we should ensure it is removed
                    //from all trees, and free the memory it was using.
                    FreeTlsPages.Remove(TlsPageAddr);

                    System.CriticalSection.Leave();

                    FreeTlsPage(PageInfo);

                    return KernelResult.Success;
                }
            }

            System.CriticalSection.Leave();

            return Result;
        }

        private KernelResult FreeTlsPage(KTlsPageInfo PageInfo)
        {
            KernelResult Result = MemoryManager.ConvertVaToPa(PageInfo.PageAddr, out ulong TlsPagePa);

            if (Result != KernelResult.Success)
            {
                throw new InvalidOperationException("Unexpected failure translating virtual address to physical.");
            }

            Result = MemoryManager.UnmapForKernel(PageInfo.PageAddr, 1, MemoryState.ThreadLocal);

            if (Result == KernelResult.Success)
            {
                System.UserSlabHeapPages.Free(TlsPagePa);
            }

            return Result;
        }

        private void GenerateRandomEntropy()
        {
            //TODO.
        }

        public KernelResult Start(int MainThreadPriority, ulong StackSize)
        {
            lock (ProcessLock)
            {
                if (State > ProcessState.CreatedAttached)
                {
                    return KernelResult.InvalidState;
                }

                if (ResourceLimit != null && !ResourceLimit.Reserve(LimitableResource.Thread, 1))
                {
                    return KernelResult.ResLimitExceeded;
                }

                KResourceLimit ThreadResourceLimit = ResourceLimit;
                KResourceLimit MemoryResourceLimit = null;

                if (MainThreadStackSize != 0)
                {
                    throw new InvalidOperationException("Trying to start a process with a invalid state!");
                }

                ulong StackSizeRounded = BitUtils.AlignUp(StackSize, KMemoryManager.PageSize);

                ulong NeededSize = StackSizeRounded + ImageSize;

                //Check if the needed size for the code and the stack will fit on the
                //memory usage capacity of this Process. Also check for possible overflow
                //on the above addition.
                if (NeededSize > MemoryUsageCapacity ||
                    NeededSize < StackSizeRounded)
                {
                    ThreadResourceLimit?.Release(LimitableResource.Thread, 1);

                    return KernelResult.OutOfMemory;
                }

                if (StackSizeRounded != 0 && ResourceLimit != null)
                {
                    MemoryResourceLimit = ResourceLimit;

                    if (!MemoryResourceLimit.Reserve(LimitableResource.Memory, StackSizeRounded))
                    {
                        ThreadResourceLimit?.Release(LimitableResource.Thread, 1);

                        return KernelResult.ResLimitExceeded;
                    }
                }

                KernelResult Result;

                KThread MainThread = null;

                ulong StackTop = 0;

                void CleanUpForError()
                {
                    MainThread?.Terminate();
                    HandleTable.Destroy();

                    if (MainThreadStackSize != 0)
                    {
                        ulong StackBottom = StackTop - MainThreadStackSize;

                        ulong StackPagesCount = MainThreadStackSize / KMemoryManager.PageSize;

                        MemoryManager.UnmapForKernel(StackBottom, StackPagesCount, MemoryState.Stack);
                    }

                    MemoryResourceLimit?.Release(LimitableResource.Memory, StackSizeRounded);
                    ThreadResourceLimit?.Release(LimitableResource.Thread, 1);
                }

                if (StackSizeRounded != 0)
                {
                    ulong StackPagesCount = StackSizeRounded / KMemoryManager.PageSize;

                    ulong RegionStart = MemoryManager.StackRegionStart;
                    ulong RegionSize  = MemoryManager.StackRegionEnd - RegionStart;

                    ulong RegionPagesCount = RegionSize / KMemoryManager.PageSize;

                    Result = MemoryManager.AllocateOrMapPa(
                        StackPagesCount,
                        KMemoryManager.PageSize,
                        0,
                        false,
                        RegionStart,
                        RegionPagesCount,
                        MemoryState.Stack,
                        MemoryPermission.ReadAndWrite,
                        out ulong StackBottom);

                    if (Result != KernelResult.Success)
                    {
                        CleanUpForError();

                        return Result;
                    }

                    MainThreadStackSize += StackSizeRounded;

                    StackTop = StackBottom + StackSizeRounded;
                }

                ulong HeapCapacity = MemoryUsageCapacity - MainThreadStackSize - ImageSize;

                Result = MemoryManager.SetHeapCapacity(HeapCapacity);

                if (Result != KernelResult.Success)
                {
                    CleanUpForError();

                    return Result;
                }

                HandleTable = new KHandleTable(System);

                Result = HandleTable.Initialize(Capabilities.HandleTableSize);

                if (Result != KernelResult.Success)
                {
                    CleanUpForError();

                    return Result;
                }

                MainThread = new KThread(System);

                Result = MainThread.Initialize(
                    Entrypoint,
                    0,
                    StackTop,
                    MainThreadPriority,
                    DefaultCpuCore,
                    this);

                if (Result != KernelResult.Success)
                {
                    CleanUpForError();

                    return Result;
                }

                Result = HandleTable.GenerateHandle(MainThread, out int MainThreadHandle);

                if (Result != KernelResult.Success)
                {
                    CleanUpForError();

                    return Result;
                }

                MainThread.SetEntryArguments(0, MainThreadHandle);

                ProcessState OldState = State;
                ProcessState NewState = State != ProcessState.Created
                    ? ProcessState.Attached
                    : ProcessState.Started;

                SetState(NewState);

                //TODO: We can't call KThread.Start from a non-guest thread.
                //We will need to make some changes to allow the creation of
                //dummy threads that will be used to initialize the current
                //thread on KCoreContext so that GetCurrentThread doesn't fail.
                /* Result = MainThread.Start();

                if (Result != KernelResult.Success)
                {
                    SetState(OldState);

                    CleanUpForError();
                } */

                MainThread.Reschedule(ThreadSchedState.Running);

                return Result;
            }
        }

        private void SetState(ProcessState NewState)
        {
            if (State != NewState)
            {
                State    = NewState;
                Signaled = true;

                Signal();
            }
        }

        public KernelResult InitializeThread(
            KThread Thread,
            ulong   Entrypoint,
            ulong   ArgsPtr,
            ulong   StackTop,
            int     Priority,
            int     CpuCore)
        {
            lock (ProcessLock)
            {
                return Thread.Initialize(Entrypoint, ArgsPtr, StackTop, Priority, CpuCore, this);
            }
        }

        public void SubscribeThreadEventHandlers(CpuThread Context)
        {
            Context.ThreadState.Interrupt += InterruptHandler;
            Context.ThreadState.SvcCall   += SvcHandler.SvcCall;
        }

        private void InterruptHandler(object sender, EventArgs e)
        {
            System.Scheduler.ContextSwitch();
        }

        public void IncrementThreadCount()
        {
            Interlocked.Increment(ref ThreadCount);

            System.ThreadCounter.AddCount();
        }

        public void DecrementThreadCountAndTerminateIfZero()
        {
            System.ThreadCounter.Signal();

            if (Interlocked.Decrement(ref ThreadCount) == 0)
            {
                Terminate();
            }
        }

        public ulong GetMemoryCapacity()
        {
            ulong TotalCapacity = (ulong)ResourceLimit.GetRemainingValue(LimitableResource.Memory);

            TotalCapacity += MemoryManager.GetTotalHeapSize();

            TotalCapacity += GetPersonalMmHeapSize();

            TotalCapacity += ImageSize + MainThreadStackSize;

            if (TotalCapacity <= MemoryUsageCapacity)
            {
                return TotalCapacity;
            }

            return MemoryUsageCapacity;
        }

        public ulong GetMemoryUsage()
        {
            return ImageSize + MainThreadStackSize + MemoryManager.GetTotalHeapSize() + GetPersonalMmHeapSize();
        }

        public ulong GetMemoryCapacityWithoutPersonalMmHeap()
        {
            return GetMemoryCapacity() - GetPersonalMmHeapSize();
        }

        public ulong GetMemoryUsageWithoutPersonalMmHeap()
        {
            return GetMemoryUsage() - GetPersonalMmHeapSize();
        }

        private ulong GetPersonalMmHeapSize()
        {
            return GetPersonalMmHeapSize(PersonalMmHeapPagesCount, MemRegion);
        }

        private static ulong GetPersonalMmHeapSize(ulong PersonalMmHeapPagesCount, MemoryRegion MemRegion)
        {
            if (MemRegion == MemoryRegion.Applet)
            {
                return 0;
            }

            return PersonalMmHeapPagesCount * KMemoryManager.PageSize;
        }

        public void AddThread(KThread Thread)
        {
            lock (ThreadingLock)
            {
                Thread.ProcessListNode = Threads.AddLast(Thread);
            }
        }

        public void RemoveThread(KThread Thread)
        {
            lock (ThreadingLock)
            {
                Threads.Remove(Thread.ProcessListNode);
            }
        }

        public bool IsCpuCoreAllowed(int Core)
        {
            return (Capabilities.AllowedCpuCoresMask & (1L << Core)) != 0;
        }

        public bool IsPriorityAllowed(int Priority)
        {
            return (Capabilities.AllowedThreadPriosMask & (1L << Priority)) != 0;
        }

        public override bool IsSignaled()
        {
            return Signaled;
        }

        public KernelResult Terminate()
        {
            KernelResult Result;

            bool ShallTerminate = false;

            System.CriticalSection.Enter();

            lock (ProcessLock)
            {
                if (State >= ProcessState.Started)
                {
                    if (State == ProcessState.Started  ||
                        State == ProcessState.Crashed  ||
                        State == ProcessState.Attached ||
                        State == ProcessState.DebugSuspended)
                    {
                        SetState(ProcessState.Exiting);

                        ShallTerminate = true;
                    }

                    Result = KernelResult.Success;
                }
                else
                {
                    Result = KernelResult.InvalidState;
                }
            }

            System.CriticalSection.Leave();

            if (ShallTerminate)
            {
                //UnpauseAndTerminateAllThreadsExcept(System.Scheduler.GetCurrentThread());

                HandleTable.Destroy();

                SignalExitForDebugEvent();
                SignalExit();
            }

            return Result;
        }

        private void UnpauseAndTerminateAllThreadsExcept(KThread Thread)
        {
            //TODO.
        }

        private void SignalExitForDebugEvent()
        {
            //TODO: Debug events.
        }

        private void SignalExit()
        {
            if (ResourceLimit != null)
            {
                ResourceLimit.Release(LimitableResource.Memory, GetMemoryUsage());
            }

            System.CriticalSection.Enter();

            SetState(ProcessState.Exited);

            System.CriticalSection.Leave();
        }

        public KernelResult ClearIfNotExited()
        {
            KernelResult Result;

            System.CriticalSection.Enter();

            lock (ProcessLock)
            {
                if (State != ProcessState.Exited && Signaled)
                {
                    Signaled = false;

                    Result = KernelResult.Success;
                }
                else
                {
                    Result = KernelResult.InvalidState;
                }
            }

            System.CriticalSection.Leave();

            return Result;
        }

        public void StopAllThreads()
        {
            lock (ThreadingLock)
            {
                foreach (KThread Thread in Threads)
                {
                    Thread.Context.StopExecution();

                    System.Scheduler.CoreManager.Set(Thread.Context.Work);
                }
            }
        }

        private void InvalidAccessHandler(object sender, InvalidAccessEventArgs e)
        {
            PrintCurrentThreadStackTrace();
        }

        public void PrintCurrentThreadStackTrace()
        {
            System.Scheduler.GetCurrentThread().PrintGuestStackTrace();
        }

        private void CpuTraceHandler(object sender, CpuTraceEventArgs e)
        {
            Logger.PrintInfo(LogClass.Cpu, $"Executing at 0x{e.Position:X16}.");
        }
    }
}