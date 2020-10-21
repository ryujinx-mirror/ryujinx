namespace ARMeilleure.Decoders
{
    class OpCode32SimdTbl : OpCode32SimdReg
    {
        public int Length { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdTbl(inst, address, opCode);

        public OpCode32SimdTbl(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
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
