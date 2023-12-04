namespace ARMeilleure.Decoders
{
    class OpCode32SimdShImm : OpCode32Simd
    {
        public int Shift { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdShImm(inst, address, opCode, false);
        public new static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdShImm(inst, address, opCode, true);

        public OpCode32SimdShImm(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
        {
            int imm6 = (opCode >> 16) & 0x3f;
            int limm6 = ((opCode >> 1) & 0x40) | imm6;

            if ((limm6 & 0x40) == 0b1000000)
            {
                Size = 3;
                Shift = imm6;
            }
            else if ((limm6 & 0x60) == 0b0100000)
            {
                Size = 2;
                Shift = imm6 - 32;
            }
            else if ((limm6 & 0x70) == 0b0010000)
            {
                Size = 1;
                Shift = imm6 - 16;
            }
            else if ((limm6 & 0x78) == 0b0001000)
            {
                Size = 0;
                Shift = imm6 - 8;
            }
            else
            {
                Instruction = InstDescriptor.Undefined;
            }

            if (GetType() == typeof(OpCode32SimdShImm) && DecoderHelper.VectorArgumentsInvalid(Q, Vd, Vm))
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
