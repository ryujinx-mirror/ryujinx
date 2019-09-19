namespace Ryujinx.HLE.HOS.Services.Friend.ServiceCreator.NotificationService
{
    enum NotificationEventType : uint
    {
        Invalid          = 0x0,
        FriendListUpdate = 0x1,
        NewFriendRequest = 0x65
    }
}