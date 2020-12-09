using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KThread : KSynchronizationObject, IKFutureSchedulerObject
    {
        public const int MaxWaitSyncObjects = 64;

        private ManualResetEvent _schedulerWaitEvent;

        public ManualResetEvent SchedulerWaitEvent => _schedulerWaitEvent;

        public Thread HostThread { get; private set; }

        public ARMeilleure.State.ExecutionContext Context { get; private set; }

        public KThreadContext ThreadContext { get; private set; }

        public int DynamicPriority { get; set; }
        public long AffinityMask { get; set; }

        public long ThreadUid { get; private set; }

        private long _totalTimeRunning;

        public long TotalTimeRunning => _totalTimeRunning;

        public KSynchronizationObject SignaledObj { get; set; }

        public ulong CondVarAddress { get; set; }

        private ulong _entrypoint;
        private ThreadStart _customThreadStart;
        private bool _forcedUnschedulable;

        public bool IsSchedulable => _customThreadStart == null && !_forcedUnschedulable;

        public ulong MutexAddress { get; set; }

        public KProcess Owner { get; private set; }

        private ulong _tlsAddress;

        public ulong TlsAddress => _tlsAddress;
        public ulong TlsDramAddress { get; private set; }

        public KSynchronizationObject[] WaitSyncObjects { get; }
        public int[] WaitSyncHandles { get; }

        public long LastScheduledTime { get; set; }

        public LinkedListNode<KThread>[] SiblingsPerCore { get; private set; }

        public LinkedList<KThread> Withholder { get; set; }
        public LinkedListNode<KThread> WithholderNode { get; set; }

        public LinkedListNode<KThread> ProcessListNode { get; set; }

        private LinkedList<KThread> _mutexWaiters;
        private LinkedListNode<KThread> _mutexWaiterNode;

        public KThread MutexOwner { get; private set; }

        public int ThreadHandleForUserMutex { get; set; }

        private ThreadSchedState _forcePauseFlags;

        public KernelResult ObjSyncResult { get; set; }

        public int BasePriority { get; set; }
        public int PreferredCore { get; set; }

        public int CurrentCore { get; set; }
        public int ActiveCore { get; set; }

        private long _affinityMaskOverride;
        private int _preferredCoreOverride;
#pragma warning disable CS0649
        private int _affinityOverrideCount;
#pragma warning restore CS0649

        public ThreadSchedState SchedFlags { get; private set; }

        private int _shallBeTerminated;

        public bool ShallBeTerminated
        {
            get => _shallBeTerminated != 0;
            set => _shallBeTerminated = value ? 1 : 0;
        }

        public bool TerminationRequested => ShallBeTerminated || SchedFlags == ThreadSchedState.TerminationPending;

        public bool SyncCancelled { get; set; }
        public bool WaitingSync { get; set; }

        private int _hasExited;
        private bool _hasBeenInitialized;
        private bool _hasBeenReleased;

        public bool WaitingInArbitration { get; set; }

        public long LastPc { get; set; }

        public KThread(KernelContext context) : base(context)
        {
            WaitSyncObjects = new KSynchronizationObject[MaxWaitSyncObjects];
            WaitSyncHandles = new int[MaxWaitSyncObjects];

            SiblingsPerCore = new LinkedListNode<KThread>[KScheduler.CpuCoresCount];

            _mutexWaiters = new LinkedList<KThread>();
        }

        public KernelResult Initialize(
            ulong entrypoint,
            ulong argsPtr,
            ulong stackTop,
            int priority,
            int cpuCore,
            KProcess owner,
            ThreadType type,
            ThreadStart customThreadStart = null)
        {
            if ((uint)type > 3)
            {
                throw new ArgumentException($"Invalid thread type \"{type}\".");
            }

            ThreadContext = new KThreadContext();

            PreferredCore = cpuCore;
            AffinityMask |= 1L << cpuCore;

            SchedFlags = type == ThreadType.Dummy
                ? ThreadSchedState.Running
                : ThreadSchedState.None;

            ActiveCore = cpuCore;
            ObjSyncResult = KernelResult.ThreadNotStarted;
            DynamicPriority = priority;
            BasePriority = priority;
            CurrentCore = cpuCore;

            _entrypoint = entrypoint;
            _customThreadStart = customThreadStart;

            if (type == ThreadType.User)
            {
                if (owner.AllocateThreadLocalStorage(out _tlsAddress) != KernelResult.Success)
                {
                    return KernelResult.OutOfMemory;
                }

                TlsDramAddress = owner.MemoryManager.GetDramAddressFromVa(_tlsAddress);

                MemoryHelper.FillWithZeros(owner.CpuMemory, (long)_tlsAddress, KTlsPageInfo.TlsEntrySize);
            }

            bool is64Bits;

            if (owner != null)
            {
                Owner = owner;

                owner.IncrementReferenceCount();
                owner.IncrementThreadCount();

                is64Bits = owner.Flags.HasFlag(ProcessCreationFlags.Is64Bit);
            }
            else
            {
                is64Bits = true;
            }

            HostThread = new Thread(ThreadStart);

            Context = CpuContext.CreateExecutionContext();

            Context.IsAarch32 = !is64Bits;

            Context.SetX(0, argsPtr);

            if (is64Bits)
            {
                Context.SetX(31, stackTop);
            }
            else
            {
                Context.SetX(13, (uint)stackTop);
            }

            Context.CntfrqEl0 = 19200000;
            Context.Tpidr = (long)_tlsAddress;

            ThreadUid = KernelContext.NewThreadUid();

            HostThread.Name = customThreadStart != null ? $"HLE.OsThread.{ThreadUid}" : $"HLE.GuestThread.{ThreadUid}";

            _hasBeenInitialized = true;

            if (owner != null)
            {
                owner.SubscribeThreadEventHandlers(Context);
                owner.AddThread(this);

                if (owner.IsPaused)
                {
                    KernelContext.CriticalSection.Enter();

                    if (TerminationRequested)
                    {
                        KernelContext.CriticalSection.Leave();

                        return KernelResult.Success;
                    }

                    _forcePauseFlags |= ThreadSchedState.ProcessPauseFlag;

                    CombineForcePauseFlags();

                    KernelContext.CriticalSection.Leave();
                }
            }

            return KernelResult.Success;
        }

        public KernelResult Start()
        {
            if (!KernelContext.KernelInitialized)
            {
                KernelContext.CriticalSection.Enter();

                if (!TerminationRequested)
                {
                    _forcePauseFlags |= ThreadSchedState.KernelInitPauseFlag;

                    CombineForcePauseFlags();
                }

                KernelContext.CriticalSection.Leave();
            }

            KernelResult result = KernelResult.ThreadTerminating;

            KernelContext.CriticalSection.Enter();

            if (!ShallBeTerminated)
            {
                KThread currentThread = KernelStatic.GetCurrentThread();

                while (SchedFlags != ThreadSchedState.TerminationPending && (currentThread == null || !currentThread.TerminationRequested))
                {
                    if ((SchedFlags & ThreadSchedState.LowMask) != ThreadSchedState.None)
                    {
                        result = KernelResult.InvalidState;
                        break;
                    }

                    if (currentThread == null || currentThread._forcePauseFlags == ThreadSchedState.None)
                    {
                        if (Owner != null && _forcePauseFlags != ThreadSchedState.None)
                        {
                            CombineForcePauseFlags();
                        }

                        SetNewSchedFlags(ThreadSchedState.Running);

                        StartHostThread();

                        result = KernelResult.Success;
                        break;
                    }
                    else
                    {
                        currentThread.CombineForcePauseFlags();

                        KernelContext.CriticalSection.Leave();
                        KernelContext.CriticalSection.Enter();

                        if (currentThread.ShallBeTerminated)
                        {
                            break;
                        }
                    }
                }
            }

            KernelContext.CriticalSection.Leave();

            return result;
        }

        public ThreadSchedState PrepareForTermination()
        {
            KernelContext.CriticalSection.Enter();

            ThreadSchedState result;

            if (Interlocked.CompareExchange(ref _shallBeTerminated, 1, 0) == 0)
            {
                if ((SchedFlags & ThreadSchedState.LowMask) == ThreadSchedState.None)
                {
                    SchedFlags = ThreadSchedState.TerminationPending;
                }
                else
                {
                    if (_forcePauseFlags != ThreadSchedState.None)
                    {
                        _forcePauseFlags &= ~ThreadSchedState.ThreadPauseFlag;

                        ThreadSchedState oldSchedFlags = SchedFlags;

                        SchedFlags &= ThreadSchedState.LowMask;

                        AdjustScheduling(oldSchedFlags);
                    }

                    if (BasePriority >= 0x10)
                    {
                        SetPriority(0xF);
                    }

                    if ((SchedFlags & ThreadSchedState.LowMask) == ThreadSchedState.Running)
                    {
                        // TODO: GIC distributor stuffs (sgir changes ect)
                        Context.RequestInterrupt();
                    }

                    SignaledObj = null;
                    ObjSyncResult = KernelResult.ThreadTerminating;

                    ReleaseAndResume();
                }
            }

            result = SchedFlags;

            KernelContext.CriticalSection.Leave();

            return result & ThreadSchedState.LowMask;
        }

        public void Terminate()
        {
            ThreadSchedState state = PrepareForTermination();

            if (state != ThreadSchedState.TerminationPending)
            {
                KernelContext.Synchronization.WaitFor(new KSynchronizationObject[] { this }, -1, out _);
            }
        }

        public void HandlePostSyscall()
        {
            ThreadSchedState state;

            do
            {
                if (TerminationRequested)
                {
                    Exit();

                    // As the death of the thread is handled by the CPU emulator, we differ from the official kernel and return here.
                    break;
                }

                KernelContext.CriticalSection.Enter();

                if (TerminationRequested)
                {
                    state = ThreadSchedState.TerminationPending;
                }
                else
                {
                    if (_forcePauseFlags != ThreadSchedState.None)
                    {
                        CombineForcePauseFlags();
                    }

                    state = ThreadSchedState.Running;
                }

                KernelContext.CriticalSection.Leave();
            } while (state == ThreadSchedState.TerminationPending);
        }

        public void Exit()
        {
            // TODO: Debug event.

            if (Owner != null)
            {
                Owner.ResourceLimit?.Release(LimitableResource.Thread, 0, 1);

                _hasBeenReleased = true;
            }

            KernelContext.CriticalSection.Enter();

            _forcePauseFlags &= ~ThreadSchedState.ForcePauseMask;

            bool decRef = ExitImpl();

            Context.StopRunning();

            KernelContext.CriticalSection.Leave();

            if (decRef)
            {
                DecrementReferenceCount();
            }
        }

        private bool ExitImpl()
        {
            KernelContext.CriticalSection.Enter();

            SetNewSchedFlags(ThreadSchedState.TerminationPending);

            bool decRef = Interlocked.Exchange(ref _hasExited, 1) == 0;

            Signal();

            KernelContext.CriticalSection.Leave();

            return decRef;
        }

        public KernelResult Sleep(long timeout)
        {
            KernelContext.CriticalSection.Enter();

            if (ShallBeTerminated || SchedFlags == ThreadSchedState.TerminationPending)
            {
                KernelContext.CriticalSection.Leave();

                return KernelResult.ThreadTerminating;
            }

            SetNewSchedFlags(ThreadSchedState.Paused);

            if (timeout > 0)
            {
                KernelContext.TimeManager.ScheduleFutureInvocation(this, timeout);
            }

            KernelContext.CriticalSection.Leave();

            if (timeout > 0)
            {
                KernelContext.TimeManager.UnscheduleFutureInvocation(this);
            }

            return 0;
        }

        public void SetPriority(int priority)
        {
            KernelContext.CriticalSection.Enter();

            BasePriority = priority;

            UpdatePriorityInheritance();

            KernelContext.CriticalSection.Leave();
        }

        public KernelResult SetActivity(bool pause)
        {
            KernelResult result = KernelResult.Success;

            KernelContext.CriticalSection.Enter();

            ThreadSchedState lowNibble = SchedFlags & ThreadSchedState.LowMask;

            if (lowNibble != ThreadSchedState.Paused && lowNibble != ThreadSchedState.Running)
            {
                KernelContext.CriticalSection.Leave();

                return KernelResult.InvalidState;
            }

            KernelContext.CriticalSection.Enter();

            if (!ShallBeTerminated && SchedFlags != ThreadSchedState.TerminationPending)
            {
                if (pause)
                {
                    // Pause, the force pause flag should be clear (thread is NOT paused).
                    if ((_forcePauseFlags & ThreadSchedState.ThreadPauseFlag) == 0)
                    {
                        _forcePauseFlags |= ThreadSchedState.ThreadPauseFlag;

                        CombineForcePauseFlags();
                    }
                    else
                    {
                        result = KernelResult.InvalidState;
                    }
                }
                else
                {
                    // Unpause, the force pause flag should be set (thread is paused).
                    if ((_forcePauseFlags & ThreadSchedState.ThreadPauseFlag) != 0)
                    {
                        ThreadSchedState oldForcePauseFlags = _forcePauseFlags;

                        _forcePauseFlags &= ~ThreadSchedState.ThreadPauseFlag;

                        if ((oldForcePauseFlags & ~ThreadSchedState.ThreadPauseFlag) == ThreadSchedState.None)
                        {
                            ThreadSchedState oldSchedFlags = SchedFlags;

                            SchedFlags &= ThreadSchedState.LowMask;

                            AdjustScheduling(oldSchedFlags);
                        }
                    }
                    else
                    {
                        result = KernelResult.InvalidState;
                    }
                }
            }

            KernelContext.CriticalSection.Leave();
            KernelContext.CriticalSection.Leave();

            return result;
        }

        public void CancelSynchronization()
        {
            KernelContext.CriticalSection.Enter();

            if ((SchedFlags & ThreadSchedState.LowMask) != ThreadSchedState.Paused || !WaitingSync)
            {
                SyncCancelled = true;
            }
            else if (Withholder != null)
            {
                Withholder.Remove(WithholderNode);

                SetNewSchedFlags(ThreadSchedState.Running);

                Withholder = null;

                SyncCancelled = true;
            }
            else
            {
                SignaledObj = null;
                ObjSyncResult = KernelResult.Cancelled;

                SetNewSchedFlags(ThreadSchedState.Running);

                SyncCancelled = false;
            }

            KernelContext.CriticalSection.Leave();
        }

        public KernelResult SetCoreAndAffinityMask(int newCore, long newAffinityMask)
        {
            KernelContext.CriticalSection.Enter();

            bool useOverride = _affinityOverrideCount != 0;

            // The value -3 is "do not change the preferred core".
            if (newCore == -3)
            {
                newCore = useOverride ? _preferredCoreOverride : PreferredCore;

                if ((newAffinityMask & (1 << newCore)) == 0)
                {
                    KernelContext.CriticalSection.Leave();

                    return KernelResult.InvalidCombination;
                }
            }

            if (useOverride)
            {
                _preferredCoreOverride = newCore;
                _affinityMaskOverride = newAffinityMask;
            }
            else
            {
                long oldAffinityMask = AffinityMask;

                PreferredCore = newCore;
                AffinityMask = newAffinityMask;

                if (oldAffinityMask != newAffinityMask)
                {
                    int oldCore = ActiveCore;

                    if (oldCore >= 0 && ((AffinityMask >> oldCore) & 1) == 0)
                    {
                        if (PreferredCore < 0)
                        {
                            ActiveCore = sizeof(ulong) * 8 - 1 - BitOperations.LeadingZeroCount((ulong)AffinityMask);
                        }
                        else
                        {
                            ActiveCore = PreferredCore;
                        }
                    }

                    AdjustSchedulingForNewAffinity(oldAffinityMask, oldCore);
                }
            }

            KernelContext.CriticalSection.Leave();

            return KernelResult.Success;
        }

        private void CombineForcePauseFlags()
        {
            ThreadSchedState oldFlags = SchedFlags;
            ThreadSchedState lowNibble = SchedFlags & ThreadSchedState.LowMask;

            SchedFlags = lowNibble | _forcePauseFlags;

            AdjustScheduling(oldFlags);
        }

        private void SetNewSchedFlags(ThreadSchedState newFlags)
        {
            KernelContext.CriticalSection.Enter();

            ThreadSchedState oldFlags = SchedFlags;

            SchedFlags = (oldFlags & ThreadSchedState.HighMask) | newFlags;

            if ((oldFlags & ThreadSchedState.LowMask) != newFlags)
            {
                AdjustScheduling(oldFlags);
            }

            KernelContext.CriticalSection.Leave();
        }

        public void ReleaseAndResume()
        {
            KernelContext.CriticalSection.Enter();

            if ((SchedFlags & ThreadSchedState.LowMask) == ThreadSchedState.Paused)
            {
                if (Withholder != null)
                {
                    Withholder.Remove(WithholderNode);

                    SetNewSchedFlags(ThreadSchedState.Running);

                    Withholder = null;
                }
                else
                {
                    SetNewSchedFlags(ThreadSchedState.Running);
                }
            }

            KernelContext.CriticalSection.Leave();
        }

        public void Reschedule(ThreadSchedState newFlags)
        {
            KernelContext.CriticalSection.Enter();

            ThreadSchedState oldFlags = SchedFlags;

            SchedFlags = (oldFlags & ThreadSchedState.HighMask) |
                         (newFlags & ThreadSchedState.LowMask);

            AdjustScheduling(oldFlags);

            KernelContext.CriticalSection.Leave();
        }

        public void AddMutexWaiter(KThread requester)
        {
            AddToMutexWaitersList(requester);

            requester.MutexOwner = this;

            UpdatePriorityInheritance();
        }

        public void RemoveMutexWaiter(KThread thread)
        {
            if (thread._mutexWaiterNode?.List != null)
            {
                _mutexWaiters.Remove(thread._mutexWaiterNode);
            }

            thread.MutexOwner = null;

            UpdatePriorityInheritance();
        }

        public KThread RelinquishMutex(ulong mutexAddress, out int count)
        {
            count = 0;

            if (_mutexWaiters.First == null)
            {
                return null;
            }

            KThread newMutexOwner = null;

            LinkedListNode<KThread> currentNode = _mutexWaiters.First;

            do
            {
                // Skip all threads that are not waiting for this mutex.
                while (currentNode != null && currentNode.Value.MutexAddress != mutexAddress)
                {
                    currentNode = currentNode.Next;
                }

                if (currentNode == null)
                {
                    break;
                }

                LinkedListNode<KThread> nextNode = currentNode.Next;

                _mutexWaiters.Remove(currentNode);

                currentNode.Value.MutexOwner = newMutexOwner;

                if (newMutexOwner != null)
                {
                    // New owner was already selected, re-insert on new owner list.
                    newMutexOwner.AddToMutexWaitersList(currentNode.Value);
                }
                else
                {
                    // New owner not selected yet, use current thread.
                    newMutexOwner = currentNode.Value;
                }

                count++;

                currentNode = nextNode;
            }
            while (currentNode != null);

            if (newMutexOwner != null)
            {
                UpdatePriorityInheritance();

                newMutexOwner.UpdatePriorityInheritance();
            }

            return newMutexOwner;
        }

        private void UpdatePriorityInheritance()
        {
            // If any of the threads waiting for the mutex has
            // higher priority than the current thread, then
            // the current thread inherits that priority.
            int highestPriority = BasePriority;

            if (_mutexWaiters.First != null)
            {
                int waitingDynamicPriority = _mutexWaiters.First.Value.DynamicPriority;

                if (waitingDynamicPriority < highestPriority)
                {
                    highestPriority = waitingDynamicPriority;
                }
            }

            if (highestPriority != DynamicPriority)
            {
                int oldPriority = DynamicPriority;

                DynamicPriority = highestPriority;

                AdjustSchedulingForNewPriority(oldPriority);

                if (MutexOwner != null)
                {
                    // Remove and re-insert to ensure proper sorting based on new priority.
                    MutexOwner._mutexWaiters.Remove(_mutexWaiterNode);

                    MutexOwner.AddToMutexWaitersList(this);

                    MutexOwner.UpdatePriorityInheritance();
                }
            }
        }

        private void AddToMutexWaitersList(KThread thread)
        {
            LinkedListNode<KThread> nextPrio = _mutexWaiters.First;

            int currentPriority = thread.DynamicPriority;

            while (nextPrio != null && nextPrio.Value.DynamicPriority <= currentPriority)
            {
                nextPrio = nextPrio.Next;
            }

            if (nextPrio != null)
            {
                thread._mutexWaiterNode = _mutexWaiters.AddBefore(nextPrio, thread);
            }
            else
            {
                thread._mutexWaiterNode = _mutexWaiters.AddLast(thread);
            }
        }

        private void AdjustScheduling(ThreadSchedState oldFlags)
        {
            if (oldFlags == SchedFlags)
            {
                return;
            }

            if (!IsSchedulable)
            {
                // Ensure our thread is running and we have an event.
                StartHostThread();

                // If the thread is not schedulable, we want to just run or pause
                // it directly as we don't care about priority or the core it is
                // running on in this case.
                if (SchedFlags == ThreadSchedState.Running)
                {
                    _schedulerWaitEvent.Set();
                }
                else
                {
                    _schedulerWaitEvent.Reset();
                }

                return;
            }

            if (oldFlags == ThreadSchedState.Running)
            {
                // Was running, now it's stopped.
                if (ActiveCore >= 0)
                {
                    KernelContext.PriorityQueue.Unschedule(DynamicPriority, ActiveCore, this);
                }

                for (int core = 0; core < KScheduler.CpuCoresCount; core++)
                {
                    if (core != ActiveCore && ((AffinityMask >> core) & 1) != 0)
                    {
                        KernelContext.PriorityQueue.Unsuggest(DynamicPriority, core, this);
                    }
                }
            }
            else if (SchedFlags == ThreadSchedState.Running)
            {
                // Was stopped, now it's running.
                if (ActiveCore >= 0)
                {
                    KernelContext.PriorityQueue.Schedule(DynamicPriority, ActiveCore, this);
                }

                for (int core = 0; core < KScheduler.CpuCoresCount; core++)
                {
                    if (core != ActiveCore && ((AffinityMask >> core) & 1) != 0)
                    {
                        KernelContext.PriorityQueue.Suggest(DynamicPriority, core, this);
                    }
                }
            }

            KernelContext.ThreadReselectionRequested = true;
        }

        private void AdjustSchedulingForNewPriority(int oldPriority)
        {
            if (SchedFlags != ThreadSchedState.Running || !IsSchedulable)
            {
                return;
            }

            // Remove thread from the old priority queues.
            if (ActiveCore >= 0)
            {
                KernelContext.PriorityQueue.Unschedule(oldPriority, ActiveCore, this);
            }

            for (int core = 0; core < KScheduler.CpuCoresCount; core++)
            {
                if (core != ActiveCore && ((AffinityMask >> core) & 1) != 0)
                {
                    KernelContext.PriorityQueue.Unsuggest(oldPriority, core, this);
                }
            }

            // Add thread to the new priority queues.
            KThread currentThread = KernelStatic.GetCurrentThread();

            if (ActiveCore >= 0)
            {
                if (currentThread == this)
                {
                    KernelContext.PriorityQueue.SchedulePrepend(DynamicPriority, ActiveCore, this);
                }
                else
                {
                    KernelContext.PriorityQueue.Schedule(DynamicPriority, ActiveCore, this);
                }
            }

            for (int core = 0; core < KScheduler.CpuCoresCount; core++)
            {
                if (core != ActiveCore && ((AffinityMask >> core) & 1) != 0)
                {
                    KernelContext.PriorityQueue.Suggest(DynamicPriority, core, this);
                }
            }

            KernelContext.ThreadReselectionRequested = true;
        }

        private void AdjustSchedulingForNewAffinity(long oldAffinityMask, int oldCore)
        {
            if (SchedFlags != ThreadSchedState.Running || DynamicPriority >= KScheduler.PrioritiesCount || !IsSchedulable)
            {
                return;
            }

            // Remove thread from the old priority queues.
            for (int core = 0; core < KScheduler.CpuCoresCount; core++)
            {
                if (((oldAffinityMask >> core) & 1) != 0)
                {
                    if (core == oldCore)
                    {
                        KernelContext.PriorityQueue.Unschedule(DynamicPriority, core, this);
                    }
                    else
                    {
                        KernelContext.PriorityQueue.Unsuggest(DynamicPriority, core, this);
                    }
                }
            }

            // Add thread to the new priority queues.
            for (int core = 0; core < KScheduler.CpuCoresCount; core++)
            {
                if (((AffinityMask >> core) & 1) != 0)
                {
                    if (core == ActiveCore)
                    {
                        KernelContext.PriorityQueue.Schedule(DynamicPriority, core, this);
                    }
                    else
                    {
                        KernelContext.PriorityQueue.Suggest(DynamicPriority, core, this);
                    }
                }
            }

            KernelContext.ThreadReselectionRequested = true;
        }

        public void SetEntryArguments(long argsPtr, int threadHandle)
        {
            Context.SetX(0, (ulong)argsPtr);
            Context.SetX(1, (ulong)threadHandle);
        }

        public void TimeUp()
        {
            ReleaseAndResume();
        }

        public string GetGuestStackTrace()
        {
            return Owner.Debugger.GetGuestStackTrace(Context);
        }

        public void PrintGuestStackTrace()
        {
            Logger.Info?.Print(LogClass.Cpu, $"Guest stack trace:\n{GetGuestStackTrace()}\n");
        }

        public void AddCpuTime(long ticks)
        {
            Interlocked.Add(ref _totalTimeRunning, ticks);
        }

        public void StartHostThread()
        {
            if (_schedulerWaitEvent == null)
            {
                var schedulerWaitEvent = new ManualResetEvent(false);

                if (Interlocked.Exchange(ref _schedulerWaitEvent, schedulerWaitEvent) == null)
                {
                    HostThread.Start();
                }
                else
                {
                    schedulerWaitEvent.Dispose();
                }
            }
        }

        private void ThreadStart()
        {
            _schedulerWaitEvent.WaitOne();
            KernelStatic.SetKernelContext(KernelContext, this);

            if (_customThreadStart != null)
            {
                _customThreadStart();
            }
            else
            {
                Owner.Context.Execute(Context, _entrypoint);
            }

            Context.Dispose();
            _schedulerWaitEvent.Dispose();
        }

        public void MakeUnschedulable()
        {
            _forcedUnschedulable = true;
        }

        public override bool IsSignaled()
        {
            return _hasExited != 0;
        }

        protected override void Destroy()
        {
            if (_hasBeenInitialized)
            {
                FreeResources();

                bool released = Owner != null || _hasBeenReleased;

                if (Owner != null)
                {
                    Owner.ResourceLimit?.Release(LimitableResource.Thread, 1, released ? 0 : 1);

                    Owner.DecrementReferenceCount();
                }
                else
                {
                    KernelContext.ResourceLimit.Release(LimitableResource.Thread, 1, released ? 0 : 1);
                }
            }
        }

        private void FreeResources()
        {
            Owner?.RemoveThread(this);

            if (_tlsAddress != 0 && Owner.FreeThreadLocalStorage(_tlsAddress) != KernelResult.Success)
            {
                throw new InvalidOperationException("Unexpected failure freeing thread local storage.");
            }

            KernelContext.CriticalSection.Enter();

            // Wake up all threads that may be waiting for a mutex being held by this thread.
            foreach (KThread thread in _mutexWaiters)
            {
                thread.MutexOwner = null;
                thread._preferredCoreOverride = 0;
                thread.ObjSyncResult = KernelResult.InvalidState;

                thread.ReleaseAndResume();
            }

            KernelContext.CriticalSection.Leave();

            Owner?.DecrementThreadCountAndTerminateIfZero();
        }
    }
}