using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gpu.Synchronization;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl.Types;
using Ryujinx.HLE.HOS.Services.Nv.Types;
using Ryujinx.HLE.HOS.Services.Settings;
using Ryujinx.Memory;
using System;
using System.Text;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl
{
    internal class NvHostCtrlDeviceFile : NvDeviceFile
    {
        public const int EventsCount = 64;

        private bool          _isProductionMode;
        private Switch        _device;
        private NvHostEvent[] _events;

        public NvHostCtrlDeviceFile(ServiceCtx context, IVirtualMemoryManager memory, long owner) : base(context, owner)
        {
            if (NxSettings.Settings.TryGetValue("nv!rmos_set_production_mode", out object productionModeSetting))
            {
                _isProductionMode = ((string)productionModeSetting) != "0"; // Default value is ""
            }
            else
            {
                _isProductionMode = true;
            }

            _device = context.Device;

            _events = new NvHostEvent[EventsCount];
        }

        public override NvInternalResult Ioctl(NvIoctl command, Span<byte> arguments)
        {
            NvInternalResult result = NvInternalResult.NotImplemented;

            if (command.Type == NvIoctl.NvHostCustomMagic)
            {
                switch (command.Number)
                {
                    case 0x14:
                        result = CallIoctlMethod<NvFence>(SyncptRead, arguments);
                        break;
                    case 0x15:
                        result = CallIoctlMethod<uint>(SyncptIncr, arguments);
                        break;
                    case 0x16:
                        result = CallIoctlMethod<SyncptWaitArguments>(SyncptWait, arguments);
                        break;
                    case 0x19:
                        result = CallIoctlMethod<SyncptWaitExArguments>(SyncptWaitEx, arguments);
                        break;
                    case 0x1a:
                        result = CallIoctlMethod<NvFence>(SyncptReadMax, arguments);
                        break;
                    case 0x1b:
                        // As Marshal cannot handle unaligned arrays, we do everything by hand here.
                        GetConfigurationArguments configArgument = GetConfigurationArguments.FromSpan(arguments);
                        result = GetConfig(configArgument);

                        if (result == NvInternalResult.Success)
                        {
                            configArgument.CopyTo(arguments);
                        }
                        break;
                    case 0x1c:
                        result = CallIoctlMethod<uint>(EventSignal, arguments);
                        break;
                    case 0x1d:
                        result = CallIoctlMethod<EventWaitArguments>(EventWait, arguments);
                        break;
                    case 0x1e:
                        result = CallIoctlMethod<EventWaitArguments>(EventWaitAsync, arguments);
                        break;
                    case 0x1f:
                        result = CallIoctlMethod<uint>(EventRegister, arguments);
                        break;
                    case 0x20:
                        result = CallIoctlMethod<uint>(EventUnregister, arguments);
                        break;
                    case 0x21:
                        result = CallIoctlMethod<ulong>(EventKill, arguments);
                        break;
                }
            }

            return result;
        }

        private KEvent QueryEvent(uint eventId)
        {
            lock (_events)
            {
                uint eventSlot;
                uint syncpointId;

                if ((eventId >> 28) == 1)
                {
                    eventSlot = eventId & 0xFFFF;
                    syncpointId = (eventId >> 16) & 0xFFF;
                }
                else
                {
                    eventSlot = eventId & 0xFF;
                    syncpointId = eventId >> 4;
                }

                if (eventSlot >= EventsCount || _events[eventSlot] == null || _events[eventSlot].Fence.Id != syncpointId)
                {
                    return null;
                }

                return _events[eventSlot].Event;
            }
        }

        public override NvInternalResult QueryEvent(out int eventHandle, uint eventId)
        {
            KEvent targetEvent = QueryEvent(eventId);

            if (targetEvent != null)
            {
                if (Context.Process.HandleTable.GenerateHandle(targetEvent.ReadableEvent, out eventHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }
            else
            {
                eventHandle = 0;

                return NvInternalResult.InvalidInput;
            }

            return NvInternalResult.Success;
        }

        private NvInternalResult SyncptRead(ref NvFence arguments)
        {
            return SyncptReadMinOrMax(ref arguments, max: false);
        }

        private NvInternalResult SyncptIncr(ref uint id)
        {
            if (id >= SynchronizationManager.MaxHardwareSyncpoints)
            {
                return NvInternalResult.InvalidInput;
            }

            _device.System.HostSyncpoint.Increment(id);

            return NvInternalResult.Success;
        }

        private NvInternalResult SyncptWait(ref SyncptWaitArguments arguments)
        {
            uint dummyValue = 0;

            return EventWait(ref arguments.Fence, ref dummyValue, arguments.Timeout, isWaitEventAsyncCmd: false, isWaitEventCmd: false);
        }

        private NvInternalResult SyncptWaitEx(ref SyncptWaitExArguments arguments)
        {
            return EventWait(ref arguments.Input.Fence, ref arguments.Value, arguments.Input.Timeout, isWaitEventAsyncCmd: false, isWaitEventCmd: false);
        }

        private NvInternalResult SyncptReadMax(ref NvFence arguments)
        {
            return SyncptReadMinOrMax(ref arguments, max: true);
        }

        private NvInternalResult GetConfig(GetConfigurationArguments arguments)
        {
            if (!_isProductionMode && NxSettings.Settings.TryGetValue($"{arguments.Domain}!{arguments.Parameter}".ToLower(), out object nvSetting))
            {
                byte[] settingBuffer = new byte[0x101];

                if (nvSetting is string stringValue)
                {
                    if (stringValue.Length > 0x100)
                    {
                        Logger.Error?.Print(LogClass.ServiceNv, $"{arguments.Domain}!{arguments.Parameter} String value size is too big!");
                    }
                    else
                    {
                        settingBuffer = Encoding.ASCII.GetBytes(stringValue + "\0");
                    }
                }
                else if (nvSetting is int intValue)
                {
                    settingBuffer = BitConverter.GetBytes(intValue);
                }
                else if (nvSetting is bool boolValue)
                {
                    settingBuffer[0] = boolValue ? (byte)1 : (byte)0;
                }
                else
                {
                    throw new NotImplementedException(nvSetting.GetType().Name);
                }

                Logger.Debug?.Print(LogClass.ServiceNv, $"Got setting {arguments.Domain}!{arguments.Parameter}");

                arguments.Configuration = settingBuffer;

                return NvInternalResult.Success;
            }

            // NOTE: This actually return NotAvailableInProduction but this is directly translated as a InvalidInput before returning the ioctl.
            //return NvInternalResult.NotAvailableInProduction;
            return NvInternalResult.InvalidInput;
        }

        private NvInternalResult EventWait(ref EventWaitArguments arguments)
        {
            return EventWait(ref arguments.Fence, ref arguments.Value, arguments.Timeout, isWaitEventAsyncCmd: false, isWaitEventCmd: true);
        }

        private NvInternalResult EventWaitAsync(ref EventWaitArguments arguments)
        {
            return EventWait(ref arguments.Fence, ref arguments.Value, arguments.Timeout, isWaitEventAsyncCmd: true, isWaitEventCmd: false);
        }

        private NvInternalResult EventRegister(ref uint userEventId)
        {
            lock (_events)
            {
                NvInternalResult result = EventUnregister(ref userEventId);

                if (result == NvInternalResult.Success)
                {
                    _events[userEventId] = new NvHostEvent(_device.System.HostSyncpoint, userEventId, _device.System);
                }

                return result;
            }

        }

        private NvInternalResult EventUnregister(ref uint userEventId)
        {
            lock (_events)
            {
                if (userEventId >= EventsCount)
                {
                    return NvInternalResult.InvalidInput;
                }

                NvHostEvent hostEvent = _events[userEventId];

                if (hostEvent == null)
                {
                    return NvInternalResult.Success;
                }

                if (hostEvent.State == NvHostEventState.Available ||
                    hostEvent.State == NvHostEventState.Cancelled ||
                    hostEvent.State == NvHostEventState.Signaled)
                {
                    _events[userEventId].Dispose();
                    _events[userEventId] = null;

                    return NvInternalResult.Success;
                }

                return NvInternalResult.Busy;
            }
        }

        private NvInternalResult EventKill(ref ulong eventMask)
        {
            lock (_events)
            {
                NvInternalResult result = NvInternalResult.Success;

                for (uint eventId = 0; eventId < EventsCount; eventId++)
                {
                    if ((eventMask & (1UL << (int)eventId)) != 0)
                    {
                        NvInternalResult tmp = EventUnregister(ref eventId);

                        if (tmp != NvInternalResult.Success)
                        {
                            result = tmp;
                        }
                    }
                }

                return result;
            }
        }

        private NvInternalResult EventSignal(ref uint userEventId)
        {
            uint eventId = userEventId & ushort.MaxValue;

            if (eventId >= EventsCount)
            {
                return NvInternalResult.InvalidInput;
            }

            lock (_events)
            {
                NvHostEvent hostEvent = _events[eventId];

                if (hostEvent == null)
                {
                    return NvInternalResult.InvalidInput;
                }

                lock (hostEvent.Lock)
                {

                    NvHostEventState oldState = hostEvent.State;

                    if (oldState == NvHostEventState.Waiting)
                    {
                        hostEvent.State = NvHostEventState.Cancelling;

                        hostEvent.Cancel(_device.Gpu);
                    }

                    hostEvent.State = NvHostEventState.Cancelled;

                    _device.System.HostSyncpoint.UpdateMin(hostEvent.Fence.Id);

                    return NvInternalResult.Success;
                }
            }
        }

        private NvInternalResult SyncptReadMinOrMax(ref NvFence arguments, bool max)
        {
            if (arguments.Id >= SynchronizationManager.MaxHardwareSyncpoints)
            {
                return NvInternalResult.InvalidInput;
            }

            if (max)
            {
                arguments.Value = _device.System.HostSyncpoint.ReadSyncpointMaxValue(arguments.Id);
            }
            else
            {
                arguments.Value = _device.System.HostSyncpoint.ReadSyncpointValue(arguments.Id);
            }

            return NvInternalResult.Success;
        }

        private NvInternalResult EventWait(ref NvFence fence, ref uint value, int timeout, bool isWaitEventAsyncCmd, bool isWaitEventCmd)
        {
            if (fence.Id >= SynchronizationManager.MaxHardwareSyncpoints)
            {
                return NvInternalResult.InvalidInput;
            }

            // First try to check if the syncpoint is already expired on the CPU side
            if (_device.System.HostSyncpoint.IsSyncpointExpired(fence.Id, fence.Value))
            {
                value = _device.System.HostSyncpoint.ReadSyncpointMinValue(fence.Id);

                return NvInternalResult.Success;
            }

            // Try to invalidate the CPU cache and check for expiration again.
            uint newCachedSyncpointValue = _device.System.HostSyncpoint.UpdateMin(fence.Id);

            // Has the fence already expired?
            if (_device.System.HostSyncpoint.IsSyncpointExpired(fence.Id, fence.Value))
            {
                value = newCachedSyncpointValue;

                return NvInternalResult.Success;
            }

            // If the timeout is 0, directly return.
            if (timeout == 0)
            {
                return NvInternalResult.TryAgain;
            }

            // The syncpoint value isn't at the fence yet, we need to wait.

            if (!isWaitEventAsyncCmd)
            {
                value = 0;
            }

            NvHostEvent hostEvent;

            NvInternalResult result;

            uint eventIndex;

            lock (_events)
            {
                if (isWaitEventAsyncCmd)
                {
                    eventIndex = value;

                    if (eventIndex >= EventsCount)
                    {
                        return NvInternalResult.InvalidInput;
                    }

                    hostEvent = _events[eventIndex];
                }
                else
                {
                    hostEvent = GetFreeEventLocked(fence.Id, out eventIndex);
                }

                if (hostEvent != null)
                {
                    lock (hostEvent.Lock)
                    {
                        if (hostEvent.State == NvHostEventState.Available ||
                            hostEvent.State == NvHostEventState.Signaled ||
                            hostEvent.State == NvHostEventState.Cancelled)
                        {
                            bool timedOut = hostEvent.Wait(_device.Gpu, fence);

                            if (timedOut)
                            {
                                if (isWaitEventCmd)
                                {
                                    value = ((fence.Id & 0xfff) << 16) | 0x10000000;
                                }
                                else
                                {
                                    value = fence.Id << 4;
                                }

                                value |= eventIndex;

                                result = NvInternalResult.TryAgain;
                            }
                            else
                            {
                                value = fence.Value;

                                return NvInternalResult.Success;
                            }
                        }
                        else
                        {
                            Logger.Error?.Print(LogClass.ServiceNv, $"Invalid Event at index {eventIndex} (isWaitEventAsyncCmd: {isWaitEventAsyncCmd}, isWaitEventCmd: {isWaitEventCmd})");

                            if (hostEvent != null)
                            {
                                Logger.Error?.Print(LogClass.ServiceNv, hostEvent.DumpState(_device.Gpu));
                            }

                            result = NvInternalResult.InvalidInput;
                        }
                    }
                }
                else
                {
                    Logger.Error?.Print(LogClass.ServiceNv, $"Invalid Event at index {eventIndex} (isWaitEventAsyncCmd: {isWaitEventAsyncCmd}, isWaitEventCmd: {isWaitEventCmd})");

                    result = NvInternalResult.InvalidInput;
                }
            }

            return result;
        }

        private NvHostEvent GetFreeEventLocked(uint id, out uint eventIndex)
        {
            eventIndex = EventsCount;

            uint nullIndex = EventsCount;

            for (uint index = 0; index < EventsCount; index++)
            {
                NvHostEvent Event = _events[index];

                if (Event != null)
                {
                    if (Event.State == NvHostEventState.Available ||
                        Event.State == NvHostEventState.Signaled   ||
                        Event.State == NvHostEventState.Cancelled)
                    {
                        eventIndex = index;

                        if (Event.Fence.Id == id)
                        {
                            return Event;
                        }
                    }
                }
                else if (nullIndex == EventsCount)
                {
                    nullIndex = index;
                }
            }

            if (nullIndex < EventsCount)
            {
                eventIndex = nullIndex;

                EventRegister(ref eventIndex);

                return _events[nullIndex];
            }

            if (eventIndex < EventsCount)
            {
                return _events[eventIndex];
            }

            return null;
        }

        public override void Close()
        {
            Logger.Warning?.Print(LogClass.ServiceNv, "Closing channel");

            lock (_events)
            {
                // If the device file need to be closed, cancel all user events and dispose events.
                for (int i = 0; i < _events.Length; i++)
                {
                    NvHostEvent evnt = _events[i];

                    if (evnt != null)
                    {
                        lock (evnt.Lock)
                        {
                            if (evnt.State == NvHostEventState.Waiting)
                            {
                                evnt.State = NvHostEventState.Cancelling;

                                evnt.Cancel(_device.Gpu);
                            }
                            else if (evnt.State == NvHostEventState.Signaling)
                            {
                                // Wait at max 9ms if the guest app is trying to signal the event while closing it..
                                int retryCount = 0;
                                do
                                {
                                    if (retryCount++ > 9)
                                    {
                                        break;
                                    }

                                    // TODO: This should be handled by the kernel (reschedule the current thread ect), waiting for Kernel decoupling work.
                                    Thread.Sleep(1);
                                } while (evnt.State != NvHostEventState.Signaled);
                            }

                            evnt.Dispose();

                            _events[i] = null;
                        }
                    }
                }
            }
        }
    }
}
