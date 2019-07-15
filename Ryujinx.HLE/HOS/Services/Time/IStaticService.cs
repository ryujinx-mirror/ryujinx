using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using System;

namespace Ryujinx.HLE.HOS.Services.Time
{
    [Service("time:a", TimePermissions.Applet)]
    [Service("time:s", TimePermissions.System)]
    [Service("time:u", TimePermissions.User)]
    class IStaticService : IpcService
    {
        private TimePermissions _permissions;

        private int _timeSharedMemoryNativeHandle = 0;

        private static readonly DateTime StartupDate = DateTime.UtcNow;

        public IStaticService(ServiceCtx context, TimePermissions permissions)
        {
            _permissions = permissions;
        }

        [Command(0)]
        // GetStandardUserSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public ResultCode GetStandardUserSystemClock(ServiceCtx context)
        {
            MakeObject(context, new ISystemClock(StandardUserSystemClockCore.Instance, (_permissions & TimePermissions.UserSystemClockWritableMask) != 0));

            return ResultCode.Success;
        }

        [Command(1)]
        // GetStandardNetworkSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public ResultCode GetStandardNetworkSystemClock(ServiceCtx context)
        {
            MakeObject(context, new ISystemClock(StandardNetworkSystemClockCore.Instance, (_permissions & TimePermissions.NetworkSystemClockWritableMask) != 0));

            return ResultCode.Success;
        }

        [Command(2)]
        // GetStandardSteadyClock() -> object<nn::timesrv::detail::service::ISteadyClock>
        public ResultCode GetStandardSteadyClock(ServiceCtx context)
        {
            MakeObject(context, new ISteadyClock());

            return ResultCode.Success;
        }

        [Command(3)]
        // GetTimeZoneService() -> object<nn::timesrv::detail::service::ITimeZoneService>
        public ResultCode GetTimeZoneService(ServiceCtx context)
        {
            MakeObject(context, new ITimeZoneService());

            return ResultCode.Success;
        }

        [Command(4)]
        // GetStandardLocalSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public ResultCode GetStandardLocalSystemClock(ServiceCtx context)
        {
            MakeObject(context, new ISystemClock(StandardLocalSystemClockCore.Instance, (_permissions & TimePermissions.LocalSystemClockWritableMask) != 0));

            return ResultCode.Success;
        }

        [Command(20)] // 6.0.0+
        // GetSharedMemoryNativeHandle() -> handle<copy>
        public ResultCode GetSharedMemoryNativeHandle(ServiceCtx context)
        {
            if (_timeSharedMemoryNativeHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(context.Device.System.TimeSharedMem, out _timeSharedMemoryNativeHandle) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_timeSharedMemoryNativeHandle);

            return ResultCode.Success;
        }

        [Command(100)]
        // IsStandardUserSystemClockAutomaticCorrectionEnabled() -> bool
        public ResultCode IsStandardUserSystemClockAutomaticCorrectionEnabled(ServiceCtx context)
        {
            context.ResponseData.Write(StandardUserSystemClockCore.Instance.IsAutomaticCorrectionEnabled());

            return ResultCode.Success;
        }

        [Command(101)]
        // SetStandardUserSystemClockAutomaticCorrectionEnabled(b8)
        public ResultCode SetStandardUserSystemClockAutomaticCorrectionEnabled(ServiceCtx context)
        {
            if ((_permissions & TimePermissions.UserSystemClockWritableMask) == 0)
            {
                return ResultCode.PermissionDenied;
            }

            bool autoCorrectionEnabled = context.RequestData.ReadBoolean();

            return StandardUserSystemClockCore.Instance.SetAutomaticCorrectionEnabled(context.Thread, autoCorrectionEnabled);
        }

        [Command(200)] // 3.0.0+
        // IsStandardNetworkSystemClockAccuracySufficient() -> bool
        public ResultCode IsStandardNetworkSystemClockAccuracySufficient(ServiceCtx context)
        {
            context.ResponseData.Write(StandardNetworkSystemClockCore.Instance.IsStandardNetworkSystemClockAccuracySufficient(context.Thread));

            return ResultCode.Success;
        }

        [Command(300)] // 4.0.0+
        // CalculateMonotonicSystemClockBaseTimePoint(nn::time::SystemClockContext) -> u64
        public ResultCode CalculateMonotonicSystemClockBaseTimePoint(ServiceCtx context)
        {
            // TODO: reimplement this
            long timeOffset              = (long)(DateTime.UtcNow - StartupDate).TotalSeconds;
            long systemClockContextEpoch = context.RequestData.ReadInt64();

            context.ResponseData.Write(timeOffset + systemClockContextEpoch);

            return ResultCode.Success;
        }
    }
}