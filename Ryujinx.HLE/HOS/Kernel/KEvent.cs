namespace Ryujinx.HLE.HOS.Kernel
{
    class KEvent
    {
        public KReadableEvent ReadableEvent { get; }
        public KWritableEvent WritableEvent { get; }

        public KEvent(Horizon system)
        {
            ReadableEvent = new KReadableEvent(system, this);
            WritableEvent = new KWritableEvent(this);
        }
    }
}