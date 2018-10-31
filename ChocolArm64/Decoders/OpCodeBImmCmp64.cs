using ChocolArm64.Instructions;
using ChocolArm64.State;

namespace ChocolArm64.Decoders
{
    class OpCodeBImmCmp64 : OpCodeBImm64
    {
        public int Rt { get; private set; }

        public OpCodeBImmCmp64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rt = opCode & 0x1f;

            Imm = position + DecoderHelper.DecodeImmS19_2(opCode);

            RegisterSize = (opCode >> 31) != 0
                ? State.RegisterSize.Int64
                : State.RegisterSize.Int32;
        }
    }
}