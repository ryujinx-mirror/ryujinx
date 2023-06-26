namespace ARMeilleure.Decoders
{
    class OpCodeException : OpCode
    {
        public int Id { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeException(inst, address, opCode);

        public OpCodeException(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Id = (opCode >> 5) & 0xffff;
        }
    }
}
