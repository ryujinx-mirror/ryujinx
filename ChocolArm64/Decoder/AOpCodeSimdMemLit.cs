using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdMemLit : AOpCode, IAOpCodeSimd, IAOpCodeLit
    {
        public int  Rt   { get; private set; }
        public long Imm  { get; private set; }
        public int  Size { get; private set; }
        public bool Signed   => false;
        public bool Prefetch => false;

        public AOpCodeSimdMemLit(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            int Opc = (OpCode >> 30) & 3;

            if (Opc == 3)
            {
                Emitter = AInstEmit.Und;

                return;
            }

            Rt = OpCode & 0x1f;

            Imm = Position + ADecoderHelper.DecodeImmS19_2(OpCode);

            Size = Opc + 2;
        }
    }
}