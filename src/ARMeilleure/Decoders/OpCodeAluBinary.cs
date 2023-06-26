namespace ARMeilleure.Decoders
{
    class OpCodeAluBinary : OpCodeAlu
    {
        public int Rm { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeAluBinary(inst, address, opCode);

        public OpCodeAluBinary(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 16) & 0x1f;
        }
    }
}
