using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Friends.Detail.Ipc
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10, Pack = 0x8)]
    struct SizedFriendFilter
    {
        public PresenceStatusFilter PresenceStatus;
        public bool IsFavoriteOnly;
        public bool IsSameAppPresenceOnly;
        public bool IsSameAppPlayedOnly;
        public bool IsArbitraryAppPlayedOnly;
        public ulong PresenceGroupId;

        public readonly override string ToString()
        {
            return $"{{ PresenceStatus: {PresenceStatus}, " +
                $"IsFavoriteOnly: {IsFavoriteOnly}, " +
                $"IsSameAppPresenceOnly: {IsSameAppPresenceOnly}, " +
                $"IsSameAppPlayedOnly: {IsSameAppPlayedOnly}, " +
                $"IsArbitraryAppPlayedOnly: {IsArbitraryAppPlayedOnly}, " +
                $"PresenceGroupId: {PresenceGroupId} }}";
        }
    }
}
