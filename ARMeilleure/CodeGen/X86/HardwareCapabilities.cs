using System;
using System.Runtime.Intrinsics.X86;

namespace ARMeilleure.CodeGen.X86
{
    static class HardwareCapabilities
    {
        static HardwareCapabilities()
        {
            if (!X86Base.IsSupported)
            {
                return;
            }

            (_, _, int ecx, int edx) = X86Base.CpuId(0x00000001, 0x00000000);

            FeatureInfoEdx = (FeatureFlagsEdx)edx;
            FeatureInfoEcx = (FeatureFlagsEcx)ecx;
        }

        [Flags]
        public enum FeatureFlagsEdx
        {
            Sse = 1 << 25,
            Sse2 = 1 << 26
        }

        [Flags]
        public enum FeatureFlagsEcx
        {
            Sse3 = 1 << 0,
            Pclmulqdq = 1 << 1,
            Ssse3 = 1 << 9,
            Fma = 1 << 12,
            Sse41 = 1 << 19,
            Sse42 = 1 << 20,
            Popcnt = 1 << 23,
            Aes = 1 << 25,
            Avx = 1 << 28,
            F16c = 1 << 29
        }

        public static FeatureFlagsEdx FeatureInfoEdx { get; }
        public static FeatureFlagsEcx FeatureInfoEcx { get; }

        public static bool SupportsSse => FeatureInfoEdx.HasFlag(FeatureFlagsEdx.Sse);
        public static bool SupportsSse2 => FeatureInfoEdx.HasFlag(FeatureFlagsEdx.Sse2);
        public static bool SupportsSse3 => FeatureInfoEcx.HasFlag(FeatureFlagsEcx.Sse3);
        public static bool SupportsPclmulqdq => FeatureInfoEcx.HasFlag(FeatureFlagsEcx.Pclmulqdq);
        public static bool SupportsSsse3 => FeatureInfoEcx.HasFlag(FeatureFlagsEcx.Ssse3);
        public static bool SupportsFma => FeatureInfoEcx.HasFlag(FeatureFlagsEcx.Fma);
        public static bool SupportsSse41 => FeatureInfoEcx.HasFlag(FeatureFlagsEcx.Sse41);
        public static bool SupportsSse42 => FeatureInfoEcx.HasFlag(FeatureFlagsEcx.Sse42);
        public static bool SupportsPopcnt => FeatureInfoEcx.HasFlag(FeatureFlagsEcx.Popcnt);
        public static bool SupportsAesni => FeatureInfoEcx.HasFlag(FeatureFlagsEcx.Aes);
        public static bool SupportsAvx => FeatureInfoEcx.HasFlag(FeatureFlagsEcx.Avx);
        public static bool SupportsF16c => FeatureInfoEcx.HasFlag(FeatureFlagsEcx.F16c);

        public static bool ForceLegacySse { get; set; }

        public static bool SupportsVexEncoding => SupportsAvx && !ForceLegacySse;
    }
}