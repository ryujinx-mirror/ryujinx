using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCodeAluRsImm32 : OpCodeAlu32
    {
        public int Rm  { get; private set; }
        public int Imm { get; private set; }

        public ShiftType ShiftType { get; private set; }

        public OpCodeAluRsImm32(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rm  = (opCode >> 0) & 0xf;
            Imm = (opCode >> 7) & 0x1f;

            ShiftType = (ShiftType)((opCode >> 5) & 3);
        }
    }
}