namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    enum DisconnectReason : uint
    {
        None,
        DisconnectedByUser,
        DisconnectedBySystem,
        DestroyedByUser,
        DestroyedBySystem,
        Rejected,
        SignalLost,
    }
}
