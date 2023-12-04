using System;

namespace ARMeilleure.CodeGen.X86
{
    [Flags]
    enum Mxcsr
    {
        Ftz = 1 << 15, // Flush To Zero.
        Rhi = 1 << 14, // Round Mode high bit.
        Rlo = 1 << 13, // Round Mode low bit.
        Um = 1 << 11,  // Underflow Mask.
        Dm = 1 << 8,   // Denormal Mask.
        Daz = 1 << 6, // Denormals Are Zero.
    }
}
