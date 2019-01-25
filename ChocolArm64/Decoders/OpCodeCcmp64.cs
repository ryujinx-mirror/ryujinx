using ChocolArm64.Instructions;
using ChocolArm64.State;

namespace ChocolArm64.Decoders
{
    class OpCodeCcmp64 : OpCodeAlu64, IOpCodeCond64
    {
        public    int Nzcv { get; private set; }
        protected int RmImm;

        public Condition Cond { get; private set; }

        public OpCodeCcmp64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            int o3 = (opCode >> 4) & 1;

            if (o3 != 0)
            {
                Emitter = InstEmit.Und;

                return;
            }

            Nzcv  =             (opCode >>  0) & 0xf;
            Cond  = (Condition)((opCode >> 12) & 0xf);
            RmImm =             (opCode >> 16) & 0x1f;

            Rd = RegisterAlias.Zr;
        }
    }
}