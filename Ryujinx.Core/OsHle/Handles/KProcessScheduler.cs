using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Core.OsHle.Handles
{
    public class KProcessScheduler : IDisposable
    {
        private class SchedulerThread : IDisposable
        {
            public HThread Thread { get; private set; }

            public AutoResetEvent WaitEvent { get; private set; }

            public SchedulerThread(HThread Thread)
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

            public SchedulerThread Pop(int MinPriority = 0x40)
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
        }

        private ConcurrentDictionary<HThread, SchedulerThread> AllThreads;

        private ThreadQueue[] WaitingToRun;

        private HashSet<int> ActiveProcessors;

        private object SchedLock;

        public KProcessScheduler()
        {
            AllThreads = new ConcurrentDictionary<HThread, SchedulerThread>();

            WaitingToRun = new ThreadQueue[4];

            for (int Index = 0; Index < 4; Index++)
            {
                WaitingToRun[Index] = new ThreadQueue();
            }

            ActiveProcessors = new HashSet<int>();

            SchedLock = new object();
        }

        public void StartThread(HThread Thread)
        {
            lock (SchedLock)
            {
                SchedulerThread SchedThread = new SchedulerThread(Thread);

                if (!AllThreads.TryAdd(Thread, SchedThread))
                {
                    return;
                }

                if (!ActiveProcessors.Contains(Thread.ProcessorId))
                {
                    ActiveProcessors.Add(Thread.ProcessorId);

                    Thread.Thread.Execute();

                    Logging.Debug($"{GetDbgThreadInfo(Thread)} running.");
                }
                else
                {
                    WaitingToRun[Thread.ProcessorId].Push(SchedThread);

                    Logging.Debug($"{GetDbgThreadInfo(SchedThread.Thread)} waiting to run.");
                }
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
                    ActiveProcessors.Remove(ProcessorId);
                }
            }
        }

        public void Resume(HThread CurrThread)
        {
            SchedulerThread SchedThread;

            Logging.Debug($"{GetDbgThreadInfo(CurrThread)} entering ipc delay wait state.");

            lock (SchedLock)
            {
                if (!AllThreads.TryGetValue(CurrThread, out SchedThread))
                {
                    Logging.Error($"{GetDbgThreadInfo(CurrThread)} was not found on the scheduler queue!");

                    return;
                }
            }

            TryResumingExecution(SchedThread);
        }

        public void WaitForSignal(HThread Thread, int Timeout = -1)
        {
            SchedulerThread SchedThread;

            Logging.Debug($"{GetDbgThreadInfo(Thread)} entering signal wait state.");

            lock (SchedLock)
            {
                SchedThread = WaitingToRun[Thread.ProcessorId].Pop();

                if (SchedThread != null)
                {
                    RunThread(SchedThread);
                }
                else
                {
                    ActiveProcessors.Remove(Thread.ProcessorId);
                }

                if (!AllThreads.TryGetValue(Thread, out SchedThread))
                {
                    Logging.Error($"{GetDbgThreadInfo(Thread)} was not found on the scheduler queue!");

                    return;
                }
            }

            if (Timeout >= 0)
            {
                Logging.Debug($"{GetDbgThreadInfo(Thread)} has wait timeout of {Timeout}ms.");

                SchedThread.WaitEvent.WaitOne(Timeout);
            }
            else
            {
                SchedThread.WaitEvent.WaitOne();
            }

            TryResumingExecution(SchedThread);
        }

        private void TryResumingExecution(SchedulerThread SchedThread)
        {
            HThread Thread = SchedThread.Thread;

            lock (SchedLock)
            {
                if (ActiveProcessors.Add(Thread.ProcessorId))
                {
                    Logging.Debug($"{GetDbgThreadInfo(Thread)} resuming execution...");

                    return;
                }

                WaitingToRun[Thread.ProcessorId].Push(SchedThread);
            }

            SchedThread.WaitEvent.WaitOne();

            Logging.Debug($"{GetDbgThreadInfo(Thread)} resuming execution...");
        }

        public void Yield(HThread Thread)
        {
            SchedulerThread SchedThread;

            Logging.Debug($"{GetDbgThreadInfo(Thread)} yielded execution.");

            lock (SchedLock)
            {
                SchedThread = WaitingToRun[Thread.ProcessorId].Pop(Thread.Priority);

                if (SchedThread == null)
                {
                    Logging.Debug($"{GetDbgThreadInfo(Thread)} resumed because theres nothing better to run.");

                    return;
                }
                
                RunThread(SchedThread);

                if (!AllThreads.TryGetValue(Thread, out SchedThread))
                {
                    Logging.Error($"{GetDbgThreadInfo(Thread)} was not found on the scheduler queue!");

                    return;
                }

                WaitingToRun[Thread.ProcessorId].Push(SchedThread);
            }

            SchedThread.WaitEvent.WaitOne();

            Logging.Debug($"{GetDbgThreadInfo(Thread)} resuming execution...");
        }

        private void RunThread(SchedulerThread SchedThread)
        {
            if (!SchedThread.Thread.Thread.Execute())
            {
                SchedThread.WaitEvent.Set();
            }
            else
            {
                Logging.Debug($"{GetDbgThreadInfo(SchedThread.Thread)} running.");
            }
        }

        public void Signal(params HThread[] Threads)
        {
            lock (SchedLock)
            {
                foreach (HThread Thread in Threads)
                {
                    if (AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
                    {
                        if (!WaitingToRun[Thread.ProcessorId].HasThread(SchedThread))
                        {
                            Logging.Debug($"{GetDbgThreadInfo(Thread)} signaled.");

                            SchedThread.WaitEvent.Set();
                        }
                    }
                }
            }
        }

        private string GetDbgThreadInfo(HThread Thread)
        {
            return $"Thread {Thread.ThreadId} (core {Thread.ProcessorId}) prio {Thread.Priority}";
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