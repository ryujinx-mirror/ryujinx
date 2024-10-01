namespace Ryujinx.Graphics.GAL
{
    public enum BlendOp
    {
        Add = 1,
        Subtract,
        ReverseSubtract,
        Minimum,
        Maximum,

        AddGl = 0x8006,
        MinimumGl = 0x8007,
        MaximumGl = 0x8008,
        SubtractGl = 0x800a,
        ReverseSubtractGl = 0x800b,
    }
}
