using System;

namespace Ryujinx.Graphics.Gal.Shader
{
    static class ShaderOpCodeTable
    {
        private const int EncodingBits = 14;

        private class ShaderDecodeEntry
        {
            public ShaderDecodeFunc Func;

            public int XBits;

            public ShaderDecodeEntry(ShaderDecodeFunc func, int xBits)
            {
                Func  = func;
                XBits = xBits;
            }
        }

        private static ShaderDecodeEntry[] _opCodes;

        static ShaderOpCodeTable()
        {
            _opCodes = new ShaderDecodeEntry[1 << EncodingBits];

#region Instructions
            Set("0100110000000x", ShaderDecode.Bfe_C);
            Set("0011100x00000x", ShaderDecode.Bfe_I);
            Set("0101110000000x", ShaderDecode.Bfe_R);
            Set("111000100100xx", ShaderDecode.Bra);
            Set("111000110000xx", ShaderDecode.Exit);
            Set("0100110010101x", ShaderDecode.F2f_C);
            Set("0011100x10101x", ShaderDecode.F2f_I);
            Set("0101110010101x", ShaderDecode.F2f_R);
            Set("0100110010110x", ShaderDecode.F2i_C);
            Set("0011100x10110x", ShaderDecode.F2i_I);
            Set("0101110010110x", ShaderDecode.F2i_R);
            Set("0100110001011x", ShaderDecode.Fadd_C);
            Set("0011100x01011x", ShaderDecode.Fadd_I);
            Set("000010xxxxxxxx", ShaderDecode.Fadd_I32);
            Set("0101110001011x", ShaderDecode.Fadd_R);
            Set("010010011xxxxx", ShaderDecode.Ffma_CR);
            Set("0011001x1xxxxx", ShaderDecode.Ffma_I);
            Set("010100011xxxxx", ShaderDecode.Ffma_RC);
            Set("010110011xxxxx", ShaderDecode.Ffma_RR);
            Set("0100110001101x", ShaderDecode.Fmul_C);
            Set("0011100x01101x", ShaderDecode.Fmul_I);
            Set("00011110xxxxxx", ShaderDecode.Fmul_I32);
            Set("0101110001101x", ShaderDecode.Fmul_R);
            Set("0100110001100x", ShaderDecode.Fmnmx_C);
            Set("0011100x01100x", ShaderDecode.Fmnmx_I);
            Set("0101110001100x", ShaderDecode.Fmnmx_R);
            Set("0100100xxxxxxx", ShaderDecode.Fset_C);
            Set("0011000xxxxxxx", ShaderDecode.Fset_I);
            Set("01011000xxxxxx", ShaderDecode.Fset_R);
            Set("010010111011xx", ShaderDecode.Fsetp_C);
            Set("0011011x1011xx", ShaderDecode.Fsetp_I);
            Set("010110111011xx", ShaderDecode.Fsetp_R);
            Set("0101110100010x", ShaderDecode.Hadd2_R);
            Set("0101110100001x", ShaderDecode.Hmul2_R);
            Set("0100110010111x", ShaderDecode.I2f_C);
            Set("0011100x10111x", ShaderDecode.I2f_I);
            Set("0101110010111x", ShaderDecode.I2f_R);
            Set("0100110011100x", ShaderDecode.I2i_C);
            Set("0011100x11100x", ShaderDecode.I2i_I);
            Set("0101110011100x", ShaderDecode.I2i_R);
            Set("0100110000010x", ShaderDecode.Iadd_C);
            Set("0011100000010x", ShaderDecode.Iadd_I);
            Set("0001110x0xxxxx", ShaderDecode.Iadd_I32);
            Set("0101110000010x", ShaderDecode.Iadd_R);
            Set("010011001100xx", ShaderDecode.Iadd3_C);
            Set("001110001100xx", ShaderDecode.Iadd3_I);
            Set("010111001100xx", ShaderDecode.Iadd3_R);
            Set("0100110000100x", ShaderDecode.Imnmx_C);
            Set("0011100x00100x", ShaderDecode.Imnmx_I);
            Set("0101110000100x", ShaderDecode.Imnmx_R);
            Set("1110111111010x", ShaderDecode.Isberd);
            Set("11100000xxxxxx", ShaderDecode.Ipa);
            Set("0100110000011x", ShaderDecode.Iscadd_C);
            Set("0011100x00011x", ShaderDecode.Iscadd_I);
            Set("0101110000011x", ShaderDecode.Iscadd_R);
            Set("010010110101xx", ShaderDecode.Iset_C);
            Set("001101100101xx", ShaderDecode.Iset_I);
            Set("010110110101xx", ShaderDecode.Iset_R);
            Set("010010110110xx", ShaderDecode.Isetp_C);
            Set("0011011x0110xx", ShaderDecode.Isetp_I);
            Set("010110110110xx", ShaderDecode.Isetp_R);
            Set("111000110011xx", ShaderDecode.Kil);
            Set("1110111111011x", ShaderDecode.Ld_A);
            Set("1110111110010x", ShaderDecode.Ld_C);
            Set("0100110001000x", ShaderDecode.Lop_C);
            Set("0011100001000x", ShaderDecode.Lop_I);
            Set("000001xxxxxxxx", ShaderDecode.Lop_I32);
            Set("0101110001000x", ShaderDecode.Lop_R);
            Set("0100110010011x", ShaderDecode.Mov_C);
            Set("0011100x10011x", ShaderDecode.Mov_I);
            Set("000000010000xx", ShaderDecode.Mov_I32);
            Set("0101110010011x", ShaderDecode.Mov_R);
            Set("1111000011001x", ShaderDecode.Mov_S);
            Set("0101000010000x", ShaderDecode.Mufu);
            Set("1111101111100x", ShaderDecode.Out_R);
            Set("0101000010010x", ShaderDecode.Psetp);
            Set("0100110010010x", ShaderDecode.Rro_C);
            Set("0011100x10010x", ShaderDecode.Rro_I);
            Set("0101110010010x", ShaderDecode.Rro_R);
            Set("0100110010100x", ShaderDecode.Sel_C);
            Set("0011100010100x", ShaderDecode.Sel_I);
            Set("0101110010100x", ShaderDecode.Sel_R);
            Set("0100110001001x", ShaderDecode.Shl_C);
            Set("0011100x01001x", ShaderDecode.Shl_I);
            Set("0101110001001x", ShaderDecode.Shl_R);
            Set("0100110000101x", ShaderDecode.Shr_C);
            Set("0011100x00101x", ShaderDecode.Shr_I);
            Set("0101110000101x", ShaderDecode.Shr_R);
            Set("111000101001xx", ShaderDecode.Ssy);
            Set("1110111111110x", ShaderDecode.St_A);
            Set("1111000011111x", ShaderDecode.Sync);
            Set("110000xxxx111x", ShaderDecode.Tex);
            Set("1101111010111x", ShaderDecode.Tex_B);
            Set("1101111101001x", ShaderDecode.Texq);
            Set("1101x00xxxxxxx", ShaderDecode.Texs);
            Set("1101101xxxxxxx", ShaderDecode.Tlds);
            Set("110010xxxx111x", ShaderDecode.Tld4);
            Set("1101111100xxxx", ShaderDecode.Tld4S);
            Set("01011111xxxxxx", ShaderDecode.Vmad);
            Set("0100111xxxxxxx", ShaderDecode.Xmad_CR);
            Set("0011011x00xxxx", ShaderDecode.Xmad_I);
            Set("010100010xxxxx", ShaderDecode.Xmad_RC);
            Set("0101101100xxxx", ShaderDecode.Xmad_RR);
#endregion
        }

