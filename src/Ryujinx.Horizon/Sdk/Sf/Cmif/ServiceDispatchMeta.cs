namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    struct ServiceDispatchMeta
    {
        public ServiceDispatchTableBase DispatchTable { get; }

        public ServiceDispatchMeta(ServiceDispatchTableBase dispatchTable)
        {
            DispatchTable = dispatchTable;
        }
    }
}
