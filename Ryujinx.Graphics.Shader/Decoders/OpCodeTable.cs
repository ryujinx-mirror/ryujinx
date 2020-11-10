using Ryujinx.Graphics.Shader.Instructions;
using System;

namespace Ryujinx.Graphics.Shader.Decoders
{
    static class OpCodeTable
    {
        public delegate OpCode MakeOp(InstEmitter emitter, ulong address, long opCode);

        private const int EncodingBits = 14;

        private class TableEntry
        {
            public InstEmitter Emitter { get; }

            public MakeOp MakeOp { get; }

            public int XBits { get; }

            public TableEntry(InstEmitter emitter, MakeOp makeOp, int xBits)
            {
                Emitter = emitter;
                MakeOp  = makeOp;
                XBits   = xBits;
            }
        }

        private static TableEntry[] _opCodes;

        static OpCodeTable()
        {
            _opCodes = new TableEntry[1 << EncodingBits];

#region Instructions
            Set("1110111111011x", InstEmit.Ald,     OpCodeAttribute.Create);
            Set("1110111111110x", InstEmit.Ast,     OpCodeAttribute.Create);
            Set("11101101xxxxxx", InstEmit.Atom,    OpCodeAtom.Create);
            Set("11101100xxxxxx", InstEmit.Atoms,   OpCodeAtom.Create);
            Set("1111000010101x", InstEmit.Bar,     OpCodeBarrier.Create);
            Set("0100110000000x", InstEmit.Bfe,     OpCodeAluCbuf.Create);
            Set("0011100x00000x", InstEmit.Bfe,     OpCodeAluImm.Create);
            Set("0101110000000x", InstEmit.Bfe,     OpCodeAluReg.Create);
            Set("0100101111110x", InstEmit.Bfi,     OpCodeAluCbuf.Create);
            Set("0011011x11110x", InstEmit.Bfi,     OpCodeAluImm.Create);
            Set("0101001111110x", InstEmit.Bfi,     OpCodeAluRegCbuf.Create);
            Set("0101101111110x", InstEmit.Bfi,     OpCodeAluReg.Create);
            Set("111000100100xx", InstEmit.Bra,     OpCodeBranch.Create);
            Set("111000110100xx", InstEmit.Brk,     OpCodeBranchPop.Create);
            Set("111000100101xx", InstEmit.Brx,     OpCodeBranchIndir.Create);
            Set("111000100110xx", InstEmit.Cal,     OpCodeBranch.Create);
            Set("0101000010100x", InstEmit.Csetp,   OpCodePset.Create);
            Set("0100110001110x", InstEmit.Dadd,    OpCodeFArithCbuf.Create);
            Set("0011100x01110x", InstEmit.Dadd,    OpCodeDArithImm.Create);
            Set("0101110001110x", InstEmit.Dadd,    OpCodeFArithReg.Create);
            Set("1111000011110x", InstEmit.Depbar,  OpCode.Create);
            Set("010010110111xx", InstEmit.Dfma,    OpCodeFArithCbuf.Create);
            Set("0011011x0111xx", InstEmit.Dfma,    OpCodeDArithImm.Create);
            Set("010100110111xx", InstEmit.Dfma,    OpCodeFArithRegCbuf.Create);
            Set("010110110111xx", InstEmit.Dfma,    OpCodeFArithReg.Create);
            Set("0100110010000x", InstEmit.Dmul,    OpCodeFArithCbuf.Create);
            Set("0011100x10000x", InstEmit.Dmul,    OpCodeDArithImm.Create);
            Set("0101110010000x", InstEmit.Dmul,    OpCodeFArithReg.Create);
            Set("111000110000xx", InstEmit.Exit,    OpCodeExit.Create);
            Set("0100110010101x", InstEmit.F2F,     OpCodeFArithCbuf.Create);
            Set("0011100x10101x", InstEmit.F2F,     OpCodeFArithImm.Create);
            Set("0101110010101x", InstEmit.F2F,     OpCodeFArithReg.Create);
            Set("0100110010110x", InstEmit.F2I,     OpCodeFArithCbuf.Create);
            Set("0011100x10110x", InstEmit.F2I,     OpCodeFArithImm.Create);
            Set("0101110010110x", InstEmit.F2I,     OpCodeFArithReg.Create);
            Set("0100110001011x", InstEmit.Fadd,    OpCodeFArithCbuf.Create);
            Set("0011100x01011x", InstEmit.Fadd,    OpCodeFArithImm.Create);
            Set("000010xxxxxxxx", InstEmit.Fadd,    OpCodeFArithImm32.Create);
            Set("0101110001011x", InstEmit.Fadd,    OpCodeFArithReg.Create);
            Set("010010111010xx", InstEmit.Fcmp,    OpCodeFArithCbuf.Create);
            Set("0011011x1010xx", InstEmit.Fcmp,    OpCodeFArithImm.Create);
            Set("010110111010xx", InstEmit.Fcmp,    OpCodeFArithReg.Create);
            Set("010100111010xx", InstEmit.Fcmp,    OpCodeFArithRegCbuf.Create);
            Set("010010011xxxxx", InstEmit.Ffma,    OpCodeFArithCbuf.Create);
            Set("0011001x1xxxxx", InstEmit.Ffma,    OpCodeFArithImm.Create);
            Set("000011xxxxxxxx", InstEmit.Ffma32i, OpCodeFArithImm32.Create);
            Set("010100011xxxxx", InstEmit.Ffma,    OpCodeFArithRegCbuf.Create);
            Set("010110011xxxxx", InstEmit.Ffma,    OpCodeFArithReg.Create);
            Set("0100110000110x", InstEmit.Flo,     OpCodeAluCbuf.Create);
            Set("0011100x00110x", InstEmit.Flo,     OpCodeAluImm.Create);
            Set("0101110000110x", InstEmit.Flo,     OpCodeAluReg.Create);
            Set("0100110001100x", InstEmit.Fmnmx,   OpCodeFArithCbuf.Create);
            Set("0011100x01100x", InstEmit.Fmnmx,   OpCodeFArithImm.Create);
            Set("0101110001100x", InstEmit.Fmnmx,   OpCodeFArithReg.Create);
            Set("0100110001101x", InstEmit.Fmul,    OpCodeFArithCbuf.Create);
            Set("0011100x01101x", InstEmit.Fmul,    OpCodeFArithImm.Create);
            Set("00011110xxxxxx", InstEmit.Fmul,    OpCodeFArithImm32.Create);
            Set("0101110001101x", InstEmit.Fmul,    OpCodeFArithReg.Create);
            Set("0100100xxxxxxx", InstEmit.Fset,    OpCodeSetCbuf.Create);
            Set("0011000xxxxxxx", InstEmit.Fset,    OpCodeFsetImm.Create);
            Set("01011000xxxxxx", InstEmit.Fset,    OpCodeSetReg.Create);
            Set("010010111011xx", InstEmit.Fsetp,   OpCodeSetCbuf.Create);
            Set("0011011x1011xx", InstEmit.Fsetp,   OpCodeFsetImm.Create);
            Set("010110111011xx", InstEmit.Fsetp,   OpCodeSetReg.Create);
            Set("0101000011111x", InstEmit.Fswzadd, OpCodeAluReg.Create);
            Set("0111101x1xxxxx", InstEmit.Hadd2,   OpCodeAluCbuf.Create);
            Set("0111101x0xxxxx", InstEmit.Hadd2,   OpCodeAluImm2x10.Create);
            Set("0010110xxxxxxx", InstEmit.Hadd2,   OpCodeAluImm32.Create);
            Set("0101110100010x", InstEmit.Hadd2,   OpCodeAluReg.Create);
            Set("01110xxx1xxxxx", InstEmit.Hfma2,   OpCodeHfmaCbuf.Create);
            Set("01110xxx0xxxxx", InstEmit.Hfma2,   OpCodeHfmaImm2x10.Create);
            Set("0010100xxxxxxx", InstEmit.Hfma2,   OpCodeHfmaImm32.Create);
            Set("0101110100000x", InstEmit.Hfma2,   OpCodeHfmaReg.Create);
            Set("01100xxx1xxxxx", InstEmit.Hfma2,   OpCodeHfmaRegCbuf.Create);
            Set("0111100x1xxxxx", InstEmit.Hmul2,   OpCodeAluCbuf.Create);
            Set("0111100x0xxxxx", InstEmit.Hmul2,   OpCodeAluImm2x10.Create);
            Set("0010101xxxxxxx", InstEmit.Hmul2,   OpCodeAluImm32.Create);
            Set("0101110100001x", InstEmit.Hmul2,   OpCodeAluReg.Create);
            Set("0111110x1xxxxx", InstEmit.Hset2,   OpCodeSetCbuf.Create);
            Set("0111110x0xxxxx", InstEmit.Hset2,   OpCodeHsetImm2x10.Create);
            Set("0101110100011x", InstEmit.Hset2,   OpCodeSetReg.Create);
            Set("0111111x1xxxxx", InstEmit.Hsetp2,  OpCodeSetCbuf.Create);
            Set("0111111x0xxxxx", InstEmit.Hsetp2,  OpCodeHsetImm2x10.Create);
            Set("0101110100100x", InstEmit.Hsetp2,  OpCodeSetReg.Create);
            Set("0100110010111x", InstEmit.I2F,     OpCodeAluCbuf.Create);
            Set("0011100x10111x", InstEmit.I2F,     OpCodeAluImm.Create);
            Set("0101110010111x", InstEmit.I2F,     OpCodeAluReg.Create);
            Set("0100110011100x", InstEmit.I2I,     OpCodeAluCbuf.Create);
            Set("0011100x11100x", InstEmit.I2I,     OpCodeAluImm.Create);
            Set("0101110011100x", InstEmit.I2I,     OpCodeAluReg.Create);
            Set("0100110000010x", InstEmit.Iadd,    OpCodeAluCbuf.Create);
            Set("0011100x00010x", InstEmit.Iadd,    OpCodeAluImm.Create);
            Set("0001110x0xxxxx", InstEmit.Iadd,    OpCodeAluImm32.Create);
            Set("0101110000010x", InstEmit.Iadd,    OpCodeAluReg.Create);
            Set("010011001100xx", InstEmit.Iadd3,   OpCodeAluCbuf.Create);
            Set("0011100x1100xx", InstEmit.Iadd3,   OpCodeAluImm.Create);
            Set("010111001100xx", InstEmit.Iadd3,   OpCodeAluReg.Create);
            Set("010010110100xx", InstEmit.Icmp,    OpCodeAluCbuf.Create);
            Set("0011011x0100xx", InstEmit.Icmp,    OpCodeAluImm.Create);
            Set("010110110100xx", InstEmit.Icmp,    OpCodeAluReg.Create);
            Set("010100110100xx", InstEmit.Icmp,    OpCodeAluRegCbuf.Create);
            Set("010010100xxxxx", InstEmit.Imad,    OpCodeAluCbuf.Create);
            Set("0011010x0xxxxx", InstEmit.Imad,    OpCodeAluImm.Create);
            Set("010110100xxxxx", InstEmit.Imad,    OpCodeAluReg.Create);
            Set("010100100xxxxx", InstEmit.Imad,    OpCodeAluRegCbuf.Create);
            Set("0100110000100x", InstEmit.Imnmx,   OpCodeAluCbuf.Create);
            Set("0011100x00100x", InstEmit.Imnmx,   OpCodeAluImm.Create);
            Set("0101110000100x", InstEmit.Imnmx,   OpCodeAluReg.Create);
            Set("11100000xxxxxx", InstEmit.Ipa,     OpCodeIpa.Create);
            Set("1110111111010x", InstEmit.Isberd,  OpCodeAlu.Create);
            Set("0100110000011x", InstEmit.Iscadd,  OpCodeAluCbuf.Create);
            Set("0011100x00011x", InstEmit.Iscadd,  OpCodeAluImm.Create);
            Set("000101xxxxxxxx", InstEmit.Iscadd,  OpCodeAluImm32.Create);
            Set("0101110000011x", InstEmit.Iscadd,  OpCodeAluReg.Create);
            Set("010010110101xx", InstEmit.Iset,    OpCodeSetCbuf.Create);
            Set("0011011x0101xx", InstEmit.Iset,    OpCodeSetImm.Create);
            Set("010110110101xx", InstEmit.Iset,    OpCodeSetReg.Create);
            Set("010010110110xx", InstEmit.Isetp,   OpCodeSetCbuf.Create);
            Set("0011011x0110xx", InstEmit.Isetp,   OpCodeSetImm.Create);
            Set("010110110110xx", InstEmit.Isetp,   OpCodeSetReg.Create);
            Set("111000110011xx", InstEmit.Kil,     OpCodeExit.Create);
            Set("1110111101000x", InstEmit.Ld,      OpCodeMemory.Create);
            Set("1110111110010x", InstEmit.Ldc,     OpCodeLdc.Create);
            Set("1110111011010x", InstEmit.Ldg,     OpCodeMemory.Create);
            Set("1110111101001x", InstEmit.Lds,     OpCodeMemory.Create);
            Set("010010111101xx", InstEmit.Lea,     OpCodeAluCbuf.Create);
            Set("0011011x11010x", InstEmit.Lea,     OpCodeAluImm.Create);
            Set("0101101111010x", InstEmit.Lea,     OpCodeAluReg.Create);
            Set("000110xxxxxxxx", InstEmit.Lea_Hi,  OpCodeAluCbuf.Create);
            Set("0101101111011x", InstEmit.Lea_Hi,  OpCodeAluReg.Create);
            Set("0100110001000x", InstEmit.Lop,     OpCodeLopCbuf.Create);
            Set("0011100001000x", InstEmit.Lop,     OpCodeLopImm.Create);
            Set("000001xxxxxxxx", InstEmit.Lop,     OpCodeLopImm32.Create);
            Set("0101110001000x", InstEmit.Lop,     OpCodeLopReg.Create);
            Set("0000001xxxxxxx", InstEmit.Lop3,    OpCodeLopCbuf.Create);
            Set("001111xxxxxxxx", InstEmit.Lop3,    OpCodeLopImm.Create);
            Set("0101101111100x", InstEmit.Lop3,    OpCodeLopReg.Create);
            Set("1110111110011x", InstEmit.Membar,  OpCodeMemoryBarrier.Create);
            Set("0100110010011x", InstEmit.Mov,     OpCodeAluCbuf.Create);
            Set("0011100x10011x", InstEmit.Mov,     OpCodeAluImm.Create);
            Set("000000010000xx", InstEmit.Mov,     OpCodeAluImm32.Create);
            Set("0101110010011x", InstEmit.Mov,     OpCodeAluReg.Create);
            Set("0101000010000x", InstEmit.Mufu,    OpCodeFArith.Create);
            Set("0101000010110x", InstEmit.Nop,     OpCode.Create);
            Set("1111101111100x", InstEmit.Out,     OpCode.Create);
            Set("111000101010xx", InstEmit.Pbk,     OpCodePush.Create);
            Set("0100110000001x", InstEmit.Popc,    OpCodeAluCbuf.Create);
            Set("0011100x00001x", InstEmit.Popc,    OpCodeAluImm.Create);
            Set("0101110000001x", InstEmit.Popc,    OpCodeAluReg.Create);
            Set("0101000010001x", InstEmit.Pset,    OpCodePset.Create);
            Set("0101000010010x", InstEmit.Psetp,   OpCodePset.Create);
            Set("0100110011110x", InstEmit.R2p,     OpCodeAluCbuf.Create);
            Set("0011100x11110x", InstEmit.R2p,     OpCodeAluImm.Create);
            Set("0101110011110x", InstEmit.R2p,     OpCodeAluReg.Create);
            Set("1110101111111x", InstEmit.Red,     OpCodeRed.Create);
            Set("111000110010xx", InstEmit.Ret,     OpCodeExit.Create);
            Set("0100110010010x", InstEmit.Rro,     OpCodeFArithCbuf.Create);
            Set("0011100x10010x", InstEmit.Rro,     OpCodeFArithImm.Create);
            Set("0101110010010x", InstEmit.Rro,     OpCodeFArithReg.Create);
            Set("1111000011001x", InstEmit.S2r,     OpCodeAlu.Create);
            Set("0100110010100x", InstEmit.Sel,     OpCodeAluCbuf.Create);
            Set("0011100x10100x", InstEmit.Sel,     OpCodeAluImm.Create);
            Set("0101110010100x", InstEmit.Sel,     OpCodeAluReg.Create);
            Set("1110111100010x", InstEmit.Shfl,    OpCodeShuffle.Create);
            Set("0100110001001x", InstEmit.Shl,     OpCodeAluCbuf.Create);
            Set("0011100x01001x", InstEmit.Shl,     OpCodeAluImm.Create);
            Set("0101110001001x", InstEmit.Shl,     OpCodeAluReg.Create);
            Set("0100110000101x", InstEmit.Shr,     OpCodeAluCbuf.Create);
            Set("0011100x00101x", InstEmit.Shr,     OpCodeAluImm.Create);
            Set("0101110000101x", InstEmit.Shr,     OpCodeAluReg.Create);
            Set("111000101001xx", InstEmit.Ssy,     OpCodePush.Create);
            Set("1110111101010x", InstEmit.St,      OpCodeMemory.Create);
            Set("1110111011011x", InstEmit.Stg,     OpCodeMemory.Create);
            Set("1110111101011x", InstEmit.Sts,     OpCodeMemory.Create);
            Set("11101011000xxx", InstEmit.Suld,    OpCodeImage.Create);
            Set("11101011001xxx", InstEmit.Sust,    OpCodeImage.Create);
            Set("1111000011111x", InstEmit.Sync,    OpCodeBranchPop.Create);
            Set("110000xxxx111x", InstEmit.Tex,     OpCodeTex.Create);
            Set("1101111010111x", InstEmit.TexB,    OpCodeTexB.Create);
            Set("1101x00xxxxxxx", InstEmit.Texs,    OpCodeTexs.Create);
            Set("1101x01xxxxxxx", InstEmit.Texs,    OpCodeTlds.Create);
            Set("11011111x0xxxx", InstEmit.Texs,    OpCodeTld4s.Create);
            Set("11011100xx111x", InstEmit.Tld,     OpCodeTld.Create);
            Set("11011101xx111x", InstEmit.TldB,    OpCodeTld.Create);
            Set("110010xxxx111x", InstEmit.Tld4,    OpCodeTld4.Create);
            Set("1101111011111x", InstEmit.Tld4,    OpCodeTld4B.Create);
            Set("11011111011000", InstEmit.TmmlB,   OpCodeTexture.Create);
            Set("11011111010110", InstEmit.Tmml,    OpCodeTexture.Create);
            Set("110111100x1110", InstEmit.Txd,     OpCodeTxd.Create);
            Set("1101111101001x", InstEmit.Txq,     OpCodeTex.Create);
            Set("1101111101010x", InstEmit.TxqB,    OpCodeTex.Create);
            Set("01011111xxxxxx", InstEmit.Vmad,    OpCodeVideo.Create);
            Set("0011101xxxxxxx", InstEmit.Vmnmx,   OpCodeVideo.Create);
            Set("0101000011011x", InstEmit.Vote,    OpCodeVote.Create);
            Set("0100111xxxxxxx", InstEmit.Xmad,    OpCodeAluCbuf.Create);
            Set("0011011x00xxxx", InstEmit.Xmad,    OpCodeAluImm.Create);
            Set("010100010xxxxx", InstEmit.Xmad,    OpCodeAluRegCbuf.Create);
            Set("0101101100xxxx", InstEmit.Xmad,    OpCodeAluReg.Create);
#endregion
        }

        private static void Set(string encoding, InstEmitter emitter, MakeOp makeOp)
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

            TableEntry entry = new TableEntry(emitter, makeOp, xBits);

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

        public static (InstEmitter Emitter, MakeOp MakeOp) GetEmitter(long opCode)
        {
            TableEntry entry = _opCodes[(ulong)opCode >> (64 - EncodingBits)];

            if (entry != null)
            {
                return (entry.Emitter, entry.MakeOp);
            }

            return (null, null);
        }
    }
}