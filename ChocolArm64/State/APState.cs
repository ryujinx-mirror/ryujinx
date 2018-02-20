using System;

namespace ChocolArm64.State
{
    [Flags]
    public enum APState
    {
        VBit = 28,
        CBit = 29,
        ZBit = 30,
        NBit = 31,

        V = 1 << VBit,
        C = 1 << CBit,
        Z = 1 << ZBit,
        N = 1 << NBit,

        NZ = N | Z,
        CV = C | V,

        NZCV = NZ | CV
    }
}