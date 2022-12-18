using ARMeilleure.CodeGen.X86;

namespace ARMeilleure
{
    public static class Optimizations
    {
        public static bool FastFP { get; set; } = true;

        public static bool AllowLcqInFunctionTable  { get; set; } = true;
        public static bool UseUnmanagedDispatchLoop { get; set; } = true;

        public static bool UseSseIfAvailable       { get; set; } = true;
        public static bool UseSse2IfAvailable      { get; set; } = true;
        public static bool UseSse3IfAvailable      { get; set; } = true;
        public static bool UseSsse3IfAvailable     { get; set; } = true;
        public static bool UseSse41IfAvailable     { get; set; } = true;
        public static bool UseSse42IfAvailable     { get; set; } = true;
        public static bool UsePopCntIfAvailable    { get; set; } = true;
        public static bool UseAvxIfAvailable       { get; set; } = true;
        public static bool UseAvx512FIfAvailable   { get; set; } = true;
        public static bool UseAvx512VlIfAvailable  { get; set; } = true;
        public static bool UseAvx512BwIfAvailable  { get; set; } = true;
        public static bool UseAvx512DqIfAvailable  { get; set; } = true;
        public static bool UseF16cIfAvailable      { get; set; } = true;
        public static bool UseFmaIfAvailable       { get; set; } = true;
        public static bool UseAesniIfAvailable     { get; set; } = true;
        public static bool UsePclmulqdqIfAvailable { get; set; } = true;
        public static bool UseShaIfAvailable       { get; set; } = true;
        public static bool UseGfniIfAvailable      { get; set; } = true;

        public static bool ForceLegacySse
        {
            get => HardwareCapabilities.ForceLegacySse;
            set => HardwareCapabilities.ForceLegacySse = value;
        }

        internal static bool UseSse       => UseSseIfAvailable       && HardwareCapabilities.SupportsSse;
        internal static bool UseSse2      => UseSse2IfAvailable      && HardwareCapabilities.SupportsSse2;
        internal static bool UseSse3      => UseSse3IfAvailable      && HardwareCapabilities.SupportsSse3;
        internal static bool UseSsse3     => UseSsse3IfAvailable     && HardwareCapabilities.SupportsSsse3;
        internal static bool UseSse41     => UseSse41IfAvailable     && HardwareCapabilities.SupportsSse41;
        internal static bool UseSse42     => UseSse42IfAvailable     && HardwareCapabilities.SupportsSse42;
        internal static bool UsePopCnt    => UsePopCntIfAvailable    && HardwareCapabilities.SupportsPopcnt;
        internal static bool UseAvx       => UseAvxIfAvailable       && HardwareCapabilities.SupportsAvx && !ForceLegacySse;
        internal static bool UseAvx512F   => UseAvx512FIfAvailable   && HardwareCapabilities.SupportsAvx512F && !ForceLegacySse;
        internal static bool UseAvx512Vl  => UseAvx512VlIfAvailable  && HardwareCapabilities.SupportsAvx512Vl && !ForceLegacySse;
        internal static bool UseAvx512Bw  => UseAvx512BwIfAvailable  && HardwareCapabilities.SupportsAvx512Bw && !ForceLegacySse;
        internal static bool UseAvx512Dq  => UseAvx512DqIfAvailable  && HardwareCapabilities.SupportsAvx512Dq && !ForceLegacySse;
        internal static bool UseF16c      => UseF16cIfAvailable      && HardwareCapabilities.SupportsF16c;
        internal static bool UseFma       => UseFmaIfAvailable       && HardwareCapabilities.SupportsFma;
        internal static bool UseAesni     => UseAesniIfAvailable     && HardwareCapabilities.SupportsAesni;
        internal static bool UsePclmulqdq => UsePclmulqdqIfAvailable && HardwareCapabilities.SupportsPclmulqdq;
        internal static bool UseSha       => UseShaIfAvailable       && HardwareCapabilities.SupportsSha;
        internal static bool UseGfni      => UseGfniIfAvailable      && HardwareCapabilities.SupportsGfni;

        internal static bool UseAvx512Ortho      => UseAvx512F && UseAvx512Vl;
        internal static bool UseAvx512OrthoFloat => UseAvx512Ortho && UseAvx512Dq;
    }
}