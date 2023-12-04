using Ryujinx.Cpu;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class TickBasedSteadyClockCore : SteadyClockCore
    {
        public TickBasedSteadyClockCore() { }

        public override SteadyClockTimePoint GetTimePoint(ITickSource tickSource)
        {
            SteadyClockTimePoint result = new()
            {
                TimePoint = 0,
                ClockSourceId = GetClockSourceId(),
            };

            TimeSpanType ticksTimeSpan = TimeSpanType.FromTicks(tickSource.Counter, tickSource.Frequency);

            result.TimePoint = ticksTimeSpan.ToSeconds();

            return result;
        }
    }
}
