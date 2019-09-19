using System;

namespace Ryujinx.HLE.HOS.Services.Time
{
    [Flags]
    enum TimePermissions
    {
        LocalSystemClockWritableMask   = 0x1,
        UserSystemClockWritableMask    = 0x2,
        NetworkSystemClockWritableMask = 0x4,
        UnknownPermissionMask          = 0x8,

        User   = 0,
        Applet = LocalSystemClockWritableMask | UserSystemClockWritableMask | UnknownPermissionMask,
        System = NetworkSystemClockWritableMask
    }
}
