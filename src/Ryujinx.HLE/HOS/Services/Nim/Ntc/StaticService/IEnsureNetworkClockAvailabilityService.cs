using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Nim.Ntc.StaticService
{
    class IEnsureNetworkClockAvailabilityService : IpcService
    {
        private readonly KEvent _finishNotificationEvent;
        private ResultCode _taskResultCode;

        public IEnsureNetworkClockAvailabilityService(ServiceCtx context)
        {
            _finishNotificationEvent = new KEvent(context.Device.System.KernelContext);
            _taskResultCode = ResultCode.Success;

            // NOTE: The service starts a thread that polls Nintendo NTP server and syncs the time with it.
            //       Additionnally it gets and uses some settings too:
            //       autonomic_correction_interval_seconds, autonomic_correction_failed_retry_interval_seconds,
            //       autonomic_correction_immediate_try_count_max, autonomic_correction_immediate_try_interval_milliseconds
        }

        [CommandCmif(0)]
        // StartTask()
        public ResultCode StartTask(ServiceCtx context)
        {
            if (!context.Device.Configuration.EnableInternetAccess)
            {
                return (ResultCode)Time.ResultCode.NetworkTimeNotAvailable;
            }

            // NOTE: Since we don't support the Nintendo NTP server, we can signal the event now to confirm the update task is done.
            _finishNotificationEvent.ReadableEvent.Signal();

            Logger.Stub?.PrintStub(LogClass.ServiceNtc);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // GetFinishNotificationEvent() -> handle<copy>
        public ResultCode GetFinishNotificationEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_finishNotificationEvent.ReadableEvent, out int finishNotificationEventHandle) != Result.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(finishNotificationEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // GetResult()
        public ResultCode GetResult(ServiceCtx context)
        {
            return _taskResultCode;
        }

        [CommandCmif(3)]
        // Cancel()
        public ResultCode Cancel(ServiceCtx context)
        {
            // NOTE: The update task should be canceled here.
            _finishNotificationEvent.ReadableEvent.Signal();

            _taskResultCode = (ResultCode)Time.ResultCode.NetworkTimeTaskCanceled;

            Logger.Stub?.PrintStub(LogClass.ServiceNtc);

            return ResultCode.Success;
        }
    }
}
