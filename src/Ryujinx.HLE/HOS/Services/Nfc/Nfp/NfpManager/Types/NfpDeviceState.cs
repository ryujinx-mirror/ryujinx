namespace Ryujinx.HLE.HOS.Services.Nfc.Nfp.NfpManager
{
    enum NfpDeviceState
    {
        Initialized = 0,
        SearchingForTag = 1,
        TagFound = 2,
        TagRemoved = 3,
        TagMounted = 4,
        Unavailable = 5,
        Finalized = 6,
    }
}
