namespace Ryujinx.Graphics.GAL.Blend
{
    public enum BlendOp
    {
        Add = 1,
        Subtract,
        ReverseSubtract,
        Minimum,
        Maximum,

        AddGl             = 0x8006,
        SubtractGl        = 0x8007,
        ReverseSubtractGl = 0x8008,
        MinimumGl         = 0x800a,
        MaximumGl         = 0x800b
    }
}
