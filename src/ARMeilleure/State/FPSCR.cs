using System;

namespace ARMeilleure.State
{
    [Flags]
    public enum FPSCR : uint
    {
        V = 1u << 28,
        C = 1u << 29,
        Z = 1u << 30,
        N = 1u << 31,

        Mask = N | Z | C | V | FPSR.Mask | FPCR.Mask, // 0xFFC09F9Fu
    }
}
