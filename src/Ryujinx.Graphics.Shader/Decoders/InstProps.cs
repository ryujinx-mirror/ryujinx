using System;

namespace Ryujinx.Graphics.Shader.Decoders
{
    [Flags]
    enum InstProps : ushort
    {
        None = 0,
        Rd = 1 << 0,
        Rd2 = 1 << 1,
        Ra = 1 << 2,
        Rb = 1 << 3,
        Rb2 = 1 << 4,
        Ib = 1 << 5,
        Rc = 1 << 6,

        Pd = 1 << 7,
        LPd = 2 << 7,
        SPd = 3 << 7,
        TPd = 4 << 7,
        VPd = 5 << 7,
        PdMask = 7 << 7,

        Pdn = 1 << 10,
        Ps = 1 << 11,
        Tex = 1 << 12,
        TexB = 1 << 13,
        Bra = 1 << 14,
        NoPred = 1 << 15,
    }
}
