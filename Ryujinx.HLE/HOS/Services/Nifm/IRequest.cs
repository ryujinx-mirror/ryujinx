using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Nifm
{
    class IRequest : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private KEvent Event0;
        private KEvent Event1;

        public IRequest(Horizon System)
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

            Event0 = new KEvent(System);
            Event1 = new KEvent(System);
        }

        public long GetRequestState(ServiceCtx Context)
        {
            Context.ResponseData.Write(1);

            Logger.PrintStub(LogClass.ServiceNifm, "Stubbed.");

            return 0;
        }

        public long GetResult(ServiceCtx Context)
        {
            Logger.PrintStub(LogClass.ServiceNifm, "Stubbed.");

            return 0;
        }

        public long GetSystemEventReadableHandles(ServiceCtx Context)
        {
            if (Context.Process.HandleTable.GenerateHandle(Event0.ReadableEvent, out int Handle0) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            if (Context.Process.HandleTable.GenerateHandle(Event1.ReadableEvent, out int Handle1) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            Context.Response.HandleDesc = IpcHandleDesc.MakeCopy(Handle0, Handle1);

            return 0;
        }

        public long Cancel(ServiceCtx Context)
        {
            Logger.PrintStub(LogClass.ServiceNifm, "Stubbed.");

            return 0;
        }

        public long Submit(ServiceCtx Context)
        {
            Logger.PrintStub(LogClass.ServiceNifm, "Stubbed.");

            return 0;
        }

        public long SetConnectionConfirmationOption(ServiceCtx Context)
        {
            Logger.PrintStub(LogClass.ServiceNifm, "Stubbed.");

            return 0;
        }
    }
}