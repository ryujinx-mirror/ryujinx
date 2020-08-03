using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using Ryujinx.HLE.HOS.Services.Time.TimeZone;
using Ryujinx.HLE.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Time.StaticService
{
    class ITimeZoneServiceForPsc : IpcService
    {
        private TimeZoneManager _timeZoneManager;
        private bool            _writePermission;

        public ITimeZoneServiceForPsc(TimeZoneManager timeZoneManager, bool writePermission)
        {
            _timeZoneManager = timeZoneManager;
            _writePermission = writePermission;
        }

        [Command(0)]
        // GetDeviceLocationName() -> nn::time::LocationName
        public ResultCode GetDeviceLocationName(ServiceCtx context)
        {
            ResultCode result = _timeZoneManager.GetDeviceLocationName(out string deviceLocationName);

            if (result == ResultCode.Success)
            {
                WriteLocationName(context, deviceLocationName);
            }

            return result;
        }

        [Command(1)]
        // SetDeviceLocationName(nn::time::LocationName)
        public ResultCode SetDeviceLocationName(ServiceCtx context)
        {
            if (!_writePermission)
            {
                return ResultCode.PermissionDenied;
            }
            
            return ResultCode.NotImplemented;
        }

        [Command(2)]
        // GetTotalLocationNameCount() -> u32
        public ResultCode GetTotalLocationNameCount(ServiceCtx context)
        {
            ResultCode result = _timeZoneManager.GetTotalLocationNameCount(out uint totalLocationNameCount);

            if (result == ResultCode.Success)
            {
                context.ResponseData.Write(totalLocationNameCount);
            }

            return ResultCode.Success;
        }

        [Command(3)]
        // LoadLocationNameList(u32 index) -> (u32 outCount, buffer<nn::time::LocationName, 6>)
        public ResultCode LoadLocationNameList(ServiceCtx context)
        {
            return ResultCode.NotImplemented;
        }

        [Command(4)]
        // LoadTimeZoneRule(nn::time::LocationName locationName) -> buffer<nn::time::TimeZoneRule, 0x16>
        public ResultCode LoadTimeZoneRule(ServiceCtx context)
        {
            return ResultCode.NotImplemented;
        }

        [Command(5)] // 2.0.0+
        // GetTimeZoneRuleVersion() -> nn::time::TimeZoneRuleVersion
        public ResultCode GetTimeZoneRuleVersion(ServiceCtx context)
        {
            ResultCode result = _timeZoneManager.GetTimeZoneRuleVersion(out UInt128 timeZoneRuleVersion);

            if (result == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(timeZoneRuleVersion);
            }

            return result;
        }

        [Command(6)] // 5.0.0+
        // GetDeviceLocationNameAndUpdatedTime() -> (nn::time::LocationName, nn::time::SteadyClockTimePoint)
        public ResultCode GetDeviceLocationNameAndUpdatedTime(ServiceCtx context)
        {
            ResultCode result = _timeZoneManager.GetDeviceLocationName(out string deviceLocationName);

            if (result == ResultCode.Success)
            {
                result = _timeZoneManager.GetUpdatedTime(out SteadyClockTimePoint timeZoneUpdateTimePoint);

                if (result == ResultCode.Success)
                {
                    WriteLocationName(context, deviceLocationName);

                    // Skip padding
                    context.ResponseData.BaseStream.Position += 0x4;

                    context.ResponseData.WriteStruct(timeZoneUpdateTimePoint);
                }
            }

            return result;
        }

        [Command(7)] // 9.0.0+
        // SetDeviceLocationNameWithTimeZoneRule(nn::time::LocationName locationName, buffer<nn::time::TimeZoneBinary, 0x21> timeZoneBinary)
        public ResultCode SetDeviceLocationNameWithTimeZoneRule(ServiceCtx context)
        {
            if (!_writePermission)
            {
                return ResultCode.PermissionDenied;
            }

            (long bufferPosition, long bufferSize) = context.Request.GetBufferType0x21();

            string locationName = Encoding.ASCII.GetString(context.RequestData.ReadBytes(0x24)).TrimEnd('\0');

            ResultCode result;

            byte[] temp = new byte[bufferSize];

            context.Memory.Read((ulong)bufferPosition, temp);

            using (MemoryStream timeZoneBinaryStream = new MemoryStream(temp))
            {
                result = _timeZoneManager.SetDeviceLocationNameWithTimeZoneRule(locationName, timeZoneBinaryStream);
            }

            return result;
        }

        [Command(8)] // 9.0.0+
        // ParseTimeZoneBinary(buffer<nn::time::TimeZoneBinary, 0x21> timeZoneBinary) -> buffer<nn::time::TimeZoneRule, 0x16>
        public ResultCode ParseTimeZoneBinary(ServiceCtx context)
        {
            (long bufferPosition, long bufferSize) = context.Request.GetBufferType0x21();

            long timeZoneRuleBufferPosition = context.Request.ReceiveBuff[0].Position;
            long timeZoneRuleBufferSize     = context.Request.ReceiveBuff[0].Size;

            if (timeZoneRuleBufferSize != 0x4000)
            {
                // TODO: find error code here
                Logger.Error?.Print(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{timeZoneRuleBufferSize:x} (expected 0x4000)");

                throw new InvalidOperationException();
            }

            ResultCode result;

            byte[] temp = new byte[bufferSize];

            context.Memory.Read((ulong)bufferPosition, temp);

            using (MemoryStream timeZoneBinaryStream = new MemoryStream(temp))
            {
                result = _timeZoneManager.ParseTimeZoneRuleBinary(out TimeZoneRule timeZoneRule, timeZoneBinaryStream);

                if (result == ResultCode.Success)
                {
                    MemoryHelper.Write(context.Memory, timeZoneRuleBufferPosition, timeZoneRule);
                }
            }

            return result;
        }

        [Command(20)] // 9.0.0+
        // GetDeviceLocationNameOperationEventReadableHandle() -> handle<copy>
        public ResultCode GetDeviceLocationNameOperationEventReadableHandle(ServiceCtx context)
        {
            return ResultCode.NotImplemented;
        }

        [Command(100)]
        // ToCalendarTime(nn::time::PosixTime time, buffer<nn::time::TimeZoneRule, 0x15> rules) -> (nn::time::CalendarTime, nn::time::sf::CalendarAdditionalInfo)
        public ResultCode ToCalendarTime(ServiceCtx context)
        {
            long posixTime      = context.RequestData.ReadInt64();
            long bufferPosition = context.Request.SendBuff[0].Position;
            long bufferSize     = context.Request.SendBuff[0].Size;

            if (bufferSize != 0x4000)
            {
                // TODO: find error code here
                Logger.Error?.Print(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{bufferSize:x} (expected 0x4000)");

                throw new InvalidOperationException();
            }

            TimeZoneRule rules = MemoryHelper.Read<TimeZoneRule>(context.Memory, bufferPosition);

            ResultCode resultCode = _timeZoneManager.ToCalendarTime(rules, posixTime, out CalendarInfo calendar);

            if (resultCode == 0)
            {
                context.ResponseData.WriteStruct(calendar);
            }

            return resultCode;
        }

        [Command(101)]
        // ToCalendarTimeWithMyRule(nn::time::PosixTime) -> (nn::time::CalendarTime, nn::time::sf::CalendarAdditionalInfo)
        public ResultCode ToCalendarTimeWithMyRule(ServiceCtx context)
        {
            long posixTime = context.RequestData.ReadInt64();

            ResultCode resultCode = _timeZoneManager.ToCalendarTimeWithMyRules(posixTime, out CalendarInfo calendar);

            if (resultCode == 0)
            {
                context.ResponseData.WriteStruct(calendar);
            }

            return resultCode;
        }

        [Command(201)]
        // ToPosixTime(nn::time::CalendarTime calendarTime, buffer<nn::time::TimeZoneRule, 0x15> rules) -> (u32 outCount, buffer<nn::time::PosixTime, 0xa>)
        public ResultCode ToPosixTime(ServiceCtx context)
        {
            long inBufferPosition = context.Request.SendBuff[0].Position;
            long inBufferSize     = context.Request.SendBuff[0].Size;

            CalendarTime calendarTime = context.RequestData.ReadStruct<CalendarTime>();

            if (inBufferSize != 0x4000)
            {
                // TODO: find error code here
                Logger.Error?.Print(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{inBufferSize:x} (expected 0x4000)");

                throw new InvalidOperationException();
            }

            TimeZoneRule rules = MemoryHelper.Read<TimeZoneRule>(context.Memory, inBufferPosition);

            ResultCode resultCode = _timeZoneManager.ToPosixTime(rules, calendarTime, out long posixTime);

            if (resultCode == 0)
            {
                long outBufferPosition = context.Request.RecvListBuff[0].Position;
                long outBufferSize     = context.Request.RecvListBuff[0].Size;

                context.Memory.Write((ulong)outBufferPosition, posixTime);
                context.ResponseData.Write(1);
            }

            return resultCode;
        }

        [Command(202)]
        // ToPosixTimeWithMyRule(nn::time::CalendarTime calendarTime) -> (u32 outCount, buffer<nn::time::PosixTime, 0xa>)
        public ResultCode ToPosixTimeWithMyRule(ServiceCtx context)
        {
            CalendarTime calendarTime = context.RequestData.ReadStruct<CalendarTime>();

            ResultCode resultCode = _timeZoneManager.ToPosixTimeWithMyRules(calendarTime, out long posixTime);

            if (resultCode == 0)
            {
                long outBufferPosition = context.Request.RecvListBuff[0].Position;
                long outBufferSize     = context.Request.RecvListBuff[0].Size;

                context.Memory.Write((ulong)outBufferPosition, posixTime);

                // There could be only one result on one calendar as leap seconds aren't supported.
                context.ResponseData.Write(1);
            }

            return resultCode;
        }

        private void WriteLocationName(ServiceCtx context, string locationName)
        {
            char[] locationNameArray = locationName.ToCharArray();

            int padding = 0x24 - locationNameArray.Length;

            Debug.Assert(padding >= 0, "LocationName exceeded limit (0x24 bytes)");

            context.ResponseData.Write(locationNameArray);

            for (int index = 0; index < padding; index++)
            {
                context.ResponseData.Write((byte)0);
            }
        }
    }
}
