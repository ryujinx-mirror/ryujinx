using ARMeilleure.CodeGen.X86;

namespace ARMeilleure
{
    public static class Optimizations
    {
        public static bool AssumeStrictAbiCompliance { get; set; } = true;

        public static bool FastFP { get; set; } = true;

        public static bool UseSseIfAvailable    { get; set; } = true;
        public static bool UseSse2IfAvailable   { get; set; } = true;
        public static bool UseSse3IfAvailable   { get; set; } = true;
        public static bool UseSsse3IfAvailable  { get; set; } = true;
        public static bool UseSse41IfAvailable  { get; set; } = true;
        public static bool UseSse42IfAvailable  { get; set; } = true;
        public static bool UsePopCntIfAvailable { get; set; } = true;

        public static bool ForceLegacySse
        {
            get => HardwareCapabilities.ForceLegacySse;
            set => HardwareCapabilities.ForceLegacySse = value;
        }

        internal static bool UseSse    => UseSseIfAvailable    && HardwareCapabilities.SupportsSse;
        internal static bool UseSse2   => UseSse2IfAvailable   && HardwareCapabilities.SupportsSse2;
        internal static bool UseSse3   => UseSse3IfAvailable   && HardwareCapabilities.SupportsSse3;
        internal static bool UseSsse3  => UseSsse3IfAvailable  && HardwareCapabilities.SupportsSsse3;
        internal static bool UseSse41  => UseSse41IfAvailable  && HardwareCapabilities.SupportsSse41;
        internal static bool UseSse42  => UseSse42IfAvailable  && HardwareCapabilities.SupportsSse42;
        internal static bool UsePopCnt => UsePopCntIfAvailable && HardwareCapabilities.SupportsPopcnt;
    }
}