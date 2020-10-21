namespace ARMeilleure.Decoders
{
    class OpCode32SimdShImmLong : OpCode32Simd
    {
        public int Shift { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdShImmLong(inst, address, opCode);

        public OpCode32SimdShImmLong(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Q = false;
            RegisterSize = RegisterSize.Simd64;

            int imm6 = (opCode >> 16) & 0x3f;

            if ((imm6 & 0x20) == 0b100000)
            {
                Size = 2;
                Shift = imm6 - 32;
            }
            else if ((imm6 & 0x30) == 0b010000)
            {
                Size = 1;
                Shift = imm6 - 16;
            }
            else if ((imm6 & 0x38) == 0b001000)
            {
                Size = 0;
                Shift = imm6 - 8;
            }
            else
            {
                Instruction = InstDescriptor.Undefined;
            }

            if (GetType() == typeof(OpCode32SimdShImmLong) && DecoderHelper.VectorArgumentsInvalid(true, Vd))
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
