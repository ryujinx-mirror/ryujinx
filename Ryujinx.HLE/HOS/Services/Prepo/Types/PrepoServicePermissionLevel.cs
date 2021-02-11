using System;

namespace Ryujinx.HLE.HOS.Services.Prepo
{
    enum PrepoServicePermissionLevel
    {
        Admin   = -1,
        User    = 1,
        System  = 2,
        Manager = 6
    }
}