using ARMeilleure.State;

namespace ARMeilleure.Decoders
{
    class OpCodeCcmp : OpCodeAlu, IOpCodeCond
    {
        public    int Nzcv { get; private set; }
        protected int RmImm;

        public Condition Cond { get; private set; }

        public OpCodeCcmp(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            int o3 = (opCode >> 4) & 1;

            if (o3 != 0)
            {
                Instruction = InstDescriptor.Undefined;

                return;
            }

            Nzcv  =             (opCode >>  0) & 0xf;
            Cond  = (Condition)((opCode >> 12) & 0xf);
            RmImm =             (opCode >> 16) & 0x1f;

            Rd = RegisterAlias.Zr;
        }
    }
}