using Ryujinx.HLE.HOS.Services.Account.Acc.AsyncContext;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    class IAsyncNetworkServiceLicenseKindContext : IAsyncContext
    {
        private readonly NetworkServiceLicenseKind? _serviceLicenseKind;

        public IAsyncNetworkServiceLicenseKindContext(AsyncExecution asyncExecution, NetworkServiceLicenseKind? serviceLicenseKind) : base(asyncExecution)
        {
            _serviceLicenseKind = serviceLicenseKind;
        }

        [CommandCmif(100)]
        // GetNetworkServiceLicenseKind() -> nn::account::NetworkServiceLicenseKind
        public ResultCode GetNetworkServiceLicenseKind(ServiceCtx context)
        {
            if (!AsyncExecution.IsInitialized)
            {
                return ResultCode.AsyncExecutionNotInitialized;
            }

            if (!AsyncExecution.SystemEvent.ReadableEvent.IsSignaled())
            {
                return ResultCode.Unknown41;
            }

            if (!_serviceLicenseKind.HasValue)
            {
                return ResultCode.MissingNetworkServiceLicenseKind;
            }

            context.ResponseData.Write((uint)_serviceLicenseKind.Value);

            return ResultCode.Success;
        }
    }
}
