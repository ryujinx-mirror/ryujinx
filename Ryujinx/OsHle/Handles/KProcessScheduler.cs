using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.OsHle.Handles
{
    class KProcessScheduler : IDisposable
    {
        private enum ThreadState
        {
            WaitingToRun,
            WaitingSignal,
            Running
        }

        private class SchedulerThread : IDisposable
        {
            public bool Signaled { get; set; }

            public ThreadState State { get; set; }

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

        private Dictionary<HThread, SchedulerThread> AllThreads;

        private Queue<SchedulerThread>[] WaitingThreads;

        private HashSet<int> ActiveProcessors;

        private object SchedLock;

        public KProcessScheduler()
        {
            AllThreads = new Dictionary<HThread, SchedulerThread>();

            WaitingThreads = new Queue<SchedulerThread>[4];

            for (int Index = 0; Index < WaitingThreads.Length; Index++)
            {
                WaitingThreads[Index] = new Queue<SchedulerThread>();
            }

            ActiveProcessors = new HashSet<int>();

            SchedLock = new object();
        }

        public void StartThread(HThread Thread)
        {
            lock (SchedLock)
            {
                if (AllThreads.ContainsKey(Thread))
                {
                    return;
                }

                SchedulerThread SchedThread = new SchedulerThread(Thread);

                AllThreads.Add(Thread, SchedThread);

                if (!ActiveProcessors.Contains(Thread.ProcessorId))
                {
                    ActiveProcessors.Add(Thread.ProcessorId);

                    Thread.Thread.Execute();

                    SetThreadAsRunning(SchedThread);

                    SchedThread.State = ThreadState.Running;
                }
                else
                {
                    InsertSorted(SchedThread);

                    SchedThread.State = ThreadState.WaitingToRun;

                    Logging.Debug($"{GetDbgThreadInfo(SchedThread.Thread)} waiting to run.");
                }
            }
        }

        public void WaitForSignal(HThread Thread, int TimeoutMs)
        {
            Logging.Debug($"{GetDbgThreadInfo(Thread)} entering signal wait state with timeout.");

            PutThreadToWait(Thread, ThreadState.WaitingSignal, TimeoutMs);
        }

        public void WaitForSignal(HThread Thread)
        {
            Logging.Debug($"{GetDbgThreadInfo(Thread)} entering signal wait state.");

            PutThreadToWait(Thread, ThreadState.WaitingSignal);
        }

        public void Yield(HThread Thread)
        {
            Logging.Debug($"{GetDbgThreadInfo(Thread)} yielded execution.");

            if (WaitingThreads[Thread.ProcessorId].Count == 0)
            {
                Logging.Debug($"{GetDbgThreadInfo(Thread)} resumed because theres nothing to run.");

                return;
            }            

            PutThreadToWait(Thread, ThreadState.WaitingToRun);
        }

        private void PutThreadToWait(HThread Thread, ThreadState State, int TimeoutMs = -1)
        {
            SchedulerThread SchedThread;

            lock (SchedLock)
            {
                if (!AllThreads.TryGetValue(Thread, out SchedThread))
                {
                    return;
                }

                if (SchedThread.Signaled && SchedThread.State == ThreadState.WaitingSignal)
                {
                    SchedThread.Signaled = false;

                    return;
                }

                ActiveProcessors.Remove(Thread.ProcessorId);

                SchedThread.State = State;

                TryRunningWaitingThead(SchedThread.Thread.ProcessorId);

                if (State == ThreadState.WaitingSignal)
                {
                    InsertSorted(SchedThread);
                }
                else
                {
                    InsertAtEnd(SchedThread);
                }
            }

            if (TimeoutMs >= 0)
            {
                Logging.Debug($"{GetDbgThreadInfo(SchedThread.Thread)} waiting with timeout of {TimeoutMs}ms.");

                SchedThread.WaitEvent.WaitOne(TimeoutMs);
            }
            else
            {
                Logging.Debug($"{GetDbgThreadInfo(SchedThread.Thread)} waiting indefinitely.");

                SchedThread.WaitEvent.WaitOne();
            }

            while (true)
            {
                lock (SchedLock)
                {
                    Logging.Debug($"Trying to run {GetDbgThreadInfo(SchedThread.Thread)}.");

                    if (!ActiveProcessors.Contains(SchedThread.Thread.ProcessorId))
                    {
                        SetThreadAsRunning(SchedThread);

                        break;
                    }
                    else
                    {
                        SchedThread.State = ThreadState.WaitingToRun;

                        Logging.Debug($"{GetDbgThreadInfo(SchedThread.Thread)} waiting to run.");
                    }
                }

                SchedThread.WaitEvent.WaitOne();
            }
        }

        public void Signal(params HThread[] Threads)
        {
            lock (SchedLock)
            {
                HashSet<int> SignaledProcessorIds = new HashSet<int>();

                foreach (HThread Thread in Threads)
                {
                    Logging.Debug($"{GetDbgThreadInfo(Thread)} signaled.");

                    if (AllThreads.TryGetValue(Thread, out SchedulerThread SchedThread))
                    {
                        if (SchedThread.State == ThreadState.WaitingSignal)
                        {
                            SchedThread.State = ThreadState.WaitingToRun;

                            SignaledProcessorIds.Add(Thread.ProcessorId);
                        }

                        SchedThread.Signaled = true;
                    }
                }

                foreach (int ProcessorId in SignaledProcessorIds)
                {
                    TryRunningWaitingThead(ProcessorId);
                }
            }
        }

        private void TryRunningWaitingThead(int ProcessorId)
        {
            Logging.Debug($"TryRunningWaitingThead core {ProcessorId}.");

            lock (SchedLock)
            {
                if (!ActiveProcessors.Contains(ProcessorId) && WaitingThreads[ProcessorId].Count > 0)
                {
                    SchedulerThread SchedThread = WaitingThreads[ProcessorId].Dequeue();

                    Logging.Debug($"Now trying to run {GetDbgThreadInfo(SchedThread.Thread)}.");

                    if (!SchedThread.Thread.Thread.Execute())
                    {
                        SchedThread.WaitEvent.Set();
                    }
                    else
                    {
                        SetThreadAsRunning(SchedThread);
                    }
                }
                else
                {
                    Logging.Debug($"Processor id {ProcessorId} already being used or no waiting threads.");
                }
            }
        }

        private void SetThreadAsRunning(SchedulerThread SchedThread)
        {
            ActiveProcessors.Add(SchedThread.Thread.ProcessorId);

            SchedThread.State = ThreadState.Running;

            SchedThread.Signaled = false;

            Logging.Debug($"{GetDbgThreadInfo(SchedThread.Thread)} running.");
        }

        private void InsertSorted(SchedulerThread SchedThread)
        {
            HThread Thread = SchedThread.Thread;

            Queue<SchedulerThread> CoreQueue = WaitingThreads[Thread.ProcessorId];

            Queue<SchedulerThread> TempQueue = new Queue<SchedulerThread>(CoreQueue.Count);

            while (CoreQueue.Count > 0)
            {
                if (CoreQueue.Peek().Thread.Priority >= Thread.Priority)
                {
                    break;
                }

                TempQueue.Enqueue(CoreQueue.Dequeue());
            }

            CoreQueue.Enqueue(SchedThread);

            while (CoreQueue.Count > 0)
            {
                TempQueue.Enqueue(CoreQueue.Dequeue());
            }

            while (TempQueue.Count > 0)
            {
                CoreQueue.Enqueue(TempQueue.Dequeue());
            }
        }

        private void InsertAtEnd(SchedulerThread SchedThread)
        {
            WaitingThreads[SchedThread.Thread.ProcessorId].Enqueue(SchedThread);
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
                foreach (Queue<SchedulerThread> SchedThreads in WaitingThreads)
                {
                    foreach (SchedulerThread SchedThread in SchedThreads)
                    {
                        SchedThread.Dispose();
                    }

                    SchedThreads.Clear();
                }
            }
        }
    }
}