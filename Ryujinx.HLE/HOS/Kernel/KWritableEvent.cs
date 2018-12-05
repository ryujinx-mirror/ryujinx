namespace Ryujinx.HLE.HOS.Kernel
{
    class KWritableEvent
    {
        private KEvent Parent;

        public KWritableEvent(KEvent Parent)
        {
            this.Parent = Parent;
        }

        public void Signal()
        {
            Parent.ReadableEvent.Signal();
        }

        public KernelResult Clear()
        {
            return Parent.ReadableEvent.Clear();
        }
    }
}