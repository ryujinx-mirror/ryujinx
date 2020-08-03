using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Arp;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    class IManagerForApplication : IpcService
    {
        private UserId                    _userId;
        private ApplicationLaunchProperty _applicationLaunchProperty;

        private const long NetworkServiceAccountId = 0xcafe;

        public IManagerForApplication(UserId userId, ApplicationLaunchProperty applicationLaunchProperty)
        {
            _userId                    = userId;
            _applicationLaunchProperty = applicationLaunchProperty;
        }

        [Command(0)]
        // CheckAvailability()
        public ResultCode CheckAvailability(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAcc);

            return ResultCode.Success;
        }

        [Command(1)]
        // GetAccountId() -> nn::account::NetworkServiceAccountId
        public ResultCode GetAccountId(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAcc, new { NetworkServiceAccountId });

            context.ResponseData.Write(NetworkServiceAccountId);

            return ResultCode.Success;
        }

        [Command(130)]
        // GetNintendoAccountUserResourceCacheForApplication() -> (nn::account::NintendoAccountId, buffer<nn::account::nas::NasUserBaseForApplication, 0x1a>, buffer<bytes, 6>)
        public ResultCode GetNintendoAccountUserResourceCacheForApplication(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAcc, new { NetworkServiceAccountId });

            context.ResponseData.Write(NetworkServiceAccountId);

            // TODO: determine and fill the two output IPC buffers.

            return ResultCode.Success;
        }
    }
}