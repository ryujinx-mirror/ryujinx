using Ryujinx.Common;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using System;

namespace Ryujinx.HLE.HOS.Services.Time.StaticService
{
    class ISystemClock : IpcService
    {
        private SystemClockCore _clockCore;
        private bool            _writePermission;
        private bool            _bypassUninitializedClock;
        private int             _operationEventReadableHandle;

        public ISystemClock(SystemClockCore clockCore, bool writePermission, bool bypassUninitializedClock)
        {
            _clockCore                    = clockCore;
            _writePermission              = writePermission;
            _bypassUninitializedClock     = bypassUninitializedClock;
            _operationEventReadableHandle = 0;
        }

        [Command(0)]
        // GetCurrentTime() -> nn::time::PosixTime
        public ResultCode GetCurrentTime(ServiceCtx context)
        {
            if (!_bypassUninitializedClock && !_clockCore.IsInitialized())
            {
                return ResultCode.UninitializedClock;
            }

            ResultCode result = _clockCore.GetCurrentTime(context.Thread, out long posixTime);

            if (result == ResultCode.Success)
            {
                context.ResponseData.Write(posixTime);
            }

            return result;
        }

        [Command(1)]
        // SetCurrentTime(nn::time::PosixTime)
        public ResultCode SetCurrentTime(ServiceCtx context)
        {
            if (!_writePermission)
            {
                return ResultCode.PermissionDenied;
            }

            if (!_bypassUninitializedClock && !_clockCore.IsInitialized())
            {
                return ResultCode.UninitializedClock;
            }

            long posixTime = context.RequestData.ReadInt64();

            return _clockCore.SetCurrentTime(context.Thread, posixTime);
        }

        [Command(2)]
        // GetClockContext() -> nn::time::SystemClockContext
        public ResultCode GetSystemClockContext(ServiceCtx context)
        {
            if (!_bypassUninitializedClock && !_clockCore.IsInitialized())
            {
                return ResultCode.UninitializedClock;
            }

            ResultCode result = _clockCore.GetClockContext(context.Thread, out SystemClockContext clockContext);

            if (result == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(clockContext);
            }

            return result;
        }

        [Command(3)]
        // SetClockContext(nn::time::SystemClockContext)
        public ResultCode SetSystemClockContext(ServiceCtx context)
        {
            if (!_writePermission)
            {
                return ResultCode.PermissionDenied;
            }

            if (!_bypassUninitializedClock && !_clockCore.IsInitialized())
            {
                return ResultCode.UninitializedClock;
            }

            SystemClockContext clockContext = context.RequestData.ReadStruct<SystemClockContext>();

            ResultCode result = _clockCore.SetSystemClockContext(clockContext);

            return result;
        }

        [Command(4)] // 9.0.0+
        // GetOperationEventReadableHandle() -> handle<copy>
        public ResultCode GetOperationEventReadableHandle(ServiceCtx context)
        {
            if (_operationEventReadableHandle == 0)
            {
                KEvent kEvent = new KEvent(context.Device.System.KernelContext);

                _clockCore.RegisterOperationEvent(kEvent.WritableEvent);

                if (context.Process.HandleTable.GenerateHandle(kEvent.ReadableEvent, out _operationEventReadableHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_operationEventReadableHandle);

            return ResultCode.Success;
        }
    }
}