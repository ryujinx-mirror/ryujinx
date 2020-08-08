using System;

namespace ARMeilleure.State
{
    [Flags]
    public enum FPSR : uint
    {
        Ufc = 1u << 3,
        Qc  = 1u << 27,

        Nzcv = (1u << 31) | (1u << 30) | (1u << 29) | (1u << 28),

        A32Mask = 0xF800009Fu
    }
}
