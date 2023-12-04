namespace Ryujinx.HLE.HOS.Services.Spl.Types
{
    enum SmcResult
    {
        Success = 0,
        NotImplemented = 1,
        InvalidArgument = 2,
        Busy = 3,
        NoAsyncOperation = 4,
        InvalidAsyncOperation = 5,
        NotPermitted = 6,
        NotInitialized = 7,

        PsciNotSupported = -1,
        PsciInvalidParameters = -2,
        PsciDenied = -3,
        PsciAlreadyOn = -4,
    }
}
