namespace Ryujinx.HLE.HOS.Services.Ssl.Types
{
    enum OptionType : uint
    {
        DoNotCloseSocket,
        GetServerCertChain, // 3.0.0+
        SkipDefaultVerify,  // 5.0.0+
        EnableAlpn, // 9.0.0+
    }
}
