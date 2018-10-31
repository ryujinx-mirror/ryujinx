using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeMemPair64 : OpCodeMemImm64
    {
        public int Rt2 { get; private set; }

        public OpCodeMemPair64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rt2      =  (opCode >> 10) & 0x1f;
            WBack    = ((opCode >> 23) & 0x1) != 0;
            PostIdx  = ((opCode >> 23) & 0x3) == 1;
            Extend64 = ((opCode >> 30) & 0x3) == 1;
            Size     = ((opCode >> 31) & 0x1) | 2;

            DecodeImm(opCode);
        }

        protected void DecodeImm(int opCode)
        {
            Imm = ((long)(opCode >> 15) << 57) >> (57 - Size);
        }
    }
}