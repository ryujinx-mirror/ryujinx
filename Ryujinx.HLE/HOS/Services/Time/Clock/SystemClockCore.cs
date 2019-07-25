using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    abstract class SystemClockCore
    {
        private SteadyClockCore    _steadyClockCore;
        private SystemClockContext _context;

        public SystemClockCore(SteadyClockCore steadyClockCore)
        {
            _steadyClockCore = steadyClockCore;
            _context         = new SystemClockContext();

            _context.SteadyTimePoint.ClockSourceId = steadyClockCore.GetClockSourceId();
        }

        public virtual SteadyClockCore GetSteadyClockCore()
        {
            return _steadyClockCore;
        }

        public virtual ResultCode GetSystemClockContext(KThread thread, out SystemClockContext context)
        {
            context = _context;

            return ResultCode.Success;
        }

        public virtual ResultCode SetSystemClockContext(SystemClockContext context)
        {
            _context = context;

            return ResultCode.Success;
        }

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
