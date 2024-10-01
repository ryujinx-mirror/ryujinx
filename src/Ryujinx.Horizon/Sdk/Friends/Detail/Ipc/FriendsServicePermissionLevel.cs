namespace Ryujinx.Horizon.Sdk.Friends.Detail.Ipc
{
    enum FriendsServicePermissionLevel
    {
        UserMask = 1,
        ViewerMask = 2,
        ManagerMask = 4,
        SystemMask = 8,

        Admin = -1,
        User = UserMask,
        Viewer = UserMask | ViewerMask,
        Manager = UserMask | ViewerMask | ManagerMask,
        System = UserMask | SystemMask,
    }
}
