namespace Ryujinx.Horizon
{
    public struct HorizonOptions
    {
        public bool IgnoreMissingServices    { get; }
        public bool ThrowOnInvalidCommandIds { get; }

        public HorizonOptions(bool ignoreMissingServices)
        {
            IgnoreMissingServices    = ignoreMissingServices;
            ThrowOnInvalidCommandIds = true;
        }
    }
}
