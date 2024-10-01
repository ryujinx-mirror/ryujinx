namespace Ryujinx.HLE.HOS.Services.Apm
{
    [Service("apm")]
    [Service("apm:am")] // 8.0.0+
    class ManagerServer : IManager
    {
        private readonly ServiceCtx _context;

        public ManagerServer(ServiceCtx context) : base(context)
        {
            _context = context;
        }

        protected override ResultCode OpenSession(out SessionServer sessionServer)
        {
            sessionServer = new SessionServer(_context);

            return ResultCode.Success;
        }

        protected override PerformanceMode GetPerformanceMode()
        {
            return _context.Device.System.PerformanceState.PerformanceMode;
        }

        protected override bool IsCpuOverclockEnabled()
        {
            return _context.Device.System.PerformanceState.CpuOverclockEnabled;
        }
    }
}
