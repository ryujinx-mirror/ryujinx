using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeBImm64 : OpCode64
    {
        public long Imm { get; protected set; }

        public OpCodeBImm64(Inst inst, long position, int opCode) : base(inst, position, opCode) { }
    }
}