using System;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    [Flags]
    enum HelperFunctionsMask
    {
        MultiplyHighS32 = 1 << 0,
        MultiplyHighU32 = 1 << 1,
        Shuffle         = 1 << 2,
        ShuffleDown     = 1 << 3,
        ShuffleUp       = 1 << 4,
        ShuffleXor      = 1 << 5,
        SwizzleAdd      = 1 << 6
    }
}