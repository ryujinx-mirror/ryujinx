using ARMeilleure.State;
using System;

namespace ARMeilleure.Decoders
{
    class OpCode32SimdMemPair : OpCode32, IOpCode32Simd
    {
        private static int[] RegsMap =
        {
            1, 1, 4, 2,
            1, 1, 3, 1,
            1, 1, 2, 1,
            1, 1, 1, 1 
        };

        public int Vd { get; private set; }
        public int Rn { get; private set; }
        public int Rm { get; private set; }
        public int Align { get; private set; }
        public bool WBack { get; private set; }
        public bool RegisterIndex { get; private set; }
        public int Size { get; private set; }
        public int Elems => 8 >> Size;
        public int Regs { get; private set; }
        public int Increment { get; private set; }

        public OpCode32SimdMemPair(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Vd = (opCode >> 12) & 0xf;
            Vd |= (opCode >> 18) & 0x10;

            Size = (opCode >> 6) & 0x3;

            Align = (opCode >> 4) & 0x3;
            Rm = (opCode >> 0) & 0xf;
            Rn = (opCode >> 16) & 0xf;

            WBack = Rm != RegisterAlias.Aarch32Pc;
            RegisterIndex = Rm != RegisterAlias.Aarch32Pc && Rm != RegisterAlias.Aarch32Sp;

            Regs = RegsMap[(opCode >> 8) & 0xf];

            Increment = Math.Min(Regs, ((opCode >> 8) & 0x1) + 1);
        }
    }
}
