using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Services.Time.TimeZone;
using System;
using System.Text;

namespace Ryujinx.HLE.HOS.Services.Time.StaticService
{
    class ITimeZoneServiceForGlue : IpcService
    {
        private TimeZoneContentManager _timeZoneContentManager;
        private ITimeZoneServiceForPsc _inner;
        private bool                   _writePermission;

        public ITimeZoneServiceForGlue(TimeZoneContentManager timeZoneContentManager, bool writePermission)
        {
            _timeZoneContentManager = timeZoneContentManager;
            _writePermission        = writePermission;
            _inner                  = new ITimeZoneServiceForPsc(timeZoneContentManager.Manager, writePermission);
        }

        [Command(0)]
        // GetDeviceLocationName() -> nn::time::LocationName
        public ResultCode GetDeviceLocationName(ServiceCtx context)
        {
            return _inner.GetDeviceLocationName(context);
        }

        [Command(1)]
        // SetDeviceLocationName(nn::time::LocationName)
        public ResultCode SetDeviceLocationName(ServiceCtx context)
        {
            if (!_writePermission)
            {
                return ResultCode.PermissionDenied;
            }

            string locationName = Encoding.ASCII.GetString(context.RequestData.ReadBytes(0x24)).TrimEnd('\0');

            return _timeZoneContentManager.SetDeviceLocationName(locationName);
        }

        [Command(2)]
        // GetTotalLocationNameCount() -> u32
        public ResultCode GetTotalLocationNameCount(ServiceCtx context)
        {
            return _inner.GetTotalLocationNameCount(context);
        }

        [Command(3)]
        // LoadLocationNameList(u32 index) -> (u32 outCount, buffer<nn::time::LocationName, 6>)
        public ResultCode LoadLocationNameList(ServiceCtx context)
        {
            uint index          = context.RequestData.ReadUInt32();
            long bufferPosition = context.Request.ReceiveBuff[0].Position;
            long bufferSize     = context.Request.ReceiveBuff[0].Size;

            ResultCode errorCode = _timeZoneContentManager.LoadLocationNameList(index, out string[] locationNameArray, (uint)bufferSize / 0x24);

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

                    context.Memory.Write((ulong)bufferPosition + offset, Encoding.ASCII.GetBytes(locationName));
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
                Logger.Error?.Print(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{bufferSize:x} (expected 0x4000)");

                throw new InvalidOperationException();
            }

            string locationName = Encoding.ASCII.GetString(context.RequestData.ReadBytes(0x24)).TrimEnd('\0');

            ResultCode resultCode = _timeZoneContentManager.LoadTimeZoneRule(out TimeZoneRule rules, locationName);

            // Write TimeZoneRule if success
            if (resultCode == ResultCode.Success)
            {
                MemoryHelper.Write(context.Memory, bufferPosition, rules);
            }

            return resultCode;
        }

        [Command(100)]
        // ToCalendarTime(nn::time::PosixTime time, buffer<nn::time::TimeZoneRule, 0x15> rules) -> (nn::time::CalendarTime, nn::time::sf::CalendarAdditionalInfo)
        public ResultCode ToCalendarTime(ServiceCtx context)
        {
            return _inner.ToCalendarTime(context);
        }

        [Command(101)]
        // ToCalendarTimeWithMyRule(nn::time::PosixTime) -> (nn::time::CalendarTime, nn::time::sf::CalendarAdditionalInfo)
        public ResultCode ToCalendarTimeWithMyRule(ServiceCtx context)
        {
            return _inner.ToCalendarTimeWithMyRule(context);
        }

        [Command(201)]
        // ToPosixTime(nn::time::CalendarTime calendarTime, buffer<nn::time::TimeZoneRule, 0x15> rules) -> (u32 outCount, buffer<nn::time::PosixTime, 0xa>)
        public ResultCode ToPosixTime(ServiceCtx context)
        {
            return _inner.ToPosixTime(context);
        }

        [Command(202)]
        // ToPosixTimeWithMyRule(nn::time::CalendarTime calendarTime) -> (u32 outCount, buffer<nn::time::PosixTime, 0xa>)
        public ResultCode ToPosixTimeWithMyRule(ServiceCtx context)
        {
            return _inner.ToPosixTimeWithMyRule(context);
        }
    }
}