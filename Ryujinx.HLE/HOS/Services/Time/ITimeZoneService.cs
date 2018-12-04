using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;
using System.Text;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Time
{
    class ITimeZoneService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private TimeZoneInfo _timeZone = TimeZoneInfo.Local;

        public ITimeZoneService()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,   GetDeviceLocationName     },
                { 1,   SetDeviceLocationName     },
                { 2,   GetTotalLocationNameCount },
                { 3,   LoadLocationNameList      },
                { 4,   LoadTimeZoneRule          },
                { 100, ToCalendarTime            },
                { 101, ToCalendarTimeWithMyRule  },
                { 201, ToPosixTime               },
                { 202, ToPosixTimeWithMyRule     }
            };
        }

        public long GetDeviceLocationName(ServiceCtx context)
        {
            char[] tzName = _timeZone.Id.ToCharArray();

            context.ResponseData.Write(tzName);

            int padding = 0x24 - tzName.Length;

            for (int index = 0; index < padding; index++)
            {
                context.ResponseData.Write((byte)0);
            }

            return 0;
        }

        public long SetDeviceLocationName(ServiceCtx context)
        {
            byte[] locationName = context.RequestData.ReadBytes(0x24);

            string tzId = Encoding.ASCII.GetString(locationName).TrimEnd('\0');

            long resultCode = 0;

            try
            {
                _timeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            }
            catch (TimeZoneNotFoundException)
            {
                resultCode = MakeError(ErrorModule.Time, 0x3dd);
            }

            return resultCode;
        }

        public long GetTotalLocationNameCount(ServiceCtx context)
        {
            context.ResponseData.Write(TimeZoneInfo.GetSystemTimeZones().Count);

            return 0;
        }

        public long LoadLocationNameList(ServiceCtx context)
        {
            long bufferPosition = context.Response.SendBuff[0].Position;
            long bufferSize     = context.Response.SendBuff[0].Size;

            int offset = 0;

            foreach (TimeZoneInfo info in TimeZoneInfo.GetSystemTimeZones())
            {
                byte[] tzData = Encoding.ASCII.GetBytes(info.Id);

                context.Memory.WriteBytes(bufferPosition + offset, tzData);

                int padding = 0x24 - tzData.Length;

                for (int index = 0; index < padding; index++)
                {
                    context.ResponseData.Write((byte)0);
                }

                offset += 0x24;
            }

            return 0;
        }

        public long LoadTimeZoneRule(ServiceCtx context)
        {
            long bufferPosition = context.Request.ReceiveBuff[0].Position;
            long bufferSize     = context.Request.ReceiveBuff[0].Size;

            if (bufferSize != 0x4000)
            {
                Logger.PrintWarning(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{bufferSize:x} (expected 0x4000)");
            }

            long resultCode = 0;

            byte[] locationName = context.RequestData.ReadBytes(0x24);

            string tzId = Encoding.ASCII.GetString(locationName).TrimEnd('\0');

            // Check if the Time Zone exists, otherwise error out.
            try
            {
                TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById(tzId);

                byte[] tzData = Encoding.ASCII.GetBytes(info.Id);

                // FIXME: This is not in ANY cases accurate, but the games don't care about the content of the buffer, they only pass it.
                // TODO: Reverse the TZif2 conversion in PCV to make this match with real hardware.
                context.Memory.WriteBytes(bufferPosition, tzData);
            }
            catch (TimeZoneNotFoundException)
            {
                Logger.PrintWarning(LogClass.ServiceTime, $"Timezone not found for string: {tzId} (len: {tzId.Length})");

                resultCode = MakeError(ErrorModule.Time, 0x3dd);
            }

            return resultCode;
        }

        private long ToCalendarTimeWithTz(ServiceCtx context, long posixTime, TimeZoneInfo info)
        {
            DateTime currentTime = Epoch.AddSeconds(posixTime);

            currentTime = TimeZoneInfo.ConvertTimeFromUtc(currentTime, info);

            context.ResponseData.Write((ushort)currentTime.Year);
            context.ResponseData.Write((byte)currentTime.Month);
            context.ResponseData.Write((byte)currentTime.Day);
            context.ResponseData.Write((byte)currentTime.Hour);
            context.ResponseData.Write((byte)currentTime.Minute);
            context.ResponseData.Write((byte)currentTime.Second);
            context.ResponseData.Write((byte)0); //MilliSecond ?
            context.ResponseData.Write((int)currentTime.DayOfWeek);
            context.ResponseData.Write(currentTime.DayOfYear - 1);
            context.ResponseData.Write(new byte[8]); //TODO: Find out the names used.
            context.ResponseData.Write((byte)(currentTime.IsDaylightSavingTime() ? 1 : 0));
            context.ResponseData.Write((int)info.GetUtcOffset(currentTime).TotalSeconds);

            return 0;
        }

        public long ToCalendarTime(ServiceCtx context)
        {
            long posixTime      = context.RequestData.ReadInt64();
            long bufferPosition = context.Request.SendBuff[0].Position;
            long bufferSize     = context.Request.SendBuff[0].Size;

            if (bufferSize != 0x4000)
            {
                Logger.PrintWarning(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{bufferSize:x} (expected 0x4000)");
            }

            // TODO: Reverse the TZif2 conversion in PCV to make this match with real hardware.
            byte[] tzData = context.Memory.ReadBytes(bufferPosition, 0x24);

            string tzId = Encoding.ASCII.GetString(tzData).TrimEnd('\0');

            long resultCode = 0;

            // Check if the Time Zone exists, otherwise error out.
            try
            {
                TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById(tzId);

                resultCode = ToCalendarTimeWithTz(context, posixTime, info);
            }
            catch (TimeZoneNotFoundException)
            {
                Logger.PrintWarning(LogClass.ServiceTime, $"Timezone not found for string: {tzId} (len: {tzId.Length})");

                resultCode = MakeError(ErrorModule.Time, 0x3dd);
            }

            return resultCode;
        }

        public long ToCalendarTimeWithMyRule(ServiceCtx context)
        {
            long posixTime = context.RequestData.ReadInt64();

            return ToCalendarTimeWithTz(context, posixTime, _timeZone);
        }

        public long ToPosixTime(ServiceCtx context)
        {
            long bufferPosition = context.Request.SendBuff[0].Position;
            long bufferSize     = context.Request.SendBuff[0].Size;

            ushort year   = context.RequestData.ReadUInt16();
            byte   month  = context.RequestData.ReadByte();
            byte   day    = context.RequestData.ReadByte();
            byte   hour   = context.RequestData.ReadByte();
            byte   minute = context.RequestData.ReadByte();
            byte   second = context.RequestData.ReadByte();

            DateTime calendarTime = new DateTime(year, month, day, hour, minute, second);

            if (bufferSize != 0x4000)
            {
                Logger.PrintWarning(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{bufferSize:x} (expected 0x4000)");
            }

            // TODO: Reverse the TZif2 conversion in PCV to make this match with real hardware.
            byte[] tzData = context.Memory.ReadBytes(bufferPosition, 0x24);

            string tzId = Encoding.ASCII.GetString(tzData).TrimEnd('\0');

            long resultCode = 0;

            // Check if the Time Zone exists, otherwise error out.
            try
            {
                TimeZoneInfo info = TimeZoneInfo.FindSystemTimeZoneById(tzId);

                return ToPosixTimeWithTz(context, calendarTime, info);
            }
            catch (TimeZoneNotFoundException)
            {
                Logger.PrintWarning(LogClass.ServiceTime, $"Timezone not found for string: {tzId} (len: {tzId.Length})");

                resultCode = MakeError(ErrorModule.Time, 0x3dd);
            }

            return resultCode;
        }

        public long ToPosixTimeWithMyRule(ServiceCtx context)
        {
            ushort year   = context.RequestData.ReadUInt16();
            byte   month  = context.RequestData.ReadByte();
            byte   day    = context.RequestData.ReadByte();
            byte   hour   = context.RequestData.ReadByte();
            byte   minute = context.RequestData.ReadByte();
            byte   second = context.RequestData.ReadByte();

            DateTime calendarTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);

            return ToPosixTimeWithTz(context, calendarTime, _timeZone);
        }

        private long ToPosixTimeWithTz(ServiceCtx context, DateTime calendarTime, TimeZoneInfo info)
        {
            DateTime calenderTimeUtc = TimeZoneInfo.ConvertTimeToUtc(calendarTime, info);

            long posixTime = ((DateTimeOffset)calenderTimeUtc).ToUnixTimeSeconds();

            long position = context.Request.RecvListBuff[0].Position;
            long size     = context.Request.RecvListBuff[0].Size;

            context.Memory.WriteInt64(position, posixTime);

            context.ResponseData.Write(1);

            return 0;
        }
    }
}
