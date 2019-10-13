namespace Ryujinx.Graphics.Shader.Decoders
{
    enum IntegerCondition
    {
        Less    = 1 << 0,
        Equal   = 1 << 1,
        Greater = 1 << 2,

        Never = 0,

        LessOrEqual    = Less    | Equal,
        NotEqual       = Less    | Greater,
        GreaterOrEqual = Greater | Equal,
        Number         = Greater | Equal | Less,

        Always = 7
    }
}