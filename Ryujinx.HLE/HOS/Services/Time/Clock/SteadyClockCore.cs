using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.Utilities;
using System;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    abstract class SteadyClockCore
    {
        private UInt128 _clockSourceId;

        public SteadyClockCore()
        {
            _clockSourceId = new UInt128(Guid.NewGuid().ToByteArray());
        }

        public UInt128 GetClockSourceId()
        {
            return _clockSourceId;
        }

        public virtual TimeSpanType GetTestOffset()
        {
            return new TimeSpanType(0);
        }

        public virtual void SetTestOffset(TimeSpanType testOffset) {}

        public virtual ResultCode GetRtcValue(out ulong rtcValue)
        {
            rtcValue = 0;

            return ResultCode.NotImplemented;
        }

        public virtual ResultCode GetSetupResultValue()
        {
            return ResultCode.NotImplemented;
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

        public SteadyClockTimePoint GetCurrentTimePoint(KThread thread)
        {
            SteadyClockTimePoint result = GetTimePoint(thread);

            result.TimePoint += GetTestOffset().ToSeconds();
            result.TimePoint += GetInternalOffset().ToSeconds();

            return result;
        }
    }
}
