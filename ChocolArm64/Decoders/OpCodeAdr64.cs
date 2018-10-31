using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeAdr64 : OpCode64
    {
        public int  Rd  { get; private set; }
        public long Imm { get; private set; }

         public OpCodeAdr64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rd = opCode & 0x1f;

            Imm  = DecoderHelper.DecodeImmS19_2(opCode);
            Imm |= ((long)opCode >> 29) & 3;
        }
    }
}