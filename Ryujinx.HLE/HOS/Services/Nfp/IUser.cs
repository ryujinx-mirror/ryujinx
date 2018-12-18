using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.Input;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Nfp
{
    class IUser : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private const HidControllerId NpadId = HidControllerId.ControllerPlayer1;

        private State _state = State.NonInitialized;

        private DeviceState _deviceState = DeviceState.Initialized;

        private KEvent _activateEvent;

        private KEvent _deactivateEvent;

        private KEvent _availabilityChangeEvent;

        public IUser(Horizon system)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,  Initialize                    },
                { 17, AttachActivateEvent           },
                { 18, AttachDeactivateEvent         },
                { 19, GetState                      },
                { 20, GetDeviceState                },
                { 21, GetNpadId                     },
                { 23, AttachAvailabilityChangeEvent }
            };

            _activateEvent           = new KEvent(system);
            _deactivateEvent         = new KEvent(system);
            _availabilityChangeEvent = new KEvent(system);
        }

        public long Initialize(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            _state = State.Initialized;

            return 0;
        }

        public long AttachActivateEvent(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            if (context.Process.HandleTable.GenerateHandle(_activateEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return 0;
        }

        public long AttachDeactivateEvent(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            if (context.Process.HandleTable.GenerateHandle(_deactivateEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return 0;
        }

        public long GetState(ServiceCtx context)
        {
            context.ResponseData.Write((int)_state);

            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            return 0;
        }

        public long GetDeviceState(ServiceCtx context)
        {
            context.ResponseData.Write((int)_deviceState);

            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            return 0;
        }

        public long GetNpadId(ServiceCtx context)
        {
            context.ResponseData.Write((int)NpadId);

            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            return 0;
        }

        public long AttachAvailabilityChangeEvent(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            if (context.Process.HandleTable.GenerateHandle(_availabilityChangeEvent.ReadableEvent, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            return 0;
        }
    }
}