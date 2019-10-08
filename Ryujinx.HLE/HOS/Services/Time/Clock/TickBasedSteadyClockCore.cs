using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class TickBasedSteadyClockCore : SteadyClockCore
    {
        public TickBasedSteadyClockCore() {}

        public override SteadyClockTimePoint GetTimePoint(KThread thread)
        {
            SteadyClockTimePoint result = new SteadyClockTimePoint
            {
                TimePoint     = 0,
                ClockSourceId = GetClockSourceId()
            };

            TimeSpanType ticksTimeSpan;

            // As this may be called before the guest code, we support passing a null thread to make this api usable.
            if (thread == null)
            {
                ticksTimeSpan = TimeSpanType.FromSeconds(0);
            }
            else
            {
                ticksTimeSpan = TimeSpanType.FromTicks(thread.Context.CntpctEl0, thread.Context.CntfrqEl0);
            }

            result.TimePoint = ticksTimeSpan.ToSeconds();

            return result;
        }
    }
}
