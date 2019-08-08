namespace ARMeilleure.Decoders
{
    class OpCodeBImmCmp : OpCodeBImm
    {
        public int Rt { get; private set; }

        public OpCodeBImmCmp(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rt = opCode & 0x1f;

            Immediate = (long)address + DecoderHelper.DecodeImmS19_2(opCode);

            RegisterSize = (opCode >> 31) != 0
                ? RegisterSize.Int64
                : RegisterSize.Int32;
        }
    }
}