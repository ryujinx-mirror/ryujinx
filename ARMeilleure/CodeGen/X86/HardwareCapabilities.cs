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

            (int maxNum, _, _, _) = X86Base.CpuId(0x00000000, 0x00000000);

            (_, _, int ecx1, int edx1) = X86Base.CpuId(0x00000001, 0x00000000);
            FeatureInfo1Edx = (FeatureFlags1Edx)edx1;
            FeatureInfo1Ecx = (FeatureFlags1Ecx)ecx1;

            if (maxNum >= 7)
            {
                (_, int ebx7, _, _) = X86Base.CpuId(0x00000007, 0x00000000);
                FeatureInfo7Ebx = (FeatureFlags7Ebx)ebx7;
            }
        }

        [Flags]
        public enum FeatureFlags1Edx
        {
            Sse = 1 << 25,
            Sse2 = 1 << 26
        }

        [Flags]
        public enum FeatureFlags1Ecx
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

        [Flags]
        public enum FeatureFlags7Ebx
        {
            Avx2 = 1 << 5,
            Sha = 1 << 29
        }

        public static FeatureFlags1Edx FeatureInfo1Edx { get; }
        public static FeatureFlags1Ecx FeatureInfo1Ecx { get; }
        public static FeatureFlags7Ebx FeatureInfo7Ebx { get; } = 0;

        public static bool SupportsSse => FeatureInfo1Edx.HasFlag(FeatureFlags1Edx.Sse);
        public static bool SupportsSse2 => FeatureInfo1Edx.HasFlag(FeatureFlags1Edx.Sse2);
        public static bool SupportsSse3 => FeatureInfo1Ecx.HasFlag(FeatureFlags1Ecx.Sse3);
        public static bool SupportsPclmulqdq => FeatureInfo1Ecx.HasFlag(FeatureFlags1Ecx.Pclmulqdq);
        public static bool SupportsSsse3 => FeatureInfo1Ecx.HasFlag(FeatureFlags1Ecx.Ssse3);
        public static bool SupportsFma => FeatureInfo1Ecx.HasFlag(FeatureFlags1Ecx.Fma);
        public static bool SupportsSse41 => FeatureInfo1Ecx.HasFlag(FeatureFlags1Ecx.Sse41);
        public static bool SupportsSse42 => FeatureInfo1Ecx.HasFlag(FeatureFlags1Ecx.Sse42);
        public static bool SupportsPopcnt => FeatureInfo1Ecx.HasFlag(FeatureFlags1Ecx.Popcnt);
        public static bool SupportsAesni => FeatureInfo1Ecx.HasFlag(FeatureFlags1Ecx.Aes);
        public static bool SupportsAvx => FeatureInfo1Ecx.HasFlag(FeatureFlags1Ecx.Avx);
        public static bool SupportsAvx2 => FeatureInfo7Ebx.HasFlag(FeatureFlags7Ebx.Avx2) && SupportsAvx;
        public static bool SupportsF16c => FeatureInfo1Ecx.HasFlag(FeatureFlags1Ecx.F16c);
        public static bool SupportsSha => FeatureInfo7Ebx.HasFlag(FeatureFlags7Ebx.Sha);

        public static bool ForceLegacySse { get; set; }

        public static bool SupportsVexEncoding => SupportsAvx && !ForceLegacySse;
    }
}