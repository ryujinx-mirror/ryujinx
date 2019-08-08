using ARMeilleure.Memory;
using ARMeilleure.State;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KThread : KSynchronizationObject, IKFutureSchedulerObject
    {
        private int _hostThreadRunning;

        public Thread HostThread { get; private set; }

        public IExecutionContext Context { get; private set; }

        public long AffinityMask { get; set; }

        public long ThreadUid { get; private set; }

        public long TotalTimeRunning { get; set; }

        public KSynchronizationObject SignaledObj { get; set; }

        public ulong CondVarAddress { get; set; }

        private ulong _entrypoint;

        public ulong MutexAddress { get; set; }

        public KProcess Owner { get; private set; }

        private ulong _tlsAddress;

        public ulong TlsAddress => _tlsAddress;
        public ulong TlsDramAddress { get; private set; }

        public long LastScheduledTime { get; set; }

        public LinkedListNode<KThread>[] SiblingsPerCore { get; private set; }

        public LinkedList<KThread>     Withholder     { get; set; }
        public LinkedListNode<KThread> WithholderNode { get; set; }

        public LinkedListNode<KThread> ProcessListNode { get; set; }

        private LinkedList<KThread>     _mutexWaiters;
        private LinkedListNode<KThread> _mutexWaiterNode;

        public KThread MutexOwner { get; private set; }

        public int ThreadHandleForUserMutex { get; set; }

        private ThreadSchedState _forcePauseFlags;

        public KernelResult ObjSyncResult { get; set; }

        public int DynamicPriority { get; set; }
        public int CurrentCore     { get; set; }
        public int BasePriority    { get; set; }
        public int PreferredCore   { get; set; }

        private long _affinityMaskOverride;
        private int  _preferredCoreOverride;
        private int  _affinityOverrideCount;

        public ThreadSchedState SchedFlags { get; private set; }

        public bool ShallBeTerminated { get; private set; }

        public bool SyncCancelled { get; set; }
        public bool WaitingSync   { get; set; }

        private bool _hasExited;
        private bool _hasBeenInitialized;
        private bool _hasBeenReleased;

        public bool WaitingInArbitration { get; set; }

        private KScheduler _scheduler;

        private KSchedulingData _schedulingData;

        public long LastPc { get; set; }

        public KThread(Horizon system) : base(system)
        {
            _scheduler      = system.Scheduler;
            _schedulingData = system.Scheduler.SchedulingData;

            SiblingsPerCore = new LinkedListNode<KThread>[KScheduler.CpuCoresCount];

            _mutexWaiters = new LinkedList<KThread>();
        }

        public KernelResult Initialize(
            ulong      entrypoint,
            ulong      argsPtr,
            ulong      stackTop,
            int        priority,
            int        defaultCpuCore,
            KProcess   owner,
            ThreadType type = ThreadType.User)
        {
            if ((uint)type > 3)
            {
                throw new ArgumentException($"Invalid thread type \"{type}\".");
            }

            PreferredCore = defaultCpuCore;

            AffinityMask |= 1L << defaultCpuCore;

            SchedFlags = type == ThreadType.Dummy
                ? ThreadSchedState.Running
                : ThreadSchedState.None;

            CurrentCore = PreferredCore;

            DynamicPriority = priority;
            BasePriority    = priority;

            ObjSyncResult = KernelResult.ThreadNotStarted;

            _entrypoint = entrypoint;

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

                is64Bits = (owner.MmuFlags & 1) != 0;
            }
            else
            {
                is64Bits = true;
            }

            HostThread = new Thread(() => ThreadStart(entrypoint));

            if (System.UseLegacyJit)
            {
                Context = new ChocolArm64.State.CpuThreadState();
            }
            else
            {
                Context = new ARMeilleure.State.ExecutionContext();
            }

            bool isAarch32 = (Owner.MmuFlags & 1) == 0;

            Context.SetX(0, argsPtr);

            if (isAarch32)
            {
                Context.SetX(13, (uint)stackTop);
            }
            else
            {
                Context.SetX(31, stackTop);
            }

            Context.CntfrqEl0 = 19200000;
            Context.Tpidr     = (long)_tlsAddress;

            owner.SubscribeThreadEventHandlers(Context);

            ThreadUid = System.GetThreadUid();

            _hasBeenInitialized = true;

            if (owner != null)
            {
                owner.AddThread(this);

                if (owner.IsPaused)
                {
                    System.CriticalSection.Enter();

                    if (ShallBeTerminated || SchedFlags == ThreadSchedState.TerminationPending)
                    {
                        System.CriticalSection.Leave();

                        return KernelResult.Success;
                    }

                    _forcePauseFlags |= ThreadSchedState.ProcessPauseFlag;

                    CombineForcePauseFlags();

                    System.CriticalSection.Leave();
                }
            }

            return KernelResult.Success;
        }

        public KernelResult Start()
        {
            if (!System.KernelInitialized)
            {
                System.CriticalSection.Enter();

                if (!ShallBeTerminated && SchedFlags != ThreadSchedState.TerminationPending)
                {
                    _forcePauseFlags |= ThreadSchedState.KernelInitPauseFlag;

                    CombineForcePauseFlags();
                }

                System.CriticalSection.Leave();
            }

            KernelResult result = KernelResult.ThreadTerminating;

            System.CriticalSection.Enter();

            if (!ShallBeTerminated)
            {
                KThread currentThread = System.Scheduler.GetCurrentThread();

                while (SchedFlags               != ThreadSchedState.TerminationPending &&
                       currentThread.SchedFlags != ThreadSchedState.TerminationPending &&
                       !currentThread.ShallBeTerminated)
                {
                    if ((SchedFlags & ThreadSchedState.LowMask) != ThreadSchedState.None)
                    {
                        result = KernelResult.InvalidState;

                        break;
                    }

                    if (currentThread._forcePauseFlags == ThreadSchedState.None)
                    {
                        if (Owner != null && _forcePauseFlags != ThreadSchedState.None)
                        {
                            CombineForcePauseFlags();
                        }

                        SetNewSchedFlags(ThreadSchedState.Running);

                        result = KernelResult.Success;

                        break;
                    }
                    else
                    {
                        currentThread.CombineForcePauseFlags();

                        System.CriticalSection.Leave();
                        System.CriticalSection.Enter();

                        if (currentThread.ShallBeTerminated)
                        {
                            break;
                        }
                    }
                }
            }

            System.CriticalSection.Leave();

            return result;
        }

        public void Exit()
        {
            // TODO: Debug event.

            if (Owner != null)
            {
                Owner.ResourceLimit?.Release(LimitableResource.Thread, 0, 1);

                _hasBeenReleased = true;
            }

            System.CriticalSection.Enter();

            _forcePauseFlags &= ~ThreadSchedState.ForcePauseMask;

            ExitImpl();

            System.CriticalSection.Leave();

            DecrementReferenceCount();
        }

        private void ExitImpl()
        {
            System.CriticalSection.Enter();

            SetNewSchedFlags(ThreadSchedState.TerminationPending);

            _hasExited = true;

            Signal();

            System.CriticalSection.Leave();
        }

        public KernelResult Sleep(long timeout)
        {
            System.CriticalSection.Enter();

            if (ShallBeTerminated || SchedFlags == ThreadSchedState.TerminationPending)
            {
                System.CriticalSection.Leave();

                return KernelResult.ThreadTerminating;
            }

            SetNewSchedFlags(ThreadSchedState.Paused);

            if (timeout > 0)
            {
                System.TimeManager.ScheduleFutureInvocation(this, timeout);
            }

            System.CriticalSection.Leave();

            if (timeout > 0)
            {
                System.TimeManager.UnscheduleFutureInvocation(this);
            }

            return 0;
        }

        public void Yield()
        {
            System.CriticalSection.Enter();

            if (SchedFlags != ThreadSchedState.Running)
            {
                System.CriticalSection.Leave();

                System.Scheduler.ContextSwitch();

                return;
            }

            if (DynamicPriority < KScheduler.PrioritiesCount)
            {
                // Move current thread to the end of the queue.
                _schedulingData.Reschedule(DynamicPriority, CurrentCore, this);
            }

            _scheduler.ThreadReselectionRequested = true;

            System.CriticalSection.Leave();

            System.Scheduler.ContextSwitch();
        }

        public void YieldWithLoadBalancing()
        {
            System.CriticalSection.Enter();

            if (SchedFlags != ThreadSchedState.Running)
            {
                System.CriticalSection.Leave();

                System.Scheduler.ContextSwitch();

                return;
            }

            int prio = DynamicPriority;
            int core = CurrentCore;

            KThread nextThreadOnCurrentQueue = null;

            if (DynamicPriority < KScheduler.PrioritiesCount)
            {
                // Move current thread to the end of the queue.
                _schedulingData.Reschedule(prio, core, this);

                Func<KThread, bool> predicate = x => x.DynamicPriority == prio;

                nextThreadOnCurrentQueue = _schedulingData.ScheduledThreads(core).FirstOrDefault(predicate);
            }

            IEnumerable<KThread> SuitableCandidates()
            {
                foreach (KThread thread in _schedulingData.SuggestedThreads(core))
                {
                    int srcCore = thread.CurrentCore;

                    if (srcCore >= 0)
                    {
                        KThread selectedSrcCore = _scheduler.CoreContexts[srcCore].SelectedThread;

                        if (selectedSrcCore == thread || ((selectedSrcCore?.DynamicPriority ?? 2) < 2))
                        {
                            continue;
                        }
                    }

                    // If the candidate was scheduled after the current thread, then it's not worth it,
                    // unless the priority is higher than the current one.
                    if (nextThreadOnCurrentQueue.LastScheduledTime >= thread.LastScheduledTime ||
                        nextThreadOnCurrentQueue.DynamicPriority    <  thread.DynamicPriority)
                    {
                        yield return thread;
                    }
                }
            }

            KThread dst = SuitableCandidates().FirstOrDefault(x => x.DynamicPriority <= prio);

            if (dst != null)
            {
                _schedulingData.TransferToCore(dst.DynamicPriority, core, dst);

                _scheduler.ThreadReselectionRequested = true;
            }

            if (this != nextThreadOnCurrentQueue)
            {
                _scheduler.ThreadReselectionRequested = true;
            }

            System.CriticalSection.Leave();

            System.Scheduler.ContextSwitch();
        }

        public void YieldAndWaitForLoadBalancing()
        {
            System.CriticalSection.Enter();

            if (SchedFlags != ThreadSchedState.Running)
            {
                System.CriticalSection.Leave();

                System.Scheduler.ContextSwitch();

                return;
            }

            int core = CurrentCore;

            _schedulingData.TransferToCore(DynamicPriority, -1, this);

            KThread selectedThread = null;

            if (!_schedulingData.ScheduledThreads(core).Any())
            {
                foreach (KThread thread in _schedulingData.SuggestedThreads(core))
                {
                    if (thread.CurrentCore < 0)
                    {
                        continue;
                    }

                    KThread firstCandidate = _schedulingData.ScheduledThreads(thread.CurrentCore).FirstOrDefault();

                    if (firstCandidate == thread)
                    {
                        continue;
                    }

                    if (firstCandidate == null || firstCandidate.DynamicPriority >= 2)
                    {
                        _schedulingData.TransferToCore(thread.DynamicPriority, core, thread);

                        selectedThread = thread;
                    }

                    break;
                }
            }

            if (selectedThread != this)
            {
                _scheduler.ThreadReselectionRequested = true;
            }

            System.CriticalSection.Leave();

            System.Scheduler.ContextSwitch();
        }

        public void SetPriority(int priority)
        {
            System.CriticalSection.Enter();

            BasePriority = priority;

            UpdatePriorityInheritance();

            System.CriticalSection.Leave();
        }

        public KernelResult SetActivity(bool pause)
        {
            KernelResult result = KernelResult.Success;

            System.CriticalSection.Enter();

            ThreadSchedState lowNibble = SchedFlags & ThreadSchedState.LowMask;

            if (lowNibble != ThreadSchedState.Paused && lowNibble != ThreadSchedState.Running)
            {
                System.CriticalSection.Leave();

                return KernelResult.InvalidState;
            }

            System.CriticalSection.Enter();

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

            System.CriticalSection.Leave();
            System.CriticalSection.Leave();

            return result;
        }

        public void CancelSynchronization()
        {
            System.CriticalSection.Enter();

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
                SignaledObj   = null;
                ObjSyncResult = KernelResult.Cancelled;

                SetNewSchedFlags(ThreadSchedState.Running);

                SyncCancelled = false;
            }

            System.CriticalSection.Leave();
        }

        public KernelResult SetCoreAndAffinityMask(int newCore, long newAffinityMask)
        {
            System.CriticalSection.Enter();

            bool useOverride = _affinityOverrideCount != 0;

            // The value -3 is "do not change the preferred core".
            if (newCore == -3)
            {
                newCore = useOverride ? _preferredCoreOverride : PreferredCore;

                if ((newAffinityMask & (1 << newCore)) == 0)
                {
                    System.CriticalSection.Leave();

                    return KernelResult.InvalidCombination;
                }
            }

            if (useOverride)
            {
                _preferredCoreOverride = newCore;
                _affinityMaskOverride  = newAffinityMask;
            }
            else
            {
                long oldAffinityMask = AffinityMask;

                PreferredCore = newCore;
                AffinityMask  = newAffinityMask;

                if (oldAffinityMask != newAffinityMask)
                {
                    int oldCore = CurrentCore;

                    if (CurrentCore >= 0 && ((AffinityMask >> CurrentCore) & 1) == 0)
                    {
                        if (PreferredCore < 0)
                        {
                            CurrentCore = HighestSetCore(AffinityMask);
                        }
                        else
                        {
                            CurrentCore = PreferredCore;
                        }
                    }

                    AdjustSchedulingForNewAffinity(oldAffinityMask, oldCore);
                }
            }

            System.CriticalSection.Leave();

            return KernelResult.Success;
        }

        private static int HighestSetCore(long mask)
        {
            for (int core = KScheduler.CpuCoresCount - 1; core >= 0; core--)
            {
                if (((mask >> core) & 1) != 0)
                {
                    return core;
                }
            }

            return -1;
        }

        private void CombineForcePauseFlags()
        {
            ThreadSchedState oldFlags  = SchedFlags;
            ThreadSchedState lowNibble = SchedFlags & ThreadSchedState.LowMask;

            SchedFlags = lowNibble | _forcePauseFlags;

            AdjustScheduling(oldFlags);
        }

        private void SetNewSchedFlags(ThreadSchedState newFlags)
        {
            System.CriticalSection.Enter();

            ThreadSchedState oldFlags = SchedFlags;

            SchedFlags = (oldFlags & ThreadSchedState.HighMask) | newFlags;

            if ((oldFlags & ThreadSchedState.LowMask) != newFlags)
            {
                AdjustScheduling(oldFlags);
            }

            System.CriticalSection.Leave();
        }

        public void ReleaseAndResume()
        {
            System.CriticalSection.Enter();

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

            System.CriticalSection.Leave();
        }

        public void Reschedule(ThreadSchedState newFlags)
        {
            System.CriticalSection.Enter();

            ThreadSchedState oldFlags = SchedFlags;

            SchedFlags = (oldFlags & ThreadSchedState.HighMask) |
                         (newFlags & ThreadSchedState.LowMask);

            AdjustScheduling(oldFlags);

            System.CriticalSection.Leave();
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

            if (oldFlags == ThreadSchedState.Running)
            {
                // Was running, now it's stopped.
                if (CurrentCore >= 0)
                {
                    _schedulingData.Unschedule(DynamicPriority, CurrentCore, this);
                }

                for (int core = 0; core < KScheduler.CpuCoresCount; core++)
                {
                    if (core != CurrentCore && ((AffinityMask >> core) & 1) != 0)
                    {
                        _schedulingData.Unsuggest(DynamicPriority, core, this);
                    }
                }
            }
            else if (SchedFlags == ThreadSchedState.Running)
            {
                // Was stopped, now it's running.
                if (CurrentCore >= 0)
                {
                    _schedulingData.Schedule(DynamicPriority, CurrentCore, this);
                }

                for (int core = 0; core < KScheduler.CpuCoresCount; core++)
                {
                    if (core != CurrentCore && ((AffinityMask >> core) & 1) != 0)
                    {
                        _schedulingData.Suggest(DynamicPriority, core, this);
                    }
                }
            }

            _scheduler.ThreadReselectionRequested = true;
        }

        private void AdjustSchedulingForNewPriority(int oldPriority)
        {
            if (SchedFlags != ThreadSchedState.Running)
            {
                return;
            }

            // Remove thread from the old priority queues.
            if (CurrentCore >= 0)
            {
                _schedulingData.Unschedule(oldPriority, CurrentCore, this);
            }

            for (int core = 0; core < KScheduler.CpuCoresCount; core++)
            {
                if (core != CurrentCore && ((AffinityMask >> core) & 1) != 0)
                {
                    _schedulingData.Unsuggest(oldPriority, core, this);
                }
            }

            // Add thread to the new priority queues.
            KThread currentThread = _scheduler.GetCurrentThread();

            if (CurrentCore >= 0)
            {
                if (currentThread == this)
                {
                    _schedulingData.SchedulePrepend(DynamicPriority, CurrentCore, this);
                }
                else
                {
                    _schedulingData.Schedule(DynamicPriority, CurrentCore, this);
                }
            }

            for (int core = 0; core < KScheduler.CpuCoresCount; core++)
            {
                if (core != CurrentCore && ((AffinityMask >> core) & 1) != 0)
                {
                    _schedulingData.Suggest(DynamicPriority, core, this);
                }
            }

            _scheduler.ThreadReselectionRequested = true;
        }

        private void AdjustSchedulingForNewAffinity(long oldAffinityMask, int oldCore)
        {
            if (SchedFlags != ThreadSchedState.Running || DynamicPriority >= KScheduler.PrioritiesCount)
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
                        _schedulingData.Unschedule(DynamicPriority, core, this);
                    }
                    else
                    {
                        _schedulingData.Unsuggest(DynamicPriority, core, this);
                    }
                }
            }

            // Add thread to the new priority queues.
            for (int core = 0; core < KScheduler.CpuCoresCount; core++)
            {
                if (((AffinityMask >> core) & 1) != 0)
                {
                    if (core == CurrentCore)
                    {
                        _schedulingData.Schedule(DynamicPriority, core, this);
                    }
                    else
                    {
                        _schedulingData.Suggest(DynamicPriority, core, this);
                    }
                }
            }

            _scheduler.ThreadReselectionRequested = true;
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
            StringBuilder trace = new StringBuilder();

            trace.AppendLine("Guest stack trace:");
            trace.AppendLine(GetGuestStackTrace());

            Logger.PrintInfo(LogClass.Cpu, trace.ToString());
        }

        public void Execute()
        {
            if (Interlocked.CompareExchange(ref _hostThreadRunning, 1, 0) == 0)
            {
                HostThread.Start();
            }
        }

        private void ThreadStart(ulong entrypoint)
        {
            Owner.Translator.Execute(Context, entrypoint);

            ThreadExit();
        }

        private void ThreadExit()
        {
            System.Scheduler.ExitThread(this);
            System.Scheduler.RemoveThread(this);
        }

        public bool IsCurrentHostThread()
        {
            return Thread.CurrentThread == HostThread;
        }

        public override bool IsSignaled()
        {
            return _hasExited;
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
                    System.ResourceLimit.Release(LimitableResource.Thread, 1, released ? 0 : 1);
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

            System.CriticalSection.Enter();

            // Wake up all threads that may be waiting for a mutex being held by this thread.
            foreach (KThread thread in _mutexWaiters)
            {
                thread.MutexOwner             = null;
                thread._preferredCoreOverride = 0;
                thread.ObjSyncResult          = KernelResult.InvalidState;

                thread.ReleaseAndResume();
            }

            System.CriticalSection.Leave();

            Owner?.DecrementThreadCountAndTerminateIfZero();
        }
    }
}