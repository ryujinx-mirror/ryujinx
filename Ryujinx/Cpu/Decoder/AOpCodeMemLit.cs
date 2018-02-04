using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeMemLit : AOpCode, IAOpCodeLit
    {
        public int  Rt       { get; private set; }
        public long Imm      { get; private set; }
        public int  Size     { get; private set; }
        public bool Signed   { get; private set; }
        public bool Prefetch { get; private set; }

        public AOpCodeMemLit(AInst Inst, long Position, int OpCode) : base(Inst, Position)
        {
            Rt = OpCode & 0x1f;

            Imm = Position + ADecoderHelper.DecodeImmS19_2(OpCode);

            switch ((OpCode >> 30) & 3)
            {
                case 0: Size = 2; Signed = false; Prefetch = false; break;
                case 1: Size = 3; Signed = false; Prefetch = false; break;
                case 2: Size = 2; Signed = true;  Prefetch = false; break;
                case 3: Size = 0; Signed = false; Prefetch = true;  break;
            }
        }
    }
}