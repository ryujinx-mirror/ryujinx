using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class TickBasedSteadyClockCore : SteadyClockCore
    {
        private static TickBasedSteadyClockCore _instance;

        public static TickBasedSteadyClockCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TickBasedSteadyClockCore();
                }

                return _instance;
            }
        }

        private TickBasedSteadyClockCore() {}

        public override SteadyClockTimePoint GetTimePoint(KThread thread)
        {
            SteadyClockTimePoint result = new SteadyClockTimePoint
            {
                TimePoint     = 0,
                ClockSourceId = GetClockSourceId()
            };

            TimeSpanType ticksTimeSpan = TimeSpanType.FromTicks(thread.Context.CntpctEl0, thread.Context.CntfrqEl0);

            result.TimePoint = ticksTimeSpan.ToSeconds();

            return result;
        }
    }
}
