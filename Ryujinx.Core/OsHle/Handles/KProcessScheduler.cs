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

            public bool IsActive { get; set; }

            public AutoResetEvent   WaitSync     { get; private set; }
            public ManualResetEvent WaitActivity { get; private set; }
            public AutoResetEvent   WaitSched    { get; private set; }

            public SchedulerThread(KThread Thread)
            {
                this.Thread = Thread;

                IsActive = true;

                WaitSync  = new AutoResetEvent(false);

                WaitActivity = new ManualResetEvent(true);

                WaitSched = new AutoResetEvent(false);
            }

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool Disposing)
            {
                if (Disposing)
                {
                    WaitSync.Dispose();

                    WaitActivity.Dispose();

                    WaitSched.Dispose();
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

            SchedThread.IsActive = Active;

            if (Active)
            {
                SchedThread.WaitActivity.Set();
            }
            else
            {
                SchedThread.WaitActivity.Reset();
            }
        }

        public void EnterWait(KThread Thread)
        {
            if (!AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
            {
                throw new InvalidOperationException();
            }

            Suspend(Thread.ProcessorId);

            SchedThread.WaitSync.WaitOne();

            TryResumingExecution(SchedThread);
        }

        public bool EnterWait(KThread Thread, int Timeout)
        {
            if (!AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
            {
                throw new InvalidOperationException();
            }

            Suspend(Thread.ProcessorId);

            bool Result = SchedThread.WaitSync.WaitOne(Timeout);

            TryResumingExecution(SchedThread);

            return Result;
        }

        public void WakeUp(KThread Thread)
        {
            if (!AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
            {
                throw new InvalidOperationException();
            }

            SchedThread.WaitSync.Set();
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

            if (IsActive(Thread))
            {
                lock (SchedLock)
                {
                    SchedulerThread SchedThread = WaitingToRun[Thread.ProcessorId].Pop(Thread.ActualPriority);

                    if (SchedThread == null)
                    {
                        PrintDbgThreadInfo(Thread, "resumed because theres nothing better to run.");

                        return;
                    }

                    if (SchedThread != null)
                    {
                        RunThread(SchedThread);
                    }
                }
            }
            else
            {
                //Just stop running the thread if it's not active,
                //and run whatever is waiting to run with the higuest priority.
                Suspend(Thread.ProcessorId);
            }

            Resume(Thread);
        }

        public void Resume(KThread Thread)
        {
            if (!AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
            {
                throw new InvalidOperationException();
            }

            TryResumingExecution(SchedThread);
        }

        private void TryResumingExecution(SchedulerThread SchedThread)
        {
            KThread Thread = SchedThread.Thread;

            PrintDbgThreadInfo(Thread, "trying to resume...");

            SchedThread.WaitActivity.WaitOne();

            lock (SchedLock)
            {
                if (ActiveProcessors.Add(Thread.ProcessorId))
                {
                    PrintDbgThreadInfo(Thread, "resuming execution...");

                    return;
                }

                WaitingToRun[Thread.ProcessorId].Push(SchedThread);

                PrintDbgThreadInfo(Thread, "entering wait state...");
            }

            SchedThread.WaitSched.WaitOne();

            PrintDbgThreadInfo(Thread, "resuming execution...");
        }

        private void RunThread(SchedulerThread SchedThread)
        {
            if (!SchedThread.Thread.Thread.Execute())
            {
                SchedThread.WaitSched.Set();
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

            return SchedThread.IsActive;
        }

        private void PrintDbgThreadInfo(KThread Thread, string Message)
        {
            Log.PrintDebug(LogClass.KernelScheduler, "(" +
                "ThreadId = "       + Thread.ThreadId       + ", " +
                "ProcessorId = "    + Thread.ProcessorId    + ", " +
                "ActualPriority = " + Thread.ActualPriority + ", " +
                "WantedPriority = " + Thread.WantedPriority + ") " + Message);
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