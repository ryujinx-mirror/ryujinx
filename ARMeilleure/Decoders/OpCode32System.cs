namespace ARMeilleure.Decoders
{
    class OpCode32System : OpCode32
    {
        public int Opc1 { get; }
        public int CRn { get; }
        public int Rt { get; }
        public int Opc2 { get; }
        public int CRm { get; }
        public int MrrcOp { get; }

        public int Coproc { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32System(inst, address, opCode);

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
