namespace ARMeilleure.CodeGen.X86
{
    struct IntrinsicInfo
    {
        public X86Instruction Inst { get; }
        public IntrinsicType  Type { get; }

        public IntrinsicInfo(X86Instruction inst, IntrinsicType type)
        {
            Inst = inst;
            Type = type;
        }
    }
}