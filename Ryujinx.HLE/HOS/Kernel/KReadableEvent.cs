namespace Ryujinx.HLE.HOS.Kernel
{
    class KReadableEvent : KSynchronizationObject
    {
        private KEvent Parent;

        private bool Signaled;

        public KReadableEvent(Horizon System, KEvent Parent) : base(System)
        {
            this.Parent = Parent;
        }

        public override void Signal()
        {
            System.CriticalSection.Enter();

            if (!Signaled)
            {
                Signaled = true;

                base.Signal();
            }

            System.CriticalSection.Leave();
        }

        public KernelResult Clear()
        {
            Signaled = false;

            return KernelResult.Success;
        }

        public KernelResult ClearIfSignaled()
        {
            KernelResult Result;

            System.CriticalSection.Enter();

            if (Signaled)
            {
                Signaled = false;

                Result = KernelResult.Success;
            }
            else
            {
                Result = KernelResult.InvalidState;
            }

            System.CriticalSection.Leave();

            return Result;
        }

        public override bool IsSignaled()
        {
            return Signaled;
        }
    }
}