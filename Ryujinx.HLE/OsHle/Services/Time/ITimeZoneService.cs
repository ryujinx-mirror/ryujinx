using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Ipc;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ryujinx.HLE.OsHle.Services.Time
{
    class ITimeZoneService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private TimeZoneInfo TimeZone = TimeZoneInfo.Local;

        public ITimeZoneService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0,   GetDeviceLocationName     },
                { 1,   SetDeviceLocationName     },
                { 2,   GetTotalLocationNameCount },
                { 3,   LoadLocationNameList      },
                { 4,   LoadTimeZoneRule          },
                { 100, ToCalendarTime            },
                { 101, ToCalendarTimeWithMyRule  }
            };
        }

        public long GetDeviceLocationName(ServiceCtx Context)
        {
            char[] TzName = TimeZone.Id.ToCharArray();

            Context.ResponseData.Write(TzName);

            int Padding = 0x24 - TzName.Length;

            for (int Index = 0; Index < Padding; Index++)
            {
                Context.ResponseData.Write((byte)0);
            }

            return 0;
        }

        public long SetDeviceLocationName(ServiceCtx Context)
        {
            byte[] LocationName = Context.RequestData.ReadBytes(0x24);
            string TzID         = Encoding.ASCII.GetString(LocationName).TrimEnd('\0');

            long ResultCode = 0;

            try
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById(TzID);
            }
            catch (TimeZoneNotFoundException e)
            {
                ResultCode = 0x7BA74;
            }

            return ResultCode;
        }

        public long GetTotalLocationNameCount(ServiceCtx Context)
        {
            Context.ResponseData.Write(TimeZoneInfo.GetSystemTimeZones().Count);

            return 0;
        }

        public long LoadLocationNameList(ServiceCtx Context)
        {
            long BufferPosition = Context.Response.SendBuff[0].Position;
            long BufferSize     = Context.Response.SendBuff[0].Size;

            int i = 0;
            foreach (TimeZoneInfo info in TimeZoneInfo.GetSystemTimeZones())
            {
                byte[] TzData = Encoding.ASCII.GetBytes(info.Id);

                Context.Memory.WriteBytes(BufferPosition + i, TzData);

                int Padding = 0x24 - TzData.Length;

                for (int Index = 0; Index < Padding; Index++)
                {
                    Context.ResponseData.Write((byte)0);
                }

                i += 0x24;
            }
            return 0;
        }

        public long LoadTimeZoneRule(ServiceCtx Context)
        {
            long BufferPosition = Context.Request.ReceiveBuff[0].Position;
            long BufferSize     = Context.Request.ReceiveBuff[0].Size;

            if (BufferSize != 0x4000)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{BufferSize:x} (expected 0x4000)");
            }

            long ResultCode = 0;

            byte[] LocationName = Context.RequestData.ReadBytes(0x24);
            string TzID         = Encoding.ASCII.GetString(LocationName).TrimEnd('\0');

            // Check if the Time Zone exists, otherwise error out.
            try
            {
                TimeZoneInfo Info = TimeZoneInfo.FindSystemTimeZoneById(TzID);
                byte[] TzData     = Encoding.ASCII.GetBytes(Info.Id);

                // FIXME: This is not in ANY cases accurate, but the games don't about the content of the buffer, they only pass it.
                // TODO: Reverse the TZif2 conversion in PCV to make this match with real hardware.
                Context.Memory.WriteBytes(BufferPosition, TzData);
            }
            catch (TimeZoneNotFoundException e)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceTime, $"Timezone not found for string: {TzID} (len: {TzID.Length})");
                ResultCode = 0x7BA74;
            }

            return ResultCode;
        }

        private long ToCalendarTimeWithTz(ServiceCtx Context, long PosixTime, TimeZoneInfo Info)
        {
            DateTime CurrentTime = Epoch.AddSeconds(PosixTime);
            CurrentTime          = TimeZoneInfo.ConvertTimeFromUtc(CurrentTime, Info);

            Context.ResponseData.Write((ushort)CurrentTime.Year);
            Context.ResponseData.Write((byte)CurrentTime.Month);
            Context.ResponseData.Write((byte)CurrentTime.Day);
            Context.ResponseData.Write((byte)CurrentTime.Hour);
            Context.ResponseData.Write((byte)CurrentTime.Minute);
            Context.ResponseData.Write((byte)CurrentTime.Second);
            Context.ResponseData.Write((byte)0); //MilliSecond ?
            Context.ResponseData.Write((int)CurrentTime.DayOfWeek);
            Context.ResponseData.Write(CurrentTime.DayOfYear - 1);
            Context.ResponseData.Write(new byte[8]); //TODO: Find out the names used.
            Context.ResponseData.Write((byte)(CurrentTime.IsDaylightSavingTime() ? 1 : 0));
            Context.ResponseData.Write((int)Info.GetUtcOffset(CurrentTime).TotalSeconds);

            return 0;
        }

        public long ToCalendarTime(ServiceCtx Context)
        {
            long PosixTime      = Context.RequestData.ReadInt64();
            long BufferPosition = Context.Request.SendBuff[0].Position;
            long BufferSize     = Context.Request.SendBuff[0].Size;

            if (BufferSize != 0x4000)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceTime, $"TimeZoneRule buffer size is 0x{BufferSize:x} (expected 0x4000)");
            }

            // TODO: Reverse the TZif2 conversion in PCV to make this match with real hardware.
            byte[] TzData = Context.Memory.ReadBytes(BufferPosition, 0x24);
            string TzID   = Encoding.ASCII.GetString(TzData).TrimEnd('\0');

            long ResultCode = 0;

            // Check if the Time Zone exists, otherwise error out.
            try
            {
                TimeZoneInfo Info = TimeZoneInfo.FindSystemTimeZoneById(TzID);

                ResultCode = ToCalendarTimeWithTz(Context, PosixTime, Info);
            }
            catch (TimeZoneNotFoundException e)
            {
                Context.Ns.Log.PrintWarning(LogClass.ServiceTime, $"Timezone not found for string: {TzID} (len: {TzID.Length})");
                ResultCode = 0x7BA74;
            }

            return ResultCode;
        }

        public long ToCalendarTimeWithMyRule(ServiceCtx Context)
        {
            long PosixTime = Context.RequestData.ReadInt64();

            return ToCalendarTimeWithTz(Context, PosixTime, TimeZone);
        }
    }
}
