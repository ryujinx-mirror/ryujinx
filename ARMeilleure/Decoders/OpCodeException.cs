namespace ARMeilleure.Decoders
{
    class OpCodeException : OpCode
    {
        public int Id { get; private set; }

        public OpCodeException(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Id = (opCode >> 5) & 0xffff;
        }
    }
}