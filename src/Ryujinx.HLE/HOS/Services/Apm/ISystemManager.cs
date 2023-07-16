namespace Ryujinx.HLE.HOS.Services.Apm
{
    abstract class ISystemManager : IpcService
    {
        public ISystemManager(ServiceCtx context) { }

        protected abstract void RequestPerformanceMode(PerformanceMode performanceMode);
        internal abstract void SetCpuBoostMode(CpuBoostMode cpuBoostMode);
        protected abstract PerformanceConfiguration GetCurrentPerformanceConfiguration();

        [CommandCmif(0)]
        // RequestPerformanceMode(nn::apm::PerformanceMode)
        public ResultCode RequestPerformanceMode(ServiceCtx context)
        {
            RequestPerformanceMode((PerformanceMode)context.RequestData.ReadInt32());

            // NOTE: This call seems to overclock the system related to the PerformanceMode, since we emulate it, it's fine to do nothing instead.

            return ResultCode.Success;
        }

        [CommandCmif(6)] // 7.0.0+
        // SetCpuBoostMode(nn::apm::CpuBootMode)
        public ResultCode SetCpuBoostMode(ServiceCtx context)
        {
            SetCpuBoostMode((CpuBoostMode)context.RequestData.ReadUInt32());

            // NOTE: This call seems to overclock the system related to the CpuBoostMode, since we emulate it, it's fine to do nothing instead.

            return ResultCode.Success;
        }

        [CommandCmif(7)] // 7.0.0+
        // GetCurrentPerformanceConfiguration() -> nn::apm::PerformanceConfiguration
        public ResultCode GetCurrentPerformanceConfiguration(ServiceCtx context)
        {
            context.ResponseData.Write((uint)GetCurrentPerformanceConfiguration());

            return ResultCode.Success;
        }
    }
}
