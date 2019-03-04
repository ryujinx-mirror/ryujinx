using System;

namespace Ryujinx.Graphics.Texture
{
    [Flags]
    public enum TextureInstructionSuffix
    {
        None  = 0x00,  // No Modifier
        Lz    = 0x02,  // Load LOD Zero
        Lb    = 0x08,  // Load Bias
        Ll    = 0x10,  // Load LOD
        Lba   = 0x20,  // Load Bias with OperA? Auto?
        Lla   = 0x40,  // Load LOD with OperA? Auto?
        Dc    = 0x80,  // Depth Compare
        AOffI = 0x100, // Offset
        Mz    = 0x200, // Multisample Zero?
        Ptp   = 0x400  // ???
    }
}
