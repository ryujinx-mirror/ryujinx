using Ryujinx.Common;
using Ryujinx.HLE.HOS.Services.Time.Clock;

namespace Ryujinx.HLE.HOS.Services.Time
{
    class ISteadyClock : IpcService
    {
        [Command(0)]
        // GetCurrentTimePoint() -> nn::time::SteadyClockTimePoint
        public ResultCode GetCurrentTimePoint(ServiceCtx context)
        {
            SteadyClockTimePoint currentTimePoint = SteadyClockCore.Instance.GetCurrentTimePoint(context.Thread);

            context.ResponseData.WriteStruct(currentTimePoint);

            return ResultCode.Success;
        }

        [Command(1)]
        // GetTestOffset() -> nn::TimeSpanType
        public ResultCode GetTestOffset(ServiceCtx context)
        {
            context.ResponseData.WriteStruct(SteadyClockCore.Instance.GetTestOffset());

            return ResultCode.Success;
        }

        [Command(2)]
        // SetTestOffset(nn::TimeSpanType)
        public ResultCode SetTestOffset(ServiceCtx context)
        {
            TimeSpanType testOffset = context.RequestData.ReadStruct<TimeSpanType>();

            SteadyClockCore.Instance.SetTestOffset(testOffset);

            return 0;
        }

        [Command(100)] // 2.0.0+
        // GetRtcValue() -> u64
        public ResultCode GetRtcValue(ServiceCtx context)
        {
            ResultCode result = SteadyClockCore.Instance.GetRtcValue(out ulong rtcValue);

            if (result == ResultCode.Success)
            {
                context.ResponseData.Write(rtcValue);
            }

            return result;
        }

        [Command(101)] // 2.0.0+
        // IsRtcResetDetected() -> bool
        public ResultCode IsRtcResetDetected(ServiceCtx context)
        {
            context.ResponseData.Write(SteadyClockCore.Instance.IsRtcResetDetected());

            return ResultCode.Success;
        }

        [Command(102)] // 2.0.0+
        // GetSetupResultValue() -> u32
        public ResultCode GetSetupResultValue(ServiceCtx context)
        {
            context.ResponseData.Write((uint)SteadyClockCore.Instance.GetSetupResultCode());

            return ResultCode.Success;
        }

        [Command(200)] // 3.0.0+
        // GetInternalOffset() -> nn::TimeSpanType
        public ResultCode GetInternalOffset(ServiceCtx context)
        {
            context.ResponseData.WriteStruct(SteadyClockCore.Instance.GetInternalOffset());

            return ResultCode.Success;
        }

        [Command(201)] // 3.0.0-3.0.2
        // SetInternalOffset(nn::TimeSpanType)
        public ResultCode SetInternalOffset(ServiceCtx context)
        {
            TimeSpanType internalOffset = context.RequestData.ReadStruct<TimeSpanType>();

            SteadyClockCore.Instance.SetInternalOffset(internalOffset);

            return ResultCode.Success;
        }
    }
}