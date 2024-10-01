namespace ARMeilleure.Decoders
{
    class OpCode32MemLdEx : OpCode32Mem, IOpCode32MemEx
    {
        public int Rd { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32MemLdEx(inst, address, opCode);

        public OpCode32MemLdEx(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = opCode & 0xf;
        }
    }
}
