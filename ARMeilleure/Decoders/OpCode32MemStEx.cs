namespace ARMeilleure.Decoders
{
    class OpCode32MemStEx : OpCode32Mem, IOpCode32MemEx
    {
        public int Rd { get; private set; }

        public OpCode32MemStEx(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = (opCode >> 12) & 0xf;
            Rt = (opCode >> 0) & 0xf;
        }
    }
}
