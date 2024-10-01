namespace ARMeilleure.Decoders
{
    class OpCode32SimdTbl : OpCode32SimdReg
    {
        public int Length { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdTbl(inst, address, opCode, false);
        public new static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdTbl(inst, address, opCode, true);

        public OpCode32SimdTbl(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
        {
            Length = (opCode >> 8) & 3;
            Size = 0;
            Opc = Q ? 1 : 0;
            Q = false;
            RegisterSize = RegisterSize.Simd64;

            if (Vn + Length + 1 > 32)
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
