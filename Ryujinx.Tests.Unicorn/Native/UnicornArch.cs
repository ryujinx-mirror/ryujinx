using System;

namespace Ryujinx.Tests.Unicorn.Native
{
    public enum UnicornArch
    {
        UC_ARCH_ARM = 1,    // ARM architecture (including Thumb, Thumb-2)
        UC_ARCH_ARM64,      // ARM-64, also called AArch64
        UC_ARCH_MIPS,       // Mips architecture
        UC_ARCH_X86,        // X86 architecture (including x86 & x86-64)
        UC_ARCH_PPC,        // PowerPC architecture (currently unsupported)
        UC_ARCH_SPARC,      // Sparc architecture
        UC_ARCH_M68K,       // M68K architecture
        UC_ARCH_MAX,
    }
}
