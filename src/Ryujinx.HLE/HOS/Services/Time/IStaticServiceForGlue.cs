using Ryujinx.Common;
using Ryujinx.HLE.HOS.Services.Pcv.Bpc;
using Ryujinx.HLE.HOS.Services.Settings;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using Ryujinx.HLE.HOS.Services.Time.StaticService;
using System;

namespace Ryujinx.HLE.HOS.Services.Time
{
    [Service("time:a", TimePermissions.Admin)]
    [Service("time:r", TimePermissions.Repair)]
    [Service("time:u", TimePermissions.User)]
    class IStaticServiceForGlue : IpcService
    {
        private readonly IStaticServiceForPsc _inner;
        private readonly TimePermissions _permissions;

        public IStaticServiceForGlue(ServiceCtx context, TimePermissions permissions) : base(context.Device.System.TimeServer)
        {
            _permissions = permissions;
            _inner = new IStaticServiceForPsc(context, permissions);
            _inner.TrySetServer(Server);
            _inner.SetParent(this);
        }

        [CommandCmif(0)]
        // GetStandardUserSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public ResultCode GetStandardUserSystemClock(ServiceCtx context)
        {
            return _inner.GetStandardUserSystemClock(context);
        }

        [CommandCmif(1)]
        // GetStandardNetworkSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public ResultCode GetStandardNetworkSystemClock(ServiceCtx context)
        {
            return _inner.GetStandardNetworkSystemClock(context);
        }

        [CommandCmif(2)]
        // GetStandardSteadyClock() -> object<nn::timesrv::detail::service::ISteadyClock>
        public ResultCode GetStandardSteadyClock(ServiceCtx context)
        {
            return _inner.GetStandardSteadyClock(context);
        }

