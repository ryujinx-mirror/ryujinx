using System;

namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    [Flags]
    enum TextureFlags
    {
        None      = 0,
        Bindless  = 1 << 0,
        Gather    = 1 << 1,
        IntCoords = 1 << 2,
        LodBias   = 1 << 3,
        LodLevel  = 1 << 4,
        Offset    = 1 << 5,
        Offsets   = 1 << 6
    }
}