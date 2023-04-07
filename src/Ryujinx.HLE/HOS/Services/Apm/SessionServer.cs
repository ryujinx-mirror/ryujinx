using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Apm
{
    class SessionServer : ISession
    {
        private readonly ServiceCtx _context;

        public SessionServer(ServiceCtx context) : base(context) 
        {
            _context = context;
        }

        protected override ResultCode SetPerformanceConfiguration(PerformanceMode performanceMode, PerformanceConfiguration performanceConfiguration)
        {
            if (performanceMode > PerformanceMode.Boost)
            {
                return ResultCode.InvalidParameters;
            }

            switch (performanceMode)
            {
                case PerformanceMode.Default:
                    _context.Device.System.PerformanceState.DefaultPerformanceConfiguration = performanceConfiguration;
                    break;
                case PerformanceMode.Boost:
                    _context.Device.System.PerformanceState.BoostPerformanceConfiguration = performanceConfiguration;
                    break;
                default:
                    Logger.Error?.Print(LogClass.ServiceApm, $"PerformanceMode isn't supported: {performanceMode}");
                    break;
            }

            return ResultCode.Success;
        }

        protected override ResultCode GetPerformanceConfiguration(PerformanceMode performanceMode, out PerformanceConfiguration performanceConfiguration)
        {
            if (performanceMode > PerformanceMode.Boost)
            {
                performanceConfiguration = 0;

                return ResultCode.InvalidParameters;
            }

            performanceConfiguration = _context.Device.System.PerformanceState.GetCurrentPerformanceConfiguration(performanceMode);

            return ResultCode.Success;
        }

        protected override void SetCpuOverclockEnabled(bool enabled)
        {
            _context.Device.System.PerformanceState.CpuOverclockEnabled = enabled;

            // NOTE: This call seems to overclock the system, since we emulate it, it's fine to do nothing instead.
        }
    }
}