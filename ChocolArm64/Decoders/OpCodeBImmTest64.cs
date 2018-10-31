using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeBImmTest64 : OpCodeBImm64
    {
        public int Rt  { get; private set; }
        public int Pos { get; private set; }

        public OpCodeBImmTest64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rt = opCode & 0x1f;

            Imm = position + DecoderHelper.DecodeImmS14_2(opCode);

            Pos  = (opCode >> 19) & 0x1f;
            Pos |= (opCode >> 26) & 0x20;
        }
    }
}