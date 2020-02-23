using ARMeilleure.Instructions;
using ARMeilleure.State;
using System;
using System.Collections.Generic;

namespace ARMeilleure.Decoders
{
    static class OpCodeTable
    {
        private const int FastLookupSize = 0x1000;

        private struct InstInfo
        {
            public int Mask  { get; }
            public int Value { get; }

            public InstDescriptor Inst { get; }

            public Type Type { get; }

            public InstInfo(int mask, int value, InstDescriptor inst, Type type)
            {
                Mask  = mask;
                Value = value;
                Inst  = inst;
                Type  = type;
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
#region "OpCode Table (AArch64)"
            // Base
            SetA64("x0011010000xxxxx000000xxxxxxxxxx", InstName.Adc,             InstEmit.Adc,             typeof(OpCodeAluRs));
            SetA64("x0111010000xxxxx000000xxxxxxxxxx", InstName.Adcs,            InstEmit.Adcs,            typeof(OpCodeAluRs));
            SetA64("x00100010xxxxxxxxxxxxxxxxxxxxxxx", InstName.Add,             InstEmit.Add,             typeof(OpCodeAluImm));
            SetA64("00001011<<0xxxxx0xxxxxxxxxxxxxxx", InstName.Add,             InstEmit.Add,             typeof(OpCodeAluRs));
            SetA64("10001011<<0xxxxxxxxxxxxxxxxxxxxx", InstName.Add,             InstEmit.Add,             typeof(OpCodeAluRs));
            SetA64("x0001011001xxxxxxxx0xxxxxxxxxxxx", InstName.Add,             InstEmit.Add,             typeof(OpCodeAluRx));
            SetA64("x0001011001xxxxxxxx100xxxxxxxxxx", InstName.Add,             InstEmit.Add,             typeof(OpCodeAluRx));
            SetA64("x01100010xxxxxxxxxxxxxxxxxxxxxxx", InstName.Adds,            InstEmit.Adds,            typeof(OpCodeAluImm));
            SetA64("00101011<<0xxxxx0xxxxxxxxxxxxxxx", InstName.Adds,            InstEmit.Adds,            typeof(OpCodeAluRs));
            SetA64("10101011<<0xxxxxxxxxxxxxxxxxxxxx", InstName.Adds,            InstEmit.Adds,            typeof(OpCodeAluRs));
            SetA64("x0101011001xxxxxxxx0xxxxxxxxxxxx", InstName.Adds,            InstEmit.Adds,            typeof(OpCodeAluRx));
            SetA64("x0101011001xxxxxxxx100xxxxxxxxxx", InstName.Adds,            InstEmit.Adds,            typeof(OpCodeAluRx));
            SetA64("0xx10000xxxxxxxxxxxxxxxxxxxxxxxx", InstName.Adr,             InstEmit.Adr,             typeof(OpCodeAdr));
            SetA64("1xx10000xxxxxxxxxxxxxxxxxxxxxxxx", InstName.Adrp,            InstEmit.Adrp,            typeof(OpCodeAdr));
            SetA64("0001001000xxxxxxxxxxxxxxxxxxxxxx", InstName.And,             InstEmit.And,             typeof(OpCodeAluImm));
            SetA64("100100100xxxxxxxxxxxxxxxxxxxxxxx", InstName.And,             InstEmit.And,             typeof(OpCodeAluImm));
            SetA64("00001010xx0xxxxx0xxxxxxxxxxxxxxx", InstName.And,             InstEmit.And,             typeof(OpCodeAluRs));
            SetA64("10001010xx0xxxxxxxxxxxxxxxxxxxxx", InstName.And,             InstEmit.And,             typeof(OpCodeAluRs));
            SetA64("0111001000xxxxxxxxxxxxxxxxxxxxxx", InstName.Ands,            InstEmit.Ands,            typeof(OpCodeAluImm));
            SetA64("111100100xxxxxxxxxxxxxxxxxxxxxxx", InstName.Ands,            InstEmit.Ands,            typeof(OpCodeAluImm));
            SetA64("01101010xx0xxxxx0xxxxxxxxxxxxxxx", InstName.Ands,            InstEmit.Ands,            typeof(OpCodeAluRs));
            SetA64("11101010xx0xxxxxxxxxxxxxxxxxxxxx", InstName.Ands,            InstEmit.Ands,            typeof(OpCodeAluRs));
            SetA64("x0011010110xxxxx001010xxxxxxxxxx", InstName.Asrv,            InstEmit.Asrv,            typeof(OpCodeAluRs));
            SetA64("000101xxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.B,               InstEmit.B,               typeof(OpCodeBImmAl));
            SetA64("01010100xxxxxxxxxxxxxxxxxxx0xxxx", InstName.B_Cond,          InstEmit.B_Cond,          typeof(OpCodeBImmCond));
            SetA64("00110011000xxxxx0xxxxxxxxxxxxxxx", InstName.Bfm,             InstEmit.Bfm,             typeof(OpCodeBfm));
            SetA64("1011001101xxxxxxxxxxxxxxxxxxxxxx", InstName.Bfm,             InstEmit.Bfm,             typeof(OpCodeBfm));
            SetA64("00001010xx1xxxxx0xxxxxxxxxxxxxxx", InstName.Bic,             InstEmit.Bic,             typeof(OpCodeAluRs));
            SetA64("10001010xx1xxxxxxxxxxxxxxxxxxxxx", InstName.Bic,             InstEmit.Bic,             typeof(OpCodeAluRs));
            SetA64("01101010xx1xxxxx0xxxxxxxxxxxxxxx", InstName.Bics,            InstEmit.Bics,            typeof(OpCodeAluRs));
            SetA64("11101010xx1xxxxxxxxxxxxxxxxxxxxx", InstName.Bics,            InstEmit.Bics,            typeof(OpCodeAluRs));
            SetA64("100101xxxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Bl,              InstEmit.Bl,              typeof(OpCodeBImmAl));
            SetA64("1101011000111111000000xxxxx00000", InstName.Blr,             InstEmit.Blr,             typeof(OpCodeBReg));
            SetA64("1101011000011111000000xxxxx00000", InstName.Br,              InstEmit.Br,              typeof(OpCodeBReg));
            SetA64("11010100001xxxxxxxxxxxxxxxx00000", InstName.Brk,             InstEmit.Brk,             typeof(OpCodeException));
            SetA64("x0110101xxxxxxxxxxxxxxxxxxxxxxxx", InstName.Cbnz,            InstEmit.Cbnz,            typeof(OpCodeBImmCmp));
            SetA64("x0110100xxxxxxxxxxxxxxxxxxxxxxxx", InstName.Cbz,             InstEmit.Cbz,             typeof(OpCodeBImmCmp));
            SetA64("x0111010010xxxxxxxxx10xxxxx0xxxx", InstName.Ccmn,            InstEmit.Ccmn,            typeof(OpCodeCcmpImm));
            SetA64("x0111010010xxxxxxxxx00xxxxx0xxxx", InstName.Ccmn,            InstEmit.Ccmn,            typeof(OpCodeCcmpReg));
            SetA64("x1111010010xxxxxxxxx10xxxxx0xxxx", InstName.Ccmp,            InstEmit.Ccmp,            typeof(OpCodeCcmpImm));
            SetA64("x1111010010xxxxxxxxx00xxxxx0xxxx", InstName.Ccmp,            InstEmit.Ccmp,            typeof(OpCodeCcmpReg));
            SetA64("11010101000000110011xxxx01011111", InstName.Clrex,           InstEmit.Clrex,           typeof(OpCodeSystem));
            SetA64("x101101011000000000101xxxxxxxxxx", InstName.Cls,             InstEmit.Cls,             typeof(OpCodeAlu));
            SetA64("x101101011000000000100xxxxxxxxxx", InstName.Clz,             InstEmit.Clz,             typeof(OpCodeAlu));
            SetA64("00011010110xxxxx010000xxxxxxxxxx", InstName.Crc32b,          InstEmit.Crc32b,          typeof(OpCodeAluBinary));
            SetA64("00011010110xxxxx010001xxxxxxxxxx", InstName.Crc32h,          InstEmit.Crc32h,          typeof(OpCodeAluBinary));
            SetA64("00011010110xxxxx010010xxxxxxxxxx", InstName.Crc32w,          InstEmit.Crc32w,          typeof(OpCodeAluBinary));
            SetA64("10011010110xxxxx010011xxxxxxxxxx", InstName.Crc32x,          InstEmit.Crc32x,          typeof(OpCodeAluBinary));
            SetA64("00011010110xxxxx010100xxxxxxxxxx", InstName.Crc32cb,         InstEmit.Crc32cb,         typeof(OpCodeAluBinary));
            SetA64("00011010110xxxxx010101xxxxxxxxxx", InstName.Crc32ch,         InstEmit.Crc32ch,         typeof(OpCodeAluBinary));
            SetA64("00011010110xxxxx010110xxxxxxxxxx", InstName.Crc32cw,         InstEmit.Crc32cw,         typeof(OpCodeAluBinary));
            SetA64("10011010110xxxxx010111xxxxxxxxxx", InstName.Crc32cx,         InstEmit.Crc32cx,         typeof(OpCodeAluBinary));
            SetA64("x0011010100xxxxxxxxx00xxxxxxxxxx", InstName.Csel,            InstEmit.Csel,            typeof(OpCodeCsel));
            SetA64("x0011010100xxxxxxxxx01xxxxxxxxxx", InstName.Csinc,           InstEmit.Csinc,           typeof(OpCodeCsel));
            SetA64("x1011010100xxxxxxxxx00xxxxxxxxxx", InstName.Csinv,           InstEmit.Csinv,           typeof(OpCodeCsel));
            SetA64("x1011010100xxxxxxxxx01xxxxxxxxxx", InstName.Csneg,           InstEmit.Csneg,           typeof(OpCodeCsel));
            SetA64("11010101000000110011xxxx10111111", InstName.Dmb,             InstEmit.Dmb,             typeof(OpCodeSystem));
            SetA64("11010101000000110011xxxx10011111", InstName.Dsb,             InstEmit.Dsb,             typeof(OpCodeSystem));
            SetA64("01001010xx1xxxxx0xxxxxxxxxxxxxxx", InstName.Eon,             InstEmit.Eon,             typeof(OpCodeAluRs));
            SetA64("11001010xx1xxxxxxxxxxxxxxxxxxxxx", InstName.Eon,             InstEmit.Eon,             typeof(OpCodeAluRs));
            SetA64("0101001000xxxxxxxxxxxxxxxxxxxxxx", InstName.Eor,             InstEmit.Eor,             typeof(OpCodeAluImm));
            SetA64("110100100xxxxxxxxxxxxxxxxxxxxxxx", InstName.Eor,             InstEmit.Eor,             typeof(OpCodeAluImm));
            SetA64("01001010xx0xxxxx0xxxxxxxxxxxxxxx", InstName.Eor,             InstEmit.Eor,             typeof(OpCodeAluRs));
            SetA64("11001010xx0xxxxxxxxxxxxxxxxxxxxx", InstName.Eor,             InstEmit.Eor,             typeof(OpCodeAluRs));
            SetA64("00010011100xxxxx0xxxxxxxxxxxxxxx", InstName.Extr,            InstEmit.Extr,            typeof(OpCodeAluRs));
            SetA64("10010011110xxxxxxxxxxxxxxxxxxxxx", InstName.Extr,            InstEmit.Extr,            typeof(OpCodeAluRs));
            SetA64("11010101000000110010xxxxxxx11111", InstName.Hint,            InstEmit.Hint,            typeof(OpCodeSystem));
            SetA64("11010101000000110011xxxx11011111", InstName.Isb,             InstEmit.Isb,             typeof(OpCodeSystem));
            SetA64("xx001000110xxxxx1xxxxxxxxxxxxxxx", InstName.Ldar,            InstEmit.Ldar,            typeof(OpCodeMemEx));
            SetA64("1x001000011xxxxx1xxxxxxxxxxxxxxx", InstName.Ldaxp,           InstEmit.Ldaxp,           typeof(OpCodeMemEx));
            SetA64("xx001000010xxxxx1xxxxxxxxxxxxxxx", InstName.Ldaxr,           InstEmit.Ldaxr,           typeof(OpCodeMemEx));
            SetA64("<<10100xx1xxxxxxxxxxxxxxxxxxxxxx", InstName.Ldp,             InstEmit.Ldp,             typeof(OpCodeMemPair));
            SetA64("xx111000010xxxxxxxxxxxxxxxxxxxxx", InstName.Ldr,             InstEmit.Ldr,             typeof(OpCodeMemImm));
            SetA64("xx11100101xxxxxxxxxxxxxxxxxxxxxx", InstName.Ldr,             InstEmit.Ldr,             typeof(OpCodeMemImm));
            SetA64("xx111000011xxxxxxxxx10xxxxxxxxxx", InstName.Ldr,             InstEmit.Ldr,             typeof(OpCodeMemReg));
            SetA64("xx011000xxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ldr_Literal,     InstEmit.Ldr_Literal,     typeof(OpCodeMemLit));
            SetA64("0x1110001x0xxxxxxxxxxxxxxxxxxxxx", InstName.Ldrs,            InstEmit.Ldrs,            typeof(OpCodeMemImm));
            SetA64("0x1110011xxxxxxxxxxxxxxxxxxxxxxx", InstName.Ldrs,            InstEmit.Ldrs,            typeof(OpCodeMemImm));
            SetA64("10111000100xxxxxxxxxxxxxxxxxxxxx", InstName.Ldrs,            InstEmit.Ldrs,            typeof(OpCodeMemImm));
            SetA64("1011100110xxxxxxxxxxxxxxxxxxxxxx", InstName.Ldrs,            InstEmit.Ldrs,            typeof(OpCodeMemImm));
            SetA64("0x1110001x1xxxxxxxxx10xxxxxxxxxx", InstName.Ldrs,            InstEmit.Ldrs,            typeof(OpCodeMemReg));
            SetA64("10111000101xxxxxxxxx10xxxxxxxxxx", InstName.Ldrs,            InstEmit.Ldrs,            typeof(OpCodeMemReg));
            SetA64("xx001000010xxxxx0xxxxxxxxxxxxxxx", InstName.Ldxr,            InstEmit.Ldxr,            typeof(OpCodeMemEx));
            SetA64("1x001000011xxxxx0xxxxxxxxxxxxxxx", InstName.Ldxp,            InstEmit.Ldxp,            typeof(OpCodeMemEx));
            SetA64("x0011010110xxxxx001000xxxxxxxxxx", InstName.Lslv,            InstEmit.Lslv,            typeof(OpCodeAluRs));
            SetA64("x0011010110xxxxx001001xxxxxxxxxx", InstName.Lsrv,            InstEmit.Lsrv,            typeof(OpCodeAluRs));
            SetA64("x0011011000xxxxx0xxxxxxxxxxxxxxx", InstName.Madd,            InstEmit.Madd,            typeof(OpCodeMul));
            SetA64("0111001010xxxxxxxxxxxxxxxxxxxxxx", InstName.Movk,            InstEmit.Movk,            typeof(OpCodeMov));
            SetA64("111100101xxxxxxxxxxxxxxxxxxxxxxx", InstName.Movk,            InstEmit.Movk,            typeof(OpCodeMov));
            SetA64("0001001010xxxxxxxxxxxxxxxxxxxxxx", InstName.Movn,            InstEmit.Movn,            typeof(OpCodeMov));
            SetA64("100100101xxxxxxxxxxxxxxxxxxxxxxx", InstName.Movn,            InstEmit.Movn,            typeof(OpCodeMov));
            SetA64("0101001010xxxxxxxxxxxxxxxxxxxxxx", InstName.Movz,            InstEmit.Movz,            typeof(OpCodeMov));
            SetA64("110100101xxxxxxxxxxxxxxxxxxxxxxx", InstName.Movz,            InstEmit.Movz,            typeof(OpCodeMov));
            SetA64("110101010011xxxxxxxxxxxxxxxxxxxx", InstName.Mrs,             InstEmit.Mrs,             typeof(OpCodeSystem));
            SetA64("110101010001xxxxxxxxxxxxxxxxxxxx", InstName.Msr,             InstEmit.Msr,             typeof(OpCodeSystem));
            SetA64("x0011011000xxxxx1xxxxxxxxxxxxxxx", InstName.Msub,            InstEmit.Msub,            typeof(OpCodeMul));
            SetA64("11010101000000110010000000011111", InstName.Nop,             InstEmit.Nop,             typeof(OpCodeSystem));
            SetA64("00101010xx1xxxxx0xxxxxxxxxxxxxxx", InstName.Orn,             InstEmit.Orn,             typeof(OpCodeAluRs));
            SetA64("10101010xx1xxxxxxxxxxxxxxxxxxxxx", InstName.Orn,             InstEmit.Orn,             typeof(OpCodeAluRs));
            SetA64("0011001000xxxxxxxxxxxxxxxxxxxxxx", InstName.Orr,             InstEmit.Orr,             typeof(OpCodeAluImm));
            SetA64("101100100xxxxxxxxxxxxxxxxxxxxxxx", InstName.Orr,             InstEmit.Orr,             typeof(OpCodeAluImm));
            SetA64("00101010xx0xxxxx0xxxxxxxxxxxxxxx", InstName.Orr,             InstEmit.Orr,             typeof(OpCodeAluRs));
            SetA64("10101010xx0xxxxxxxxxxxxxxxxxxxxx", InstName.Orr,             InstEmit.Orr,             typeof(OpCodeAluRs));
            SetA64("1111100110xxxxxxxxxxxxxxxxxxxxxx", InstName.Pfrm,            InstEmit.Pfrm,            typeof(OpCodeMemImm));
            SetA64("11111000100xxxxxxxxx00xxxxxxxxxx", InstName.Pfrm,            InstEmit.Pfrm,            typeof(OpCodeMemImm));
            SetA64("11011000xxxxxxxxxxxxxxxxxxxxxxxx", InstName.Pfrm,            InstEmit.Pfrm,            typeof(OpCodeMemLit));
            SetA64("x101101011000000000000xxxxxxxxxx", InstName.Rbit,            InstEmit.Rbit,            typeof(OpCodeAlu));
            SetA64("1101011001011111000000xxxxx00000", InstName.Ret,             InstEmit.Ret,             typeof(OpCodeBReg));
            SetA64("x101101011000000000001xxxxxxxxxx", InstName.Rev16,           InstEmit.Rev16,           typeof(OpCodeAlu));
            SetA64("x101101011000000000010xxxxxxxxxx", InstName.Rev32,           InstEmit.Rev32,           typeof(OpCodeAlu));
            SetA64("1101101011000000000011xxxxxxxxxx", InstName.Rev64,           InstEmit.Rev64,           typeof(OpCodeAlu));
            SetA64("x0011010110xxxxx001011xxxxxxxxxx", InstName.Rorv,            InstEmit.Rorv,            typeof(OpCodeAluRs));
            SetA64("x1011010000xxxxx000000xxxxxxxxxx", InstName.Sbc,             InstEmit.Sbc,             typeof(OpCodeAluRs));
            SetA64("x1111010000xxxxx000000xxxxxxxxxx", InstName.Sbcs,            InstEmit.Sbcs,            typeof(OpCodeAluRs));
            SetA64("00010011000xxxxx0xxxxxxxxxxxxxxx", InstName.Sbfm,            InstEmit.Sbfm,            typeof(OpCodeBfm));
            SetA64("1001001101xxxxxxxxxxxxxxxxxxxxxx", InstName.Sbfm,            InstEmit.Sbfm,            typeof(OpCodeBfm));
            SetA64("x0011010110xxxxx000011xxxxxxxxxx", InstName.Sdiv,            InstEmit.Sdiv,            typeof(OpCodeAluBinary));
            SetA64("10011011001xxxxx0xxxxxxxxxxxxxxx", InstName.Smaddl,          InstEmit.Smaddl,          typeof(OpCodeMul));
            SetA64("10011011001xxxxx1xxxxxxxxxxxxxxx", InstName.Smsubl,          InstEmit.Smsubl,          typeof(OpCodeMul));
            SetA64("10011011010xxxxx0xxxxxxxxxxxxxxx", InstName.Smulh,           InstEmit.Smulh,           typeof(OpCodeMul));
            SetA64("xx001000100xxxxx1xxxxxxxxxxxxxxx", InstName.Stlr,            InstEmit.Stlr,            typeof(OpCodeMemEx));
            SetA64("1x001000001xxxxx1xxxxxxxxxxxxxxx", InstName.Stlxp,           InstEmit.Stlxp,           typeof(OpCodeMemEx));
            SetA64("xx001000000xxxxx1xxxxxxxxxxxxxxx", InstName.Stlxr,           InstEmit.Stlxr,           typeof(OpCodeMemEx));
            SetA64("x010100xx0xxxxxxxxxxxxxxxxxxxxxx", InstName.Stp,             InstEmit.Stp,             typeof(OpCodeMemPair));
            SetA64("xx111000000xxxxxxxxxxxxxxxxxxxxx", InstName.Str,             InstEmit.Str,             typeof(OpCodeMemImm));
            SetA64("xx11100100xxxxxxxxxxxxxxxxxxxxxx", InstName.Str,             InstEmit.Str,             typeof(OpCodeMemImm));
            SetA64("xx111000001xxxxxxxxx10xxxxxxxxxx", InstName.Str,             InstEmit.Str,             typeof(OpCodeMemReg));
            SetA64("1x001000001xxxxx0xxxxxxxxxxxxxxx", InstName.Stxp,            InstEmit.Stxp,            typeof(OpCodeMemEx));
            SetA64("xx001000000xxxxx0xxxxxxxxxxxxxxx", InstName.Stxr,            InstEmit.Stxr,            typeof(OpCodeMemEx));
            SetA64("x10100010xxxxxxxxxxxxxxxxxxxxxxx", InstName.Sub,             InstEmit.Sub,             typeof(OpCodeAluImm));
            SetA64("01001011<<0xxxxx0xxxxxxxxxxxxxxx", InstName.Sub,             InstEmit.Sub,             typeof(OpCodeAluRs));
            SetA64("11001011<<0xxxxxxxxxxxxxxxxxxxxx", InstName.Sub,             InstEmit.Sub,             typeof(OpCodeAluRs));
            SetA64("x1001011001xxxxxxxx0xxxxxxxxxxxx", InstName.Sub,             InstEmit.Sub,             typeof(OpCodeAluRx));
            SetA64("x1001011001xxxxxxxx100xxxxxxxxxx", InstName.Sub,             InstEmit.Sub,             typeof(OpCodeAluRx));
            SetA64("x11100010xxxxxxxxxxxxxxxxxxxxxxx", InstName.Subs,            InstEmit.Subs,            typeof(OpCodeAluImm));
            SetA64("01101011<<0xxxxx0xxxxxxxxxxxxxxx", InstName.Subs,            InstEmit.Subs,            typeof(OpCodeAluRs));
            SetA64("11101011<<0xxxxxxxxxxxxxxxxxxxxx", InstName.Subs,            InstEmit.Subs,            typeof(OpCodeAluRs));
            SetA64("x1101011001xxxxxxxx0xxxxxxxxxxxx", InstName.Subs,            InstEmit.Subs,            typeof(OpCodeAluRx));
            SetA64("x1101011001xxxxxxxx100xxxxxxxxxx", InstName.Subs,            InstEmit.Subs,            typeof(OpCodeAluRx));
            SetA64("11010100000xxxxxxxxxxxxxxxx00001", InstName.Svc,             InstEmit.Svc,             typeof(OpCodeException));
            SetA64("1101010100001xxxxxxxxxxxxxxxxxxx", InstName.Sys,             InstEmit.Sys,             typeof(OpCodeSystem));
            SetA64("x0110111xxxxxxxxxxxxxxxxxxxxxxxx", InstName.Tbnz,            InstEmit.Tbnz,            typeof(OpCodeBImmTest));
            SetA64("x0110110xxxxxxxxxxxxxxxxxxxxxxxx", InstName.Tbz,             InstEmit.Tbz,             typeof(OpCodeBImmTest));
            SetA64("01010011000xxxxx0xxxxxxxxxxxxxxx", InstName.Ubfm,            InstEmit.Ubfm,            typeof(OpCodeBfm));
            SetA64("1101001101xxxxxxxxxxxxxxxxxxxxxx", InstName.Ubfm,            InstEmit.Ubfm,            typeof(OpCodeBfm));
            SetA64("x0011010110xxxxx000010xxxxxxxxxx", InstName.Udiv,            InstEmit.Udiv,            typeof(OpCodeAluBinary));
            SetA64("10011011101xxxxx0xxxxxxxxxxxxxxx", InstName.Umaddl,          InstEmit.Umaddl,          typeof(OpCodeMul));
            SetA64("10011011101xxxxx1xxxxxxxxxxxxxxx", InstName.Umsubl,          InstEmit.Umsubl,          typeof(OpCodeMul));
            SetA64("10011011110xxxxx0xxxxxxxxxxxxxxx", InstName.Umulh,           InstEmit.Umulh,           typeof(OpCodeMul));

            // FP & SIMD
            SetA64("0101111011100000101110xxxxxxxxxx", InstName.Abs_S,           InstEmit.Abs_S,           typeof(OpCodeSimd));
            SetA64("0>001110<<100000101110xxxxxxxxxx", InstName.Abs_V,           InstEmit.Abs_V,           typeof(OpCodeSimd));
            SetA64("01011110111xxxxx100001xxxxxxxxxx", InstName.Add_S,           InstEmit.Add_S,           typeof(OpCodeSimdReg));
            SetA64("0>001110<<1xxxxx100001xxxxxxxxxx", InstName.Add_V,           InstEmit.Add_V,           typeof(OpCodeSimdReg));
            SetA64("0x001110<<1xxxxx010000xxxxxxxxxx", InstName.Addhn_V,         InstEmit.Addhn_V,         typeof(OpCodeSimdReg));
            SetA64("0101111011110001101110xxxxxxxxxx", InstName.Addp_S,          InstEmit.Addp_S,          typeof(OpCodeSimd));
            SetA64("0>001110<<1xxxxx101111xxxxxxxxxx", InstName.Addp_V,          InstEmit.Addp_V,          typeof(OpCodeSimdReg));
            SetA64("000011100x110001101110xxxxxxxxxx", InstName.Addv_V,          InstEmit.Addv_V,          typeof(OpCodeSimd));
            SetA64("01001110<<110001101110xxxxxxxxxx", InstName.Addv_V,          InstEmit.Addv_V,          typeof(OpCodeSimd));
            SetA64("0100111000101000010110xxxxxxxxxx", InstName.Aesd_V,          InstEmit.Aesd_V,          typeof(OpCodeSimd));
            SetA64("0100111000101000010010xxxxxxxxxx", InstName.Aese_V,          InstEmit.Aese_V,          typeof(OpCodeSimd));
            SetA64("0100111000101000011110xxxxxxxxxx", InstName.Aesimc_V,        InstEmit.Aesimc_V,        typeof(OpCodeSimd));
            SetA64("0100111000101000011010xxxxxxxxxx", InstName.Aesmc_V,         InstEmit.Aesmc_V,         typeof(OpCodeSimd));
            SetA64("0x001110001xxxxx000111xxxxxxxxxx", InstName.And_V,           InstEmit.And_V,           typeof(OpCodeSimdReg));
            SetA64("0x001110011xxxxx000111xxxxxxxxxx", InstName.Bic_V,           InstEmit.Bic_V,           typeof(OpCodeSimdReg));
            SetA64("0x10111100000xxx0xx101xxxxxxxxxx", InstName.Bic_Vi,          InstEmit.Bic_Vi,          typeof(OpCodeSimdImm));
            SetA64("0x10111100000xxx10x101xxxxxxxxxx", InstName.Bic_Vi,          InstEmit.Bic_Vi,          typeof(OpCodeSimdImm));
            SetA64("0x101110111xxxxx000111xxxxxxxxxx", InstName.Bif_V,           InstEmit.Bif_V,           typeof(OpCodeSimdReg));
            SetA64("0x101110101xxxxx000111xxxxxxxxxx", InstName.Bit_V,           InstEmit.Bit_V,           typeof(OpCodeSimdReg));
            SetA64("0x101110011xxxxx000111xxxxxxxxxx", InstName.Bsl_V,           InstEmit.Bsl_V,           typeof(OpCodeSimdReg));
            SetA64("0x001110<<100000010010xxxxxxxxxx", InstName.Cls_V,           InstEmit.Cls_V,           typeof(OpCodeSimd));
            SetA64("0x101110<<100000010010xxxxxxxxxx", InstName.Clz_V,           InstEmit.Clz_V,           typeof(OpCodeSimd));
            SetA64("01111110111xxxxx100011xxxxxxxxxx", InstName.Cmeq_S,          InstEmit.Cmeq_S,          typeof(OpCodeSimdReg));
            SetA64("0101111011100000100110xxxxxxxxxx", InstName.Cmeq_S,          InstEmit.Cmeq_S,          typeof(OpCodeSimd));
            SetA64("0>101110<<1xxxxx100011xxxxxxxxxx", InstName.Cmeq_V,          InstEmit.Cmeq_V,          typeof(OpCodeSimdReg));
            SetA64("0>001110<<100000100110xxxxxxxxxx", InstName.Cmeq_V,          InstEmit.Cmeq_V,          typeof(OpCodeSimd));
            SetA64("01011110111xxxxx001111xxxxxxxxxx", InstName.Cmge_S,          InstEmit.Cmge_S,          typeof(OpCodeSimdReg));
            SetA64("0111111011100000100010xxxxxxxxxx", InstName.Cmge_S,          InstEmit.Cmge_S,          typeof(OpCodeSimd));
            SetA64("0>001110<<1xxxxx001111xxxxxxxxxx", InstName.Cmge_V,          InstEmit.Cmge_V,          typeof(OpCodeSimdReg));
            SetA64("0>101110<<100000100010xxxxxxxxxx", InstName.Cmge_V,          InstEmit.Cmge_V,          typeof(OpCodeSimd));
            SetA64("01011110111xxxxx001101xxxxxxxxxx", InstName.Cmgt_S,          InstEmit.Cmgt_S,          typeof(OpCodeSimdReg));
            SetA64("0101111011100000100010xxxxxxxxxx", InstName.Cmgt_S,          InstEmit.Cmgt_S,          typeof(OpCodeSimd));
            SetA64("0>001110<<1xxxxx001101xxxxxxxxxx", InstName.Cmgt_V,          InstEmit.Cmgt_V,          typeof(OpCodeSimdReg));
            SetA64("0>001110<<100000100010xxxxxxxxxx", InstName.Cmgt_V,          InstEmit.Cmgt_V,          typeof(OpCodeSimd));
            SetA64("01111110111xxxxx001101xxxxxxxxxx", InstName.Cmhi_S,          InstEmit.Cmhi_S,          typeof(OpCodeSimdReg));
            SetA64("0>101110<<1xxxxx001101xxxxxxxxxx", InstName.Cmhi_V,          InstEmit.Cmhi_V,          typeof(OpCodeSimdReg));
            SetA64("01111110111xxxxx001111xxxxxxxxxx", InstName.Cmhs_S,          InstEmit.Cmhs_S,          typeof(OpCodeSimdReg));
            SetA64("0>101110<<1xxxxx001111xxxxxxxxxx", InstName.Cmhs_V,          InstEmit.Cmhs_V,          typeof(OpCodeSimdReg));
            SetA64("0111111011100000100110xxxxxxxxxx", InstName.Cmle_S,          InstEmit.Cmle_S,          typeof(OpCodeSimd));
            SetA64("0>101110<<100000100110xxxxxxxxxx", InstName.Cmle_V,          InstEmit.Cmle_V,          typeof(OpCodeSimd));
            SetA64("0101111011100000101010xxxxxxxxxx", InstName.Cmlt_S,          InstEmit.Cmlt_S,          typeof(OpCodeSimd));
            SetA64("0>001110<<100000101010xxxxxxxxxx", InstName.Cmlt_V,          InstEmit.Cmlt_V,          typeof(OpCodeSimd));
            SetA64("01011110111xxxxx100011xxxxxxxxxx", InstName.Cmtst_S,         InstEmit.Cmtst_S,         typeof(OpCodeSimdReg));
            SetA64("0>001110<<1xxxxx100011xxxxxxxxxx", InstName.Cmtst_V,         InstEmit.Cmtst_V,         typeof(OpCodeSimdReg));
            SetA64("0x00111000100000010110xxxxxxxxxx", InstName.Cnt_V,           InstEmit.Cnt_V,           typeof(OpCodeSimd));
            SetA64("0>001110000x<>>>000011xxxxxxxxxx", InstName.Dup_Gp,          InstEmit.Dup_Gp,          typeof(OpCodeSimdIns));
            SetA64("01011110000xxxxx000001xxxxxxxxxx", InstName.Dup_S,           InstEmit.Dup_S,           typeof(OpCodeSimdIns));
            SetA64("0>001110000x<>>>000001xxxxxxxxxx", InstName.Dup_V,           InstEmit.Dup_V,           typeof(OpCodeSimdIns));
            SetA64("0x101110001xxxxx000111xxxxxxxxxx", InstName.Eor_V,           InstEmit.Eor_V,           typeof(OpCodeSimdReg));
            SetA64("0>101110000xxxxx0<xxx0xxxxxxxxxx", InstName.Ext_V,           InstEmit.Ext_V,           typeof(OpCodeSimdExt));
            SetA64("011111101x1xxxxx110101xxxxxxxxxx", InstName.Fabd_S,          InstEmit.Fabd_S,          typeof(OpCodeSimdReg));
            SetA64("0>1011101<1xxxxx110101xxxxxxxxxx", InstName.Fabd_V,          InstEmit.Fabd_V,          typeof(OpCodeSimdReg));
            SetA64("000111100x100000110000xxxxxxxxxx", InstName.Fabs_S,          InstEmit.Fabs_S,          typeof(OpCodeSimd));
            SetA64("0>0011101<100000111110xxxxxxxxxx", InstName.Fabs_V,          InstEmit.Fabs_V,          typeof(OpCodeSimd));
            SetA64("000111100x1xxxxx001010xxxxxxxxxx", InstName.Fadd_S,          InstEmit.Fadd_S,          typeof(OpCodeSimdReg));
            SetA64("0>0011100<1xxxxx110101xxxxxxxxxx", InstName.Fadd_V,          InstEmit.Fadd_V,          typeof(OpCodeSimdReg));
            SetA64("011111100x110000110110xxxxxxxxxx", InstName.Faddp_S,         InstEmit.Faddp_S,         typeof(OpCodeSimd));
            SetA64("0>1011100<1xxxxx110101xxxxxxxxxx", InstName.Faddp_V,         InstEmit.Faddp_V,         typeof(OpCodeSimdReg));
            SetA64("000111100x1xxxxxxxxx01xxxxx0xxxx", InstName.Fccmp_S,         InstEmit.Fccmp_S,         typeof(OpCodeSimdFcond));
            SetA64("000111100x1xxxxxxxxx01xxxxx1xxxx", InstName.Fccmpe_S,        InstEmit.Fccmpe_S,        typeof(OpCodeSimdFcond));
            SetA64("010111100x1xxxxx111001xxxxxxxxxx", InstName.Fcmeq_S,         InstEmit.Fcmeq_S,         typeof(OpCodeSimdReg));
            SetA64("010111101x100000110110xxxxxxxxxx", InstName.Fcmeq_S,         InstEmit.Fcmeq_S,         typeof(OpCodeSimd));
            SetA64("0>0011100<1xxxxx111001xxxxxxxxxx", InstName.Fcmeq_V,         InstEmit.Fcmeq_V,         typeof(OpCodeSimdReg));
            SetA64("0>0011101<100000110110xxxxxxxxxx", InstName.Fcmeq_V,         InstEmit.Fcmeq_V,         typeof(OpCodeSimd));
            SetA64("011111100x1xxxxx111001xxxxxxxxxx", InstName.Fcmge_S,         InstEmit.Fcmge_S,         typeof(OpCodeSimdReg));
            SetA64("011111101x100000110010xxxxxxxxxx", InstName.Fcmge_S,         InstEmit.Fcmge_S,         typeof(OpCodeSimd));
            SetA64("0>1011100<1xxxxx111001xxxxxxxxxx", InstName.Fcmge_V,         InstEmit.Fcmge_V,         typeof(OpCodeSimdReg));
            SetA64("0>1011101<100000110010xxxxxxxxxx", InstName.Fcmge_V,         InstEmit.Fcmge_V,         typeof(OpCodeSimd));
            SetA64("011111101x1xxxxx111001xxxxxxxxxx", InstName.Fcmgt_S,         InstEmit.Fcmgt_S,         typeof(OpCodeSimdReg));
            SetA64("010111101x100000110010xxxxxxxxxx", InstName.Fcmgt_S,         InstEmit.Fcmgt_S,         typeof(OpCodeSimd));
            SetA64("0>1011101<1xxxxx111001xxxxxxxxxx", InstName.Fcmgt_V,         InstEmit.Fcmgt_V,         typeof(OpCodeSimdReg));
            SetA64("0>0011101<100000110010xxxxxxxxxx", InstName.Fcmgt_V,         InstEmit.Fcmgt_V,         typeof(OpCodeSimd));
            SetA64("011111101x100000110110xxxxxxxxxx", InstName.Fcmle_S,         InstEmit.Fcmle_S,         typeof(OpCodeSimd));
            SetA64("0>1011101<100000110110xxxxxxxxxx", InstName.Fcmle_V,         InstEmit.Fcmle_V,         typeof(OpCodeSimd));
            SetA64("010111101x100000111010xxxxxxxxxx", InstName.Fcmlt_S,         InstEmit.Fcmlt_S,         typeof(OpCodeSimd));
            SetA64("0>0011101<100000111010xxxxxxxxxx", InstName.Fcmlt_V,         InstEmit.Fcmlt_V,         typeof(OpCodeSimd));
            SetA64("000111100x1xxxxx001000xxxxx0x000", InstName.Fcmp_S,          InstEmit.Fcmp_S,          typeof(OpCodeSimdReg));
            SetA64("000111100x1xxxxx001000xxxxx1x000", InstName.Fcmpe_S,         InstEmit.Fcmpe_S,         typeof(OpCodeSimdReg));
            SetA64("000111100x1xxxxxxxxx11xxxxxxxxxx", InstName.Fcsel_S,         InstEmit.Fcsel_S,         typeof(OpCodeSimdFcond));
            SetA64("00011110xx10001xx10000xxxxxxxxxx", InstName.Fcvt_S,          InstEmit.Fcvt_S,          typeof(OpCodeSimd));
            SetA64("x00111100x100100000000xxxxxxxxxx", InstName.Fcvtas_Gp,       InstEmit.Fcvtas_Gp,       typeof(OpCodeSimdCvt));
            SetA64("x00111100x100101000000xxxxxxxxxx", InstName.Fcvtau_Gp,       InstEmit.Fcvtau_Gp,       typeof(OpCodeSimdCvt));
            SetA64("0x0011100x100001011110xxxxxxxxxx", InstName.Fcvtl_V,         InstEmit.Fcvtl_V,         typeof(OpCodeSimd));
            SetA64("x00111100x110000000000xxxxxxxxxx", InstName.Fcvtms_Gp,       InstEmit.Fcvtms_Gp,       typeof(OpCodeSimdCvt));
            SetA64("x00111100x110001000000xxxxxxxxxx", InstName.Fcvtmu_Gp,       InstEmit.Fcvtmu_Gp,       typeof(OpCodeSimdCvt));
            SetA64("0x0011100x100001011010xxxxxxxxxx", InstName.Fcvtn_V,         InstEmit.Fcvtn_V,         typeof(OpCodeSimd));
            SetA64("010111100x100001101010xxxxxxxxxx", InstName.Fcvtns_S,        InstEmit.Fcvtns_S,        typeof(OpCodeSimd));
            SetA64("0>0011100<100001101010xxxxxxxxxx", InstName.Fcvtns_V,        InstEmit.Fcvtns_V,        typeof(OpCodeSimd));
            SetA64("011111100x100001101010xxxxxxxxxx", InstName.Fcvtnu_S,        InstEmit.Fcvtnu_S,        typeof(OpCodeSimd));
            SetA64("0>1011100<100001101010xxxxxxxxxx", InstName.Fcvtnu_V,        InstEmit.Fcvtnu_V,        typeof(OpCodeSimd));
            SetA64("x00111100x101000000000xxxxxxxxxx", InstName.Fcvtps_Gp,       InstEmit.Fcvtps_Gp,       typeof(OpCodeSimdCvt));
            SetA64("x00111100x101001000000xxxxxxxxxx", InstName.Fcvtpu_Gp,       InstEmit.Fcvtpu_Gp,       typeof(OpCodeSimdCvt));
            SetA64("x00111100x111000000000xxxxxxxxxx", InstName.Fcvtzs_Gp,       InstEmit.Fcvtzs_Gp,       typeof(OpCodeSimdCvt));
            SetA64(">00111100x011000>xxxxxxxxxxxxxxx", InstName.Fcvtzs_Gp_Fixed, InstEmit.Fcvtzs_Gp_Fixed, typeof(OpCodeSimdCvt));
            SetA64("010111101x100001101110xxxxxxxxxx", InstName.Fcvtzs_S,        InstEmit.Fcvtzs_S,        typeof(OpCodeSimd));
            SetA64("0>0011101<100001101110xxxxxxxxxx", InstName.Fcvtzs_V,        InstEmit.Fcvtzs_V,        typeof(OpCodeSimd));
            SetA64("0x001111001xxxxx111111xxxxxxxxxx", InstName.Fcvtzs_V_Fixed,  InstEmit.Fcvtzs_V_Fixed,  typeof(OpCodeSimdShImm));
            SetA64("0100111101xxxxxx111111xxxxxxxxxx", InstName.Fcvtzs_V_Fixed,  InstEmit.Fcvtzs_V_Fixed,  typeof(OpCodeSimdShImm));
            SetA64("x00111100x111001000000xxxxxxxxxx", InstName.Fcvtzu_Gp,       InstEmit.Fcvtzu_Gp,       typeof(OpCodeSimdCvt));
            SetA64(">00111100x011001>xxxxxxxxxxxxxxx", InstName.Fcvtzu_Gp_Fixed, InstEmit.Fcvtzu_Gp_Fixed, typeof(OpCodeSimdCvt));
            SetA64("011111101x100001101110xxxxxxxxxx", InstName.Fcvtzu_S,        InstEmit.Fcvtzu_S,        typeof(OpCodeSimd));
            SetA64("0>1011101<100001101110xxxxxxxxxx", InstName.Fcvtzu_V,        InstEmit.Fcvtzu_V,        typeof(OpCodeSimd));
            SetA64("0x101111001xxxxx111111xxxxxxxxxx", InstName.Fcvtzu_V_Fixed,  InstEmit.Fcvtzu_V_Fixed,  typeof(OpCodeSimdShImm));
            SetA64("0110111101xxxxxx111111xxxxxxxxxx", InstName.Fcvtzu_V_Fixed,  InstEmit.Fcvtzu_V_Fixed,  typeof(OpCodeSimdShImm));
            SetA64("000111100x1xxxxx000110xxxxxxxxxx", InstName.Fdiv_S,          InstEmit.Fdiv_S,          typeof(OpCodeSimdReg));
            SetA64("0>1011100<1xxxxx111111xxxxxxxxxx", InstName.Fdiv_V,          InstEmit.Fdiv_V,          typeof(OpCodeSimdReg));
            SetA64("000111110x0xxxxx0xxxxxxxxxxxxxxx", InstName.Fmadd_S,         InstEmit.Fmadd_S,         typeof(OpCodeSimdReg));
            SetA64("000111100x1xxxxx010010xxxxxxxxxx", InstName.Fmax_S,          InstEmit.Fmax_S,          typeof(OpCodeSimdReg));
            SetA64("0>0011100<1xxxxx111101xxxxxxxxxx", InstName.Fmax_V,          InstEmit.Fmax_V,          typeof(OpCodeSimdReg));
            SetA64("000111100x1xxxxx011010xxxxxxxxxx", InstName.Fmaxnm_S,        InstEmit.Fmaxnm_S,        typeof(OpCodeSimdReg));
            SetA64("0>0011100<1xxxxx110001xxxxxxxxxx", InstName.Fmaxnm_V,        InstEmit.Fmaxnm_V,        typeof(OpCodeSimdReg));
            SetA64("0>1011100<1xxxxx111101xxxxxxxxxx", InstName.Fmaxp_V,         InstEmit.Fmaxp_V,         typeof(OpCodeSimdReg));
            SetA64("000111100x1xxxxx010110xxxxxxxxxx", InstName.Fmin_S,          InstEmit.Fmin_S,          typeof(OpCodeSimdReg));
            SetA64("0>0011101<1xxxxx111101xxxxxxxxxx", InstName.Fmin_V,          InstEmit.Fmin_V,          typeof(OpCodeSimdReg));
            SetA64("000111100x1xxxxx011110xxxxxxxxxx", InstName.Fminnm_S,        InstEmit.Fminnm_S,        typeof(OpCodeSimdReg));
            SetA64("0>0011101<1xxxxx110001xxxxxxxxxx", InstName.Fminnm_V,        InstEmit.Fminnm_V,        typeof(OpCodeSimdReg));
            SetA64("0>1011101<1xxxxx111101xxxxxxxxxx", InstName.Fminp_V,         InstEmit.Fminp_V,         typeof(OpCodeSimdReg));
            SetA64("010111111xxxxxxx0001x0xxxxxxxxxx", InstName.Fmla_Se,         InstEmit.Fmla_Se,         typeof(OpCodeSimdRegElemF));
            SetA64("0>0011100<1xxxxx110011xxxxxxxxxx", InstName.Fmla_V,          InstEmit.Fmla_V,          typeof(OpCodeSimdReg));
            SetA64("0>0011111<xxxxxx0001x0xxxxxxxxxx", InstName.Fmla_Ve,         InstEmit.Fmla_Ve,         typeof(OpCodeSimdRegElemF));
            SetA64("010111111xxxxxxx0101x0xxxxxxxxxx", InstName.Fmls_Se,         InstEmit.Fmls_Se,         typeof(OpCodeSimdRegElemF));
            SetA64("0>0011101<1xxxxx110011xxxxxxxxxx", InstName.Fmls_V,          InstEmit.Fmls_V,          typeof(OpCodeSimdReg));
            SetA64("0>0011111<xxxxxx0101x0xxxxxxxxxx", InstName.Fmls_Ve,         InstEmit.Fmls_Ve,         typeof(OpCodeSimdRegElemF));
            SetA64("000111100x100000010000xxxxxxxxxx", InstName.Fmov_S,          InstEmit.Fmov_S,          typeof(OpCodeSimd));
            SetA64("000111100x1xxxxxxxx10000000xxxxx", InstName.Fmov_Si,         InstEmit.Fmov_Si,         typeof(OpCodeSimdFmov));
            SetA64("0x00111100000xxx111101xxxxxxxxxx", InstName.Fmov_Vi,         InstEmit.Fmov_Vi,         typeof(OpCodeSimdImm));
            SetA64("0110111100000xxx111101xxxxxxxxxx", InstName.Fmov_Vi,         InstEmit.Fmov_Vi,         typeof(OpCodeSimdImm));
            SetA64("0001111000100110000000xxxxxxxxxx", InstName.Fmov_Ftoi,       InstEmit.Fmov_Ftoi,       typeof(OpCodeSimd));
            SetA64("1001111001100110000000xxxxxxxxxx", InstName.Fmov_Ftoi,       InstEmit.Fmov_Ftoi,       typeof(OpCodeSimd));
            SetA64("0001111000100111000000xxxxxxxxxx", InstName.Fmov_Itof,       InstEmit.Fmov_Itof,       typeof(OpCodeSimd));
            SetA64("1001111001100111000000xxxxxxxxxx", InstName.Fmov_Itof,       InstEmit.Fmov_Itof,       typeof(OpCodeSimd));
            SetA64("1001111010101110000000xxxxxxxxxx", InstName.Fmov_Ftoi1,      InstEmit.Fmov_Ftoi1,      typeof(OpCodeSimd));
            SetA64("1001111010101111000000xxxxxxxxxx", InstName.Fmov_Itof1,      InstEmit.Fmov_Itof1,      typeof(OpCodeSimd));
            SetA64("000111110x0xxxxx1xxxxxxxxxxxxxxx", InstName.Fmsub_S,         InstEmit.Fmsub_S,         typeof(OpCodeSimdReg));
            SetA64("000111100x1xxxxx000010xxxxxxxxxx", InstName.Fmul_S,          InstEmit.Fmul_S,          typeof(OpCodeSimdReg));
            SetA64("010111111xxxxxxx1001x0xxxxxxxxxx", InstName.Fmul_Se,         InstEmit.Fmul_Se,         typeof(OpCodeSimdRegElemF));
            SetA64("0>1011100<1xxxxx110111xxxxxxxxxx", InstName.Fmul_V,          InstEmit.Fmul_V,          typeof(OpCodeSimdReg));
            SetA64("0>0011111<xxxxxx1001x0xxxxxxxxxx", InstName.Fmul_Ve,         InstEmit.Fmul_Ve,         typeof(OpCodeSimdRegElemF));
            SetA64("010111100x1xxxxx110111xxxxxxxxxx", InstName.Fmulx_S,         InstEmit.Fmulx_S,         typeof(OpCodeSimdReg));
            SetA64("011111111xxxxxxx1001x0xxxxxxxxxx", InstName.Fmulx_Se,        InstEmit.Fmulx_Se,        typeof(OpCodeSimdRegElemF));
            SetA64("0>0011100<1xxxxx110111xxxxxxxxxx", InstName.Fmulx_V,         InstEmit.Fmulx_V,         typeof(OpCodeSimdReg));
            SetA64("0>1011111<xxxxxx1001x0xxxxxxxxxx", InstName.Fmulx_Ve,        InstEmit.Fmulx_Ve,        typeof(OpCodeSimdRegElemF));
            SetA64("000111100x100001010000xxxxxxxxxx", InstName.Fneg_S,          InstEmit.Fneg_S,          typeof(OpCodeSimd));
            SetA64("0>1011101<100000111110xxxxxxxxxx", InstName.Fneg_V,          InstEmit.Fneg_V,          typeof(OpCodeSimd));
            SetA64("000111110x1xxxxx0xxxxxxxxxxxxxxx", InstName.Fnmadd_S,        InstEmit.Fnmadd_S,        typeof(OpCodeSimdReg));
            SetA64("000111110x1xxxxx1xxxxxxxxxxxxxxx", InstName.Fnmsub_S,        InstEmit.Fnmsub_S,        typeof(OpCodeSimdReg));
            SetA64("000111100x1xxxxx100010xxxxxxxxxx", InstName.Fnmul_S,         InstEmit.Fnmul_S,         typeof(OpCodeSimdReg));
            SetA64("010111101x100001110110xxxxxxxxxx", InstName.Frecpe_S,        InstEmit.Frecpe_S,        typeof(OpCodeSimd));
            SetA64("0>0011101<100001110110xxxxxxxxxx", InstName.Frecpe_V,        InstEmit.Frecpe_V,        typeof(OpCodeSimd));
            SetA64("010111100x1xxxxx111111xxxxxxxxxx", InstName.Frecps_S,        InstEmit.Frecps_S,        typeof(OpCodeSimdReg));
            SetA64("0>0011100<1xxxxx111111xxxxxxxxxx", InstName.Frecps_V,        InstEmit.Frecps_V,        typeof(OpCodeSimdReg));
            SetA64("010111101x100001111110xxxxxxxxxx", InstName.Frecpx_S,        InstEmit.Frecpx_S,        typeof(OpCodeSimd));
            SetA64("000111100x100110010000xxxxxxxxxx", InstName.Frinta_S,        InstEmit.Frinta_S,        typeof(OpCodeSimd));
            SetA64("0>1011100<100001100010xxxxxxxxxx", InstName.Frinta_V,        InstEmit.Frinta_V,        typeof(OpCodeSimd));
            SetA64("000111100x100111110000xxxxxxxxxx", InstName.Frinti_S,        InstEmit.Frinti_S,        typeof(OpCodeSimd));
            SetA64("0>1011101<100001100110xxxxxxxxxx", InstName.Frinti_V,        InstEmit.Frinti_V,        typeof(OpCodeSimd));
            SetA64("000111100x100101010000xxxxxxxxxx", InstName.Frintm_S,        InstEmit.Frintm_S,        typeof(OpCodeSimd));
            SetA64("0>0011100<100001100110xxxxxxxxxx", InstName.Frintm_V,        InstEmit.Frintm_V,        typeof(OpCodeSimd));
            SetA64("000111100x100100010000xxxxxxxxxx", InstName.Frintn_S,        InstEmit.Frintn_S,        typeof(OpCodeSimd));
            SetA64("0>0011100<100001100010xxxxxxxxxx", InstName.Frintn_V,        InstEmit.Frintn_V,        typeof(OpCodeSimd));
            SetA64("000111100x100100110000xxxxxxxxxx", InstName.Frintp_S,        InstEmit.Frintp_S,        typeof(OpCodeSimd));
            SetA64("0>0011101<100001100010xxxxxxxxxx", InstName.Frintp_V,        InstEmit.Frintp_V,        typeof(OpCodeSimd));
            SetA64("000111100x100111010000xxxxxxxxxx", InstName.Frintx_S,        InstEmit.Frintx_S,        typeof(OpCodeSimd));
            SetA64("0>1011100<100001100110xxxxxxxxxx", InstName.Frintx_V,        InstEmit.Frintx_V,        typeof(OpCodeSimd));
            SetA64("000111100x100101110000xxxxxxxxxx", InstName.Frintz_S,        InstEmit.Frintz_S,        typeof(OpCodeSimd));
            SetA64("0>0011101<100001100110xxxxxxxxxx", InstName.Frintz_V,        InstEmit.Frintz_V,        typeof(OpCodeSimd));
            SetA64("011111101x100001110110xxxxxxxxxx", InstName.Frsqrte_S,       InstEmit.Frsqrte_S,       typeof(OpCodeSimd));
            SetA64("0>1011101<100001110110xxxxxxxxxx", InstName.Frsqrte_V,       InstEmit.Frsqrte_V,       typeof(OpCodeSimd));
            SetA64("010111101x1xxxxx111111xxxxxxxxxx", InstName.Frsqrts_S,       InstEmit.Frsqrts_S,       typeof(OpCodeSimdReg));
            SetA64("0>0011101<1xxxxx111111xxxxxxxxxx", InstName.Frsqrts_V,       InstEmit.Frsqrts_V,       typeof(OpCodeSimdReg));
            SetA64("000111100x100001110000xxxxxxxxxx", InstName.Fsqrt_S,         InstEmit.Fsqrt_S,         typeof(OpCodeSimd));
            SetA64("0>1011101<100001111110xxxxxxxxxx", InstName.Fsqrt_V,         InstEmit.Fsqrt_V,         typeof(OpCodeSimd));
            SetA64("000111100x1xxxxx001110xxxxxxxxxx", InstName.Fsub_S,          InstEmit.Fsub_S,          typeof(OpCodeSimdReg));
            SetA64("0>0011101<1xxxxx110101xxxxxxxxxx", InstName.Fsub_V,          InstEmit.Fsub_V,          typeof(OpCodeSimdReg));
            SetA64("01001110000xxxxx000111xxxxxxxxxx", InstName.Ins_Gp,          InstEmit.Ins_Gp,          typeof(OpCodeSimdIns));
            SetA64("01101110000xxxxx0xxxx1xxxxxxxxxx", InstName.Ins_V,           InstEmit.Ins_V,           typeof(OpCodeSimdIns));
            SetA64("0x00110001000000xxxxxxxxxxxxxxxx", InstName.Ld__Vms,         InstEmit.Ld__Vms,         typeof(OpCodeSimdMemMs));
            SetA64("0x001100110xxxxxxxxxxxxxxxxxxxxx", InstName.Ld__Vms,         InstEmit.Ld__Vms,         typeof(OpCodeSimdMemMs));
            SetA64("0x00110101x00000xxxxxxxxxxxxxxxx", InstName.Ld__Vss,         InstEmit.Ld__Vss,         typeof(OpCodeSimdMemSs));
            SetA64("0x00110111xxxxxxxxxxxxxxxxxxxxxx", InstName.Ld__Vss,         InstEmit.Ld__Vss,         typeof(OpCodeSimdMemSs));
            SetA64("xx10110xx1xxxxxxxxxxxxxxxxxxxxxx", InstName.Ldp,             InstEmit.Ldp,             typeof(OpCodeSimdMemPair));
            SetA64("xx111100x10xxxxxxxxx00xxxxxxxxxx", InstName.Ldr,             InstEmit.Ldr,             typeof(OpCodeSimdMemImm));
            SetA64("xx111100x10xxxxxxxxx01xxxxxxxxxx", InstName.Ldr,             InstEmit.Ldr,             typeof(OpCodeSimdMemImm));
            SetA64("xx111100x10xxxxxxxxx11xxxxxxxxxx", InstName.Ldr,             InstEmit.Ldr,             typeof(OpCodeSimdMemImm));
            SetA64("xx111101x1xxxxxxxxxxxxxxxxxxxxxx", InstName.Ldr,             InstEmit.Ldr,             typeof(OpCodeSimdMemImm));
            SetA64("xx111100x11xxxxxxxxx10xxxxxxxxxx", InstName.Ldr,             InstEmit.Ldr,             typeof(OpCodeSimdMemReg));
            SetA64("xx011100xxxxxxxxxxxxxxxxxxxxxxxx", InstName.Ldr_Literal,     InstEmit.Ldr_Literal,     typeof(OpCodeSimdMemLit));
            SetA64("0x001110<<1xxxxx100101xxxxxxxxxx", InstName.Mla_V,           InstEmit.Mla_V,           typeof(OpCodeSimdReg));
            SetA64("0x101111xxxxxxxx0000x0xxxxxxxxxx", InstName.Mla_Ve,          InstEmit.Mla_Ve,          typeof(OpCodeSimdRegElem));
            SetA64("0x101110<<1xxxxx100101xxxxxxxxxx", InstName.Mls_V,           InstEmit.Mls_V,           typeof(OpCodeSimdReg));
            SetA64("0x101111xxxxxxxx0100x0xxxxxxxxxx", InstName.Mls_Ve,          InstEmit.Mls_Ve,          typeof(OpCodeSimdRegElem));
            SetA64("0x00111100000xxx0xx001xxxxxxxxxx", InstName.Movi_V,          InstEmit.Movi_V,          typeof(OpCodeSimdImm));
            SetA64("0x00111100000xxx10x001xxxxxxxxxx", InstName.Movi_V,          InstEmit.Movi_V,          typeof(OpCodeSimdImm));
            SetA64("0x00111100000xxx110x01xxxxxxxxxx", InstName.Movi_V,          InstEmit.Movi_V,          typeof(OpCodeSimdImm));
            SetA64("0xx0111100000xxx111001xxxxxxxxxx", InstName.Movi_V,          InstEmit.Movi_V,          typeof(OpCodeSimdImm));
            SetA64("0x001110<<1xxxxx100111xxxxxxxxxx", InstName.Mul_V,           InstEmit.Mul_V,           typeof(OpCodeSimdReg));
            SetA64("0x001111xxxxxxxx1000x0xxxxxxxxxx", InstName.Mul_Ve,          InstEmit.Mul_Ve,          typeof(OpCodeSimdRegElem));
            SetA64("0x10111100000xxx0xx001xxxxxxxxxx", InstName.Mvni_V,          InstEmit.Mvni_V,          typeof(OpCodeSimdImm));
            SetA64("0x10111100000xxx10x001xxxxxxxxxx", InstName.Mvni_V,          InstEmit.Mvni_V,          typeof(OpCodeSimdImm));
            SetA64("0x10111100000xxx110x01xxxxxxxxxx", InstName.Mvni_V,          InstEmit.Mvni_V,          typeof(OpCodeSimdImm));
            SetA64("0111111011100000101110xxxxxxxxxx", InstName.Neg_S,           InstEmit.Neg_S,           typeof(OpCodeSimd));
            SetA64("0>101110<<100000101110xxxxxxxxxx", InstName.Neg_V,           InstEmit.Neg_V,           typeof(OpCodeSimd));
            SetA64("0x10111000100000010110xxxxxxxxxx", InstName.Not_V,           InstEmit.Not_V,           typeof(OpCodeSimd));
            SetA64("0x001110111xxxxx000111xxxxxxxxxx", InstName.Orn_V,           InstEmit.Orn_V,           typeof(OpCodeSimdReg));
            SetA64("0x001110101xxxxx000111xxxxxxxxxx", InstName.Orr_V,           InstEmit.Orr_V,           typeof(OpCodeSimdReg));
            SetA64("0x00111100000xxx0xx101xxxxxxxxxx", InstName.Orr_Vi,          InstEmit.Orr_Vi,          typeof(OpCodeSimdImm));
            SetA64("0x00111100000xxx10x101xxxxxxxxxx", InstName.Orr_Vi,          InstEmit.Orr_Vi,          typeof(OpCodeSimdImm));
            SetA64("0x101110<<1xxxxx010000xxxxxxxxxx", InstName.Raddhn_V,        InstEmit.Raddhn_V,        typeof(OpCodeSimdReg));
            SetA64("0x10111001100000010110xxxxxxxxxx", InstName.Rbit_V,          InstEmit.Rbit_V,          typeof(OpCodeSimd));
            SetA64("0x00111000100000000110xxxxxxxxxx", InstName.Rev16_V,         InstEmit.Rev16_V,         typeof(OpCodeSimd));
            SetA64("0x1011100x100000000010xxxxxxxxxx", InstName.Rev32_V,         InstEmit.Rev32_V,         typeof(OpCodeSimd));
            SetA64("0x001110<<100000000010xxxxxxxxxx", InstName.Rev64_V,         InstEmit.Rev64_V,         typeof(OpCodeSimd));
            SetA64("0x00111100>>>xxx100011xxxxxxxxxx", InstName.Rshrn_V,         InstEmit.Rshrn_V,         typeof(OpCodeSimdShImm));
            SetA64("0x101110<<1xxxxx011000xxxxxxxxxx", InstName.Rsubhn_V,        InstEmit.Rsubhn_V,        typeof(OpCodeSimdReg));
            SetA64("0x001110<<1xxxxx011111xxxxxxxxxx", InstName.Saba_V,          InstEmit.Saba_V,          typeof(OpCodeSimdReg));
            SetA64("0x001110<<1xxxxx010100xxxxxxxxxx", InstName.Sabal_V,         InstEmit.Sabal_V,         typeof(OpCodeSimdReg));
            SetA64("0x001110<<1xxxxx011101xxxxxxxxxx", InstName.Sabd_V,          InstEmit.Sabd_V,          typeof(OpCodeSimdReg));
            SetA64("0x001110<<1xxxxx011100xxxxxxxxxx", InstName.Sabdl_V,         InstEmit.Sabdl_V,         typeof(OpCodeSimdReg));
            SetA64("0x001110<<100000011010xxxxxxxxxx", InstName.Sadalp_V,        InstEmit.Sadalp_V,        typeof(OpCodeSimd));
            SetA64("0x001110<<1xxxxx000000xxxxxxxxxx", InstName.Saddl_V,         InstEmit.Saddl_V,         typeof(OpCodeSimdReg));
            SetA64("0x001110<<100000001010xxxxxxxxxx", InstName.Saddlp_V,        InstEmit.Saddlp_V,        typeof(OpCodeSimd));
            SetA64("000011100x110000001110xxxxxxxxxx", InstName.Saddlv_V,        InstEmit.Saddlv_V,        typeof(OpCodeSimd));
            SetA64("01001110<<110000001110xxxxxxxxxx", InstName.Saddlv_V,        InstEmit.Saddlv_V,        typeof(OpCodeSimd));
            SetA64("0x001110<<1xxxxx000100xxxxxxxxxx", InstName.Saddw_V,         InstEmit.Saddw_V,         typeof(OpCodeSimdReg));
            SetA64("x00111100x100010000000xxxxxxxxxx", InstName.Scvtf_Gp,        InstEmit.Scvtf_Gp,        typeof(OpCodeSimdCvt));
            SetA64(">00111100x000010>xxxxxxxxxxxxxxx", InstName.Scvtf_Gp_Fixed,  InstEmit.Scvtf_Gp_Fixed,  typeof(OpCodeSimdCvt));
            SetA64("010111100x100001110110xxxxxxxxxx", InstName.Scvtf_S,         InstEmit.Scvtf_S,         typeof(OpCodeSimd));
            SetA64("0>0011100<100001110110xxxxxxxxxx", InstName.Scvtf_V,         InstEmit.Scvtf_V,         typeof(OpCodeSimd));
            SetA64("0x001111001xxxxx111001xxxxxxxxxx", InstName.Scvtf_V_Fixed,   InstEmit.Scvtf_V_Fixed,   typeof(OpCodeSimdShImm));
            SetA64("0100111101xxxxxx111001xxxxxxxxxx", InstName.Scvtf_V_Fixed,   InstEmit.Scvtf_V_Fixed,   typeof(OpCodeSimdShImm));
            SetA64("01011110000xxxxx000000xxxxxxxxxx", InstName.Sha1c_V,         InstEmit.Sha1c_V,         typeof(OpCodeSimdReg));
            SetA64("0101111000101000000010xxxxxxxxxx", InstName.Sha1h_V,         InstEmit.Sha1h_V,         typeof(OpCodeSimd));
            SetA64("01011110000xxxxx001000xxxxxxxxxx", InstName.Sha1m_V,         InstEmit.Sha1m_V,         typeof(OpCodeSimdReg));
            SetA64("01011110000xxxxx000100xxxxxxxxxx", InstName.Sha1p_V,         InstEmit.Sha1p_V,         typeof(OpCodeSimdReg));
            SetA64("01011110000xxxxx001100xxxxxxxxxx", InstName.Sha1su0_V,       InstEmit.Sha1su0_V,       typeof(OpCodeSimdReg));
            SetA64("0101111000101000000110xxxxxxxxxx", InstName.Sha1su1_V,       InstEmit.Sha1su1_V,       typeof(OpCodeSimd));
            SetA64("01011110000xxxxx010000xxxxxxxxxx", InstName.Sha256h_V,       InstEmit.Sha256h_V,       typeof(OpCodeSimdReg));
            SetA64("01011110000xxxxx010100xxxxxxxxxx", InstName.Sha256h2_V,      InstEmit.Sha256h2_V,      typeof(OpCodeSimdReg));
            SetA64("0101111000101000001010xxxxxxxxxx", InstName.Sha256su0_V,     InstEmit.Sha256su0_V,     typeof(OpCodeSimd));
            SetA64("01011110000xxxxx011000xxxxxxxxxx", InstName.Sha256su1_V,     InstEmit.Sha256su1_V,     typeof(OpCodeSimdReg));
            SetA64("0x001110<<1xxxxx000001xxxxxxxxxx", InstName.Shadd_V,         InstEmit.Shadd_V,         typeof(OpCodeSimdReg));
            SetA64("0101111101xxxxxx010101xxxxxxxxxx", InstName.Shl_S,           InstEmit.Shl_S,           typeof(OpCodeSimdShImm));
            SetA64("0x00111100>>>xxx010101xxxxxxxxxx", InstName.Shl_V,           InstEmit.Shl_V,           typeof(OpCodeSimdShImm));
            SetA64("0100111101xxxxxx010101xxxxxxxxxx", InstName.Shl_V,           InstEmit.Shl_V,           typeof(OpCodeSimdShImm));
            SetA64("0x101110<<100001001110xxxxxxxxxx", InstName.Shll_V,          InstEmit.Shll_V,          typeof(OpCodeSimd));
            SetA64("0x00111100>>>xxx100001xxxxxxxxxx", InstName.Shrn_V,          InstEmit.Shrn_V,          typeof(OpCodeSimdShImm));
            SetA64("0x001110<<1xxxxx001001xxxxxxxxxx", InstName.Shsub_V,         InstEmit.Shsub_V,         typeof(OpCodeSimdReg));
            SetA64("0111111101xxxxxx010101xxxxxxxxxx", InstName.Sli_S,           InstEmit.Sli_S,           typeof(OpCodeSimdShImm));
            SetA64("0x10111100>>>xxx010101xxxxxxxxxx", InstName.Sli_V,           InstEmit.Sli_V,           typeof(OpCodeSimdShImm));
            SetA64("0110111101xxxxxx010101xxxxxxxxxx", InstName.Sli_V,           InstEmit.Sli_V,           typeof(OpCodeSimdShImm));
            SetA64("0x001110<<1xxxxx011001xxxxxxxxxx", InstName.Smax_V,          InstEmit.Smax_V,          typeof(OpCodeSimdReg));
            SetA64("0x001110<<1xxxxx101001xxxxxxxxxx", InstName.Smaxp_V,         InstEmit.Smaxp_V,         typeof(OpCodeSimdReg));
            SetA64("000011100x110000101010xxxxxxxxxx", InstName.Smaxv_V,         InstEmit.Smaxv_V,         typeof(OpCodeSimd));
            SetA64("01001110<<110000101010xxxxxxxxxx", InstName.Smaxv_V,         InstEmit.Smaxv_V,         typeof(OpCodeSimd));
            SetA64("0x001110<<1xxxxx011011xxxxxxxxxx", InstName.Smin_V,          InstEmit.Smin_V,          typeof(OpCodeSimdReg));
            SetA64("0x001110<<1xxxxx101011xxxxxxxxxx", InstName.Sminp_V,         InstEmit.Sminp_V,         typeof(OpCodeSimdReg));
            SetA64("000011100x110001101010xxxxxxxxxx", InstName.Sminv_V,         InstEmit.Sminv_V,         typeof(OpCodeSimd));
            SetA64("01001110<<110001101010xxxxxxxxxx", InstName.Sminv_V,         InstEmit.Sminv_V,         typeof(OpCodeSimd));
            SetA64("0x001110<<1xxxxx100000xxxxxxxxxx", InstName.Smlal_V,         InstEmit.Smlal_V,         typeof(OpCodeSimdReg));
            SetA64("0x001111xxxxxxxx0010x0xxxxxxxxxx", InstName.Smlal_Ve,        InstEmit.Smlal_Ve,        typeof(OpCodeSimdRegElem));
            SetA64("0x001110<<1xxxxx101000xxxxxxxxxx", InstName.Smlsl_V,         InstEmit.Smlsl_V,         typeof(OpCodeSimdReg));
            SetA64("0x001111xxxxxxxx0110x0xxxxxxxxxx", InstName.Smlsl_Ve,        InstEmit.Smlsl_Ve,        typeof(OpCodeSimdRegElem));
            SetA64("0x001110000xxxxx001011xxxxxxxxxx", InstName.Smov_S,          InstEmit.Smov_S,          typeof(OpCodeSimdIns));
            SetA64("0x001110<<1xxxxx110000xxxxxxxxxx", InstName.Smull_V,         InstEmit.Smull_V,         typeof(OpCodeSimdReg));
            SetA64("0x001111xxxxxxxx1010x0xxxxxxxxxx", InstName.Smull_Ve,        InstEmit.Smull_Ve,        typeof(OpCodeSimdRegElem));
            SetA64("01011110xx100000011110xxxxxxxxxx", InstName.Sqabs_S,         InstEmit.Sqabs_S,         typeof(OpCodeSimd));
            SetA64("0>001110<<100000011110xxxxxxxxxx", InstName.Sqabs_V,         InstEmit.Sqabs_V,         typeof(OpCodeSimd));
            SetA64("01011110xx1xxxxx000011xxxxxxxxxx", InstName.Sqadd_S,         InstEmit.Sqadd_S,         typeof(OpCodeSimdReg));
            SetA64("0>001110<<1xxxxx000011xxxxxxxxxx", InstName.Sqadd_V,         InstEmit.Sqadd_V,         typeof(OpCodeSimdReg));
            SetA64("01011110011xxxxx101101xxxxxxxxxx", InstName.Sqdmulh_S,       InstEmit.Sqdmulh_S,       typeof(OpCodeSimdReg));
            SetA64("01011110101xxxxx101101xxxxxxxxxx", InstName.Sqdmulh_S,       InstEmit.Sqdmulh_S,       typeof(OpCodeSimdReg));
            SetA64("0x001110011xxxxx101101xxxxxxxxxx", InstName.Sqdmulh_V,       InstEmit.Sqdmulh_V,       typeof(OpCodeSimdReg));
            SetA64("0x001110101xxxxx101101xxxxxxxxxx", InstName.Sqdmulh_V,       InstEmit.Sqdmulh_V,       typeof(OpCodeSimdReg));
            SetA64("01111110xx100000011110xxxxxxxxxx", InstName.Sqneg_S,         InstEmit.Sqneg_S,         typeof(OpCodeSimd));
            SetA64("0>101110<<100000011110xxxxxxxxxx", InstName.Sqneg_V,         InstEmit.Sqneg_V,         typeof(OpCodeSimd));
            SetA64("01111110011xxxxx101101xxxxxxxxxx", InstName.Sqrdmulh_S,      InstEmit.Sqrdmulh_S,      typeof(OpCodeSimdReg));
            SetA64("01111110101xxxxx101101xxxxxxxxxx", InstName.Sqrdmulh_S,      InstEmit.Sqrdmulh_S,      typeof(OpCodeSimdReg));
            SetA64("0x101110011xxxxx101101xxxxxxxxxx", InstName.Sqrdmulh_V,      InstEmit.Sqrdmulh_V,      typeof(OpCodeSimdReg));
            SetA64("0x101110101xxxxx101101xxxxxxxxxx", InstName.Sqrdmulh_V,      InstEmit.Sqrdmulh_V,      typeof(OpCodeSimdReg));
            SetA64("0>001110<<1xxxxx010111xxxxxxxxxx", InstName.Sqrshl_V,        InstEmit.Sqrshl_V,        typeof(OpCodeSimdReg));
            SetA64("0101111100>>>xxx100111xxxxxxxxxx", InstName.Sqrshrn_S,       InstEmit.Sqrshrn_S,       typeof(OpCodeSimdShImm));
            SetA64("0x00111100>>>xxx100111xxxxxxxxxx", InstName.Sqrshrn_V,       InstEmit.Sqrshrn_V,       typeof(OpCodeSimdShImm));
            SetA64("0111111100>>>xxx100011xxxxxxxxxx", InstName.Sqrshrun_S,      InstEmit.Sqrshrun_S,      typeof(OpCodeSimdShImm));
            SetA64("0x10111100>>>xxx100011xxxxxxxxxx", InstName.Sqrshrun_V,      InstEmit.Sqrshrun_V,      typeof(OpCodeSimdShImm));
            SetA64("0>001110<<1xxxxx010011xxxxxxxxxx", InstName.Sqshl_V,         InstEmit.Sqshl_V,         typeof(OpCodeSimdReg));
            SetA64("0101111100>>>xxx100101xxxxxxxxxx", InstName.Sqshrn_S,        InstEmit.Sqshrn_S,        typeof(OpCodeSimdShImm));
            SetA64("0x00111100>>>xxx100101xxxxxxxxxx", InstName.Sqshrn_V,        InstEmit.Sqshrn_V,        typeof(OpCodeSimdShImm));
            SetA64("0111111100>>>xxx100001xxxxxxxxxx", InstName.Sqshrun_S,       InstEmit.Sqshrun_S,       typeof(OpCodeSimdShImm));
            SetA64("0x10111100>>>xxx100001xxxxxxxxxx", InstName.Sqshrun_V,       InstEmit.Sqshrun_V,       typeof(OpCodeSimdShImm));
            SetA64("01011110xx1xxxxx001011xxxxxxxxxx", InstName.Sqsub_S,         InstEmit.Sqsub_S,         typeof(OpCodeSimdReg));
            SetA64("0>001110<<1xxxxx001011xxxxxxxxxx", InstName.Sqsub_V,         InstEmit.Sqsub_V,         typeof(OpCodeSimdReg));
            SetA64("01011110<<100001010010xxxxxxxxxx", InstName.Sqxtn_S,         InstEmit.Sqxtn_S,         typeof(OpCodeSimd));
            SetA64("0x001110<<100001010010xxxxxxxxxx", InstName.Sqxtn_V,         InstEmit.Sqxtn_V,         typeof(OpCodeSimd));
            SetA64("01111110<<100001001010xxxxxxxxxx", InstName.Sqxtun_S,        InstEmit.Sqxtun_S,        typeof(OpCodeSimd));
            SetA64("0x101110<<100001001010xxxxxxxxxx", InstName.Sqxtun_V,        InstEmit.Sqxtun_V,        typeof(OpCodeSimd));
            SetA64("0x001110<<1xxxxx000101xxxxxxxxxx", InstName.Srhadd_V,        InstEmit.Srhadd_V,        typeof(OpCodeSimdReg));
            SetA64("0111111101xxxxxx010001xxxxxxxxxx", InstName.Sri_S,           InstEmit.Sri_S,           typeof(OpCodeSimdShImm));
            SetA64("0x10111100>>>xxx010001xxxxxxxxxx", InstName.Sri_V,           InstEmit.Sri_V,           typeof(OpCodeSimdShImm));
            SetA64("0110111101xxxxxx010001xxxxxxxxxx", InstName.Sri_V,           InstEmit.Sri_V,           typeof(OpCodeSimdShImm));
            SetA64("0>001110<<1xxxxx010101xxxxxxxxxx", InstName.Srshl_V,         InstEmit.Srshl_V,         typeof(OpCodeSimdReg));
            SetA64("0101111101xxxxxx001001xxxxxxxxxx", InstName.Srshr_S,         InstEmit.Srshr_S,         typeof(OpCodeSimdShImm));
            SetA64("0x00111100>>>xxx001001xxxxxxxxxx", InstName.Srshr_V,         InstEmit.Srshr_V,         typeof(OpCodeSimdShImm));
            SetA64("0100111101xxxxxx001001xxxxxxxxxx", InstName.Srshr_V,         InstEmit.Srshr_V,         typeof(OpCodeSimdShImm));
            SetA64("0101111101xxxxxx001101xxxxxxxxxx", InstName.Srsra_S,         InstEmit.Srsra_S,         typeof(OpCodeSimdShImm));
            SetA64("0x00111100>>>xxx001101xxxxxxxxxx", InstName.Srsra_V,         InstEmit.Srsra_V,         typeof(OpCodeSimdShImm));
            SetA64("0100111101xxxxxx001101xxxxxxxxxx", InstName.Srsra_V,         InstEmit.Srsra_V,         typeof(OpCodeSimdShImm));
            SetA64("0>001110<<1xxxxx010001xxxxxxxxxx", InstName.Sshl_V,          InstEmit.Sshl_V,          typeof(OpCodeSimdReg));
            SetA64("0x00111100>>>xxx101001xxxxxxxxxx", InstName.Sshll_V,         InstEmit.Sshll_V,         typeof(OpCodeSimdShImm));
            SetA64("0101111101xxxxxx000001xxxxxxxxxx", InstName.Sshr_S,          InstEmit.Sshr_S,          typeof(OpCodeSimdShImm));
            SetA64("0x00111100>>>xxx000001xxxxxxxxxx", InstName.Sshr_V,          InstEmit.Sshr_V,          typeof(OpCodeSimdShImm));
            SetA64("0100111101xxxxxx000001xxxxxxxxxx", InstName.Sshr_V,          InstEmit.Sshr_V,          typeof(OpCodeSimdShImm));
            SetA64("0101111101xxxxxx000101xxxxxxxxxx", InstName.Ssra_S,          InstEmit.Ssra_S,          typeof(OpCodeSimdShImm));
            SetA64("0x00111100>>>xxx000101xxxxxxxxxx", InstName.Ssra_V,          InstEmit.Ssra_V,          typeof(OpCodeSimdShImm));
            SetA64("0100111101xxxxxx000101xxxxxxxxxx", InstName.Ssra_V,          InstEmit.Ssra_V,          typeof(OpCodeSimdShImm));
            SetA64("0x001110<<1xxxxx001000xxxxxxxxxx", InstName.Ssubl_V,         InstEmit.Ssubl_V,         typeof(OpCodeSimdReg));
            SetA64("0x001110<<1xxxxx001100xxxxxxxxxx", InstName.Ssubw_V,         InstEmit.Ssubw_V,         typeof(OpCodeSimdReg));
            SetA64("0x00110000000000xxxxxxxxxxxxxxxx", InstName.St__Vms,         InstEmit.St__Vms,         typeof(OpCodeSimdMemMs));
            SetA64("0x001100100xxxxxxxxxxxxxxxxxxxxx", InstName.St__Vms,         InstEmit.St__Vms,         typeof(OpCodeSimdMemMs));
            SetA64("0x00110100x00000xxxxxxxxxxxxxxxx", InstName.St__Vss,         InstEmit.St__Vss,         typeof(OpCodeSimdMemSs));
            SetA64("0x00110110xxxxxxxxxxxxxxxxxxxxxx", InstName.St__Vss,         InstEmit.St__Vss,         typeof(OpCodeSimdMemSs));
            SetA64("xx10110xx0xxxxxxxxxxxxxxxxxxxxxx", InstName.Stp,             InstEmit.Stp,             typeof(OpCodeSimdMemPair));
            SetA64("xx111100x00xxxxxxxxx00xxxxxxxxxx", InstName.Str,             InstEmit.Str,             typeof(OpCodeSimdMemImm));
            SetA64("xx111100x00xxxxxxxxx01xxxxxxxxxx", InstName.Str,             InstEmit.Str,             typeof(OpCodeSimdMemImm));
            SetA64("xx111100x00xxxxxxxxx11xxxxxxxxxx", InstName.Str,             InstEmit.Str,             typeof(OpCodeSimdMemImm));
            SetA64("xx111101x0xxxxxxxxxxxxxxxxxxxxxx", InstName.Str,             InstEmit.Str,             typeof(OpCodeSimdMemImm));
            SetA64("xx111100x01xxxxxxxxx10xxxxxxxxxx", InstName.Str,             InstEmit.Str,             typeof(OpCodeSimdMemReg));
            SetA64("01111110111xxxxx100001xxxxxxxxxx", InstName.Sub_S,           InstEmit.Sub_S,           typeof(OpCodeSimdReg));
            SetA64("0>101110<<1xxxxx100001xxxxxxxxxx", InstName.Sub_V,           InstEmit.Sub_V,           typeof(OpCodeSimdReg));
            SetA64("0x001110<<1xxxxx011000xxxxxxxxxx", InstName.Subhn_V,         InstEmit.Subhn_V,         typeof(OpCodeSimdReg));
            SetA64("01011110xx100000001110xxxxxxxxxx", InstName.Suqadd_S,        InstEmit.Suqadd_S,        typeof(OpCodeSimd));
            SetA64("0>001110<<100000001110xxxxxxxxxx", InstName.Suqadd_V,        InstEmit.Suqadd_V,        typeof(OpCodeSimd));
            SetA64("0x001110000xxxxx0xx000xxxxxxxxxx", InstName.Tbl_V,           InstEmit.Tbl_V,           typeof(OpCodeSimdTbl));
            SetA64("0x001110000xxxxx0xx100xxxxxxxxxx", InstName.Tbx_V,           InstEmit.Tbx_V,           typeof(OpCodeSimdTbl));
            SetA64("0>001110<<0xxxxx001010xxxxxxxxxx", InstName.Trn1_V,          InstEmit.Trn1_V,          typeof(OpCodeSimdReg));
            SetA64("0>001110<<0xxxxx011010xxxxxxxxxx", InstName.Trn2_V,          InstEmit.Trn2_V,          typeof(OpCodeSimdReg));
            SetA64("0x101110<<1xxxxx011111xxxxxxxxxx", InstName.Uaba_V,          InstEmit.Uaba_V,          typeof(OpCodeSimdReg));
            SetA64("0x101110<<1xxxxx010100xxxxxxxxxx", InstName.Uabal_V,         InstEmit.Uabal_V,         typeof(OpCodeSimdReg));
            SetA64("0x101110<<1xxxxx011101xxxxxxxxxx", InstName.Uabd_V,          InstEmit.Uabd_V,          typeof(OpCodeSimdReg));
            SetA64("0x101110<<1xxxxx011100xxxxxxxxxx", InstName.Uabdl_V,         InstEmit.Uabdl_V,         typeof(OpCodeSimdReg));
            SetA64("0x101110<<100000011010xxxxxxxxxx", InstName.Uadalp_V,        InstEmit.Uadalp_V,        typeof(OpCodeSimd));
            SetA64("0x101110<<1xxxxx000000xxxxxxxxxx", InstName.Uaddl_V,         InstEmit.Uaddl_V,         typeof(OpCodeSimdReg));
            SetA64("0x101110<<100000001010xxxxxxxxxx", InstName.Uaddlp_V,        InstEmit.Uaddlp_V,        typeof(OpCodeSimd));
            SetA64("001011100x110000001110xxxxxxxxxx", InstName.Uaddlv_V,        InstEmit.Uaddlv_V,        typeof(OpCodeSimd));
            SetA64("01101110<<110000001110xxxxxxxxxx", InstName.Uaddlv_V,        InstEmit.Uaddlv_V,        typeof(OpCodeSimd));
            SetA64("0x101110<<1xxxxx000100xxxxxxxxxx", InstName.Uaddw_V,         InstEmit.Uaddw_V,         typeof(OpCodeSimdReg));
            SetA64("x00111100x100011000000xxxxxxxxxx", InstName.Ucvtf_Gp,        InstEmit.Ucvtf_Gp,        typeof(OpCodeSimdCvt));
            SetA64(">00111100x000011>xxxxxxxxxxxxxxx", InstName.Ucvtf_Gp_Fixed,  InstEmit.Ucvtf_Gp_Fixed,  typeof(OpCodeSimdCvt));
            SetA64("011111100x100001110110xxxxxxxxxx", InstName.Ucvtf_S,         InstEmit.Ucvtf_S,         typeof(OpCodeSimd));
            SetA64("0>1011100<100001110110xxxxxxxxxx", InstName.Ucvtf_V,         InstEmit.Ucvtf_V,         typeof(OpCodeSimd));
            SetA64("0x101111001xxxxx111001xxxxxxxxxx", InstName.Ucvtf_V_Fixed,   InstEmit.Ucvtf_V_Fixed,   typeof(OpCodeSimdShImm));
            SetA64("0110111101xxxxxx111001xxxxxxxxxx", InstName.Ucvtf_V_Fixed,   InstEmit.Ucvtf_V_Fixed,   typeof(OpCodeSimdShImm));
            SetA64("0x101110<<1xxxxx000001xxxxxxxxxx", InstName.Uhadd_V,         InstEmit.Uhadd_V,         typeof(OpCodeSimdReg));
            SetA64("0x101110<<1xxxxx001001xxxxxxxxxx", InstName.Uhsub_V,         InstEmit.Uhsub_V,         typeof(OpCodeSimdReg));
            SetA64("0x101110<<1xxxxx011001xxxxxxxxxx", InstName.Umax_V,          InstEmit.Umax_V,          typeof(OpCodeSimdReg));
            SetA64("0x101110<<1xxxxx101001xxxxxxxxxx", InstName.Umaxp_V,         InstEmit.Umaxp_V,         typeof(OpCodeSimdReg));
            SetA64("001011100x110000101010xxxxxxxxxx", InstName.Umaxv_V,         InstEmit.Umaxv_V,         typeof(OpCodeSimd));
            SetA64("01101110<<110000101010xxxxxxxxxx", InstName.Umaxv_V,         InstEmit.Umaxv_V,         typeof(OpCodeSimd));
            SetA64("0x101110<<1xxxxx011011xxxxxxxxxx", InstName.Umin_V,          InstEmit.Umin_V,          typeof(OpCodeSimdReg));
            SetA64("0x101110<<1xxxxx101011xxxxxxxxxx", InstName.Uminp_V,         InstEmit.Uminp_V,         typeof(OpCodeSimdReg));
            SetA64("001011100x110001101010xxxxxxxxxx", InstName.Uminv_V,         InstEmit.Uminv_V,         typeof(OpCodeSimd));
            SetA64("01101110<<110001101010xxxxxxxxxx", InstName.Uminv_V,         InstEmit.Uminv_V,         typeof(OpCodeSimd));
            SetA64("0x101110<<1xxxxx100000xxxxxxxxxx", InstName.Umlal_V,         InstEmit.Umlal_V,         typeof(OpCodeSimdReg));
            SetA64("0x101111xxxxxxxx0010x0xxxxxxxxxx", InstName.Umlal_Ve,        InstEmit.Umlal_Ve,        typeof(OpCodeSimdRegElem));
            SetA64("0x101110<<1xxxxx101000xxxxxxxxxx", InstName.Umlsl_V,         InstEmit.Umlsl_V,         typeof(OpCodeSimdReg));
            SetA64("0x101111xxxxxxxx0110x0xxxxxxxxxx", InstName.Umlsl_Ve,        InstEmit.Umlsl_Ve,        typeof(OpCodeSimdRegElem));
            SetA64("0x001110000xxxxx001111xxxxxxxxxx", InstName.Umov_S,          InstEmit.Umov_S,          typeof(OpCodeSimdIns));
            SetA64("0x101110<<1xxxxx110000xxxxxxxxxx", InstName.Umull_V,         InstEmit.Umull_V,         typeof(OpCodeSimdReg));
            SetA64("0x101111xxxxxxxx1010x0xxxxxxxxxx", InstName.Umull_Ve,        InstEmit.Umull_Ve,        typeof(OpCodeSimdRegElem));
            SetA64("01111110xx1xxxxx000011xxxxxxxxxx", InstName.Uqadd_S,         InstEmit.Uqadd_S,         typeof(OpCodeSimdReg));
            SetA64("0>101110<<1xxxxx000011xxxxxxxxxx", InstName.Uqadd_V,         InstEmit.Uqadd_V,         typeof(OpCodeSimdReg));
            SetA64("0>101110<<1xxxxx010111xxxxxxxxxx", InstName.Uqrshl_V,        InstEmit.Uqrshl_V,        typeof(OpCodeSimdReg));
            SetA64("0111111100>>>xxx100111xxxxxxxxxx", InstName.Uqrshrn_S,       InstEmit.Uqrshrn_S,       typeof(OpCodeSimdShImm));
            SetA64("0x10111100>>>xxx100111xxxxxxxxxx", InstName.Uqrshrn_V,       InstEmit.Uqrshrn_V,       typeof(OpCodeSimdShImm));
            SetA64("0>101110<<1xxxxx010011xxxxxxxxxx", InstName.Uqshl_V,         InstEmit.Uqshl_V,         typeof(OpCodeSimdReg));
            SetA64("0111111100>>>xxx100101xxxxxxxxxx", InstName.Uqshrn_S,        InstEmit.Uqshrn_S,        typeof(OpCodeSimdShImm));
            SetA64("0x10111100>>>xxx100101xxxxxxxxxx", InstName.Uqshrn_V,        InstEmit.Uqshrn_V,        typeof(OpCodeSimdShImm));
            SetA64("01111110xx1xxxxx001011xxxxxxxxxx", InstName.Uqsub_S,         InstEmit.Uqsub_S,         typeof(OpCodeSimdReg));
            SetA64("0>101110<<1xxxxx001011xxxxxxxxxx", InstName.Uqsub_V,         InstEmit.Uqsub_V,         typeof(OpCodeSimdReg));
            SetA64("01111110<<100001010010xxxxxxxxxx", InstName.Uqxtn_S,         InstEmit.Uqxtn_S,         typeof(OpCodeSimd));
            SetA64("0x101110<<100001010010xxxxxxxxxx", InstName.Uqxtn_V,         InstEmit.Uqxtn_V,         typeof(OpCodeSimd));
            SetA64("0x101110<<1xxxxx000101xxxxxxxxxx", InstName.Urhadd_V,        InstEmit.Urhadd_V,        typeof(OpCodeSimdReg));
            SetA64("0>101110<<1xxxxx010101xxxxxxxxxx", InstName.Urshl_V,         InstEmit.Urshl_V,         typeof(OpCodeSimdReg));
            SetA64("0111111101xxxxxx001001xxxxxxxxxx", InstName.Urshr_S,         InstEmit.Urshr_S,         typeof(OpCodeSimdShImm));
            SetA64("0x10111100>>>xxx001001xxxxxxxxxx", InstName.Urshr_V,         InstEmit.Urshr_V,         typeof(OpCodeSimdShImm));
            SetA64("0110111101xxxxxx001001xxxxxxxxxx", InstName.Urshr_V,         InstEmit.Urshr_V,         typeof(OpCodeSimdShImm));
            SetA64("0111111101xxxxxx001101xxxxxxxxxx", InstName.Ursra_S,         InstEmit.Ursra_S,         typeof(OpCodeSimdShImm));
            SetA64("0x10111100>>>xxx001101xxxxxxxxxx", InstName.Ursra_V,         InstEmit.Ursra_V,         typeof(OpCodeSimdShImm));
            SetA64("0110111101xxxxxx001101xxxxxxxxxx", InstName.Ursra_V,         InstEmit.Ursra_V,         typeof(OpCodeSimdShImm));
            SetA64("0>101110<<1xxxxx010001xxxxxxxxxx", InstName.Ushl_V,          InstEmit.Ushl_V,          typeof(OpCodeSimdReg));
            SetA64("0x10111100>>>xxx101001xxxxxxxxxx", InstName.Ushll_V,         InstEmit.Ushll_V,         typeof(OpCodeSimdShImm));
            SetA64("0111111101xxxxxx000001xxxxxxxxxx", InstName.Ushr_S,          InstEmit.Ushr_S,          typeof(OpCodeSimdShImm));
            SetA64("0x10111100>>>xxx000001xxxxxxxxxx", InstName.Ushr_V,          InstEmit.Ushr_V,          typeof(OpCodeSimdShImm));
            SetA64("0110111101xxxxxx000001xxxxxxxxxx", InstName.Ushr_V,          InstEmit.Ushr_V,          typeof(OpCodeSimdShImm));
            SetA64("01111110xx100000001110xxxxxxxxxx", InstName.Usqadd_S,        InstEmit.Usqadd_S,        typeof(OpCodeSimd));
            SetA64("0>101110<<100000001110xxxxxxxxxx", InstName.Usqadd_V,        InstEmit.Usqadd_V,        typeof(OpCodeSimd));
            SetA64("0111111101xxxxxx000101xxxxxxxxxx", InstName.Usra_S,          InstEmit.Usra_S,          typeof(OpCodeSimdShImm));
            SetA64("0x10111100>>>xxx000101xxxxxxxxxx", InstName.Usra_V,          InstEmit.Usra_V,          typeof(OpCodeSimdShImm));
            SetA64("0110111101xxxxxx000101xxxxxxxxxx", InstName.Usra_V,          InstEmit.Usra_V,          typeof(OpCodeSimdShImm));
            SetA64("0x101110<<1xxxxx001000xxxxxxxxxx", InstName.Usubl_V,         InstEmit.Usubl_V,         typeof(OpCodeSimdReg));
            SetA64("0x101110<<1xxxxx001100xxxxxxxxxx", InstName.Usubw_V,         InstEmit.Usubw_V,         typeof(OpCodeSimdReg));
            SetA64("0>001110<<0xxxxx000110xxxxxxxxxx", InstName.Uzp1_V,          InstEmit.Uzp1_V,          typeof(OpCodeSimdReg));
            SetA64("0>001110<<0xxxxx010110xxxxxxxxxx", InstName.Uzp2_V,          InstEmit.Uzp2_V,          typeof(OpCodeSimdReg));
            SetA64("0x001110<<100001001010xxxxxxxxxx", InstName.Xtn_V,           InstEmit.Xtn_V,           typeof(OpCodeSimd));
            SetA64("0>001110<<0xxxxx001110xxxxxxxxxx", InstName.Zip1_V,          InstEmit.Zip1_V,          typeof(OpCodeSimdReg));
            SetA64("0>001110<<0xxxxx011110xxxxxxxxxx", InstName.Zip2_V,          InstEmit.Zip2_V,          typeof(OpCodeSimdReg));
#endregion

#region "OpCode Table (AArch32)"
            // Base
            SetA32("<<<<0010101xxxxxxxxxxxxxxxxxxxxx", InstName.Adc,    InstEmit32.Adc,    typeof(OpCode32AluImm));
            SetA32("<<<<0000101xxxxxxxxxxxxxxxx0xxxx", InstName.Adc,    InstEmit32.Adc,    typeof(OpCode32AluRsImm));
            SetA32("<<<<0000101xxxxxxxxxxxxx0xx1xxxx", InstName.Adc,    InstEmit32.Adc,    typeof(OpCode32AluRsReg));
            SetA32("<<<<0010100xxxxxxxxxxxxxxxxxxxxx", InstName.Add,    InstEmit32.Add,    typeof(OpCode32AluImm));
            SetA32("<<<<0000100xxxxxxxxxxxxxxxx0xxxx", InstName.Add,    InstEmit32.Add,    typeof(OpCode32AluRsImm));
            SetA32("<<<<0000100xxxxxxxxxxxxx0xx1xxxx", InstName.Add,    InstEmit32.Add,    typeof(OpCode32AluRsReg));
            SetA32("<<<<0010000xxxxxxxxxxxxxxxxxxxxx", InstName.And,    InstEmit32.And,    typeof(OpCode32AluImm));
            SetA32("<<<<0000000xxxxxxxxxxxxxxxx0xxxx", InstName.And,    InstEmit32.And,    typeof(OpCode32AluRsImm));
            SetA32("<<<<0000000xxxxxxxxxxxxx0xx1xxxx", InstName.And,    InstEmit32.And,    typeof(OpCode32AluRsReg));
            SetA32("<<<<1010xxxxxxxxxxxxxxxxxxxxxxxx", InstName.B,      InstEmit32.B,      typeof(OpCode32BImm));
            SetA32("<<<<0111110xxxxxxxxxxxxxx0011111", InstName.Bfc,    InstEmit32.Bfc,    typeof(OpCode32AluBf));
            SetA32("<<<<0111110xxxxxxxxxxxxxx001xxxx", InstName.Bfi,    InstEmit32.Bfi,    typeof(OpCode32AluBf));
            SetA32("<<<<0011110xxxxxxxxxxxxxxxxxxxxx", InstName.Bic,    InstEmit32.Bic,    typeof(OpCode32AluImm));
            SetA32("<<<<0001110xxxxxxxxxxxxxxxx0xxxx", InstName.Bic,    InstEmit32.Bic,    typeof(OpCode32AluRsImm));
            SetA32("<<<<0001110xxxxxxxxxxxxx0xx1xxxx", InstName.Bic,    InstEmit32.Bic,    typeof(OpCode32AluRsReg));
            SetA32("<<<<1011xxxxxxxxxxxxxxxxxxxxxxxx", InstName.Bl,     InstEmit32.Bl,     typeof(OpCode32BImm));
            SetA32("1111101xxxxxxxxxxxxxxxxxxxxxxxxx", InstName.Blx,    InstEmit32.Blx,    typeof(OpCode32BImm));
            SetA32("<<<<000100101111111111110011xxxx", InstName.Blx,    InstEmit32.Blxr,   typeof(OpCode32BReg));
            SetA32("<<<<000100101111111111110001xxxx", InstName.Bx,     InstEmit32.Bx,     typeof(OpCode32BReg));
            SetT32("xxxxxxxxxxxxxxxx010001110xxxx000", InstName.Bx,     InstEmit32.Bx,     typeof(OpCodeT16BReg));
            SetA32("11110101011111111111000000011111", InstName.Clrex,  InstEmit32.Clrex,  typeof(OpCode32));
            SetA32("<<<<000101101111xxxx11110001xxxx", InstName.Clz,    InstEmit32.Clz,    typeof(OpCode32AluReg));
            SetA32("<<<<00110111xxxx0000xxxxxxxxxxxx", InstName.Cmn,    InstEmit32.Cmn,    typeof(OpCode32AluImm));
            SetA32("<<<<00010111xxxx0000xxxxxxx0xxxx", InstName.Cmn,    InstEmit32.Cmn,    typeof(OpCode32AluRsImm));
            SetA32("<<<<00110101xxxx0000xxxxxxxxxxxx", InstName.Cmp,    InstEmit32.Cmp,    typeof(OpCode32AluImm));
            SetA32("<<<<00010101xxxx0000xxxxxxx0xxxx", InstName.Cmp,    InstEmit32.Cmp,    typeof(OpCode32AluRsImm));
            SetA32("<<<<00010101xxxx0000xxxx0xx1xxxx", InstName.Cmp,    InstEmit32.Cmp,    typeof(OpCode32AluRsReg));
            SetA32("1111010101111111111100000101xxxx", InstName.Dmb,    InstEmit32.Dmb,    typeof(OpCode32));
            SetA32("1111010101111111111100000100xxxx", InstName.Dsb,    InstEmit32.Dsb,    typeof(OpCode32));
            SetA32("<<<<0010001xxxxxxxxxxxxxxxxxxxxx", InstName.Eor,    InstEmit32.Eor,    typeof(OpCode32AluImm));
            SetA32("<<<<0000001xxxxxxxxxxxxxxxx0xxxx", InstName.Eor,    InstEmit32.Eor,    typeof(OpCode32AluRsImm));
            SetA32("<<<<0000001xxxxxxxxxxxxx0xx1xxxx", InstName.Eor,    InstEmit32.Eor,    typeof(OpCode32AluRsReg));
            SetA32("1111010101111111111100000110xxxx", InstName.Isb,    InstEmit32.Nop,    typeof(OpCode32));
            SetA32("<<<<00011001xxxxxxxx110010011111", InstName.Lda,    InstEmit32.Lda,    typeof(OpCode32MemLdEx));
            SetA32("<<<<00011101xxxxxxxx110010011111", InstName.Ldab,   InstEmit32.Ldab,   typeof(OpCode32MemLdEx));
            SetA32("<<<<00011001xxxxxxxx111010011111", InstName.Ldaex,  InstEmit32.Ldaex,  typeof(OpCode32MemLdEx));
            SetA32("<<<<00011101xxxxxxxx111010011111", InstName.Ldaexb, InstEmit32.Ldaexb, typeof(OpCode32MemLdEx));
            SetA32("<<<<00011011xxxxxxxx111010011111", InstName.Ldaexd, InstEmit32.Ldaexd, typeof(OpCode32MemLdEx));
            SetA32("<<<<00011111xxxxxxxx111010011111", InstName.Ldaexh, InstEmit32.Ldaexh, typeof(OpCode32MemLdEx));
            SetA32("<<<<00011111xxxxxxxx110010011111", InstName.Ldah,   InstEmit32.Ldah,   typeof(OpCode32MemLdEx));
            SetA32("<<<<100xx0x1xxxxxxxxxxxxxxxxxxxx", InstName.Ldm,    InstEmit32.Ldm,    typeof(OpCode32MemMult));
            SetA32("<<<<010xx0x1xxxxxxxxxxxxxxxxxxxx", InstName.Ldr,    InstEmit32.Ldr,    typeof(OpCode32MemImm));
            SetA32("<<<<011xx0x1xxxxxxxxxxxxxxx0xxxx", InstName.Ldr,    InstEmit32.Ldr,    typeof(OpCode32MemRsImm));
            SetA32("<<<<010xx1x1xxxxxxxxxxxxxxxxxxxx", InstName.Ldrb,   InstEmit32.Ldrb,   typeof(OpCode32MemImm));
            SetA32("<<<<011xx1x1xxxxxxxxxxxxxxx0xxxx", InstName.Ldrb,   InstEmit32.Ldrb,   typeof(OpCode32MemRsImm));
            SetA32("<<<<000xx1x0xxxxxxxxxxxx1101xxxx", InstName.Ldrd,   InstEmit32.Ldrd,   typeof(OpCode32MemImm8));
            SetA32("<<<<000xx0x0xxxxxxxx00001101xxxx", InstName.Ldrd,   InstEmit32.Ldrd,   typeof(OpCode32MemReg));
            SetA32("<<<<00011001xxxxxxxx111110011111", InstName.Ldrex,  InstEmit32.Ldrex,  typeof(OpCode32MemLdEx));
            SetA32("<<<<00011101xxxxxxxx111110011111", InstName.Ldrexb, InstEmit32.Ldrexb, typeof(OpCode32MemLdEx));
            SetA32("<<<<00011011xxxxxxxx111110011111", InstName.Ldrexd, InstEmit32.Ldrexd, typeof(OpCode32MemLdEx));
            SetA32("<<<<00011111xxxxxxxx111110011111", InstName.Ldrexh, InstEmit32.Ldrexh, typeof(OpCode32MemLdEx));
            SetA32("<<<<000xx1x1xxxxxxxxxxxx1011xxxx", InstName.Ldrh,   InstEmit32.Ldrh,   typeof(OpCode32MemImm8));
            SetA32("<<<<000xx0x1xxxxxxxx00001011xxxx", InstName.Ldrh,   InstEmit32.Ldrh,   typeof(OpCode32MemReg));
            SetA32("<<<<000xx1x1xxxxxxxxxxxx1101xxxx", InstName.Ldrsb,  InstEmit32.Ldrsb,  typeof(OpCode32MemImm8));
            SetA32("<<<<000xx0x1xxxxxxxx00001101xxxx", InstName.Ldrsb,  InstEmit32.Ldrsb,  typeof(OpCode32MemReg));
            SetA32("<<<<000xx1x1xxxxxxxxxxxx1111xxxx", InstName.Ldrsh,  InstEmit32.Ldrsh,  typeof(OpCode32MemImm8));
            SetA32("<<<<000xx0x1xxxxxxxx00001111xxxx", InstName.Ldrsh,  InstEmit32.Ldrsh,  typeof(OpCode32MemReg));
            SetA32("<<<<1110xxx0xxxxxxxx111xxxx1xxxx", InstName.Mcr,    InstEmit32.Mcr,    typeof(OpCode32System));
            SetA32("<<<<0000001xxxxxxxxxxxxx1001xxxx", InstName.Mla,    InstEmit32.Mla,    typeof(OpCode32AluMla));
            SetA32("<<<<00000110xxxxxxxxxxxx1001xxxx", InstName.Mls,    InstEmit32.Mls,    typeof(OpCode32AluMla));
            SetA32("<<<<0011101x0000xxxxxxxxxxxxxxxx", InstName.Mov,    InstEmit32.Mov,    typeof(OpCode32AluImm));
            SetA32("<<<<0001101x0000xxxxxxxxxxx0xxxx", InstName.Mov,    InstEmit32.Mov,    typeof(OpCode32AluRsImm));
            SetA32("<<<<0001101x0000xxxxxxxx0xx1xxxx", InstName.Mov,    InstEmit32.Mov,    typeof(OpCode32AluRsReg));
            SetA32("<<<<00110000xxxxxxxxxxxxxxxxxxxx", InstName.Mov,    InstEmit32.Mov,    typeof(OpCode32AluImm16));
            SetT32("xxxxxxxxxxxxxxxx00100xxxxxxxxxxx", InstName.Mov,    InstEmit32.Mov,    typeof(OpCodeT16AluImm8));
            SetA32("<<<<00110100xxxxxxxxxxxxxxxxxxxx", InstName.Movt,   InstEmit32.Movt,   typeof(OpCode32AluImm16));
            SetA32("<<<<1110xxx1xxxxxxxx111xxxx1xxxx", InstName.Mrc,    InstEmit32.Mrc,    typeof(OpCode32System));
            SetA32("<<<<11000101xxxxxxxx111xxxxxxxxx", InstName.Mrrc,   InstEmit32.Mrrc,   typeof(OpCode32System));
            SetA32("<<<<0000000xxxxx0000xxxx1001xxxx", InstName.Mul,    InstEmit32.Mul,    typeof(OpCode32AluMla));
            SetA32("<<<<0011111x0000xxxxxxxxxxxxxxxx", InstName.Mvn,    InstEmit32.Mvn,    typeof(OpCode32AluImm));
            SetA32("<<<<0001111x0000xxxxxxxxxxx0xxxx", InstName.Mvn,    InstEmit32.Mvn,    typeof(OpCode32AluRsImm));
            SetA32("<<<<0001111x0000xxxxxxxx0xx1xxxx", InstName.Mvn,    InstEmit32.Mvn,    typeof(OpCode32AluRsReg));
            SetA32("<<<<0011100xxxxxxxxxxxxxxxxxxxxx", InstName.Orr,    InstEmit32.Orr,    typeof(OpCode32AluImm));
            SetA32("<<<<0001100xxxxxxxxxxxxxxxx0xxxx", InstName.Orr,    InstEmit32.Orr,    typeof(OpCode32AluRsImm));
            SetA32("<<<<0001100xxxxxxxxxxxxx0xx1xxxx", InstName.Orr,    InstEmit32.Orr,    typeof(OpCode32AluRsReg));
            SetA32("<<<<01101000xxxxxxxxxxxxxx01xxxx", InstName.Pkh,    InstEmit32.Pkh,    typeof(OpCode32AluRsImm));
            SetA32("11110101xx01xxxx1111xxxxxxxxxxxx", InstName.Pld,    InstEmit32.Nop,    typeof(OpCode32));
            SetA32("11110111xx01xxxx1111xxxxxxx0xxxx", InstName.Pld,    InstEmit32.Nop,    typeof(OpCode32));
            SetA32("<<<<011011111111xxxx11110011xxxx", InstName.Rbit,   InstEmit32.Rbit,   typeof(OpCode32AluReg));
            SetA32("<<<<011010111111xxxx11110011xxxx", InstName.Rev,    InstEmit32.Rev,    typeof(OpCode32AluReg));
            SetA32("<<<<011010111111xxxx11111011xxxx", InstName.Rev16,  InstEmit32.Rev16,  typeof(OpCode32AluReg));
            SetA32("<<<<011011111111xxxx11111011xxxx", InstName.Revsh,  InstEmit32.Revsh,  typeof(OpCode32AluReg));
            SetA32("<<<<0010011xxxxxxxxxxxxxxxxxxxxx", InstName.Rsb,    InstEmit32.Rsb,    typeof(OpCode32AluImm));
            SetA32("<<<<0000011xxxxxxxxxxxxxxxx0xxxx", InstName.Rsb,    InstEmit32.Rsb,    typeof(OpCode32AluRsImm));
            SetA32("<<<<0000011xxxxxxxxxxxxx0xx1xxxx", InstName.Rsb,    InstEmit32.Rsb,    typeof(OpCode32AluRsReg));
            SetA32("<<<<0010111xxxxxxxxxxxxxxxxxxxxx", InstName.Rsc,    InstEmit32.Rsc,    typeof(OpCode32AluImm));
            SetA32("<<<<0000111xxxxxxxxxxxxxxxx0xxxx", InstName.Rsc,    InstEmit32.Rsc,    typeof(OpCode32AluRsImm));
            SetA32("<<<<0000111xxxxxxxxxxxxx0xx1xxxx", InstName.Rsc,    InstEmit32.Rsc,    typeof(OpCode32AluRsReg));
            SetA32("<<<<0010110xxxxxxxxxxxxxxxxxxxxx", InstName.Sbc,    InstEmit32.Sbc,    typeof(OpCode32AluImm));
            SetA32("<<<<0000110xxxxxxxxxxxxxxxx0xxxx", InstName.Sbc,    InstEmit32.Sbc,    typeof(OpCode32AluRsImm));
            SetA32("<<<<0000110xxxxxxxxxxxxx0xx1xxxx", InstName.Sbc,    InstEmit32.Sbc,    typeof(OpCode32AluRsReg));
            SetA32("<<<<0111101xxxxxxxxxxxxxx101xxxx", InstName.Sbfx,   InstEmit32.Sbfx,   typeof(OpCode32AluBf));
            SetA32("<<<<01110001xxxx1111xxxx0001xxxx", InstName.Sdiv,   InstEmit32.Sdiv,   typeof(OpCode32AluMla));
            SetA32("<<<<00010000xxxxxxxxxxxx1xx0xxxx", InstName.Smlab,  InstEmit32.Smlab,  typeof(OpCode32AluMla));
            SetA32("<<<<0000111xxxxxxxxxxxxx1001xxxx", InstName.Smlal,  InstEmit32.Smlal,  typeof(OpCode32AluUmull));
            SetA32("<<<<00010100xxxxxxxxxxxx1xx0xxxx", InstName.Smlalh, InstEmit32.Smlalh, typeof(OpCode32AluUmull));
            SetA32("<<<<01110101xxxxxxxxxxxx00x1xxxx", InstName.Smmla,  InstEmit32.Smmla,  typeof(OpCode32AluMla));
            SetA32("<<<<01110101xxxxxxxxxxxx11x1xxxx", InstName.Smmls,  InstEmit32.Smmls,  typeof(OpCode32AluMla));
            SetA32("<<<<00010110xxxxxxxxxxxx1xx0xxxx", InstName.Smulh,  InstEmit32.Smulh,  typeof(OpCode32AluMla));
            SetA32("<<<<0000110xxxxxxxxxxxxx1001xxxx", InstName.Smull,  InstEmit32.Smull,  typeof(OpCode32AluUmull));
            SetA32("<<<<00011000xxxx111111001001xxxx", InstName.Stl,    InstEmit32.Stl,    typeof(OpCode32MemStEx));
            SetA32("<<<<00011100xxxx111111001001xxxx", InstName.Stlb,   InstEmit32.Stlb,   typeof(OpCode32MemStEx));
            SetA32("<<<<00011000xxxxxxxx11101001xxxx", InstName.Stlex,  InstEmit32.Stlex,  typeof(OpCode32MemStEx));
            SetA32("<<<<00011100xxxxxxxx11101001xxxx", InstName.Stlexb, InstEmit32.Stlexb, typeof(OpCode32MemStEx));
            SetA32("<<<<00011010xxxxxxxx11101001xxxx", InstName.Stlexd, InstEmit32.Stlexd, typeof(OpCode32MemStEx));
            SetA32("<<<<00011110xxxxxxxx11101001xxxx", InstName.Stlexh, InstEmit32.Stlexh, typeof(OpCode32MemStEx));
            SetA32("<<<<00011110xxxx111111001001xxxx", InstName.Stlh,   InstEmit32.Stlh,   typeof(OpCode32MemStEx));
            SetA32("<<<<100xx0x0xxxxxxxxxxxxxxxxxxxx", InstName.Stm,    InstEmit32.Stm,    typeof(OpCode32MemMult));
            SetA32("<<<<010xx0x0xxxxxxxxxxxxxxxxxxxx", InstName.Str,    InstEmit32.Str,    typeof(OpCode32MemImm));
            SetA32("<<<<011xx0x0xxxxxxxxxxxxxxx0xxxx", InstName.Str,    InstEmit32.Str,    typeof(OpCode32MemRsImm));
            SetA32("<<<<010xx1x0xxxxxxxxxxxxxxxxxxxx", InstName.Strb,   InstEmit32.Strb,   typeof(OpCode32MemImm));
            SetA32("<<<<011xx1x0xxxxxxxxxxxxxxx0xxxx", InstName.Strb,   InstEmit32.Strb,   typeof(OpCode32MemRsImm));
            SetA32("<<<<000xx1x0xxxxxxxxxxxx1111xxxx", InstName.Strd,   InstEmit32.Strd,   typeof(OpCode32MemImm8));
            SetA32("<<<<000xx0x0xxxxxxxx00001111xxxx", InstName.Strd,   InstEmit32.Strd,   typeof(OpCode32MemReg));
            SetA32("<<<<00011000xxxxxxxx11111001xxxx", InstName.Strex,  InstEmit32.Strex,  typeof(OpCode32MemStEx));
            SetA32("<<<<00011100xxxxxxxx11111001xxxx", InstName.Strexb, InstEmit32.Strexb, typeof(OpCode32MemStEx));
            SetA32("<<<<00011010xxxxxxxx11111001xxxx", InstName.Strexd, InstEmit32.Strexd, typeof(OpCode32MemStEx));
            SetA32("<<<<00011110xxxxxxxx11111001xxxx", InstName.Strexh, InstEmit32.Strexh, typeof(OpCode32MemStEx));
            SetA32("<<<<000xx1x0xxxxxxxxxxxx1011xxxx", InstName.Strh,   InstEmit32.Strh,   typeof(OpCode32MemImm8));
            SetA32("<<<<000xx0x0xxxxxxxx00001011xxxx", InstName.Strh,   InstEmit32.Strh,   typeof(OpCode32MemReg));
            SetA32("<<<<0010010xxxxxxxxxxxxxxxxxxxxx", InstName.Sub,    InstEmit32.Sub,    typeof(OpCode32AluImm));
            SetA32("<<<<0000010xxxxxxxxxxxxxxxx0xxxx", InstName.Sub,    InstEmit32.Sub,    typeof(OpCode32AluRsImm));
            SetA32("<<<<0000010xxxxxxxxxxxxx0xx1xxxx", InstName.Sub,    InstEmit32.Sub,    typeof(OpCode32AluRsReg));
            SetA32("<<<<1111xxxxxxxxxxxxxxxxxxxxxxxx", InstName.Svc,    InstEmit32.Svc,    typeof(OpCode32Exception));
            SetA32("<<<<01101010xxxxxxxxxx000111xxxx", InstName.Sxtb,   InstEmit32.Sxtb,   typeof(OpCode32AluUx));
            SetA32("<<<<01101000xxxxxxxxxx000111xxxx", InstName.Sxtb16, InstEmit32.Sxtb16, typeof(OpCode32AluUx));
            SetA32("<<<<01101011xxxxxxxxxx000111xxxx", InstName.Sxth,   InstEmit32.Sxth,   typeof(OpCode32AluUx));
            SetA32("<<<<00110011xxxx0000xxxxxxxxxxxx", InstName.Teq,    InstEmit32.Teq,    typeof(OpCode32AluImm));
            SetA32("<<<<00010011xxxx0000xxxxxxx0xxxx", InstName.Teq,    InstEmit32.Teq,    typeof(OpCode32AluRsImm));
            SetA32("<<<<00010011xxxx0000xxxx0xx1xxxx", InstName.Teq,    InstEmit32.Teq,    typeof(OpCode32AluRsReg));
            SetA32("<<<<0111111111111101111011111110", InstName.Trap,   InstEmit32.Trap,   typeof(OpCode32Exception));
            SetA32("<<<<00110001xxxx0000xxxxxxxxxxxx", InstName.Tst,    InstEmit32.Tst,    typeof(OpCode32AluImm));
            SetA32("<<<<00010001xxxx0000xxxxxxx0xxxx", InstName.Tst,    InstEmit32.Tst,    typeof(OpCode32AluRsImm));
            SetA32("<<<<00010001xxxx0000xxxx0xx1xxxx", InstName.Tst,    InstEmit32.Tst,    typeof(OpCode32AluRsReg));
            SetA32("<<<<0111111xxxxxxxxxxxxxx101xxxx", InstName.Ubfx,   InstEmit32.Ubfx,   typeof(OpCode32AluBf));
            SetA32("<<<<01110011xxxx1111xxxx0001xxxx", InstName.Udiv,   InstEmit32.Udiv,   typeof(OpCode32AluMla));
            SetA32("<<<<0000101xxxxxxxxxxxxx1001xxxx", InstName.Umlal,  InstEmit32.Umlal,  typeof(OpCode32AluUmull));
            SetA32("<<<<0000100xxxxxxxxxxxxx1001xxxx", InstName.Umull,  InstEmit32.Umull,  typeof(OpCode32AluUmull));
            SetA32("<<<<01101110xxxxxxxxxx000111xxxx", InstName.Uxtb,   InstEmit32.Uxtb,   typeof(OpCode32AluUx));
            SetA32("<<<<01101100xxxxxxxxxx000111xxxx", InstName.Uxtb16, InstEmit32.Uxtb16, typeof(OpCode32AluUx));
            SetA32("<<<<01101111xxxxxxxxxx000111xxxx", InstName.Uxth,   InstEmit32.Uxth,   typeof(OpCode32AluUx));

            // FP & SIMD
            SetA32("<<<<11101x110000xxxx10xx11x0xxxx", InstName.Vabs,    InstEmit32.Vabs_S,   typeof(OpCode32SimdRegS));
            SetA32("111100111x11xx01xxxx0x110xx0xxxx", InstName.Vabs,    InstEmit32.Vabs_V,   typeof(OpCode32SimdReg));
            SetA32("111100100xxxxxxxxxxx1000xxx0xxxx", InstName.Vadd,    InstEmit32.Vadd_I,   typeof(OpCode32SimdReg));
            SetA32("<<<<11100x11xxxxxxxx101xx0x0xxxx", InstName.Vadd,    InstEmit32.Vadd_S,   typeof(OpCode32SimdRegS));
            SetA32("111100100x00xxxxxxxx1101xxx0xxxx", InstName.Vadd,    InstEmit32.Vadd_V,   typeof(OpCode32SimdReg));
            SetA32("111100100x00xxxxxxxx0001xxx1xxxx", InstName.Vand,    InstEmit32.Vand_I,   typeof(OpCode32SimdBinary));
            SetA32("111100110x11xxxxxxxx0001xxx1xxxx", InstName.Vbif,    InstEmit32.Vbif,     typeof(OpCode32SimdBinary));
            SetA32("111100110x10xxxxxxxx0001xxx1xxxx", InstName.Vbit,    InstEmit32.Vbit,     typeof(OpCode32SimdBinary));
            SetA32("111100110x01xxxxxxxx0001xxx1xxxx", InstName.Vbsl,    InstEmit32.Vbsl,     typeof(OpCode32SimdBinary));
            SetA32("111100110x<<xxxxxxxx1000xxx1xxxx", InstName.Vceq,    InstEmit32.Vceq_I,   typeof(OpCode32SimdReg)); 
            SetA32("111100100x00xxxxxxxx1110xxx0xxxx", InstName.Vceq,    InstEmit32.Vceq_V,   typeof(OpCode32SimdReg));
            SetA32("111100111x11xx01xxxx0x010xx0xxxx", InstName.Vceq,    InstEmit32.Vceq_Z,   typeof(OpCode32SimdCmpZ));
            SetA32("1111001x0x<<xxxxxxxx0011xxx1xxxx", InstName.Vcge,    InstEmit32.Vcge_I,   typeof(OpCode32SimdReg));
            SetA32("111100110x00xxxxxxxx1110xxx0xxxx", InstName.Vcge,    InstEmit32.Vcge_V,   typeof(OpCode32SimdReg));
            SetA32("111100111x11xx01xxxx0x001xx0xxxx", InstName.Vcge,    InstEmit32.Vcge_Z,   typeof(OpCode32SimdCmpZ));
            SetA32("1111001x0x<<xxxxxxxx0011xxx0xxxx", InstName.Vcgt,    InstEmit32.Vcgt_I,   typeof(OpCode32SimdReg));
            SetA32("111100110x10xxxxxxxx1110xxx0xxxx", InstName.Vcgt,    InstEmit32.Vcgt_V,   typeof(OpCode32SimdReg));
            SetA32("111100111x11xx01xxxx0x000xx0xxxx", InstName.Vcgt,    InstEmit32.Vcgt_Z,   typeof(OpCode32SimdCmpZ));
            SetA32("111100111x11xx01xxxx0x011xx0xxxx", InstName.Vcle,    InstEmit32.Vcle_Z,   typeof(OpCode32SimdCmpZ));
            SetA32("111100111x11xx01xxxx0x100xx0xxxx", InstName.Vclt,    InstEmit32.Vclt_Z,   typeof(OpCode32SimdCmpZ));
            SetA32("<<<<11101x11010xxxxx101x01x0xxxx", InstName.Vcmp,    InstEmit32.Vcmp,     typeof(OpCode32SimdS));
            SetA32("<<<<11101x11010xxxxx101x11x0xxxx", InstName.Vcmpe,   InstEmit32.Vcmpe,    typeof(OpCode32SimdS));
            SetA32("<<<<11101x110111xxxx101x11x0xxxx", InstName.Vcvt,    InstEmit32.Vcvt_FD,  typeof(OpCode32SimdS)); // FP 32 and 64, scalar.
            SetA32("<<<<11101x11110xxxxx10xx11x0xxxx", InstName.Vcvt,    InstEmit32.Vcvt_FI,  typeof(OpCode32SimdCvtFI)); // FP32 to int.
            SetA32("<<<<11101x111000xxxx10xxx1x0xxxx", InstName.Vcvt,    InstEmit32.Vcvt_FI,  typeof(OpCode32SimdCvtFI)); // Int to FP32.
            SetA32("111111101x1111xxxxxx10>>x1x0xxxx", InstName.Vcvt,    InstEmit32.Vcvt_R,   typeof(OpCode32SimdCvtFI)); // The many FP32 to int encodings (fp).
            SetA32("111100111x111011xxxx011xxxx0xxxx", InstName.Vcvt,    InstEmit32.Vcvt_V,   typeof(OpCode32SimdCmpZ)); // FP and integer, vector.
            SetA32("<<<<11101x00xxxxxxxx101xx0x0xxxx", InstName.Vdiv,    InstEmit32.Vdiv_S,   typeof(OpCode32SimdRegS));
            SetA32("<<<<11101xx0xxxxxxxx1011x0x10000", InstName.Vdup,    InstEmit32.Vdup,     typeof(OpCode32SimdDupGP));
            SetA32("111100111x11xxxxxxxx11000xx0xxxx", InstName.Vdup,    InstEmit32.Vdup_1,   typeof(OpCode32SimdDupElem));
            SetA32("111100101x11xxxxxxxxxxxxxxx0xxxx", InstName.Vext,    InstEmit32.Vext,     typeof(OpCode32SimdExt));
            SetA32("111101001x10xxxxxxxxxx00xxxxxxxx", InstName.Vld1,    InstEmit32.Vld1,     typeof(OpCode32SimdMemSingle));
            SetA32("111101000x10xxxxxxxx0111xxxxxxxx", InstName.Vld1,    InstEmit32.Vld1,     typeof(OpCode32SimdMemPair)); // Regs = 1.
            SetA32("111101000x10xxxxxxxx1010xxxxxxxx", InstName.Vld1,    InstEmit32.Vld1,     typeof(OpCode32SimdMemPair)); // Regs = 2.
            SetA32("111101000x10xxxxxxxx0110xxxxxxxx", InstName.Vld1,    InstEmit32.Vld1,     typeof(OpCode32SimdMemPair)); // Regs = 3.
            SetA32("111101000x10xxxxxxxx0010xxxxxxxx", InstName.Vld1,    InstEmit32.Vld1,     typeof(OpCode32SimdMemPair)); // Regs = 4.
            SetA32("111101001x10xxxxxxxxxx01xxxxxxxx", InstName.Vld2,    InstEmit32.Vld2,     typeof(OpCode32SimdMemSingle));
            SetA32("111101000x10xxxxxxxx100xxxxxxxxx", InstName.Vld2,    InstEmit32.Vld2,     typeof(OpCode32SimdMemPair)); // Regs = 1, inc = 1/2 (itype).
            SetA32("111101000x10xxxxxxxx0011xxxxxxxx", InstName.Vld2,    InstEmit32.Vld2,     typeof(OpCode32SimdMemPair)); // Regs = 2, inc = 2.
            SetA32("111101001x10xxxxxxxxxx10xxxxxxxx", InstName.Vld3,    InstEmit32.Vld3,     typeof(OpCode32SimdMemSingle));
            SetA32("111101000x10xxxxxxxx010xxxxxxxxx", InstName.Vld3,    InstEmit32.Vld3,     typeof(OpCode32SimdMemPair)); // Inc = 1/2 (itype).
            SetA32("111101001x10xxxxxxxxxx11xxxxxxxx", InstName.Vld4,    InstEmit32.Vld4,     typeof(OpCode32SimdMemSingle));
            SetA32("111101000x10xxxxxxxx000xxxxxxxxx", InstName.Vld4,    InstEmit32.Vld4,     typeof(OpCode32SimdMemPair)); // Inc = 1/2 (itype).
            SetA32("<<<<11001x01xxxxxxxx1011xxxxxxx0", InstName.Vldm,    InstEmit32.Vldm,     typeof(OpCode32SimdMemMult));
            SetA32("<<<<11001x11xxxxxxxx1011xxxxxxx0", InstName.Vldm,    InstEmit32.Vldm,     typeof(OpCode32SimdMemMult));
            SetA32("<<<<11010x11xxxxxxxx1011xxxxxxx0", InstName.Vldm,    InstEmit32.Vldm,     typeof(OpCode32SimdMemMult));
            SetA32("<<<<11001x01xxxxxxxx1010xxxxxxxx", InstName.Vldm,    InstEmit32.Vldm,     typeof(OpCode32SimdMemMult));
            SetA32("<<<<11001x11xxxxxxxx1010xxxxxxxx", InstName.Vldm,    InstEmit32.Vldm,     typeof(OpCode32SimdMemMult));
            SetA32("<<<<11010x11xxxxxxxx1010xxxxxxxx", InstName.Vldm,    InstEmit32.Vldm,     typeof(OpCode32SimdMemMult));
            SetA32("<<<<1101xx01xxxxxxxx101xxxxxxxxx", InstName.Vldr,    InstEmit32.Vldr,     typeof(OpCode32SimdMemImm));
            SetA32("1111001x0x<<xxxxxxxx0110xxx0xxxx", InstName.Vmax,    InstEmit32.Vmax_I,   typeof(OpCode32SimdReg));
            SetA32("111100100x00xxxxxxxx1111xxx0xxxx", InstName.Vmax,    InstEmit32.Vmax_V,   typeof(OpCode32SimdReg));
            SetA32("1111001x0x<<xxxxxxxx0110xxx1xxxx", InstName.Vmin,    InstEmit32.Vmin_I,   typeof(OpCode32SimdReg));
            SetA32("111100100x10xxxxxxxx1111xxx0xxxx", InstName.Vmin,    InstEmit32.Vmin_V,   typeof(OpCode32SimdReg));
            SetA32("111111101x00xxxxxxxx10>>x0x0xxxx", InstName.Vmaxnm,  InstEmit32.Vmaxnm_S, typeof(OpCode32SimdRegS));
            SetA32("111100110x0xxxxxxxxx1111xxx1xxxx", InstName.Vmaxnm,  InstEmit32.Vmaxnm_V, typeof(OpCode32SimdReg));
            SetA32("111111101x00xxxxxxxx10>>x1x0xxxx", InstName.Vminnm,  InstEmit32.Vminnm_S, typeof(OpCode32SimdRegS));
            SetA32("111100110x1xxxxxxxxx1111xxx1xxxx", InstName.Vminnm,  InstEmit32.Vminnm_V, typeof(OpCode32SimdReg));
            SetA32("1111001x1x<<xxxxxxxx000xx1x0xxxx", InstName.Vmla,    InstEmit32.Vmla_1,   typeof(OpCode32SimdRegElem));
            SetA32("111100100xxxxxxxxxxx1001xxx0xxxx", InstName.Vmla,    InstEmit32.Vmla_I,   typeof(OpCode32SimdReg));
            SetA32("<<<<11100x00xxxxxxxx101xx0x0xxxx", InstName.Vmla,    InstEmit32.Vmla_S,   typeof(OpCode32SimdRegS));
            SetA32("111100100x00xxxxxxxx1101xxx1xxxx", InstName.Vmla,    InstEmit32.Vmla_V,   typeof(OpCode32SimdReg));
            SetA32("1111001x1x<<xxxxxxxx010xx1x0xxxx", InstName.Vmls,    InstEmit32.Vmls_1,   typeof(OpCode32SimdRegElem));
            SetA32("<<<<11100x00xxxxxxxx101xx1x0xxxx", InstName.Vmls,    InstEmit32.Vmls_S,   typeof(OpCode32SimdRegS));
            SetA32("111100100x10xxxxxxxx1101xxx1xxxx", InstName.Vmls,    InstEmit32.Vmls_V,   typeof(OpCode32SimdReg));
            SetA32("111100110xxxxxxxxxxx1001xxx0xxxx", InstName.Vmls,    InstEmit32.Vmls_I,   typeof(OpCode32SimdReg));
            SetA32("<<<<11100xx0xxxxxxxx1011xxx10000", InstName.Vmov,    InstEmit32.Vmov_G1,  typeof(OpCode32SimdMovGpElem)); // From gen purpose.
            SetA32("<<<<1110xxx1xxxxxxxx1011xxx10000", InstName.Vmov,    InstEmit32.Vmov_G1,  typeof(OpCode32SimdMovGpElem)); // To gen purpose.
            SetA32("<<<<1100010xxxxxxxxx101000x1xxxx", InstName.Vmov,    InstEmit32.Vmov_G2,  typeof(OpCode32SimdMovGpDouble)); // To/from gen purpose x2 and single precision x2.
            SetA32("<<<<1100010xxxxxxxxx101100x1xxxx", InstName.Vmov,    InstEmit32.Vmov_GD,  typeof(OpCode32SimdMovGpDouble)); // To/from gen purpose x2 and double precision.
            SetA32("<<<<1110000xxxxxxxxx1010x0010000", InstName.Vmov,    InstEmit32.Vmov_GS,  typeof(OpCode32SimdMovGp)); // To/from gen purpose and single precision.
            SetA32("1111001x1x000xxxxxxx0xx00x01xxxx", InstName.Vmov,    InstEmit32.Vmov_I,   typeof(OpCode32SimdImm)); // D/Q vector I32.
            SetA32("<<<<11101x11xxxxxxxx101x0000xxxx", InstName.Vmov,    InstEmit32.Vmov_I,   typeof(OpCode32SimdImm44)); // Scalar f16/32/64 based on size 01 10 11.
            SetA32("1111001x1x000xxxxxxx10x00x01xxxx", InstName.Vmov,    InstEmit32.Vmov_I,   typeof(OpCode32SimdImm)); // D/Q I16.
            SetA32("1111001x1x000xxxxxxx11xx0x01xxxx", InstName.Vmov,    InstEmit32.Vmov_I,   typeof(OpCode32SimdImm)); // D/Q (dt - from cmode).
            SetA32("1111001x1x000xxxxxxx11100x11xxxx", InstName.Vmov,    InstEmit32.Vmov_I,   typeof(OpCode32SimdImm)); // D/Q I64.
            SetA32("<<<<11101x110000xxxx101x01x0xxxx", InstName.Vmov,    InstEmit32.Vmov_S,   typeof(OpCode32SimdS));
            SetA32("111100111x11xx10xxxx001000x0xxx0", InstName.Vmovn,   InstEmit32.Vmovn,    typeof(OpCode32SimdCmpZ));
            SetA32("<<<<11101111xxxxxxxx101000010000", InstName.Vmrs,    InstEmit32.Vmrs,     typeof(OpCode32SimdSpecial));
            SetA32("<<<<11101110xxxxxxxx101000010000", InstName.Vmsr,    InstEmit32.Vmsr,     typeof(OpCode32SimdSpecial));
            SetA32("1111001x1x<<xxxxxxxx100xx1x0xxxx", InstName.Vmul,    InstEmit32.Vmul_1,   typeof(OpCode32SimdRegElem));
            SetA32("1111001x0xxxxxxxxxxx1001xxx1xxxx", InstName.Vmul,    InstEmit32.Vmul_I,   typeof(OpCode32SimdReg));
            SetA32("<<<<11100x10xxxxxxxx101xx0x0xxxx", InstName.Vmul,    InstEmit32.Vmul_S,   typeof(OpCode32SimdRegS));
            SetA32("111100110x00xxxxxxxx1101xxx1xxxx", InstName.Vmul,    InstEmit32.Vmul_V,   typeof(OpCode32SimdReg));
            SetA32("1111001x1x000xxxxxxx0xx00x11xxxx", InstName.Vmvn,    InstEmit32.Vmvn_I,   typeof(OpCode32SimdImm)); // D/Q vector I32.
            SetA32("1111001x1x000xxxxxxx10x00x11xxxx", InstName.Vmvn,    InstEmit32.Vmvn_I,   typeof(OpCode32SimdImm));
            SetA32("1111001x1x000xxxxxxx110x0x11xxxx", InstName.Vmvn,    InstEmit32.Vmvn_I,   typeof(OpCode32SimdImm));
            SetA32("<<<<11101x110001xxxx101x01x0xxxx", InstName.Vneg,    InstEmit32.Vneg_S,   typeof(OpCode32SimdS));
            SetA32("111100111x11xx01xxxx0x111xx0xxxx", InstName.Vneg,    InstEmit32.Vneg_V,   typeof(OpCode32Simd));
            SetA32("<<<<11100x01xxxxxxxx101xx1x0xxxx", InstName.Vnmla,   InstEmit32.Vnmla_S,  typeof(OpCode32SimdRegS));
            SetA32("<<<<11100x01xxxxxxxx101xx0x0xxxx", InstName.Vnmls,   InstEmit32.Vnmls_S,  typeof(OpCode32SimdRegS));
            SetA32("<<<<11100x10xxxxxxxx101xx1x0xxxx", InstName.Vnmul,   InstEmit32.Vnmul_S,  typeof(OpCode32SimdRegS));
            SetA32("111100100x10xxxxxxxx0001xxx1xxxx", InstName.Vorr,    InstEmit32.Vorr_I,   typeof(OpCode32SimdBinary));
            SetA32("111100100x<<xxxxxxxx1011x0x1xxxx", InstName.Vpadd,   InstEmit32.Vpadd_I,  typeof(OpCode32SimdReg));
            SetA32("111100110x00xxxxxxxx1101x0x0xxxx", InstName.Vpadd,   InstEmit32.Vpadd_V,  typeof(OpCode32SimdReg));
            SetA32("111100111x111011xxxx010x0xx0xxxx", InstName.Vrecpe,  InstEmit32.Vrecpe,   typeof(OpCode32SimdSqrte));
            SetA32("111100100x00xxxxxxxx1111xxx1xxxx", InstName.Vrecps,  InstEmit32.Vrecps,   typeof(OpCode32SimdReg));
            SetA32("111100111x11xx00xxxx000<<xx0xxxx", InstName.Vrev,    InstEmit32.Vrev,     typeof(OpCode32SimdRev));
            SetA32("111111101x1110xxxxxx101x01x0xxxx", InstName.Vrint,   InstEmit32.Vrint_RM, typeof(OpCode32SimdCvtFI));
            SetA32("<<<<11101x110110xxxx101x11x0xxxx", InstName.Vrint,   InstEmit32.Vrint_Z,  typeof(OpCode32SimdCvtFI));
            SetA32("111100111x111011xxxx010x1xx0xxxx", InstName.Vrsqrte, InstEmit32.Vrsqrte,  typeof(OpCode32SimdSqrte));
            SetA32("111100100x10xxxxxxxx1111xxx1xxxx", InstName.Vrsqrts, InstEmit32.Vrsqrts,  typeof(OpCode32SimdReg));
            SetA32("111111100xxxxxxxxxxx101xx0x0xxxx", InstName.Vsel,    InstEmit32.Vsel,     typeof(OpCode32SimdSel));
            SetA32("111100101x>>>xxxxxxx0101>xx1xxxx", InstName.Vshl,    InstEmit32.Vshl,     typeof(OpCode32SimdShImm));
            SetA32("1111001x0xxxxxxxxxxx0100xxx0xxxx", InstName.Vshl,    InstEmit32.Vshl_I,   typeof(OpCode32SimdReg));
            SetA32("1111001x1x>>>xxxxxxx0000>xx1xxxx", InstName.Vshr,    InstEmit32.Vshr,     typeof(OpCode32SimdShImm));
            SetA32("111100101x>>>xxxxxxx100000x1xxx0", InstName.Vshrn,   InstEmit32.Vshrn,    typeof(OpCode32SimdShImm));
            SetA32("<<<<11101x110001xxxx101x11x0xxxx", InstName.Vsqrt,   InstEmit32.Vsqrt_S,  typeof(OpCode32SimdS));
            SetA32("111101001x00xxxxxxxx<<00xxxxxxxx", InstName.Vst1,    InstEmit32.Vst1,     typeof(OpCode32SimdMemSingle));
            SetA32("111101000x00xxxxxxxx0111xxxxxxxx", InstName.Vst1,    InstEmit32.Vst1,     typeof(OpCode32SimdMemPair)); // Regs = 1.
            SetA32("111101000x00xxxxxxxx1010xxxxxxxx", InstName.Vst1,    InstEmit32.Vst1,     typeof(OpCode32SimdMemPair)); // Regs = 2.
            SetA32("111101000x00xxxxxxxx0110xxxxxxxx", InstName.Vst1,    InstEmit32.Vst1,     typeof(OpCode32SimdMemPair)); // Regs = 3.
            SetA32("111101000x00xxxxxxxx0010xxxxxxxx", InstName.Vst1,    InstEmit32.Vst1,     typeof(OpCode32SimdMemPair)); // Regs = 4.
            SetA32("111101001x00xxxxxxxx<<01xxxxxxxx", InstName.Vst2,    InstEmit32.Vst2,     typeof(OpCode32SimdMemSingle));
            SetA32("111101000x00xxxxxxxx100xxxxxxxxx", InstName.Vst2,    InstEmit32.Vst2,     typeof(OpCode32SimdMemPair)); // Regs = 1, inc = 1/2 (itype).
            SetA32("111101000x00xxxxxxxx0011xxxxxxxx", InstName.Vst2,    InstEmit32.Vst2,     typeof(OpCode32SimdMemPair)); // Regs = 2, inc = 2.
            SetA32("111101001x00xxxxxxxx<<10xxxxxxxx", InstName.Vst3,    InstEmit32.Vst3,     typeof(OpCode32SimdMemSingle));
            SetA32("111101000x00xxxxxxxx010xxxxxxxxx", InstName.Vst3,    InstEmit32.Vst3,     typeof(OpCode32SimdMemPair)); // Inc = 1/2 (itype).
            SetA32("111101001x00xxxxxxxx<<11xxxxxxxx", InstName.Vst4,    InstEmit32.Vst4,     typeof(OpCode32SimdMemSingle));
            SetA32("111101000x00xxxxxxxx000xxxxxxxxx", InstName.Vst4,    InstEmit32.Vst4,     typeof(OpCode32SimdMemPair)); // Inc = 1/2 (itype).
            SetA32("<<<<11001x00xxxxxxxx1011xxxxxxx0", InstName.Vstm,    InstEmit32.Vstm,     typeof(OpCode32SimdMemMult));
            SetA32("<<<<11001x10xxxxxxxx1011xxxxxxx0", InstName.Vstm,    InstEmit32.Vstm,     typeof(OpCode32SimdMemMult));
            SetA32("<<<<11010x10xxxxxxxx1011xxxxxxx0", InstName.Vstm,    InstEmit32.Vstm,     typeof(OpCode32SimdMemMult));
            SetA32("<<<<11001x00xxxxxxxx1010xxxxxxxx", InstName.Vstm,    InstEmit32.Vstm,     typeof(OpCode32SimdMemMult));
            SetA32("<<<<11001x10xxxxxxxx1010xxxxxxxx", InstName.Vstm,    InstEmit32.Vstm,     typeof(OpCode32SimdMemMult));
            SetA32("<<<<11010x10xxxxxxxx1010xxxxxxxx", InstName.Vstm,    InstEmit32.Vstm,     typeof(OpCode32SimdMemMult));
            SetA32("<<<<1101xx00xxxxxxxx101xxxxxxxxx", InstName.Vstr,    InstEmit32.Vstr,     typeof(OpCode32SimdMemImm));
            SetA32("111100110xxxxxxxxxxx1000xxx0xxxx", InstName.Vsub,    InstEmit32.Vsub_I,   typeof(OpCode32SimdReg));
            SetA32("<<<<11100x11xxxxxxxx101xx1x0xxxx", InstName.Vsub,    InstEmit32.Vsub_S,   typeof(OpCode32SimdRegS));
            SetA32("111100100x10xxxxxxxx1101xxx0xxxx", InstName.Vsub,    InstEmit32.Vsub_V,   typeof(OpCode32SimdReg));
            SetA32("111100111x11xxxxxxxx10xxxxx0xxxx", InstName.Vtbl,    InstEmit32.Vtbl,     typeof(OpCode32SimdTbl));
            SetA32("111100111x11<<10xxxx00001xx0xxxx", InstName.Vtrn,    InstEmit32.Vtrn,     typeof(OpCode32SimdCmpZ));
            SetA32("111100111x11<<10xxxx00010xx0xxxx", InstName.Vuzp,    InstEmit32.Vuzp,     typeof(OpCode32SimdCmpZ));
            SetA32("111100111x11<<10xxxx00011xx0xxxx", InstName.Vzip,    InstEmit32.Vzip,     typeof(OpCode32SimdCmpZ));
#endregion

            FillFastLookupTable(_instA32FastLookup, _allInstA32);
            FillFastLookupTable(_instT32FastLookup, _allInstT32);
            FillFastLookupTable(_instA64FastLookup, _allInstA64);
        }

        private static void FillFastLookupTable(InstInfo[][] table, List<InstInfo> allInsts)
        {
            List<InstInfo>[] temp = new List<InstInfo>[FastLookupSize];

            for (int index = 0; index < FastLookupSize; index++)
            {
                temp[index] = new List<InstInfo>();
            }

            foreach (InstInfo inst in allInsts)
            {
                int mask  = ToFastLookupIndex(inst.Mask);
                int value = ToFastLookupIndex(inst.Value);

                for (int index = 0; index < FastLookupSize; index++)
                {
                    if ((index & mask) == value)
                    {
                        temp[index].Add(inst);
                    }
                }
            }

            for (int index = 0; index < FastLookupSize; index++)
            {
                table[index] = temp[index].ToArray();
            }
        }

        private static void SetA32(string encoding, InstName name, InstEmitter emitter, Type type)
        {
            Set(encoding, ExecutionMode.Aarch32Arm, new InstDescriptor(name, emitter), type);
        }

        private static void SetT32(string encoding, InstName name, InstEmitter emitter, Type type)
        {
            Set(encoding, ExecutionMode.Aarch32Thumb, new InstDescriptor(name, emitter), type);
        }

        private static void SetA64(string encoding, InstName name, InstEmitter emitter, Type type)
        {
            Set(encoding, ExecutionMode.Aarch64, new InstDescriptor(name, emitter), type);
        }

        private static void Set(string encoding, ExecutionMode mode, InstDescriptor inst, Type type)
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
                InsertInst(new InstInfo(xMask, value, inst, type), mode);

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
                    InsertInst(new InstInfo(xMask, value | mask, inst, type), mode);
                }
            }
        }

