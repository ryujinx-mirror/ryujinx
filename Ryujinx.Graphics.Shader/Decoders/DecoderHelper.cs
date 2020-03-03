using System;

namespace Ryujinx.Graphics.Shader.Decoders
{
    static class DecoderHelper
    {
        public static int DecodeS20Immediate(long opCode)
        {
            int imm = opCode.Extract(20, 19);

            bool sign = opCode.Extract(56);

            if (sign)
            {
                imm = (imm << 13) >> 13;
            }

            return imm;
        }

        public static int Decode2xF10Immediate(long opCode)
        {
            int immH0 = opCode.Extract(20, 9);
            int immH1 = opCode.Extract(30, 9);

            bool negateH0 = opCode.Extract(29);
            bool negateH1 = opCode.Extract(56);

            if (negateH0)
            {
                immH0 |= 1 << 9;
            }

            if (negateH1)
            {
                immH1 |= 1 << 9;
            }

            return immH1 << 22 | immH0 << 6;
        }

        public static float DecodeF20Immediate(long opCode)
        {
            int imm = opCode.Extract(20, 19);

            bool negate = opCode.Extract(56);

            imm <<= 12;

            if (negate)
            {
                imm |= 1 << 31;
            }

            return BitConverter.Int32BitsToSingle(imm);
        }

        public static float DecodeD20Immediate(long opCode)
        {
            long imm = opCode.Extract(20, 19);

            bool negate = opCode.Extract(56);

            imm <<= 44;

            if (negate)
            {
                imm |= 1L << 63;
            }

            return (float)BitConverter.Int64BitsToDouble(imm);
        }
    }
}