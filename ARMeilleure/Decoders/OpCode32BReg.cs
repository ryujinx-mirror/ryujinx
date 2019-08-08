namespace ARMeilleure.Decoders
{
    class OpCode32BReg : OpCode32, IOpCode32BReg
    {
        public int Rm { get; private set; }

        public OpCode32BReg(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = opCode & 0xf;
        }
    }
}