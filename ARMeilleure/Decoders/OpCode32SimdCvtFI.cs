namespace ARMeilleure.Decoders
{
    class OpCode32SimdCvtFI : OpCode32SimdS
    {
        public int Opc2 { get; private set; }

        public OpCode32SimdCvtFI(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Opc2 = (opCode >> 16) & 0x7;
            Opc = (opCode >> 7) & 0x1;
        }
    }
}
