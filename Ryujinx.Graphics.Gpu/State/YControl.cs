using System;

namespace Ryujinx.Graphics.Gpu.State
{
    [Flags]
    enum YControl
    {
        NegateY          = 1 << 0,
        TriangleRastFlip = 1 << 4
    }
}