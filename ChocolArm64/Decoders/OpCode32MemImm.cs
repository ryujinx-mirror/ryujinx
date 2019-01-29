using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCode32MemImm : OpCode32Mem
    {
        public OpCode32MemImm(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Imm = opCode & 0xfff;
        }
    }
}