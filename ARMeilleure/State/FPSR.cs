using System;

namespace ARMeilleure.State
{
    [Flags]
    public enum FPSR : uint
    {
        Ufc = 1 << 3,
        Qc  = 1 << 27,

        A32Mask = 0xf800000f
    }
}
