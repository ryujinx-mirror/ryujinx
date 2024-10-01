namespace ARMeilleure.Decoders
{
    class OpCode32SimdExt : OpCode32SimdReg
    {
        public int Immediate { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdExt(inst, address, opCode, false);
        public new static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdExt(inst, address, opCode, true);

        public OpCode32SimdExt(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
        {
            Immediate = (opCode >> 8) & 0xf;
            Size = 0;
            if (DecoderHelper.VectorArgumentsInvalid(Q, Vd, Vm, Vn) || (!Q && Immediate > 7))
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
