namespace ARMeilleure.Decoders
{
    class OpCodeAluBinary : OpCodeAlu
    {
        public int Rm { get; private set; }

        public OpCodeAluBinary(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rm = (opCode >> 16) & 0x1f;
        }
    }
}