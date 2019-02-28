using System;

namespace Ryujinx.Graphics.Texture
{
    [Flags]
    public enum TextureInstructionSuffix
    {
        None  = 0x00,  // No Modifier
        LZ    = 0x02,  // Load LOD Zero
        LB    = 0x08,  // Load Bias
        LL    = 0x10,  // Load LOD
        LBA   = 0x20,  // Load Bias with OperA? Auto?
        LLA   = 0x40,  // Load LOD with OperA? Auto?
        DC    = 0x80,  // Depth Compare
        AOffI = 0x100, // Offset
        MZ    = 0x200, // Multisample Zero?
        PTP   = 0x400  // ???
    }
}
