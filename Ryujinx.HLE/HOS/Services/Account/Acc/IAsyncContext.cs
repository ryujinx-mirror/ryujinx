﻿using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Services.Account.Acc.AsyncContext;
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

        [CommandHipc(0)]
        // GetSystemEvent() -> handle<copy>
        public ResultCode GetSystemEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(AsyncExecution.SystemEvent.ReadableEvent, out int _systemEventHandle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_systemEventHandle);

            return ResultCode.Success;
        }

        [CommandHipc(1)]
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

        [CommandHipc(2)]
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

        [CommandHipc(3)]
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