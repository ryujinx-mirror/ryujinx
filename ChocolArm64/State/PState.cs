using System;

namespace ChocolArm64.State
{
    [Flags]
    enum PState
    {
        TBit = 5,
        EBit = 9,

        VBit = 28,
        CBit = 29,
        ZBit = 30,
        NBit = 31,

        TMask = 1 << TBit,
        EMask = 1 << EBit,

        VMask = 1 << VBit,
        CMask = 1 << CBit,
        ZMask = 1 << ZBit,
        NMask = 1 << NBit
    }
}
