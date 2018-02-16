using ChocolArm64.Memory;
using ChocolArm64.State;
using System;
using System.Threading;

namespace ChocolArm64
{
    public class AThread
    {
        public ARegisters  Registers { get; private set; }
        public AMemory     Memory    { get; private set; }

        public long EntryPoint { get; private set; }

        private ATranslator Translator;

        private ThreadPriority Priority;

        private Thread Work;

        public event EventHandler WorkFinished;

        public int ThreadId => Registers.ThreadId;

        public bool IsAlive => Work.IsAlive;

        private bool IsExecuting;

        private object ExecuteLock;

        public AThread(AMemory Memory, ThreadPriority Priority, long EntryPoint)
        {
            this.Memory     = Memory;
            this.Priority   = Priority;
            this.EntryPoint = EntryPoint;

            Registers   = new ARegisters();
            Translator  = new ATranslator(this);
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
                Translator.ExecuteSubroutine(EntryPoint);

                Memory.RemoveMonitor(ThreadId);

                WorkFinished?.Invoke(this, EventArgs.Empty);
            });

            Work.Priority = Priority;

            Work.Start();

            return true;
        }
    }
}