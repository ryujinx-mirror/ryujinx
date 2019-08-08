namespace ARMeilleure.Decoders
{
    class OpCodeT16BReg : OpCodeT16, IOpCode32BReg
    {
        public int Rm { get; private set; }

        public OpCodeT16BReg(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 3) & 0xf;
        }
    }
}