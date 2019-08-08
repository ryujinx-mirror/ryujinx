using ARMeilleure.CodeGen.Unwinding;

namespace ARMeilleure.CodeGen
{
    struct CompiledFunction
    {
        public byte[] Code { get; }

        public UnwindInfo UnwindInfo { get; }

        public CompiledFunction(byte[] code, UnwindInfo unwindInfo)
        {
            Code       = code;
            UnwindInfo = unwindInfo;
        }
    }
}