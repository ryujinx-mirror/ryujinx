using ChocolArm64.Instructions;
using ChocolArm64.State;

namespace ChocolArm64.Decoders
{
    class OpCodeSimd64 : OpCode64, IOpCodeSimd64
    {
        public int Rd   { get; private   set; }
        public int Rn   { get; private   set; }
        public int Opc  { get; private   set; }
        public int Size { get; protected set; }

        public OpCodeSimd64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rd   = (opCode >>  0) & 0x1f;
            Rn   = (opCode >>  5) & 0x1f;
            Opc  = (opCode >> 15) & 0x3;
            Size = (opCode >> 22) & 0x3;

            RegisterSize = ((opCode >> 30) & 1) != 0
                ? RegisterSize.Simd128
                : RegisterSize.Simd64;
        }
    }
}