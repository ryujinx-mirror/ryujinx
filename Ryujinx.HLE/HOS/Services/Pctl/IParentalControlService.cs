using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Pctl
{
    class IParentalControlService : IpcService
    {
        private bool _initialized = false;

        private bool _needInitialize;

        public IParentalControlService(bool needInitialize = true)
        {
            _needInitialize = needInitialize;
        }

        [Command(1)] // 4.0.0+
        // Initialize()
        public ResultCode Initialize(ServiceCtx context)
        {
            if (_needInitialize && !_initialized)
            {
                _initialized = true;
            }
            else
            {
                Logger.PrintWarning(LogClass.ServicePctl, "Service is already initialized!");
            }

            return ResultCode.Success;
        }

        [Command(1001)]
        // CheckFreeCommunicationPermission()
        public ResultCode CheckFreeCommunicationPermission(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServicePctl);

            return ResultCode.Success;
        }
    }
}