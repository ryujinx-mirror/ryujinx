using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeSimdMemLit64 : OpCode64, IOpCodeSimd64, IOpCodeLit64
    {
        public int  Rt   { get; private set; }
        public long Imm  { get; private set; }
        public int  Size { get; private set; }
        public bool Signed   => false;
        public bool Prefetch => false;

        public OpCodeSimdMemLit64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            int opc = (opCode >> 30) & 3;

            if (opc == 3)
            {
                Emitter = InstEmit.Und;

                return;
            }

            Rt = opCode & 0x1f;

            Imm = position + DecoderHelper.DecodeImmS19_2(opCode);

            Size = opc + 2;
        }
    }
}