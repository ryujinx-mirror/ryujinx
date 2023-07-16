using Ryujinx.Common;
using Ryujinx.Cpu;
using Ryujinx.HLE.HOS.Services.Time.Clock;

namespace Ryujinx.HLE.HOS.Services.Time.StaticService
{
    class ISteadyClock : IpcService
    {
        private readonly SteadyClockCore _steadyClock;
        private readonly bool _writePermission;
        private readonly bool _bypassUninitializedClock;

        public ISteadyClock(SteadyClockCore steadyClock, bool writePermission, bool bypassUninitializedClock)
        {
            _steadyClock = steadyClock;
            _writePermission = writePermission;
            _bypassUninitializedClock = bypassUninitializedClock;
        }

        [CommandCmif(0)]
        // GetCurrentTimePoint() -> nn::time::SteadyClockTimePoint
        public ResultCode GetCurrentTimePoint(ServiceCtx context)
        {
            if (!_bypassUninitializedClock && !_steadyClock.IsInitialized())
            {
                return ResultCode.UninitializedClock;
            }

            ITickSource tickSource = context.Device.System.TickSource;

            SteadyClockTimePoint currentTimePoint = _steadyClock.GetCurrentTimePoint(tickSource);

            context.ResponseData.WriteStruct(currentTimePoint);

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // GetTestOffset() -> nn::TimeSpanType
        public ResultCode GetTestOffset(ServiceCtx context)
        {
            if (!_bypassUninitializedClock && !_steadyClock.IsInitialized())
            {
                return ResultCode.UninitializedClock;
            }

            context.ResponseData.WriteStruct(_steadyClock.GetTestOffset());

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // SetTestOffset(nn::TimeSpanType)
        public ResultCode SetTestOffset(ServiceCtx context)
        {
            if (!_writePermission)
            {
                return ResultCode.PermissionDenied;
            }

            if (!_bypassUninitializedClock && !_steadyClock.IsInitialized())
            {
                return ResultCode.UninitializedClock;
            }

            TimeSpanType testOffset = context.RequestData.ReadStruct<TimeSpanType>();

            _steadyClock.SetTestOffset(testOffset);

            return ResultCode.Success;
        }

        [CommandCmif(100)] // 2.0.0+
        // GetRtcValue() -> u64
        public ResultCode GetRtcValue(ServiceCtx context)
        {
            if (!_bypassUninitializedClock && !_steadyClock.IsInitialized())
            {
                return ResultCode.UninitializedClock;
            }

            ResultCode result = _steadyClock.GetRtcValue(out ulong rtcValue);

            if (result == ResultCode.Success)
            {
                context.ResponseData.Write(rtcValue);
            }

            return result;
        }

        [CommandCmif(101)] // 2.0.0+
        // IsRtcResetDetected() -> bool
        public ResultCode IsRtcResetDetected(ServiceCtx context)
        {
            if (!_bypassUninitializedClock && !_steadyClock.IsInitialized())
            {
                return ResultCode.UninitializedClock;
            }

            context.ResponseData.Write(_steadyClock.IsRtcResetDetected());

            return ResultCode.Success;
        }

        [CommandCmif(102)] // 2.0.0+
        // GetSetupResultValue() -> u32
        public ResultCode GetSetupResultValue(ServiceCtx context)
        {
            if (!_bypassUninitializedClock && !_steadyClock.IsInitialized())
            {
                return ResultCode.UninitializedClock;
            }

            context.ResponseData.Write((uint)_steadyClock.GetSetupResultValue());

            return ResultCode.Success;
        }

        [CommandCmif(200)] // 3.0.0+
        // GetInternalOffset() -> nn::TimeSpanType
        public ResultCode GetInternalOffset(ServiceCtx context)
        {
            if (!_bypassUninitializedClock && !_steadyClock.IsInitialized())
            {
                return ResultCode.UninitializedClock;
            }

            context.ResponseData.WriteStruct(_steadyClock.GetInternalOffset());

            return ResultCode.Success;
        }

        [CommandCmif(201)] // 3.0.0-3.0.2
        // SetInternalOffset(nn::TimeSpanType)
        public ResultCode SetInternalOffset(ServiceCtx context)
        {
            if (!_writePermission)
            {
                return ResultCode.PermissionDenied;
            }

            if (!_bypassUninitializedClock && !_steadyClock.IsInitialized())
            {
                return ResultCode.UninitializedClock;
            }

            TimeSpanType internalOffset = context.RequestData.ReadStruct<TimeSpanType>();

            _steadyClock.SetInternalOffset(internalOffset);

            return ResultCode.Success;
        }
    }
}
