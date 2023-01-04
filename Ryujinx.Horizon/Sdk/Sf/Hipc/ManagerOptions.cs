namespace Ryujinx.Horizon.Sdk.Sf.Hipc
{
    struct ManagerOptions
    {
        public static ManagerOptions Default => new ManagerOptions(0, 0, 0, false);

        public int PointerBufferSize { get; }
        public int MaxDomains { get; }
        public int MaxDomainObjects { get; }
        public bool CanDeferInvokeRequest { get; }

        public ManagerOptions(int pointerBufferSize, int maxDomains, int maxDomainObjects, bool canDeferInvokeRequest)
        {
            PointerBufferSize = pointerBufferSize;
            MaxDomains = maxDomains;
            MaxDomainObjects = maxDomainObjects;
            CanDeferInvokeRequest = canDeferInvokeRequest;
        }
    }
}
