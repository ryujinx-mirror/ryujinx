using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.SurfaceFlinger.Types;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using System;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    class BufferQueueConsumer
    {
        public BufferQueueCore Core { get; }

        public BufferQueueConsumer(BufferQueueCore core)
        {
            Core = core;
        }

        public Status AcquireBuffer(out BufferItem bufferItem, ulong expectedPresent)
        {
            lock (Core.Lock)
            {
                int numAcquiredBuffers = 0;

                for (int i = 0; i < Core.MaxBufferCountCached; i++)
                {
                    if (Core.Slots[i].BufferState == BufferState.Acquired)
                    {
                        numAcquiredBuffers++;
                    }
                }

                if (numAcquiredBuffers > Core.MaxAcquiredBufferCount)
                {
                    bufferItem = null;

                    Logger.Debug?.Print(LogClass.SurfaceFlinger, $"Max acquired buffer count reached: {numAcquiredBuffers} (max: {Core.MaxAcquiredBufferCount})");

                    return Status.InvalidOperation;
                }

                if (Core.Queue.Count == 0)
                {
                    bufferItem = null;

                    return Status.NoBufferAvailaible;
                }

                if (expectedPresent != 0)
                {
                    // TODO: support this for advanced presenting.
                    throw new NotImplementedException();
                }

                bufferItem = Core.Queue[0];

                if (Core.StillTracking(ref bufferItem))
                {
                    Core.Slots[bufferItem.Slot].AcquireCalled         = true;
                    Core.Slots[bufferItem.Slot].NeedsCleanupOnRelease = true;
                    Core.Slots[bufferItem.Slot].BufferState           = BufferState.Acquired;
                    Core.Slots[bufferItem.Slot].Fence                 = AndroidFence.NoFence;

                    ulong targetFrameNumber = Core.Slots[bufferItem.Slot].FrameNumber;

                    for (int i = 0; i < Core.BufferHistory.Length; i++)
                    {
                        if (Core.BufferHistory[i].FrameNumber == targetFrameNumber)
                        {
                            Core.BufferHistory[i].State = BufferState.Acquired;

                            break;
                        }
                    }
                }

                if (bufferItem.AcquireCalled)
                {
                    bufferItem.GraphicBuffer.Reset();
                }

                Core.Queue.RemoveAt(0);

                Core.CheckSystemEventsLocked(Core.GetMaxBufferCountLocked(true));
                Core.SignalDequeueEvent();
            }

            return Status.Success;
        }

        public Status DetachBuffer(int slot)
        {
            lock (Core.Lock)
            {
                if (Core.IsAbandoned)
                {
                    return Status.NoInit;
                }

                if (slot < 0 || slot >= Core.Slots.Length || !Core.IsOwnedByConsumerLocked(slot))
                {
                    return Status.BadValue;
                }

                if (!Core.Slots[slot].RequestBufferCalled)
                {
                    Logger.Error?.Print(LogClass.SurfaceFlinger, $"Slot {slot} was detached without requesting a buffer");

                    return Status.BadValue;
                }

                Core.FreeBufferLocked(slot);
                Core.SignalDequeueEvent();

                return Status.Success;
            }
        }

        public Status AttachBuffer(out int slot, ref AndroidStrongPointer<GraphicBuffer> graphicBuffer)
        {
            lock (Core.Lock)
            {
                int numAcquiredBuffers = 0;

                int freeSlot = BufferSlotArray.InvalidBufferSlot;

                for (int i = 0; i < Core.Slots.Length; i++)
                {
                    if (Core.Slots[i].BufferState == BufferState.Acquired)
                    {
                        numAcquiredBuffers++;
                    }
                    else if (Core.Slots[i].BufferState == BufferState.Free)
                    {
                        if (freeSlot == BufferSlotArray.InvalidBufferSlot || Core.Slots[i].FrameNumber < Core.Slots[freeSlot].FrameNumber)
                        {
                            freeSlot = i;
                        }
                    }
                }

                if (numAcquiredBuffers > Core.MaxAcquiredBufferCount + 1)
                {
                    slot = BufferSlotArray.InvalidBufferSlot;

                    Logger.Error?.Print(LogClass.SurfaceFlinger, $"Max acquired buffer count reached: {numAcquiredBuffers} (max: {Core.MaxAcquiredBufferCount})");

                    return Status.InvalidOperation;
                }

                if (freeSlot == BufferSlotArray.InvalidBufferSlot)
                {
                    slot = BufferSlotArray.InvalidBufferSlot;

                    return Status.NoMemory;
                }

                Core.UpdateMaxBufferCountCachedLocked(freeSlot);

                slot = freeSlot;

                Core.Slots[slot].GraphicBuffer.Set(graphicBuffer);

                Core.Slots[slot].BufferState           = BufferState.Acquired;
                Core.Slots[slot].AttachedByConsumer    = true;
                Core.Slots[slot].NeedsCleanupOnRelease = false;
                Core.Slots[slot].Fence                 = AndroidFence.NoFence;
                Core.Slots[slot].FrameNumber           = 0;
                Core.Slots[slot].AcquireCalled         = false;
            }

            return Status.Success;
        }

        public Status ReleaseBuffer(int slot, ulong frameNumber, ref AndroidFence fence)
        {
            if (slot < 0 || slot >= Core.Slots.Length)
            {
                return Status.BadValue;
            }

            IProducerListener listener = null;

            lock (Core.Lock)
            {
                if (Core.Slots[slot].FrameNumber != frameNumber)
                {
                    return Status.StaleBufferSlot;
                }

                foreach (BufferItem item in Core.Queue)
                {
                    if (item.Slot == slot)
                    {
                        return Status.BadValue;
                    }
                }

                if (Core.Slots[slot].BufferState == BufferState.Acquired)
                {
                    Core.Slots[slot].BufferState = BufferState.Free;
                    Core.Slots[slot].Fence       = fence;

                    listener = Core.ProducerListener;
                }
                else if (Core.Slots[slot].NeedsCleanupOnRelease)
                {
                    Core.Slots[slot].NeedsCleanupOnRelease = false;

                    return Status.StaleBufferSlot;
                }
                else
                {
                    return Status.BadValue;
                }

                Core.Slots[slot].GraphicBuffer.Object.DecrementNvMapHandleRefCount(Core.Owner);

                Core.CheckSystemEventsLocked(Core.GetMaxBufferCountLocked(true));
                Core.SignalDequeueEvent();
            }

            listener?.OnBufferReleased();

            return Status.Success;
        }

        public Status Connect(IConsumerListener consumerListener, bool controlledByApp)
        {
            if (consumerListener == null)
            {
                return Status.BadValue;
            }

            lock (Core.Lock)
            {
                if (Core.IsAbandoned)
                {
                    return Status.NoInit;
                }

                Core.ConsumerListener        = consumerListener;
                Core.ConsumerControlledByApp = controlledByApp;
            }

            return Status.Success;
        }

        public Status Disconnect()
        {
            lock (Core.Lock)
            {
                if (!Core.IsConsumerConnectedLocked())
                {
                    return Status.BadValue;
                }

                Core.IsAbandoned      = true;
                Core.ConsumerListener = null;

                Core.Queue.Clear();
                Core.FreeAllBuffersLocked();
                Core.SignalDequeueEvent();
            }

            return Status.Success;
        }

        public Status GetReleasedBuffers(out ulong slotMask)
        {
            slotMask = 0;

            lock (Core.Lock)
            {
                if (Core.IsAbandoned)
                {
                    return Status.BadValue;
                }

                for (int slot = 0; slot < Core.Slots.Length; slot++)
                {
                    if (!Core.Slots[slot].AcquireCalled)
                    {
                        slotMask |= 1UL << slot;
                    }
                }

                for (int i = 0; i < Core.Queue.Count; i++)
                {
                    if (Core.Queue[i].AcquireCalled)
                    {
                        slotMask &= ~(1UL << i);
                    }
                }
            }

            return Status.Success;
        }

        public Status SetDefaultBufferSize(uint width, uint height)
        {
            if (width == 0 || height == 0)
            {
                return Status.BadValue;
            }

            lock (Core.Lock)
            {
                Core.DefaultWidth  = (int)width;
                Core.DefaultHeight = (int)height;
            }

            return Status.Success;
        }

        public Status SetDefaultMaxBufferCount(int bufferMaxCount)
        {
            lock (Core.Lock)
            {
                return Core.SetDefaultMaxBufferCountLocked(bufferMaxCount);
            }
        }

        public Status DisableAsyncBuffer()
        {
            lock (Core.Lock)
            {
                if (Core.IsConsumerConnectedLocked())
                {
                    return Status.InvalidOperation;
                }

                Core.UseAsyncBuffer = false;
            }

            return Status.Success;
        }

        public Status SetMaxAcquiredBufferCount(int maxAcquiredBufferCount)
        {
            if (maxAcquiredBufferCount < 0 || maxAcquiredBufferCount > BufferSlotArray.MaxAcquiredBuffers)
            {
                return Status.BadValue;
            }

            lock (Core.Lock)
            {
                if (Core.IsProducerConnectedLocked())
                {
                    return Status.InvalidOperation;
                }

                Core.MaxAcquiredBufferCount = maxAcquiredBufferCount;
            }

            return Status.Success;
        }

        public Status SetDefaultBufferFormat(PixelFormat defaultFormat)
        {
            lock (Core.Lock)
            {
                Core.DefaultBufferFormat = defaultFormat;
            }

            return Status.Success;
        }

        public Status SetConsumerUsageBits(uint usage)
        {
            lock (Core.Lock)
            {
                Core.ConsumerUsageBits = usage;
            }

            return Status.Success;
        }

        public Status SetTransformHint(NativeWindowTransform transformHint)
        {
            lock (Core.Lock)
            {
                Core.TransformHint = transformHint;
            }

            return Status.Success;
        }

        public Status SetPresentTime(int slot, ulong frameNumber, TimeSpanType presentationTime)
        {
            if (slot < 0 || slot >= Core.Slots.Length)
            {
                return Status.BadValue;
            }

            lock (Core.Lock)
            {
                if (Core.Slots[slot].FrameNumber != frameNumber)
                {
                    return Status.StaleBufferSlot;
                }

                if (Core.Slots[slot].PresentationTime.NanoSeconds == 0)
                {
                    Core.Slots[slot].PresentationTime = presentationTime;
                }

                for (int i = 0; i < Core.BufferHistory.Length; i++)
                {
                    if (Core.BufferHistory[i].FrameNumber == frameNumber)
                    {
                        Core.BufferHistory[i].PresentationTime = presentationTime;

                        break;
                    }
                }
            }

            return Status.Success;
        }
    }
}
