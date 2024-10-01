namespace Ryujinx.Graphics.GAL
{
    public enum CompareOp
    {
        Never = 1,
        Less,
        Equal,
        LessOrEqual,
        Greater,
        NotEqual,
        GreaterOrEqual,
        Always,

        NeverGl = 0x200,
        LessGl = 0x201,
        EqualGl = 0x202,
        LessOrEqualGl = 0x203,
        GreaterGl = 0x204,
        NotEqualGl = 0x205,
        GreaterOrEqualGl = 0x206,
        AlwaysGl = 0x207,
    }
}
