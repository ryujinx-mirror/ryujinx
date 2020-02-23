namespace ARMeilleure.Decoders
{
    class OpCode32SimdImm44 : OpCode32, IOpCode32SimdImm
    {
        public int Vd { get; private set; }
        public long Immediate { get; private set; }
        public int Size { get; private set; }
        public int Elems { get; private set; }

        public OpCode32SimdImm44(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Size = (opCode >> 8) & 0x3;

            bool single = Size != 3;
            
            if (single)
            {
                Vd = ((opCode >> 22) & 0x1) | ((opCode >> 11) & 0x1e);
            }
            else
            {
                Vd = ((opCode >> 18) & 0x10) | ((opCode >> 12) & 0xf);
            }

            long imm;

            imm = ((uint)opCode >> 0) & 0xf;
            imm |= ((uint)opCode >> 12) & 0xf0;

            Immediate = (Size == 3) ? (long)DecoderHelper.Imm8ToFP64Table[(int)imm] : DecoderHelper.Imm8ToFP32Table[(int)imm];

            RegisterSize = (!single) ? RegisterSize.Int64 : RegisterSize.Int32;
            Elems = 1;
        }
    }
}
