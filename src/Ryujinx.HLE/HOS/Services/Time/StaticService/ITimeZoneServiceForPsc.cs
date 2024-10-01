using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using Ryujinx.HLE.HOS.Services.Time.TimeZone;
using Ryujinx.HLE.Utilities;
using Ryujinx.Memory;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Time.StaticService
{
    class ITimeZoneServiceForPsc : IpcService
    {
        private readonly TimeZoneManager _timeZoneManager;
        private readonly bool _writePermission;

        public ITimeZoneServiceForPsc(TimeZoneManager timeZoneManager, bool writePermission)
        {
            _timeZoneManager = timeZoneManager;
            _writePermission = writePermission;
        }

        [CommandCmif(0)]
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

        [CommandCmif(1)]
        // SetDeviceLocationName(nn::time::LocationName)
        public ResultCode SetDeviceLocationName(ServiceCtx context)
        {
            if (!_writePermission)
            {
                return ResultCode.PermissionDenied;
            }

            return ResultCode.NotImplemented;
        }

        [CommandCmif(2)]
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

        [CommandCmif(3)]
        // LoadLocationNameList(u32 index) -> (u32 outCount, buffer<nn::time::LocationName, 6>)
        public ResultCode LoadLocationNameList(ServiceCtx context)
        {
            return ResultCode.NotImplemented;
        }

        [CommandCmif(4)]
        // LoadTimeZoneRule(nn::time::LocationName locationName) -> buffer<nn::time::TimeZoneRule, 0x16>
        public ResultCode LoadTimeZoneRule(ServiceCtx context)
        {
            return ResultCode.NotImplemented;
        }

        [CommandCmif(5)] // 2.0.0+
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

        [CommandCmif(6)] // 5.0.0+
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

        [CommandCmif(7)] // 9.0.0+
        // SetDeviceLocationNameWithTimeZoneRule(nn::time::LocationName locationName, buffer<nn::time::TimeZoneBinary, 0x21> timeZoneBinary)
        public ResultCode SetDeviceLocationNameWithTimeZoneRule(ServiceCtx context)
        {
            if (!_writePermission)
            {
                return ResultCode.PermissionDenied;
            }

            (ulong bufferPosition, ulong bufferSize) = context.Request.GetBufferType0x21();

            string locationName = StringUtils.ReadInlinedAsciiString(context.RequestData, 0x24);

            ResultCode result;

            byte[] temp = new byte[bufferSize];

            context.Memory.Read(bufferPosition, temp);

            using MemoryStream timeZoneBinaryStream = new(temp);
            result = _timeZoneManager.SetDeviceLocationNameWithTimeZoneRule(locationName, timeZoneBinaryStream);

            return result;
        }

        [CommandCmif(8)] // 9.0.0+
        // ParseTimeZoneBinary(buffer<nn::time::TimeZoneBinary, 0x21> timeZoneBinary) -> buffer<nn::time::TimeZoneRule, 0x16>
        public ResultCode ParseTimeZoneBinary(ServiceCtx context)
        {
            (ulong bufferPosition, ulong bufferSize) = context.Request.GetBufferType0x21();

            ulong timeZoneRuleBufferPosition = context.Request.ReceiveBuff[0].Position;
            ulong timeZoneRuleBufferSize = context.Request.ReceiveBuff[0].Size;

            if (timeZoneRuleBufferSize != 0x4000)
            {
                // TODO: find error code here
                Logger.Error?.Print(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{timeZoneRuleBufferSize:x} (expected 0x4000)");

                throw new InvalidOperationException();
            }

            ResultCode result;

            byte[] temp = new byte[bufferSize];

            context.Memory.Read(bufferPosition, temp);

            using MemoryStream timeZoneBinaryStream = new(temp);
            using WritableRegion region = context.Memory.GetWritableRegion(timeZoneRuleBufferPosition, Unsafe.SizeOf<TimeZoneRule>());

            ref TimeZoneRule rule = ref MemoryMarshal.Cast<byte, TimeZoneRule>(region.Memory.Span)[0];

            result = _timeZoneManager.ParseTimeZoneRuleBinary(ref rule, timeZoneBinaryStream);

            return result;
        }

        [CommandCmif(20)] // 9.0.0+
        // GetDeviceLocationNameOperationEventReadableHandle() -> handle<copy>
        public ResultCode GetDeviceLocationNameOperationEventReadableHandle(ServiceCtx context)
        {
            return ResultCode.NotImplemented;
        }

        [CommandCmif(100)]
        // ToCalendarTime(nn::time::PosixTime time, buffer<nn::time::TimeZoneRule, 0x15> rules) -> (nn::time::CalendarTime, nn::time::sf::CalendarAdditionalInfo)
        public ResultCode ToCalendarTime(ServiceCtx context)
        {
            long posixTime = context.RequestData.ReadInt64();
            ulong bufferPosition = context.Request.SendBuff[0].Position;
            ulong bufferSize = context.Request.SendBuff[0].Size;

            if (bufferSize != 0x4000)
            {
                // TODO: find error code here
                Logger.Error?.Print(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{bufferSize:x} (expected 0x4000)");

                throw new InvalidOperationException();
            }

            ReadOnlySpan<TimeZoneRule> rules = MemoryMarshal.Cast<byte, TimeZoneRule>(context.Memory.GetSpan(bufferPosition, (int)bufferSize));

            ResultCode resultCode = _timeZoneManager.ToCalendarTime(in rules[0], posixTime, out CalendarInfo calendar);

            if (resultCode == 0)
            {
                context.ResponseData.WriteStruct(calendar);
            }

            return resultCode;
        }

        [CommandCmif(101)]
        // ToCalendarTimeWithMyRule(nn::time::PosixTime) -> (nn::time::CalendarTime, nn::time::sf::CalendarAdditionalInfo)
        public ResultCode ToCalendarTimeWithMyRule(ServiceCtx context)
        {
            long posixTime = context.RequestData.ReadInt64();

            ResultCode resultCode = _timeZoneManager.ToCalendarTimeWithMyRules(posixTime, out CalendarInfo calendar);

            if (resultCode == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(calendar);
            }

            return resultCode;
        }

        [CommandCmif(201)]
        // ToPosixTime(nn::time::CalendarTime calendarTime, buffer<nn::time::TimeZoneRule, 0x15> rules) -> (u32 outCount, buffer<nn::time::PosixTime, 0xa>)
        public ResultCode ToPosixTime(ServiceCtx context)
        {
            ulong inBufferPosition = context.Request.SendBuff[0].Position;
            ulong inBufferSize = context.Request.SendBuff[0].Size;

            CalendarTime calendarTime = context.RequestData.ReadStruct<CalendarTime>();

            if (inBufferSize != 0x4000)
            {
                // TODO: find error code here
                Logger.Error?.Print(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{inBufferSize:x} (expected 0x4000)");

                throw new InvalidOperationException();
            }

            ReadOnlySpan<TimeZoneRule> rules = MemoryMarshal.Cast<byte, TimeZoneRule>(context.Memory.GetSpan(inBufferPosition, (int)inBufferSize));

            ResultCode resultCode = _timeZoneManager.ToPosixTime(in rules[0], calendarTime, out long posixTime);

            if (resultCode == ResultCode.Success)
            {
                ulong outBufferPosition = context.Request.RecvListBuff[0].Position;
#pragma warning disable IDE0059 // Remove unnecessary value assignment
                ulong outBufferSize = context.Request.RecvListBuff[0].Size;
#pragma warning restore IDE0059

                context.Memory.Write(outBufferPosition, posixTime);
                context.ResponseData.Write(1);
            }

            return resultCode;
        }

        [CommandCmif(202)]
        // ToPosixTimeWithMyRule(nn::time::CalendarTime calendarTime) -> (u32 outCount, buffer<nn::time::PosixTime, 0xa>)
        public ResultCode ToPosixTimeWithMyRule(ServiceCtx context)
        {
            CalendarTime calendarTime = context.RequestData.ReadStruct<CalendarTime>();

            ResultCode resultCode = _timeZoneManager.ToPosixTimeWithMyRules(calendarTime, out long posixTime);

            if (resultCode == ResultCode.Success)
            {
                ulong outBufferPosition = context.Request.RecvListBuff[0].Position;
#pragma warning disable IDE0059 // Remove unnecessary value assignment
                ulong outBufferSize = context.Request.RecvListBuff[0].Size;
#pragma warning restore IDE0059

                context.Memory.Write(outBufferPosition, posixTime);

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
