using Ryujinx.Common;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using Ryujinx.HLE.HOS.Services.Time.TimeZone;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

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

        [Command(5)] // 4.0.0+
        // GetEphemeralNetworkSystemClock() -> object<nn::timesrv::detail::service::ISystemClock>
        public ResultCode GetEphemeralNetworkSystemClock(ServiceCtx context)
        {
            MakeObject(context, new ISystemClock(StandardNetworkSystemClockCore.Instance, false));

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
        // CalculateMonotonicSystemClockBaseTimePoint(nn::time::SystemClockContext) -> s64
        public ResultCode CalculateMonotonicSystemClockBaseTimePoint(ServiceCtx context)
        {
            SystemClockContext   otherContext     = context.RequestData.ReadStruct<SystemClockContext>();
            SteadyClockTimePoint currentTimePoint = StandardSteadyClockCore.Instance.GetCurrentTimePoint(context.Thread);

            ResultCode result = ResultCode.TimeMismatch;

            if (currentTimePoint.ClockSourceId == otherContext.SteadyTimePoint.ClockSourceId)
            {
                TimeSpanType ticksTimeSpan = TimeSpanType.FromTicks(context.Thread.Context.ThreadState.CntpctEl0, context.Thread.Context.ThreadState.CntfrqEl0);
                long         baseTimePoint = otherContext.Offset + currentTimePoint.TimePoint - ticksTimeSpan.ToSeconds();

                context.ResponseData.Write(baseTimePoint);

                result = 0;
            }

            return result;
        }

        [Command(400)] // 4.0.0+
        // GetClockSnapshot(u8) -> buffer<nn::time::sf::ClockSnapshot, 0x1a>
        public ResultCode GetClockSnapshot(ServiceCtx context)
        {
            byte type = context.RequestData.ReadByte();

            ResultCode result = StandardUserSystemClockCore.Instance.GetSystemClockContext(context.Thread, out SystemClockContext userContext);

            if (result == ResultCode.Success)
            {
                result = StandardNetworkSystemClockCore.Instance.GetSystemClockContext(context.Thread, out SystemClockContext networkContext);

                if (result == ResultCode.Success)
                {
                    result = GetClockSnapshotFromSystemClockContextInternal(context.Thread, userContext, networkContext, type, out ClockSnapshot clockSnapshot);

                    if (result == ResultCode.Success)
                    {
                        WriteClockSnapshotFromBuffer(context, context.Request.RecvListBuff[0], clockSnapshot);
                    }
                }
            }

            return result;
        }

        [Command(401)] // 4.0.0+
        // GetClockSnapshotFromSystemClockContext(u8, nn::time::SystemClockContext, nn::time::SystemClockContext) -> buffer<nn::time::sf::ClockSnapshot, 0x1a>
        public ResultCode GetClockSnapshotFromSystemClockContext(ServiceCtx context)
        {
            byte type = context.RequestData.ReadByte();

            context.RequestData.BaseStream.Position += 7;

            SystemClockContext userContext    = context.RequestData.ReadStruct<SystemClockContext>();
            SystemClockContext networkContext = context.RequestData.ReadStruct<SystemClockContext>();

            ResultCode result = GetClockSnapshotFromSystemClockContextInternal(context.Thread, userContext, networkContext, type, out ClockSnapshot clockSnapshot);

            if (result == ResultCode.Success)
            {
                WriteClockSnapshotFromBuffer(context, context.Request.RecvListBuff[0], clockSnapshot);
            }

            return result;
        }

        [Command(500)] // 4.0.0+
        // CalculateStandardUserSystemClockDifferenceByUser(buffer<nn::time::sf::ClockSnapshot, 0x19>, buffer<nn::time::sf::ClockSnapshot, 0x19>) -> nn::TimeSpanType
        public ResultCode CalculateStandardUserSystemClockDifferenceByUser(ServiceCtx context)
        {

            ClockSnapshot clockSnapshotA = ReadClockSnapshotFromBuffer(context, context.Request.ExchangeBuff[0]);
            ClockSnapshot clockSnapshotB = ReadClockSnapshotFromBuffer(context, context.Request.ExchangeBuff[1]);
            TimeSpanType  difference     = TimeSpanType.FromSeconds(clockSnapshotB.UserContext.Offset - clockSnapshotA.UserContext.Offset);

            if (clockSnapshotB.UserContext.SteadyTimePoint.ClockSourceId != clockSnapshotA.UserContext.SteadyTimePoint.ClockSourceId || (clockSnapshotB.IsAutomaticCorrectionEnabled && clockSnapshotA.IsAutomaticCorrectionEnabled))
            {
                difference = new TimeSpanType(0);
            }

            context.ResponseData.Write(difference.NanoSeconds);

            return ResultCode.Success;
        }

        [Command(501)] // 4.0.0+
        // CalculateSpanBetween(buffer<nn::time::sf::ClockSnapshot, 0x19>, buffer<nn::time::sf::ClockSnapshot, 0x19>) -> nn::TimeSpanType
        public ResultCode CalculateSpanBetween(ServiceCtx context)
        {
            ClockSnapshot clockSnapshotA = ReadClockSnapshotFromBuffer(context, context.Request.ExchangeBuff[0]);
            ClockSnapshot clockSnapshotB = ReadClockSnapshotFromBuffer(context, context.Request.ExchangeBuff[1]);

            TimeSpanType result;

            ResultCode resultCode = clockSnapshotA.SteadyClockTimePoint.GetSpanBetween(clockSnapshotB.SteadyClockTimePoint, out long timeSpan);

            if (resultCode != ResultCode.Success)
            {
                resultCode = ResultCode.TimeNotFound;

                if (clockSnapshotA.NetworkTime != 0 && clockSnapshotB.NetworkTime != 0)
                {
                    result     = TimeSpanType.FromSeconds(clockSnapshotB.NetworkTime - clockSnapshotA.NetworkTime);
                    resultCode = ResultCode.Success;
                }
                else
                {
                    return resultCode;
                }
            }
            else
            {
                result = TimeSpanType.FromSeconds(timeSpan);
            }

            context.ResponseData.Write(result.NanoSeconds);

            return resultCode;
        }

        private ResultCode GetClockSnapshotFromSystemClockContextInternal(KThread thread, SystemClockContext userContext, SystemClockContext networkContext, byte type, out ClockSnapshot clockSnapshot)
        {
            clockSnapshot = new ClockSnapshot();

            SteadyClockCore      steadyClockCore  = StandardSteadyClockCore.Instance;
            SteadyClockTimePoint currentTimePoint = steadyClockCore.GetCurrentTimePoint(thread);

            clockSnapshot.IsAutomaticCorrectionEnabled = StandardUserSystemClockCore.Instance.IsAutomaticCorrectionEnabled();
            clockSnapshot.UserContext                  = userContext;
            clockSnapshot.NetworkContext               = networkContext;

            char[] tzName       = TimeZoneManager.Instance.GetDeviceLocationName().ToCharArray();
            char[] locationName = new char[0x24];

            Array.Copy(tzName, locationName, tzName.Length);

            clockSnapshot.LocationName = locationName;

            ResultCode result = ClockSnapshot.GetCurrentTime(out clockSnapshot.UserTime, currentTimePoint, clockSnapshot.UserContext);

            if (result == ResultCode.Success)
            {
                result = TimeZoneManager.Instance.ToCalendarTimeWithMyRules(clockSnapshot.UserTime, out CalendarInfo userCalendarInfo);

                if (result == ResultCode.Success)
                {
                    clockSnapshot.UserCalendarTime           = userCalendarInfo.Time;
                    clockSnapshot.UserCalendarAdditionalTime = userCalendarInfo.AdditionalInfo;

                    if (ClockSnapshot.GetCurrentTime(out clockSnapshot.NetworkTime, currentTimePoint, clockSnapshot.NetworkContext) != ResultCode.Success)
                    {
                        clockSnapshot.NetworkTime = 0;
                    }

                    result = TimeZoneManager.Instance.ToCalendarTimeWithMyRules(clockSnapshot.NetworkTime, out CalendarInfo networkCalendarInfo);

                    if (result == ResultCode.Success)
                    {
                        clockSnapshot.NetworkCalendarTime           = networkCalendarInfo.Time;
                        clockSnapshot.NetworkCalendarAdditionalTime = networkCalendarInfo.AdditionalInfo;
                        clockSnapshot.Type                          = type;

                        // Probably a version field?
                        clockSnapshot.Unknown = 0;
                    }
                }
            }

            return result;
        }

        private ClockSnapshot ReadClockSnapshotFromBuffer(ServiceCtx context, IpcBuffDesc ipcDesc)
        {
            Debug.Assert(ipcDesc.Size == Marshal.SizeOf<ClockSnapshot>());

            using (BinaryReader bufferReader = new BinaryReader(new MemoryStream(context.Memory.ReadBytes(ipcDesc.Position, ipcDesc.Size))))
            {
                return bufferReader.ReadStruct<ClockSnapshot>();
            }
        }

        private void WriteClockSnapshotFromBuffer(ServiceCtx context, IpcRecvListBuffDesc ipcDesc, ClockSnapshot clockSnapshot)
        {
            Debug.Assert(ipcDesc.Size == Marshal.SizeOf<ClockSnapshot>());

            MemoryStream memory = new MemoryStream((int)ipcDesc.Size);

            using (BinaryWriter bufferWriter = new BinaryWriter(memory))
            {
                bufferWriter.WriteStruct(clockSnapshot);
            }

            context.Memory.WriteBytes(ipcDesc.Position, memory.ToArray());
            memory.Dispose();
        }
    }
}