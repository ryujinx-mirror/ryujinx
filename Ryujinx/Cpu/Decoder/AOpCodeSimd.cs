using ChocolArm64.Instruction;
using ChocolArm64.State;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimd : AOpCode, IAOpCodeSimd
    {
        public int Rd   { get; private   set; }
        public int Rn   { get; private   set; }
        public int Opc  { get; private   set; }
        public int Size { get; protected set; }

        public int SizeF => Size & 1;

        public AOpCodeSimd(AInst Inst, long Position, int OpCode) : base(Inst, Position)
        {
            Rd   = (OpCode >>  0) & 0x1f;
            Rn   = (OpCode >>  5) & 0x1f;
            Opc  = (OpCode >> 15) & 0x3;
            Size = (OpCode >> 22) & 0x3;

            RegisterSize = ((OpCode >> 30) & 1) != 0
                ? ARegisterSize.SIMD128
                : ARegisterSize.SIMD64;
        }
    }
}