using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl.Types;
using Ryujinx.HLE.HOS.Services.Nv.Types;
using Ryujinx.HLE.HOS.Services.Settings;

using System;
using System.Text;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl
{
    internal class NvHostCtrlDeviceFile : NvDeviceFile
    {
        private const int EventsCount = 64;

        private bool          _isProductionMode;
        private NvHostSyncpt  _syncpt;
        private NvHostEvent[] _events;
        private KEvent        _dummyEvent;

        public NvHostCtrlDeviceFile(ServiceCtx context) : base(context)
        {
            if (NxSettings.Settings.TryGetValue("nv!rmos_set_production_mode", out object productionModeSetting))
            {
                _isProductionMode = ((string)productionModeSetting) != "0"; // Default value is ""
            }
            else
            {
                _isProductionMode = true;
            }

            _syncpt     = new NvHostSyncpt();
            _events     = new NvHostEvent[EventsCount];
            _dummyEvent = new KEvent(context.Device.System);
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
                    case 0x1d:
                        result = CallIoctlMethod<EventWaitArguments>(EventWait, arguments);
                        break;
                    case 0x1e:
                        result = CallIoctlMethod<EventWaitArguments>(EventWaitAsync, arguments);
                        break;
                    case 0x1f:
                        result = CallIoctlMethod<uint>(EventRegister, arguments);
                        break;
                }
            }

            return result;
        }

        public override NvInternalResult QueryEvent(out int eventHandle, uint eventId)
        {
            // TODO: implement SyncPts <=> KEvent logic accurately. For now we return a dummy event.
            KEvent targetEvent = _dummyEvent;

            if (targetEvent != null)
            {
                if (Owner.HandleTable.GenerateHandle(targetEvent.ReadableEvent, out eventHandle) != KernelResult.Success)
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
            if (id >= NvHostSyncpt.SyncptsCount)
            {
                return NvInternalResult.InvalidInput;
            }

            _syncpt.Increment((int)id);

            return NvInternalResult.Success;
        }

        private NvInternalResult SyncptWait(ref SyncptWaitArguments arguments)
        {
            return SyncptWait(ref arguments, out _);
        }

        private NvInternalResult SyncptWaitEx(ref SyncptWaitExArguments arguments)
        {
            return SyncptWait(ref arguments.Input, out arguments.Value);
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
                        Logger.PrintError(LogClass.ServiceNv, $"{arguments.Domain}!{arguments.Parameter} String value size is too big!");
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

                Logger.PrintDebug(LogClass.ServiceNv, $"Got setting {arguments.Domain}!{arguments.Parameter}");

                arguments.Configuration = settingBuffer;

                return NvInternalResult.Success;
            }

            // NOTE: This actually return NotAvailableInProduction but this is directly translated as a InvalidInput before returning the ioctl.
            //return NvInternalResult.NotAvailableInProduction;
            return NvInternalResult.InvalidInput;
        }

        private NvInternalResult EventWait(ref EventWaitArguments arguments)
        {
            return EventWait(ref arguments, async: false);
        }

        private NvInternalResult EventWaitAsync(ref EventWaitArguments arguments)
        {
            return EventWait(ref arguments, async: true);
        }

        private NvInternalResult EventRegister(ref uint userEventId)
        {
            Logger.PrintStub(LogClass.ServiceNv);

            return NvInternalResult.Success;
        }

        private NvInternalResult SyncptReadMinOrMax(ref NvFence arguments, bool max)
        {
            if (arguments.Id >= NvHostSyncpt.SyncptsCount)
            {
                return NvInternalResult.InvalidInput;
            }

            if (max)
            {
                arguments.Value = (uint)_syncpt.GetMax((int)arguments.Id);
            }
            else
            {
                arguments.Value = (uint)_syncpt.GetMin((int)arguments.Id);
            }

            return NvInternalResult.Success;
        }

        private NvInternalResult SyncptWait(ref SyncptWaitArguments arguments, out int value)
        {
            if (arguments.Id >= NvHostSyncpt.SyncptsCount)
            {
                value = 0;

                return NvInternalResult.InvalidInput;
            }

            NvInternalResult result;

            if (_syncpt.MinCompare((int)arguments.Id, arguments.Thresh))
            {
                result = NvInternalResult.Success;
            }
            else if (arguments.Timeout == 0)
            {
                result = NvInternalResult.TryAgain;
            }
            else
            {
                Logger.PrintDebug(LogClass.ServiceNv, $"Waiting syncpt with timeout of {arguments.Timeout}ms...");

                using (ManualResetEvent waitEvent = new ManualResetEvent(false))
                {
                    _syncpt.AddWaiter(arguments.Thresh, waitEvent);

                    // Note: Negative (> INT_MAX) timeouts aren't valid on .NET,
                    // in this case we just use the maximum timeout possible.
                    int timeout = arguments.Timeout;

                    if (timeout < -1)
                    {
                        timeout = int.MaxValue;
                    }

                    if (timeout == -1)
                    {
                        waitEvent.WaitOne();

                        result = NvInternalResult.Success;
                    }
                    else if (waitEvent.WaitOne(timeout))
                    {
                        result = NvInternalResult.Success;
                    }
                    else
                    {
                        result = NvInternalResult.TimedOut;
                    }
                }

                Logger.PrintDebug(LogClass.ServiceNv, "Resuming...");
            }

            value = _syncpt.GetMin((int)arguments.Id);

            return result;
        }

        private NvInternalResult EventWait(ref EventWaitArguments arguments, bool async)
        {
            if (arguments.Id >= NvHostSyncpt.SyncptsCount)
            {
                return NvInternalResult.InvalidInput;
            }

            if (_syncpt.MinCompare(arguments.Id, arguments.Thresh))
            {
                arguments.Value = _syncpt.GetMin(arguments.Id);

                return NvInternalResult.Success;
            }

            if (!async)
            {
                arguments.Value = 0;
            }

            if (arguments.Timeout == 0)
            {
                return NvInternalResult.TryAgain;
            }

            NvHostEvent Event;

            NvInternalResult result;

            int eventIndex;

            if (async)
            {
                eventIndex = arguments.Value;

                if ((uint)eventIndex >= EventsCount)
                {
                    return NvInternalResult.InvalidInput;
                }

                Event = _events[eventIndex];
            }
            else
            {
                Event = GetFreeEvent(arguments.Id, out eventIndex);
            }

            if (Event != null &&
               (Event.State == NvHostEventState.Registered ||
                Event.State == NvHostEventState.Free))
            {
                Event.Id     = arguments.Id;
                Event.Thresh = arguments.Thresh;

                Event.State = NvHostEventState.Waiting;

                if (!async)
                {
                    arguments.Value = ((arguments.Id & 0xfff) << 16) | 0x10000000;
                }
                else
                {
                    arguments.Value = arguments.Id << 4;
                }

                arguments.Value |= eventIndex;

                result = NvInternalResult.TryAgain;
            }
            else
            {
                result = NvInternalResult.InvalidInput;
            }

            return result;
        }

        private NvHostEvent GetFreeEvent(int id, out int eventIndex)
        {
            eventIndex = EventsCount;

            int nullIndex = EventsCount;

            for (int index = 0; index < EventsCount; index++)
            {
                NvHostEvent Event = _events[index];

                if (Event != null)
                {
                    if (Event.State == NvHostEventState.Registered ||
                        Event.State == NvHostEventState.Free)
                    {
                        eventIndex = index;

                        if (Event.Id == id)
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

                return _events[nullIndex] = new NvHostEvent();
            }

            if (eventIndex < EventsCount)
            {
                return _events[eventIndex];
            }

            return null;
        }

        public override void Close() { }
    }
}
