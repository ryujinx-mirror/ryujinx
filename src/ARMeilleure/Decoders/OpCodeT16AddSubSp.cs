using ARMeilleure.State;

namespace ARMeilleure.Decoders
{
    class OpCodeT16AddSubSp : OpCodeT16, IOpCode32AluImm
    {
        public int Rd => RegisterAlias.Aarch32Sp;
        public int Rn => RegisterAlias.Aarch32Sp;

        public bool? SetFlags => false;

        public int Immediate { get; }

        public bool IsRotated => false;

        public static new OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16AddSubSp(inst, address, opCode);

        public OpCodeT16AddSubSp(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Immediate = ((opCode >> 0) & 0x7f) << 2;
        }
    }
}
