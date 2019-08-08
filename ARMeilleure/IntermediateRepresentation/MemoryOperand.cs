namespace ARMeilleure.IntermediateRepresentation
{
    class MemoryOperand : Operand
    {
        public Operand BaseAddress { get; set; }
        public Operand Index       { get; set; }

        public Multiplier Scale { get; }

        public int Displacement { get; }

        public MemoryOperand(
            OperandType type,
            Operand     baseAddress,
            Operand     index        = null,
            Multiplier  scale        = Multiplier.x1,
            int         displacement = 0) : base(OperandKind.Memory, type)
        {
            BaseAddress  = baseAddress;
            Index        = index;
            Scale        = scale;
            Displacement = displacement;
        }
    }
}