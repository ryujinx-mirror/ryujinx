using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeSimdMemPair64 : OpCodeMemPair64, IOpCodeSimd64
    {
        public OpCodeSimdMemPair64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Size = ((opCode >> 30) & 3) + 2;

            Extend64 = false;

            DecodeImm(opCode);
        }
    }
}