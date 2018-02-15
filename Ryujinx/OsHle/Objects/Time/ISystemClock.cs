using Ryujinx.OsHle.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.OsHle.Objects.Time
{
    class ISystemClock : IIpcInterface
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private SystemClockType ClockType;

        public ISystemClock(SystemClockType ClockType)
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetCurrentTime }
            };

            this.ClockType = ClockType;
        }

        public long GetCurrentTime(ServiceCtx Context)
        {
            DateTime CurrentTime = DateTime.Now;

            if (ClockType == SystemClockType.User ||
                ClockType == SystemClockType.Network)
            {
                CurrentTime = CurrentTime.ToUniversalTime();
            }

            Context.ResponseData.Write((long)(DateTime.Now - Epoch).TotalSeconds);

            return 0;
        }
    }
}