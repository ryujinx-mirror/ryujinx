using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeMemLit64 : OpCode64, IOpCodeLit64
    {
        public int  Rt       { get; private set; }
        public long Imm      { get; private set; }
        public int  Size     { get; private set; }
        public bool Signed   { get; private set; }
        public bool Prefetch { get; private set; }

        public OpCodeMemLit64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rt = opCode & 0x1f;

            Imm = position + DecoderHelper.DecodeImmS19_2(opCode);

            switch ((opCode >> 30) & 3)
            {
                case 0: Size = 2; Signed = false; Prefetch = false; break;
                case 1: Size = 3; Signed = false; Prefetch = false; break;
                case 2: Size = 2; Signed = true;  Prefetch = false; break;
                case 3: Size = 0; Signed = false; Prefetch = true;  break;
            }
        }
    }
}