using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeMul64 : OpCodeAlu64
    {
        public int Rm { get; private set; }
        public int Ra { get; private set; }

        public OpCodeMul64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Ra = (opCode >> 10) & 0x1f;
            Rm = (opCode >> 16) & 0x1f;
        }
    }
}