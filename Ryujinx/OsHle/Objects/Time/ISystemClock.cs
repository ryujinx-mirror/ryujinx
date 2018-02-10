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

        public ISystemClock()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetCurrentTime }
            };
        }

        public long GetCurrentTime(ServiceCtx Context)
        {
            Context.ResponseData.Write((long)(DateTime.Now - Epoch).TotalSeconds);

            return 0;
        }
    }
}