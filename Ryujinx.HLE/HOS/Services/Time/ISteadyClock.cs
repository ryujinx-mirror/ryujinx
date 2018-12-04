using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Time
{
    class ISteadyClock : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        private ulong _testOffset;

        public ISteadyClock()
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, GetCurrentTimePoint },
                { 1, GetTestOffset       },
                { 2, SetTestOffset       }
            };

            _testOffset = 0;
        }

        public long GetCurrentTimePoint(ServiceCtx context)
        {
            context.ResponseData.Write((long)(System.Diagnostics.Process.GetCurrentProcess().StartTime - DateTime.Now).TotalSeconds);

            for (int i = 0; i < 0x10; i++)
            {
                context.ResponseData.Write((byte)0);
            }

            return 0;
        }

        public long GetTestOffset(ServiceCtx context)
        {
            context.ResponseData.Write(_testOffset);

            return 0;
        }

        public long SetTestOffset(ServiceCtx context)
        {
            _testOffset = context.RequestData.ReadUInt64();

            return 0;
        }
    }
}