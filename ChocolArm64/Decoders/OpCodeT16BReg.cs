using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeT16BReg : OpCodeT16, IOpCode32BReg
    {
        public int Rm { get; private set; }

        public OpCodeT16BReg(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rm = (opCode >> 3) & 0xf;
        }
    }
}