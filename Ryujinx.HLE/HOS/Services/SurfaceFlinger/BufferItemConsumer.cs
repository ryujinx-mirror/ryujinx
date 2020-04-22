using Ryujinx.Graphics.Gpu;
using System;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    class BufferItemConsumer : ConsumerBase
    {
        private GpuContext _gpuContext;

        public BufferItemConsumer(Switch device,
            BufferQueueConsumer consumer,
            uint consumerUsage,
            int bufferCount,
            bool controlledByApp,
            IConsumerListener listener = null) : base(consumer, controlledByApp, listener)
        {
            _gpuContext = device.Gpu;

            Status status = Consumer.SetConsumerUsageBits(consumerUsage);

            if (status != Status.Success)
            {
                throw new InvalidOperationException();
            }

            if (bufferCount != -1)
            {
                status = Consumer.SetMaxAcquiredBufferCount(bufferCount);

                if (status != Status.Success)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public Status AcquireBuffer(out BufferItem bufferItem, ulong expectedPresent, bool waitForFence = false)
        {
            lock (Lock)
            {
                Status status = AcquireBufferLocked(out BufferItem tmp, expectedPresent);

                if (status != Status.Success)
                {
                    bufferItem = null;

                    return status;
                }

                // Make sure to clone the object to not temper the real instance.
                bufferItem = (BufferItem)tmp.Clone();

                if (waitForFence)
                {
                    bufferItem.Fence.WaitForever(_gpuContext);
                }

                bufferItem.GraphicBuffer.Set(Slots[bufferItem.Slot].GraphicBuffer);

                return Status.Success;
            }
        }

        public Status ReleaseBuffer(BufferItem bufferItem, ref AndroidFence fence)
        {
            lock (Lock)
            {
                Status result = AddReleaseFenceLocked(bufferItem.Slot, ref bufferItem.GraphicBuffer, ref fence);

                if (result == Status.Success)
                {
                    result = ReleaseBufferLocked(bufferItem.Slot, ref bufferItem.GraphicBuffer);
                }

                return result;
            }
        }

        public Status SetDefaultBufferSize(uint width, uint height)
        {
            lock (Lock)
            {
                return Consumer.SetDefaultBufferSize(width, height);
            }
        }

        public Status SetDefaultBufferFormat(PixelFormat defaultFormat)
        {
            lock (Lock)
            {
                return Consumer.SetDefaultBufferFormat(defaultFormat);
            }
        }
    }
}
