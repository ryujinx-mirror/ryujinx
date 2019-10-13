namespace Ryujinx.Graphics.Shader.Decoders
{
    enum Condition
    {
        Less     = 1 << 0,
        Equal    = 1 << 1,
        Greater  = 1 << 2,
        Nan      = 1 << 3,
        Unsigned = 1 << 4,

        Never = 0,

        LessOrEqual    = Less    | Equal,
        NotEqual       = Less    | Greater,
        GreaterOrEqual = Greater | Equal,
        Number         = Greater | Equal | Less,

        LessUnordered           = Less           | Nan,
        EqualUnordered          = Equal          | Nan,
        LessOrEqualUnordered    = LessOrEqual    | Nan,
        GreaterUnordered        = Greater        | Nan,
        NotEqualUnordered       = NotEqual       | Nan,
        GreaterOrEqualUnordered = GreaterOrEqual | Nan,

        Always = 0xf,

        Off          = Unsigned | Never,
        Lower        = Unsigned | Less,
        Sff          = Unsigned | Equal,
        LowerOrSame  = Unsigned | LessOrEqual,
        Higher       = Unsigned | Greater,
        Sft          = Unsigned | NotEqual,
        HigherOrSame = Unsigned | GreaterOrEqual,
        Oft          = Unsigned | Always,

        CsmTa  = 0x18,
        CsmTr  = 0x19,
        CsmMx  = 0x1a,
        FcsmTa = 0x1b,
        FcsmTr = 0x1c,
        FcsmMx = 0x1d,
        Rle    = 0x1e,
        Rgt    = 0x1f
    }
}