namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    enum AcceptPolicy : byte
    {
        AcceptAll,
        RejectAll,
        BlackList,
        WhiteList,
    }
}
