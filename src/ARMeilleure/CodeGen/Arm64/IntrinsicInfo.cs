namespace ARMeilleure.CodeGen.Arm64
{
    readonly struct IntrinsicInfo
    {
        public uint Inst { get; }
        public IntrinsicType Type { get; }

        public IntrinsicInfo(uint inst, IntrinsicType type)
        {
            Inst = inst;
            Type = type;
        }
    }
}
