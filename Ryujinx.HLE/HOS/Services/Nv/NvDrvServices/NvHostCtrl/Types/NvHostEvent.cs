using Ryujinx.Graphics.Gpu;
using Ryujinx.Graphics.Gpu.Synchronization;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Nv.Types;
using System;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl
{
    class NvHostEvent : IDisposable
    {
        public NvFence          Fence;
        public NvHostEventState State;
        public KEvent           Event;

        private uint                  _eventId;
        private NvHostSyncpt          _syncpointManager;
        private SyncpointWaiterHandle _waiterInformation;

        public NvHostEvent(NvHostSyncpt syncpointManager, uint eventId, Horizon system)
        {
            Fence.Id = 0;

            State = NvHostEventState.Available;

            Event = new KEvent(system);

            _eventId = eventId;

            _syncpointManager = syncpointManager;
        }

        public void Reset()
        {
            Fence.Id    = NvFence.InvalidSyncPointId;
            Fence.Value = 0;
            State       = NvHostEventState.Available;
        }

        private void Signal()
        {
            NvHostEventState oldState = State;

            State = NvHostEventState.Signaling;

            if (oldState == NvHostEventState.Waiting)
            {
                Event.WritableEvent.Signal();
            }

            State = NvHostEventState.Signaled;
        }

        private void GpuSignaled()
        {
            Signal();
        }

        public void Cancel(GpuContext gpuContext)
        {
            if (_waiterInformation != null)
            {
                gpuContext.Synchronization.UnregisterCallback(Fence.Id, _waiterInformation);

                Signal();
            }

            Event.WritableEvent.Clear();
        }

        public void Wait(GpuContext gpuContext, NvFence fence)
        {
            Fence = fence;
            State = NvHostEventState.Waiting;

            _waiterInformation = gpuContext.Synchronization.RegisterCallbackOnSyncpoint(Fence.Id, Fence.Value, GpuSignaled);
        }

        public string DumpState(GpuContext gpuContext)
        {
            string res = $"\nNvHostEvent {_eventId}:\n";
            res += $"\tState: {State}\n";

            if (State == NvHostEventState.Waiting)
            {
                res += "\tFence:\n";
                res += $"\t\tId            : {Fence.Id}\n";
                res += $"\t\tThreshold     : {Fence.Value}\n";
                res += $"\t\tCurrent Value : {gpuContext.Synchronization.GetSyncpointValue(Fence.Id)}\n";
                res += $"\t\tWaiter Valid  : {_waiterInformation != null}\n";
            }

            return res;
        }

        public void Dispose()
        {
            Event.ReadableEvent.DecrementReferenceCount();
            Event.WritableEvent.DecrementReferenceCount();
        }
    }
}