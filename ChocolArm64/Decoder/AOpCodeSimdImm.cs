using ChocolArm64.Instruction;
using ChocolArm64.State;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdImm : AOpCode, IAOpCodeSimd
    {
        public int  Rd   { get; private set; }
        public long Imm  { get; private set; }
        public int  Size { get; private set; }

        public AOpCodeSimdImm(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            Rd = OpCode & 0x1f;

            int CMode = (OpCode >> 12) & 0xf;
            int Op    = (OpCode >> 29) & 0x1;

            int ModeLow  = CMode &  1;
            int ModeHigh = CMode >> 1;

            long Imm;

            Imm  = ((uint)OpCode >>  5) & 0x1f;
            Imm |= ((uint)OpCode >> 11) & 0xe0;

            if (ModeHigh == 0b111)
            {
                Size = ModeLow != 0 ? Op : 3;

                switch (Op | (ModeLow << 1))
                {
                    case 0:
                        //64-bits Immediate.
                        //Transform abcd efgh into abcd efgh abcd efgh ...
                        Imm = (long)((ulong)Imm * 0x0101010101010101);
                        break;

                    case 1:
                        //64-bits Immediate.
                        //Transform abcd efgh into aaaa aaaa bbbb bbbb ...
                        Imm = (Imm & 0xf0) >> 4 | (Imm & 0x0f) << 4;
                        Imm = (Imm & 0xcc) >> 2 | (Imm & 0x33) << 2;
                        Imm = (Imm & 0xaa) >> 1 | (Imm & 0x55) << 1;

                        Imm = (long)((ulong)Imm * 0x8040201008040201);
                        Imm = (long)((ulong)Imm & 0x8080808080808080);

                        Imm |= Imm >> 4;
                        Imm |= Imm >> 2;
                        Imm |= Imm >> 1;
                        break;

                    case 2:
                    case 3:
                        //Floating point Immediate.
                        Imm = ADecoderHelper.DecodeImm8Float(Imm, Size);
                        break;
                }
            }
            else if ((ModeHigh & 0b110) == 0b100)
            {
                //16-bits shifted Immediate.
                Size = 1; Imm <<= (ModeHigh & 1) << 3; 
            }
            else if ((ModeHigh & 0b100) == 0b000)
            {
                //32-bits shifted Immediate.
                Size = 2; Imm <<= ModeHigh << 3; 
            }
            else if ((ModeHigh & 0b111) == 0b110)
            {
                //32-bits shifted Immediate (fill with ones).
                Size = 2; Imm = ShlOnes(Imm, 8 << ModeLow);
            }
            else
            {
                //8 bits without shift.
                Size = 0;
            }

            this.Imm = Imm;

            RegisterSize = ((OpCode >> 30) & 1) != 0
                ? ARegisterSize.SIMD128
                : ARegisterSize.SIMD64;
        }

        private static long ShlOnes(long Value, int Shift)
        {
            return Value << Shift | (long)(ulong.MaxValue >> (64 - Shift));
        }
    }
}