namespace ARMeilleure.Decoders
{
    class OpCode32SimdRegElem : OpCode32SimdReg
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdRegElem(inst, address, opCode, false);
        public new static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdRegElem(inst, address, opCode, true);

        public OpCode32SimdRegElem(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
        {
            Q = ((opCode >> (isThumb ? 28 : 24)) & 0x1) != 0;
            F = ((opCode >> 8) & 0x1) != 0;
            Size = (opCode >> 20) & 0x3;

            RegisterSize = Q ? RegisterSize.Simd128 : RegisterSize.Simd64;

            if (Size == 1)
            {
                Vm = ((opCode >> 3) & 0x1) | ((opCode >> 4) & 0x2) | ((opCode << 2) & 0x1c);
            }
            else /* if (Size == 2) */
            {
                Vm = ((opCode >> 5) & 0x1) | ((opCode << 1) & 0x1e);
            }

            if (GetType() == typeof(OpCode32SimdRegElem) && DecoderHelper.VectorArgumentsInvalid(Q, Vd, Vn) || Size == 0 || (Size == 1 && F))
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
