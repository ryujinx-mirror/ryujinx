using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeBReg32 : OpCode32, IOpCodeBReg32
    {
        public int Rm { get; private set; }

        public OpCodeBReg32(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rm = opCode & 0xf;
        }
    }
}