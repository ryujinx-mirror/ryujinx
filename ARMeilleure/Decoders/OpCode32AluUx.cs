using ARMeilleure.State;

namespace ARMeilleure.Decoders
{
    class OpCode32AluUx : OpCode32AluReg, IOpCode32AluUx
    {
        public int Rotate { get; private set; }
        public int RotateBits => Rotate * 8;
        public bool Add => Rn != RegisterAlias.Aarch32Pc;

        public OpCode32AluUx(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rotate = (opCode >> 10) & 0x3;
        }
    }
}
