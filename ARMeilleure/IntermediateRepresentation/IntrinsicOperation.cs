namespace ARMeilleure.IntermediateRepresentation
{
    class IntrinsicOperation : Operation
    {
        public Intrinsic Intrinsic { get; }

        public IntrinsicOperation(Intrinsic intrin, Operand dest, params Operand[] sources) : base(Instruction.Extended, dest, sources)
        {
            Intrinsic = intrin;
        }
    }
}