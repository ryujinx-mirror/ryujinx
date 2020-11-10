namespace Ryujinx.Graphics.Shader.Decoders
{
    enum AtomicOp
    {
        Add                = 0,
        Minimum            = 1,
        Maximum            = 2,
        Increment          = 3,
        Decrement          = 4,
        BitwiseAnd         = 5,
        BitwiseOr          = 6,
        BitwiseExclusiveOr = 7,
        Swap               = 8,
        SafeAdd            = 10 // Only supported by ATOM.
    }
}