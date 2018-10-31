using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeSimdReg64 : OpCodeSimd64
    {
        public bool Bit3 { get; private   set; }
        public int  Ra   { get; private   set; }
        public int  Rm   { get; protected set; }

        public OpCodeSimdReg64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Bit3 = ((opCode >>  3) & 0x1) != 0;
            Ra   =  (opCode >> 10) & 0x1f;
            Rm   =  (opCode >> 16) & 0x1f;
        }
    }
}