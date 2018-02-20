using ChocolArm64.Instruction;
using ChocolArm64.State;

namespace ChocolArm64.Decoder
{
    class AOpCodeSimdCvt : AOpCodeSimd
    {
        public int FBits { get; private set; }

        public AOpCodeSimdCvt(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            //TODO:
            //Und of Fixed Point variants.
            int Scale = (OpCode >> 10) & 0x3f;
            int SF    = (OpCode >> 31) & 0x1;

            /*if (Type != SF && !(Type == 2 && SF == 1))
            {
                Emitter = AInstEmit.Und;

                return;
            }*/

            FBits = 64 - Scale;

            RegisterSize = SF != 0
                ? ARegisterSize.Int64
                : ARegisterSize.Int32;
        }
    }
}