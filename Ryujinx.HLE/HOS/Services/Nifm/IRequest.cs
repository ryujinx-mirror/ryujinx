using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.Logging;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Nifm
{
    class IRequest : IpcService, IDisposable
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent Event0;
        private KEvent Event1;

        public IRequest()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,  GetRequestState                 },
                { 1,  GetResult                       },
                { 2,  GetSystemEventReadableHandles   },
                { 3,  Cancel                          },
                { 4,  Submit                          },
                { 11, SetConnectionConfirmationOption }
            };

            Event0 = new KEvent();
            Event1 = new KEvent();
        }

        public long GetRequestState(ServiceCtx Context)
        {
            Context.ResponseData.Write(1);

            Context.Device.Log.PrintStub(LogClass.ServiceNifm, "Stubbed.");

            return 0;
        }

        public long GetResult(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceNifm, "Stubbed.");

            return 0;
        }

        public long GetSystemEventReadableHandles(ServiceCtx Context)
        {
            int Handle0 = Context.Process.HandleTable.OpenHandle(Event0);
            int Handle1 = Context.Process.HandleTable.OpenHandle(Event1);

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle0, Handle1);

            return 0;
        }

        public long Cancel(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceNifm, "Stubbed.");

            return 0;
        }

        public long Submit(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceNifm, "Stubbed.");

            return 0;
        }

        public long SetConnectionConfirmationOption(ServiceCtx Context)
        {
            Context.Device.Log.PrintStub(LogClass.ServiceNifm, "Stubbed.");

            return 0;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                Event0.Dispose();
                Event1.Dispose();
            }
        }
    }
}