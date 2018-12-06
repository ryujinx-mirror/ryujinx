namespace Ryujinx.HLE.HOS.Services.Sfdnsres
{
    enum NetDbError
    {
        Internal = -1,
        Success,
        HostNotFound,
        TryAgain,
        NoRecovery,
        NoData,
        NoAddress = NoData
    }
}
