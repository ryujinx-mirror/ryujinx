namespace ARMeilleure.Decoders
{
    class OpCodeAdr : OpCode
    {
        public int Rd { get; private set; }

        public long Immediate { get; private set; }

         public OpCodeAdr(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = opCode & 0x1f;

            Immediate  = DecoderHelper.DecodeImmS19_2(opCode);
            Immediate |= ((long)opCode >> 29) & 3;
        }
    }
}