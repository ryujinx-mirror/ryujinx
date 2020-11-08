namespace Ryujinx.HLE.HOS.Services.Apm
{
    abstract class ISession : IpcService
    {
        public ISession(ServiceCtx context) { }

        protected abstract ResultCode SetPerformanceConfiguration(PerformanceMode performanceMode, PerformanceConfiguration performanceConfiguration);
        protected abstract ResultCode GetPerformanceConfiguration(PerformanceMode performanceMode, out PerformanceConfiguration performanceConfiguration);
        protected abstract void SetCpuOverclockEnabled(bool enabled);

        [Command(0)]
        // SetPerformanceConfiguration(nn::apm::PerformanceMode, nn::apm::PerformanceConfiguration)
        public ResultCode SetPerformanceConfiguration(ServiceCtx context)
        {
            PerformanceMode          performanceMode          = (PerformanceMode)context.RequestData.ReadInt32();
            PerformanceConfiguration performanceConfiguration = (PerformanceConfiguration)context.RequestData.ReadInt32();

            return SetPerformanceConfiguration(performanceMode, performanceConfiguration);
        }

        [Command(1)]
        // GetPerformanceConfiguration(nn::apm::PerformanceMode) -> nn::apm::PerformanceConfiguration
        public ResultCode GetPerformanceConfiguration(ServiceCtx context)
        {
            PerformanceMode performanceMode = (PerformanceMode)context.RequestData.ReadInt32();

            ResultCode resultCode = GetPerformanceConfiguration(performanceMode, out PerformanceConfiguration performanceConfiguration);

            context.ResponseData.Write((uint)performanceConfiguration);

            return resultCode;
        }

        [Command(2)] // 8.0.0+
        // SetCpuOverclockEnabled(bool)
        public ResultCode SetCpuOverclockEnabled(ServiceCtx context)
        {
            bool enabled = context.RequestData.ReadBoolean();

            SetCpuOverclockEnabled(enabled);

            return ResultCode.Success;
        }
    }
}