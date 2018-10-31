using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeAluRx64 : OpCodeAlu64, IOpCodeAluRx64
    {
        public int Shift { get; private set; }
        public int Rm    { get; private set; }

        public IntType IntType { get; private set; }

        public OpCodeAluRx64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Shift   =            (opCode >> 10) & 0x7;
            IntType = (IntType)((opCode >> 13) & 0x7);
            Rm      =            (opCode >> 16) & 0x1f;
        }
    }
}