using ARMeilleure.Common;

namespace ARMeilleure.Decoders
{
    class OpCodeSimdShImm : OpCodeSimd
    {
        public int Imm { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSimdShImm(inst, address, opCode);

        public OpCodeSimdShImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Imm = (opCode >> 16) & 0x7f;

            Size = BitUtils.HighestBitSetNibble(Imm >> 3);
        }
    }
}
