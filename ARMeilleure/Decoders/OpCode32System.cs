namespace ARMeilleure.Decoders
{
    class OpCode32System : OpCode32
    {
        public int Opc1 { get; private set; }
        public int CRn { get; private set; }
        public int Rt { get; private set; }
        public int Opc2 { get; private set; }
        public int CRm { get; private set; }
        public int MrrcOp { get; private set; }

        public int Coproc { get; private set; }

        public OpCode32System(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Opc1 = (opCode >> 21) & 0x7;
            CRn = (opCode >> 16) & 0xf;
            Rt = (opCode >> 12) & 0xf;
            Opc2 = (opCode >> 5) & 0x7;
            CRm = (opCode >> 0) & 0xf;
            MrrcOp = (opCode >> 4) & 0xf;

            Coproc = (opCode >> 8) & 0xf;
        }
    }
}
