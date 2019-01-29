using ChocolArm64.Instructions;

namespace ChocolArm64.Decoders
{
    class OpCode32AluImm : OpCode32Alu
    {
        public int Imm { get; private set; }

        public bool IsRotated { get; private set; }

        public OpCode32AluImm(Inst inst, long position, int opCode) : base(inst, position, opCode)
        {
            int value = (opCode >> 0) & 0xff;
            int shift = (opCode >> 8) & 0xf;

            Imm = BitUtils.RotateRight(value, shift * 2, 32);

            IsRotated = shift != 0;
        }
    }
}