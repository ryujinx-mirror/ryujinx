using ARMeilleure.Memory;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Time.TimeZone;
using System;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Time
{
    class ITimeZoneService : IpcService
    {
        public ITimeZoneService() { }

        [Command(0)]
        // GetDeviceLocationName() -> nn::time::LocationName
        public ResultCode GetDeviceLocationName(ServiceCtx context)
        {
            char[] tzName = TimeZoneManager.Instance.GetDeviceLocationName().ToCharArray();

            int padding = 0x24 - tzName.Length;

            if (padding < 0)
            {
                return ResultCode.LocationNameTooLong;
            }

            context.ResponseData.Write(tzName);

            for (int index = 0; index < padding; index++)
            {
                context.ResponseData.Write((byte)0);
            }

            return ResultCode.Success;
        }

        [Command(1)]
        // SetDeviceLocationName(nn::time::LocationName)
        public ResultCode SetDeviceLocationName(ServiceCtx context)
        {
            string locationName = Encoding.ASCII.GetString(context.RequestData.ReadBytes(0x24)).TrimEnd('\0');

            return TimeZoneManager.Instance.SetDeviceLocationName(locationName);
        }

        [Command(2)]
        // GetTotalLocationNameCount() -> u32
        public ResultCode GetTotalLocationNameCount(ServiceCtx context)
        {
            context.ResponseData.Write(TimeZoneManager.Instance.GetTotalLocationNameCount());

            return ResultCode.Success;
        }

        [Command(3)]
        // LoadLocationNameList(u32 index) -> (u32 outCount, buffer<nn::time::LocationName, 6>)
        public ResultCode LoadLocationNameList(ServiceCtx context)
        {
            uint index          = context.RequestData.ReadUInt32();
            long bufferPosition = context.Request.ReceiveBuff[0].Position;
            long bufferSize     = context.Request.ReceiveBuff[0].Size;

            ResultCode errorCode = TimeZoneManager.Instance.LoadLocationNameList(index, out string[] locationNameArray, (uint)bufferSize / 0x24);

            if (errorCode == 0)
            {
                uint offset = 0;

                foreach (string locationName in locationNameArray)
                {
                    int padding = 0x24 - locationName.Length;

                    if (padding < 0)
                    {
                        return ResultCode.LocationNameTooLong;
                    }

                    context.Memory.WriteBytes(bufferPosition + offset, Encoding.ASCII.GetBytes(locationName));
                    MemoryHelper.FillWithZeros(context.Memory, bufferPosition + offset + locationName.Length, padding);

                    offset += 0x24;
                }

                context.ResponseData.Write((uint)locationNameArray.Length);
            }

            return errorCode;
        }

        [Command(4)]
        // LoadTimeZoneRule(nn::time::LocationName locationName) -> buffer<nn::time::TimeZoneRule, 0x16>
        public ResultCode LoadTimeZoneRule(ServiceCtx context)
        {
            long bufferPosition = context.Request.ReceiveBuff[0].Position;
            long bufferSize     = context.Request.ReceiveBuff[0].Size;

            if (bufferSize != 0x4000)
            {
                // TODO: find error code here
                Logger.PrintError(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{bufferSize:x} (expected 0x4000)");

                throw new InvalidOperationException();
            }


            string locationName = Encoding.ASCII.GetString(context.RequestData.ReadBytes(0x24)).TrimEnd('\0');

            ResultCode resultCode = TimeZoneManager.Instance.LoadTimeZoneRules(out TimeZoneRule rules, locationName);

            // Write TimeZoneRule if success
            if (resultCode == 0)
            {
                MemoryHelper.Write(context.Memory, bufferPosition, rules);
            }

            return resultCode;
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
                Logger.PrintError(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{bufferSize:x} (expected 0x4000)");

                throw new InvalidOperationException();
            }

            TimeZoneRule rules = MemoryHelper.Read<TimeZoneRule>(context.Memory, bufferPosition);

            ResultCode resultCode = TimeZoneManager.ToCalendarTime(rules, posixTime, out CalendarInfo calendar);

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

            ResultCode resultCode = TimeZoneManager.Instance.ToCalendarTimeWithMyRules(posixTime, out CalendarInfo calendar);

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
                Logger.PrintError(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{inBufferSize:x} (expected 0x4000)");

                throw new InvalidOperationException();
            }

            TimeZoneRule rules = MemoryHelper.Read<TimeZoneRule>(context.Memory, inBufferPosition);

            ResultCode resultCode = TimeZoneManager.ToPosixTime(rules, calendarTime, out long posixTime);

            if (resultCode == 0)
            {
                long outBufferPosition = context.Request.RecvListBuff[0].Position;
                long outBufferSize     = context.Request.RecvListBuff[0].Size;

                context.Memory.WriteInt64(outBufferPosition, posixTime);
                context.ResponseData.Write(1);
            }

            return resultCode;
        }

        [Command(202)]
        // ToPosixTimeWithMyRule(nn::time::CalendarTime calendarTime) -> (u32 outCount, buffer<nn::time::PosixTime, 0xa>)
        public ResultCode ToPosixTimeWithMyRule(ServiceCtx context)
        {
            CalendarTime calendarTime = context.RequestData.ReadStruct<CalendarTime>();

            ResultCode resultCode = TimeZoneManager.Instance.ToPosixTimeWithMyRules(calendarTime, out long posixTime);

            if (resultCode == 0)
            {
                long outBufferPosition = context.Request.RecvListBuff[0].Position;
                long outBufferSize     = context.Request.RecvListBuff[0].Size;

                context.Memory.WriteInt64(outBufferPosition, posixTime);

                // There could be only one result on one calendar as leap seconds aren't supported.
                context.ResponseData.Write(1);
            }

            return resultCode;
        }
    }
}