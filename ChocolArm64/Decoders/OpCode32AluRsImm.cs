using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCode32AluRsImm : OpCode32Alu
    {
        public int Rm  { get; private set; }
        public int Imm { get; private set; }

        public ShiftType ShiftType { get; private set; }

        public OpCode32AluRsImm(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            Rm  = (opCode >> 0) & 0xf;
            Imm = (opCode >> 7) & 0x1f;

            ShiftType = (ShiftType)((opCode >> 5) & 3);
        }
    }
}