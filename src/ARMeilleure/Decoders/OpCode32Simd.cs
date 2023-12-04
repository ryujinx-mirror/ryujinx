namespace ARMeilleure.Decoders
{
    class OpCode32Simd : OpCode32SimdBase
    {
        public int Opc { get; protected set; }
        public bool Q { get; protected set; }
        public bool F { get; protected set; }
        public bool U { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32Simd(inst, address, opCode, false);
        public static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32Simd(inst, address, opCode, true);

        public OpCode32Simd(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode, isThumb)
        {
            Size = (opCode >> 20) & 0x3;
            Q = ((opCode >> 6) & 0x1) != 0;
            F = ((opCode >> 10) & 0x1) != 0;
            U = ((opCode >> (isThumb ? 28 : 24)) & 0x1) != 0;
            Opc = (opCode >> 7) & 0x3;

            RegisterSize = Q ? RegisterSize.Simd128 : RegisterSize.Simd64;

            Vd = ((opCode >> 18) & 0x10) | ((opCode >> 12) & 0xf);
            Vm = ((opCode >> 1) & 0x10) | ((opCode >> 0) & 0xf);

            // Subclasses have their own handling of Vx to account for before checking.
            if (GetType() == typeof(OpCode32Simd) && DecoderHelper.VectorArgumentsInvalid(Q, Vd, Vm))
            {
                Instruction = InstDescriptor.Undefined;
            }
        }
    }
}
