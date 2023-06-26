namespace ARMeilleure.Decoders
{
    class OpCodeT32AluImm12 : OpCodeT32Alu, IOpCode32AluImm
    {
        public int Immediate { get; }

        public bool IsRotated => false;

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32AluImm12(inst, address, opCode);

        public OpCodeT32AluImm12(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Immediate = (opCode & 0xff) | ((opCode >> 4) & 0x700) | ((opCode >> 15) & 0x800);
        }
    }
}
