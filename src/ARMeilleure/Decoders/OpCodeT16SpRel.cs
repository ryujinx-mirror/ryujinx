using ARMeilleure.State;

namespace ARMeilleure.Decoders
{
    class OpCodeT16SpRel : OpCodeT16, IOpCode32AluImm
    {
        public int Rd { get; }
        public int Rn => RegisterAlias.Aarch32Sp;

        public bool? SetFlags => false;

        public int Immediate { get; }

        public bool IsRotated => false;

        public static new OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16SpRel(inst, address, opCode);

        public OpCodeT16SpRel(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = (opCode >> 8) & 0x7;
            Immediate = ((opCode >> 0) & 0xff) << 2;
        }
    }
}
