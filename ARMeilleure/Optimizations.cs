using System.Runtime.Intrinsics.Arm;

namespace ARMeilleure
{
    using Arm64HardwareCapabilities = ARMeilleure.CodeGen.Arm64.HardwareCapabilities;
    using X86HardwareCapabilities = ARMeilleure.CodeGen.X86.HardwareCapabilities;

    public static class Optimizations
    {
        public static bool FastFP { get; set; } = true;

        public static bool AllowLcqInFunctionTable  { get; set; } = true;
        public static bool UseUnmanagedDispatchLoop { get; set; } = true;

        public static bool UseAdvSimdIfAvailable    { get; set; } = true;
        public static bool UseArm64PmullIfAvailable { get; set; } = true;

        public static bool UseSseIfAvailable       { get; set; } = true;
        public static bool UseSse2IfAvailable      { get; set; } = true;
        public static bool UseSse3IfAvailable      { get; set; } = true;
        public static bool UseSsse3IfAvailable     { get; set; } = true;
        public static bool UseSse41IfAvailable     { get; set; } = true;
        public static bool UseSse42IfAvailable     { get; set; } = true;
        public static bool UsePopCntIfAvailable    { get; set; } = true;
        public static bool UseAvxIfAvailable       { get; set; } = true;
        public static bool UseF16cIfAvailable      { get; set; } = true;
        public static bool UseFmaIfAvailable       { get; set; } = true;
        public static bool UseAesniIfAvailable     { get; set; } = true;
        public static bool UsePclmulqdqIfAvailable { get; set; } = true;
        public static bool UseShaIfAvailable       { get; set; } = true;
        public static bool UseGfniIfAvailable      { get; set; } = true;

        public static bool ForceLegacySse
        {
            get => X86HardwareCapabilities.ForceLegacySse;
            set => X86HardwareCapabilities.ForceLegacySse = value;
        }

        internal static bool UseAdvSimd    => UseAdvSimdIfAvailable    && Arm64HardwareCapabilities.SupportsAdvSimd;
        internal static bool UseArm64Pmull => UseArm64PmullIfAvailable && Arm64HardwareCapabilities.SupportsPmull;

        internal static bool UseSse       => UseSseIfAvailable       && X86HardwareCapabilities.SupportsSse;
        internal static bool UseSse2      => UseSse2IfAvailable      && X86HardwareCapabilities.SupportsSse2;
        internal static bool UseSse3      => UseSse3IfAvailable      && X86HardwareCapabilities.SupportsSse3;
        internal static bool UseSsse3     => UseSsse3IfAvailable     && X86HardwareCapabilities.SupportsSsse3;
        internal static bool UseSse41     => UseSse41IfAvailable     && X86HardwareCapabilities.SupportsSse41;
        internal static bool UseSse42     => UseSse42IfAvailable     && X86HardwareCapabilities.SupportsSse42;
        internal static bool UsePopCnt    => UsePopCntIfAvailable    && X86HardwareCapabilities.SupportsPopcnt;
        internal static bool UseAvx       => UseAvxIfAvailable       && X86HardwareCapabilities.SupportsAvx && !ForceLegacySse;
        internal static bool UseF16c      => UseF16cIfAvailable      && X86HardwareCapabilities.SupportsF16c;
        internal static bool UseFma       => UseFmaIfAvailable       && X86HardwareCapabilities.SupportsFma;
        internal static bool UseAesni     => UseAesniIfAvailable     && X86HardwareCapabilities.SupportsAesni;
        internal static bool UsePclmulqdq => UsePclmulqdqIfAvailable && X86HardwareCapabilities.SupportsPclmulqdq;
        internal static bool UseSha       => UseShaIfAvailable       && X86HardwareCapabilities.SupportsSha;
        internal static bool UseGfni      => UseGfniIfAvailable      && X86HardwareCapabilities.SupportsGfni;
    }
}
