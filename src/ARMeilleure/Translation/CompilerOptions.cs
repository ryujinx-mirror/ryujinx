using System;

namespace ARMeilleure.Translation
{
    [Flags]
    enum CompilerOptions
    {
        None = 0,
        SsaForm = 1 << 0,
        Optimize = 1 << 1,
        Lsra = 1 << 2,
        Relocatable = 1 << 3,

        MediumCq = SsaForm | Optimize,
        HighCq = SsaForm | Optimize | Lsra,
    }
}
