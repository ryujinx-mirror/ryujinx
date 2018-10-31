using System;

namespace ChocolArm64.Translation
{
    [Flags]
    enum IoType
    {
        Arg,
        Fields,
        Flag,
        Int,
        Float,
        Vector
    }
}