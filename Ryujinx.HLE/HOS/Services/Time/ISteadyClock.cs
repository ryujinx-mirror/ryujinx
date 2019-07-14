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
        public ResultCode GetCurrentTimePoint(ServiceCtx context)
        {
            context.ResponseData.Write((long)(System.Diagnostics.Process.GetCurrentProcess().StartTime - DateTime.Now).TotalSeconds);

            for (int i = 0; i < 0x10; i++)
            {
                context.ResponseData.Write((byte)0);
            }

            return ResultCode.Success;
        }

        [Command(1)]
        // GetTestOffset() -> nn::TimeSpanType
        public ResultCode GetTestOffset(ServiceCtx context)
        {
            context.ResponseData.Write(_testOffset);

            return ResultCode.Success;
        }

        [Command(2)]
        // SetTestOffset(nn::TimeSpanType)
        public ResultCode SetTestOffset(ServiceCtx context)
        {
            _testOffset = context.RequestData.ReadUInt64();

            return ResultCode.Success;
        }
    }
}