        private static void InsertInst(InstInfo info, ExecutionMode mode)
        {
            switch (mode)
            {
                case ExecutionMode.Aarch32Arm:   _allInstA32.Add(info); break;
                case ExecutionMode.Aarch32Thumb: _allInstT32.Add(info); break;
                case ExecutionMode.Aarch64:      _allInstA64.Add(info); break;
            }
        }

        public static (InstDescriptor inst, Type type) GetInstA32(int opCode)
        {
            return GetInstFromList(_instA32FastLookup[ToFastLookupIndex(opCode)], opCode);
        }

        public static (InstDescriptor inst, Type type) GetInstT32(int opCode)
        {
            return GetInstFromList(_instT32FastLookup[ToFastLookupIndex(opCode)], opCode);
        }

        public static (InstDescriptor inst, Type type) GetInstA64(int opCode)
        {
            return GetInstFromList(_instA64FastLookup[ToFastLookupIndex(opCode)], opCode);
        }

        private static (InstDescriptor inst, Type type) GetInstFromList(InstInfo[] insts, int opCode)
        {
            foreach (InstInfo info in insts)
            {
                if ((opCode & info.Mask) == info.Value)
                {
                    return (info.Inst, info.Type);
                }
            }

            return (new InstDescriptor(InstName.Und, InstEmit.Und), typeof(OpCode));
        }

        private static int ToFastLookupIndex(int value)
        {
            return ((value >> 10) & 0x00F) | ((value >> 18) & 0xFF0);
        }
    }
}
