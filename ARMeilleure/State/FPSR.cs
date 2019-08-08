using System;

namespace ARMeilleure.State
{
    [Flags]
    public enum FPSR
    {
        Ufc = 1 << 3,
        Qc  = 1 << 27
    }
}
