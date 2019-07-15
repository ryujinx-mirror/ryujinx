using System;
using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class StandardNetworkSystemClockCore : SystemClockCore
    {
        private SteadyClockCore    _steadyClockCore;
        private SystemClockContext _context;
        private TimeSpanType       _standardNetworkClockSufficientAccuracy;

        private static StandardNetworkSystemClockCore instance;

        public static StandardNetworkSystemClockCore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StandardNetworkSystemClockCore(SteadyClockCore.Instance);
                }

                return instance;
            }
        }

        public StandardNetworkSystemClockCore(SteadyClockCore steadyClockCore)
        {
            _steadyClockCore = steadyClockCore;
            _context         = new SystemClockContext();

            _context.SteadyTimePoint.ClockSourceId  = steadyClockCore.GetClockSourceId();
            _standardNetworkClockSufficientAccuracy = new TimeSpanType(0);
        }

        public override ResultCode Flush(SystemClockContext context)
        {
            // TODO: set:sys SetNetworkSystemClockContext

            return ResultCode.Success;
        }

        public override SteadyClockCore GetSteadyClockCore()
        {
            return _steadyClockCore;
        }

        public override ResultCode GetSystemClockContext(KThread thread, out SystemClockContext context)
        {
            context = _context;

            return ResultCode.Success;
        }

        public override ResultCode SetSystemClockContext(SystemClockContext context)
        {
            _context = context;

            return ResultCode.Success;
        }

        public bool IsStandardNetworkSystemClockAccuracySufficient(KThread thread)
        {
            SteadyClockCore      steadyClockCore  = GetSteadyClockCore();
            SteadyClockTimePoint currentTimePoint = steadyClockCore.GetCurrentTimePoint(thread);

            bool isStandardNetworkClockSufficientAccuracy = false;

            if (_context.SteadyTimePoint.GetSpanBetween(currentTimePoint, out long outSpan) == ResultCode.Success)
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
