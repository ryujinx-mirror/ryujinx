using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeSimdCvt64 : OpCodeSimd64
    {
        public int FBits { get; private set; }

        public OpCodeSimdCvt64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            //TODO:
            //Und of Fixed Point variants.
            int scale = (opCode >> 10) & 0x3f;
            int sf    = (opCode >> 31) & 0x1;

            /*if (Type != SF && !(Type == 2 && SF == 1))
            {
                Emitter = AInstEmit.Und;

                return;
            }*/

            FBits = 64 - scale;

            RegisterSize = sf != 0
                ? State.RegisterSize.Int64
                : State.RegisterSize.Int32;
        }
    }
}