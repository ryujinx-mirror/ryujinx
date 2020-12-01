namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    static class BufferQueue
    {
        public static BufferQueueCore CreateBufferQueue(Switch device, long pid, out BufferQueueProducer producer, out BufferQueueConsumer consumer)
        {
            BufferQueueCore core = new BufferQueueCore(device, pid);

            producer = new BufferQueueProducer(core);
            consumer = new BufferQueueConsumer(core);

            return core;
        }
    }
}
