using ARMeilleure.State;

namespace ARMeilleure.Decoders
{
    class OpCode32AluUx : OpCode32AluReg, IOpCode32AluUx
    {
        public int Rotate { get; }
        public int RotateBits => Rotate * 8;
        public bool Add => Rn != RegisterAlias.Aarch32Pc;

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCode32AluUx(inst, address, opCode);

        public OpCode32AluUx(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rotate = (opCode >> 10) & 0x3;
        }
    }
}
