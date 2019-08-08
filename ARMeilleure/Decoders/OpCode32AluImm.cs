using ARMeilleure.Common;

namespace ARMeilleure.Decoders
{
    class OpCode32AluImm : OpCode32Alu
    {
        public int Immediate { get; private set; }

        public bool IsRotated { get; private set; }

        public OpCode32AluImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            int value = (opCode >> 0) & 0xff;
            int shift = (opCode >> 8) & 0xf;

            Immediate = BitUtils.RotateRight(value, shift * 2, 32);

            IsRotated = shift != 0;
        }
    }
}