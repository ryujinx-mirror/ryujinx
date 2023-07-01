using LibHac;

namespace Ryujinx.Horizon
{
    public readonly struct HorizonOptions
    {
        public bool IgnoreMissingServices { get; }
        public bool ThrowOnInvalidCommandIds { get; }

        public HorizonClient BcatClient { get; }

        public HorizonOptions(bool ignoreMissingServices, HorizonClient bcatClient)
        {
            IgnoreMissingServices = ignoreMissingServices;
            ThrowOnInvalidCommandIds = true;
            BcatClient = bcatClient;
        }
    }
}
