using System;

namespace Ryujinx.HLE.HOS.Services.Friend
{
    [Flags]
    enum FriendServicePermissionLevel
    {
        UserMask    = 1,
        OverlayMask = 2,
        ManagerMask = 4,
        SystemMask  = 8,

        Admin   = -1,
        User    = UserMask,
        Overlay = UserMask | OverlayMask,
        Manager = UserMask | OverlayMask | ManagerMask,
        System  = UserMask | SystemMask
    }
}