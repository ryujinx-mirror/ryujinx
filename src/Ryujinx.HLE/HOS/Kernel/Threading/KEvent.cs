namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    class KEvent
    {
        public KReadableEvent ReadableEvent { get; private set; }
        public KWritableEvent WritableEvent { get; private set; }

        public KEvent(KernelContext context)
        {
            ReadableEvent = new KReadableEvent(context);
            WritableEvent = new KWritableEvent(context, this);
        }
    }
}
