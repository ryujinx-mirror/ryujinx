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

        private long EntryPoint;

        private ATranslator Translator;

        private Thread Work;

        public event EventHandler WorkFinished;

        public int ThreadId => ThreadState.ThreadId;

        private int IsExecuting;

        public AThread(ATranslator Translator, AMemory Memory, long EntryPoint)
        {
            this.Translator = Translator;
            this.Memory     = Memory;
            this.EntryPoint = EntryPoint;

            ThreadState = new AThreadState();

            ThreadState.ExecutionMode = AExecutionMode.AArch64;

            ThreadState.Running = true;
        }

        public bool Execute()
        {
            if (Interlocked.Exchange(ref IsExecuting, 1) == 1)
            {
                return false;
            }

            Work = new Thread(delegate()
            {
                Translator.ExecuteSubroutine(this, EntryPoint);

                Memory.RemoveMonitor(ThreadState);

                WorkFinished?.Invoke(this, EventArgs.Empty);
            });

            Work.Start();

            return true;
        }

        public void StopExecution()
        {
            ThreadState.Running = false;
        }

        public bool IsCurrentThread()
        {
            return Thread.CurrentThread == Work;
        }
    }
}