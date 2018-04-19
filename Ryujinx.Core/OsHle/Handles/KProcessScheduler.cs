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

            public AutoResetEvent WaitEvent { get; private set; }

            public SchedulerThread(KThread Thread)
            {
                this.Thread = Thread;

                WaitEvent = new AutoResetEvent(false);
            }

            public void Dispose()
            {
                Dispose(true);
            }

            protected virtual void Dispose(bool Disposing)
            {
                if (Disposing)
                {
                    WaitEvent.Dispose();
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

                        if (HighestPriority > SchedThread.Thread.Priority)
                        {
                            HighestPriority = SchedThread.Thread.Priority;

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
                return Threads.Remove(SchedThread);
            }
        }

        private ConcurrentDictionary<KThread, SchedulerThread> AllThreads;

        private ThreadQueue[] WaitingToRun;

        private HashSet<int> ActiveProcessors;

        private object SchedLock;

        public KProcessScheduler()
        {
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
                }

                SchedulerThread NewThread = WaitingToRun[Thread.ProcessorId].Pop();

                if (NewThread == null)
                {
                    Logging.Debug(LogClass.KernelScheduler, $"Nothing to run on core {Thread.ProcessorId}!");

                    ActiveProcessors.Remove(Thread.ProcessorId);

                    return;
                }

                RunThread(NewThread);
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
                    Logging.Debug(LogClass.KernelScheduler, $"Nothing to run on core {ProcessorId}!");

                    ActiveProcessors.Remove(ProcessorId);
                }
            }
        }

        public void Yield(KThread Thread)
        {
            PrintDbgThreadInfo(Thread, "yielded execution.");

            lock (SchedLock)
            {
                SchedulerThread SchedThread = WaitingToRun[Thread.ProcessorId].Pop(Thread.Priority);

                if (SchedThread == null)
                {
                    PrintDbgThreadInfo(Thread, "resumed because theres nothing better to run.");

                    return;
                }

                RunThread(SchedThread);
            }

            Resume(Thread);
        }

        public void Resume(KThread Thread)
        {
            SchedulerThread SchedThread;

            if (!AllThreads.TryGetValue(Thread, out SchedThread))
            {
                throw new InvalidOperationException();
            }

            TryResumingExecution(SchedThread);
        }

        private void TryResumingExecution(SchedulerThread SchedThread)
        {
            KThread Thread = SchedThread.Thread;

            lock (SchedLock)
            {
                if (ActiveProcessors.Add(Thread.ProcessorId))
                {
                    PrintDbgThreadInfo(Thread, "resuming execution...");

                    return;
                }

                PrintDbgThreadInfo(Thread, "entering wait state...");

                WaitingToRun[Thread.ProcessorId].Push(SchedThread);
            }

            SchedThread.WaitEvent.WaitOne();

            PrintDbgThreadInfo(Thread, "resuming execution...");
        }

        private void RunThread(SchedulerThread SchedThread)
        {
            if (!SchedThread.Thread.Thread.Execute())
            {
                SchedThread.WaitEvent.Set();
            }
            else
            {
                PrintDbgThreadInfo(SchedThread.Thread, "running.");
            }
        }

        private void PrintDbgThreadInfo(KThread Thread, string Message)
        {
            Logging.Debug(LogClass.KernelScheduler, "(" +
                "ThreadId: "    + Thread.ThreadId    + ", " +
                "ProcessorId: " + Thread.ProcessorId + ", " +
                "Priority: "    + Thread.Priority    + ") " + Message);
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