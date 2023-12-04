using System;

namespace Ryujinx.HLE.HOS.Services.Time
{
    [Flags]
    enum TimePermissions
    {
        LocalSystemClockWritableMask = 0x1,
        UserSystemClockWritableMask = 0x2,
        NetworkSystemClockWritableMask = 0x4,
        TimeZoneWritableMask = 0x8,
        SteadyClockWritableMask = 0x10,
        BypassUninitialized = 0x20,

        User = 0,
        Admin = LocalSystemClockWritableMask | UserSystemClockWritableMask | TimeZoneWritableMask,
        System = NetworkSystemClockWritableMask,
        SystemUpdate = BypassUninitialized,
        Repair = SteadyClockWritableMask,
        Manufacture = LocalSystemClockWritableMask | UserSystemClockWritableMask | NetworkSystemClockWritableMask | TimeZoneWritableMask | SteadyClockWritableMask,
    }
}
