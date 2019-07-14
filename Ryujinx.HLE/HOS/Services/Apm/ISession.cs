using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Apm
{
    class ISession : IpcService
    {
        public ISession() { }

        [Command(0)]
        // SetPerformanceConfiguration(nn::apm::PerformanceMode, nn::apm::PerformanceConfiguration)
        public ResultCode SetPerformanceConfiguration(ServiceCtx context)
        {
            PerformanceMode          perfMode   = (PerformanceMode)context.RequestData.ReadInt32();
            PerformanceConfiguration perfConfig = (PerformanceConfiguration)context.RequestData.ReadInt32();

            return ResultCode.Success;
        }

        [Command(1)]
        // GetPerformanceConfiguration(nn::apm::PerformanceMode) -> nn::apm::PerformanceConfiguration
        public ResultCode GetPerformanceConfiguration(ServiceCtx context)
        {
            PerformanceMode perfMode = (PerformanceMode)context.RequestData.ReadInt32();

            context.ResponseData.Write((uint)PerformanceConfiguration.PerformanceConfiguration1);

            Logger.PrintStub(LogClass.ServiceApm);

            return ResultCode.Success;
        }
    }
}