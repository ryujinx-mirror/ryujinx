using ChocolArm64;
using ChocolArm64.Events;
using ChocolArm64.Memory;
using ChocolArm64.Translation;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.SupervisorCall;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Process
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

        private SortedDictionary<ulong, KTlsPageInfo> _fullTlsPages;
        private SortedDictionary<ulong, KTlsPageInfo> _freeTlsPages;

        public int DefaultCpuCore { get; set; }

        public bool Debug { get; private set; }

        public KResourceLimit ResourceLimit { get; private set; }

        public ulong PersonalMmHeapPagesCount { get; private set; }

        private ProcessState _state;

        private object _processLock;
        private object _threadingLock;

        public KAddressArbiter AddressArbiter { get; private set; }

        public long[] RandomEntropy { get; private set; }

        private bool _signaled;
        private bool _useSystemMemBlocks;

        public string Name { get; private set; }

        private int _threadCount;

        public int MmuFlags { get; private set; }

        private MemoryRegion _memRegion;

        public KProcessCapabilities Capabilities { get; private set; }

        public long TitleId { get; private set; }
        public long Pid     { get; private set; }

        private long  _creationTimestamp;
        private ulong _entrypoint;
        private ulong _imageSize;
        private ulong _mainThreadStackSize;
        private ulong _memoryUsageCapacity;
        private int   _category;

        public KHandleTable HandleTable { get; private set; }

        public ulong UserExceptionContextAddress { get; private set; }

        private LinkedList<KThread> _threads;

        public bool IsPaused { get; private set; }

        public MemoryManager CpuMemory { get; private set; }

        public Translator Translator { get; private set; }

        private SvcHandler _svcHandler;

        private Horizon _system;

        public HleProcessDebugger Debugger { get; private set; }

        public KProcess(Horizon system) : base(system)
        {
            _processLock   = new object();
            _threadingLock = new object();

            _system = system;

            AddressArbiter = new KAddressArbiter(system);

            _fullTlsPages = new SortedDictionary<ulong, KTlsPageInfo>();
            _freeTlsPages = new SortedDictionary<ulong, KTlsPageInfo>();

            Capabilities = new KProcessCapabilities();

            RandomEntropy = new long[KScheduler.CpuCoresCount];

            _threads = new LinkedList<KThread>();

            _svcHandler = new SvcHandler(system.Device, this);

            Debugger = new HleProcessDebugger(this);
        }

        public KernelResult InitializeKip(
            ProcessCreationInfo creationInfo,
            int[]               caps,
            KPageList           pageList,
            KResourceLimit      resourceLimit,
            MemoryRegion        memRegion)
        {
            ResourceLimit = resourceLimit;
            _memRegion     = memRegion;

            AddressSpaceType addrSpaceType = (AddressSpaceType)((creationInfo.MmuFlags >> 1) & 7);

            InitializeMemoryManager(addrSpaceType, memRegion);

            bool aslrEnabled = ((creationInfo.MmuFlags >> 5) & 1) != 0;

            ulong codeAddress = creationInfo.CodeAddress;

            ulong codeSize = (ulong)creationInfo.CodePagesCount * KMemoryManager.PageSize;

            KMemoryBlockAllocator memoryBlockAllocator = (MmuFlags & 0x40) != 0
                ? System.LargeMemoryBlockAllocator
                : System.SmallMemoryBlockAllocator;

            KernelResult result = MemoryManager.InitializeForProcess(
                addrSpaceType,
                aslrEnabled,
                !aslrEnabled,
                memRegion,
                codeAddress,
                codeSize,
                memoryBlockAllocator);

            if (result != KernelResult.Success)
            {
                return result;
            }

            if (!ValidateCodeAddressAndSize(codeAddress, codeSize))
            {
                return KernelResult.InvalidMemRange;
            }

            result = MemoryManager.MapPages(
                codeAddress,
                pageList,
                MemoryState.CodeStatic,
                MemoryPermission.None);

            if (result != KernelResult.Success)
            {
                return result;
            }

            result = Capabilities.InitializeForKernel(caps, MemoryManager);

            if (result != KernelResult.Success)
            {
                return result;
            }

            Pid = System.GetKipId();

            if (Pid == 0 || (ulong)Pid >= Horizon.InitialProcessId)
            {
                throw new InvalidOperationException($"Invalid KIP Id {Pid}.");
            }

            result = ParseProcessInfo(creationInfo);

            return result;
        }

        public KernelResult Initialize(
            ProcessCreationInfo creationInfo,
            int[]               caps,
            KResourceLimit      resourceLimit,
            MemoryRegion        memRegion)
        {
            ResourceLimit = resourceLimit;
            _memRegion    = memRegion;

            ulong personalMmHeapSize = GetPersonalMmHeapSize((ulong)creationInfo.PersonalMmHeapPagesCount, memRegion);

            ulong codePagesCount = (ulong)creationInfo.CodePagesCount;

            ulong neededSizeForProcess = personalMmHeapSize + codePagesCount * KMemoryManager.PageSize;

            if (neededSizeForProcess != 0 && resourceLimit != null)
            {
                if (!resourceLimit.Reserve(LimitableResource.Memory, neededSizeForProcess))
                {
                    return KernelResult.ResLimitExceeded;
                }
            }

            void CleanUpForError()
            {
                if (neededSizeForProcess != 0 && resourceLimit != null)
                {
                    resourceLimit.Release(LimitableResource.Memory, neededSizeForProcess);
                }
            }

            PersonalMmHeapPagesCount = (ulong)creationInfo.PersonalMmHeapPagesCount;

            KMemoryBlockAllocator memoryBlockAllocator;

            if (PersonalMmHeapPagesCount != 0)
            {
                memoryBlockAllocator = new KMemoryBlockAllocator(PersonalMmHeapPagesCount * KMemoryManager.PageSize);
            }
            else
            {
                memoryBlockAllocator = (MmuFlags & 0x40) != 0
                    ? System.LargeMemoryBlockAllocator
                    : System.SmallMemoryBlockAllocator;
            }

            AddressSpaceType addrSpaceType = (AddressSpaceType)((creationInfo.MmuFlags >> 1) & 7);

            InitializeMemoryManager(addrSpaceType, memRegion);

            bool aslrEnabled = ((creationInfo.MmuFlags >> 5) & 1) != 0;

            ulong codeAddress = creationInfo.CodeAddress;

            ulong codeSize = codePagesCount * KMemoryManager.PageSize;

            KernelResult result = MemoryManager.InitializeForProcess(
                addrSpaceType,
                aslrEnabled,
                !aslrEnabled,
                memRegion,
                codeAddress,
                codeSize,
                memoryBlockAllocator);

            if (result != KernelResult.Success)
            {
                CleanUpForError();

                return result;
            }

            if (!ValidateCodeAddressAndSize(codeAddress, codeSize))
            {
                CleanUpForError();

                return KernelResult.InvalidMemRange;
            }

            result = MemoryManager.MapNewProcessCode(
                codeAddress,
                codePagesCount,
                MemoryState.CodeStatic,
                MemoryPermission.None);

            if (result != KernelResult.Success)
            {
                CleanUpForError();

                return result;
            }

            result = Capabilities.InitializeForUser(caps, MemoryManager);

            if (result != KernelResult.Success)
            {
                CleanUpForError();

                return result;
            }

            Pid = System.GetProcessId();

            if (Pid == -1 || (ulong)Pid < Horizon.InitialProcessId)
            {
                throw new InvalidOperationException($"Invalid Process Id {Pid}.");
            }

            result = ParseProcessInfo(creationInfo);

            if (result != KernelResult.Success)
            {
                CleanUpForError();
            }

            return result;
        }

        private bool ValidateCodeAddressAndSize(ulong address, ulong size)
        {
            ulong codeRegionStart;
            ulong codeRegionSize;

            switch (MemoryManager.AddrSpaceWidth)
            {
                case 32:
                    codeRegionStart = 0x200000;
                    codeRegionSize  = 0x3fe00000;
                    break;

                case 36:
                    codeRegionStart = 0x8000000;
                    codeRegionSize  = 0x78000000;
                    break;

                case 39:
                    codeRegionStart = 0x8000000;
                    codeRegionSize  = 0x7ff8000000;
                    break;

                default: throw new InvalidOperationException("Invalid address space width on memory manager.");
            }

            ulong endAddr = address + size;

            ulong codeRegionEnd = codeRegionStart + codeRegionSize;

            if (endAddr     <= address ||
                endAddr - 1 >  codeRegionEnd - 1)
            {
                return false;
            }

            if (MemoryManager.InsideHeapRegion (address, size) ||
                MemoryManager.InsideAliasRegion(address, size))
            {
                return false;
            }

            return true;
        }

        private KernelResult ParseProcessInfo(ProcessCreationInfo creationInfo)
        {
            // Ensure that the current kernel version is equal or above to the minimum required.
            uint requiredKernelVersionMajor =  (uint)Capabilities.KernelReleaseVersion >> 19;
            uint requiredKernelVersionMinor = ((uint)Capabilities.KernelReleaseVersion >> 15) & 0xf;

            if (System.EnableVersionChecks)
            {
                if (requiredKernelVersionMajor > KernelVersionMajor)
                {
                    return KernelResult.InvalidCombination;
                }

                if (requiredKernelVersionMajor != KernelVersionMajor && requiredKernelVersionMajor < 3)
                {
                    return KernelResult.InvalidCombination;
                }

                if (requiredKernelVersionMinor > KernelVersionMinor)
                {
                    return KernelResult.InvalidCombination;
                }
            }

            KernelResult result = AllocateThreadLocalStorage(out ulong userExceptionContextAddress);

            if (result != KernelResult.Success)
            {
                return result;
            }

            UserExceptionContextAddress = userExceptionContextAddress;

            MemoryHelper.FillWithZeros(CpuMemory, (long)userExceptionContextAddress, KTlsPageInfo.TlsEntrySize);

            Name = creationInfo.Name;

            _state = ProcessState.Created;

            _creationTimestamp = PerformanceCounter.ElapsedMilliseconds;

            MmuFlags    = creationInfo.MmuFlags;
            _category   = creationInfo.Category;
            TitleId     = creationInfo.TitleId;
            _entrypoint = creationInfo.CodeAddress;
            _imageSize  = (ulong)creationInfo.CodePagesCount * KMemoryManager.PageSize;

            _useSystemMemBlocks = ((MmuFlags >> 6) & 1) != 0;

            switch ((AddressSpaceType)((MmuFlags >> 1) & 7))
            {
                case AddressSpaceType.Addr32Bits:
                case AddressSpaceType.Addr36Bits:
                case AddressSpaceType.Addr39Bits:
                    _memoryUsageCapacity = MemoryManager.HeapRegionEnd -
                                           MemoryManager.HeapRegionStart;
                    break;

                case AddressSpaceType.Addr32BitsNoMap:
                    _memoryUsageCapacity = MemoryManager.HeapRegionEnd -
                                           MemoryManager.HeapRegionStart +
                                           MemoryManager.AliasRegionEnd -
                                           MemoryManager.AliasRegionStart;
                    break;

                default: throw new InvalidOperationException($"Invalid MMU flags value 0x{MmuFlags:x2}.");
            }

            GenerateRandomEntropy();

            return KernelResult.Success;
        }

        public KernelResult AllocateThreadLocalStorage(out ulong address)
        {
            System.CriticalSection.Enter();

            KernelResult result;

            if (_freeTlsPages.Count > 0)
            {
                // If we have free TLS pages available, just use the first one.
                KTlsPageInfo pageInfo = _freeTlsPages.Values.First();

                if (!pageInfo.TryGetFreePage(out address))
                {
                    throw new InvalidOperationException("Unexpected failure getting free TLS page!");
                }

                if (pageInfo.IsFull())
                {
                    _freeTlsPages.Remove(pageInfo.PageAddr);

                    _fullTlsPages.Add(pageInfo.PageAddr, pageInfo);
                }

                result = KernelResult.Success;
            }
            else
            {
                // Otherwise, we need to create a new one.
                result = AllocateTlsPage(out KTlsPageInfo pageInfo);

                if (result == KernelResult.Success)
                {
                    if (!pageInfo.TryGetFreePage(out address))
                    {
                        throw new InvalidOperationException("Unexpected failure getting free TLS page!");
                    }

                    _freeTlsPages.Add(pageInfo.PageAddr, pageInfo);
                }
                else
                {
                    address = 0;
                }
            }

            System.CriticalSection.Leave();

            return result;
        }

        private KernelResult AllocateTlsPage(out KTlsPageInfo pageInfo)
        {
            pageInfo = default(KTlsPageInfo);

            if (!System.UserSlabHeapPages.TryGetItem(out ulong tlsPagePa))
            {
                return KernelResult.OutOfMemory;
            }

            ulong regionStart = MemoryManager.TlsIoRegionStart;
            ulong regionSize  = MemoryManager.TlsIoRegionEnd - regionStart;

            ulong regionPagesCount = regionSize / KMemoryManager.PageSize;

            KernelResult result = MemoryManager.AllocateOrMapPa(
                1,
                KMemoryManager.PageSize,
                tlsPagePa,
                true,
                regionStart,
                regionPagesCount,
                MemoryState.ThreadLocal,
                MemoryPermission.ReadAndWrite,
                out ulong tlsPageVa);

            if (result != KernelResult.Success)
            {
                System.UserSlabHeapPages.Free(tlsPagePa);
            }
            else
            {
                pageInfo = new KTlsPageInfo(tlsPageVa);

                MemoryHelper.FillWithZeros(CpuMemory, (long)tlsPageVa, KMemoryManager.PageSize);
            }

            return result;
        }

        public KernelResult FreeThreadLocalStorage(ulong tlsSlotAddr)
        {
            ulong tlsPageAddr = BitUtils.AlignDown(tlsSlotAddr, KMemoryManager.PageSize);

            System.CriticalSection.Enter();

            KernelResult result = KernelResult.Success;

            KTlsPageInfo pageInfo = null;

            if (_fullTlsPages.TryGetValue(tlsPageAddr, out pageInfo))
            {
                // TLS page was full, free slot and move to free pages tree.
                _fullTlsPages.Remove(tlsPageAddr);

                _freeTlsPages.Add(tlsPageAddr, pageInfo);
            }
            else if (!_freeTlsPages.TryGetValue(tlsPageAddr, out pageInfo))
            {
                result = KernelResult.InvalidAddress;
            }

            if (pageInfo != null)
            {
                pageInfo.FreeTlsSlot(tlsSlotAddr);

                if (pageInfo.IsEmpty())
                {
                    // TLS page is now empty, we should ensure it is removed
                    // from all trees, and free the memory it was using.
                    _freeTlsPages.Remove(tlsPageAddr);

                    System.CriticalSection.Leave();

                    FreeTlsPage(pageInfo);

                    return KernelResult.Success;
                }
            }

            System.CriticalSection.Leave();

            return result;
        }

        private KernelResult FreeTlsPage(KTlsPageInfo pageInfo)
        {
            if (!MemoryManager.ConvertVaToPa(pageInfo.PageAddr, out ulong tlsPagePa))
            {
                throw new InvalidOperationException("Unexpected failure translating virtual address to physical.");
            }

            KernelResult result = MemoryManager.UnmapForKernel(pageInfo.PageAddr, 1, MemoryState.ThreadLocal);

            if (result == KernelResult.Success)
            {
                System.UserSlabHeapPages.Free(tlsPagePa);
            }

            return result;
        }

        private void GenerateRandomEntropy()
        {
            // TODO.
        }

        public KernelResult Start(int mainThreadPriority, ulong stackSize)
        {
            lock (_processLock)
            {
                if (_state > ProcessState.CreatedAttached)
                {
                    return KernelResult.InvalidState;
                }

                if (ResourceLimit != null && !ResourceLimit.Reserve(LimitableResource.Thread, 1))
                {
                    return KernelResult.ResLimitExceeded;
                }

                KResourceLimit threadResourceLimit = ResourceLimit;
                KResourceLimit memoryResourceLimit = null;

                if (_mainThreadStackSize != 0)
                {
                    throw new InvalidOperationException("Trying to start a process with a invalid state!");
                }

                ulong stackSizeRounded = BitUtils.AlignUp(stackSize, KMemoryManager.PageSize);

                ulong neededSize = stackSizeRounded + _imageSize;

                // Check if the needed size for the code and the stack will fit on the
                // memory usage capacity of this Process. Also check for possible overflow
                // on the above addition.
                if (neededSize > _memoryUsageCapacity ||
                    neededSize < stackSizeRounded)
                {
                    threadResourceLimit?.Release(LimitableResource.Thread, 1);

                    return KernelResult.OutOfMemory;
                }

                if (stackSizeRounded != 0 && ResourceLimit != null)
                {
                    memoryResourceLimit = ResourceLimit;

                    if (!memoryResourceLimit.Reserve(LimitableResource.Memory, stackSizeRounded))
                    {
                        threadResourceLimit?.Release(LimitableResource.Thread, 1);

                        return KernelResult.ResLimitExceeded;
                    }
                }

                KernelResult result;

                KThread mainThread = null;

                ulong stackTop = 0;

                void CleanUpForError()
                {
                    HandleTable.Destroy();

                    mainThread?.DecrementReferenceCount();

                    if (_mainThreadStackSize != 0)
                    {
                        ulong stackBottom = stackTop - _mainThreadStackSize;

                        ulong stackPagesCount = _mainThreadStackSize / KMemoryManager.PageSize;

                        MemoryManager.UnmapForKernel(stackBottom, stackPagesCount, MemoryState.Stack);

                        _mainThreadStackSize = 0;
                    }

                    memoryResourceLimit?.Release(LimitableResource.Memory, stackSizeRounded);
                    threadResourceLimit?.Release(LimitableResource.Thread, 1);
                }

                if (stackSizeRounded != 0)
                {
                    ulong stackPagesCount = stackSizeRounded / KMemoryManager.PageSize;

                    ulong regionStart = MemoryManager.StackRegionStart;
                    ulong regionSize  = MemoryManager.StackRegionEnd - regionStart;

                    ulong regionPagesCount = regionSize / KMemoryManager.PageSize;

                    result = MemoryManager.AllocateOrMapPa(
                        stackPagesCount,
                        KMemoryManager.PageSize,
                        0,
                        false,
                        regionStart,
                        regionPagesCount,
                        MemoryState.Stack,
                        MemoryPermission.ReadAndWrite,
                        out ulong stackBottom);

                    if (result != KernelResult.Success)
                    {
                        CleanUpForError();

                        return result;
                    }

                    _mainThreadStackSize += stackSizeRounded;

                    stackTop = stackBottom + stackSizeRounded;
                }

                ulong heapCapacity = _memoryUsageCapacity - _mainThreadStackSize - _imageSize;

                result = MemoryManager.SetHeapCapacity(heapCapacity);

                if (result != KernelResult.Success)
                {
                    CleanUpForError();

                    return result;
                }

                HandleTable = new KHandleTable(System);

                result = HandleTable.Initialize(Capabilities.HandleTableSize);

                if (result != KernelResult.Success)
                {
                    CleanUpForError();

                    return result;
                }

                mainThread = new KThread(System);

                result = mainThread.Initialize(
                    _entrypoint,
                    0,
                    stackTop,
                    mainThreadPriority,
                    DefaultCpuCore,
                    this);

                if (result != KernelResult.Success)
                {
                    CleanUpForError();

                    return result;
                }

                result = HandleTable.GenerateHandle(mainThread, out int mainThreadHandle);

                if (result != KernelResult.Success)
                {
                    CleanUpForError();

                    return result;
                }

                mainThread.SetEntryArguments(0, mainThreadHandle);

                ProcessState oldState = _state;
                ProcessState newState = _state != ProcessState.Created
                    ? ProcessState.Attached
                    : ProcessState.Started;

                SetState(newState);

                // TODO: We can't call KThread.Start from a non-guest thread.
                // We will need to make some changes to allow the creation of
                // dummy threads that will be used to initialize the current
                // thread on KCoreContext so that GetCurrentThread doesn't fail.
                /* Result = MainThread.Start();

                if (Result != KernelResult.Success)
                {
                    SetState(OldState);

                    CleanUpForError();
                } */

                mainThread.Reschedule(ThreadSchedState.Running);

                if (result == KernelResult.Success)
                {
                    mainThread.IncrementReferenceCount();
                }

                mainThread.DecrementReferenceCount();

                return result;
            }
        }

        private void SetState(ProcessState newState)
        {
            if (_state != newState)
            {
                _state    = newState;
                _signaled = true;

                Signal();
            }
        }

        public KernelResult InitializeThread(
            KThread thread,
            ulong   entrypoint,
            ulong   argsPtr,
            ulong   stackTop,
            int     priority,
            int     cpuCore)
        {
            lock (_processLock)
            {
                return thread.Initialize(entrypoint, argsPtr, stackTop, priority, cpuCore, this);
            }
        }

        public void SubscribeThreadEventHandlers(CpuThread context)
        {
            context.ThreadState.Interrupt += InterruptHandler;
            context.ThreadState.SvcCall   += _svcHandler.SvcCall;
            context.ThreadState.Undefined += UndefinedInstructionHandler;
        }

        private void InterruptHandler(object sender, EventArgs e)
        {
            System.Scheduler.ContextSwitch();
        }

        public void IncrementThreadCount()
        {
            Interlocked.Increment(ref _threadCount);

            System.ThreadCounter.AddCount();
        }

        public void DecrementThreadCountAndTerminateIfZero()
        {
            System.ThreadCounter.Signal();

            if (Interlocked.Decrement(ref _threadCount) == 0)
            {
                Terminate();
            }
        }

        public ulong GetMemoryCapacity()
        {
            ulong totalCapacity = (ulong)ResourceLimit.GetRemainingValue(LimitableResource.Memory);

            totalCapacity += MemoryManager.GetTotalHeapSize();

            totalCapacity += GetPersonalMmHeapSize();

            totalCapacity += _imageSize + _mainThreadStackSize;

            if (totalCapacity <= _memoryUsageCapacity)
            {
                return totalCapacity;
            }

            return _memoryUsageCapacity;
        }

        public ulong GetMemoryUsage()
        {
            return _imageSize + _mainThreadStackSize + MemoryManager.GetTotalHeapSize() + GetPersonalMmHeapSize();
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
            return GetPersonalMmHeapSize(PersonalMmHeapPagesCount, _memRegion);
        }

        private static ulong GetPersonalMmHeapSize(ulong personalMmHeapPagesCount, MemoryRegion memRegion)
        {
            if (memRegion == MemoryRegion.Applet)
            {
                return 0;
            }

            return personalMmHeapPagesCount * KMemoryManager.PageSize;
        }

        public void AddThread(KThread thread)
        {
            lock (_threadingLock)
            {
                thread.ProcessListNode = _threads.AddLast(thread);
            }
        }

        public void RemoveThread(KThread thread)
        {
            lock (_threadingLock)
            {
                _threads.Remove(thread.ProcessListNode);
            }
        }

        public bool IsCpuCoreAllowed(int core)
        {
            return (Capabilities.AllowedCpuCoresMask & (1L << core)) != 0;
        }

        public bool IsPriorityAllowed(int priority)
        {
            return (Capabilities.AllowedThreadPriosMask & (1L << priority)) != 0;
        }

        public override bool IsSignaled()
        {
            return _signaled;
        }

        public KernelResult Terminate()
        {
            KernelResult result;

            bool shallTerminate = false;

            System.CriticalSection.Enter();

            lock (_processLock)
            {
                if (_state >= ProcessState.Started)
                {
                    if (_state == ProcessState.Started  ||
                        _state == ProcessState.Crashed  ||
                        _state == ProcessState.Attached ||
                        _state == ProcessState.DebugSuspended)
                    {
                        SetState(ProcessState.Exiting);

                        shallTerminate = true;
                    }

                    result = KernelResult.Success;
                }
                else
                {
                    result = KernelResult.InvalidState;
                }
            }

            System.CriticalSection.Leave();

            if (shallTerminate)
            {
                // UnpauseAndTerminateAllThreadsExcept(System.Scheduler.GetCurrentThread());

                HandleTable.Destroy();

                SignalExitForDebugEvent();
                SignalExit();
            }

            return result;
        }

        private void UnpauseAndTerminateAllThreadsExcept(KThread thread)
        {
            // TODO.
        }

        private void SignalExitForDebugEvent()
        {
            // TODO: Debug events.
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
            KernelResult result;

            System.CriticalSection.Enter();

            lock (_processLock)
            {
                if (_state != ProcessState.Exited && _signaled)
                {
                    _signaled = false;

                    result = KernelResult.Success;
                }
                else
                {
                    result = KernelResult.InvalidState;
                }
            }

            System.CriticalSection.Leave();

            return result;
        }

        public void StopAllThreads()
        {
            lock (_threadingLock)
            {
                foreach (KThread thread in _threads)
                {
                    thread.Context.StopExecution();

                    System.Scheduler.CoreManager.Set(thread.Context.Work);
                }
            }
        }

        private void InitializeMemoryManager(AddressSpaceType addrSpaceType, MemoryRegion memRegion)
        {
            int addrSpaceBits;

            switch (addrSpaceType)
            {
                case AddressSpaceType.Addr32Bits:      addrSpaceBits = 32; break;
                case AddressSpaceType.Addr36Bits:      addrSpaceBits = 36; break;
                case AddressSpaceType.Addr32BitsNoMap: addrSpaceBits = 32; break;
                case AddressSpaceType.Addr39Bits:      addrSpaceBits = 39; break;

                default: throw new ArgumentException(nameof(addrSpaceType));
            }

            bool useFlatPageTable = memRegion == MemoryRegion.Application;

            CpuMemory = new MemoryManager(_system.Device.Memory.RamPointer, addrSpaceBits, useFlatPageTable);

            MemoryManager = new KMemoryManager(_system, CpuMemory);

            Translator = new Translator(CpuMemory);

            Translator.CpuTrace += CpuTraceHandler;
        }

        public void PrintCurrentThreadStackTrace()
        {
            System.Scheduler.GetCurrentThread().PrintGuestStackTrace();
        }

        private void CpuTraceHandler(object sender, CpuTraceEventArgs e)
        {
            Logger.PrintInfo(LogClass.Cpu, $"Executing at 0x{e.Position:X16}.");
        }

        private void UndefinedInstructionHandler(object sender, InstUndefinedEventArgs e)
        {
            throw new UndefinedInstructionException(e.Position, e.RawOpCode);
        }
    }
}