using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.Input;
using Ryujinx.HLE.Logging;
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
            Context.Device.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            State = State.Initialized;

            return 0;
        }

        public long AttachActivateEvent(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            int Handle = Context.Process.HandleTable.OpenHandle(ActivateEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);;

            return 0;
        }

        public long AttachDeactivateEvent(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            int Handle = Context.Process.HandleTable.OpenHandle(DeactivateEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        public long GetState(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)State);

            Context.Device.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            return 0;
        }

        public long GetDeviceState(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)DeviceState);

            Context.Device.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            return 0;
        }

        public long GetNpadId(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)NpadId);

            Context.Device.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            return 0;
        }

        public long AttachAvailabilityChangeEvent(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            int Handle = Context.Process.HandleTable.OpenHandle(AvailabilityChangeEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }
    }
}