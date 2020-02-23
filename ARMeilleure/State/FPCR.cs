using System;

namespace ARMeilleure.State
{
    [Flags]
    public enum FPCR : uint
    {
        Ufe = 1 << 11,
        Fz  = 1 << 24,
        Dn  = 1 << 25,
        Ahp = 1 << 26,

        A32Mask = 0x07ffff00
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
