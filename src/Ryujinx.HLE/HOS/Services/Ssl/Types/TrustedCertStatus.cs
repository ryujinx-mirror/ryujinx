namespace Ryujinx.HLE.HOS.Services.Ssl.Types
{
    enum TrustedCertStatus : uint
    {
        Removed,
        EnabledTrusted,
        EnabledNotTrusted,
        Revoked,

        Invalid = uint.MaxValue,
    }
}
