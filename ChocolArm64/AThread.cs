using ChocolArm64.Memory;
using ChocolArm64.State;
using System;
using System.Threading;

namespace ChocolArm64
{
    public class AThread
    {
        public AThreadState ThreadState { get; private set; }
        public AMemory      Memory      { get; private set; }

        private ATranslator Translator;

        public Thread Work;

        public event EventHandler WorkFinished;

        private int IsExecuting;

        public AThread(ATranslator Translator, AMemory Memory, long EntryPoint)
        {
            this.Translator = Translator;
            this.Memory     = Memory;

            ThreadState = new AThreadState();

            ThreadState.ExecutionMode = AExecutionMode.AArch64;

            ThreadState.Running = true;

            Work = new Thread(delegate()
            {
                Translator.ExecuteSubroutine(this, EntryPoint);

                Memory.RemoveMonitor(ThreadState.Core);

                WorkFinished?.Invoke(this, EventArgs.Empty);
            });
        }

        public bool Execute()
        {
            if (Interlocked.Exchange(ref IsExecuting, 1) == 1)
            {
                return false;
            }

            Work.Name = "cpu_thread_" + Work.ManagedThreadId;

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