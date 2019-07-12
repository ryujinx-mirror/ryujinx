using System;

namespace Ryujinx.HLE.HOS.Services.Time
{
    class ISteadyClock : IpcService
    {
        private ulong _testOffset;

        public ISteadyClock()
        {
            _testOffset = 0;
        }

        [Command(0)]
        // GetCurrentTimePoint() -> nn::time::SteadyClockTimePoint
        public long GetCurrentTimePoint(ServiceCtx context)
        {
            context.ResponseData.Write((long)(System.Diagnostics.Process.GetCurrentProcess().StartTime - DateTime.Now).TotalSeconds);

            for (int i = 0; i < 0x10; i++)
            {
                context.ResponseData.Write((byte)0);
            }

            return 0;
        }

        [Command(1)]
        // GetTestOffset() -> nn::TimeSpanType
        public long GetTestOffset(ServiceCtx context)
        {
            context.ResponseData.Write(_testOffset);

            return 0;
        }

        [Command(2)]
        // SetTestOffset(nn::TimeSpanType)
        public long SetTestOffset(ServiceCtx context)
        {
            _testOffset = context.RequestData.ReadUInt64();

            return 0;
        }
    }
}