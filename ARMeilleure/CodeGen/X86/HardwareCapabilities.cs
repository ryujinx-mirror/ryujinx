using System.Runtime.Intrinsics.X86;

namespace ARMeilleure.CodeGen.X86
{
    static class HardwareCapabilities
    {
        public static bool SupportsSse => Sse.IsSupported;
        public static bool SupportsSse2 => Sse2.IsSupported;
        public static bool SupportsSse3 => Sse3.IsSupported;
        public static bool SupportsSsse3 => Ssse3.IsSupported;
        public static bool SupportsSse41 => Sse41.IsSupported;
        public static bool SupportsSse42 => Sse42.IsSupported;
        public static bool SupportsPclmulqdq => Pclmulqdq.IsSupported;
        public static bool SupportsFma => Fma.IsSupported;
        public static bool SupportsPopcnt => Popcnt.IsSupported;
        public static bool SupportsAesni => Aes.IsSupported;
        public static bool SupportsAvx => Avx.IsSupported;

        public static bool ForceLegacySse { get; set; }

        public static bool SupportsVexEncoding => SupportsAvx && !ForceLegacySse;
    }
}