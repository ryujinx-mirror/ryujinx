using Ryujinx.HLE.HOS.Kernel.Process;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    class BufferQueue
    {
        public static void CreateBufferQueue(Switch device, KProcess process, out BufferQueueProducer producer, out BufferQueueConsumer consumer)
        {
            BufferQueueCore core = new BufferQueueCore(device, process);

            producer = new BufferQueueProducer(core);
            consumer = new BufferQueueConsumer(core);
        }
    }
}
