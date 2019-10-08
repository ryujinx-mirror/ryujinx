using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class StandardNetworkSystemClockCore : SystemClockCore
    {
        private TimeSpanType _standardNetworkClockSufficientAccuracy;

        public StandardNetworkSystemClockCore(StandardSteadyClockCore steadyClockCore) : base(steadyClockCore)
        {
            _standardNetworkClockSufficientAccuracy = new TimeSpanType(0);
        }

        public bool IsStandardNetworkSystemClockAccuracySufficient(KThread thread)
        {
            SteadyClockCore      steadyClockCore  = GetSteadyClockCore();
            SteadyClockTimePoint currentTimePoint = steadyClockCore.GetCurrentTimePoint(thread);

            bool isStandardNetworkClockSufficientAccuracy = false;

            ResultCode result = GetClockContext(thread, out SystemClockContext context);

            if (result == ResultCode.Success && context.SteadyTimePoint.GetSpanBetween(currentTimePoint, out long outSpan) == ResultCode.Success)
            {
                isStandardNetworkClockSufficientAccuracy = outSpan * 1000000000 < _standardNetworkClockSufficientAccuracy.NanoSeconds;
            }

            return isStandardNetworkClockSufficientAccuracy;
        }

        public void SetStandardNetworkClockSufficientAccuracy(TimeSpanType standardNetworkClockSufficientAccuracy)
        {
            _standardNetworkClockSufficientAccuracy = standardNetworkClockSufficientAccuracy;
        }
    }
}
