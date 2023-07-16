using System;

namespace Ryujinx.HLE.HOS.Services.Friend.ServiceCreator
{
    [Flags]
    enum FriendServicePermissionLevel
    {
        UserMask = 1,
        ViewerMask = 2,
        ManagerMask = 4,
        SystemMask = 8,

        Administrator = -1,
        User = UserMask,
        Viewer = UserMask | ViewerMask,
        Manager = UserMask | ViewerMask | ManagerMask,
        System = UserMask | SystemMask,
    }
}
