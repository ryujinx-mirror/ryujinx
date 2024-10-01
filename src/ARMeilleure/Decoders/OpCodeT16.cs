namespace ARMeilleure.Decoders
{
    class OpCodeT16 : OpCode32
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16(inst, address, opCode);

        public OpCodeT16(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Cond = Condition.Al;

            IsThumb = true;
            OpCodeSizeInBytes = 2;
        }
    }
}
