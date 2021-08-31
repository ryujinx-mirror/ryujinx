using System;

namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    [Flags]
    enum TextureFlags
    {
        None        = 0,
        Bindless    = 1 << 0,
        Gather      = 1 << 1,
        Derivatives = 1 << 2,
        IntCoords   = 1 << 3,
        LodBias     = 1 << 4,
        LodLevel    = 1 << 5,
        Offset      = 1 << 6,
        Offsets     = 1 << 7,

        AtomicMask  = 15 << 16,

        Add         = 0 << 16,
        Minimum     = 1 << 16,
        Maximum     = 2 << 16,
        Increment   = 3 << 16,
        Decrement   = 4 << 16,
        BitwiseAnd  = 5 << 16,
        BitwiseOr   = 6 << 16,
        BitwiseXor  = 7 << 16,
        Swap        = 8 << 16,
        CAS         = 9 << 16
    }
}