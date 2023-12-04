namespace Ryujinx.HLE.HOS.Applets.Browser
{
    public enum WebExitReason : uint
    {
        ExitButton,
        BackButton,
        Requested,
        LastUrl,
        ErrorDialog = 7,
    }
}
