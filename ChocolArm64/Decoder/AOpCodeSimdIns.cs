using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdIns : AOpCodeSimd
    {
        public int SrcIndex { get; private set; }
        public int DstIndex { get; private set; }

        public AOpCodeSimdIns(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            int Imm4 = (OpCode >> 11) & 0xf;
            int Imm5 = (OpCode >> 16) & 0x1f;

            if (Imm5 == 0b10000)
            {
                Emitter = AInstEmit.Und;

                return;
            }

            Size = Imm5 & -Imm5;

            switch (Size)
            {
                case 1: Size = 0; break;
                case 2: Size = 1; break;
                case 4: Size = 2; break;
                case 8: Size = 3; break;
            }

            SrcIndex = Imm4 >>  Size;
            DstIndex = Imm5 >> (Size + 1);
        }
    }
}