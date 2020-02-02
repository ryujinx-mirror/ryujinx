using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Arp;

namespace Ryujinx.HLE.HOS.Services.Account.Acc
{
    class IManagerForApplication : IpcService
    {
        private UserId                    _userId;
        private ApplicationLaunchProperty _applicationLaunchProperty;

        public IManagerForApplication(UserId userId, ApplicationLaunchProperty applicationLaunchProperty)
        {
            _userId                    = userId;
            _applicationLaunchProperty = applicationLaunchProperty;
        }

        [Command(0)]
        // CheckAvailability()
        public ResultCode CheckAvailability(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAcc);

            return ResultCode.Success;
        }

        [Command(1)]
        // GetAccountId() -> nn::account::NetworkServiceAccountId
        public ResultCode GetAccountId(ServiceCtx context)
        {
            long networkServiceAccountId = 0xcafe;

            Logger.PrintStub(LogClass.ServiceAcc, new { networkServiceAccountId });

            context.ResponseData.Write(networkServiceAccountId);

            return ResultCode.Success;
        }
    }
}