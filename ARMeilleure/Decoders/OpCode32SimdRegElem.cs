namespace ARMeilleure.Decoders
{
    class OpCode32SimdRegElem : OpCode32SimdReg
    {
        public OpCode32SimdRegElem(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Q = ((opCode >> 24) & 0x1) != 0;
            F = ((opCode >> 8) & 0x1) != 0;
            Size = ((opCode >> 20) & 0x3);

            RegisterSize = Q ? RegisterSize.Simd128 : RegisterSize.Simd64;

            Vm = ((opCode >> 5) & 0x1) | ((opCode << 1) & 0x1e);

            if (DecoderHelper.VectorArgumentsInvalid(Q, Vd, Vn) || Size == 0 || (Size == 1 && F))
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
