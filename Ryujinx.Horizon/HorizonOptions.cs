namespace Ryujinx.Horizon
{
    public struct HorizonOptions
    {
        public bool IgnoreMissingServices { get; }

        public HorizonOptions(bool ignoreMissingServices)
        {
            IgnoreMissingServices = ignoreMissingServices;
        }
    }
}
