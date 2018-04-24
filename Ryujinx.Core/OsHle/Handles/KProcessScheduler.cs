using Ryujinx.Core.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Core.OsHle.Handles
{
    class KProcessScheduler : IDisposable
    {
        private const int LowestPriority = 0x40;

        private class SchedulerThread : IDisposable
        {
            public KThread Thread { get; private set; }

            public ManualResetEvent SyncWaitEvent  { get; private set; }
            public AutoResetEvent   SchedWaitEvent { get; private set; }

            public bool Active { get; set; }

            public int SyncTimeout { get; set; }

            public SchedulerThread(KThread Thread)
            {
                this.Thread = Thread;

                SyncWaitEvent  = new ManualResetEvent(true);
                SchedWaitEvent = new AutoResetEvent(false);

                Active = true;

                SyncTimeout = 0;
            }

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool Disposing)
            {
                if (Disposing)
                {
                    SyncWaitEvent.Dispose();
                    SchedWaitEvent.Dispose();
                }
            }
        }

        private class ThreadQueue
        {
            private List<SchedulerThread> Threads;

            public ThreadQueue()
            {
                Threads = new List<SchedulerThread>();
            }

            public void Push(SchedulerThread Thread)
            {
                lock (Threads)
                {
                    Threads.Add(Thread);
                }
            }

            public SchedulerThread Pop(int MinPriority = LowestPriority)
            {
                lock (Threads)
                {
                    SchedulerThread SchedThread;

                    int HighestPriority = MinPriority;

                    int HighestPrioIndex = -1;

                    for (int Index = 0; Index < Threads.Count; Index++)
                    {
                        SchedThread = Threads[Index];

                        if (HighestPriority > SchedThread.Thread.ActualPriority)
                        {
                            HighestPriority = SchedThread.Thread.ActualPriority;

                            HighestPrioIndex = Index;
                        }
                    }

                    if (HighestPrioIndex == -1)
                    {
                        return null;
                    }

                    SchedThread = Threads[HighestPrioIndex];

                    Threads.RemoveAt(HighestPrioIndex);

                    return SchedThread;
                }
            }

            public bool HasThread(SchedulerThread SchedThread)
            {
                lock (Threads)
                {
                    return Threads.Contains(SchedThread);
                }
            }

            public bool Remove(SchedulerThread SchedThread)
            {
                lock (Threads)
                {
                    return Threads.Remove(SchedThread);
                }
            }
        }

        private ConcurrentDictionary<KThread, SchedulerThread> AllThreads;

        private ThreadQueue[] WaitingToRun;

        private HashSet<int> ActiveProcessors;

        private object SchedLock;

        private Logger Log;

        public KProcessScheduler(Logger Log)
        {
            this.Log = Log;

            AllThreads = new ConcurrentDictionary<KThread, SchedulerThread>();

            WaitingToRun = new ThreadQueue[4];

            for (int Index = 0; Index < 4; Index++)
            {
                WaitingToRun[Index] = new ThreadQueue();
            }

            ActiveProcessors = new HashSet<int>();

            SchedLock = new object();
        }

        public void StartThread(KThread Thread)
        {
            lock (SchedLock)
            {
                SchedulerThread SchedThread = new SchedulerThread(Thread);

                if (!AllThreads.TryAdd(Thread, SchedThread))
                {
                    return;
                }

                if (ActiveProcessors.Add(Thread.ProcessorId))
                {
                    Thread.Thread.Execute();

                    PrintDbgThreadInfo(Thread, "running.");
                }
                else
                {
                    WaitingToRun[Thread.ProcessorId].Push(SchedThread);

                    PrintDbgThreadInfo(Thread, "waiting to run.");
                }
            }
        }

        public void RemoveThread(KThread Thread)
        {
            PrintDbgThreadInfo(Thread, "exited.");

            lock (SchedLock)
            {
                if (AllThreads.TryRemove(Thread, out SchedulerThread SchedThread))
                {
                    WaitingToRun[Thread.ProcessorId].Remove(SchedThread);

                    SchedThread.Dispose();
                }

                SchedulerThread NewThread = WaitingToRun[Thread.ProcessorId].Pop();

                if (NewThread == null)
                {
                    Log.PrintDebug(LogClass.KernelScheduler, $"Nothing to run on core {Thread.ProcessorId}!");

                    ActiveProcessors.Remove(Thread.ProcessorId);

                    return;
                }

                RunThread(NewThread);
            }
        }

        public void SetThreadActivity(KThread Thread, bool Active)
        {
            if (!AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
            {
                throw new InvalidOperationException();
            }

            SchedThread.Active = Active;

            UpdateSyncWaitEvent(SchedThread);

            WaitIfNeeded(SchedThread);
        }

        public bool EnterWait(KThread Thread, int Timeout = -1)
        {
            if (!AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
            {
                throw new InvalidOperationException();
            }

            SchedThread.SyncTimeout = Timeout;

            UpdateSyncWaitEvent(SchedThread);

            return WaitIfNeeded(SchedThread);
        }

        public void WakeUp(KThread Thread)
        {
            if (!AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
            {
                throw new InvalidOperationException();
            }

            SchedThread.SyncTimeout = 0;

            UpdateSyncWaitEvent(SchedThread);

            WaitIfNeeded(SchedThread);
        }

        private void UpdateSyncWaitEvent(SchedulerThread SchedThread)
        {
            if (SchedThread.Active && SchedThread.SyncTimeout == 0)
            {
                SchedThread.SyncWaitEvent.Set();
            }
            else
            {
                SchedThread.SyncWaitEvent.Reset();
            }
        }

        private bool WaitIfNeeded(SchedulerThread SchedThread)
        {
            KThread Thread = SchedThread.Thread;

            if (!IsActive(SchedThread) && Thread.Thread.IsCurrentThread())
            {
                Suspend(Thread.ProcessorId);

                return Resume(Thread);
            }
            else
            {
                return false;
            }
        }

        public void Suspend(int ProcessorId)
        {
            lock (SchedLock)
            {
                SchedulerThread SchedThread = WaitingToRun[ProcessorId].Pop();

                if (SchedThread != null)
                {
                    RunThread(SchedThread);
                }
                else
                {
                    Log.PrintDebug(LogClass.KernelScheduler, $"Nothing to run on core {ProcessorId}!");

                    ActiveProcessors.Remove(ProcessorId);
                }
            }
        }

        public void Yield(KThread Thread)
        {
            PrintDbgThreadInfo(Thread, "yielded execution.");

            lock (SchedLock)
            {
                SchedulerThread SchedThread = WaitingToRun[Thread.ProcessorId].Pop(Thread.ActualPriority);

                if (IsActive(Thread) && SchedThread == null)
                {
                    PrintDbgThreadInfo(Thread, "resumed because theres nothing better to run.");

                    return;
                }

                if (SchedThread != null)
                {
                    RunThread(SchedThread);
                }
            }

            Resume(Thread);
        }

        public bool Resume(KThread Thread)
        {
            if (!AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
            {
                throw new InvalidOperationException();
            }

            return TryResumingExecution(SchedThread);
        }

        private bool TryResumingExecution(SchedulerThread SchedThread)
        {
            KThread Thread = SchedThread.Thread;

            if (!SchedThread.Active || SchedThread.SyncTimeout != 0)
            {
                PrintDbgThreadInfo(Thread, "entering inactive wait state...");
            }

            bool Result = false;

            if (SchedThread.SyncTimeout != 0)
            {
                Result = SchedThread.SyncWaitEvent.WaitOne(SchedThread.SyncTimeout);

                SchedThread.SyncTimeout = 0;
            }

            lock (SchedLock)
            {
                if (ActiveProcessors.Add(Thread.ProcessorId))
                {
                    PrintDbgThreadInfo(Thread, "resuming execution...");

                    return Result;
                }

                WaitingToRun[Thread.ProcessorId].Push(SchedThread);

                PrintDbgThreadInfo(Thread, "entering wait state...");
            }

            SchedThread.SchedWaitEvent.WaitOne();

            PrintDbgThreadInfo(Thread, "resuming execution...");

            return Result;
        }

        private void RunThread(SchedulerThread SchedThread)
        {
            if (!SchedThread.Thread.Thread.Execute())
            {
                SchedThread.SchedWaitEvent.Set();
            }
            else
            {
                PrintDbgThreadInfo(SchedThread.Thread, "running.");
            }
        }

        private bool IsActive(KThread Thread)
        {
            if (!AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
            {
                throw new InvalidOperationException();
            }

            return IsActive(SchedThread);
        }

        private bool IsActive(SchedulerThread SchedThread)
        {
            return SchedThread.Active && SchedThread.SyncTimeout == 0;
        }

        private void PrintDbgThreadInfo(KThread Thread, string Message)
        {
            Log.PrintDebug(LogClass.KernelScheduler, "(" +
                "ThreadId: "       + Thread.ThreadId       + ", " +
                "ProcessorId: "    + Thread.ProcessorId    + ", " +
                "ActualPriority: " + Thread.ActualPriority + ", " +
                "WantedPriority: " + Thread.WantedPriority + ") " + Message);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                foreach (SchedulerThread SchedThread in AllThreads.Values)
                {
                    SchedThread.Dispose();
                }
            }
        }
    }
}