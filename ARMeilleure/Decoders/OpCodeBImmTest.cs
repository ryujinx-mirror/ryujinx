namespace ARMeilleure.Decoders
{
    class OpCodeBImmTest : OpCodeBImm
    {
        public int Rt  { get; private set; }
        public int Bit { get; private set; }

        public OpCodeBImmTest(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rt = opCode & 0x1f;

            Immediate = (long)address + DecoderHelper.DecodeImmS14_2(opCode);

            Bit  = (opCode >> 19) & 0x1f;
            Bit |= (opCode >> 26) & 0x20;
        }
    }
}