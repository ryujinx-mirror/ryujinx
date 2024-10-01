namespace Ryujinx.HLE.HOS.Services.Sockets.Sfdnsres
{
    enum NetDbError
    {
        Internal = -1,
        Success,
        HostNotFound,
        TryAgain,
        NoRecovery,
        NoData,
        NoAddress = NoData,
    }
}
