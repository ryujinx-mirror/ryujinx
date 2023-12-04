namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    static class BufferQueue
    {
        public static BufferQueueCore CreateBufferQueue(Switch device, ulong pid, out BufferQueueProducer producer, out BufferQueueConsumer consumer)
        {
            BufferQueueCore core = new(device, pid);

            producer = new BufferQueueProducer(core, device.System.TickSource);
            consumer = new BufferQueueConsumer(core);

            return core;
        }
    }
}
