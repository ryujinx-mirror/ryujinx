namespace Ryujinx.HLE.HOS.Services.Friend
{
    enum PresenceStatusFilter
    {
        None,
        Online,
        OnlinePlay,
        OnlineOrOnlinePlay
    }

    struct FriendFilter
    {
        public PresenceStatusFilter PresenceStatus;
        public bool                 IsFavoriteOnly;
        public bool                 IsSameAppPresenceOnly;
        public bool                 IsSameAppPlayedOnly;
        public bool                 IsArbitraryAppPlayedOnly;
        public long                 PresenceGroupId;
    }
}
