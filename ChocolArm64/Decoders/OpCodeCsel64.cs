using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeCsel64 : OpCodeAlu64, IOpCodeCond64
    {
        public int Rm { get; private set; }

        public Cond Cond { get; private set; }

        public OpCodeCsel64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rm   =        (opCode >> 16) & 0x1f;
            Cond = (Cond)((opCode >> 12) & 0xf);
        }
    }
}