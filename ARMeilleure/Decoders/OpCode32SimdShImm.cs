namespace ARMeilleure.Decoders
{
    class OpCode32SimdShImm : OpCode32Simd
    {
        public int Immediate { get; private set; }
        public int Shift { get; private set; }

        public OpCode32SimdShImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Immediate = (opCode >> 16) & 0x3f;
            var limm = ((opCode >> 1) & 0x40) | Immediate;

            if ((limm & 0x40) == 0b1000000)
            {
                Size = 3;
                Shift = Immediate;
            } 
            else if ((limm & 0x60) == 0b0100000)
            {
                Size = 2;
                Shift = Immediate - 32;
            }
            else if ((limm & 0x70) == 0b0010000)
            {
                Size = 1;
                Shift = Immediate - 16;
            }
            else if ((limm & 0x78) == 0b0001000)
            {
                Size = 0;
                Shift = Immediate - 8;
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
