using ChocolArm64.Memory;
using ChocolArm64.State;
using System;
using System.Threading;

namespace ChocolArm64
{
    public class CpuThread
    {
        public CpuThreadState ThreadState { get; private set; }
        public MemoryManager  Memory      { get; private set; }

        private Translator _translator;

        public Thread Work;

        public event EventHandler WorkFinished;

        private int _isExecuting;

        public CpuThread(Translator translator, MemoryManager memory, long entrypoint)
        {
            _translator = translator;
            Memory      = memory;

            ThreadState = new CpuThreadState();

            ThreadState.Running = true;

            Work = new Thread(delegate()
            {
                translator.ExecuteSubroutine(this, entrypoint);

                memory.RemoveMonitor(ThreadState.Core);

                WorkFinished?.Invoke(this, EventArgs.Empty);
            });
        }

        public bool Execute()
        {
            if (Interlocked.Exchange(ref _isExecuting, 1) == 1)
            {
                return false;
            }

            Work.Start();

            return true;
        }

        public void StopExecution()
        {
            ThreadState.Running = false;
        }

        public void RequestInterrupt()
        {
            ThreadState.RequestInterrupt();
        }

        public bool IsCurrentThread()
        {
            return Thread.CurrentThread == Work;
        }
    }
}