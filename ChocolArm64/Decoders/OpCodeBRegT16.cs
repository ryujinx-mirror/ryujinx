using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeBRegT16 : OpCodeT16, IOpCodeBReg32
    {
        public int Rm { get; private set; }

        public OpCodeBRegT16(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rm = (opCode >> 3) & 0xf;
        }
    }
}