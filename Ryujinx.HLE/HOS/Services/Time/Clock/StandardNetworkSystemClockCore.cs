using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Time.Clock
{
    class StandardNetworkSystemClockCore : SystemClockCore
    {
        private SteadyClockCore    _steadyClockCore;
        private SystemClockContext _context;

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

            _context.SteadyTimePoint.ClockSourceId = steadyClockCore.GetClockSourceId();
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
    }
}
