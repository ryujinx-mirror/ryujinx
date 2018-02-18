using ChocolArm64.Instruction;
using ChocolArm64.State;

namespace ChocolArm64.Decoder
{
    class AOpCodeCcmp : AOpCodeAlu, IAOpCodeCond
    {
        public    int NZCV { get; private set; }
        protected int RmImm;

        public ACond Cond { get; private set; }

        public AOpCodeCcmp(AInst Inst, long Position, int OpCode) : base(Inst, Position, OpCode)
        {
            int O3 = (OpCode >> 4) & 1;

            if (O3 != 0)
            {
                Emitter = AInstEmit.Und;

                return;
            }

            NZCV  =         (OpCode >>  0) & 0xf;
            Cond  = (ACond)((OpCode >> 12) & 0xf);
            RmImm =         (OpCode >> 16) & 0x1f;

            Rd = AThreadState.ZRIndex;
        }
    }
}