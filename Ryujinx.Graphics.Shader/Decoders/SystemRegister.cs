namespace Ryujinx.Graphics.Shader.Decoders
{
    enum SystemRegister
    {
        LaneId     = 0,
        YDirection = 0x12,
        ThreadId   = 0x20,
        ThreadIdX  = 0x21,
        ThreadIdY  = 0x22,
        ThreadIdZ  = 0x23,
        CtaIdX     = 0x25,
        CtaIdY     = 0x26,
        CtaIdZ     = 0x27,
        EqMask     = 0x38,
        LtMask     = 0x39,
        LeMask     = 0x3a,
        GtMask     = 0x3b,
        GeMask     = 0x3c
    }
}