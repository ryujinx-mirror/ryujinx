namespace Ryujinx.Graphics.Gal
{
    public enum GalBlendEquation
    {
        FuncAdd             = 1,
        FuncSubtract        = 2,
        FuncReverseSubtract = 3,
        Min                 = 4,
        Max                 = 5,

        FuncAddGl             = 0x8006,
        FuncSubtractGl        = 0x8007,
        FuncReverseSubtractGl = 0x8008,
        MinGl                 = 0x800a,
        MaxGl                 = 0x800b
    }
}