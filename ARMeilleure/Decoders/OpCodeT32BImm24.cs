using ARMeilleure.Instructions;

namespace ARMeilleure.Decoders
{
    class OpCodeT32BImm24 : OpCodeT32, IOpCode32BImm
    {
        public long Immediate { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeT32BImm24(inst, address, opCode);

        public OpCodeT32BImm24(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            uint pc = GetPc();

            if (inst.Name == InstName.Blx)
            {
                pc &= ~3u;
            }

            int imm11 = (opCode >> 0) & 0x7ff;
            int j2 = (opCode >> 11) & 1;
            int j1 = (opCode >> 13) & 1;
            int imm10 = (opCode >> 16) & 0x3ff;
            int s = (opCode >> 26) & 1;

            int i1 = j1 ^ s ^ 1;
            int i2 = j2 ^ s ^ 1;

            int imm32 = imm11 | (imm10 << 11) | (i2 << 21) | (i1 << 22) | (s << 23);
            imm32 = (imm32 << 9) >> 8;

            Immediate = pc + imm32;
        }
    }
}