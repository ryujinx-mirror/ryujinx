using Ryujinx.HLE.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ryujinx.HLE.OsHle.Handles
{
    class KProcessScheduler : IDisposable
    {
        private ConcurrentDictionary<KThread, SchedulerThread> AllThreads;

        private ThreadQueue WaitingToRun;

        private KThread[] CoreThreads;

        private bool[] CoreReschedule;

        private object SchedLock;

        private Logger Log;

        public KProcessScheduler(Logger Log)
        {
            this.Log = Log;

            AllThreads = new ConcurrentDictionary<KThread, SchedulerThread>();

            WaitingToRun = new ThreadQueue();

            CoreThreads = new KThread[4];

            CoreReschedule = new bool[4];

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

                if (TryAddToCore(Thread))
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

                int ActualCore = Thread.ActualCore;

                SchedulerThread NewThread = WaitingToRun.Pop(ActualCore);

                if (NewThread == null)
                {
                    Log.PrintDebug(LogClass.KernelScheduler, $"Nothing to run on core {ActualCore}!");

                    CoreThreads[ActualCore] = null;

                    return;
                }

                NewThread.Thread.ActualCore = ActualCore;

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

        public void EnterWait(KThread Thread, int TimeoutMs = Timeout.Infinite)
        {
            SchedulerThread SchedThread = AllThreads[Thread];

            Suspend(Thread);

            SchedThread.WaitSync.WaitOne(TimeoutMs);

            TryResumingExecution(SchedThread);
        }

        public void WakeUp(KThread Thread)
        {
            AllThreads[Thread].WaitSync.Set();
        }

        public void TryToRun(KThread Thread)
        {
            lock (SchedLock)
            {
                if (AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
                {
                    if (WaitingToRun.HasThread(SchedThread) && TryAddToCore(Thread))
                    {
                        RunThread(SchedThread);
                    }
                    else
                    {
                        SetReschedule(Thread.ProcessorId);
                    }
                }
            }
        }

        public void Suspend(KThread Thread)
        {
            lock (SchedLock)
            {
                PrintDbgThreadInfo(Thread, "suspended.");

                int ActualCore = Thread.ActualCore;

                CoreReschedule[ActualCore] = false;

                SchedulerThread SchedThread = WaitingToRun.Pop(ActualCore);

                if (SchedThread != null)
                {
                    SchedThread.Thread.ActualCore = ActualCore;

                    CoreThreads[ActualCore] = SchedThread.Thread;

                    RunThread(SchedThread);
                }
                else
                {
                    Log.PrintDebug(LogClass.KernelScheduler, $"Nothing to run on core {Thread.ActualCore}!");

                    CoreThreads[ActualCore] = null;
                }
            }
        }

        public void SetReschedule(int Core)
        {
            lock (SchedLock)
            {
                CoreReschedule[Core] = true;
            }
        }

        public void Reschedule(KThread Thread)
        {
            bool NeedsReschedule;

            lock (SchedLock)
            {
                int ActualCore = Thread.ActualCore;

                NeedsReschedule = CoreReschedule[ActualCore];

                CoreReschedule[ActualCore] = false;
            }

            if (NeedsReschedule)
            {
                Yield(Thread, Thread.ActualPriority - 1);
            }
        }

        public void Yield(KThread Thread)
        {
            Yield(Thread, Thread.ActualPriority);
        }

        private void Yield(KThread Thread, int MinPriority)
        {
            PrintDbgThreadInfo(Thread, "yielded execution.");

            lock (SchedLock)
            {
                int ActualCore = Thread.ActualCore;

                SchedulerThread NewThread = WaitingToRun.Pop(ActualCore, MinPriority);

                if (NewThread == null)
                {
                    PrintDbgThreadInfo(Thread, "resumed because theres nothing better to run.");

                    return;
                }

                NewThread.Thread.ActualCore = ActualCore;

                CoreThreads[ActualCore] = NewThread.Thread;

                RunThread(NewThread);
            }

            Resume(Thread);
        }

        public void Resume(KThread Thread)
        {
            TryResumingExecution(AllThreads[Thread]);
        }

        private void TryResumingExecution(SchedulerThread SchedThread)
        {
            KThread Thread = SchedThread.Thread;

            PrintDbgThreadInfo(Thread, "trying to resume...");

            SchedThread.WaitActivity.WaitOne();

            lock (SchedLock)
            {
                if (TryAddToCore(Thread))
                {
                    PrintDbgThreadInfo(Thread, "resuming execution...");

                    return;
                }

                WaitingToRun.Push(SchedThread);

                SetReschedule(Thread.ProcessorId);

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

        private bool TryAddToCore(KThread Thread)
        {
            //First, try running it on Ideal Core.
            int IdealCore = Thread.IdealCore;

            if (IdealCore != -1 && CoreThreads[IdealCore] == null)
            {
                Thread.ActualCore = IdealCore;

                CoreThreads[IdealCore] = Thread;

                return true;
            }

            //If that fails, then try running on any core allowed by Core Mask.
            int CoreMask = Thread.CoreMask;

            for (int Core = 0; Core < CoreThreads.Length; Core++, CoreMask >>= 1)
            {
                if ((CoreMask & 1) != 0 && CoreThreads[Core] == null)
                {
                    Thread.ActualCore = Core;

                    CoreThreads[Core] = Thread;

                    return true;
                }
            }

            return false;
        }

        private void PrintDbgThreadInfo(KThread Thread, string Message)
        {
            Log.PrintDebug(LogClass.KernelScheduler, "(" +
                "ThreadId = "       + Thread.ThreadId                + ", " +
                "CoreMask = 0x"     + Thread.CoreMask.ToString("x1") + ", " +
                "ActualCore = "     + Thread.ActualCore              + ", " +
                "IdealCore = "      + Thread.IdealCore               + ", " +
                "ActualPriority = " + Thread.ActualPriority          + ", " +
                "WantedPriority = " + Thread.WantedPriority          + ") " + Message);
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