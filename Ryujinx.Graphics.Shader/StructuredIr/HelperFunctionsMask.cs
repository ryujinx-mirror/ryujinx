using System;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    [Flags]
    enum HelperFunctionsMask
    {
        Shuffle     = 1 << 0,
        ShuffleDown = 1 << 1,
        ShuffleUp   = 1 << 2,
        ShuffleXor  = 1 << 3,
        SwizzleAdd  = 1 << 4
    }
}