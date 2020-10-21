namespace ARMeilleure.Decoders
{
    class OpCode32SimdCvtFI : OpCode32SimdS
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdCvtFI(inst, address, opCode);

        public OpCode32SimdCvtFI(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Opc = (opCode >> 7) & 0x1;

            bool toInteger = (Opc2 & 0b100) != 0;

            if (toInteger)
            {
                Vd = ((opCode >> 22) & 0x1) | ((opCode >> 11) & 0x1e);
            }
            else
            {
                Vm = ((opCode >> 5) & 0x1) | ((opCode << 1) & 0x1e);
            }
        }
    }
}
