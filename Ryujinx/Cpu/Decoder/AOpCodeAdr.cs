using ChocolArm64.Instruction;

namespace ChocolArm64.Decoder
{
    class AOpCodeAdr : AOpCode
    {
        public int  Rd  { get; private set; }
        public long Imm { get; private set; }

         public AOpCodeAdr(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Rd = OpCode & 0x1f;

            Imm  = ADecoderHelper.DecodeImmS19_2(OpCode);
            Imm |= ((long)OpCode >> 29) & 3;
        }
    }
}