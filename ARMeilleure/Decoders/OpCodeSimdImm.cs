namespace ARMeilleure.Decoders
{
    class OpCodeSimdImm : OpCode, IOpCodeSimd
    {
        public int  Rd        { get; }
        public long Immediate { get; }
        public int  Size      { get; }

        public new static OpCode Create(InstDescriptor inst, ulong address, int opCode) => new OpCodeSimdImm(inst, address, opCode);

        public OpCodeSimdImm(InstDescriptor inst, ulong address, int opCode) : base(inst, address, opCode)
        {
            Rd = opCode & 0x1f;

            int cMode = (opCode >> 12) & 0xf;
            int op    = (opCode >> 29) & 0x1;

            int modeLow  = cMode &  1;
            int modeHigh = cMode >> 1;

            long imm;

            imm  = ((uint)opCode >>  5) & 0x1f;
            imm |= ((uint)opCode >> 11) & 0xe0;

            if (modeHigh == 0b111)
            {
                switch (op | (modeLow << 1))
                {
                    case 0:
                        // 64-bits Immediate.
                        // Transform abcd efgh into abcd efgh abcd efgh ...
                        Size = 3;
                        imm = (long)((ulong)imm * 0x0101010101010101);
                        break;

                    case 1:
                        // 64-bits Immediate.
                        // Transform abcd efgh into aaaa aaaa bbbb bbbb ...
                        Size = 3;
                        imm = (imm & 0xf0) >> 4 | (imm & 0x0f) << 4;
                        imm = (imm & 0xcc) >> 2 | (imm & 0x33) << 2;
                        imm = (imm & 0xaa) >> 1 | (imm & 0x55) << 1;

                        imm = (long)((ulong)imm * 0x8040201008040201);
                        imm = (long)((ulong)imm & 0x8080808080808080);

                        imm |= imm >> 4;
                        imm |= imm >> 2;
                        imm |= imm >> 1;
                        break;

                    case 2:
                        // 2 x 32-bits floating point Immediate.
                        Size = 0;
                        imm = (long)DecoderHelper.Imm8ToFP32Table[(int)imm];
                        imm |= imm << 32;
                        break;

                    case 3:
                        // 64-bits floating point Immediate.
                        Size = 1;
                        imm = (long)DecoderHelper.Imm8ToFP64Table[(int)imm];
                        break;
                }
            }
            else if ((modeHigh & 0b110) == 0b100)
            {
                // 16-bits shifted Immediate.
                Size = 1; imm <<= (modeHigh & 1) << 3;
            }
            else if ((modeHigh & 0b100) == 0b000)
            {
                // 32-bits shifted Immediate.
                Size = 2; imm <<= modeHigh << 3;
            }
            else if ((modeHigh & 0b111) == 0b110)
            {
                // 32-bits shifted Immediate (fill with ones).
                Size = 2; imm = ShlOnes(imm, 8 << modeLow);
            }
            else
            {
                // 8-bits without shift.
                Size = 0;
            }

            Immediate = imm;

            RegisterSize = ((opCode >> 30) & 1) != 0
                ? RegisterSize.Simd128
                : RegisterSize.Simd64;
        }

        private static long ShlOnes(long value, int shift)
        {
            if (shift != 0)
            {
                return value << shift | (long)(ulong.MaxValue >> (64 - shift));
            }
            else
            {
                return value;
            }
        }
    }
}