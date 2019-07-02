using System;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    static class HalfConversion
    {
        public static float HalfToSingle(int value)
        {
            int mantissa = (value >> 0)  & 0x3ff;
            int exponent = (value >> 10) & 0x1f;
            int sign     = (value >> 15) & 0x1;

            if (exponent == 0x1f)
            {
                // NaN or Infinity.
                mantissa <<= 13;
                exponent   = 0xff;
            }
            else if (exponent != 0 || mantissa != 0 )
            {
                if (exponent == 0)
                {
                    // Denormal.
                    int e = -1;
                    int m = mantissa;

                    do
                    {
                        e++;
                        m <<= 1;
                    }
                    while ((m & 0x400) == 0);

                    mantissa = m & 0x3ff;
                    exponent = e;
                }

                mantissa <<= 13;
                exponent   = 127 - 15 + exponent;
            }

            int output = (sign << 31) | (exponent << 23) | mantissa;

            return BitConverter.Int32BitsToSingle(output);
        }
    }
}