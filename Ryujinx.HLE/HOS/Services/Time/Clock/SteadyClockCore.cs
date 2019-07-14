using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.Utilities;
using System;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class SteadyClockCore
    {
        private TimeSpanType _testOffset;
        private TimeSpanType _internalOffset;
        private UInt128      _clockSourceId;

        private static SteadyClockCore instance;

        public static SteadyClockCore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SteadyClockCore();
                }

                return instance;
            }
        }

        private SteadyClockCore()
        {
            _testOffset     = new TimeSpanType(0);
            _internalOffset = new TimeSpanType(0);
            _clockSourceId  = new UInt128(Guid.NewGuid().ToByteArray());
        }

        private SteadyClockTimePoint GetTimePoint(KThread thread)
        {
            SteadyClockTimePoint result = new SteadyClockTimePoint
            {
                TimePoint     = 0,
                ClockSourceId = _clockSourceId
            };

            TimeSpanType ticksTimeSpan = TimeSpanType.FromTicks(thread.Context.ThreadState.CntpctEl0, thread.Context.ThreadState.CntfrqEl0);

            result.TimePoint = _internalOffset.ToSeconds() + ticksTimeSpan.ToSeconds();

            return result;
        }

        public UInt128 GetClockSourceId()
        {
            return _clockSourceId;
        }

        public SteadyClockTimePoint GetCurrentTimePoint(KThread thread)
        {
            SteadyClockTimePoint result = GetTimePoint(thread);

            result.TimePoint += _testOffset.ToSeconds();

            return result;
        }

        public TimeSpanType GetTestOffset()
        {
            return _testOffset;
        }

        public void SetTestOffset(TimeSpanType testOffset)
        {
            _testOffset = testOffset;
        }

        // TODO: check if this is accurate
        public TimeSpanType GetInternalOffset()
        {
            return _internalOffset;
        }

        // TODO: check if this is accurate
        public void SetInternalOffset(TimeSpanType internalOffset)
        {
            _internalOffset = internalOffset;
        }
    }
}
