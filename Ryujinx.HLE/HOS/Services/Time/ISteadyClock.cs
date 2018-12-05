using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Time
{
    class ISteadyClock : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        private ulong TestOffset;

        public ISteadyClock()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 0, GetCurrentTimePoint },
                { 1, GetTestOffset       },
                { 2, SetTestOffset       }
            };

            TestOffset = 0;
        }

        public long GetCurrentTimePoint(ServiceCtx Context)
        {
            Context.ResponseData.Write((long)(System.Diagnostics.Process.GetCurrentProcess().StartTime - DateTime.Now).TotalSeconds);

            for (int i = 0; i < 0x10; i++)
            {
                Context.ResponseData.Write((byte)0);
            }

            return 0;
        }

        public long GetTestOffset(ServiceCtx Context)
        {
            Context.ResponseData.Write(TestOffset);

            return 0;
        }

        public long SetTestOffset(ServiceCtx Context)
        {
            TestOffset = Context.RequestData.ReadUInt64();

            return 0;
        }
    }
}