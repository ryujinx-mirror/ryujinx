using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeCcmpImm64 : OpCodeCcmp64, IOpCodeAluImm64
    {
        public long Imm => RmImm;

        public OpCodeCcmpImm64(Inst inst, long position, int opCode) : base(inst, position, opCode) { }
    }
}