namespace ARMeilleure.Decoders
{
    class OpCode32SimdCmpZ : OpCode32Simd
    {
        public OpCode32SimdCmpZ(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Size = (opCode >> 18) & 0x3;

            if (DecoderHelper.VectorArgumentsInvalid(Q, Vd, Vm))
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
