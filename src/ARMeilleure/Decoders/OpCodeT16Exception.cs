namespace ARMeilleure.Decoders
{
    class OpCodeT16Exception : OpCodeT16, IOpCode32Exception
    {
        public int Id { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16Exception(inst, address, opCode);

        public OpCodeT16Exception(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Id = opCode & 0xFF;
        }
    }
}
