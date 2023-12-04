using ARMeilleure.State;

namespace ARMeilleure.Decoders
{
    class OpCode32SimdMemSingle : OpCode32, IOpCode32Simd
    {
        public int Vd { get; }
        public int Rn { get; }
        public int Rm { get; }
        public int IndexAlign { get; }
        public int Index { get; }
        public bool WBack { get; }
        public bool RegisterIndex { get; }
        public int Size { get; }
        public bool Replicate { get; }
        public int Increment { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdMemSingle(inst, address, opCode, false);
        public static OpCode CreateT32(InstDescriptor inst, ulong address, int opCode) => new OpCode32SimdMemSingle(inst, address, opCode, true);

        public OpCode32SimdMemSingle(InstDescriptor inst, ulong address, int opCode, bool isThumb) : base(inst, address, opCode)
        {
            IsThumb = isThumb;

            Vd = (opCode >> 12) & 0xf;
            Vd |= (opCode >> 18) & 0x10;

            IndexAlign = (opCode >> 4) & 0xf;

            Size = (opCode >> 10) & 0x3;
            Replicate = Size == 3;
            if (Replicate)
            {
                Size = (opCode >> 6) & 0x3;
                Increment = ((opCode >> 5) & 1) + 1;
                Index = 0;
            }
            else
            {
                Increment = (((IndexAlign >> Size) & 1) == 0) ? 1 : 2;
                Index = IndexAlign >> (1 + Size);
            }

            Rm = (opCode >> 0) & 0xf;
            Rn = (opCode >> 16) & 0xf;

            WBack = Rm != RegisterAlias.Aarch32Pc;
            RegisterIndex = Rm != RegisterAlias.Aarch32Pc && Rm != RegisterAlias.Aarch32Sp;
        }
    }
}
