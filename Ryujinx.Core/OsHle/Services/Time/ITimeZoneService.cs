using Ryujinx.Core.OsHle.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle.IpcServices.Time
{
    class ITimeZoneService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);

        public ITimeZoneService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 101,  ToCalendarTimeWithMyRule }
            };
        }

        //(nn::time::PosixTime)-> (nn::time::CalendarTime, nn::time::sf::CalendarAdditionalInfo)
        public long ToCalendarTimeWithMyRule(ServiceCtx Context)
        {
            long PosixTime = Context.RequestData.ReadInt64();

            Epoch = Epoch.AddSeconds(PosixTime).ToLocalTime();

            /*
                struct CalendarTime {
                    u16_le year;
                    u8 month; // Starts at 1
                    u8 day;   // Starts at 1
                    u8 hour;
                    u8 minute;
                    u8 second;
                    INSERT_PADDING_BYTES(1);
                };
            */
            Context.ResponseData.Write((short)Epoch.Year);
            Context.ResponseData.Write((byte)Epoch.Month);
            Context.ResponseData.Write((byte)Epoch.Day);
            Context.ResponseData.Write((byte)Epoch.Hour);
            Context.ResponseData.Write((byte)Epoch.Minute);
            Context.ResponseData.Write((byte)Epoch.Second);
            Context.ResponseData.Write((byte)0);

            /* Thanks to TuxSH
                struct CalendarAdditionalInfo {
	                u32 tm_wday; //day of week [0,6] (Sunday = 0)
	                s32 tm_yday; //day of year [0,365]
	                struct timezone {
		                char[8] tz_name;
		                bool isDaylightSavingTime;
		                s32 utcOffsetSeconds;
	                };
                };
            */
            Context.ResponseData.Write((int)Epoch.DayOfWeek);
            Context.ResponseData.Write(Epoch.DayOfYear);
            Context.ResponseData.Write(new byte[8]);
            Context.ResponseData.Write(Convert.ToByte(Epoch.IsDaylightSavingTime()));
            Context.ResponseData.Write(0);

            return 0;
        }
    }
}