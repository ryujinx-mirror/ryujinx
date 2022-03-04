using ARMeilleure.Instructions;

namespace ARMeilleure.Decoders
{
    class OpCodeT32BImm20 : OpCodeT32, IOpCode32BImm
    {
        public long Immediate { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32BImm20(inst, address, opCode);

        public OpCodeT32BImm20(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            uint pc = GetPc();

            int imm11 = (opCode >> 0) & 0x7ff;
            int j2 = (opCode >> 11) & 1;
            int j1 = (opCode >> 13) & 1;
            int imm6 = (opCode >> 16) & 0x3f;
            int s = (opCode >> 26) & 1;

            int imm32 = imm11 | (imm6 << 11) | (j1 << 17) | (j2 << 18) | (s << 19);
            imm32 = (imm32 << 13) >> 12;

            Immediate = pc + imm32;

            Cond = (Condition)((opCode >> 22) & 0xf);
        }
    }
}