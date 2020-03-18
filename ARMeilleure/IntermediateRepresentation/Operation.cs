namespace ARMeilleure.IntermediateRepresentation
{
    class Operation : Node
    {
        public Instruction Instruction { get; private set; }

        public Operation() : base() { }

        public Operation(
            Instruction instruction,
            Operand destination,
            Operand[] sources) : base(destination, sources.Length)
        {
            Instruction = instruction;

            for (int index = 0; index < sources.Length; index++)
            {
                SetSource(index, sources[index]);
            }
        }

        public Operation With(Instruction instruction, Operand destination)
        {
            With(destination, 0);
            Instruction = instruction;
            return this;
        }

        public Operation With(Instruction instruction, Operand destination, Operand[] sources)
        {
            With(destination, sources.Length);
            Instruction = instruction;

            for (int index = 0; index < sources.Length; index++)
            {
                SetSource(index, sources[index]);
            }
            return this;
        }

        public Operation With(Instruction instruction, Operand destination, 
            Operand source0)
        {
            With(destination, 1);
            Instruction = instruction;

            SetSource(0, source0);
            return this;
        }

        public Operation With(Instruction instruction, Operand destination,
            Operand source0, Operand source1)
        {
            With(destination, 2);
            Instruction = instruction;

            SetSource(0, source0);
            SetSource(1, source1);
            return this;
        }

        public Operation With(Instruction instruction, Operand destination, 
            Operand source0, Operand source1, Operand source2)
        {
            With(destination, 3);
            Instruction = instruction;

            SetSource(0, source0);
            SetSource(1, source1);
            SetSource(2, source2);
            return this;
        }

        public Operation With(
            Instruction instruction,
            Operand[] destinations,
            Operand[] sources)
        {
            With(destinations, sources.Length);
            Instruction = instruction;

            for (int index = 0; index < sources.Length; index++)
            {
                SetSource(index, sources[index]);
            }
            return this;
        }

        public void TurnIntoCopy(Operand source)
        {
            Instruction = Instruction.Copy;

            SetSource(source);
        }
    }
}