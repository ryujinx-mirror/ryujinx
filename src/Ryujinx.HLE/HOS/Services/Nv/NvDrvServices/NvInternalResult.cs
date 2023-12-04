namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices
{
    enum NvInternalResult
    {
        Success = 0,
        OperationNotPermitted = -1,
        NoEntry = -2,
        Interrupted = -4,
        IoError = -5,
        DeviceNotFound = -6,
        BadFileNumber = -9,
        TryAgain = -11,
        OutOfMemory = -12,
        AccessDenied = -13,
        BadAddress = -14,
        Busy = -16,
        NotADirectory = -20,
        InvalidInput = -22,
        FileTableOverflow = -23,
        Unknown0x18 = -24,
        NotSupported = -25,
        FileTooBig = -27,
        NoSpaceLeft = -28,
        ReadOnlyAttribute = -30,
        NotImplemented = -38,
        InvalidState = -40,
        Restart = -85,
        InvalidAddress = -99,
        TimedOut = -110,
        Unknown0x72 = -114,
    }
}
