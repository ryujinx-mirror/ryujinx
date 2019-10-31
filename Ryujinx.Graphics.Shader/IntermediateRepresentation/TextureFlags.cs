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
        Offsets     = 1 << 7
    }
}