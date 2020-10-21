namespace ARMeilleure.Decoders
{
    class OpCode32MemStEx : OpCode32Mem, IOpCode32MemEx
    {
        public int Rd { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32MemStEx(inst, address, opCode);

        public OpCode32MemStEx(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = (opCode >> 12) & 0xf;
            Rt = (opCode >> 0) & 0xf;
        }
    }
}
