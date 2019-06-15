using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Services.Arp;
using Ryujinx.HLE.Utilities;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Acc
{
    class IManagerForApplication : IpcService
    {
        private UInt128 _userId;

        private ApplicationLaunchProperty _applicationLaunchProperty;

        private Dictionary<int, ServiceProcessRequest> _commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => _commands;

        public IManagerForApplication(UInt128 userId, ApplicationLaunchProperty applicationLaunchProperty)
        {
            _commands = new Dictionary<int, ServiceProcessRequest>
            {
                { 0, CheckAvailability },
                { 1, GetAccountId      }
            };

            _userId                    = userId;
            _applicationLaunchProperty = applicationLaunchProperty;
        }

        // CheckAvailability()
        public long CheckAvailability(ServiceCtx context)
        {
            Logger.PrintStub(LogClass.ServiceAcc);

            return 0;
        }

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