﻿using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Services.Account.Acc.AsyncContext;
using System;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    class IAsyncContext : IpcService
    {
        AsyncExecution _asyncExecution;

        public IAsyncContext(AsyncExecution asyncExecution)
        {
            _asyncExecution = asyncExecution;
        }

        [Command(0)]
        // GetSystemEvent() -> handle<copy>
        public ResultCode GetSystemEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_asyncExecution.SystemEvent.ReadableEvent, out int _systemEventHandle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_systemEventHandle);

            return ResultCode.Success;
        }

        [Command(1)]
        // Cancel()
        public ResultCode Cancel(ServiceCtx context)
        {
            if (!_asyncExecution.IsInitialized)
            {
                return ResultCode.AsyncExecutionNotInitialized;
            }

            if (_asyncExecution.IsRunning)
            {
                _asyncExecution.Cancel();
            }

            return ResultCode.Success;
        }

        [Command(2)]
        // HasDone() -> b8
        public ResultCode HasDone(ServiceCtx context)
        {
            if (!_asyncExecution.IsInitialized)
            {
                return ResultCode.AsyncExecutionNotInitialized;
            }

            context.ResponseData.Write(_asyncExecution.SystemEvent.ReadableEvent.IsSignaled());

            return ResultCode.Success;
        }

        [Command(3)]
        // GetResult()
        public ResultCode GetResult(ServiceCtx context)
        {
            if (!_asyncExecution.IsInitialized)
            {
                return ResultCode.AsyncExecutionNotInitialized;
            }

            if (!_asyncExecution.SystemEvent.ReadableEvent.IsSignaled())
            {
                return ResultCode.Unknown41;
            }

            return ResultCode.Success;
        }
    }
}