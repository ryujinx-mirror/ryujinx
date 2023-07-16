namespace Ryujinx.HLE.HOS.Services.Apm
{
    abstract class IManager : IpcService
    {
        public IManager(ServiceCtx context) { }

        protected abstract ResultCode OpenSession(out SessionServer sessionServer);
        protected abstract PerformanceMode GetPerformanceMode();
        protected abstract bool IsCpuOverclockEnabled();

        [CommandCmif(0)]
        // OpenSession() -> object<nn::apm::ISession>
        public ResultCode OpenSession(ServiceCtx context)
        {
            ResultCode resultCode = OpenSession(out SessionServer sessionServer);

            if (resultCode == ResultCode.Success)
            {
                MakeObject(context, sessionServer);
            }

            return resultCode;
        }

        [CommandCmif(1)]
        // GetPerformanceMode() -> nn::apm::PerformanceMode
        public ResultCode GetPerformanceMode(ServiceCtx context)
        {
            context.ResponseData.Write((uint)GetPerformanceMode());

            return ResultCode.Success;
        }

        [CommandCmif(6)] // 7.0.0+
        // IsCpuOverclockEnabled() -> bool
        public ResultCode IsCpuOverclockEnabled(ServiceCtx context)
        {
            context.ResponseData.Write(IsCpuOverclockEnabled());

            return ResultCode.Success;
        }
    }
}
