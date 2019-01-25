using System;

namespace ChocolArm64.State
{
    [Flags]
    enum PState
    {
        TBit = 5,

        VBit = 28,
        CBit = 29,
        ZBit = 30,
        NBit = 31,

        T = 1 << TBit,

        V = 1 << VBit,
        C = 1 << CBit,
        Z = 1 << ZBit,
        N = 1 << NBit,

        Nz = N | Z,
        Cv = C | V,

        Nzcv = Nz | Cv
    }
}
