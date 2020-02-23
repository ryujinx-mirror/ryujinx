namespace ARMeilleure.Decoders
{
    class OpCode32Exception : OpCode32
    {
        public int Id { get; private set; }

        public OpCode32Exception(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Id = opCode & 0xFFFFFF;
        }
    }
}
