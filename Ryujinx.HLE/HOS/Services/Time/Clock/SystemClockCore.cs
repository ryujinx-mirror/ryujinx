using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    abstract class SystemClockCore
    {
        public abstract SteadyClockCore GetSteadyClockCore();

        public abstract ResultCode GetSystemClockContext(KThread thread, out SystemClockContext context);

        public abstract ResultCode SetSystemClockContext(SystemClockContext context);

        public abstract ResultCode Flush(SystemClockContext context);

        public bool IsClockSetup(KThread thread)
        {
            ResultCode result = GetSystemClockContext(thread, out SystemClockContext context);

            if (result == ResultCode.Success)
            {
                SteadyClockCore steadyClockCore = GetSteadyClockCore();

                SteadyClockTimePoint steadyClockTimePoint = steadyClockCore.GetCurrentTimePoint(thread);

                return steadyClockTimePoint.ClockSourceId == context.SteadyTimePoint.ClockSourceId;
            }

            return false;
        }
    }
}
