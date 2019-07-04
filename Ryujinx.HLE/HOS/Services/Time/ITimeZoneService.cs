using ChocolArm64.Memory;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Services.Time.TimeZone;
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

        public ITimeZoneService()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0,   GetDeviceLocationName               },
                { 1,   SetDeviceLocationName               },
                { 2,   GetTotalLocationNameCount           },
                { 3,   LoadLocationNameList                },
                { 4,   LoadTimeZoneRule                    },
              //{ 5,   GetTimeZoneRuleVersion              }, // 2.0.0+
              //{ 6,   GetDeviceLocationNameAndUpdatedTime }, // 5.0.0+
                { 100, ToCalendarTime                      },
                { 101, ToCalendarTimeWithMyRule            },
                { 201, ToPosixTime                         },
                { 202, ToPosixTimeWithMyRule               }
            };
        }

        // GetDeviceLocationName() -> nn::time::LocationName
        public long GetDeviceLocationName(ServiceCtx context)
        {
            char[] tzName = TimeZoneManager.Instance.GetDeviceLocationName().ToCharArray();

            int padding = 0x24 - tzName.Length;

            if (padding < 0)
            {
                return MakeError(ErrorModule.Time, TimeError.LocationNameTooLong);
            }

            context.ResponseData.Write(tzName);

            for (int index = 0; index < padding; index++)
            {
                context.ResponseData.Write((byte)0);
            }

            return 0;
        }

        // SetDeviceLocationName(nn::time::LocationName)
        public long SetDeviceLocationName(ServiceCtx context)
        {
            string locationName = Encoding.ASCII.GetString(context.RequestData.ReadBytes(0x24)).TrimEnd('\0');

            return TimeZoneManager.Instance.SetDeviceLocationName(locationName);
        }

        // GetTotalLocationNameCount() -> u32
        public long GetTotalLocationNameCount(ServiceCtx context)
        {
            context.ResponseData.Write(TimeZoneManager.Instance.GetTotalLocationNameCount());

            return 0;
        }

        // LoadLocationNameList(u32 index) -> (u32 outCount, buffer<nn::time::LocationName, 6>)
        public long LoadLocationNameList(ServiceCtx context)
        {
            // TODO: fix logic to use index
            uint index          = context.RequestData.ReadUInt32();
            long bufferPosition = context.Request.ReceiveBuff[0].Position;
            long bufferSize     = context.Request.ReceiveBuff[0].Size;

            uint errorCode = TimeZoneManager.Instance.LoadLocationNameList(index, out string[] locationNameArray, (uint)bufferSize / 0x24);

            if (errorCode == 0)
            {
                uint offset = 0;

                foreach (string locationName in locationNameArray)
                {
                    int padding = 0x24 - locationName.Length;

                    if (padding < 0)
                    {
                        return MakeError(ErrorModule.Time, TimeError.LocationNameTooLong);
                    }

                    context.Memory.WriteBytes(bufferPosition + offset, Encoding.ASCII.GetBytes(locationName));
                    MemoryHelper.FillWithZeros(context.Memory, bufferPosition + offset + locationName.Length, padding);

                    offset += 0x24;
                }

                context.ResponseData.Write((uint)locationNameArray.Length);
            }

            return errorCode;
        }

        // LoadTimeZoneRule(nn::time::LocationName locationName) -> buffer<nn::time::TimeZoneRule, 0x16>
        public long LoadTimeZoneRule(ServiceCtx context)
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

            long resultCode = TimeZoneManager.Instance.LoadTimeZoneRules(out TimeZoneRule rules, locationName);
            
            // Write TimeZoneRule if success
            if (resultCode == 0)
            {
                MemoryHelper.Write(context.Memory, bufferPosition, rules);
            }

            return resultCode;
        }

        // ToCalendarTime(nn::time::PosixTime time, buffer<nn::time::TimeZoneRule, 0x15> rules) -> (nn::time::CalendarTime, nn::time::sf::CalendarAdditionalInfo)
        public long ToCalendarTime(ServiceCtx context)
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

            long resultCode = TimeZoneManager.ToCalendarTime(rules, posixTime, out CalendarInfo calendar);

            if (resultCode == 0)
            {
                context.ResponseData.WriteStruct(calendar);
            }

            return resultCode;
        }

        // ToCalendarTimeWithMyRule(nn::time::PosixTime) -> (nn::time::CalendarTime, nn::time::sf::CalendarAdditionalInfo)
        public long ToCalendarTimeWithMyRule(ServiceCtx context)
        {
            long posixTime = context.RequestData.ReadInt64();

            long resultCode = TimeZoneManager.Instance.ToCalendarTimeWithMyRules(posixTime, out CalendarInfo calendar);

            if (resultCode == 0)
            {
                context.ResponseData.WriteStruct(calendar);
            }

            return resultCode;
        }

        // ToPosixTime(nn::time::CalendarTime calendarTime, buffer<nn::time::TimeZoneRule, 0x15> rules) -> (u32 outCount, buffer<nn::time::PosixTime, 0xa>)
        public long ToPosixTime(ServiceCtx context)
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

            long resultCode = TimeZoneManager.ToPosixTime(rules, calendarTime, out long posixTime);

            if (resultCode == 0)
            {
                long outBufferPosition = context.Request.RecvListBuff[0].Position;
                long outBufferSize     = context.Request.RecvListBuff[0].Size;

                context.Memory.WriteInt64(outBufferPosition, posixTime);
                context.ResponseData.Write(1);
            }

            return resultCode;
        }

        // ToPosixTimeWithMyRule(nn::time::CalendarTime calendarTime) -> (u32 outCount, buffer<nn::time::PosixTime, 0xa>)
        public long ToPosixTimeWithMyRule(ServiceCtx context)
        {
            CalendarTime calendarTime = context.RequestData.ReadStruct<CalendarTime>();

            long resultCode = TimeZoneManager.Instance.ToPosixTimeWithMyRules(calendarTime, out long posixTime);

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
