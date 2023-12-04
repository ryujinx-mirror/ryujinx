namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    enum NetworkState
    {
        None,
        Initialized,
        AccessPoint,
        AccessPointCreated,
        Station,
        StationConnected,
        Error,
    }
}
