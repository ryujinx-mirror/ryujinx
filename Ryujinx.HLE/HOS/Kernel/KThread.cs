using ChocolArm64;
using ChocolArm64.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KThread : KSynchronizationObject, IKFutureSchedulerObject
    {
        public CpuThread Context { get; private set; }

        public long AffinityMask { get; set; }

        public long ThreadUid { get; private set; }

        public long TotalTimeRunning { get; set; }

        public KSynchronizationObject SignaledObj { get; set; }

        public long CondVarAddress { get; set; }

        private ulong Entrypoint;

        public long MutexAddress { get; set; }

        public KProcess Owner { get; private set; }

        private ulong TlsAddress;

        public long LastScheduledTime { get; set; }

        public LinkedListNode<KThread>[] SiblingsPerCore { get; private set; }

        public LinkedList<KThread>     Withholder     { get; set; }
        public LinkedListNode<KThread> WithholderNode { get; set; }

        public LinkedListNode<KThread> ProcessListNode { get; set; }

        private LinkedList<KThread>     MutexWaiters;
        private LinkedListNode<KThread> MutexWaiterNode;

        public KThread MutexOwner { get; private set; }

        public int ThreadHandleForUserMutex { get; set; }

        private ThreadSchedState ForcePauseFlags;

        public int ObjSyncResult { get; set; }

        public int DynamicPriority { get; set; }
        public int CurrentCore     { get; set; }
        public int BasePriority    { get; set; }
        public int PreferredCore   { get; set; }

        private long AffinityMaskOverride;
        private int  PreferredCoreOverride;
        private int  AffinityOverrideCount;

        public ThreadSchedState SchedFlags { get; private set; }

        public bool ShallBeTerminated { get; private set; }

        public bool SyncCancelled { get; set; }
        public bool WaitingSync   { get; set; }

        private bool HasExited;

        public bool WaitingInArbitration { get; set; }

        private KScheduler Scheduler;

        private KSchedulingData SchedulingData;

        public long LastPc { get; set; }

        public KThread(Horizon System) : base(System)
        {
            Scheduler      = System.Scheduler;
            SchedulingData = System.Scheduler.SchedulingData;

            SiblingsPerCore = new LinkedListNode<KThread>[KScheduler.CpuCoresCount];

            MutexWaiters = new LinkedList<KThread>();
        }

        public KernelResult Initialize(
            ulong      Entrypoint,
            ulong      ArgsPtr,
            ulong      StackTop,
            int        Priority,
            int        DefaultCpuCore,
            KProcess   Owner,
            ThreadType Type = ThreadType.User)
        {
            if ((uint)Type > 3)
            {
                throw new ArgumentException($"Invalid thread type \"{Type}\".");
            }

            PreferredCore = DefaultCpuCore;

            AffinityMask |= 1L << DefaultCpuCore;

            SchedFlags = Type == ThreadType.Dummy
                ? ThreadSchedState.Running
                : ThreadSchedState.None;

            CurrentCore = PreferredCore;

            DynamicPriority = Priority;
            BasePriority    = Priority;

            ObjSyncResult = 0x7201;

            this.Entrypoint = Entrypoint;

            if (Type == ThreadType.User)
            {
                if (Owner.AllocateThreadLocalStorage(out TlsAddress) != KernelResult.Success)
                {
                    return KernelResult.OutOfMemory;
                }

                MemoryHelper.FillWithZeros(Owner.CpuMemory, (long)TlsAddress, KTlsPageInfo.TlsEntrySize);
            }

            bool Is64Bits;

            if (Owner != null)
            {
                this.Owner = Owner;

                Owner.IncrementThreadCount();

                Is64Bits = (Owner.MmuFlags & 1) != 0;
            }
            else
            {
                Is64Bits = true;
            }

            Context = new CpuThread(Owner.Translator, Owner.CpuMemory, (long)Entrypoint);

            Context.ThreadState.X0  = ArgsPtr;
            Context.ThreadState.X31 = StackTop;

            Context.ThreadState.CntfrqEl0 = 19200000;
            Context.ThreadState.Tpidr     = (long)TlsAddress;

            Owner.SubscribeThreadEventHandlers(Context);

            Context.WorkFinished += ThreadFinishedHandler;

            ThreadUid = System.GetThreadUid();

            if (Owner != null)
            {
                Owner.AddThread(this);

                if (Owner.IsPaused)
                {
                    System.CriticalSection.Enter();

                    if (ShallBeTerminated || SchedFlags == ThreadSchedState.TerminationPending)
                    {
                        System.CriticalSection.Leave();

                        return KernelResult.Success;
                    }

                    ForcePauseFlags |= ThreadSchedState.ProcessPauseFlag;

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
                    ForcePauseFlags |= ThreadSchedState.KernelInitPauseFlag;

                    CombineForcePauseFlags();
                }

                System.CriticalSection.Leave();
            }

            KernelResult Result = KernelResult.ThreadTerminating;

            System.CriticalSection.Enter();

            if (!ShallBeTerminated)
            {
                KThread CurrentThread = System.Scheduler.GetCurrentThread();

                while (SchedFlags               != ThreadSchedState.TerminationPending &&
                       CurrentThread.SchedFlags != ThreadSchedState.TerminationPending &&
                       !CurrentThread.ShallBeTerminated)
                {
                    if ((SchedFlags & ThreadSchedState.LowMask) != ThreadSchedState.None)
                    {
                        Result = KernelResult.InvalidState;

                        break;
                    }

                    if (CurrentThread.ForcePauseFlags == ThreadSchedState.None)
                    {
                        if (Owner != null && ForcePauseFlags != ThreadSchedState.None)
                        {
                            CombineForcePauseFlags();
                        }

                        SetNewSchedFlags(ThreadSchedState.Running);

                        Result = KernelResult.Success;

                        break;
                    }
                    else
                    {
                        CurrentThread.CombineForcePauseFlags();

                        System.CriticalSection.Leave();
                        System.CriticalSection.Enter();

                        if (CurrentThread.ShallBeTerminated)
                        {
                            break;
                        }
                    }
                }
            }

            System.CriticalSection.Leave();

            return Result;
        }

        public void Exit()
        {
            System.CriticalSection.Enter();

            ForcePauseFlags &= ~ThreadSchedState.ForcePauseMask;

            ExitImpl();

            System.CriticalSection.Leave();
        }

        private void ExitImpl()
        {
            System.CriticalSection.Enter();

            SetNewSchedFlags(ThreadSchedState.TerminationPending);

            HasExited = true;

            Signal();

            System.CriticalSection.Leave();
        }

        public long Sleep(long Timeout)
        {
            System.CriticalSection.Enter();

            if (ShallBeTerminated || SchedFlags == ThreadSchedState.TerminationPending)
            {
                System.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.ThreadTerminating);
            }

            SetNewSchedFlags(ThreadSchedState.Paused);

            if (Timeout > 0)
            {
                System.TimeManager.ScheduleFutureInvocation(this, Timeout);
            }

            System.CriticalSection.Leave();

            if (Timeout > 0)
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
                //Move current thread to the end of the queue.
                SchedulingData.Reschedule(DynamicPriority, CurrentCore, this);
            }

            Scheduler.ThreadReselectionRequested = true;

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

            int Prio = DynamicPriority;
            int Core = CurrentCore;

            KThread NextThreadOnCurrentQueue = null;

            if (DynamicPriority < KScheduler.PrioritiesCount)
            {
                //Move current thread to the end of the queue.
                SchedulingData.Reschedule(Prio, Core, this);

                Func<KThread, bool> Predicate = x => x.DynamicPriority == Prio;

                NextThreadOnCurrentQueue = SchedulingData.ScheduledThreads(Core).FirstOrDefault(Predicate);
            }

            IEnumerable<KThread> SuitableCandidates()
            {
                foreach (KThread Thread in SchedulingData.SuggestedThreads(Core))
                {
                    int SrcCore = Thread.CurrentCore;

                    if (SrcCore >= 0)
                    {
                        KThread SelectedSrcCore = Scheduler.CoreContexts[SrcCore].SelectedThread;

                        if (SelectedSrcCore == Thread || ((SelectedSrcCore?.DynamicPriority ?? 2) < 2))
                        {
                            continue;
                        }
                    }

                    //If the candidate was scheduled after the current thread, then it's not worth it,
                    //unless the priority is higher than the current one.
                    if (NextThreadOnCurrentQueue.LastScheduledTime >= Thread.LastScheduledTime ||
                        NextThreadOnCurrentQueue.DynamicPriority    <  Thread.DynamicPriority)
                    {
                        yield return Thread;
                    }
                }
            }

            KThread Dst = SuitableCandidates().FirstOrDefault(x => x.DynamicPriority <= Prio);

            if (Dst != null)
            {
                SchedulingData.TransferToCore(Dst.DynamicPriority, Core, Dst);

                Scheduler.ThreadReselectionRequested = true;
            }

            if (this != NextThreadOnCurrentQueue)
            {
                Scheduler.ThreadReselectionRequested = true;
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

            int Core = CurrentCore;

            SchedulingData.TransferToCore(DynamicPriority, -1, this);

            KThread SelectedThread = null;

            if (!SchedulingData.ScheduledThreads(Core).Any())
            {
                foreach (KThread Thread in SchedulingData.SuggestedThreads(Core))
                {
                    if (Thread.CurrentCore < 0)
                    {
                        continue;
                    }

                    KThread FirstCandidate = SchedulingData.ScheduledThreads(Thread.CurrentCore).FirstOrDefault();

                    if (FirstCandidate == Thread)
                    {
                        continue;
                    }

                    if (FirstCandidate == null || FirstCandidate.DynamicPriority >= 2)
                    {
                        SchedulingData.TransferToCore(Thread.DynamicPriority, Core, Thread);

                        SelectedThread = Thread;
                    }

                    break;
                }
            }

            if (SelectedThread != this)
            {
                Scheduler.ThreadReselectionRequested = true;
            }

            System.CriticalSection.Leave();

            System.Scheduler.ContextSwitch();
        }

        public void SetPriority(int Priority)
        {
            System.CriticalSection.Enter();

            BasePriority = Priority;

            UpdatePriorityInheritance();

            System.CriticalSection.Leave();
        }

        public long SetActivity(bool Pause)
        {
            long Result = 0;

            System.CriticalSection.Enter();

            ThreadSchedState LowNibble = SchedFlags & ThreadSchedState.LowMask;

            if (LowNibble != ThreadSchedState.Paused && LowNibble != ThreadSchedState.Running)
            {
                System.CriticalSection.Leave();

                return MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
            }

            System.CriticalSection.Enter();

            if (!ShallBeTerminated && SchedFlags != ThreadSchedState.TerminationPending)
            {
                if (Pause)
                {
                    //Pause, the force pause flag should be clear (thread is NOT paused).
                    if ((ForcePauseFlags & ThreadSchedState.ThreadPauseFlag) == 0)
                    {
                        ForcePauseFlags |= ThreadSchedState.ThreadPauseFlag;

                        CombineForcePauseFlags();
                    }
                    else
                    {
                        Result = MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
                    }
                }
                else
                {
                    //Unpause, the force pause flag should be set (thread is paused).
                    if ((ForcePauseFlags & ThreadSchedState.ThreadPauseFlag) != 0)
                    {
                        ThreadSchedState OldForcePauseFlags = ForcePauseFlags;

                        ForcePauseFlags &= ~ThreadSchedState.ThreadPauseFlag;

                        if ((OldForcePauseFlags & ~ThreadSchedState.ThreadPauseFlag) == ThreadSchedState.None)
                        {
                            ThreadSchedState OldSchedFlags = SchedFlags;

                            SchedFlags &= ThreadSchedState.LowMask;

                            AdjustScheduling(OldSchedFlags);
                        }
                    }
                    else
                    {
                        Result = MakeError(ErrorModule.Kernel, KernelErr.InvalidState);
                    }
                }
            }

            System.CriticalSection.Leave();
            System.CriticalSection.Leave();

            return Result;
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
                ObjSyncResult = (int)MakeError(ErrorModule.Kernel, KernelErr.Cancelled);

                SetNewSchedFlags(ThreadSchedState.Running);

                SyncCancelled = false;
            }

            System.CriticalSection.Leave();
        }

        public KernelResult SetCoreAndAffinityMask(int NewCore, long NewAffinityMask)
        {
            System.CriticalSection.Enter();

            bool UseOverride = AffinityOverrideCount != 0;

            //The value -3 is "do not change the preferred core".
            if (NewCore == -3)
            {
                NewCore = UseOverride ? PreferredCoreOverride : PreferredCore;

                if ((NewAffinityMask & (1 << NewCore)) == 0)
                {
                    System.CriticalSection.Leave();

                    return KernelResult.InvalidCombination;
                }
            }

            if (UseOverride)
            {
                PreferredCoreOverride = NewCore;
                AffinityMaskOverride  = NewAffinityMask;
            }
            else
            {
                long OldAffinityMask = AffinityMask;

                PreferredCore = NewCore;
                AffinityMask  = NewAffinityMask;

                if (OldAffinityMask != NewAffinityMask)
                {
                    int OldCore = CurrentCore;

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

                    AdjustSchedulingForNewAffinity(OldAffinityMask, OldCore);
                }
            }

            System.CriticalSection.Leave();

            return KernelResult.Success;
        }

        private static int HighestSetCore(long Mask)
        {
            for (int Core = KScheduler.CpuCoresCount - 1; Core >= 0; Core--)
            {
                if (((Mask >> Core) & 1) != 0)
                {
                    return Core;
                }
            }

            return -1;
        }

        private void CombineForcePauseFlags()
        {
            ThreadSchedState OldFlags  = SchedFlags;
            ThreadSchedState LowNibble = SchedFlags & ThreadSchedState.LowMask;

            SchedFlags = LowNibble | ForcePauseFlags;

            AdjustScheduling(OldFlags);
        }

        private void SetNewSchedFlags(ThreadSchedState NewFlags)
        {
            System.CriticalSection.Enter();

            ThreadSchedState OldFlags = SchedFlags;

            SchedFlags = (OldFlags & ThreadSchedState.HighMask) | NewFlags;

            if ((OldFlags & ThreadSchedState.LowMask) != NewFlags)
            {
                AdjustScheduling(OldFlags);
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

        public void Reschedule(ThreadSchedState NewFlags)
        {
            System.CriticalSection.Enter();

            ThreadSchedState OldFlags = SchedFlags;

            SchedFlags = (OldFlags & ThreadSchedState.HighMask) |
                         (NewFlags & ThreadSchedState.LowMask);

            AdjustScheduling(OldFlags);

            System.CriticalSection.Leave();
        }

        public void AddMutexWaiter(KThread Requester)
        {
            AddToMutexWaitersList(Requester);

            Requester.MutexOwner = this;

            UpdatePriorityInheritance();
        }

        public void RemoveMutexWaiter(KThread Thread)
        {
            if (Thread.MutexWaiterNode?.List != null)
            {
                MutexWaiters.Remove(Thread.MutexWaiterNode);
            }

            Thread.MutexOwner = null;

            UpdatePriorityInheritance();
        }

        public KThread RelinquishMutex(long MutexAddress, out int Count)
        {
            Count = 0;

            if (MutexWaiters.First == null)
            {
                return null;
            }

            KThread NewMutexOwner = null;

            LinkedListNode<KThread> CurrentNode = MutexWaiters.First;

            do
            {
                //Skip all threads that are not waiting for this mutex.
                while (CurrentNode != null && CurrentNode.Value.MutexAddress != MutexAddress)
                {
                    CurrentNode = CurrentNode.Next;
                }

                if (CurrentNode == null)
                {
                    break;
                }

                LinkedListNode<KThread> NextNode = CurrentNode.Next;

                MutexWaiters.Remove(CurrentNode);

                CurrentNode.Value.MutexOwner = NewMutexOwner;

                if (NewMutexOwner != null)
                {
                    //New owner was already selected, re-insert on new owner list.
                    NewMutexOwner.AddToMutexWaitersList(CurrentNode.Value);
                }
                else
                {
                    //New owner not selected yet, use current thread.
                    NewMutexOwner = CurrentNode.Value;
                }

                Count++;

                CurrentNode = NextNode;
            }
            while (CurrentNode != null);

            if (NewMutexOwner != null)
            {
                UpdatePriorityInheritance();

                NewMutexOwner.UpdatePriorityInheritance();
            }

            return NewMutexOwner;
        }

        private void UpdatePriorityInheritance()
        {
            //If any of the threads waiting for the mutex has
            //higher priority than the current thread, then
            //the current thread inherits that priority.
            int HighestPriority = BasePriority;

            if (MutexWaiters.First != null)
            {
                int WaitingDynamicPriority = MutexWaiters.First.Value.DynamicPriority;

                if (WaitingDynamicPriority < HighestPriority)
                {
                    HighestPriority = WaitingDynamicPriority;
                }
            }

            if (HighestPriority != DynamicPriority)
            {
                int OldPriority = DynamicPriority;

                DynamicPriority = HighestPriority;

                AdjustSchedulingForNewPriority(OldPriority);

                if (MutexOwner != null)
                {
                    //Remove and re-insert to ensure proper sorting based on new priority.
                    MutexOwner.MutexWaiters.Remove(MutexWaiterNode);

                    MutexOwner.AddToMutexWaitersList(this);

                    MutexOwner.UpdatePriorityInheritance();
                }
            }
        }

        private void AddToMutexWaitersList(KThread Thread)
        {
            LinkedListNode<KThread> NextPrio = MutexWaiters.First;

            int CurrentPriority = Thread.DynamicPriority;

            while (NextPrio != null && NextPrio.Value.DynamicPriority <= CurrentPriority)
            {
                NextPrio = NextPrio.Next;
            }

            if (NextPrio != null)
            {
                Thread.MutexWaiterNode = MutexWaiters.AddBefore(NextPrio, Thread);
            }
            else
            {
                Thread.MutexWaiterNode = MutexWaiters.AddLast(Thread);
            }
        }

        private void AdjustScheduling(ThreadSchedState OldFlags)
        {
            if (OldFlags == SchedFlags)
            {
                return;
            }

            if (OldFlags == ThreadSchedState.Running)
            {
                //Was running, now it's stopped.
                if (CurrentCore >= 0)
                {
                    SchedulingData.Unschedule(DynamicPriority, CurrentCore, this);
                }

                for (int Core = 0; Core < KScheduler.CpuCoresCount; Core++)
                {
                    if (Core != CurrentCore && ((AffinityMask >> Core) & 1) != 0)
                    {
                        SchedulingData.Unsuggest(DynamicPriority, Core, this);
                    }
                }
            }
            else if (SchedFlags == ThreadSchedState.Running)
            {
                //Was stopped, now it's running.
                if (CurrentCore >= 0)
                {
                    SchedulingData.Schedule(DynamicPriority, CurrentCore, this);
                }

                for (int Core = 0; Core < KScheduler.CpuCoresCount; Core++)
                {
                    if (Core != CurrentCore && ((AffinityMask >> Core) & 1) != 0)
                    {
                        SchedulingData.Suggest(DynamicPriority, Core, this);
                    }
                }
            }

            Scheduler.ThreadReselectionRequested = true;
        }

        private void AdjustSchedulingForNewPriority(int OldPriority)
        {
            if (SchedFlags != ThreadSchedState.Running)
            {
                return;
            }

            //Remove thread from the old priority queues.
            if (CurrentCore >= 0)
            {
                SchedulingData.Unschedule(OldPriority, CurrentCore, this);
            }

            for (int Core = 0; Core < KScheduler.CpuCoresCount; Core++)
            {
                if (Core != CurrentCore && ((AffinityMask >> Core) & 1) != 0)
                {
                    SchedulingData.Unsuggest(OldPriority, Core, this);
                }
            }

            //Add thread to the new priority queues.
            KThread CurrentThread = Scheduler.GetCurrentThread();

            if (CurrentCore >= 0)
            {
                if (CurrentThread == this)
                {
                    SchedulingData.SchedulePrepend(DynamicPriority, CurrentCore, this);
                }
                else
                {
                    SchedulingData.Schedule(DynamicPriority, CurrentCore, this);
                }
            }

            for (int Core = 0; Core < KScheduler.CpuCoresCount; Core++)
            {
                if (Core != CurrentCore && ((AffinityMask >> Core) & 1) != 0)
                {
                    SchedulingData.Suggest(DynamicPriority, Core, this);
                }
            }

            Scheduler.ThreadReselectionRequested = true;
        }

        private void AdjustSchedulingForNewAffinity(long OldAffinityMask, int OldCore)
        {
            if (SchedFlags != ThreadSchedState.Running || DynamicPriority >= KScheduler.PrioritiesCount)
            {
                return;
            }

            //Remove from old queues.
            for (int Core = 0; Core < KScheduler.CpuCoresCount; Core++)
            {
                if (((OldAffinityMask >> Core) & 1) != 0)
                {
                    if (Core == OldCore)
                    {
                        SchedulingData.Unschedule(DynamicPriority, Core, this);
                    }
                    else
                    {
                        SchedulingData.Unsuggest(DynamicPriority, Core, this);
                    }
                }
            }

            //Insert on new queues.
            for (int Core = 0; Core < KScheduler.CpuCoresCount; Core++)
            {
                if (((AffinityMask >> Core) & 1) != 0)
                {
                    if (Core == CurrentCore)
                    {
                        SchedulingData.Schedule(DynamicPriority, Core, this);
                    }
                    else
                    {
                        SchedulingData.Suggest(DynamicPriority, Core, this);
                    }
                }
            }

            Scheduler.ThreadReselectionRequested = true;
        }

        public override bool IsSignaled()
        {
            return HasExited;
        }

        public void SetEntryArguments(long ArgsPtr, int ThreadHandle)
        {
            Context.ThreadState.X0 = (ulong)ArgsPtr;
            Context.ThreadState.X1 = (ulong)ThreadHandle;
        }

        public void ClearExclusive()
        {
            Owner.CpuMemory.ClearExclusive(CurrentCore);
        }

        public void TimeUp()
        {
            ReleaseAndResume();
        }

        public void PrintGuestStackTrace()
        {
            Owner.Debugger.PrintGuestStackTrace(Context.ThreadState);
        }

        private void ThreadFinishedHandler(object sender, EventArgs e)
        {
            System.Scheduler.ExitThread(this);

            Terminate();

            System.Scheduler.RemoveThread(this);
        }

        public void Terminate()
        {
            Owner?.RemoveThread(this);

            if (TlsAddress != 0 && Owner.FreeThreadLocalStorage(TlsAddress) != KernelResult.Success)
            {
                throw new InvalidOperationException("Unexpected failure freeing thread local storage.");
            }

            System.CriticalSection.Enter();

            //Wake up all threads that may be waiting for a mutex being held
            //by this thread.
            foreach (KThread Thread in MutexWaiters)
            {
                Thread.MutexOwner            = null;
                Thread.PreferredCoreOverride = 0;
                Thread.ObjSyncResult         = 0xfa01;

                Thread.ReleaseAndResume();
            }

            System.CriticalSection.Leave();

            Owner?.DecrementThreadCountAndTerminateIfZero();
        }
    }
}