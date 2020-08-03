using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gpu;
using Ryujinx.Graphics.Gpu.Synchronization;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Nv.Types;
using System;
using System.Threading;

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

        private NvFence _previousFailingFence;
        private uint    _failingCount;

        public object Lock = new object();

        /// <summary>
        /// Max failing count until waiting on CPU.
        /// FIXME: This seems enough for most of the cases, reduce if needed.
        /// </summary>
        private const uint FailingCountMax = 2;

        public NvHostEvent(NvHostSyncpt syncpointManager, uint eventId, Horizon system)
        {
            Fence.Id = 0;

            State = NvHostEventState.Available;

            Event = new KEvent(system.KernelContext);

            _eventId = eventId;

            _syncpointManager = syncpointManager;

            ResetFailingState();
        }

        private void ResetFailingState()
        {
            _previousFailingFence.Id    = NvFence.InvalidSyncPointId;
            _previousFailingFence.Value = 0;
            _failingCount               = 0;
        }

        private void Signal()
        {
            lock (Lock)
            {
                NvHostEventState oldState = State;

                State = NvHostEventState.Signaling;

                if (oldState == NvHostEventState.Waiting)
                {
                    Event.WritableEvent.Signal();
                }

                State = NvHostEventState.Signaled;
            }
        }

        private void GpuSignaled()
        {
            lock (Lock)
            {
                ResetFailingState();

                Signal();
            }
        }

        public void Cancel(GpuContext gpuContext)
        {
            lock (Lock)
            {
                if (_waiterInformation != null)
                {
                    gpuContext.Synchronization.UnregisterCallback(Fence.Id, _waiterInformation);

                    if (_previousFailingFence.Id == Fence.Id && _previousFailingFence.Value == Fence.Value)
                    {
                        _failingCount++;
                    }
                    else
                    {
                        _failingCount = 1;

                        _previousFailingFence = Fence;
                    }

                    Signal();
                }

                Event.WritableEvent.Clear();
            }
        }

        public bool Wait(GpuContext gpuContext, NvFence fence)
        {
            lock (Lock)
            {
                Fence = fence;
                State = NvHostEventState.Waiting;

                // NOTE: nvservices code should always wait on the GPU side.
                //       If we do this, we may get an abort or undefined behaviour when the GPU processing thread is blocked for a long period (for example, during shader compilation).
                //       The reason for this is that the NVN code will try to wait until giving up.
                //       This is done by trying to wait and signal multiple times until aborting after you are past the timeout.
                //       As such, if it fails too many time, we enforce a wait on the CPU side indefinitely.
                //       This allows to keep GPU and CPU in sync when we are slow.
                if (_failingCount == FailingCountMax)
                {
                    Logger.Warning?.Print(LogClass.ServiceNv, "GPU processing thread is too slow, waiting on CPU...");

                    bool timedOut = Fence.Wait(gpuContext, Timeout.InfiniteTimeSpan);

                    GpuSignaled();

                    return timedOut;
                }
                else
                {
                    _waiterInformation = gpuContext.Synchronization.RegisterCallbackOnSyncpoint(Fence.Id, Fence.Value, GpuSignaled);

                    return true;
                }
            }
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