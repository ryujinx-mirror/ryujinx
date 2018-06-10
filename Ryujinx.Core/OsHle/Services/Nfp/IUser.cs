using Ryujinx.Core.Input;
using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Ipc;
using Ryujinx.Core.OsHle.Services.Hid;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.Services.Nfp
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
        
        public IUser()
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

            ActivateEvent           = new KEvent();
            DeactivateEvent         = new KEvent();
            AvailabilityChangeEvent = new KEvent();
        }

        public long Initialize(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            State = State.Initialized;

            return 0;
        }

        public long AttachActivateEvent(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            int Handle = Context.Process.HandleTable.OpenHandle(ActivateEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);;

            return 0;
        }

        public long AttachDeactivateEvent(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            int Handle = Context.Process.HandleTable.OpenHandle(DeactivateEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }

        public long GetState(ServiceCtx Context)
        {    
            Context.ResponseData.Write((int)State);
            
            Context.Ns.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");          

            return 0;
        }

        public long GetDeviceState(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)DeviceState);
            
            Context.Ns.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            return 0;
        }

        public long GetNpadId(ServiceCtx Context)
        {
            Context.ResponseData.Write((int)NpadId);
            
            Context.Ns.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");

            return 0;
        }

        public long AttachAvailabilityChangeEvent(ServiceCtx Context)
        {
            Context.Ns.Log.PrintStub(LogClass.ServiceNfp, "Stubbed.");
         
            int Handle = Context.Process.HandleTable.OpenHandle(AvailabilityChangeEvent);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle);

            return 0;
        }
    }
}