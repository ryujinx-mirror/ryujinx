using ChocolArm64.Decoders;
using ChocolArm64.Instructions;
using ChocolArm64.State;
using System;
using System.Collections.Generic;

namespace ChocolArm64
{
    static class OpCodeTable
    {
        private const int FastLookupSize = 0x1000;

        private class InstInfo
        {
            public int Mask;
            public int Value;

            public Inst Inst;

            public InstInfo(int mask, int value, Inst inst)
            {
                Mask  = mask;
                Value = value;
                Inst  = inst;
            }
        }

        private static List<InstInfo> _allInstA32 = new List<InstInfo>();
        private static List<InstInfo> _allInstT32 = new List<InstInfo>();
        private static List<InstInfo> _allInstA64 = new List<InstInfo>();

        private static InstInfo[][] _instA32FastLookup = new InstInfo[FastLookupSize][];
        private static InstInfo[][] _instT32FastLookup = new InstInfo[FastLookupSize][];
        private static InstInfo[][] _instA64FastLookup = new InstInfo[FastLookupSize][];

        static OpCodeTable()
        {
#region "OpCode Table (AArch32)"
            // Integer
            SetA32("<<<<0010100xxxxxxxxxxxxxxxxxxxxx", InstEmit32.Add,           typeof(OpCode32AluImm));
            SetA32("<<<<0000100xxxxxxxxxxxxxxxx0xxxx", InstEmit32.Add,           typeof(OpCode32AluRsImm));
            SetA32("<<<<1010xxxxxxxxxxxxxxxxxxxxxxxx", InstEmit32.B,             typeof(OpCode32BImm));
            SetA32("<<<<1011xxxxxxxxxxxxxxxxxxxxxxxx", InstEmit32.Bl,            typeof(OpCode32BImm));
            SetA32("1111101xxxxxxxxxxxxxxxxxxxxxxxxx", InstEmit32.Blx,           typeof(OpCode32BImm));
            SetA32("<<<<000100101111111111110001xxxx", InstEmit32.Bx,            typeof(OpCode32BReg));
            SetT32("010001110xxxx000",                 InstEmit32.Bx,            typeof(OpCodeT16BReg));
            SetA32("<<<<00110101xxxx0000xxxxxxxxxxxx", InstEmit32.Cmp,           typeof(OpCode32AluImm));
            SetA32("<<<<00010101xxxx0000xxxxxxx0xxxx", InstEmit32.Cmp,           typeof(OpCode32AluRsImm));
            SetA32("<<<<100xx0x1xxxxxxxxxxxxxxxxxxxx", InstEmit32.Ldm,           typeof(OpCode32MemMult));
            SetA32("<<<<010xx0x1xxxxxxxxxxxxxxxxxxxx", InstEmit32.Ldr,           typeof(OpCode32MemImm));
            SetA32("<<<<010xx1x1xxxxxxxxxxxxxxxxxxxx", InstEmit32.Ldrb,          typeof(OpCode32MemImm));
            SetA32("<<<<000xx1x0xxxxxxxxxxxx1101xxxx", InstEmit32.Ldrd,          typeof(OpCode32MemImm8));
            SetA32("<<<<000xx1x1xxxxxxxxxxxx1011xxxx", InstEmit32.Ldrh,          typeof(OpCode32MemImm8));
            SetA32("<<<<000xx1x1xxxxxxxxxxxx1101xxxx", InstEmit32.Ldrsb,         typeof(OpCode32MemImm8));
            SetA32("<<<<000xx1x1xxxxxxxxxxxx1111xxxx", InstEmit32.Ldrsh,         typeof(OpCode32MemImm8));
            SetA32("<<<<0011101x0000xxxxxxxxxxxxxxxx", InstEmit32.Mov,           typeof(OpCode32AluImm));
            SetA32("<<<<0001101x0000xxxxxxxxxxx0xxxx", InstEmit32.Mov,           typeof(OpCode32AluRsImm));
            SetT32("00100xxxxxxxxxxx",                 InstEmit32.Mov,           typeof(OpCodeT16AluImm8));
            SetA32("<<<<100xx0x0xxxxxxxxxxxxxxxxxxxx", InstEmit32.Stm,           typeof(OpCode32MemMult));
            SetA32("<<<<010xx0x0xxxxxxxxxxxxxxxxxxxx", InstEmit32.Str,           typeof(OpCode32MemImm));
            SetA32("<<<<010xx1x0xxxxxxxxxxxxxxxxxxxx", InstEmit32.Strb,          typeof(OpCode32MemImm));
            SetA32("<<<<000xx1x0xxxxxxxxxxxx1111xxxx", InstEmit32.Strd,          typeof(OpCode32MemImm8));
            SetA32("<<<<000xx1x0xxxxxxxxxxxx1011xxxx", InstEmit32.Strh,          typeof(OpCode32MemImm8));
            SetA32("<<<<0010010xxxxxxxxxxxxxxxxxxxxx", InstEmit32.Sub,           typeof(OpCode32AluImm));
            SetA32("<<<<0000010xxxxxxxxxxxxxxxx0xxxx", InstEmit32.Sub,           typeof(OpCode32AluRsImm));
#endregion

#region "OpCode Table (AArch64)"
            // Integer
            SetA64("x0011010000xxxxx000000xxxxxxxxxx", InstEmit.Adc,             typeof(OpCodeAluRs64));
            SetA64("x0111010000xxxxx000000xxxxxxxxxx", InstEmit.Adcs,            typeof(OpCodeAluRs64));
            SetA64("x00100010xxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Add,             typeof(OpCodeAluImm64));
            SetA64("00001011<<0xxxxx0xxxxxxxxxxxxxxx", InstEmit.Add,             typeof(OpCodeAluRs64));
            SetA64("10001011<<0xxxxxxxxxxxxxxxxxxxxx", InstEmit.Add,             typeof(OpCodeAluRs64));
            SetA64("x0001011001xxxxxxxx0xxxxxxxxxxxx", InstEmit.Add,             typeof(OpCodeAluRx64));
            SetA64("x0001011001xxxxxxxx100xxxxxxxxxx", InstEmit.Add,             typeof(OpCodeAluRx64));
            SetA64("x01100010xxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Adds,            typeof(OpCodeAluImm64));
            SetA64("00101011<<0xxxxx0xxxxxxxxxxxxxxx", InstEmit.Adds,            typeof(OpCodeAluRs64));
            SetA64("10101011<<0xxxxxxxxxxxxxxxxxxxxx", InstEmit.Adds,            typeof(OpCodeAluRs64));
            SetA64("x0101011001xxxxxxxx0xxxxxxxxxxxx", InstEmit.Adds,            typeof(OpCodeAluRx64));
            SetA64("x0101011001xxxxxxxx100xxxxxxxxxx", InstEmit.Adds,            typeof(OpCodeAluRx64));
            SetA64("0xx10000xxxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Adr,             typeof(OpCodeAdr64));
            SetA64("1xx10000xxxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Adrp,            typeof(OpCodeAdr64));
            SetA64("0001001000xxxxxxxxxxxxxxxxxxxxxx", InstEmit.And,             typeof(OpCodeAluImm64));
            SetA64("100100100xxxxxxxxxxxxxxxxxxxxxxx", InstEmit.And,             typeof(OpCodeAluImm64));
            SetA64("00001010xx0xxxxx0xxxxxxxxxxxxxxx", InstEmit.And,             typeof(OpCodeAluRs64));
            SetA64("10001010xx0xxxxxxxxxxxxxxxxxxxxx", InstEmit.And,             typeof(OpCodeAluRs64));
            SetA64("0111001000xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Ands,            typeof(OpCodeAluImm64));
            SetA64("111100100xxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Ands,            typeof(OpCodeAluImm64));
            SetA64("01101010xx0xxxxx0xxxxxxxxxxxxxxx", InstEmit.Ands,            typeof(OpCodeAluRs64));
            SetA64("11101010xx0xxxxxxxxxxxxxxxxxxxxx", InstEmit.Ands,            typeof(OpCodeAluRs64));
            SetA64("x0011010110xxxxx001010xxxxxxxxxx", InstEmit.Asrv,            typeof(OpCodeAluRs64));
            SetA64("000101xxxxxxxxxxxxxxxxxxxxxxxxxx", InstEmit.B,               typeof(OpCodeBImmAl64));
            SetA64("01010100xxxxxxxxxxxxxxxxxxx0xxxx", InstEmit.B_Cond,          typeof(OpCodeBImmCond64));
            SetA64("00110011000xxxxx0xxxxxxxxxxxxxxx", InstEmit.Bfm,             typeof(OpCodeBfm64));
            SetA64("1011001101xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Bfm,             typeof(OpCodeBfm64));
            SetA64("00001010xx1xxxxx0xxxxxxxxxxxxxxx", InstEmit.Bic,             typeof(OpCodeAluRs64));
            SetA64("10001010xx1xxxxxxxxxxxxxxxxxxxxx", InstEmit.Bic,             typeof(OpCodeAluRs64));
            SetA64("01101010xx1xxxxx0xxxxxxxxxxxxxxx", InstEmit.Bics,            typeof(OpCodeAluRs64));
            SetA64("11101010xx1xxxxxxxxxxxxxxxxxxxxx", InstEmit.Bics,            typeof(OpCodeAluRs64));
            SetA64("100101xxxxxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Bl,              typeof(OpCodeBImmAl64));
            SetA64("1101011000111111000000xxxxx00000", InstEmit.Blr,             typeof(OpCodeBReg64));
            SetA64("1101011000011111000000xxxxx00000", InstEmit.Br,              typeof(OpCodeBReg64));
            SetA64("11010100001xxxxxxxxxxxxxxxx00000", InstEmit.Brk,             typeof(OpCodeException64));
            SetA64("x0110101xxxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Cbnz,            typeof(OpCodeBImmCmp64));
            SetA64("x0110100xxxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Cbz,             typeof(OpCodeBImmCmp64));
            SetA64("x0111010010xxxxxxxxx10xxxxx0xxxx", InstEmit.Ccmn,            typeof(OpCodeCcmpImm64));
            SetA64("x0111010010xxxxxxxxx00xxxxx0xxxx", InstEmit.Ccmn,            typeof(OpCodeCcmpReg64));
            SetA64("x1111010010xxxxxxxxx10xxxxx0xxxx", InstEmit.Ccmp,            typeof(OpCodeCcmpImm64));
            SetA64("x1111010010xxxxxxxxx00xxxxx0xxxx", InstEmit.Ccmp,            typeof(OpCodeCcmpReg64));
            SetA64("11010101000000110011xxxx01011111", InstEmit.Clrex,           typeof(OpCodeSystem64));
            SetA64("x101101011000000000101xxxxxxxxxx", InstEmit.Cls,             typeof(OpCodeAlu64));
            SetA64("x101101011000000000100xxxxxxxxxx", InstEmit.Clz,             typeof(OpCodeAlu64));
            SetA64("00011010110xxxxx010000xxxxxxxxxx", InstEmit.Crc32b,          typeof(OpCodeAluRs64));
            SetA64("00011010110xxxxx010001xxxxxxxxxx", InstEmit.Crc32h,          typeof(OpCodeAluRs64));
            SetA64("00011010110xxxxx010010xxxxxxxxxx", InstEmit.Crc32w,          typeof(OpCodeAluRs64));
            SetA64("10011010110xxxxx010011xxxxxxxxxx", InstEmit.Crc32x,          typeof(OpCodeAluRs64));
            SetA64("00011010110xxxxx010100xxxxxxxxxx", InstEmit.Crc32cb,         typeof(OpCodeAluRs64));
            SetA64("00011010110xxxxx010101xxxxxxxxxx", InstEmit.Crc32ch,         typeof(OpCodeAluRs64));
            SetA64("00011010110xxxxx010110xxxxxxxxxx", InstEmit.Crc32cw,         typeof(OpCodeAluRs64));
            SetA64("10011010110xxxxx010111xxxxxxxxxx", InstEmit.Crc32cx,         typeof(OpCodeAluRs64));
            SetA64("x0011010100xxxxxxxxx00xxxxxxxxxx", InstEmit.Csel,            typeof(OpCodeCsel64));
            SetA64("x0011010100xxxxxxxxx01xxxxxxxxxx", InstEmit.Csinc,           typeof(OpCodeCsel64));
            SetA64("x1011010100xxxxxxxxx00xxxxxxxxxx", InstEmit.Csinv,           typeof(OpCodeCsel64));
            SetA64("x1011010100xxxxxxxxx01xxxxxxxxxx", InstEmit.Csneg,           typeof(OpCodeCsel64));
            SetA64("11010101000000110011xxxx10111111", InstEmit.Dmb,             typeof(OpCodeSystem64));
            SetA64("11010101000000110011xxxx10011111", InstEmit.Dsb,             typeof(OpCodeSystem64));
            SetA64("01001010xx1xxxxx0xxxxxxxxxxxxxxx", InstEmit.Eon,             typeof(OpCodeAluRs64));
            SetA64("11001010xx1xxxxxxxxxxxxxxxxxxxxx", InstEmit.Eon,             typeof(OpCodeAluRs64));
            SetA64("0101001000xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Eor,             typeof(OpCodeAluImm64));
            SetA64("110100100xxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Eor,             typeof(OpCodeAluImm64));
            SetA64("01001010xx0xxxxx0xxxxxxxxxxxxxxx", InstEmit.Eor,             typeof(OpCodeAluRs64));
            SetA64("11001010xx0xxxxxxxxxxxxxxxxxxxxx", InstEmit.Eor,             typeof(OpCodeAluRs64));
            SetA64("00010011100xxxxx0xxxxxxxxxxxxxxx", InstEmit.Extr,            typeof(OpCodeAluRs64));
            SetA64("10010011110xxxxxxxxxxxxxxxxxxxxx", InstEmit.Extr,            typeof(OpCodeAluRs64));
            SetA64("11010101000000110010xxxxxxx11111", InstEmit.Hint,            typeof(OpCodeSystem64));
            SetA64("11010101000000110011xxxx11011111", InstEmit.Isb,             typeof(OpCodeSystem64));
            SetA64("xx001000110xxxxx1xxxxxxxxxxxxxxx", InstEmit.Ldar,            typeof(OpCodeMemEx64));
            SetA64("1x001000011xxxxx1xxxxxxxxxxxxxxx", InstEmit.Ldaxp,           typeof(OpCodeMemEx64));
            SetA64("xx001000010xxxxx1xxxxxxxxxxxxxxx", InstEmit.Ldaxr,           typeof(OpCodeMemEx64));
            SetA64("<<10100xx1xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Ldp,             typeof(OpCodeMemPair64));
            SetA64("xx111000010xxxxxxxxxxxxxxxxxxxxx", InstEmit.Ldr,             typeof(OpCodeMemImm64));
            SetA64("xx11100101xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Ldr,             typeof(OpCodeMemImm64));
            SetA64("xx111000011xxxxxxxxx10xxxxxxxxxx", InstEmit.Ldr,             typeof(OpCodeMemReg64));
            SetA64("xx011000xxxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Ldr_Literal,     typeof(OpCodeMemLit64));
            SetA64("0x1110001x0xxxxxxxxxxxxxxxxxxxxx", InstEmit.Ldrs,            typeof(OpCodeMemImm64));
            SetA64("0x1110011xxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Ldrs,            typeof(OpCodeMemImm64));
            SetA64("10111000100xxxxxxxxxxxxxxxxxxxxx", InstEmit.Ldrs,            typeof(OpCodeMemImm64));
            SetA64("1011100110xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Ldrs,            typeof(OpCodeMemImm64));
            SetA64("0x1110001x1xxxxxxxxx10xxxxxxxxxx", InstEmit.Ldrs,            typeof(OpCodeMemReg64));
            SetA64("10111000101xxxxxxxxx10xxxxxxxxxx", InstEmit.Ldrs,            typeof(OpCodeMemReg64));
            SetA64("xx001000010xxxxx0xxxxxxxxxxxxxxx", InstEmit.Ldxr,            typeof(OpCodeMemEx64));
            SetA64("1x001000011xxxxx0xxxxxxxxxxxxxxx", InstEmit.Ldxp,            typeof(OpCodeMemEx64));
            SetA64("x0011010110xxxxx001000xxxxxxxxxx", InstEmit.Lslv,            typeof(OpCodeAluRs64));
            SetA64("x0011010110xxxxx001001xxxxxxxxxx", InstEmit.Lsrv,            typeof(OpCodeAluRs64));
            SetA64("x0011011000xxxxx0xxxxxxxxxxxxxxx", InstEmit.Madd,            typeof(OpCodeMul64));
            SetA64("0111001010xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Movk,            typeof(OpCodeMov64));
            SetA64("111100101xxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Movk,            typeof(OpCodeMov64));
            SetA64("0001001010xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Movn,            typeof(OpCodeMov64));
            SetA64("100100101xxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Movn,            typeof(OpCodeMov64));
            SetA64("0101001010xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Movz,            typeof(OpCodeMov64));
            SetA64("110100101xxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Movz,            typeof(OpCodeMov64));
            SetA64("110101010011xxxxxxxxxxxxxxxxxxxx", InstEmit.Mrs,             typeof(OpCodeSystem64));
            SetA64("110101010001xxxxxxxxxxxxxxxxxxxx", InstEmit.Msr,             typeof(OpCodeSystem64));
            SetA64("x0011011000xxxxx1xxxxxxxxxxxxxxx", InstEmit.Msub,            typeof(OpCodeMul64));
            SetA64("11010101000000110010000000011111", InstEmit.Nop,             typeof(OpCodeSystem64));
            SetA64("00101010xx1xxxxx0xxxxxxxxxxxxxxx", InstEmit.Orn,             typeof(OpCodeAluRs64));
            SetA64("10101010xx1xxxxxxxxxxxxxxxxxxxxx", InstEmit.Orn,             typeof(OpCodeAluRs64));
            SetA64("0011001000xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Orr,             typeof(OpCodeAluImm64));
            SetA64("101100100xxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Orr,             typeof(OpCodeAluImm64));
            SetA64("00101010xx0xxxxx0xxxxxxxxxxxxxxx", InstEmit.Orr,             typeof(OpCodeAluRs64));
            SetA64("10101010xx0xxxxxxxxxxxxxxxxxxxxx", InstEmit.Orr,             typeof(OpCodeAluRs64));
            SetA64("1111100110xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Pfrm,            typeof(OpCodeMemImm64));
            SetA64("11111000100xxxxxxxxx00xxxxxxxxxx", InstEmit.Pfrm,            typeof(OpCodeMemImm64));
            SetA64("11011000xxxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Pfrm,            typeof(OpCodeMemLit64));
            SetA64("x101101011000000000000xxxxxxxxxx", InstEmit.Rbit,            typeof(OpCodeAlu64));
            SetA64("1101011001011111000000xxxxx00000", InstEmit.Ret,             typeof(OpCodeBReg64));
            SetA64("x101101011000000000001xxxxxxxxxx", InstEmit.Rev16,           typeof(OpCodeAlu64));
            SetA64("x101101011000000000010xxxxxxxxxx", InstEmit.Rev32,           typeof(OpCodeAlu64));
            SetA64("1101101011000000000011xxxxxxxxxx", InstEmit.Rev64,           typeof(OpCodeAlu64));
            SetA64("x0011010110xxxxx001011xxxxxxxxxx", InstEmit.Rorv,            typeof(OpCodeAluRs64));
            SetA64("x1011010000xxxxx000000xxxxxxxxxx", InstEmit.Sbc,             typeof(OpCodeAluRs64));
            SetA64("x1111010000xxxxx000000xxxxxxxxxx", InstEmit.Sbcs,            typeof(OpCodeAluRs64));
            SetA64("00010011000xxxxx0xxxxxxxxxxxxxxx", InstEmit.Sbfm,            typeof(OpCodeBfm64));
            SetA64("1001001101xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Sbfm,            typeof(OpCodeBfm64));
            SetA64("x0011010110xxxxx000011xxxxxxxxxx", InstEmit.Sdiv,            typeof(OpCodeAluRs64));
            SetA64("10011011001xxxxx0xxxxxxxxxxxxxxx", InstEmit.Smaddl,          typeof(OpCodeMul64));
            SetA64("10011011001xxxxx1xxxxxxxxxxxxxxx", InstEmit.Smsubl,          typeof(OpCodeMul64));
            SetA64("10011011010xxxxx0xxxxxxxxxxxxxxx", InstEmit.Smulh,           typeof(OpCodeMul64));
            SetA64("xx001000100xxxxx1xxxxxxxxxxxxxxx", InstEmit.Stlr,            typeof(OpCodeMemEx64));
            SetA64("1x001000001xxxxx1xxxxxxxxxxxxxxx", InstEmit.Stlxp,           typeof(OpCodeMemEx64));
            SetA64("xx001000000xxxxx1xxxxxxxxxxxxxxx", InstEmit.Stlxr,           typeof(OpCodeMemEx64));
            SetA64("x010100xx0xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Stp,             typeof(OpCodeMemPair64));
            SetA64("xx111000000xxxxxxxxxxxxxxxxxxxxx", InstEmit.Str,             typeof(OpCodeMemImm64));
            SetA64("xx11100100xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Str,             typeof(OpCodeMemImm64));
            SetA64("xx111000001xxxxxxxxx10xxxxxxxxxx", InstEmit.Str,             typeof(OpCodeMemReg64));
            SetA64("1x001000001xxxxx0xxxxxxxxxxxxxxx", InstEmit.Stxp,            typeof(OpCodeMemEx64));
            SetA64("xx001000000xxxxx0xxxxxxxxxxxxxxx", InstEmit.Stxr,            typeof(OpCodeMemEx64));
            SetA64("x10100010xxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Sub,             typeof(OpCodeAluImm64));
            SetA64("01001011<<0xxxxx0xxxxxxxxxxxxxxx", InstEmit.Sub,             typeof(OpCodeAluRs64));
            SetA64("11001011<<0xxxxxxxxxxxxxxxxxxxxx", InstEmit.Sub,             typeof(OpCodeAluRs64));
            SetA64("x1001011001xxxxxxxx0xxxxxxxxxxxx", InstEmit.Sub,             typeof(OpCodeAluRx64));
            SetA64("x1001011001xxxxxxxx100xxxxxxxxxx", InstEmit.Sub,             typeof(OpCodeAluRx64));
            SetA64("x11100010xxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Subs,            typeof(OpCodeAluImm64));
            SetA64("01101011<<0xxxxx0xxxxxxxxxxxxxxx", InstEmit.Subs,            typeof(OpCodeAluRs64));
            SetA64("11101011<<0xxxxxxxxxxxxxxxxxxxxx", InstEmit.Subs,            typeof(OpCodeAluRs64));
            SetA64("x1101011001xxxxxxxx0xxxxxxxxxxxx", InstEmit.Subs,            typeof(OpCodeAluRx64));
            SetA64("x1101011001xxxxxxxx100xxxxxxxxxx", InstEmit.Subs,            typeof(OpCodeAluRx64));
            SetA64("11010100000xxxxxxxxxxxxxxxx00001", InstEmit.Svc,             typeof(OpCodeException64));
            SetA64("1101010100001xxxxxxxxxxxxxxxxxxx", InstEmit.Sys,             typeof(OpCodeSystem64));
            SetA64("x0110111xxxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Tbnz,            typeof(OpCodeBImmTest64));
            SetA64("x0110110xxxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Tbz,             typeof(OpCodeBImmTest64));
            SetA64("01010011000xxxxx0xxxxxxxxxxxxxxx", InstEmit.Ubfm,            typeof(OpCodeBfm64));
            SetA64("1101001101xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Ubfm,            typeof(OpCodeBfm64));
            SetA64("x0011010110xxxxx000010xxxxxxxxxx", InstEmit.Udiv,            typeof(OpCodeAluRs64));
            SetA64("10011011101xxxxx0xxxxxxxxxxxxxxx", InstEmit.Umaddl,          typeof(OpCodeMul64));
            SetA64("10011011101xxxxx1xxxxxxxxxxxxxxx", InstEmit.Umsubl,          typeof(OpCodeMul64));
            SetA64("10011011110xxxxx0xxxxxxxxxxxxxxx", InstEmit.Umulh,           typeof(OpCodeMul64));

            // Vector
            SetA64("0101111011100000101110xxxxxxxxxx", InstEmit.Abs_S,           typeof(OpCodeSimd64));
            SetA64("0>001110<<100000101110xxxxxxxxxx", InstEmit.Abs_V,           typeof(OpCodeSimd64));
            SetA64("01011110111xxxxx100001xxxxxxxxxx", InstEmit.Add_S,           typeof(OpCodeSimdReg64));
            SetA64("0>001110<<1xxxxx100001xxxxxxxxxx", InstEmit.Add_V,           typeof(OpCodeSimdReg64));
            SetA64("0x001110<<1xxxxx010000xxxxxxxxxx", InstEmit.Addhn_V,         typeof(OpCodeSimdReg64));
            SetA64("0101111011110001101110xxxxxxxxxx", InstEmit.Addp_S,          typeof(OpCodeSimd64));
            SetA64("0>001110<<1xxxxx101111xxxxxxxxxx", InstEmit.Addp_V,          typeof(OpCodeSimdReg64));
            SetA64("000011100x110001101110xxxxxxxxxx", InstEmit.Addv_V,          typeof(OpCodeSimd64));
            SetA64("01001110<<110001101110xxxxxxxxxx", InstEmit.Addv_V,          typeof(OpCodeSimd64));
            SetA64("0100111000101000010110xxxxxxxxxx", InstEmit.Aesd_V,          typeof(OpCodeSimd64));
            SetA64("0100111000101000010010xxxxxxxxxx", InstEmit.Aese_V,          typeof(OpCodeSimd64));
            SetA64("0100111000101000011110xxxxxxxxxx", InstEmit.Aesimc_V,        typeof(OpCodeSimd64));
            SetA64("0100111000101000011010xxxxxxxxxx", InstEmit.Aesmc_V,         typeof(OpCodeSimd64));
            SetA64("0x001110001xxxxx000111xxxxxxxxxx", InstEmit.And_V,           typeof(OpCodeSimdReg64));
            SetA64("0x001110011xxxxx000111xxxxxxxxxx", InstEmit.Bic_V,           typeof(OpCodeSimdReg64));
            SetA64("0x10111100000xxx0xx101xxxxxxxxxx", InstEmit.Bic_Vi,          typeof(OpCodeSimdImm64));
            SetA64("0x10111100000xxx10x101xxxxxxxxxx", InstEmit.Bic_Vi,          typeof(OpCodeSimdImm64));
            SetA64("0x101110111xxxxx000111xxxxxxxxxx", InstEmit.Bif_V,           typeof(OpCodeSimdReg64));
            SetA64("0x101110101xxxxx000111xxxxxxxxxx", InstEmit.Bit_V,           typeof(OpCodeSimdReg64));
            SetA64("0x101110011xxxxx000111xxxxxxxxxx", InstEmit.Bsl_V,           typeof(OpCodeSimdReg64));
            SetA64("0x001110<<100000010010xxxxxxxxxx", InstEmit.Cls_V,           typeof(OpCodeSimd64));
            SetA64("0x101110<<100000010010xxxxxxxxxx", InstEmit.Clz_V,           typeof(OpCodeSimd64));
            SetA64("01111110111xxxxx100011xxxxxxxxxx", InstEmit.Cmeq_S,          typeof(OpCodeSimdReg64));
            SetA64("0101111011100000100110xxxxxxxxxx", InstEmit.Cmeq_S,          typeof(OpCodeSimd64));
            SetA64("0>101110<<1xxxxx100011xxxxxxxxxx", InstEmit.Cmeq_V,          typeof(OpCodeSimdReg64));
            SetA64("0>001110<<100000100110xxxxxxxxxx", InstEmit.Cmeq_V,          typeof(OpCodeSimd64));
            SetA64("01011110111xxxxx001111xxxxxxxxxx", InstEmit.Cmge_S,          typeof(OpCodeSimdReg64));
            SetA64("0111111011100000100010xxxxxxxxxx", InstEmit.Cmge_S,          typeof(OpCodeSimd64));
            SetA64("0>001110<<1xxxxx001111xxxxxxxxxx", InstEmit.Cmge_V,          typeof(OpCodeSimdReg64));
            SetA64("0>101110<<100000100010xxxxxxxxxx", InstEmit.Cmge_V,          typeof(OpCodeSimd64));
            SetA64("01011110111xxxxx001101xxxxxxxxxx", InstEmit.Cmgt_S,          typeof(OpCodeSimdReg64));
            SetA64("0101111011100000100010xxxxxxxxxx", InstEmit.Cmgt_S,          typeof(OpCodeSimd64));
            SetA64("0>001110<<1xxxxx001101xxxxxxxxxx", InstEmit.Cmgt_V,          typeof(OpCodeSimdReg64));
            SetA64("0>001110<<100000100010xxxxxxxxxx", InstEmit.Cmgt_V,          typeof(OpCodeSimd64));
            SetA64("01111110111xxxxx001101xxxxxxxxxx", InstEmit.Cmhi_S,          typeof(OpCodeSimdReg64));
            SetA64("0>101110<<1xxxxx001101xxxxxxxxxx", InstEmit.Cmhi_V,          typeof(OpCodeSimdReg64));
            SetA64("01111110111xxxxx001111xxxxxxxxxx", InstEmit.Cmhs_S,          typeof(OpCodeSimdReg64));
            SetA64("0>101110<<1xxxxx001111xxxxxxxxxx", InstEmit.Cmhs_V,          typeof(OpCodeSimdReg64));
            SetA64("0111111011100000100110xxxxxxxxxx", InstEmit.Cmle_S,          typeof(OpCodeSimd64));
            SetA64("0>101110<<100000100110xxxxxxxxxx", InstEmit.Cmle_V,          typeof(OpCodeSimd64));
            SetA64("0101111011100000101010xxxxxxxxxx", InstEmit.Cmlt_S,          typeof(OpCodeSimd64));
            SetA64("0>001110<<100000101010xxxxxxxxxx", InstEmit.Cmlt_V,          typeof(OpCodeSimd64));
            SetA64("01011110111xxxxx100011xxxxxxxxxx", InstEmit.Cmtst_S,         typeof(OpCodeSimdReg64));
            SetA64("0>001110<<1xxxxx100011xxxxxxxxxx", InstEmit.Cmtst_V,         typeof(OpCodeSimdReg64));
            SetA64("0x00111000100000010110xxxxxxxxxx", InstEmit.Cnt_V,           typeof(OpCodeSimd64));
            SetA64("0>001110000x<>>>000011xxxxxxxxxx", InstEmit.Dup_Gp,          typeof(OpCodeSimdIns64));
            SetA64("01011110000xxxxx000001xxxxxxxxxx", InstEmit.Dup_S,           typeof(OpCodeSimdIns64));
            SetA64("0>001110000x<>>>000001xxxxxxxxxx", InstEmit.Dup_V,           typeof(OpCodeSimdIns64));
            SetA64("0x101110001xxxxx000111xxxxxxxxxx", InstEmit.Eor_V,           typeof(OpCodeSimdReg64));
            SetA64("0>101110000xxxxx0<xxx0xxxxxxxxxx", InstEmit.Ext_V,           typeof(OpCodeSimdExt64));
            SetA64("011111101x1xxxxx110101xxxxxxxxxx", InstEmit.Fabd_S,          typeof(OpCodeSimdReg64));
            SetA64("0>1011101<1xxxxx110101xxxxxxxxxx", InstEmit.Fabd_V,          typeof(OpCodeSimdReg64));
            SetA64("000111100x100000110000xxxxxxxxxx", InstEmit.Fabs_S,          typeof(OpCodeSimd64));
            SetA64("0>0011101<100000111110xxxxxxxxxx", InstEmit.Fabs_V,          typeof(OpCodeSimd64));
            SetA64("000111100x1xxxxx001010xxxxxxxxxx", InstEmit.Fadd_S,          typeof(OpCodeSimdReg64));
            SetA64("0>0011100<1xxxxx110101xxxxxxxxxx", InstEmit.Fadd_V,          typeof(OpCodeSimdReg64));
            SetA64("011111100x110000110110xxxxxxxxxx", InstEmit.Faddp_S,         typeof(OpCodeSimd64));
            SetA64("0>1011100<1xxxxx110101xxxxxxxxxx", InstEmit.Faddp_V,         typeof(OpCodeSimdReg64));
            SetA64("000111100x1xxxxxxxxx01xxxxx0xxxx", InstEmit.Fccmp_S,         typeof(OpCodeSimdFcond64));
            SetA64("000111100x1xxxxxxxxx01xxxxx1xxxx", InstEmit.Fccmpe_S,        typeof(OpCodeSimdFcond64));
            SetA64("010111100x1xxxxx111001xxxxxxxxxx", InstEmit.Fcmeq_S,         typeof(OpCodeSimdReg64));
            SetA64("010111101x100000110110xxxxxxxxxx", InstEmit.Fcmeq_S,         typeof(OpCodeSimd64));
            SetA64("0>0011100<1xxxxx111001xxxxxxxxxx", InstEmit.Fcmeq_V,         typeof(OpCodeSimdReg64));
            SetA64("0>0011101<100000110110xxxxxxxxxx", InstEmit.Fcmeq_V,         typeof(OpCodeSimd64));
            SetA64("011111100x1xxxxx111001xxxxxxxxxx", InstEmit.Fcmge_S,         typeof(OpCodeSimdReg64));
            SetA64("011111101x100000110010xxxxxxxxxx", InstEmit.Fcmge_S,         typeof(OpCodeSimd64));
            SetA64("0>1011100<1xxxxx111001xxxxxxxxxx", InstEmit.Fcmge_V,         typeof(OpCodeSimdReg64));
            SetA64("0>1011101<100000110010xxxxxxxxxx", InstEmit.Fcmge_V,         typeof(OpCodeSimd64));
            SetA64("011111101x1xxxxx111001xxxxxxxxxx", InstEmit.Fcmgt_S,         typeof(OpCodeSimdReg64));
            SetA64("010111101x100000110010xxxxxxxxxx", InstEmit.Fcmgt_S,         typeof(OpCodeSimd64));
            SetA64("0>1011101<1xxxxx111001xxxxxxxxxx", InstEmit.Fcmgt_V,         typeof(OpCodeSimdReg64));
            SetA64("0>0011101<100000110010xxxxxxxxxx", InstEmit.Fcmgt_V,         typeof(OpCodeSimd64));
            SetA64("011111101x100000110110xxxxxxxxxx", InstEmit.Fcmle_S,         typeof(OpCodeSimd64));
            SetA64("0>1011101<100000110110xxxxxxxxxx", InstEmit.Fcmle_V,         typeof(OpCodeSimd64));
            SetA64("010111101x100000111010xxxxxxxxxx", InstEmit.Fcmlt_S,         typeof(OpCodeSimd64));
            SetA64("0>0011101<100000111010xxxxxxxxxx", InstEmit.Fcmlt_V,         typeof(OpCodeSimd64));
            SetA64("000111100x1xxxxx001000xxxxx0x000", InstEmit.Fcmp_S,          typeof(OpCodeSimdReg64));
            SetA64("000111100x1xxxxx001000xxxxx1x000", InstEmit.Fcmpe_S,         typeof(OpCodeSimdReg64));
            SetA64("000111100x1xxxxxxxxx11xxxxxxxxxx", InstEmit.Fcsel_S,         typeof(OpCodeSimdFcond64));
            SetA64("00011110xx10001xx10000xxxxxxxxxx", InstEmit.Fcvt_S,          typeof(OpCodeSimd64));
            SetA64("x00111100x100100000000xxxxxxxxxx", InstEmit.Fcvtas_Gp,       typeof(OpCodeSimdCvt64));
            SetA64("x00111100x100101000000xxxxxxxxxx", InstEmit.Fcvtau_Gp,       typeof(OpCodeSimdCvt64));
            SetA64("0x0011100x100001011110xxxxxxxxxx", InstEmit.Fcvtl_V,         typeof(OpCodeSimd64));
            SetA64("x00111100x110000000000xxxxxxxxxx", InstEmit.Fcvtms_Gp,       typeof(OpCodeSimdCvt64));
            SetA64("x00111100x110001000000xxxxxxxxxx", InstEmit.Fcvtmu_Gp,       typeof(OpCodeSimdCvt64));
            SetA64("0x0011100x100001011010xxxxxxxxxx", InstEmit.Fcvtn_V,         typeof(OpCodeSimd64));
            SetA64("010111100x100001101010xxxxxxxxxx", InstEmit.Fcvtns_S,        typeof(OpCodeSimd64));
            SetA64("0>0011100<100001101010xxxxxxxxxx", InstEmit.Fcvtns_V,        typeof(OpCodeSimd64));
            SetA64("011111100x100001101010xxxxxxxxxx", InstEmit.Fcvtnu_S,        typeof(OpCodeSimd64));
            SetA64("0>1011100<100001101010xxxxxxxxxx", InstEmit.Fcvtnu_V,        typeof(OpCodeSimd64));
            SetA64("x00111100x101000000000xxxxxxxxxx", InstEmit.Fcvtps_Gp,       typeof(OpCodeSimdCvt64));
            SetA64("x00111100x101001000000xxxxxxxxxx", InstEmit.Fcvtpu_Gp,       typeof(OpCodeSimdCvt64));
            SetA64("x00111100x111000000000xxxxxxxxxx", InstEmit.Fcvtzs_Gp,       typeof(OpCodeSimdCvt64));
            SetA64(">00111100x011000>xxxxxxxxxxxxxxx", InstEmit.Fcvtzs_Gp_Fixed, typeof(OpCodeSimdCvt64));
            SetA64("010111101x100001101110xxxxxxxxxx", InstEmit.Fcvtzs_S,        typeof(OpCodeSimd64));
            SetA64("0>0011101<100001101110xxxxxxxxxx", InstEmit.Fcvtzs_V,        typeof(OpCodeSimd64));
            SetA64("0x001111001xxxxx111111xxxxxxxxxx", InstEmit.Fcvtzs_V_Fixed,  typeof(OpCodeSimdShImm64));
            SetA64("0100111101xxxxxx111111xxxxxxxxxx", InstEmit.Fcvtzs_V_Fixed,  typeof(OpCodeSimdShImm64));
            SetA64("x00111100x111001000000xxxxxxxxxx", InstEmit.Fcvtzu_Gp,       typeof(OpCodeSimdCvt64));
            SetA64(">00111100x011001>xxxxxxxxxxxxxxx", InstEmit.Fcvtzu_Gp_Fixed, typeof(OpCodeSimdCvt64));
            SetA64("011111101x100001101110xxxxxxxxxx", InstEmit.Fcvtzu_S,        typeof(OpCodeSimd64));
            SetA64("0>1011101<100001101110xxxxxxxxxx", InstEmit.Fcvtzu_V,        typeof(OpCodeSimd64));
            SetA64("0x101111001xxxxx111111xxxxxxxxxx", InstEmit.Fcvtzu_V_Fixed,  typeof(OpCodeSimdShImm64));
            SetA64("0110111101xxxxxx111111xxxxxxxxxx", InstEmit.Fcvtzu_V_Fixed,  typeof(OpCodeSimdShImm64));
            SetA64("000111100x1xxxxx000110xxxxxxxxxx", InstEmit.Fdiv_S,          typeof(OpCodeSimdReg64));
            SetA64("0>1011100<1xxxxx111111xxxxxxxxxx", InstEmit.Fdiv_V,          typeof(OpCodeSimdReg64));
            SetA64("000111110x0xxxxx0xxxxxxxxxxxxxxx", InstEmit.Fmadd_S,         typeof(OpCodeSimdReg64));
            SetA64("000111100x1xxxxx010010xxxxxxxxxx", InstEmit.Fmax_S,          typeof(OpCodeSimdReg64));
            SetA64("0>0011100<1xxxxx111101xxxxxxxxxx", InstEmit.Fmax_V,          typeof(OpCodeSimdReg64));
            SetA64("000111100x1xxxxx011010xxxxxxxxxx", InstEmit.Fmaxnm_S,        typeof(OpCodeSimdReg64));
            SetA64("0>0011100<1xxxxx110001xxxxxxxxxx", InstEmit.Fmaxnm_V,        typeof(OpCodeSimdReg64));
            SetA64("0>1011100<1xxxxx111101xxxxxxxxxx", InstEmit.Fmaxp_V,         typeof(OpCodeSimdReg64));
            SetA64("000111100x1xxxxx010110xxxxxxxxxx", InstEmit.Fmin_S,          typeof(OpCodeSimdReg64));
            SetA64("0>0011101<1xxxxx111101xxxxxxxxxx", InstEmit.Fmin_V,          typeof(OpCodeSimdReg64));
            SetA64("000111100x1xxxxx011110xxxxxxxxxx", InstEmit.Fminnm_S,        typeof(OpCodeSimdReg64));
            SetA64("0>0011101<1xxxxx110001xxxxxxxxxx", InstEmit.Fminnm_V,        typeof(OpCodeSimdReg64));
            SetA64("0>1011101<1xxxxx111101xxxxxxxxxx", InstEmit.Fminp_V,         typeof(OpCodeSimdReg64));
            SetA64("010111111xxxxxxx0001x0xxxxxxxxxx", InstEmit.Fmla_Se,         typeof(OpCodeSimdRegElemF64));
            SetA64("0>0011100<1xxxxx110011xxxxxxxxxx", InstEmit.Fmla_V,          typeof(OpCodeSimdReg64));
            SetA64("0>0011111<xxxxxx0001x0xxxxxxxxxx", InstEmit.Fmla_Ve,         typeof(OpCodeSimdRegElemF64));
            SetA64("010111111xxxxxxx0101x0xxxxxxxxxx", InstEmit.Fmls_Se,         typeof(OpCodeSimdRegElemF64));
            SetA64("0>0011101<1xxxxx110011xxxxxxxxxx", InstEmit.Fmls_V,          typeof(OpCodeSimdReg64));
            SetA64("0>0011111<xxxxxx0101x0xxxxxxxxxx", InstEmit.Fmls_Ve,         typeof(OpCodeSimdRegElemF64));
            SetA64("000111100x100000010000xxxxxxxxxx", InstEmit.Fmov_S,          typeof(OpCodeSimd64));
            SetA64("000111100x1xxxxxxxx10000000xxxxx", InstEmit.Fmov_Si,         typeof(OpCodeSimdFmov64));
            SetA64("0x00111100000xxx111101xxxxxxxxxx", InstEmit.Fmov_Vi,         typeof(OpCodeSimdImm64));
            SetA64("0110111100000xxx111101xxxxxxxxxx", InstEmit.Fmov_Vi,         typeof(OpCodeSimdImm64));
            SetA64("0001111000100110000000xxxxxxxxxx", InstEmit.Fmov_Ftoi,       typeof(OpCodeSimd64));
            SetA64("1001111001100110000000xxxxxxxxxx", InstEmit.Fmov_Ftoi,       typeof(OpCodeSimd64));
            SetA64("0001111000100111000000xxxxxxxxxx", InstEmit.Fmov_Itof,       typeof(OpCodeSimd64));
            SetA64("1001111001100111000000xxxxxxxxxx", InstEmit.Fmov_Itof,       typeof(OpCodeSimd64));
            SetA64("1001111010101110000000xxxxxxxxxx", InstEmit.Fmov_Ftoi1,      typeof(OpCodeSimd64));
            SetA64("1001111010101111000000xxxxxxxxxx", InstEmit.Fmov_Itof1,      typeof(OpCodeSimd64));
            SetA64("000111110x0xxxxx1xxxxxxxxxxxxxxx", InstEmit.Fmsub_S,         typeof(OpCodeSimdReg64));
            SetA64("000111100x1xxxxx000010xxxxxxxxxx", InstEmit.Fmul_S,          typeof(OpCodeSimdReg64));
            SetA64("010111111xxxxxxx1001x0xxxxxxxxxx", InstEmit.Fmul_Se,         typeof(OpCodeSimdRegElemF64));
            SetA64("0>1011100<1xxxxx110111xxxxxxxxxx", InstEmit.Fmul_V,          typeof(OpCodeSimdReg64));
            SetA64("0>0011111<xxxxxx1001x0xxxxxxxxxx", InstEmit.Fmul_Ve,         typeof(OpCodeSimdRegElemF64));
            SetA64("010111100x1xxxxx110111xxxxxxxxxx", InstEmit.Fmulx_S,         typeof(OpCodeSimdReg64));
            SetA64("011111111xxxxxxx1001x0xxxxxxxxxx", InstEmit.Fmulx_Se,        typeof(OpCodeSimdRegElemF64));
            SetA64("0>0011100<1xxxxx110111xxxxxxxxxx", InstEmit.Fmulx_V,         typeof(OpCodeSimdReg64));
            SetA64("0>1011111<xxxxxx1001x0xxxxxxxxxx", InstEmit.Fmulx_Ve,        typeof(OpCodeSimdRegElemF64));
            SetA64("000111100x100001010000xxxxxxxxxx", InstEmit.Fneg_S,          typeof(OpCodeSimd64));
            SetA64("0>1011101<100000111110xxxxxxxxxx", InstEmit.Fneg_V,          typeof(OpCodeSimd64));
            SetA64("000111110x1xxxxx0xxxxxxxxxxxxxxx", InstEmit.Fnmadd_S,        typeof(OpCodeSimdReg64));
            SetA64("000111110x1xxxxx1xxxxxxxxxxxxxxx", InstEmit.Fnmsub_S,        typeof(OpCodeSimdReg64));
            SetA64("000111100x1xxxxx100010xxxxxxxxxx", InstEmit.Fnmul_S,         typeof(OpCodeSimdReg64));
            SetA64("010111101x100001110110xxxxxxxxxx", InstEmit.Frecpe_S,        typeof(OpCodeSimd64));
            SetA64("0>0011101<100001110110xxxxxxxxxx", InstEmit.Frecpe_V,        typeof(OpCodeSimd64));
            SetA64("010111100x1xxxxx111111xxxxxxxxxx", InstEmit.Frecps_S,        typeof(OpCodeSimdReg64));
            SetA64("0>0011100<1xxxxx111111xxxxxxxxxx", InstEmit.Frecps_V,        typeof(OpCodeSimdReg64));
            SetA64("010111101x100001111110xxxxxxxxxx", InstEmit.Frecpx_S,        typeof(OpCodeSimd64));
            SetA64("000111100x100110010000xxxxxxxxxx", InstEmit.Frinta_S,        typeof(OpCodeSimd64));
            SetA64("0>1011100<100001100010xxxxxxxxxx", InstEmit.Frinta_V,        typeof(OpCodeSimd64));
            SetA64("000111100x100111110000xxxxxxxxxx", InstEmit.Frinti_S,        typeof(OpCodeSimd64));
            SetA64("0>1011101<100001100110xxxxxxxxxx", InstEmit.Frinti_V,        typeof(OpCodeSimd64));
            SetA64("000111100x100101010000xxxxxxxxxx", InstEmit.Frintm_S,        typeof(OpCodeSimd64));
            SetA64("0>0011100<100001100110xxxxxxxxxx", InstEmit.Frintm_V,        typeof(OpCodeSimd64));
            SetA64("000111100x100100010000xxxxxxxxxx", InstEmit.Frintn_S,        typeof(OpCodeSimd64));
            SetA64("0>0011100<100001100010xxxxxxxxxx", InstEmit.Frintn_V,        typeof(OpCodeSimd64));
            SetA64("000111100x100100110000xxxxxxxxxx", InstEmit.Frintp_S,        typeof(OpCodeSimd64));
            SetA64("0>0011101<100001100010xxxxxxxxxx", InstEmit.Frintp_V,        typeof(OpCodeSimd64));
            SetA64("000111100x100111010000xxxxxxxxxx", InstEmit.Frintx_S,        typeof(OpCodeSimd64));
            SetA64("0>1011100<100001100110xxxxxxxxxx", InstEmit.Frintx_V,        typeof(OpCodeSimd64));
            SetA64("000111100x100101110000xxxxxxxxxx", InstEmit.Frintz_S,        typeof(OpCodeSimd64));
            SetA64("0>0011101<100001100110xxxxxxxxxx", InstEmit.Frintz_V,        typeof(OpCodeSimd64));
            SetA64("011111101x100001110110xxxxxxxxxx", InstEmit.Frsqrte_S,       typeof(OpCodeSimd64));
            SetA64("0>1011101<100001110110xxxxxxxxxx", InstEmit.Frsqrte_V,       typeof(OpCodeSimd64));
            SetA64("010111101x1xxxxx111111xxxxxxxxxx", InstEmit.Frsqrts_S,       typeof(OpCodeSimdReg64));
            SetA64("0>0011101<1xxxxx111111xxxxxxxxxx", InstEmit.Frsqrts_V,       typeof(OpCodeSimdReg64));
            SetA64("000111100x100001110000xxxxxxxxxx", InstEmit.Fsqrt_S,         typeof(OpCodeSimd64));
            SetA64("0>1011101<100001111110xxxxxxxxxx", InstEmit.Fsqrt_V,         typeof(OpCodeSimd64));
            SetA64("000111100x1xxxxx001110xxxxxxxxxx", InstEmit.Fsub_S,          typeof(OpCodeSimdReg64));
            SetA64("0>0011101<1xxxxx110101xxxxxxxxxx", InstEmit.Fsub_V,          typeof(OpCodeSimdReg64));
            SetA64("01001110000xxxxx000111xxxxxxxxxx", InstEmit.Ins_Gp,          typeof(OpCodeSimdIns64));
            SetA64("01101110000xxxxx0xxxx1xxxxxxxxxx", InstEmit.Ins_V,           typeof(OpCodeSimdIns64));
            SetA64("0x00110001000000xxxxxxxxxxxxxxxx", InstEmit.Ld__Vms,         typeof(OpCodeSimdMemMs64));
            SetA64("0x001100110xxxxxxxxxxxxxxxxxxxxx", InstEmit.Ld__Vms,         typeof(OpCodeSimdMemMs64));
            SetA64("0x00110101x00000xxxxxxxxxxxxxxxx", InstEmit.Ld__Vss,         typeof(OpCodeSimdMemSs64));
            SetA64("0x00110111xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Ld__Vss,         typeof(OpCodeSimdMemSs64));
            SetA64("xx10110xx1xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Ldp,             typeof(OpCodeSimdMemPair64));
            SetA64("xx111100x10xxxxxxxxx00xxxxxxxxxx", InstEmit.Ldr,             typeof(OpCodeSimdMemImm64));
            SetA64("xx111100x10xxxxxxxxx01xxxxxxxxxx", InstEmit.Ldr,             typeof(OpCodeSimdMemImm64));
            SetA64("xx111100x10xxxxxxxxx11xxxxxxxxxx", InstEmit.Ldr,             typeof(OpCodeSimdMemImm64));
            SetA64("xx111101x1xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Ldr,             typeof(OpCodeSimdMemImm64));
            SetA64("xx111100x11xxxxxxxxx10xxxxxxxxxx", InstEmit.Ldr,             typeof(OpCodeSimdMemReg64));
            SetA64("xx011100xxxxxxxxxxxxxxxxxxxxxxxx", InstEmit.Ldr_Literal,     typeof(OpCodeSimdMemLit64));
            SetA64("0x001110<<1xxxxx100101xxxxxxxxxx", InstEmit.Mla_V,           typeof(OpCodeSimdReg64));
            SetA64("0x101111xxxxxxxx0000x0xxxxxxxxxx", InstEmit.Mla_Ve,          typeof(OpCodeSimdRegElem64));
            SetA64("0x101110<<1xxxxx100101xxxxxxxxxx", InstEmit.Mls_V,           typeof(OpCodeSimdReg64));
            SetA64("0x101111xxxxxxxx0100x0xxxxxxxxxx", InstEmit.Mls_Ve,          typeof(OpCodeSimdRegElem64));
            SetA64("0x00111100000xxx0xx001xxxxxxxxxx", InstEmit.Movi_V,          typeof(OpCodeSimdImm64));
            SetA64("0x00111100000xxx10x001xxxxxxxxxx", InstEmit.Movi_V,          typeof(OpCodeSimdImm64));
            SetA64("0x00111100000xxx110x01xxxxxxxxxx", InstEmit.Movi_V,          typeof(OpCodeSimdImm64));
            SetA64("0xx0111100000xxx111001xxxxxxxxxx", InstEmit.Movi_V,          typeof(OpCodeSimdImm64));
            SetA64("0x001110<<1xxxxx100111xxxxxxxxxx", InstEmit.Mul_V,           typeof(OpCodeSimdReg64));
            SetA64("0x001111xxxxxxxx1000x0xxxxxxxxxx", InstEmit.Mul_Ve,          typeof(OpCodeSimdRegElem64));
            SetA64("0x10111100000xxx0xx001xxxxxxxxxx", InstEmit.Mvni_V,          typeof(OpCodeSimdImm64));
            SetA64("0x10111100000xxx10x001xxxxxxxxxx", InstEmit.Mvni_V,          typeof(OpCodeSimdImm64));
            SetA64("0x10111100000xxx110x01xxxxxxxxxx", InstEmit.Mvni_V,          typeof(OpCodeSimdImm64));
            SetA64("0111111011100000101110xxxxxxxxxx", InstEmit.Neg_S,           typeof(OpCodeSimd64));
            SetA64("0>101110<<100000101110xxxxxxxxxx", InstEmit.Neg_V,           typeof(OpCodeSimd64));
            SetA64("0x10111000100000010110xxxxxxxxxx", InstEmit.Not_V,           typeof(OpCodeSimd64));
            SetA64("0x001110111xxxxx000111xxxxxxxxxx", InstEmit.Orn_V,           typeof(OpCodeSimdReg64));
            SetA64("0x001110101xxxxx000111xxxxxxxxxx", InstEmit.Orr_V,           typeof(OpCodeSimdReg64));
            SetA64("0x00111100000xxx0xx101xxxxxxxxxx", InstEmit.Orr_Vi,          typeof(OpCodeSimdImm64));
            SetA64("0x00111100000xxx10x101xxxxxxxxxx", InstEmit.Orr_Vi,          typeof(OpCodeSimdImm64));
            SetA64("0x101110<<1xxxxx010000xxxxxxxxxx", InstEmit.Raddhn_V,        typeof(OpCodeSimdReg64));
            SetA64("0x10111001100000010110xxxxxxxxxx", InstEmit.Rbit_V,          typeof(OpCodeSimd64));
            SetA64("0x00111000100000000110xxxxxxxxxx", InstEmit.Rev16_V,         typeof(OpCodeSimd64));
            SetA64("0x1011100x100000000010xxxxxxxxxx", InstEmit.Rev32_V,         typeof(OpCodeSimd64));
            SetA64("0x001110<<100000000010xxxxxxxxxx", InstEmit.Rev64_V,         typeof(OpCodeSimd64));
            SetA64("0x00111100>>>xxx100011xxxxxxxxxx", InstEmit.Rshrn_V,         typeof(OpCodeSimdShImm64));
            SetA64("0x101110<<1xxxxx011000xxxxxxxxxx", InstEmit.Rsubhn_V,        typeof(OpCodeSimdReg64));
            SetA64("0x001110<<1xxxxx011111xxxxxxxxxx", InstEmit.Saba_V,          typeof(OpCodeSimdReg64));
            SetA64("0x001110<<1xxxxx010100xxxxxxxxxx", InstEmit.Sabal_V,         typeof(OpCodeSimdReg64));
            SetA64("0x001110<<1xxxxx011101xxxxxxxxxx", InstEmit.Sabd_V,          typeof(OpCodeSimdReg64));
            SetA64("0x001110<<1xxxxx011100xxxxxxxxxx", InstEmit.Sabdl_V,         typeof(OpCodeSimdReg64));
            SetA64("0x001110<<100000011010xxxxxxxxxx", InstEmit.Sadalp_V,        typeof(OpCodeSimd64));
            SetA64("0x001110<<1xxxxx000000xxxxxxxxxx", InstEmit.Saddl_V,         typeof(OpCodeSimdReg64));
            SetA64("0x001110<<100000001010xxxxxxxxxx", InstEmit.Saddlp_V,        typeof(OpCodeSimd64));
            SetA64("000011100x110000001110xxxxxxxxxx", InstEmit.Saddlv_V,        typeof(OpCodeSimd64));
            SetA64("01001110<<110000001110xxxxxxxxxx", InstEmit.Saddlv_V,        typeof(OpCodeSimd64));
            SetA64("0x001110<<1xxxxx000100xxxxxxxxxx", InstEmit.Saddw_V,         typeof(OpCodeSimdReg64));
            SetA64("x00111100x100010000000xxxxxxxxxx", InstEmit.Scvtf_Gp,        typeof(OpCodeSimdCvt64));
            SetA64(">00111100x000010>xxxxxxxxxxxxxxx", InstEmit.Scvtf_Gp_Fixed,  typeof(OpCodeSimdCvt64));
            SetA64("010111100x100001110110xxxxxxxxxx", InstEmit.Scvtf_S,         typeof(OpCodeSimd64));
            SetA64("0>0011100<100001110110xxxxxxxxxx", InstEmit.Scvtf_V,         typeof(OpCodeSimd64));
            SetA64("0x001111001xxxxx111001xxxxxxxxxx", InstEmit.Scvtf_V_Fixed,   typeof(OpCodeSimdShImm64));
            SetA64("0100111101xxxxxx111001xxxxxxxxxx", InstEmit.Scvtf_V_Fixed,   typeof(OpCodeSimdShImm64));
            SetA64("01011110000xxxxx000000xxxxxxxxxx", InstEmit.Sha1c_V,         typeof(OpCodeSimdReg64));
            SetA64("0101111000101000000010xxxxxxxxxx", InstEmit.Sha1h_V,         typeof(OpCodeSimd64));
            SetA64("01011110000xxxxx001000xxxxxxxxxx", InstEmit.Sha1m_V,         typeof(OpCodeSimdReg64));
            SetA64("01011110000xxxxx000100xxxxxxxxxx", InstEmit.Sha1p_V,         typeof(OpCodeSimdReg64));
            SetA64("01011110000xxxxx001100xxxxxxxxxx", InstEmit.Sha1su0_V,       typeof(OpCodeSimdReg64));
            SetA64("0101111000101000000110xxxxxxxxxx", InstEmit.Sha1su1_V,       typeof(OpCodeSimd64));
            SetA64("01011110000xxxxx010000xxxxxxxxxx", InstEmit.Sha256h_V,       typeof(OpCodeSimdReg64));
            SetA64("01011110000xxxxx010100xxxxxxxxxx", InstEmit.Sha256h2_V,      typeof(OpCodeSimdReg64));
            SetA64("0101111000101000001010xxxxxxxxxx", InstEmit.Sha256su0_V,     typeof(OpCodeSimd64));
            SetA64("01011110000xxxxx011000xxxxxxxxxx", InstEmit.Sha256su1_V,     typeof(OpCodeSimdReg64));
            SetA64("0x001110<<1xxxxx000001xxxxxxxxxx", InstEmit.Shadd_V,         typeof(OpCodeSimdReg64));
            SetA64("0101111101xxxxxx010101xxxxxxxxxx", InstEmit.Shl_S,           typeof(OpCodeSimdShImm64));
            SetA64("0x00111100>>>xxx010101xxxxxxxxxx", InstEmit.Shl_V,           typeof(OpCodeSimdShImm64));
            SetA64("0100111101xxxxxx010101xxxxxxxxxx", InstEmit.Shl_V,           typeof(OpCodeSimdShImm64));
            SetA64("0x101110<<100001001110xxxxxxxxxx", InstEmit.Shll_V,          typeof(OpCodeSimd64));
            SetA64("0x00111100>>>xxx100001xxxxxxxxxx", InstEmit.Shrn_V,          typeof(OpCodeSimdShImm64));
            SetA64("0x001110<<1xxxxx001001xxxxxxxxxx", InstEmit.Shsub_V,         typeof(OpCodeSimdReg64));
            SetA64("0x10111100>>>xxx010101xxxxxxxxxx", InstEmit.Sli_V,           typeof(OpCodeSimdShImm64));
            SetA64("0110111101xxxxxx010101xxxxxxxxxx", InstEmit.Sli_V,           typeof(OpCodeSimdShImm64));
            SetA64("0x001110<<1xxxxx011001xxxxxxxxxx", InstEmit.Smax_V,          typeof(OpCodeSimdReg64));
            SetA64("0x001110<<1xxxxx101001xxxxxxxxxx", InstEmit.Smaxp_V,         typeof(OpCodeSimdReg64));
            SetA64("000011100x110000101010xxxxxxxxxx", InstEmit.Smaxv_V,         typeof(OpCodeSimd64));
            SetA64("01001110<<110000101010xxxxxxxxxx", InstEmit.Smaxv_V,         typeof(OpCodeSimd64));
            SetA64("0x001110<<1xxxxx011011xxxxxxxxxx", InstEmit.Smin_V,          typeof(OpCodeSimdReg64));
            SetA64("0x001110<<1xxxxx101011xxxxxxxxxx", InstEmit.Sminp_V,         typeof(OpCodeSimdReg64));
            SetA64("000011100x110001101010xxxxxxxxxx", InstEmit.Sminv_V,         typeof(OpCodeSimd64));
            SetA64("01001110<<110001101010xxxxxxxxxx", InstEmit.Sminv_V,         typeof(OpCodeSimd64));
            SetA64("0x001110<<1xxxxx100000xxxxxxxxxx", InstEmit.Smlal_V,         typeof(OpCodeSimdReg64));
            SetA64("0x001111xxxxxxxx0010x0xxxxxxxxxx", InstEmit.Smlal_Ve,        typeof(OpCodeSimdRegElem64));
            SetA64("0x001110<<1xxxxx101000xxxxxxxxxx", InstEmit.Smlsl_V,         typeof(OpCodeSimdReg64));
            SetA64("0x001111xxxxxxxx0110x0xxxxxxxxxx", InstEmit.Smlsl_Ve,        typeof(OpCodeSimdRegElem64));
            SetA64("0x001110000xxxxx001011xxxxxxxxxx", InstEmit.Smov_S,          typeof(OpCodeSimdIns64));
            SetA64("0x001110<<1xxxxx110000xxxxxxxxxx", InstEmit.Smull_V,         typeof(OpCodeSimdReg64));
            SetA64("0x001111xxxxxxxx1010x0xxxxxxxxxx", InstEmit.Smull_Ve,        typeof(OpCodeSimdRegElem64));
            SetA64("01011110xx100000011110xxxxxxxxxx", InstEmit.Sqabs_S,         typeof(OpCodeSimd64));
            SetA64("0>001110<<100000011110xxxxxxxxxx", InstEmit.Sqabs_V,         typeof(OpCodeSimd64));
            SetA64("01011110xx1xxxxx000011xxxxxxxxxx", InstEmit.Sqadd_S,         typeof(OpCodeSimdReg64));
            SetA64("0>001110<<1xxxxx000011xxxxxxxxxx", InstEmit.Sqadd_V,         typeof(OpCodeSimdReg64));
            SetA64("01011110011xxxxx101101xxxxxxxxxx", InstEmit.Sqdmulh_S,       typeof(OpCodeSimdReg64));
            SetA64("01011110101xxxxx101101xxxxxxxxxx", InstEmit.Sqdmulh_S,       typeof(OpCodeSimdReg64));
            SetA64("0x001110011xxxxx101101xxxxxxxxxx", InstEmit.Sqdmulh_V,       typeof(OpCodeSimdReg64));
            SetA64("0x001110101xxxxx101101xxxxxxxxxx", InstEmit.Sqdmulh_V,       typeof(OpCodeSimdReg64));
            SetA64("01111110xx100000011110xxxxxxxxxx", InstEmit.Sqneg_S,         typeof(OpCodeSimd64));
            SetA64("0>101110<<100000011110xxxxxxxxxx", InstEmit.Sqneg_V,         typeof(OpCodeSimd64));
            SetA64("01111110011xxxxx101101xxxxxxxxxx", InstEmit.Sqrdmulh_S,      typeof(OpCodeSimdReg64));
            SetA64("01111110101xxxxx101101xxxxxxxxxx", InstEmit.Sqrdmulh_S,      typeof(OpCodeSimdReg64));
            SetA64("0x101110011xxxxx101101xxxxxxxxxx", InstEmit.Sqrdmulh_V,      typeof(OpCodeSimdReg64));
            SetA64("0x101110101xxxxx101101xxxxxxxxxx", InstEmit.Sqrdmulh_V,      typeof(OpCodeSimdReg64));
            SetA64("0>001110<<1xxxxx010111xxxxxxxxxx", InstEmit.Sqrshl_V,        typeof(OpCodeSimdReg64));
            SetA64("0101111100>>>xxx100111xxxxxxxxxx", InstEmit.Sqrshrn_S,       typeof(OpCodeSimdShImm64));
            SetA64("0x00111100>>>xxx100111xxxxxxxxxx", InstEmit.Sqrshrn_V,       typeof(OpCodeSimdShImm64));
            SetA64("0111111100>>>xxx100011xxxxxxxxxx", InstEmit.Sqrshrun_S,      typeof(OpCodeSimdShImm64));
            SetA64("0x10111100>>>xxx100011xxxxxxxxxx", InstEmit.Sqrshrun_V,      typeof(OpCodeSimdShImm64));
            SetA64("0>001110<<1xxxxx010011xxxxxxxxxx", InstEmit.Sqshl_V,         typeof(OpCodeSimdReg64));
            SetA64("0101111100>>>xxx100101xxxxxxxxxx", InstEmit.Sqshrn_S,        typeof(OpCodeSimdShImm64));
            SetA64("0x00111100>>>xxx100101xxxxxxxxxx", InstEmit.Sqshrn_V,        typeof(OpCodeSimdShImm64));
            SetA64("0111111100>>>xxx100001xxxxxxxxxx", InstEmit.Sqshrun_S,       typeof(OpCodeSimdShImm64));
            SetA64("0x10111100>>>xxx100001xxxxxxxxxx", InstEmit.Sqshrun_V,       typeof(OpCodeSimdShImm64));
            SetA64("01011110xx1xxxxx001011xxxxxxxxxx", InstEmit.Sqsub_S,         typeof(OpCodeSimdReg64));
            SetA64("0>001110<<1xxxxx001011xxxxxxxxxx", InstEmit.Sqsub_V,         typeof(OpCodeSimdReg64));
            SetA64("01011110<<100001010010xxxxxxxxxx", InstEmit.Sqxtn_S,         typeof(OpCodeSimd64));
            SetA64("0x001110<<100001010010xxxxxxxxxx", InstEmit.Sqxtn_V,         typeof(OpCodeSimd64));
            SetA64("01111110<<100001001010xxxxxxxxxx", InstEmit.Sqxtun_S,        typeof(OpCodeSimd64));
            SetA64("0x101110<<100001001010xxxxxxxxxx", InstEmit.Sqxtun_V,        typeof(OpCodeSimd64));
            SetA64("0x001110<<1xxxxx000101xxxxxxxxxx", InstEmit.Srhadd_V,        typeof(OpCodeSimdReg64));
            SetA64("0>001110<<1xxxxx010101xxxxxxxxxx", InstEmit.Srshl_V,         typeof(OpCodeSimdReg64));
            SetA64("0101111101xxxxxx001001xxxxxxxxxx", InstEmit.Srshr_S,         typeof(OpCodeSimdShImm64));
            SetA64("0x00111100>>>xxx001001xxxxxxxxxx", InstEmit.Srshr_V,         typeof(OpCodeSimdShImm64));
            SetA64("0100111101xxxxxx001001xxxxxxxxxx", InstEmit.Srshr_V,         typeof(OpCodeSimdShImm64));
            SetA64("0101111101xxxxxx001101xxxxxxxxxx", InstEmit.Srsra_S,         typeof(OpCodeSimdShImm64));
            SetA64("0x00111100>>>xxx001101xxxxxxxxxx", InstEmit.Srsra_V,         typeof(OpCodeSimdShImm64));
            SetA64("0100111101xxxxxx001101xxxxxxxxxx", InstEmit.Srsra_V,         typeof(OpCodeSimdShImm64));
            SetA64("0>001110<<1xxxxx010001xxxxxxxxxx", InstEmit.Sshl_V,          typeof(OpCodeSimdReg64));
            SetA64("0x00111100>>>xxx101001xxxxxxxxxx", InstEmit.Sshll_V,         typeof(OpCodeSimdShImm64));
            SetA64("0101111101xxxxxx000001xxxxxxxxxx", InstEmit.Sshr_S,          typeof(OpCodeSimdShImm64));
            SetA64("0x00111100>>>xxx000001xxxxxxxxxx", InstEmit.Sshr_V,          typeof(OpCodeSimdShImm64));
            SetA64("0100111101xxxxxx000001xxxxxxxxxx", InstEmit.Sshr_V,          typeof(OpCodeSimdShImm64));
            SetA64("0101111101xxxxxx000101xxxxxxxxxx", InstEmit.Ssra_S,          typeof(OpCodeSimdShImm64));
            SetA64("0x00111100>>>xxx000101xxxxxxxxxx", InstEmit.Ssra_V,          typeof(OpCodeSimdShImm64));
            SetA64("0100111101xxxxxx000101xxxxxxxxxx", InstEmit.Ssra_V,          typeof(OpCodeSimdShImm64));
            SetA64("0x001110<<1xxxxx001000xxxxxxxxxx", InstEmit.Ssubl_V,         typeof(OpCodeSimdReg64));
            SetA64("0x001110<<1xxxxx001100xxxxxxxxxx", InstEmit.Ssubw_V,         typeof(OpCodeSimdReg64));
            SetA64("0x00110000000000xxxxxxxxxxxxxxxx", InstEmit.St__Vms,         typeof(OpCodeSimdMemMs64));
            SetA64("0x001100100xxxxxxxxxxxxxxxxxxxxx", InstEmit.St__Vms,         typeof(OpCodeSimdMemMs64));
            SetA64("0x00110100x00000xxxxxxxxxxxxxxxx", InstEmit.St__Vss,         typeof(OpCodeSimdMemSs64));
            SetA64("0x00110110xxxxxxxxxxxxxxxxxxxxxx", InstEmit.St__Vss,         typeof(OpCodeSimdMemSs64));
            SetA64("xx10110xx0xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Stp,             typeof(OpCodeSimdMemPair64));
            SetA64("xx111100x00xxxxxxxxx00xxxxxxxxxx", InstEmit.Str,             typeof(OpCodeSimdMemImm64));
            SetA64("xx111100x00xxxxxxxxx01xxxxxxxxxx", InstEmit.Str,             typeof(OpCodeSimdMemImm64));
            SetA64("xx111100x00xxxxxxxxx11xxxxxxxxxx", InstEmit.Str,             typeof(OpCodeSimdMemImm64));
            SetA64("xx111101x0xxxxxxxxxxxxxxxxxxxxxx", InstEmit.Str,             typeof(OpCodeSimdMemImm64));
            SetA64("xx111100x01xxxxxxxxx10xxxxxxxxxx", InstEmit.Str,             typeof(OpCodeSimdMemReg64));
            SetA64("01111110111xxxxx100001xxxxxxxxxx", InstEmit.Sub_S,           typeof(OpCodeSimdReg64));
            SetA64("0>101110<<1xxxxx100001xxxxxxxxxx", InstEmit.Sub_V,           typeof(OpCodeSimdReg64));
            SetA64("0x001110<<1xxxxx011000xxxxxxxxxx", InstEmit.Subhn_V,         typeof(OpCodeSimdReg64));
            SetA64("01011110xx100000001110xxxxxxxxxx", InstEmit.Suqadd_S,        typeof(OpCodeSimd64));
            SetA64("0>001110<<100000001110xxxxxxxxxx", InstEmit.Suqadd_V,        typeof(OpCodeSimd64));
            SetA64("0x001110000xxxxx0xx000xxxxxxxxxx", InstEmit.Tbl_V,           typeof(OpCodeSimdTbl64));
            SetA64("0>001110<<0xxxxx001010xxxxxxxxxx", InstEmit.Trn1_V,          typeof(OpCodeSimdReg64));
            SetA64("0>001110<<0xxxxx011010xxxxxxxxxx", InstEmit.Trn2_V,          typeof(OpCodeSimdReg64));
            SetA64("0x101110<<1xxxxx011111xxxxxxxxxx", InstEmit.Uaba_V,          typeof(OpCodeSimdReg64));
            SetA64("0x101110<<1xxxxx010100xxxxxxxxxx", InstEmit.Uabal_V,         typeof(OpCodeSimdReg64));
            SetA64("0x101110<<1xxxxx011101xxxxxxxxxx", InstEmit.Uabd_V,          typeof(OpCodeSimdReg64));
            SetA64("0x101110<<1xxxxx011100xxxxxxxxxx", InstEmit.Uabdl_V,         typeof(OpCodeSimdReg64));
            SetA64("0x101110<<100000011010xxxxxxxxxx", InstEmit.Uadalp_V,        typeof(OpCodeSimd64));
            SetA64("0x101110<<1xxxxx000000xxxxxxxxxx", InstEmit.Uaddl_V,         typeof(OpCodeSimdReg64));
            SetA64("0x101110<<100000001010xxxxxxxxxx", InstEmit.Uaddlp_V,        typeof(OpCodeSimd64));
            SetA64("001011100x110000001110xxxxxxxxxx", InstEmit.Uaddlv_V,        typeof(OpCodeSimd64));
            SetA64("01101110<<110000001110xxxxxxxxxx", InstEmit.Uaddlv_V,        typeof(OpCodeSimd64));
            SetA64("0x101110<<1xxxxx000100xxxxxxxxxx", InstEmit.Uaddw_V,         typeof(OpCodeSimdReg64));
            SetA64("x00111100x100011000000xxxxxxxxxx", InstEmit.Ucvtf_Gp,        typeof(OpCodeSimdCvt64));
            SetA64(">00111100x000011>xxxxxxxxxxxxxxx", InstEmit.Ucvtf_Gp_Fixed,  typeof(OpCodeSimdCvt64));
            SetA64("011111100x100001110110xxxxxxxxxx", InstEmit.Ucvtf_S,         typeof(OpCodeSimd64));
            SetA64("0>1011100<100001110110xxxxxxxxxx", InstEmit.Ucvtf_V,         typeof(OpCodeSimd64));
            SetA64("0x101111001xxxxx111001xxxxxxxxxx", InstEmit.Ucvtf_V_Fixed,   typeof(OpCodeSimdShImm64));
            SetA64("0110111101xxxxxx111001xxxxxxxxxx", InstEmit.Ucvtf_V_Fixed,   typeof(OpCodeSimdShImm64));
            SetA64("0x101110<<1xxxxx000001xxxxxxxxxx", InstEmit.Uhadd_V,         typeof(OpCodeSimdReg64));
            SetA64("0x101110<<1xxxxx001001xxxxxxxxxx", InstEmit.Uhsub_V,         typeof(OpCodeSimdReg64));
            SetA64("0x101110<<1xxxxx011001xxxxxxxxxx", InstEmit.Umax_V,          typeof(OpCodeSimdReg64));
            SetA64("0x101110<<1xxxxx101001xxxxxxxxxx", InstEmit.Umaxp_V,         typeof(OpCodeSimdReg64));
            SetA64("001011100x110000101010xxxxxxxxxx", InstEmit.Umaxv_V,         typeof(OpCodeSimd64));
            SetA64("01101110<<110000101010xxxxxxxxxx", InstEmit.Umaxv_V,         typeof(OpCodeSimd64));
            SetA64("0x101110<<1xxxxx011011xxxxxxxxxx", InstEmit.Umin_V,          typeof(OpCodeSimdReg64));
            SetA64("0x101110<<1xxxxx101011xxxxxxxxxx", InstEmit.Uminp_V,         typeof(OpCodeSimdReg64));
            SetA64("001011100x110001101010xxxxxxxxxx", InstEmit.Uminv_V,         typeof(OpCodeSimd64));
            SetA64("01101110<<110001101010xxxxxxxxxx", InstEmit.Uminv_V,         typeof(OpCodeSimd64));
            SetA64("0x101110<<1xxxxx100000xxxxxxxxxx", InstEmit.Umlal_V,         typeof(OpCodeSimdReg64));
            SetA64("0x101111xxxxxxxx0010x0xxxxxxxxxx", InstEmit.Umlal_Ve,        typeof(OpCodeSimdRegElem64));
            SetA64("0x101110<<1xxxxx101000xxxxxxxxxx", InstEmit.Umlsl_V,         typeof(OpCodeSimdReg64));
            SetA64("0x101111xxxxxxxx0110x0xxxxxxxxxx", InstEmit.Umlsl_Ve,        typeof(OpCodeSimdRegElem64));
            SetA64("0x001110000xxxxx001111xxxxxxxxxx", InstEmit.Umov_S,          typeof(OpCodeSimdIns64));
            SetA64("0x101110<<1xxxxx110000xxxxxxxxxx", InstEmit.Umull_V,         typeof(OpCodeSimdReg64));
            SetA64("0x101111xxxxxxxx1010x0xxxxxxxxxx", InstEmit.Umull_Ve,        typeof(OpCodeSimdRegElem64));
            SetA64("01111110xx1xxxxx000011xxxxxxxxxx", InstEmit.Uqadd_S,         typeof(OpCodeSimdReg64));
            SetA64("0>101110<<1xxxxx000011xxxxxxxxxx", InstEmit.Uqadd_V,         typeof(OpCodeSimdReg64));
            SetA64("0>101110<<1xxxxx010111xxxxxxxxxx", InstEmit.Uqrshl_V,        typeof(OpCodeSimdReg64));
            SetA64("0111111100>>>xxx100111xxxxxxxxxx", InstEmit.Uqrshrn_S,       typeof(OpCodeSimdShImm64));
            SetA64("0x10111100>>>xxx100111xxxxxxxxxx", InstEmit.Uqrshrn_V,       typeof(OpCodeSimdShImm64));
            SetA64("0>101110<<1xxxxx010011xxxxxxxxxx", InstEmit.Uqshl_V,         typeof(OpCodeSimdReg64));
            SetA64("0111111100>>>xxx100101xxxxxxxxxx", InstEmit.Uqshrn_S,        typeof(OpCodeSimdShImm64));
            SetA64("0x10111100>>>xxx100101xxxxxxxxxx", InstEmit.Uqshrn_V,        typeof(OpCodeSimdShImm64));
            SetA64("01111110xx1xxxxx001011xxxxxxxxxx", InstEmit.Uqsub_S,         typeof(OpCodeSimdReg64));
            SetA64("0>101110<<1xxxxx001011xxxxxxxxxx", InstEmit.Uqsub_V,         typeof(OpCodeSimdReg64));
            SetA64("01111110<<100001010010xxxxxxxxxx", InstEmit.Uqxtn_S,         typeof(OpCodeSimd64));
            SetA64("0x101110<<100001010010xxxxxxxxxx", InstEmit.Uqxtn_V,         typeof(OpCodeSimd64));
            SetA64("0x101110<<1xxxxx000101xxxxxxxxxx", InstEmit.Urhadd_V,        typeof(OpCodeSimdReg64));
            SetA64("0>101110<<1xxxxx010101xxxxxxxxxx", InstEmit.Urshl_V,         typeof(OpCodeSimdReg64));
            SetA64("0111111101xxxxxx001001xxxxxxxxxx", InstEmit.Urshr_S,         typeof(OpCodeSimdShImm64));
            SetA64("0x10111100>>>xxx001001xxxxxxxxxx", InstEmit.Urshr_V,         typeof(OpCodeSimdShImm64));
            SetA64("0110111101xxxxxx001001xxxxxxxxxx", InstEmit.Urshr_V,         typeof(OpCodeSimdShImm64));
            SetA64("0111111101xxxxxx001101xxxxxxxxxx", InstEmit.Ursra_S,         typeof(OpCodeSimdShImm64));
            SetA64("0x10111100>>>xxx001101xxxxxxxxxx", InstEmit.Ursra_V,         typeof(OpCodeSimdShImm64));
            SetA64("0110111101xxxxxx001101xxxxxxxxxx", InstEmit.Ursra_V,         typeof(OpCodeSimdShImm64));
            SetA64("0>101110<<1xxxxx010001xxxxxxxxxx", InstEmit.Ushl_V,          typeof(OpCodeSimdReg64));
            SetA64("0x10111100>>>xxx101001xxxxxxxxxx", InstEmit.Ushll_V,         typeof(OpCodeSimdShImm64));
            SetA64("0111111101xxxxxx000001xxxxxxxxxx", InstEmit.Ushr_S,          typeof(OpCodeSimdShImm64));
            SetA64("0x10111100>>>xxx000001xxxxxxxxxx", InstEmit.Ushr_V,          typeof(OpCodeSimdShImm64));
            SetA64("0110111101xxxxxx000001xxxxxxxxxx", InstEmit.Ushr_V,          typeof(OpCodeSimdShImm64));
            SetA64("01111110xx100000001110xxxxxxxxxx", InstEmit.Usqadd_S,        typeof(OpCodeSimd64));
            SetA64("0>101110<<100000001110xxxxxxxxxx", InstEmit.Usqadd_V,        typeof(OpCodeSimd64));
            SetA64("0111111101xxxxxx000101xxxxxxxxxx", InstEmit.Usra_S,          typeof(OpCodeSimdShImm64));
            SetA64("0x10111100>>>xxx000101xxxxxxxxxx", InstEmit.Usra_V,          typeof(OpCodeSimdShImm64));
            SetA64("0110111101xxxxxx000101xxxxxxxxxx", InstEmit.Usra_V,          typeof(OpCodeSimdShImm64));
            SetA64("0x101110<<1xxxxx001000xxxxxxxxxx", InstEmit.Usubl_V,         typeof(OpCodeSimdReg64));
            SetA64("0x101110<<1xxxxx001100xxxxxxxxxx", InstEmit.Usubw_V,         typeof(OpCodeSimdReg64));
            SetA64("0>001110<<0xxxxx000110xxxxxxxxxx", InstEmit.Uzp1_V,          typeof(OpCodeSimdReg64));
            SetA64("0>001110<<0xxxxx010110xxxxxxxxxx", InstEmit.Uzp2_V,          typeof(OpCodeSimdReg64));
            SetA64("0x001110<<100001001010xxxxxxxxxx", InstEmit.Xtn_V,           typeof(OpCodeSimd64));
            SetA64("0>001110<<0xxxxx001110xxxxxxxxxx", InstEmit.Zip1_V,          typeof(OpCodeSimdReg64));
            SetA64("0>001110<<0xxxxx011110xxxxxxxxxx", InstEmit.Zip2_V,          typeof(OpCodeSimdReg64));
#endregion

            FillFastLookupTable(_instA32FastLookup, _allInstA32);
            FillFastLookupTable(_instT32FastLookup, _allInstT32);
            FillFastLookupTable(_instA64FastLookup, _allInstA64);
        }

        private static void SetA32(string encoding, InstEmitter emitter, Type type)
        {
            Set(encoding, new Inst(emitter, type), ExecutionMode.Aarch32Arm);
        }

        private static void SetT32(string encoding, InstEmitter emitter, Type type)
        {
            if (encoding.Length == 16)
            {
                encoding = "xxxxxxxxxxxxxxxx" + encoding;
            }

            Set(encoding, new Inst(emitter, type), ExecutionMode.Aarch32Thumb);
        }

        private static void SetA64(string encoding, InstEmitter emitter, Type type)
        {
            Set(encoding, new Inst(emitter, type), ExecutionMode.Aarch64);
        }

        private static void Set(string encoding, Inst inst, ExecutionMode mode)
        {
            int bit   = encoding.Length - 1;
            int value = 0;
            int xMask = 0;
            int xBits = 0;

            int[] xPos = new int[encoding.Length];

            int blacklisted = 0;

            for (int index = 0; index < encoding.Length; index++, bit--)
            {
                // Note: < and > are used on special encodings.
                // The < means that we should never have ALL bits with the '<' set.
                // So, when the encoding has <<, it means that 00, 01, and 10 are valid,
                // but not 11. <<< is 000, 001, ..., 110 but NOT 111, and so on...
                // For >, the invalid value is zero. So, for >> 01, 10 and 11 are valid,
                // but 00 isn't.
                char chr = encoding[index];

                if (chr == '1')
                {
                    value |= 1 << bit;
                }
                else if (chr == 'x')
                {
                    xMask |= 1 << bit;
                }
                else if (chr == '>')
                {
                    xPos[xBits++] = bit;
                }
                else if (chr == '<')
                {
                    xPos[xBits++] = bit;

                    blacklisted |= 1 << bit;
                }
                else if (chr != '0')
                {
                    throw new ArgumentException(nameof(encoding));
                }
            }

            xMask = ~xMask;

            if (xBits == 0)
            {
                InsertInst(xMask, value, inst, mode);

                return;
            }

            for (int index = 0; index < (1 << xBits); index++)
            {
                int mask = 0;

                for (int x = 0; x < xBits; x++)
                {
                    mask |= ((index >> x) & 1) << xPos[x];
                }

                if (mask != blacklisted)
                {
                    InsertInst(xMask, value | mask, inst, mode);
                }
            }
        }

        private static void InsertInst(int xMask, int value, Inst inst, ExecutionMode mode)
        {
            InstInfo info = new InstInfo(xMask, value, inst);

            switch (mode)
            {
                case ExecutionMode.Aarch32Arm:   _allInstA32.Add(info); break;
                case ExecutionMode.Aarch32Thumb: _allInstT32.Add(info); break;
                case ExecutionMode.Aarch64:      _allInstA64.Add(info); break;
            }
        }

        private static void FillFastLookupTable(InstInfo[][] table, List<InstInfo> allInsts)
        {
            List<InstInfo>[] tmp = new List<InstInfo>[FastLookupSize];

            for (int i = 0; i < FastLookupSize; i++)
            {
                tmp[i] = new List<InstInfo>();
            }

            foreach (InstInfo inst in allInsts)
            {
                int mask  = ToFastLookupIndex(inst.Mask);
                int value = ToFastLookupIndex(inst.Value);

                for (int i = 0; i < FastLookupSize; i++)
                {
                    if ((i & mask) == value)
                    {
                        tmp[i].Add(inst);
                    }
                }
            }

            for (int i = 0; i < FastLookupSize; i++)
            {
                table[i] = tmp[i].ToArray();
            }
        }

        public static Inst GetInstA32(int opCode)
        {
            return GetInstFromList(_instA32FastLookup[ToFastLookupIndex(opCode)], opCode);
        }

        public static Inst GetInstT32(int opCode)
        {
            return GetInstFromList(_instT32FastLookup[ToFastLookupIndex(opCode)], opCode);
        }

        public static Inst GetInstA64(int opCode)
        {
            return GetInstFromList(_instA64FastLookup[ToFastLookupIndex(opCode)], opCode);
        }

        private static int ToFastLookupIndex(int value)
        {
            return ((value >> 10) & 0x00F) | ((value >> 18) & 0xFF0);
        }

        private static Inst GetInstFromList(IEnumerable<InstInfo> instList, int opCode)
        {
            foreach (InstInfo node in instList)
            {
                if ((opCode & node.Mask) == node.Value)
                {
                    return node.Inst;
                }
            }

            return Inst.Undefined;
        }
    }
}