        private static void Set(string encoding, ShaderDecodeFunc func)
        {
            if (encoding.Length != EncodingBits)
            {
                throw new ArgumentException(nameof(encoding));
            }

            int bit   = encoding.Length - 1;
            int value = 0;
            int xMask = 0;
            int xBits = 0;

            int[] xPos = new int[encoding.Length];

            for (int index = 0; index < encoding.Length; index++, bit--)
            {
                char chr = encoding[index];

                if (chr == '1')
                {
                    value |= 1 << bit;
                }
                else if (chr == 'x')
                {
                    xMask |= 1 << bit;

                    xPos[xBits++] = bit;
                }
            }

            xMask = ~xMask;

            ShaderDecodeEntry entry = new ShaderDecodeEntry(func, xBits);

            for (int index = 0; index < (1 << xBits); index++)
            {
                value &= xMask;

                for (int x = 0; x < xBits; x++)
                {
                    value |= ((index >> x) & 1) << xPos[x];
                }

                if (_opCodes[value] == null || _opCodes[value].XBits > xBits)
                {
                    _opCodes[value] = entry;
                }
            }
        }

        public static ShaderDecodeFunc GetDecoder(long opCode)
        {
            return _opCodes[(ulong)opCode >> (64 - EncodingBits)]?.Func;
        }
    }
}