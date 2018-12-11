using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeAluRs64 : OpCodeAlu64, IOpCodeAluRs64
    {
        public int Shift { get; private set; }
        public int Rm    { get; private set; }

        public ShiftType ShiftType { get; private set; }

        public OpCodeAluRs64(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            int shift = (opCode >> 10) & 0x3f;

            if (shift >= GetBitsCount())
            {
                Emitter = InstEmit.Und;

                return;
            }

            Shift = shift;

            Rm        =             (opCode >> 16) & 0x1f;
            ShiftType = (ShiftType)((opCode >> 22) & 0x3);
        }
    }
}