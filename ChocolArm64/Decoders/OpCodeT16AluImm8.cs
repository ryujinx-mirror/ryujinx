using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeT16AluImm8 : OpCodeT16, IOpCode32Alu
    {
        private int _rdn;

        public int Rd => _rdn;
        public int Rn => _rdn;

        public bool SetFlags => false;

        public int Imm { get; private set; }

        public OpCodeT16AluImm8(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Imm  = (opCode >> 0) & 0xff;
            _rdn = (opCode >> 8) & 0x7;
        }
    }
}