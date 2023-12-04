namespace Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.LdnMitm.Types
{
    internal enum LanPacketType : byte
    {
        Scan,
        ScanResponse,
        Connect,
        SyncNetwork,
    }
}
