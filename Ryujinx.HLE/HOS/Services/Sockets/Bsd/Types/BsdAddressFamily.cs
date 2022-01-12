namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd
{
    enum BsdAddressFamily : uint
    {
        Unspecified,
        InterNetwork = 2,
        InterNetworkV6 = 28,

        Unknown = uint.MaxValue
    }
}
