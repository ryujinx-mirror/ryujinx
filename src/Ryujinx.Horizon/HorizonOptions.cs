using LibHac;
using Ryujinx.Horizon.Sdk.Account;
using Ryujinx.Horizon.Sdk.Fs;

namespace Ryujinx.Horizon
{
    public readonly struct HorizonOptions
    {
        public bool IgnoreMissingServices { get; }
        public bool ThrowOnInvalidCommandIds { get; }

        public HorizonClient BcatClient { get; }
        public IFsClient FsClient { get; }
        public IEmulatorAccountManager AccountManager { get; }

        public HorizonOptions(bool ignoreMissingServices, HorizonClient bcatClient, IFsClient fsClient, IEmulatorAccountManager accountManager)
        {
            IgnoreMissingServices = ignoreMissingServices;
            ThrowOnInvalidCommandIds = true;
            BcatClient = bcatClient;
            FsClient = fsClient;
            AccountManager = accountManager;
        }
    }
}
