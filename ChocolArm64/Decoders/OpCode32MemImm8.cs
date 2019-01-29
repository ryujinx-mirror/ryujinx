using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCode32MemImm8 : OpCode32Mem
    {
        public OpCode32MemImm8(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            int imm4L = (opCode >> 0) & 0xf;
            int imm4H = (opCode >> 8) & 0xf;

            Imm = imm4L | (imm4H << 4);
        }
    }
}