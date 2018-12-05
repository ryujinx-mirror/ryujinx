using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Time
{
    class ISystemClock : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private SystemClockType ClockType;

        private DateTime SystemClockContextEpoch;

        private long SystemClockTimePoint;

        private byte[] SystemClockContextEnding;

        private long TimeOffset;

        public ISystemClock(SystemClockType ClockType)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetCurrentTime        },
                { 1, SetCurrentTime        },
                { 2, GetSystemClockContext },
                { 3, SetSystemClockContext }
            };

            this.ClockType           = ClockType;
            SystemClockContextEpoch  = System.Diagnostics.Process.GetCurrentProcess().StartTime;
            SystemClockContextEnding = new byte[0x10];
            TimeOffset               = 0;

            if (ClockType == SystemClockType.User ||
                ClockType == SystemClockType.Network)
            {
                SystemClockContextEpoch = SystemClockContextEpoch.ToUniversalTime();
            }

            SystemClockTimePoint = (long)(SystemClockContextEpoch - Epoch).TotalSeconds;
        }

        public long GetCurrentTime(ServiceCtx Context)
        {
            DateTime CurrentTime = DateTime.Now;

            if (ClockType == SystemClockType.User ||
                ClockType == SystemClockType.Network)
            {
                CurrentTime = CurrentTime.ToUniversalTime();
            }

            Context.ResponseData.Write((long)((CurrentTime - Epoch).TotalSeconds) + TimeOffset);

            return 0;
        }

        public long SetCurrentTime(ServiceCtx Context)
        {
            DateTime CurrentTime = DateTime.Now;

            if (ClockType == SystemClockType.User ||
                ClockType == SystemClockType.Network)
            {
                CurrentTime = CurrentTime.ToUniversalTime();
            }

            TimeOffset = (Context.RequestData.ReadInt64() - (long)(CurrentTime - Epoch).TotalSeconds);

            return 0;
        }

        public long GetSystemClockContext(ServiceCtx Context)
        {
            Context.ResponseData.Write((long)(SystemClockContextEpoch - Epoch).TotalSeconds);

            // The point in time, TODO: is there a link between epoch and this?
            Context.ResponseData.Write(SystemClockTimePoint);

            // This seems to be some kind of identifier?
            for (int i = 0; i < 0x10; i++)
            {
                Context.ResponseData.Write(SystemClockContextEnding[i]);
            }

            return 0;
        }

        public long SetSystemClockContext(ServiceCtx Context)
        {
            long NewSystemClockEpoch     = Context.RequestData.ReadInt64();
            long NewSystemClockTimePoint = Context.RequestData.ReadInt64();

            SystemClockContextEpoch      = Epoch.Add(TimeSpan.FromSeconds(NewSystemClockEpoch));
            SystemClockTimePoint         = NewSystemClockTimePoint;
            SystemClockContextEnding     = Context.RequestData.ReadBytes(0x10);

            return 0;
        }
    }
}