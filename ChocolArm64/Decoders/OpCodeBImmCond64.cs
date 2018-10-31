using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeBImmCond64 : OpCodeBImm64, IOpCodeCond64
    {
        public Cond Cond { get; private set; }

        public OpCodeBImmCond64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            int o0 = (opCode >> 4) & 1;

            if (o0 != 0)
            {
                Emitter = InstEmit.Und;

                return;
            }

            Cond = (Cond)(opCode & 0xf);

            Imm = position + DecoderHelper.DecodeImmS19_2(opCode);
        }
    }
}