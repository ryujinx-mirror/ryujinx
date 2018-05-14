using Ryujinx.Core.Logging;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.Core.OsHle.Handles
{
    class KProcessScheduler : IDisposable
    {
        private ConcurrentDictionary<KThread, SchedulerThread> AllThreads;

        private ThreadQueue WaitingToRun;

        private int ActiveCores;

        private object SchedLock;

        private Logger Log;

        public KProcessScheduler(Logger Log)
        {
            this.Log = Log;

            AllThreads = new ConcurrentDictionary<KThread, SchedulerThread>();

            WaitingToRun = new ThreadQueue();

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

                if (AddActiveCore(Thread))
                {
                    Thread.Thread.Execute();

                    PrintDbgThreadInfo(Thread, "running.");
                }
                else
                {
                    WaitingToRun.Push(SchedThread);

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
                    WaitingToRun.Remove(SchedThread);

                    SchedThread.Dispose();
                }

                SchedulerThread NewThread = WaitingToRun.Pop(Thread.ActualCore);

                if (NewThread == null)
                {
                    Log.PrintDebug(LogClass.KernelScheduler, $"Nothing to run on core {Thread.ActualCore}!");

                    RemoveActiveCore(Thread.ActualCore);

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

            Suspend(Thread);

            SchedThread.WaitSync.WaitOne();

            TryResumingExecution(SchedThread);
        }

        public bool EnterWait(KThread Thread, int Timeout)
        {
            if (!AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
            {
                throw new InvalidOperationException();
            }

            Suspend(Thread);

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

        public void Suspend(KThread Thread)
        {
            lock (SchedLock)
            {
                PrintDbgThreadInfo(Thread, "suspended.");

                SchedulerThread SchedThread = WaitingToRun.Pop(Thread.ActualCore);

                if (SchedThread != null)
                {
                    RunThread(SchedThread);
                }
                else
                {
                    Log.PrintDebug(LogClass.KernelScheduler, $"Nothing to run on core {Thread.ActualCore}!");

                    RemoveActiveCore(Thread.ActualCore);
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
                    SchedulerThread SchedThread = WaitingToRun.Pop(
                        Thread.ActualCore,
                        Thread.ActualPriority);

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
                Suspend(Thread);
            }

            Resume(Thread);
        }

        public bool TryRunning(KThread Thread)
        {
            if (!AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
            {
                throw new InvalidOperationException();
            }

            lock (SchedLock)
            {
                if (WaitingToRun.HasThread(SchedThread) && AddActiveCore(Thread))
                {
                    WaitingToRun.Remove(SchedThread);

                    RunThread(SchedThread);

                    return true;
                }

                return false;
            }
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
                if (AddActiveCore(Thread))
                {
                    PrintDbgThreadInfo(Thread, "resuming execution...");

                    return;
                }

                WaitingToRun.Push(SchedThread);

                PrintDbgThreadInfo(Thread, "entering wait state...");
            }

            SchedThread.WaitSched.WaitOne();

            PrintDbgThreadInfo(Thread, "resuming execution...");
        }

        private void RunThread(SchedulerThread SchedThread)
        {
            if (!SchedThread.Thread.Thread.Execute())
            {
                PrintDbgThreadInfo(SchedThread.Thread, "waked.");

                SchedThread.WaitSched.Set();
            }
            else
            {
                PrintDbgThreadInfo(SchedThread.Thread, "running.");
            }
        }

        public void Resort(KThread Thread)
        {
            if (AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
            {
                WaitingToRun.Resort(SchedThread);
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

        private bool AddActiveCore(KThread Thread)
        {
            lock (SchedLock)
            {
                //First, try running it on Ideal Core.
                int CoreMask = 1 << Thread.IdealCore;

                if ((ActiveCores & CoreMask) == 0)
                {
                    ActiveCores |= CoreMask;

                    Thread.ActualCore = Thread.IdealCore;

                    return true;
                }

                //If that fails, then try running on any core allowed by Core Mask.
                CoreMask = Thread.CoreMask & ~ActiveCores;

                if (CoreMask != 0)
                {
                    CoreMask &= -CoreMask;

                    ActiveCores |= CoreMask;

                    for (int Bit = 0; Bit < 32; Bit++)
                    {
                        if (((CoreMask >> Bit) & 1) != 0)
                        {
                            Thread.ActualCore = Bit;

                            return true;
                        }
                    }

                    throw new InvalidOperationException();
                }

                return false;
            }
        }

        private void RemoveActiveCore(int Core)
        {
            lock (SchedLock)
            {
                ActiveCores &= ~(1 << Core);
            }
        }

        private void PrintDbgThreadInfo(KThread Thread, string Message)
        {
            Log.PrintDebug(LogClass.KernelScheduler, "(" +
                "ThreadId = "       + Thread.ThreadId       + ", " +
                "ActualCore = "     + Thread.ActualCore     + ", " +
                "IdealCore = "      + Thread.IdealCore      + ", " +
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