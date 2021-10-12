namespace Ryujinx.Graphics.Shader.Decoders
{
    enum InstProps : ushort
    {
        None = 0,
        Rd = 1 << 0,
        Rd2 = 1 << 1,
        Ra = 1 << 2,
        Rb = 1 << 3,
        Ib = 1 << 4,
        Rc = 1 << 5,
        Pd = 1 << 6,
        Pd2 = 1 << 7,
        Pdn = 1 << 8,
        Tex = 1 << 9,
        TexB = 1 << 10,
        Bra = 1 << 11,
        NoPred = 1 << 12
    }
}