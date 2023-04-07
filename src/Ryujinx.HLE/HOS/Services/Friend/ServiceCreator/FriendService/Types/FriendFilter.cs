using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Friend.ServiceCreator.FriendService
{
    [StructLayout(LayoutKind.Sequential)]
    struct FriendFilter
    {
        public PresenceStatusFilter PresenceStatus;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsFavoriteOnly;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsSameAppPresenceOnly;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsSameAppPlayedOnly;

        [MarshalAs(UnmanagedType.I1)]
        public bool IsArbitraryAppPlayedOnly;

        public long PresenceGroupId;
    }
}