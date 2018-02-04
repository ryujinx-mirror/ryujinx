using System;

namespace ChocolArm64.Translation
{
    [Flags]
    enum AIoType
    {
        Arg,
        Fields,
        Flag,
        Int,
        Float,
        Vector,
        Mask    = 0xff,
        VectorI = Vector | 1 << 8,
        VectorF = Vector | 1 << 9
    }
}