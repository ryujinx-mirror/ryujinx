using ARMeilleure.State;

namespace ARMeilleure.Decoders
{
    class OpCodeT16MemLit : OpCodeT16, IOpCode32Mem
    {
        public int Rt { get; }
        public int Rn => RegisterAlias.Aarch32Pc;

        public bool WBack => false;
        public bool IsLoad => true;
        public bool Index => true;
        public bool Add => true;

        public int Immediate { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT16MemLit(inst, address, opCode);

        public OpCodeT16MemLit(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rt = (opCode >> 8) & 7;

            Immediate = (opCode & 0xff) << 2;
        }
    }
}
