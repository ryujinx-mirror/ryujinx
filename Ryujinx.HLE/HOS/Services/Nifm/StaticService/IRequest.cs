using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using System;

namespace Ryujinx.HLE.HOS.Services.Nifm.StaticService
{
    class IRequest : IpcService
    {
        private KEvent _event0;
        private KEvent _event1;

        private uint _version;

        public IRequest(Horizon system, uint version)
        {
            _event0 = new KEvent(system);
            _event1 = new KEvent(system);

            _version = version;
        }

        [Command(0)]
        // GetRequestState() -> u32
        public ResultCode GetRequestState(ServiceCtx context)
        {
            context.ResponseData.Write(1);

            Logger.PrintStub(LogClass.ServiceNifm);

            return ResultCode.Success;
        }

        [Command(1)]
        // GetResult()
        public ResultCode GetResult(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNifm);

            return ResultCode.Success;
        }

        [Command(2)]
        // GetSystemEventReadableHandles() -> (handle<copy>, handle<copy>)
        public ResultCode GetSystemEventReadableHandles(ServiceCtx context)
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

            return ResultCode.Success;
        }

        [Command(3)]
        // Cancel()
        public ResultCode Cancel(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNifm);

            return ResultCode.Success;
        }

        [Command(4)]
        // Submit()
        public ResultCode Submit(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNifm);

            return ResultCode.Success;
        }

        [Command(11)]
        // SetConnectionConfirmationOption(i8)
        public ResultCode SetConnectionConfirmationOption(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceNifm);

            return ResultCode.Success;
        }
    }
}