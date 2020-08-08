using System;

namespace ARMeilleure.State
{
    [Flags]
    public enum FPCR : uint
    {
        Ufe = 1u << 11,
        Fz  = 1u << 24,
        Dn  = 1u << 25,
        Ahp = 1u << 26,

        A32Mask = 0x07FF9F00u
    }

    public static class FPCRExtensions
    {
        private const int RModeShift = 22;

        public static FPRoundingMode GetRoundingMode(this FPCR fpcr)
        {
            return (FPRoundingMode)(((int)fpcr >> RModeShift) & 3);
        }
    }
}
