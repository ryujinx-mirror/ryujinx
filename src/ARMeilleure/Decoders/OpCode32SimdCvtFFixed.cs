namespace ARMeilleure.Decoders
{
    class OpCode32SimdCvtFFixed : OpCode32Simd
    {
        public int Fbits { get; protected set; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdCvtFFixed(inst, address, opCode, false);
        public new static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdCvtFFixed(inst, address, opCode, true);

        public OpCode32SimdCvtFFixed(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
        {
            Opc = (opCode >> 8) & 0x1;

            Size = Opc == 1 ? 0 : 2;
            Fbits = 64 - ((opCode >> 16) & 0x3f);

            if (DecoderHelper.VectorArgumentsInvalid(Q, Vd, Vm))
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
