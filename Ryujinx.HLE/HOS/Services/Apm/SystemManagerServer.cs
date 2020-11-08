namespace Ryujinx.HLE.HOS.Services.Apm
{
    [Service("apm:sys")]
    class SystemManagerServer : ISystemManager
    {
        private readonly ServiceCtx _context;

        public SystemManagerServer(ServiceCtx context) : base(context)
        {
            _context = context;
        }

        protected override void RequestPerformanceMode(PerformanceMode performanceMode)
        {
            _context.Device.System.PerformanceState.PerformanceMode = performanceMode;
        }

        internal override void SetCpuBoostMode(CpuBoostMode cpuBoostMode)
        {
            _context.Device.System.PerformanceState.CpuBoostMode = cpuBoostMode;
        }

        protected override PerformanceConfiguration GetCurrentPerformanceConfiguration()
        {
            return _context.Device.System.PerformanceState.GetCurrentPerformanceConfiguration(_context.Device.System.PerformanceState.PerformanceMode);
        }
    }
}