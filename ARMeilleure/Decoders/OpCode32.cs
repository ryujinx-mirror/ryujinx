namespace ARMeilleure.Decoders
{
    class OpCode32 : OpCode
    {
        public Condition Cond { get; protected set; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32(inst, address, opCode);

        public OpCode32(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            RegisterSize = RegisterSize.Int32;

            Cond = (Condition)((uint)opCode >> 28);
        }

        public uint GetPc()
        {
            // Due to backwards compatibility and legacy behavior of ARMv4 CPUs pipeline,
            // the PC actually points 2 instructions ahead.
            return (uint)Address + (uint)OpCodeSizeInBytes * 2;
        }
    }
}