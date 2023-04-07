using ARMeilleure.State;

namespace ARMeilleure.Decoders
{
    class OpCodeT32AluUx : OpCodeT32AluReg, IOpCode32AluUx
    {
        public int Rotate { get; }
        public int RotateBits => Rotate * 8;
        public bool Add => Rn != RegisterAlias.Aarch32Pc;

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32AluUx(inst, address, opCode);

        public OpCodeT32AluUx(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rotate = (opCode >> 4) & 0x3;
        }
    }
}
