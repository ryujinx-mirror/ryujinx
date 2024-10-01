using System;

namespace ARMeilleure.State
{
    [Flags]
    public enum FPCR : uint
    {
        Ioe = 1u << 8,
        Dze = 1u << 9,
        Ofe = 1u << 10,
        Ufe = 1u << 11,
        Ixe = 1u << 12,
        Ide = 1u << 15,
        RMode0 = 1u << 22,
        RMode1 = 1u << 23,
        Fz = 1u << 24,
        Dn = 1u << 25,
        Ahp = 1u << 26,

        Mask = Ahp | Dn | Fz | RMode1 | RMode0 | Ide | Ixe | Ufe | Ofe | Dze | Ioe, // 0x07C09F00u
    }
}
