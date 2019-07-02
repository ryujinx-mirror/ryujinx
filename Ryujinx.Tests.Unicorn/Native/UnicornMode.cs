// ReSharper disable InconsistentNaming
namespace Ryujinx.Tests.Unicorn.Native
{
    public enum UnicornMode
    {
        UC_MODE_LITTLE_ENDIAN = 0,    // little-endian mode (default mode)
        UC_MODE_BIG_ENDIAN = 1 << 30, // big-endian mode
        // arm / arm64
        UC_MODE_ARM = 0,              // ARM mode
        UC_MODE_THUMB = 1 << 4,       // THUMB mode (including Thumb-2)
        UC_MODE_MCLASS = 1 << 5,      // ARM's Cortex-M series (currently unsupported)
        UC_MODE_V8 = 1 << 6,          // ARMv8 A32 encodings for ARM (currently unsupported)
        // mips
        UC_MODE_MICRO = 1 << 4,       // MicroMips mode (currently unsupported)
        UC_MODE_MIPS3 = 1 << 5,       // Mips III ISA (currently unsupported)
        UC_MODE_MIPS32R6 = 1 << 6,    // Mips32r6 ISA (currently unsupported)
        UC_MODE_MIPS32 = 1 << 2,      // Mips32 ISA
        UC_MODE_MIPS64 = 1 << 3,      // Mips64 ISA
        // x86 / x64
        UC_MODE_16 = 1 << 1,          // 16-bit mode
        UC_MODE_32 = 1 << 2,          // 32-bit mode
        UC_MODE_64 = 1 << 3,          // 64-bit mode
        // ppc
        UC_MODE_PPC32 = 1 << 2,       // 32-bit mode (currently unsupported)
        UC_MODE_PPC64 = 1 << 3,       // 64-bit mode (currently unsupported)
        UC_MODE_QPX = 1 << 4,         // Quad Processing eXtensions mode (currently unsupported)
        // sparc
        UC_MODE_SPARC32 = 1 << 2,     // 32-bit mode
        UC_MODE_SPARC64 = 1 << 3,     // 64-bit mode
        UC_MODE_V9 = 1 << 4,          // SparcV9 mode (currently unsupported)
        // m68k
    }
}
