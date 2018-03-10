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

        private ThreadPriority Priority;

        private Thread Work;

        public event EventHandler WorkFinished;

        public int ThreadId => ThreadState.ThreadId;

        public bool IsAlive => Work.IsAlive;

        private bool IsExecuting;

        private object ExecuteLock;

        public AThread(ATranslator Translator, AMemory Memory, ThreadPriority Priority, long EntryPoint)
        {
            this.Translator = Translator;
            this.Memory     = Memory;
            this.Priority   = Priority;
            this.EntryPoint = EntryPoint;

            ThreadState = new AThreadState();
            ExecuteLock = new object();
        }

        public void StopExecution() => Translator.StopExecution();

        public bool Execute()
        {
            lock (ExecuteLock)
            {
                if (IsExecuting)
                {
                    return false;
                }

                IsExecuting = true;
            }

            Work = new Thread(delegate()
            {
                Translator.ExecuteSubroutine(this, EntryPoint);

                Memory.RemoveMonitor(ThreadId);

                WorkFinished?.Invoke(this, EventArgs.Empty);
            });

            Work.Priority = Priority;

            Work.Start();

            return true;
        }
    }
}