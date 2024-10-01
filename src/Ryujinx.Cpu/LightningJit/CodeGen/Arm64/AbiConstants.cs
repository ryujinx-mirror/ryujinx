namespace Ryujinx.Cpu.LightningJit.CodeGen.Arm64
{
    static class AbiConstants
    {
        // Some of those register have specific roles and can't be used as general purpose registers.
        // X18 - Reserved for platform specific usage.
        // X29 - Frame pointer.
        // X30 - Return address.
        // X31 - Not an actual register, in some cases maps to SP, and in others to ZR.
        public const uint ReservedRegsMask = (1u << 18) | (1u << 29) | (1u << 30) | (1u << 31);

        public const uint GprCalleeSavedRegsMask = 0x1ff80000; // X19 to X28
        public const uint FpSimdCalleeSavedRegsMask = 0xff00; // D8 to D15
    }
}
