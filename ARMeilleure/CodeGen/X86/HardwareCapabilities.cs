using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

namespace ARMeilleure.CodeGen.X86
{
    static class HardwareCapabilities
    {
        private delegate ulong GetFeatureInfo();

        private static ulong _featureInfo;

        public static bool SupportsSse3      => (_featureInfo & (1UL << 0))  != 0;
        public static bool SupportsPclmulqdq => (_featureInfo & (1UL << 1))  != 0;
        public static bool SupportsSsse3     => (_featureInfo & (1UL << 9))  != 0;
        public static bool SupportsFma       => (_featureInfo & (1UL << 12)) != 0;
        public static bool SupportsCx16      => (_featureInfo & (1UL << 13)) != 0;
        public static bool SupportsSse41     => (_featureInfo & (1UL << 19)) != 0;
        public static bool SupportsSse42     => (_featureInfo & (1UL << 20)) != 0;
        public static bool SupportsPopcnt    => (_featureInfo & (1UL << 23)) != 0;
        public static bool SupportsAesni     => (_featureInfo & (1UL << 25)) != 0;
        public static bool SupportsAvx       => (_featureInfo & (1UL << 28)) != 0;
        public static bool SupportsF16c      => (_featureInfo & (1UL << 29)) != 0;

        public static bool SupportsSse  => (_featureInfo & (1UL << 32 + 25)) != 0;
        public static bool SupportsSse2 => (_featureInfo & (1UL << 32 + 26)) != 0;

        public static bool ForceLegacySse { get; set; }

        public static bool SupportsVexEncoding => !ForceLegacySse && SupportsAvx;

        static HardwareCapabilities()
        {
            EmitterContext context = new EmitterContext();

            Operand featureInfo = context.CpuId();

            context.Return(featureInfo);

            ControlFlowGraph cfg = context.GetControlFlowGraph();

            OperandType[] argTypes = new OperandType[0];

            GetFeatureInfo getFeatureInfo = Compiler.Compile<GetFeatureInfo>(
                cfg,
                argTypes,
                OperandType.I64,
                CompilerOptions.HighCq);

            _featureInfo = getFeatureInfo();
        }
    }
}