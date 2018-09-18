namespace Ryujinx.HLE.HOS.Kernel
{
    class KEvent : KSynchronizationObject
    {
        private bool Signaled;

        public string Name { get; private set; }

        public KEvent(Horizon System, string Name = "") : base(System)
        {
            this.Name = Name;
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

        public void Reset()
        {
            Signaled = false;
        }

        public override bool IsSignaled()
        {
            return Signaled;
        }
    }
}