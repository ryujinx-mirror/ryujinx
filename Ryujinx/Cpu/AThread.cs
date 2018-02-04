using ChocolArm64.Memory;
using ChocolArm64.State;
using System;
using System.Threading;

namespace ChocolArm64
{
    class AThread
    {
        public ARegisters  Registers { get; private set; }
        public AMemory     Memory    { get; private set; }

        private ATranslator Translator;
        private Thread      Work;

        public event EventHandler WorkFinished;

        public int ThreadId => Registers.ThreadId;

        public bool IsAlive => Work.IsAlive;

        public long EntryPoint { get; private set; }
        public int  Priority   { get; private set; }

        public AThread(AMemory Memory, long EntryPoint = 0, int Priority = 0)
        {
            this.Memory     = Memory;
            this.EntryPoint = EntryPoint;
            this.Priority   = Priority;

            Registers  = new ARegisters();
            Translator = new ATranslator(this);
        }

        public void StopExecution() => Translator.StopExecution();

        public void Execute() => Execute(EntryPoint);

        public void Execute(long EntryPoint)
        {
            Work = new Thread(delegate()
            {
                Translator.ExecuteSubroutine(EntryPoint);

                Memory.RemoveMonitor(ThreadId);

                WorkFinished?.Invoke(this, EventArgs.Empty);
            });

            if (Priority < 12)
            {
                Work.Priority = ThreadPriority.Highest;
            }
            else if (Priority < 24)
            {
                Work.Priority = ThreadPriority.AboveNormal;
            }
            else if (Priority < 36)
            {
                Work.Priority = ThreadPriority.Normal;
            }
            else if (Priority < 48)
            {
                Work.Priority = ThreadPriority.BelowNormal;
            }
            else
            {
                Work.Priority = ThreadPriority.Lowest;
            }

            Work.Start();
        }
    }
}