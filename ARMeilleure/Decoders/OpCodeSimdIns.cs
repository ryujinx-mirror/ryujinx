namespace ARMeilleure.Decoders
{
    class OpCodeSimdIns : OpCodeSimd
    {
        public int SrcIndex { get; }
        public int DstIndex { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSimdIns(inst, address, opCode);

        public OpCodeSimdIns(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            int imm4 = (opCode >> 11) & 0xf;
            int imm5 = (opCode >> 16) & 0x1f;

            if (imm5 == 0b10000)
            {
                Instruction = InstDescriptor.Undefined;

                return;
            }

            Size = imm5 & -imm5;

            switch (Size)
            {
                case 1: Size = 0; break;
                case 2: Size = 1; break;
                case 4: Size = 2; break;
                case 8: Size = 3; break;
            }

            SrcIndex = imm4 >>  Size;
            DstIndex = imm5 >> (Size + 1);
        }
    }
}