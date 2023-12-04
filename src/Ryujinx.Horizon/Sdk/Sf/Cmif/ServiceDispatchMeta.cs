namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    readonly struct ServiceDispatchMeta
    {
        public ServiceDispatchTableBase DispatchTable { get; }

        public ServiceDispatchMeta(ServiceDispatchTableBase dispatchTable)
        {
            DispatchTable = dispatchTable;
        }
    }
}
