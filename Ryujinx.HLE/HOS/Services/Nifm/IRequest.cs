using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Nifm
{
    class IRequest : IpcService
    {
        private KEvent _event0;
        private KEvent _event1;

        public IRequest(Horizon system)
        {
            _event0 = new KEvent(system);
            _event1 = new KEvent(system);
        }

        [Command(0)]
        // GetRequestState() -> u32
        public long GetRequestState(ServiceCtx context)
        {
            context.ResponseData.Write(1);

            Logger.PrintStub(LogClass.ServiceNifm);

            return 0;
        }

        [Command(1)]
        // GetResult()
        public long GetResult(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNifm);

            return 0;
        }

        [Command(2)]
        // GetSystemEventReadableHandles() -> (handle<copy>, handle<copy>)
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

        [Command(3)]
        // Cancel()
        public long Cancel(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNifm);

            return 0;
        }

        [Command(4)]
        // Submit()
        public long Submit(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNifm);

            return 0;
        }

        [Command(11)]
        // SetConnectionConfirmationOption(i8)
        public long SetConnectionConfirmationOption(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNifm);

            return 0;
        }
    }
}