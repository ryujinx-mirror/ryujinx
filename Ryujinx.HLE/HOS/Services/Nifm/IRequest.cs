using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Nifm
{
    class IRequest : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private KEvent _event0;
        private KEvent _event1;

        public IRequest(Horizon system)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,  GetRequestState                 },
                { 1,  GetResult                       },
                { 2,  GetSystemEventReadableHandles   },
                { 3,  Cancel                          },
                { 4,  Submit                          },
                { 11, SetConnectionConfirmationOption }
            };

            _event0 = new KEvent(system);
            _event1 = new KEvent(system);
        }

        public long GetRequestState(ServiceCtx context)
        {
            context.ResponseData.Write(1);

            Logger.PrintStub(LogClass.ServiceNifm);

            return 0;
        }

        public long GetResult(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNifm);

            return 0;
        }

        public long GetSystemEventReadableHandles(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_event0.ReadableEvent, out int handle0) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            if (context.Process.HandleTable.GenerateHandle(_event1.ReadableEvent, out int handle1) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle0, handle1);

            return 0;
        }

        public long Cancel(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNifm);

            return 0;
        }

        public long Submit(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNifm);

            return 0;
        }

        public long SetConnectionConfirmationOption(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNifm);

            return 0;
        }
    }
}