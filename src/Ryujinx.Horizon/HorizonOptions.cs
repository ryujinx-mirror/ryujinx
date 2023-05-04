using LibHac;

namespace Ryujinx.Horizon
{
    public struct HorizonOptions
    {
        public bool IgnoreMissingServices    { get; }
        public bool ThrowOnInvalidCommandIds { get; }

        public HorizonClient BcatClient { get; }

        public HorizonOptions(bool ignoreMissingServices, HorizonClient bcatClient)
        {
            IgnoreMissingServices    = ignoreMissingServices;
            ThrowOnInvalidCommandIds = true;
            BcatClient               = bcatClient;
        }
    }
}
