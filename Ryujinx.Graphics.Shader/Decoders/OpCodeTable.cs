using Ryujinx.Graphics.Shader.Instructions;
using System;

namespace Ryujinx.Graphics.Shader.Decoders
{
    static class OpCodeTable
    {
        private const int EncodingBits = 14;

        private class TableEntry
        {
            public InstEmitter Emitter { get; }

            public Type OpCodeType { get; }

            public int XBits { get; }

            public TableEntry(InstEmitter emitter, Type opCodeType, int xBits)
            {
                Emitter    = emitter;
                OpCodeType = opCodeType;
                XBits      = xBits;
            }
        }

        private static TableEntry[] _opCodes;

        static OpCodeTable()
        {
            _opCodes = new TableEntry[1 << EncodingBits];

#region Instructions
            Set("1110111111011x", InstEmit.Ald,     typeof(OpCodeAttribute));
            Set("1110111111110x", InstEmit.Ast,     typeof(OpCodeAttribute));
            Set("11101100xxxxxx", InstEmit.Atoms,   typeof(OpCodeAtom));
            Set("0100110000000x", InstEmit.Bfe,     typeof(OpCodeAluCbuf));
            Set("0011100x00000x", InstEmit.Bfe,     typeof(OpCodeAluImm));
            Set("0101110000000x", InstEmit.Bfe,     typeof(OpCodeAluReg));
            Set("0100101111110x", InstEmit.Bfi,     typeof(OpCodeAluCbuf));
            Set("0011011x11110x", InstEmit.Bfi,     typeof(OpCodeAluImm));
            Set("0101001111110x", InstEmit.Bfi,     typeof(OpCodeAluRegCbuf));
            Set("0101101111110x", InstEmit.Bfi,     typeof(OpCodeAluReg));
            Set("111000100100xx", InstEmit.Bra,     typeof(OpCodeBranch));
            Set("111000110100xx", InstEmit.Brk,     typeof(OpCodeBranchPop));
            Set("111000100101xx", InstEmit.Brx,     typeof(OpCodeBranchIndir));
            Set("0101000010100x", InstEmit.Csetp,   typeof(OpCodePsetp));
            Set("111000110000xx", InstEmit.Exit,    typeof(OpCodeExit));
            Set("0100110010101x", InstEmit.F2F,     typeof(OpCodeFArithCbuf));
            Set("0011100x10101x", InstEmit.F2F,     typeof(OpCodeFArithImm));
            Set("0101110010101x", InstEmit.F2F,     typeof(OpCodeFArithReg));
            Set("0100110010110x", InstEmit.F2I,     typeof(OpCodeFArithCbuf));
            Set("0011100x10110x", InstEmit.F2I,     typeof(OpCodeFArithImm));
            Set("0101110010110x", InstEmit.F2I,     typeof(OpCodeFArithReg));
            Set("0100110001011x", InstEmit.Fadd,    typeof(OpCodeFArithCbuf));
            Set("0011100x01011x", InstEmit.Fadd,    typeof(OpCodeFArithImm));
            Set("000010xxxxxxxx", InstEmit.Fadd,    typeof(OpCodeFArithImm32));
            Set("0101110001011x", InstEmit.Fadd,    typeof(OpCodeFArithReg));
            Set("010010011xxxxx", InstEmit.Ffma,    typeof(OpCodeFArithCbuf));
            Set("0011001x1xxxxx", InstEmit.Ffma,    typeof(OpCodeFArithImm));
            Set("010100011xxxxx", InstEmit.Ffma,    typeof(OpCodeFArithRegCbuf));
            Set("010110011xxxxx", InstEmit.Ffma,    typeof(OpCodeFArithReg));
            Set("0100110000110x", InstEmit.Flo,     typeof(OpCodeAluCbuf));
            Set("0011100x00110x", InstEmit.Flo,     typeof(OpCodeAluImm));
            Set("0101110000110x", InstEmit.Flo,     typeof(OpCodeAluReg));
            Set("0100110001100x", InstEmit.Fmnmx,   typeof(OpCodeFArithCbuf));
            Set("0011100x01100x", InstEmit.Fmnmx,   typeof(OpCodeFArithImm));
            Set("0101110001100x", InstEmit.Fmnmx,   typeof(OpCodeFArithReg));
            Set("0100110001101x", InstEmit.Fmul,    typeof(OpCodeFArithCbuf));
            Set("0011100x01101x", InstEmit.Fmul,    typeof(OpCodeFArithImm));
            Set("00011110xxxxxx", InstEmit.Fmul,    typeof(OpCodeFArithImm32));
            Set("0101110001101x", InstEmit.Fmul,    typeof(OpCodeFArithReg));
            Set("0100100xxxxxxx", InstEmit.Fset,    typeof(OpCodeSetCbuf));
            Set("0011000xxxxxxx", InstEmit.Fset,    typeof(OpCodeFsetImm));
            Set("01011000xxxxxx", InstEmit.Fset,    typeof(OpCodeSetReg));
            Set("010010111011xx", InstEmit.Fsetp,   typeof(OpCodeSetCbuf));
            Set("0011011x1011xx", InstEmit.Fsetp,   typeof(OpCodeFsetImm));
            Set("010110111011xx", InstEmit.Fsetp,   typeof(OpCodeSetReg));
            Set("0101000011111x", InstEmit.Fswzadd, typeof(OpCodeAluReg));
            Set("0111101x1xxxxx", InstEmit.Hadd2,   typeof(OpCodeAluCbuf));
            Set("0111101x0xxxxx", InstEmit.Hadd2,   typeof(OpCodeAluImm2x10));
            Set("0010110xxxxxxx", InstEmit.Hadd2,   typeof(OpCodeAluImm32));
            Set("0101110100010x", InstEmit.Hadd2,   typeof(OpCodeAluReg));
            Set("01110xxx1xxxxx", InstEmit.Hfma2,   typeof(OpCodeHfmaCbuf));
            Set("01110xxx0xxxxx", InstEmit.Hfma2,   typeof(OpCodeHfmaImm2x10));
            Set("0010100xxxxxxx", InstEmit.Hfma2,   typeof(OpCodeHfmaImm32));
            Set("0101110100000x", InstEmit.Hfma2,   typeof(OpCodeHfmaReg));
            Set("01100xxx1xxxxx", InstEmit.Hfma2,   typeof(OpCodeHfmaRegCbuf));
            Set("0111100x1xxxxx", InstEmit.Hmul2,   typeof(OpCodeAluCbuf));
            Set("0111100x0xxxxx", InstEmit.Hmul2,   typeof(OpCodeAluImm2x10));
            Set("0010101xxxxxxx", InstEmit.Hmul2,   typeof(OpCodeAluImm32));
            Set("0101110100001x", InstEmit.Hmul2,   typeof(OpCodeAluReg));
            Set("0111111x1xxxxx", InstEmit.Hsetp2,  typeof(OpCodeSetCbuf));
            Set("0111111x0xxxxx", InstEmit.Hsetp2,  typeof(OpCodeHsetImm2x10));
            Set("0101110100100x", InstEmit.Hsetp2,  typeof(OpCodeSetReg));
            Set("0100110010111x", InstEmit.I2F,     typeof(OpCodeAluCbuf));
            Set("0011100x10111x", InstEmit.I2F,     typeof(OpCodeAluImm));
            Set("0101110010111x", InstEmit.I2F,     typeof(OpCodeAluReg));
            Set("0100110011100x", InstEmit.I2I,     typeof(OpCodeAluCbuf));
            Set("0011100x11100x", InstEmit.I2I,     typeof(OpCodeAluImm));
            Set("0101110011100x", InstEmit.I2I,     typeof(OpCodeAluReg));
            Set("0100110000010x", InstEmit.Iadd,    typeof(OpCodeAluCbuf));
            Set("0011100000010x", InstEmit.Iadd,    typeof(OpCodeAluImm));
            Set("0001110x0xxxxx", InstEmit.Iadd,    typeof(OpCodeAluImm32));
            Set("0101110000010x", InstEmit.Iadd,    typeof(OpCodeAluReg));
            Set("010011001100xx", InstEmit.Iadd3,   typeof(OpCodeAluCbuf));
            Set("001110001100xx", InstEmit.Iadd3,   typeof(OpCodeAluImm));
            Set("010111001100xx", InstEmit.Iadd3,   typeof(OpCodeAluReg));
            Set("0100110000100x", InstEmit.Imnmx,   typeof(OpCodeAluCbuf));
            Set("0011100x00100x", InstEmit.Imnmx,   typeof(OpCodeAluImm));
            Set("0101110000100x", InstEmit.Imnmx,   typeof(OpCodeAluReg));
            Set("11100000xxxxxx", InstEmit.Ipa,     typeof(OpCodeIpa));
            Set("1110111111010x", InstEmit.Isberd,  typeof(OpCodeAlu));
            Set("0100110000011x", InstEmit.Iscadd,  typeof(OpCodeAluCbuf));
            Set("0011100x00011x", InstEmit.Iscadd,  typeof(OpCodeAluImm));
            Set("000101xxxxxxxx", InstEmit.Iscadd,  typeof(OpCodeAluImm32));
            Set("0101110000011x", InstEmit.Iscadd,  typeof(OpCodeAluReg));
            Set("010010110101xx", InstEmit.Iset,    typeof(OpCodeSetCbuf));
            Set("001101100101xx", InstEmit.Iset,    typeof(OpCodeSetImm));
            Set("010110110101xx", InstEmit.Iset,    typeof(OpCodeSetReg));
            Set("010010110110xx", InstEmit.Isetp,   typeof(OpCodeSetCbuf));
            Set("0011011x0110xx", InstEmit.Isetp,   typeof(OpCodeSetImm));
            Set("010110110110xx", InstEmit.Isetp,   typeof(OpCodeSetReg));
            Set("111000110011xx", InstEmit.Kil,     typeof(OpCodeExit));
            Set("1110111101000x", InstEmit.Ld,      typeof(OpCodeMemory));
            Set("1110111110010x", InstEmit.Ldc,     typeof(OpCodeLdc));
            Set("1110111011010x", InstEmit.Ldg,     typeof(OpCodeMemory));
            Set("1110111101001x", InstEmit.Lds,     typeof(OpCodeMemory));
            Set("0100110001000x", InstEmit.Lop,     typeof(OpCodeLopCbuf));
            Set("0011100001000x", InstEmit.Lop,     typeof(OpCodeLopImm));
            Set("000001xxxxxxxx", InstEmit.Lop,     typeof(OpCodeLopImm32));
            Set("0101110001000x", InstEmit.Lop,     typeof(OpCodeLopReg));
            Set("0010000xxxxxxx", InstEmit.Lop3,    typeof(OpCodeLopCbuf));
            Set("001111xxxxxxxx", InstEmit.Lop3,    typeof(OpCodeLopImm));
            Set("0101101111100x", InstEmit.Lop3,    typeof(OpCodeLopReg));
            Set("0100110010011x", InstEmit.Mov,     typeof(OpCodeAluCbuf));
            Set("0011100x10011x", InstEmit.Mov,     typeof(OpCodeAluImm));
            Set("000000010000xx", InstEmit.Mov,     typeof(OpCodeAluImm32));
            Set("0101110010011x", InstEmit.Mov,     typeof(OpCodeAluReg));
            Set("0101000010000x", InstEmit.Mufu,    typeof(OpCodeFArith));
            Set("1111101111100x", InstEmit.Out,     typeof(OpCode));
            Set("111000101010xx", InstEmit.Pbk,     typeof(OpCodePush));
            Set("0100110000001x", InstEmit.Popc,    typeof(OpCodeAluCbuf));
            Set("0011100x00001x", InstEmit.Popc,    typeof(OpCodeAluImm));
            Set("0101110000001x", InstEmit.Popc,    typeof(OpCodeAluReg));
            Set("0101000010010x", InstEmit.Psetp,   typeof(OpCodePsetp));
            Set("0100110011110x", InstEmit.R2p,     typeof(OpCodeAluCbuf));
            Set("0011100x11110x", InstEmit.R2p,     typeof(OpCodeAluImm));
            Set("0101110011110x", InstEmit.R2p,     typeof(OpCodeAluReg));
            Set("1110101111111x", InstEmit.Red,     typeof(OpCodeRed));
            Set("0100110010010x", InstEmit.Rro,     typeof(OpCodeFArithCbuf));
            Set("0011100x10010x", InstEmit.Rro,     typeof(OpCodeFArithImm));
            Set("0101110010010x", InstEmit.Rro,     typeof(OpCodeFArithReg));
            Set("1111000011001x", InstEmit.S2r,     typeof(OpCodeAlu));
            Set("0100110010100x", InstEmit.Sel,     typeof(OpCodeAluCbuf));
            Set("0011100x10100x", InstEmit.Sel,     typeof(OpCodeAluImm));
            Set("0101110010100x", InstEmit.Sel,     typeof(OpCodeAluReg));
            Set("1110111100010x", InstEmit.Shfl,    typeof(OpCodeShuffle));
            Set("0100110001001x", InstEmit.Shl,     typeof(OpCodeAluCbuf));
            Set("0011100x01001x", InstEmit.Shl,     typeof(OpCodeAluImm));
            Set("0101110001001x", InstEmit.Shl,     typeof(OpCodeAluReg));
            Set("0100110000101x", InstEmit.Shr,     typeof(OpCodeAluCbuf));
            Set("0011100x00101x", InstEmit.Shr,     typeof(OpCodeAluImm));
            Set("0101110000101x", InstEmit.Shr,     typeof(OpCodeAluReg));
            Set("111000101001xx", InstEmit.Ssy,     typeof(OpCodePush));
            Set("1110111101010x", InstEmit.St,      typeof(OpCodeMemory));
            Set("1110111011011x", InstEmit.Stg,     typeof(OpCodeMemory));
            Set("1110111101011x", InstEmit.Sts,     typeof(OpCodeMemory));
            Set("11101011001xxx", InstEmit.Sust,    typeof(OpCodeImage));
            Set("1111000011111x", InstEmit.Sync,    typeof(OpCodeBranchPop));
            Set("110000xxxx111x", InstEmit.Tex,     typeof(OpCodeTex));
            Set("1101111010111x", InstEmit.TexB,    typeof(OpCodeTexB));
            Set("1101x00xxxxxxx", InstEmit.Texs,    typeof(OpCodeTexs));
            Set("1101x01xxxxxxx", InstEmit.Texs,    typeof(OpCodeTlds));
            Set("11011111x0xxxx", InstEmit.Texs,    typeof(OpCodeTld4s));
            Set("11011100xx111x", InstEmit.Tld,     typeof(OpCodeTld));
            Set("11011101xx111x", InstEmit.TldB,    typeof(OpCodeTld));
            Set("110010xxxx111x", InstEmit.Tld4,    typeof(OpCodeTld4));
            Set("110111100x1110", InstEmit.Txd,     typeof(OpCodeTxd));
            Set("1101111101001x", InstEmit.Txq,     typeof(OpCodeTex));
            Set("1101111101010x", InstEmit.TxqB,    typeof(OpCodeTex));
            Set("01011111xxxxxx", InstEmit.Vmad,    typeof(OpCodeVideo));
            Set("0101000011011x", InstEmit.Vote,    typeof(OpCodeVote));
            Set("0100111xxxxxxx", InstEmit.Xmad,    typeof(OpCodeAluCbuf));
            Set("0011011x00xxxx", InstEmit.Xmad,    typeof(OpCodeAluImm));
            Set("010100010xxxxx", InstEmit.Xmad,    typeof(OpCodeAluRegCbuf));
            Set("0101101100xxxx", InstEmit.Xmad,    typeof(OpCodeAluReg));
#endregion
        }

        private static void Set(string encoding, InstEmitter emitter, Type opCodeType)
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

            TableEntry entry = new TableEntry(emitter, opCodeType, xBits);

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

        public static (InstEmitter emitter, Type opCodeType) GetEmitter(long opCode)
        {
            TableEntry entry = _opCodes[(ulong)opCode >> (64 - EncodingBits)];

            if (entry != null)
            {
                return (entry.Emitter, entry.OpCodeType);
            }

            return (null, null);
        }
    }
}