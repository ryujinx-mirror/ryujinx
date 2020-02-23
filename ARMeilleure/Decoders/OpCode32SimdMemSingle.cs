using ARMeilleure.State;

namespace ARMeilleure.Decoders
{
    class OpCode32SimdMemSingle : OpCode32, IOpCode32Simd
    {
        public int Vd { get; private set; }
        public int Rn { get; private set; }
        public int Rm { get; private set; }
        public int IndexAlign { get; private set; }
        public int Index { get; private set; }
        public bool WBack { get; private set; }
        public bool RegisterIndex { get; private set; }
        public int Size { get; private set; }
        public bool Replicate { get; private set; }
        public int Increment { get; private set; }

        public OpCode32SimdMemSingle(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
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
