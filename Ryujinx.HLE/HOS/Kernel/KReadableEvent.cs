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
            System.CriticalSectionLock.Lock();

            if (!Signaled)
            {
                Signaled = true;

                base.Signal();
            }

            System.CriticalSectionLock.Unlock();
        }

        public KernelResult Clear()
        {
            Signaled = false;

            return KernelResult.Success;
        }

        public KernelResult ClearIfSignaled()
        {
            KernelResult Result;

            System.CriticalSectionLock.Lock();

            if (Signaled)
            {
                Signaled = false;

                Result = KernelResult.Success;
            }
            else
            {
                Result = KernelResult.InvalidState;
            }

            System.CriticalSectionLock.Unlock();

            return Result;
        }

        public override bool IsSignaled()
        {
            return Signaled;
        }
    }
}