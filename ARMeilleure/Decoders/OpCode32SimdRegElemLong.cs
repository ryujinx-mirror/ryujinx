namespace ARMeilleure.Decoders
{
    class OpCode32SimdRegElemLong : OpCode32SimdRegElem
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdRegElemLong(inst, address, opCode);

        public OpCode32SimdRegElemLong(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Q = false;
            F = false;

            RegisterSize = RegisterSize.Simd64;

            // (Vd & 1) != 0 || Size == 3 are also invalid, but they are checked on encoding.
            if (Size == 0)
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
