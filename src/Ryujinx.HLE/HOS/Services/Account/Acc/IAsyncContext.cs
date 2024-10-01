using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Services.Account.Acc.AsyncContext;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    class IAsyncContext : IpcService
    {
        protected AsyncExecution AsyncExecution;

        public IAsyncContext(AsyncExecution asyncExecution)
        {
            AsyncExecution = asyncExecution;
        }

        [CommandCmif(0)]
        // GetSystemEvent() -> handle<copy>
        public ResultCode GetSystemEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(AsyncExecution.SystemEvent.ReadableEvent, out int systemEventHandle) != Result.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(systemEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // Cancel()
        public ResultCode Cancel(ServiceCtx context)
        {
            if (!AsyncExecution.IsInitialized)
            {
                return ResultCode.AsyncExecutionNotInitialized;
            }

            if (AsyncExecution.IsRunning)
            {
                AsyncExecution.Cancel();
            }

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // HasDone() -> b8
        public ResultCode HasDone(ServiceCtx context)
        {
            if (!AsyncExecution.IsInitialized)
            {
                return ResultCode.AsyncExecutionNotInitialized;
            }

            context.ResponseData.Write(AsyncExecution.SystemEvent.ReadableEvent.IsSignaled());

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // GetResult()
        public ResultCode GetResult(ServiceCtx context)
        {
            if (!AsyncExecution.IsInitialized)
            {
                return ResultCode.AsyncExecutionNotInitialized;
            }

            if (!AsyncExecution.SystemEvent.ReadableEvent.IsSignaled())
            {
                return ResultCode.Unknown41;
            }

            return ResultCode.Success;
        }
    }
}
