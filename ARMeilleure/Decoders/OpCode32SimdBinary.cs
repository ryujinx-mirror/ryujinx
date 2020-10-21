namespace ARMeilleure.Decoders
{
    /// <summary>
    /// A special alias that always runs in 64 bit int, to speed up binary ops a little.
    /// </summary>
    class OpCode32SimdBinary : OpCode32SimdReg
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdBinary(inst, address, opCode);

        public OpCode32SimdBinary(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Size = 3;

            if (DecoderHelper.VectorArgumentsInvalid(Q, Vd, Vm, Vn))
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
