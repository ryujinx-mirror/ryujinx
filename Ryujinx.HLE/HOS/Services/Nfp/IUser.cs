using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.Input;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Nfp
{
    class IUser : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private const HidControllerId NpadId = HidControllerId.CONTROLLER_PLAYER_1;

        private State State = State.NonInitialized;

        private DeviceState DeviceState = DeviceState.Initialized;

        private KEvent ActivateEvent;

        private KEvent DeactivateEvent;

        private KEvent AvailabilityChangeEvent;

        public IUser(Horizon System)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,  Initialize                    },
                { 17, AttachActivateEvent           },
                { 18, AttachDeactivateEvent         },
                { 19, GetState                      },
                { 20, GetDeviceState                },
                { 21, GetNpadId                     },
                { 23, AttachAvailabilityChangeEvent }
            };

            ActivateEvent           = new KEvent(System);
            DeactivateEvent         = new KEvent(System);
            AvailabilityChangeEvent = new KEvent(System);
        }

        public long Initialize(ServiceCtx Context)
        {
            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            State = State.Initialized;

            return 0;
        }

        public long AttachActivateEvent(ServiceCtx Context)
        {
            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            if (Context.Process.HandleTable.GenerateHandle(ActivateEvent.ReadableEvent, out int Handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);;

            return 0;
        }

        public long AttachDeactivateEvent(ServiceCtx Context)
        {
            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            if (Context.Process.HandleTable.GenerateHandle(DeactivateEvent.ReadableEvent, out int Handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        public long GetState(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)State);

            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            return 0;
        }

        public long GetDeviceState(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)DeviceState);

            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            return 0;
        }

        public long GetNpadId(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)NpadId);

            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            return 0;
        }

        public long AttachAvailabilityChangeEvent(ServiceCtx Context)
        {
            Logger.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            if (Context.Process.HandleTable.GenerateHandle(AvailabilityChangeEvent.ReadableEvent, out int Handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }
    }
}