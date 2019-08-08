namespace ARMeilleure.CodeGen.X86
{
    enum X86Condition
    {
        Overflow       = 0x0,
        NotOverflow    = 0x1,
        Below          = 0x2,
        AboveOrEqual   = 0x3,
        Equal          = 0x4,
        NotEqual       = 0x5,
        BelowOrEqual   = 0x6,
        Above          = 0x7,
        Sign           = 0x8,
        NotSign        = 0x9,
        ParityEven     = 0xa,
        ParityOdd      = 0xb,
        Less           = 0xc,
        GreaterOrEqual = 0xd,
        LessOrEqual    = 0xe,
        Greater        = 0xf
    }
}