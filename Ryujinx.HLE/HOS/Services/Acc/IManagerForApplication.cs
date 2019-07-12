using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Arp;
using Ryujinx.HLE.Utilities;

namespace Ryujinx.HLE.HOS.Services.Acc
{
    class IManagerForApplication : IpcService
    {
        private UInt128                   _userId;
        private ApplicationLaunchProperty _applicationLaunchProperty;

        public IManagerForApplication(UInt128 userId, ApplicationLaunchProperty applicationLaunchProperty)
        {
            _userId                    = userId;
            _applicationLaunchProperty = applicationLaunchProperty;
        }

        [Command(0)]
        // CheckAvailability()
        public long CheckAvailability(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAcc);

            return 0;
        }

        [Command(1)]
        // GetAccountId() -> nn::account::NetworkServiceAccountId
        public long GetAccountId(ServiceCtx context)
        {
            long networkServiceAccountId = 0xcafe;

            Logger.PrintStub(LogClass.ServiceAcc, new { networkServiceAccountId });

            context.ResponseData.Write(networkServiceAccountId);

            return 0;
        }
    }
}