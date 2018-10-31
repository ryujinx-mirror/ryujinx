using System;

namespace ChocolArm64.State
{
    [Flags]
    enum PState
    {
        VBit = 28,
        CBit = 29,
        ZBit = 30,
        NBit = 31,

        V = 1 << VBit,
        C = 1 << CBit,
        Z = 1 << ZBit,
        N = 1 << NBit,

        Nz = N | Z,
        Cv = C | V,

        Nzcv = Nz | Cv
    }
}