        [CommandCmif(3)]
        // GetTimeZoneService() -> object<nn::timesrv::detail::service::ITimeZoneService>
        public ResultCode GetTimeZoneService(ServiceCtx context)
        {
            MakeObject(context, new ITimeZoneServiceForGlue(TimeManager.Instance.TimeZone, (_permissions & TimePermissions.TimeZoneWritableMask) != 0));

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // GetStandardLocalSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public ResultCode GetStandardLocalSystemClock(ServiceCtx context)
        {
            return _inner.GetStandardLocalSystemClock(context);
        }

        [CommandCmif(5)] // 4.0.0+
        // GetEphemeralNetworkSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public ResultCode GetEphemeralNetworkSystemClock(ServiceCtx context)
        {
            return _inner.GetEphemeralNetworkSystemClock(context);
        }

        [CommandCmif(20)] // 6.0.0+
        // GetSharedMemoryNativeHandle() -> handle<copy>
        public ResultCode GetSharedMemoryNativeHandle(ServiceCtx context)
        {
            return _inner.GetSharedMemoryNativeHandle(context);
        }

        [CommandCmif(50)] // 4.0.0+
        // SetStandardSteadyClockInternalOffset(nn::TimeSpanType internal_offset)
        public ResultCode SetStandardSteadyClockInternalOffset(ServiceCtx context)
        {
            if ((_permissions & TimePermissions.SteadyClockWritableMask) == 0)
            {
                return ResultCode.PermissionDenied;
            }

#pragma warning disable IDE0059 // Remove unnecessary value assignment
            TimeSpanType internalOffset = context.RequestData.ReadStruct<TimeSpanType>();
#pragma warning restore IDE0059

            // TODO: set:sys SetExternalSteadyClockInternalOffset(internalOffset.ToSeconds())

            return ResultCode.Success;
        }

        [CommandCmif(51)] // 9.0.0+
        // GetStandardSteadyClockRtcValue() -> u64
        public ResultCode GetStandardSteadyClockRtcValue(ServiceCtx context)
        {
            ResultCode result = (ResultCode)IRtcManager.GetExternalRtcValue(out ulong rtcValue);

            if (result == ResultCode.Success)
            {
                context.ResponseData.Write(rtcValue);
            }

            return result;
        }

        [CommandCmif(100)]
        // IsStandardUserSystemClockAutomaticCorrectionEnabled() -> bool
        public ResultCode IsStandardUserSystemClockAutomaticCorrectionEnabled(ServiceCtx context)
        {
            return _inner.IsStandardUserSystemClockAutomaticCorrectionEnabled(context);
        }

        [CommandCmif(101)]
        // SetStandardUserSystemClockAutomaticCorrectionEnabled(b8)
        public ResultCode SetStandardUserSystemClockAutomaticCorrectionEnabled(ServiceCtx context)
        {
            return _inner.SetStandardUserSystemClockAutomaticCorrectionEnabled(context);
        }

        [CommandCmif(102)] // 5.0.0+
        // GetStandardUserSystemClockInitialYear() -> u32
        public ResultCode GetStandardUserSystemClockInitialYear(ServiceCtx context)
        {
            if (!NxSettings.Settings.TryGetValue("time!standard_user_clock_initial_year", out object standardUserSystemClockInitialYear))
            {
                throw new InvalidOperationException("standard_user_clock_initial_year isn't defined in system settings!");
            }

            context.ResponseData.Write((int)standardUserSystemClockInitialYear);

            return ResultCode.Success;
        }

        [CommandCmif(200)] // 3.0.0+
        // IsStandardNetworkSystemClockAccuracySufficient() -> bool
        public ResultCode IsStandardNetworkSystemClockAccuracySufficient(ServiceCtx context)
        {
            return _inner.IsStandardNetworkSystemClockAccuracySufficient(context);
        }

        [CommandCmif(201)] // 6.0.0+
        // GetStandardUserSystemClockAutomaticCorrectionUpdatedTime() -> nn::time::SteadyClockTimePoint
        public ResultCode GetStandardUserSystemClockAutomaticCorrectionUpdatedTime(ServiceCtx context)
        {
            return _inner.GetStandardUserSystemClockAutomaticCorrectionUpdatedTime(context);
        }

        [CommandCmif(300)] // 4.0.0+
        // CalculateMonotonicSystemClockBaseTimePoint(nn::time::SystemClockContext) -> s64
        public ResultCode CalculateMonotonicSystemClockBaseTimePoint(ServiceCtx context)
        {
            return _inner.CalculateMonotonicSystemClockBaseTimePoint(context);
        }

        [CommandCmif(400)] // 4.0.0+
        // GetClockSnapshot(u8) -> buffer<nn::time::sf::ClockSnapshot, 0x1a>
        public ResultCode GetClockSnapshot(ServiceCtx context)
        {
            return _inner.GetClockSnapshot(context);
        }

        [CommandCmif(401)] // 4.0.0+
        // GetClockSnapshotFromSystemClockContext(u8, nn::time::SystemClockContext, nn::time::SystemClockContext) -> buffer<nn::time::sf::ClockSnapshot, 0x1a>
        public ResultCode GetClockSnapshotFromSystemClockContext(ServiceCtx context)
        {
            return _inner.GetClockSnapshotFromSystemClockContext(context);
        }

        [CommandCmif(500)] // 4.0.0+
        // CalculateStandardUserSystemClockDifferenceByUser(buffer<nn::time::sf::ClockSnapshot, 0x19>, buffer<nn::time::sf::ClockSnapshot, 0x19>) -> nn::TimeSpanType
        public ResultCode CalculateStandardUserSystemClockDifferenceByUser(ServiceCtx context)
        {
            return _inner.CalculateStandardUserSystemClockDifferenceByUser(context);
        }

        [CommandCmif(501)] // 4.0.0+
        // CalculateSpanBetween(buffer<nn::time::sf::ClockSnapshot, 0x19>, buffer<nn::time::sf::ClockSnapshot, 0x19>) -> nn::TimeSpanType
        public ResultCode CalculateSpanBetween(ServiceCtx context)
        {
            return _inner.CalculateSpanBetween(context);
        }
    }
}
