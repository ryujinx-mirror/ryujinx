namespace ARMeilleure.IntermediateRepresentation
{
    class MemoryOperand : Operand
    {
        public Operand BaseAddress { get; set; }
        public Operand Index       { get; set; }

        public Multiplier Scale { get; private set; }

        public int Displacement { get; private set; }

        public MemoryOperand() { }

        public MemoryOperand With(
            OperandType type,
            Operand     baseAddress,
            Operand     index        = null,
            Multiplier  scale        = Multiplier.x1,
            int         displacement = 0)
        {
            With(OperandKind.Memory, type);
            BaseAddress  = baseAddress;
            Index        = index;
            Scale        = scale;
            Displacement = displacement;
            return this;
        }
    }
}