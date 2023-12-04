namespace ARMeilleure.Decoders
{
    class OpCode32SimdReg : OpCode32Simd
    {
        public int Vn { get; }

        public int Qn => GetQuadwordIndex(Vn);
        public int In => GetQuadwordSubindex(Vn) << (3 - Size);
        public int Fn => GetQuadwordSubindex(Vn) << (1 - (Size & 1));

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdReg(inst, address, opCode, false);
        public new static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdReg(inst, address, opCode, true);

        public OpCode32SimdReg(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
        {
            Vn = ((opCode >> 3) & 0x10) | ((opCode >> 16) & 0xf);

            // Subclasses have their own handling of Vx to account for before checking.
            if (GetType() == typeof(OpCode32SimdReg) && DecoderHelper.VectorArgumentsInvalid(Q, Vd, Vm, Vn))
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
