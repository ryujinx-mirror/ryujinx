using Ryujinx.Common;
using Ryujinx.HLE.HOS.Services.Time.Clock;

namespace Ryujinx.HLE.HOS.Services.Time
{
    class ISystemClock : IpcService
    {
        private SystemClockCore _clockCore;
        private bool            _writePermission;

        public ISystemClock(SystemClockCore clockCore, bool writePermission)
        {
            _clockCore       = clockCore;
            _writePermission = writePermission;
        }

        [Command(0)]
        // GetCurrentTime() -> nn::time::PosixTime
        public ResultCode GetCurrentTime(ServiceCtx context)
        {
            SteadyClockCore      steadyClockCore  = _clockCore.GetSteadyClockCore();
            SteadyClockTimePoint currentTimePoint = steadyClockCore.GetCurrentTimePoint(context.Thread);

            ResultCode result = _clockCore.GetSystemClockContext(context.Thread, out SystemClockContext clockContext);

            if (result == ResultCode.Success)
            {
                result = ResultCode.TimeMismatch;

                if (currentTimePoint.ClockSourceId == clockContext.SteadyTimePoint.ClockSourceId)
                {
                    ulong posixTime = clockContext.Offset + currentTimePoint.TimePoint;

                    context.ResponseData.Write(posixTime);

                    result = 0;
                }
            }

            return result;
        }

        [Command(1)]
        // SetCurrentTime(nn::time::PosixTime)
        public ResultCode SetCurrentTime(ServiceCtx context)
        {
            if (!_writePermission)
            {
                return ResultCode.PermissionDenied;
            }

            ulong                posixTime        = context.RequestData.ReadUInt64();
            SteadyClockCore      steadyClockCore  = _clockCore.GetSteadyClockCore();
            SteadyClockTimePoint currentTimePoint = steadyClockCore.GetCurrentTimePoint(context.Thread);

            SystemClockContext clockContext = new SystemClockContext()
            {
                Offset          = posixTime - currentTimePoint.TimePoint,
                SteadyTimePoint = currentTimePoint
            };

            ResultCode result = _clockCore.SetSystemClockContext(clockContext);

            if (result == ResultCode.Success)
            {
                result = _clockCore.Flush(clockContext);
            }

            return result;
        }

        [Command(2)]
        // GetSystemClockContext() -> nn::time::SystemClockContext
        public ResultCode GetSystemClockContext(ServiceCtx context)
        {
            ResultCode result = _clockCore.GetSystemClockContext(context.Thread, out SystemClockContext clockContext);

            if (result == ResultCode.Success)
            {
                context.ResponseData.WriteStruct(clockContext);
            }

            return result;
        }

        [Command(3)]
        // SetSystemClockContext(nn::time::SystemClockContext)
        public ResultCode SetSystemClockContext(ServiceCtx context)
        {
            if (!_writePermission)
            {
                return ResultCode.PermissionDenied;
            }

            SystemClockContext clockContext = context.RequestData.ReadStruct<SystemClockContext>();

            ResultCode result = _clockCore.SetSystemClockContext(clockContext);

            if (result == ResultCode.Success)
            {
                result = _clockCore.Flush(clockContext);
            }

            return result;
        }
    }
}