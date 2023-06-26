namespace ARMeilleure.Decoders
{
    class OpCodeSystem : OpCode
    {
        public int Rt { get; }
        public int Op2 { get; }
        public int CRm { get; }
        public int CRn { get; }
        public int Op1 { get; }
        public int Op0 { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSystem(inst, address, opCode);

        public OpCodeSystem(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rt = (opCode >> 0) & 0x1f;
            Op2 = (opCode >> 5) & 0x7;
            CRm = (opCode >> 8) & 0xf;
            CRn = (opCode >> 12) & 0xf;
            Op1 = (opCode >> 16) & 0x7;
            Op0 = ((opCode >> 19) & 0x1) | 2;
        }
    }
}
