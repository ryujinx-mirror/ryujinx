using Ryujinx.Common;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using Ryujinx.HLE.Utilities;
using System;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Time
{
    [Service("time:m")] // 9.0.0+
    class ITimeServiceManager : IpcService
    {
        private TimeManager _timeManager;
        private int         _automaticCorrectionEvent;

        public ITimeServiceManager(ServiceCtx context)
        {
            _timeManager              = TimeManager.Instance;
            _automaticCorrectionEvent = 0;
        }

        [Command(0)]
        // GetUserStaticService() -> object<nn::timesrv::detail::service::IStaticService>
        public ResultCode GetUserStaticService(ServiceCtx context)
        {
            MakeObject(context, new IStaticServiceForPsc(_timeManager, TimePermissions.User));

            return ResultCode.Success;
        }

        [Command(5)]
        // GetAdminStaticService() -> object<nn::timesrv::detail::service::IStaticService>
        public ResultCode GetAdminStaticService(ServiceCtx context)
        {
            MakeObject(context, new IStaticServiceForPsc(_timeManager, TimePermissions.Admin));

            return ResultCode.Success;
        }

        [Command(6)]
        // GetRepairStaticService() -> object<nn::timesrv::detail::service::IStaticService>
        public ResultCode GetRepairStaticService(ServiceCtx context)
        {
            MakeObject(context, new IStaticServiceForPsc(_timeManager, TimePermissions.Repair));

            return ResultCode.Success;
        }

        [Command(9)]
        // GetManufactureStaticService() -> object<nn::timesrv::detail::service::IStaticService>
        public ResultCode GetManufactureStaticService(ServiceCtx context)
        {
            MakeObject(context, new IStaticServiceForPsc(_timeManager, TimePermissions.Manufacture));

            return ResultCode.Success;
        }

        [Command(10)]
        // SetupStandardSteadyClock(nn::util::Uuid clock_source_id, nn::TimeSpanType setup_value,  nn::TimeSpanType internal_offset,  nn::TimeSpanType test_offset, bool is_rtc_reset_detected)
        public ResultCode SetupStandardSteadyClock(ServiceCtx context)
        {
            UInt128      clockSourceId      = context.RequestData.ReadStruct<UInt128>();
            TimeSpanType setupValue         = context.RequestData.ReadStruct<TimeSpanType>();
            TimeSpanType internalOffset     = context.RequestData.ReadStruct<TimeSpanType>();
            TimeSpanType testOffset         = context.RequestData.ReadStruct<TimeSpanType>();
            bool         isRtcResetDetected = context.RequestData.ReadBoolean();

            _timeManager.SetupStandardSteadyClock(context.Thread, clockSourceId, setupValue, internalOffset, testOffset, isRtcResetDetected);

            return ResultCode.Success;
        }

        [Command(11)]
        // SetupStandardLocalSystemClock(nn::time::SystemClockContext context, nn::time::PosixTime posix_time)
        public ResultCode SetupStandardLocalSystemClock(ServiceCtx context)
        {
            SystemClockContext clockContext = context.RequestData.ReadStruct<SystemClockContext>();
            long               posixTime    = context.RequestData.ReadInt64();

            _timeManager.SetupStandardLocalSystemClock(context.Thread, clockContext, posixTime);

            return ResultCode.Success;
        }

        [Command(12)]
        // SetupStandardNetworkSystemClock(nn::time::SystemClockContext context, nn::TimeSpanType sufficient_accuracy)
        public ResultCode SetupStandardNetworkSystemClock(ServiceCtx context)
        {
            SystemClockContext clockContext       = context.RequestData.ReadStruct<SystemClockContext>();
            TimeSpanType       sufficientAccuracy = context.RequestData.ReadStruct<TimeSpanType>();

            _timeManager.SetupStandardNetworkSystemClock(clockContext, sufficientAccuracy);

            return ResultCode.Success;
        }

        [Command(13)]
        // SetupStandardUserSystemClock(bool automatic_correction_enabled, nn::time::SteadyClockTimePoint steady_clock_timepoint)
        public ResultCode SetupStandardUserSystemClock(ServiceCtx context)
        {
            bool isAutomaticCorrectionEnabled = context.RequestData.ReadBoolean();

            context.RequestData.BaseStream.Position += 7;

            SteadyClockTimePoint steadyClockTimePoint = context.RequestData.ReadStruct<SteadyClockTimePoint>();

            _timeManager.SetupStandardUserSystemClock(context.Thread, isAutomaticCorrectionEnabled, steadyClockTimePoint);

            return ResultCode.Success;
        }

        [Command(14)]
        // SetupTimeZoneManager(nn::time::LocationName location_name, nn::time::SteadyClockTimePoint timezone_update_timepoint, u32 total_location_name_count, nn::time::TimeZoneRuleVersion timezone_rule_version, buffer<nn::time::TimeZoneBinary, 0x21> timezone_binary)
        public ResultCode SetupTimeZoneManager(ServiceCtx context)
        {
            string               locationName            = Encoding.ASCII.GetString(context.RequestData.ReadBytes(0x24)).TrimEnd('\0');
            SteadyClockTimePoint timeZoneUpdateTimePoint = context.RequestData.ReadStruct<SteadyClockTimePoint>();
            uint                 totalLocationNameCount  = context.RequestData.ReadUInt32();
            UInt128              timeZoneRuleVersion     = context.RequestData.ReadStruct<UInt128>();

            (long bufferPosition, long bufferSize) = context.Request.GetBufferType0x21();

            byte[] temp = new byte[bufferSize];

            context.Memory.Read((ulong)bufferPosition, temp);

            using (MemoryStream timeZoneBinaryStream = new MemoryStream(temp))
            {
                _timeManager.SetupTimeZoneManager(locationName, timeZoneUpdateTimePoint, totalLocationNameCount, timeZoneRuleVersion, timeZoneBinaryStream);
            }

            return ResultCode.Success;
        }

        [Command(15)]
        // SetupEphemeralNetworkSystemClock()
        public ResultCode SetupEphemeralNetworkSystemClock(ServiceCtx context)
        {
            _timeManager.SetupEphemeralNetworkSystemClock();

            return ResultCode.Success;
        }

        [Command(50)]
        // Unknown50() -> handle<copy>
        public ResultCode Unknown50(ServiceCtx context)
        {
            // TODO: figure out the usage of this event
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(51)]
        // Unknown51() -> handle<copy>
        public ResultCode Unknown51(ServiceCtx context)
        {
            // TODO: figure out the usage of this event
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(52)]
        // Unknown52() -> handle<copy>
        public ResultCode Unknown52(ServiceCtx context)
        {
            // TODO: figure out the usage of this event
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(60)]
        // GetStandardUserSystemClockAutomaticCorrectionEvent() -> handle<copy>
        public ResultCode GetStandardUserSystemClockAutomaticCorrectionEvent(ServiceCtx context)
        {
            if (_automaticCorrectionEvent == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_timeManager.StandardUserSystemClock.GetAutomaticCorrectionReadableEvent(), out _automaticCorrectionEvent) != KernelResult.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_automaticCorrectionEvent);

            return ResultCode.Success;
        }

        [Command(100)]
        // SetStandardSteadyClockRtcOffset(nn::TimeSpanType rtc_offset)
        public ResultCode SetStandardSteadyClockRtcOffset(ServiceCtx context)
        {
            TimeSpanType rtcOffset = context.RequestData.ReadStruct<TimeSpanType>();

            _timeManager.SetStandardSteadyClockRtcOffset(context.Thread, rtcOffset);

            return ResultCode.Success;
        }

        [Command(200)]
        // GetAlarmRegistrationEvent() -> handle<copy>
        public ResultCode GetAlarmRegistrationEvent(ServiceCtx context)
        {
            // TODO
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(201)]
        // UpdateSteadyAlarms()
        public ResultCode UpdateSteadyAlarms(ServiceCtx context)
        {
            // TODO
            throw new ServiceNotImplementedException(this, context);
        }

        [Command(202)]
        // TryGetNextSteadyClockAlarmSnapshot() -> (bool, nn::time::SteadyClockAlarmSnapshot)
        public ResultCode TryGetNextSteadyClockAlarmSnapshot(ServiceCtx context)
        {
            // TODO
            throw new ServiceNotImplementedException(this, context);
        }
    }
}
