using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.Utilities;
using System;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    abstract class SteadyClockCore
    {
        private UInt128 _clockSourceId;
        private bool    _isRtcResetDetected;
        private bool    _isInitialized;

        public SteadyClockCore()
        {
            _clockSourceId      = new UInt128(Guid.NewGuid().ToByteArray());
            _isRtcResetDetected = false;
            _isInitialized      = false;
        }

        public UInt128 GetClockSourceId()
        {
            return _clockSourceId;
        }

        public void SetClockSourceId(UInt128 clockSourceId)
        {
            _clockSourceId = clockSourceId;
        }

        public void SetRtcReset()
        {
            _isRtcResetDetected = true;
        }

        public virtual TimeSpanType GetTestOffset()
        {
            return new TimeSpanType(0);
        }

        public virtual void SetTestOffset(TimeSpanType testOffset) {}

        public ResultCode GetRtcValue(out ulong rtcValue)
        {
            rtcValue = 0;

            return ResultCode.NotImplemented;
        }

        public bool IsRtcResetDetected()
        {
            return _isRtcResetDetected;
        }

        public ResultCode GetSetupResultValue()
        {
            return ResultCode.Success;
        }

        public virtual TimeSpanType GetInternalOffset()
        {
            return new TimeSpanType(0);
        }

        public virtual void SetInternalOffset(TimeSpanType internalOffset) {}

        public virtual SteadyClockTimePoint GetTimePoint(KThread thread)
        {
            throw new NotImplementedException();
        }

        public virtual TimeSpanType GetCurrentRawTimePoint(KThread thread)
        {
            SteadyClockTimePoint timePoint = GetTimePoint(thread);

            return TimeSpanType.FromSeconds(timePoint.TimePoint);
        }

        public SteadyClockTimePoint GetCurrentTimePoint(KThread thread)
        {
            SteadyClockTimePoint result = GetTimePoint(thread);

            result.TimePoint += GetTestOffset().ToSeconds();
            result.TimePoint += GetInternalOffset().ToSeconds();

            return result;
        }

        public bool IsInitialized()
        {
            return _isInitialized;
        }

        public void MarkInitialized()
        {
            _isInitialized = true;
        }
    }
}
