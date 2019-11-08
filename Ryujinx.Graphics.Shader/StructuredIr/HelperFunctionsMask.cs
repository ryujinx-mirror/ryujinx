using System;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    [Flags]
    enum HelperFunctionsMask
    {
        GlobalMemory = 1 << 0,
        Shuffle      = 1 << 1,
        ShuffleDown  = 1 << 2,
        ShuffleUp    = 1 << 3,
        ShuffleXor   = 1 << 4,
        SwizzleAdd   = 1 << 5
    }
}