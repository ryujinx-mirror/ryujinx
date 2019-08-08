namespace ARMeilleure.IntermediateRepresentation
{
    class Operation : Node
    {
        public Instruction Instruction { get; private set; }

        public Operation(
            Instruction instruction,
            Operand destination,
            params Operand[] sources) : base(destination, sources.Length)
        {
            Instruction = instruction;

            for (int index = 0; index < sources.Length; index++)
            {
                SetSource(index, sources[index]);
            }
        }

        public Operation(
            Instruction instruction,
            Operand[] destinations,
            Operand[] sources) : base(destinations, sources.Length)
        {
            Instruction = instruction;

            for (int index = 0; index < sources.Length; index++)
            {
                SetSource(index, sources[index]);
            }
        }

        public void TurnIntoCopy(Operand source)
        {
            Instruction = Instruction.Copy;

            SetSources(new Operand[] { source });
        }
    }
}