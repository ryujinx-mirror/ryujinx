namespace ARMeilleure.Decoders
{
    class OpCodeT32 : OpCode32
    {
        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32(inst, address, opCode);

        public OpCodeT32(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Cond = Condition.Al;

            IsThumb = true;
            OpCodeSizeInBytes = 4;
        }
    }
}
