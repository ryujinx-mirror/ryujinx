using ChocolArm64.Decoder;
using ChocolArm64.Decoder32;
using ChocolArm64.Instruction;
using ChocolArm64.Instruction32;
using ChocolArm64.State;
using System;
using System.Collections.Generic;

namespace ChocolArm64
{
    static class AOpCodeTable
    {
        static AOpCodeTable()
        {
#region "OpCode Table (AArch32)"
            //Integer
            SetA32("<<<<1010xxxxxxxxxxxxxxxxxxxxxxxx", A32InstInterpret.B,      typeof(A32OpCodeBImmAl));
            SetA32("<<<<1011xxxxxxxxxxxxxxxxxxxxxxxx", A32InstInterpret.Bl,     typeof(A32OpCodeBImmAl));
            SetA32("1111101xxxxxxxxxxxxxxxxxxxxxxxxx", A32InstInterpret.Blx,    typeof(A32OpCodeBImmAl));
#endregion

#region "OpCode Table (AArch64)"
            //Integer
            SetA64("x0011010000xxxxx000000xxxxxxxxxx", AInstEmit.Adc,           typeof(AOpCodeAluRs));
            SetA64("x0111010000xxxxx000000xxxxxxxxxx", AInstEmit.Adcs,          typeof(AOpCodeAluRs));
            SetA64("x00100010xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Add,           typeof(AOpCodeAluImm));
            SetA64("00001011<<0xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Add,           typeof(AOpCodeAluRs));
            SetA64("10001011<<0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Add,           typeof(AOpCodeAluRs));
            SetA64("x0001011001xxxxxxxx0xxxxxxxxxxxx", AInstEmit.Add,           typeof(AOpCodeAluRx));
            SetA64("x0001011001xxxxxxxx100xxxxxxxxxx", AInstEmit.Add,           typeof(AOpCodeAluRx));
            SetA64("x01100010xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Adds,          typeof(AOpCodeAluImm));
            SetA64("00101011<<0xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Adds,          typeof(AOpCodeAluRs));
            SetA64("10101011<<0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Adds,          typeof(AOpCodeAluRs));
            SetA64("x0101011001xxxxxxxx0xxxxxxxxxxxx", AInstEmit.Adds,          typeof(AOpCodeAluRx));
            SetA64("x0101011001xxxxxxxx100xxxxxxxxxx", AInstEmit.Adds,          typeof(AOpCodeAluRx));
            SetA64("0xx10000xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Adr,           typeof(AOpCodeAdr));
            SetA64("1xx10000xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Adrp,          typeof(AOpCodeAdr));
            SetA64("0001001000xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.And,           typeof(AOpCodeAluImm));
            SetA64("100100100xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.And,           typeof(AOpCodeAluImm));
            SetA64("00001010xx0xxxxx0xxxxxxxxxxxxxxx", AInstEmit.And,           typeof(AOpCodeAluRs));
            SetA64("10001010xx0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.And,           typeof(AOpCodeAluRs));
            SetA64("0111001000xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ands,          typeof(AOpCodeAluImm));
            SetA64("111100100xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ands,          typeof(AOpCodeAluImm));
            SetA64("01101010xx0xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Ands,          typeof(AOpCodeAluRs));
            SetA64("11101010xx0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ands,          typeof(AOpCodeAluRs));
            SetA64("x0011010110xxxxx001010xxxxxxxxxx", AInstEmit.Asrv,          typeof(AOpCodeAluRs));
            SetA64("000101xxxxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.B,             typeof(AOpCodeBImmAl));
            SetA64("01010100xxxxxxxxxxxxxxxxxxx0xxxx", AInstEmit.B_Cond,        typeof(AOpCodeBImmCond));
            SetA64("00110011000xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Bfm,           typeof(AOpCodeBfm));
            SetA64("1011001101xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Bfm,           typeof(AOpCodeBfm));
            SetA64("00001010xx1xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Bic,           typeof(AOpCodeAluRs));
            SetA64("10001010xx1xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Bic,           typeof(AOpCodeAluRs));
            SetA64("01101010xx1xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Bics,          typeof(AOpCodeAluRs));
            SetA64("11101010xx1xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Bics,          typeof(AOpCodeAluRs));
            SetA64("100101xxxxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Bl,            typeof(AOpCodeBImmAl));
            SetA64("1101011000111111000000xxxxx00000", AInstEmit.Blr,           typeof(AOpCodeBReg));
            SetA64("1101011000011111000000xxxxx00000", AInstEmit.Br,            typeof(AOpCodeBReg));
            SetA64("11010100001xxxxxxxxxxxxxxxx00000", AInstEmit.Brk,           typeof(AOpCodeException));
            SetA64("x0110101xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Cbnz,          typeof(AOpCodeBImmCmp));
            SetA64("x0110100xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Cbz,           typeof(AOpCodeBImmCmp));
            SetA64("x0111010010xxxxxxxxx10xxxxx0xxxx", AInstEmit.Ccmn,          typeof(AOpCodeCcmpImm));
            SetA64("x0111010010xxxxxxxxx00xxxxx0xxxx", AInstEmit.Ccmn,          typeof(AOpCodeCcmpReg));
            SetA64("x1111010010xxxxxxxxx10xxxxx0xxxx", AInstEmit.Ccmp,          typeof(AOpCodeCcmpImm));
            SetA64("x1111010010xxxxxxxxx00xxxxx0xxxx", AInstEmit.Ccmp,          typeof(AOpCodeCcmpReg));
            SetA64("11010101000000110011xxxx01011111", AInstEmit.Clrex,         typeof(AOpCodeSystem));
            SetA64("x101101011000000000101xxxxxxxxxx", AInstEmit.Cls,           typeof(AOpCodeAlu));
            SetA64("x101101011000000000100xxxxxxxxxx", AInstEmit.Clz,           typeof(AOpCodeAlu));
            SetA64("00011010110xxxxx010000xxxxxxxxxx", AInstEmit.Crc32b,        typeof(AOpCodeAluRs));
            SetA64("00011010110xxxxx010001xxxxxxxxxx", AInstEmit.Crc32h,        typeof(AOpCodeAluRs));
            SetA64("00011010110xxxxx010010xxxxxxxxxx", AInstEmit.Crc32w,        typeof(AOpCodeAluRs));
            SetA64("10011010110xxxxx010011xxxxxxxxxx", AInstEmit.Crc32x,        typeof(AOpCodeAluRs));
            SetA64("00011010110xxxxx010100xxxxxxxxxx", AInstEmit.Crc32cb,       typeof(AOpCodeAluRs));
            SetA64("00011010110xxxxx010101xxxxxxxxxx", AInstEmit.Crc32ch,       typeof(AOpCodeAluRs));
            SetA64("00011010110xxxxx010110xxxxxxxxxx", AInstEmit.Crc32cw,       typeof(AOpCodeAluRs));
            SetA64("10011010110xxxxx010111xxxxxxxxxx", AInstEmit.Crc32cx,       typeof(AOpCodeAluRs));
            SetA64("x0011010100xxxxxxxxx00xxxxxxxxxx", AInstEmit.Csel,          typeof(AOpCodeCsel));
            SetA64("x0011010100xxxxxxxxx01xxxxxxxxxx", AInstEmit.Csinc,         typeof(AOpCodeCsel));
            SetA64("x1011010100xxxxxxxxx00xxxxxxxxxx", AInstEmit.Csinv,         typeof(AOpCodeCsel));
            SetA64("x1011010100xxxxxxxxx01xxxxxxxxxx", AInstEmit.Csneg,         typeof(AOpCodeCsel));
            SetA64("11010101000000110011xxxx10111111", AInstEmit.Dmb,           typeof(AOpCodeSystem));
            SetA64("11010101000000110011xxxx10011111", AInstEmit.Dsb,           typeof(AOpCodeSystem));
            SetA64("01001010xx1xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Eon,           typeof(AOpCodeAluRs));
            SetA64("11001010xx1xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Eon,           typeof(AOpCodeAluRs));
            SetA64("0101001000xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Eor,           typeof(AOpCodeAluImm));
            SetA64("110100100xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Eor,           typeof(AOpCodeAluImm));
            SetA64("01001010xx0xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Eor,           typeof(AOpCodeAluRs));
            SetA64("11001010xx0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Eor,           typeof(AOpCodeAluRs));
            SetA64("00010011100xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Extr,          typeof(AOpCodeAluRs));
            SetA64("10010011110xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Extr,          typeof(AOpCodeAluRs));
            SetA64("11010101000000110010xxxxxxx11111", AInstEmit.Hint,          typeof(AOpCodeSystem));
            SetA64("xx001000110xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Ldar,          typeof(AOpCodeMemEx));
            SetA64("1x001000011xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Ldaxp,         typeof(AOpCodeMemEx));
            SetA64("xx001000010xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Ldaxr,         typeof(AOpCodeMemEx));
            SetA64("<<10100xx1xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldp,           typeof(AOpCodeMemPair));
            SetA64("xx111000010xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeMemImm));
            SetA64("xx11100101xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeMemImm));
            SetA64("xx111000011xxxxxxxxx10xxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeMemReg));
            SetA64("xx011000xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.LdrLit,        typeof(AOpCodeMemLit));
            SetA64("0x1110001x0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldrs,          typeof(AOpCodeMemImm));
            SetA64("0x1110011xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldrs,          typeof(AOpCodeMemImm));
            SetA64("10111000100xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldrs,          typeof(AOpCodeMemImm));
            SetA64("1011100110xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldrs,          typeof(AOpCodeMemImm));
            SetA64("0x1110001x1xxxxxxxxx10xxxxxxxxxx", AInstEmit.Ldrs,          typeof(AOpCodeMemReg));
            SetA64("10111000101xxxxxxxxx10xxxxxxxxxx", AInstEmit.Ldrs,          typeof(AOpCodeMemReg));
            SetA64("xx001000010xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Ldxr,          typeof(AOpCodeMemEx));
            SetA64("1x001000011xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Ldxp,          typeof(AOpCodeMemEx));
            SetA64("x0011010110xxxxx001000xxxxxxxxxx", AInstEmit.Lslv,          typeof(AOpCodeAluRs));
            SetA64("x0011010110xxxxx001001xxxxxxxxxx", AInstEmit.Lsrv,          typeof(AOpCodeAluRs));
            SetA64("x0011011000xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Madd,          typeof(AOpCodeMul));
            SetA64("0111001010xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Movk,          typeof(AOpCodeMov));
            SetA64("111100101xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Movk,          typeof(AOpCodeMov));
            SetA64("0001001010xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Movn,          typeof(AOpCodeMov));
            SetA64("100100101xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Movn,          typeof(AOpCodeMov));
            SetA64("0101001010xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Movz,          typeof(AOpCodeMov));
            SetA64("110100101xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Movz,          typeof(AOpCodeMov));
            SetA64("110101010011xxxxxxxxxxxxxxxxxxxx", AInstEmit.Mrs,           typeof(AOpCodeSystem));
            SetA64("110101010001xxxxxxxxxxxxxxxxxxxx", AInstEmit.Msr,           typeof(AOpCodeSystem));
            SetA64("x0011011000xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Msub,          typeof(AOpCodeMul));
            SetA64("11010101000000110010000000011111", AInstEmit.Nop,           typeof(AOpCodeSystem));
            SetA64("00101010xx1xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Orn,           typeof(AOpCodeAluRs));
            SetA64("10101010xx1xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Orn,           typeof(AOpCodeAluRs));
            SetA64("0011001000xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Orr,           typeof(AOpCodeAluImm));
            SetA64("101100100xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Orr,           typeof(AOpCodeAluImm));
            SetA64("00101010xx0xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Orr,           typeof(AOpCodeAluRs));
            SetA64("10101010xx0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Orr,           typeof(AOpCodeAluRs));
            SetA64("1111100110xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Pfrm,          typeof(AOpCodeMemImm));
            SetA64("11111000100xxxxxxxxx00xxxxxxxxxx", AInstEmit.Pfrm,          typeof(AOpCodeMemImm));
            SetA64("11011000xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Pfrm,          typeof(AOpCodeMemLit));
            SetA64("x101101011000000000000xxxxxxxxxx", AInstEmit.Rbit,          typeof(AOpCodeAlu));
            SetA64("1101011001011111000000xxxxx00000", AInstEmit.Ret,           typeof(AOpCodeBReg));
            SetA64("x101101011000000000001xxxxxxxxxx", AInstEmit.Rev16,         typeof(AOpCodeAlu));
            SetA64("x101101011000000000010xxxxxxxxxx", AInstEmit.Rev32,         typeof(AOpCodeAlu));
            SetA64("1101101011000000000011xxxxxxxxxx", AInstEmit.Rev64,         typeof(AOpCodeAlu));
            SetA64("x0011010110xxxxx001011xxxxxxxxxx", AInstEmit.Rorv,          typeof(AOpCodeAluRs));
            SetA64("x1011010000xxxxx000000xxxxxxxxxx", AInstEmit.Sbc,           typeof(AOpCodeAluRs));
            SetA64("x1111010000xxxxx000000xxxxxxxxxx", AInstEmit.Sbcs,          typeof(AOpCodeAluRs));
            SetA64("00010011000xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Sbfm,          typeof(AOpCodeBfm));
            SetA64("1001001101xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Sbfm,          typeof(AOpCodeBfm));
            SetA64("x0011010110xxxxx000011xxxxxxxxxx", AInstEmit.Sdiv,          typeof(AOpCodeAluRs));
            SetA64("10011011001xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Smaddl,        typeof(AOpCodeMul));
            SetA64("10011011001xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Smsubl,        typeof(AOpCodeMul));
            SetA64("10011011010xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Smulh,         typeof(AOpCodeMul));
            SetA64("xx001000100xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Stlr,          typeof(AOpCodeMemEx));
            SetA64("1x001000001xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Stlxp,         typeof(AOpCodeMemEx));
            SetA64("xx001000000xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Stlxr,         typeof(AOpCodeMemEx));
            SetA64("x010100xx0xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Stp,           typeof(AOpCodeMemPair));
            SetA64("xx111000000xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeMemImm));
            SetA64("xx11100100xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeMemImm));
            SetA64("xx111000001xxxxxxxxx10xxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeMemReg));
            SetA64("1x001000001xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Stxp,          typeof(AOpCodeMemEx));
            SetA64("xx001000000xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Stxr,          typeof(AOpCodeMemEx));
            SetA64("x10100010xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Sub,           typeof(AOpCodeAluImm));
            SetA64("01001011<<0xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Sub,           typeof(AOpCodeAluRs));
            SetA64("11001011<<0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Sub,           typeof(AOpCodeAluRs));
            SetA64("x1001011001xxxxxxxx0xxxxxxxxxxxx", AInstEmit.Sub,           typeof(AOpCodeAluRx));
            SetA64("x1001011001xxxxxxxx100xxxxxxxxxx", AInstEmit.Sub,           typeof(AOpCodeAluRx));
            SetA64("x11100010xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Subs,          typeof(AOpCodeAluImm));
            SetA64("01101011<<0xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Subs,          typeof(AOpCodeAluRs));
            SetA64("11101011<<0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Subs,          typeof(AOpCodeAluRs));
            SetA64("x1101011001xxxxxxxx0xxxxxxxxxxxx", AInstEmit.Subs,          typeof(AOpCodeAluRx));
            SetA64("x1101011001xxxxxxxx100xxxxxxxxxx", AInstEmit.Subs,          typeof(AOpCodeAluRx));
            SetA64("11010100000xxxxxxxxxxxxxxxx00001", AInstEmit.Svc,           typeof(AOpCodeException));
            SetA64("1101010100001xxxxxxxxxxxxxxxxxxx", AInstEmit.Sys,           typeof(AOpCodeSystem));
            SetA64("x0110111xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Tbnz,          typeof(AOpCodeBImmTest));
            SetA64("x0110110xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Tbz,           typeof(AOpCodeBImmTest));
            SetA64("01010011000xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Ubfm,          typeof(AOpCodeBfm));
            SetA64("1101001101xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ubfm,          typeof(AOpCodeBfm));
            SetA64("x0011010110xxxxx000010xxxxxxxxxx", AInstEmit.Udiv,          typeof(AOpCodeAluRs));
            SetA64("10011011101xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Umaddl,        typeof(AOpCodeMul));
            SetA64("10011011101xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Umsubl,        typeof(AOpCodeMul));
            SetA64("10011011110xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Umulh,         typeof(AOpCodeMul));

            //Vector
            SetA64("0101111011100000101110xxxxxxxxxx", AInstEmit.Abs_S,         typeof(AOpCodeSimd));
            SetA64("0>001110<<100000101110xxxxxxxxxx", AInstEmit.Abs_V,         typeof(AOpCodeSimd));
            SetA64("01011110111xxxxx100001xxxxxxxxxx", AInstEmit.Add_S,         typeof(AOpCodeSimdReg));
            SetA64("0>001110<<1xxxxx100001xxxxxxxxxx", AInstEmit.Add_V,         typeof(AOpCodeSimdReg));
            SetA64("0x001110<<1xxxxx010000xxxxxxxxxx", AInstEmit.Addhn_V,       typeof(AOpCodeSimdReg));
            SetA64("0101111011110001101110xxxxxxxxxx", AInstEmit.Addp_S,        typeof(AOpCodeSimd));
            SetA64("0>001110<<1xxxxx101111xxxxxxxxxx", AInstEmit.Addp_V,        typeof(AOpCodeSimdReg));
            SetA64("000011100x110001101110xxxxxxxxxx", AInstEmit.Addv_V,        typeof(AOpCodeSimd));
            SetA64("01001110<<110001101110xxxxxxxxxx", AInstEmit.Addv_V,        typeof(AOpCodeSimd));
            SetA64("0100111000101000010110xxxxxxxxxx", AInstEmit.Aesd_V,        typeof(AOpCodeSimd));
            SetA64("0100111000101000010010xxxxxxxxxx", AInstEmit.Aese_V,        typeof(AOpCodeSimd));
            SetA64("0100111000101000011110xxxxxxxxxx", AInstEmit.Aesimc_V,      typeof(AOpCodeSimd));
            SetA64("0100111000101000011010xxxxxxxxxx", AInstEmit.Aesmc_V,       typeof(AOpCodeSimd));
            SetA64("0x001110001xxxxx000111xxxxxxxxxx", AInstEmit.And_V,         typeof(AOpCodeSimdReg));
            SetA64("0x001110011xxxxx000111xxxxxxxxxx", AInstEmit.Bic_V,         typeof(AOpCodeSimdReg));
            SetA64("0x10111100000xxx<<x101xxxxxxxxxx", AInstEmit.Bic_Vi,        typeof(AOpCodeSimdImm));
            SetA64("0x101110111xxxxx000111xxxxxxxxxx", AInstEmit.Bif_V,         typeof(AOpCodeSimdReg));
            SetA64("0x101110101xxxxx000111xxxxxxxxxx", AInstEmit.Bit_V,         typeof(AOpCodeSimdReg));
            SetA64("0x101110011xxxxx000111xxxxxxxxxx", AInstEmit.Bsl_V,         typeof(AOpCodeSimdReg));
            SetA64("0x001110<<100000010010xxxxxxxxxx", AInstEmit.Cls_V,         typeof(AOpCodeSimd));
            SetA64("0x101110<<100000010010xxxxxxxxxx", AInstEmit.Clz_V,         typeof(AOpCodeSimd));
            SetA64("01111110111xxxxx100011xxxxxxxxxx", AInstEmit.Cmeq_S,        typeof(AOpCodeSimdReg));
            SetA64("0101111011100000100110xxxxxxxxxx", AInstEmit.Cmeq_S,        typeof(AOpCodeSimd));
            SetA64("0>101110<<1xxxxx100011xxxxxxxxxx", AInstEmit.Cmeq_V,        typeof(AOpCodeSimdReg));
            SetA64("0>001110<<100000100110xxxxxxxxxx", AInstEmit.Cmeq_V,        typeof(AOpCodeSimd));
            SetA64("01011110111xxxxx001111xxxxxxxxxx", AInstEmit.Cmge_S,        typeof(AOpCodeSimdReg));
            SetA64("0111111011100000100010xxxxxxxxxx", AInstEmit.Cmge_S,        typeof(AOpCodeSimd));
            SetA64("0>001110<<1xxxxx001111xxxxxxxxxx", AInstEmit.Cmge_V,        typeof(AOpCodeSimdReg));
            SetA64("0>101110<<100000100010xxxxxxxxxx", AInstEmit.Cmge_V,        typeof(AOpCodeSimd));
            SetA64("01011110111xxxxx001101xxxxxxxxxx", AInstEmit.Cmgt_S,        typeof(AOpCodeSimdReg));
            SetA64("0101111011100000100010xxxxxxxxxx", AInstEmit.Cmgt_S,        typeof(AOpCodeSimd));
            SetA64("0>001110<<1xxxxx001101xxxxxxxxxx", AInstEmit.Cmgt_V,        typeof(AOpCodeSimdReg));
            SetA64("0>001110<<100000100010xxxxxxxxxx", AInstEmit.Cmgt_V,        typeof(AOpCodeSimd));
            SetA64("01111110111xxxxx001101xxxxxxxxxx", AInstEmit.Cmhi_S,        typeof(AOpCodeSimdReg));
            SetA64("0>101110<<1xxxxx001101xxxxxxxxxx", AInstEmit.Cmhi_V,        typeof(AOpCodeSimdReg));
            SetA64("01111110111xxxxx001111xxxxxxxxxx", AInstEmit.Cmhs_S,        typeof(AOpCodeSimdReg));
            SetA64("0>101110<<1xxxxx001111xxxxxxxxxx", AInstEmit.Cmhs_V,        typeof(AOpCodeSimdReg));
            SetA64("0111111011100000100110xxxxxxxxxx", AInstEmit.Cmle_S,        typeof(AOpCodeSimd));
            SetA64("0>101110<<100000100110xxxxxxxxxx", AInstEmit.Cmle_V,        typeof(AOpCodeSimd));
            SetA64("0101111011100000101010xxxxxxxxxx", AInstEmit.Cmlt_S,        typeof(AOpCodeSimd));
            SetA64("0>001110<<100000101010xxxxxxxxxx", AInstEmit.Cmlt_V,        typeof(AOpCodeSimd));
            SetA64("01011110111xxxxx100011xxxxxxxxxx", AInstEmit.Cmtst_S,       typeof(AOpCodeSimdReg));
            SetA64("0>001110<<1xxxxx100011xxxxxxxxxx", AInstEmit.Cmtst_V,       typeof(AOpCodeSimdReg));
            SetA64("0x00111000100000010110xxxxxxxxxx", AInstEmit.Cnt_V,         typeof(AOpCodeSimd));
            SetA64("0x001110000xxxxx000011xxxxxxxxxx", AInstEmit.Dup_Gp,        typeof(AOpCodeSimdIns));
            SetA64("01011110000xxxxx000001xxxxxxxxxx", AInstEmit.Dup_S,         typeof(AOpCodeSimdIns));
            SetA64("0x001110000xxxxx000001xxxxxxxxxx", AInstEmit.Dup_V,         typeof(AOpCodeSimdIns));
            SetA64("0x101110001xxxxx000111xxxxxxxxxx", AInstEmit.Eor_V,         typeof(AOpCodeSimdReg));
            SetA64("0>101110000xxxxx0<xxx0xxxxxxxxxx", AInstEmit.Ext_V,         typeof(AOpCodeSimdExt));
            SetA64("011111101x1xxxxx110101xxxxxxxxxx", AInstEmit.Fabd_S,        typeof(AOpCodeSimdReg));
            SetA64("000111100x100000110000xxxxxxxxxx", AInstEmit.Fabs_S,        typeof(AOpCodeSimd));
            SetA64("0>0011101<100000111110xxxxxxxxxx", AInstEmit.Fabs_V,        typeof(AOpCodeSimd));
            SetA64("000111100x1xxxxx001010xxxxxxxxxx", AInstEmit.Fadd_S,        typeof(AOpCodeSimdReg));
            SetA64("0>0011100<1xxxxx110101xxxxxxxxxx", AInstEmit.Fadd_V,        typeof(AOpCodeSimdReg));
            SetA64("011111100x110000110110xxxxxxxxxx", AInstEmit.Faddp_S,       typeof(AOpCodeSimd));
            SetA64("0>1011100<1xxxxx110101xxxxxxxxxx", AInstEmit.Faddp_V,       typeof(AOpCodeSimdReg));
            SetA64("000111100x1xxxxxxxxx01xxxxx0xxxx", AInstEmit.Fccmp_S,       typeof(AOpCodeSimdFcond));
            SetA64("000111100x1xxxxxxxxx01xxxxx1xxxx", AInstEmit.Fccmpe_S,      typeof(AOpCodeSimdFcond));
            SetA64("010111100x1xxxxx111001xxxxxxxxxx", AInstEmit.Fcmeq_S,       typeof(AOpCodeSimdReg));
            SetA64("010111101x100000110110xxxxxxxxxx", AInstEmit.Fcmeq_S,       typeof(AOpCodeSimd));
            SetA64("0>0011100<1xxxxx111001xxxxxxxxxx", AInstEmit.Fcmeq_V,       typeof(AOpCodeSimdReg));
            SetA64("0>0011101<100000110110xxxxxxxxxx", AInstEmit.Fcmeq_V,       typeof(AOpCodeSimd));
            SetA64("011111100x1xxxxx111001xxxxxxxxxx", AInstEmit.Fcmge_S,       typeof(AOpCodeSimdReg));
            SetA64("011111101x100000110010xxxxxxxxxx", AInstEmit.Fcmge_S,       typeof(AOpCodeSimd));
            SetA64("0>1011100<1xxxxx111001xxxxxxxxxx", AInstEmit.Fcmge_V,       typeof(AOpCodeSimdReg));
            SetA64("0>1011101<100000110010xxxxxxxxxx", AInstEmit.Fcmge_V,       typeof(AOpCodeSimd));
            SetA64("011111101x1xxxxx111001xxxxxxxxxx", AInstEmit.Fcmgt_S,       typeof(AOpCodeSimdReg));
            SetA64("010111101x100000110010xxxxxxxxxx", AInstEmit.Fcmgt_S,       typeof(AOpCodeSimd));
            SetA64("0>1011101<1xxxxx111001xxxxxxxxxx", AInstEmit.Fcmgt_V,       typeof(AOpCodeSimdReg));
            SetA64("0>0011101<100000110010xxxxxxxxxx", AInstEmit.Fcmgt_V,       typeof(AOpCodeSimd));
            SetA64("011111101x100000110110xxxxxxxxxx", AInstEmit.Fcmle_S,       typeof(AOpCodeSimd));
            SetA64("0>1011101<100000110110xxxxxxxxxx", AInstEmit.Fcmle_V,       typeof(AOpCodeSimd));
            SetA64("010111101x100000111010xxxxxxxxxx", AInstEmit.Fcmlt_S,       typeof(AOpCodeSimd));
            SetA64("0>0011101<100000111010xxxxxxxxxx", AInstEmit.Fcmlt_V,       typeof(AOpCodeSimd));
            SetA64("000111100x1xxxxx001000xxxxx0x000", AInstEmit.Fcmp_S,        typeof(AOpCodeSimdReg));
            SetA64("000111100x1xxxxx001000xxxxx1x000", AInstEmit.Fcmpe_S,       typeof(AOpCodeSimdReg));
            SetA64("000111100x1xxxxxxxxx11xxxxxxxxxx", AInstEmit.Fcsel_S,       typeof(AOpCodeSimdFcond));
            SetA64("000111100x10001xx10000xxxxxxxxxx", AInstEmit.Fcvt_S,        typeof(AOpCodeSimd));
            SetA64("x00111100x100100000000xxxxxxxxxx", AInstEmit.Fcvtas_Gp,     typeof(AOpCodeSimdCvt));
            SetA64("x00111100x100101000000xxxxxxxxxx", AInstEmit.Fcvtau_Gp,     typeof(AOpCodeSimdCvt));
            SetA64("0x0011100x100001011110xxxxxxxxxx", AInstEmit.Fcvtl_V,       typeof(AOpCodeSimd));
            SetA64("x00111100x110000000000xxxxxxxxxx", AInstEmit.Fcvtms_Gp,     typeof(AOpCodeSimdCvt));
            SetA64("x00111100x110001000000xxxxxxxxxx", AInstEmit.Fcvtmu_Gp,     typeof(AOpCodeSimdCvt));
            SetA64("0x0011100x100001011010xxxxxxxxxx", AInstEmit.Fcvtn_V,       typeof(AOpCodeSimd));
            SetA64("x00111100x101000000000xxxxxxxxxx", AInstEmit.Fcvtps_Gp,     typeof(AOpCodeSimdCvt));
            SetA64("x00111100x101001000000xxxxxxxxxx", AInstEmit.Fcvtpu_Gp,     typeof(AOpCodeSimdCvt));
            SetA64("x00111100x111000000000xxxxxxxxxx", AInstEmit.Fcvtzs_Gp,     typeof(AOpCodeSimdCvt));
            SetA64("x00111100x011000xxxxxxxxxxxxxxxx", AInstEmit.Fcvtzs_Gp_Fix, typeof(AOpCodeSimdCvt));
            SetA64("010111101x100001101110xxxxxxxxxx", AInstEmit.Fcvtzs_S,      typeof(AOpCodeSimd));
            SetA64("0>0011101<100001101110xxxxxxxxxx", AInstEmit.Fcvtzs_V,      typeof(AOpCodeSimd));
            SetA64("0x0011110>>xxxxx111111xxxxxxxxxx", AInstEmit.Fcvtzs_V,      typeof(AOpCodeSimdShImm));
            SetA64("x00111100x111001000000xxxxxxxxxx", AInstEmit.Fcvtzu_Gp,     typeof(AOpCodeSimdCvt));
            SetA64("x00111100x011001xxxxxxxxxxxxxxxx", AInstEmit.Fcvtzu_Gp_Fix, typeof(AOpCodeSimdCvt));
            SetA64("011111101x100001101110xxxxxxxxxx", AInstEmit.Fcvtzu_S,      typeof(AOpCodeSimd));
            SetA64("0>1011101<100001101110xxxxxxxxxx", AInstEmit.Fcvtzu_V,      typeof(AOpCodeSimd));
            SetA64("0x1011110>>xxxxx111111xxxxxxxxxx", AInstEmit.Fcvtzu_V,      typeof(AOpCodeSimdShImm));
            SetA64("000111100x1xxxxx000110xxxxxxxxxx", AInstEmit.Fdiv_S,        typeof(AOpCodeSimdReg));
            SetA64("0>1011100<1xxxxx111111xxxxxxxxxx", AInstEmit.Fdiv_V,        typeof(AOpCodeSimdReg));
            SetA64("000111110x0xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Fmadd_S,       typeof(AOpCodeSimdReg));
            SetA64("000111100x1xxxxx010010xxxxxxxxxx", AInstEmit.Fmax_S,        typeof(AOpCodeSimdReg));
            SetA64("0>0011100<1xxxxx111101xxxxxxxxxx", AInstEmit.Fmax_V,        typeof(AOpCodeSimdReg));
            SetA64("000111100x1xxxxx011010xxxxxxxxxx", AInstEmit.Fmaxnm_S,      typeof(AOpCodeSimdReg));
            SetA64("0>0011100<1xxxxx110001xxxxxxxxxx", AInstEmit.Fmaxnm_V,      typeof(AOpCodeSimdReg));
            SetA64("000111100x1xxxxx010110xxxxxxxxxx", AInstEmit.Fmin_S,        typeof(AOpCodeSimdReg));
            SetA64("0>0011101<1xxxxx111101xxxxxxxxxx", AInstEmit.Fmin_V,        typeof(AOpCodeSimdReg));
            SetA64("000111100x1xxxxx011110xxxxxxxxxx", AInstEmit.Fminnm_S,      typeof(AOpCodeSimdReg));
            SetA64("0>0011101<1xxxxx110001xxxxxxxxxx", AInstEmit.Fminnm_V,      typeof(AOpCodeSimdReg));
            SetA64("010111111<<xxxxx0001x0xxxxxxxxxx", AInstEmit.Fmla_Se,       typeof(AOpCodeSimdRegElemF));
            SetA64("0>0011100<1xxxxx110011xxxxxxxxxx", AInstEmit.Fmla_V,        typeof(AOpCodeSimdReg));
            SetA64("0x0011111<<xxxxx0001x0xxxxxxxxxx", AInstEmit.Fmla_Ve,       typeof(AOpCodeSimdRegElemF));
            SetA64("0>0011101<1xxxxx110011xxxxxxxxxx", AInstEmit.Fmls_V,        typeof(AOpCodeSimdReg));
            SetA64("0x0011111<<xxxxx0101x0xxxxxxxxxx", AInstEmit.Fmls_Ve,       typeof(AOpCodeSimdRegElemF));
            SetA64("000111100x100000010000xxxxxxxxxx", AInstEmit.Fmov_S,        typeof(AOpCodeSimd));
            SetA64("00011110xx1xxxxxxxx100xxxxxxxxxx", AInstEmit.Fmov_Si,       typeof(AOpCodeSimdFmov));
            SetA64("0xx0111100000xxx111101xxxxxxxxxx", AInstEmit.Fmov_V,        typeof(AOpCodeSimdImm));
            SetA64("x00111100x100110000000xxxxxxxxxx", AInstEmit.Fmov_Ftoi,     typeof(AOpCodeSimdCvt));
            SetA64("x00111100x100111000000xxxxxxxxxx", AInstEmit.Fmov_Itof,     typeof(AOpCodeSimdCvt));
            SetA64("1001111010101110000000xxxxxxxxxx", AInstEmit.Fmov_Ftoi1,    typeof(AOpCodeSimdCvt));
            SetA64("1001111010101111000000xxxxxxxxxx", AInstEmit.Fmov_Itof1,    typeof(AOpCodeSimdCvt));
            SetA64("000111110x0xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Fmsub_S,       typeof(AOpCodeSimdReg));
            SetA64("000111100x1xxxxx000010xxxxxxxxxx", AInstEmit.Fmul_S,        typeof(AOpCodeSimdReg));
            SetA64("010111111<<xxxxx1001x0xxxxxxxxxx", AInstEmit.Fmul_Se,       typeof(AOpCodeSimdRegElemF));
            SetA64("0>1011100<1xxxxx110111xxxxxxxxxx", AInstEmit.Fmul_V,        typeof(AOpCodeSimdReg));
            SetA64("0x0011111<<xxxxx1001x0xxxxxxxxxx", AInstEmit.Fmul_Ve,       typeof(AOpCodeSimdRegElemF));
            SetA64("000111100x100001010000xxxxxxxxxx", AInstEmit.Fneg_S,        typeof(AOpCodeSimd));
            SetA64("0>1011101<100000111110xxxxxxxxxx", AInstEmit.Fneg_V,        typeof(AOpCodeSimd));
            SetA64("000111110x1xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Fnmadd_S,      typeof(AOpCodeSimdReg));
            SetA64("000111110x1xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Fnmsub_S,      typeof(AOpCodeSimdReg));
            SetA64("000111100x1xxxxx100010xxxxxxxxxx", AInstEmit.Fnmul_S,       typeof(AOpCodeSimdReg));
            SetA64("010111101x100001110110xxxxxxxxxx", AInstEmit.Frecpe_S,      typeof(AOpCodeSimd));
            SetA64("0>0011101<100001110110xxxxxxxxxx", AInstEmit.Frecpe_V,      typeof(AOpCodeSimd));
            SetA64("010111100x1xxxxx111111xxxxxxxxxx", AInstEmit.Frecps_S,      typeof(AOpCodeSimdReg));
            SetA64("0>0011100<1xxxxx111111xxxxxxxxxx", AInstEmit.Frecps_V,      typeof(AOpCodeSimdReg));
            SetA64("000111100x100110010000xxxxxxxxxx", AInstEmit.Frinta_S,      typeof(AOpCodeSimd));
            SetA64("0>1011100<100001100010xxxxxxxxxx", AInstEmit.Frinta_V,      typeof(AOpCodeSimd));
            SetA64("000111100x100111110000xxxxxxxxxx", AInstEmit.Frinti_S,      typeof(AOpCodeSimd));
            SetA64("0>1011101<100001100110xxxxxxxxxx", AInstEmit.Frinti_V,      typeof(AOpCodeSimd));
            SetA64("000111100x100101010000xxxxxxxxxx", AInstEmit.Frintm_S,      typeof(AOpCodeSimd));
            SetA64("0>0011100<100001100110xxxxxxxxxx", AInstEmit.Frintm_V,      typeof(AOpCodeSimd));
            SetA64("000111100x100100010000xxxxxxxxxx", AInstEmit.Frintn_S,      typeof(AOpCodeSimd));
            SetA64("0>0011100<100001100010xxxxxxxxxx", AInstEmit.Frintn_V,      typeof(AOpCodeSimd));
            SetA64("000111100x100100110000xxxxxxxxxx", AInstEmit.Frintp_S,      typeof(AOpCodeSimd));
            SetA64("0>0011101<100001100010xxxxxxxxxx", AInstEmit.Frintp_V,      typeof(AOpCodeSimd));
            SetA64("000111100x100111010000xxxxxxxxxx", AInstEmit.Frintx_S,      typeof(AOpCodeSimd));
            SetA64("0>1011100<100001100110xxxxxxxxxx", AInstEmit.Frintx_V,      typeof(AOpCodeSimd));
            SetA64("011111101x100001110110xxxxxxxxxx", AInstEmit.Frsqrte_S,     typeof(AOpCodeSimd));
            SetA64("0>1011101<100001110110xxxxxxxxxx", AInstEmit.Frsqrte_V,     typeof(AOpCodeSimd));
            SetA64("010111101x1xxxxx111111xxxxxxxxxx", AInstEmit.Frsqrts_S,     typeof(AOpCodeSimdReg));
            SetA64("0>0011101<1xxxxx111111xxxxxxxxxx", AInstEmit.Frsqrts_V,     typeof(AOpCodeSimdReg));
            SetA64("000111100x100001110000xxxxxxxxxx", AInstEmit.Fsqrt_S,       typeof(AOpCodeSimd));
            SetA64("000111100x1xxxxx001110xxxxxxxxxx", AInstEmit.Fsub_S,        typeof(AOpCodeSimdReg));
            SetA64("0>0011101<1xxxxx110101xxxxxxxxxx", AInstEmit.Fsub_V,        typeof(AOpCodeSimdReg));
            SetA64("01001110000xxxxx000111xxxxxxxxxx", AInstEmit.Ins_Gp,        typeof(AOpCodeSimdIns));
            SetA64("01101110000xxxxx0xxxx1xxxxxxxxxx", AInstEmit.Ins_V,         typeof(AOpCodeSimdIns));
            SetA64("0x00110001000000xxxxxxxxxxxxxxxx", AInstEmit.Ld__Vms,       typeof(AOpCodeSimdMemMs));
            SetA64("0x001100110xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ld__Vms,       typeof(AOpCodeSimdMemMs));
            SetA64("0x00110101x00000xxxxxxxxxxxxxxxx", AInstEmit.Ld__Vss,       typeof(AOpCodeSimdMemSs));
            SetA64("0x00110111xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ld__Vss,       typeof(AOpCodeSimdMemSs));
            SetA64("xx10110xx1xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldp,           typeof(AOpCodeSimdMemPair));
            SetA64("xx111100x10xxxxxxxxx00xxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeSimdMemImm));
            SetA64("xx111100x10xxxxxxxxx01xxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeSimdMemImm));
            SetA64("xx111100x10xxxxxxxxx11xxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeSimdMemImm));
            SetA64("xx111101x1xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeSimdMemImm));
            SetA64("xx111100x11xxxxxxxxx10xxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeSimdMemReg));
            SetA64("xx011100xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.LdrLit,        typeof(AOpCodeSimdMemLit));
            SetA64("0x001110<<1xxxxx100101xxxxxxxxxx", AInstEmit.Mla_V,         typeof(AOpCodeSimdReg));
            SetA64("0x101111xxxxxxxx0000x0xxxxxxxxxx", AInstEmit.Mla_Ve,        typeof(AOpCodeSimdRegElem));
            SetA64("0x101110<<1xxxxx100101xxxxxxxxxx", AInstEmit.Mls_V,         typeof(AOpCodeSimdReg));
            SetA64("0x00111100000xxx0xx001xxxxxxxxxx", AInstEmit.Movi_V,        typeof(AOpCodeSimdImm));
            SetA64("0x00111100000xxx10x001xxxxxxxxxx", AInstEmit.Movi_V,        typeof(AOpCodeSimdImm));
            SetA64("0x00111100000xxx110x01xxxxxxxxxx", AInstEmit.Movi_V,        typeof(AOpCodeSimdImm));
            SetA64("0xx0111100000xxx111001xxxxxxxxxx", AInstEmit.Movi_V,        typeof(AOpCodeSimdImm));
            SetA64("0x001110<<1xxxxx100111xxxxxxxxxx", AInstEmit.Mul_V,         typeof(AOpCodeSimdReg));
            SetA64("0x001111xxxxxxxx1000x0xxxxxxxxxx", AInstEmit.Mul_Ve,        typeof(AOpCodeSimdRegElem));
            SetA64("0x10111100000xxx0xx001xxxxxxxxxx", AInstEmit.Mvni_V,        typeof(AOpCodeSimdImm));
            SetA64("0x10111100000xxx10x001xxxxxxxxxx", AInstEmit.Mvni_V,        typeof(AOpCodeSimdImm));
            SetA64("0x10111100000xxx110x01xxxxxxxxxx", AInstEmit.Mvni_V,        typeof(AOpCodeSimdImm));
            SetA64("0111111011100000101110xxxxxxxxxx", AInstEmit.Neg_S,         typeof(AOpCodeSimd));
            SetA64("0>101110<<100000101110xxxxxxxxxx", AInstEmit.Neg_V,         typeof(AOpCodeSimd));
            SetA64("0x10111000100000010110xxxxxxxxxx", AInstEmit.Not_V,         typeof(AOpCodeSimd));
            SetA64("0x001110111xxxxx000111xxxxxxxxxx", AInstEmit.Orn_V,         typeof(AOpCodeSimdReg));
            SetA64("0x001110101xxxxx000111xxxxxxxxxx", AInstEmit.Orr_V,         typeof(AOpCodeSimdReg));
            SetA64("0x00111100000xxx<<x101xxxxxxxxxx", AInstEmit.Orr_Vi,        typeof(AOpCodeSimdImm));
            SetA64("0x101110<<1xxxxx010000xxxxxxxxxx", AInstEmit.Raddhn_V,      typeof(AOpCodeSimdReg));
            SetA64("0x10111001100000010110xxxxxxxxxx", AInstEmit.Rbit_V,        typeof(AOpCodeSimd));
            SetA64("0x00111000100000000110xxxxxxxxxx", AInstEmit.Rev16_V,       typeof(AOpCodeSimd));
            SetA64("0x1011100x100000000010xxxxxxxxxx", AInstEmit.Rev32_V,       typeof(AOpCodeSimd));
            SetA64("0x001110<<100000000010xxxxxxxxxx", AInstEmit.Rev64_V,       typeof(AOpCodeSimd));
            SetA64("0x101110<<1xxxxx011000xxxxxxxxxx", AInstEmit.Rsubhn_V,      typeof(AOpCodeSimdReg));
            SetA64("0x001110<<1xxxxx011111xxxxxxxxxx", AInstEmit.Saba_V,        typeof(AOpCodeSimdReg));
            SetA64("0x001110<<1xxxxx010100xxxxxxxxxx", AInstEmit.Sabal_V,       typeof(AOpCodeSimdReg));
            SetA64("0x001110<<1xxxxx011101xxxxxxxxxx", AInstEmit.Sabd_V,        typeof(AOpCodeSimdReg));
            SetA64("0x001110<<1xxxxx011100xxxxxxxxxx", AInstEmit.Sabdl_V,       typeof(AOpCodeSimdReg));
            SetA64("0x001110<<100000011010xxxxxxxxxx", AInstEmit.Sadalp_V,      typeof(AOpCodeSimd));
            SetA64("0x001110<<100000001010xxxxxxxxxx", AInstEmit.Saddlp_V,      typeof(AOpCodeSimd));
            SetA64("0x001110<<1xxxxx000100xxxxxxxxxx", AInstEmit.Saddw_V,       typeof(AOpCodeSimdReg));
            SetA64("x0011110xx100010000000xxxxxxxxxx", AInstEmit.Scvtf_Gp,      typeof(AOpCodeSimdCvt));
            SetA64("010111100x100001110110xxxxxxxxxx", AInstEmit.Scvtf_S,       typeof(AOpCodeSimd));
            SetA64("0x0011100x100001110110xxxxxxxxxx", AInstEmit.Scvtf_V,       typeof(AOpCodeSimd));
            SetA64("01011110000xxxxx010000xxxxxxxxxx", AInstEmit.Sha256h_V,     typeof(AOpCodeSimdReg));
            SetA64("01011110000xxxxx010100xxxxxxxxxx", AInstEmit.Sha256h2_V,    typeof(AOpCodeSimdReg));
            SetA64("0101111000101000001010xxxxxxxxxx", AInstEmit.Sha256su0_V,   typeof(AOpCodeSimd));
            SetA64("01011110000xxxxx011000xxxxxxxxxx", AInstEmit.Sha256su1_V,   typeof(AOpCodeSimdReg));
            SetA64("0x001110<<1xxxxx000001xxxxxxxxxx", AInstEmit.Shadd_V,       typeof(AOpCodeSimdReg));
            SetA64("010111110>>>>xxx010101xxxxxxxxxx", AInstEmit.Shl_S,         typeof(AOpCodeSimdShImm));
            SetA64("0x0011110>>>>xxx010101xxxxxxxxxx", AInstEmit.Shl_V,         typeof(AOpCodeSimdShImm));
            SetA64("0x101110<<100001001110xxxxxxxxxx", AInstEmit.Shll_V,        typeof(AOpCodeSimd));
            SetA64("0x00111100>>>xxx100001xxxxxxxxxx", AInstEmit.Shrn_V,        typeof(AOpCodeSimdShImm));
            SetA64("0x001110<<1xxxxx001001xxxxxxxxxx", AInstEmit.Shsub_V,       typeof(AOpCodeSimdReg));
            SetA64("0x1011110>>>>xxx010101xxxxxxxxxx", AInstEmit.Sli_V,         typeof(AOpCodeSimdShImm));
            SetA64("0x001110<<1xxxxx011001xxxxxxxxxx", AInstEmit.Smax_V,        typeof(AOpCodeSimdReg));
            SetA64("0x001110<<1xxxxx101001xxxxxxxxxx", AInstEmit.Smaxp_V,       typeof(AOpCodeSimdReg));
            SetA64("0x001110<<1xxxxx011011xxxxxxxxxx", AInstEmit.Smin_V,        typeof(AOpCodeSimdReg));
            SetA64("0x001110<<1xxxxx101011xxxxxxxxxx", AInstEmit.Sminp_V,       typeof(AOpCodeSimdReg));
            SetA64("0x001110<<1xxxxx100000xxxxxxxxxx", AInstEmit.Smlal_V,       typeof(AOpCodeSimdReg));
            SetA64("0x001110<<1xxxxx101000xxxxxxxxxx", AInstEmit.Smlsl_V,       typeof(AOpCodeSimdReg));
            SetA64("0x001110<<1xxxxx110000xxxxxxxxxx", AInstEmit.Smull_V,       typeof(AOpCodeSimdReg));
            SetA64("01011110xx100000011110xxxxxxxxxx", AInstEmit.Sqabs_S,       typeof(AOpCodeSimd));
            SetA64("0>001110<<100000011110xxxxxxxxxx", AInstEmit.Sqabs_V,       typeof(AOpCodeSimd));
            SetA64("01011110xx1xxxxx000011xxxxxxxxxx", AInstEmit.Sqadd_S,       typeof(AOpCodeSimdReg));
            SetA64("0>001110<<1xxxxx000011xxxxxxxxxx", AInstEmit.Sqadd_V,       typeof(AOpCodeSimdReg));
            SetA64("01011110011xxxxx101101xxxxxxxxxx", AInstEmit.Sqdmulh_S,     typeof(AOpCodeSimdReg));
            SetA64("01011110101xxxxx101101xxxxxxxxxx", AInstEmit.Sqdmulh_S,     typeof(AOpCodeSimdReg));
            SetA64("0x001110011xxxxx101101xxxxxxxxxx", AInstEmit.Sqdmulh_V,     typeof(AOpCodeSimdReg));
            SetA64("0x001110101xxxxx101101xxxxxxxxxx", AInstEmit.Sqdmulh_V,     typeof(AOpCodeSimdReg));
            SetA64("01111110xx100000011110xxxxxxxxxx", AInstEmit.Sqneg_S,       typeof(AOpCodeSimd));
            SetA64("0>101110<<100000011110xxxxxxxxxx", AInstEmit.Sqneg_V,       typeof(AOpCodeSimd));
            SetA64("01111110011xxxxx101101xxxxxxxxxx", AInstEmit.Sqrdmulh_S,    typeof(AOpCodeSimdReg));
            SetA64("01111110101xxxxx101101xxxxxxxxxx", AInstEmit.Sqrdmulh_S,    typeof(AOpCodeSimdReg));
            SetA64("0x101110011xxxxx101101xxxxxxxxxx", AInstEmit.Sqrdmulh_V,    typeof(AOpCodeSimdReg));
            SetA64("0x101110101xxxxx101101xxxxxxxxxx", AInstEmit.Sqrdmulh_V,    typeof(AOpCodeSimdReg));
            SetA64("0x00111100>>>xxx100111xxxxxxxxxx", AInstEmit.Sqrshrn_V,     typeof(AOpCodeSimdShImm));
            SetA64("01011110xx1xxxxx001011xxxxxxxxxx", AInstEmit.Sqsub_S,       typeof(AOpCodeSimdReg));
            SetA64("0>001110<<1xxxxx001011xxxxxxxxxx", AInstEmit.Sqsub_V,       typeof(AOpCodeSimdReg));
            SetA64("01011110<<100001010010xxxxxxxxxx", AInstEmit.Sqxtn_S,       typeof(AOpCodeSimd));
            SetA64("0x001110<<100001010010xxxxxxxxxx", AInstEmit.Sqxtn_V,       typeof(AOpCodeSimd));
            SetA64("01111110<<100001001010xxxxxxxxxx", AInstEmit.Sqxtun_S,      typeof(AOpCodeSimd));
            SetA64("0x101110<<100001001010xxxxxxxxxx", AInstEmit.Sqxtun_V,      typeof(AOpCodeSimd));
            SetA64("0x001110<<1xxxxx000101xxxxxxxxxx", AInstEmit.Srhadd_V,      typeof(AOpCodeSimdReg));
            SetA64("0x00111100>>>xxx001001xxxxxxxxxx", AInstEmit.Srshr_V,       typeof(AOpCodeSimdShImm));
            SetA64("0100111101xxxxxx001001xxxxxxxxxx", AInstEmit.Srshr_V,       typeof(AOpCodeSimdShImm));
            SetA64("0>001110<<1xxxxx010001xxxxxxxxxx", AInstEmit.Sshl_V,        typeof(AOpCodeSimdReg));
            SetA64("0x00111100>>>xxx101001xxxxxxxxxx", AInstEmit.Sshll_V,       typeof(AOpCodeSimdShImm));
            SetA64("0101111101xxxxxx000001xxxxxxxxxx", AInstEmit.Sshr_S,        typeof(AOpCodeSimdShImm));
            SetA64("0x00111100>>>xxx000001xxxxxxxxxx", AInstEmit.Sshr_V,        typeof(AOpCodeSimdShImm));
            SetA64("0100111101xxxxxx000001xxxxxxxxxx", AInstEmit.Sshr_V,        typeof(AOpCodeSimdShImm));
            SetA64("0x00111100>>>xxx000101xxxxxxxxxx", AInstEmit.Ssra_V,        typeof(AOpCodeSimdShImm));
            SetA64("0100111101xxxxxx000101xxxxxxxxxx", AInstEmit.Ssra_V,        typeof(AOpCodeSimdShImm));
            SetA64("0x001110<<1xxxxx001100xxxxxxxxxx", AInstEmit.Ssubw_V,       typeof(AOpCodeSimdReg));
            SetA64("0x00110000000000xxxxxxxxxxxxxxxx", AInstEmit.St__Vms,       typeof(AOpCodeSimdMemMs));
            SetA64("0x001100100xxxxxxxxxxxxxxxxxxxxx", AInstEmit.St__Vms,       typeof(AOpCodeSimdMemMs));
            SetA64("0x00110100x00000xxxxxxxxxxxxxxxx", AInstEmit.St__Vss,       typeof(AOpCodeSimdMemSs));
            SetA64("0x00110110xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.St__Vss,       typeof(AOpCodeSimdMemSs));
            SetA64("xx10110xx0xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Stp,           typeof(AOpCodeSimdMemPair));
            SetA64("xx111100x00xxxxxxxxx00xxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeSimdMemImm));
            SetA64("xx111100x00xxxxxxxxx01xxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeSimdMemImm));
            SetA64("xx111100x00xxxxxxxxx11xxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeSimdMemImm));
            SetA64("xx111101x0xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeSimdMemImm));
            SetA64("xx111100x01xxxxxxxxx10xxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeSimdMemReg));
            SetA64("01111110111xxxxx100001xxxxxxxxxx", AInstEmit.Sub_S,         typeof(AOpCodeSimdReg));
            SetA64("0>101110<<1xxxxx100001xxxxxxxxxx", AInstEmit.Sub_V,         typeof(AOpCodeSimdReg));
            SetA64("0x001110<<1xxxxx011000xxxxxxxxxx", AInstEmit.Subhn_V,       typeof(AOpCodeSimdReg));
            SetA64("01011110xx100000001110xxxxxxxxxx", AInstEmit.Suqadd_S,      typeof(AOpCodeSimd));
            SetA64("0>001110<<100000001110xxxxxxxxxx", AInstEmit.Suqadd_V,      typeof(AOpCodeSimd));
            SetA64("0x001110000xxxxx0xx000xxxxxxxxxx", AInstEmit.Tbl_V,         typeof(AOpCodeSimdTbl));
            SetA64("0>001110<<0xxxxx001010xxxxxxxxxx", AInstEmit.Trn1_V,        typeof(AOpCodeSimdReg));
            SetA64("0>001110<<0xxxxx011010xxxxxxxxxx", AInstEmit.Trn2_V,        typeof(AOpCodeSimdReg));
            SetA64("0x101110<<1xxxxx011111xxxxxxxxxx", AInstEmit.Uaba_V,        typeof(AOpCodeSimdReg));
            SetA64("0x101110<<1xxxxx010100xxxxxxxxxx", AInstEmit.Uabal_V,       typeof(AOpCodeSimdReg));
            SetA64("0x101110<<1xxxxx011101xxxxxxxxxx", AInstEmit.Uabd_V,        typeof(AOpCodeSimdReg));
            SetA64("0x101110<<1xxxxx011100xxxxxxxxxx", AInstEmit.Uabdl_V,       typeof(AOpCodeSimdReg));
            SetA64("0x101110<<100000011010xxxxxxxxxx", AInstEmit.Uadalp_V,      typeof(AOpCodeSimd));
            SetA64("0x101110<<1xxxxx000000xxxxxxxxxx", AInstEmit.Uaddl_V,       typeof(AOpCodeSimdReg));
            SetA64("0x101110<<100000001010xxxxxxxxxx", AInstEmit.Uaddlp_V,      typeof(AOpCodeSimd));
            SetA64("001011100x110000001110xxxxxxxxxx", AInstEmit.Uaddlv_V,      typeof(AOpCodeSimd));
            SetA64("01101110<<110000001110xxxxxxxxxx", AInstEmit.Uaddlv_V,      typeof(AOpCodeSimd));
            SetA64("0x101110<<1xxxxx000100xxxxxxxxxx", AInstEmit.Uaddw_V,       typeof(AOpCodeSimdReg));
            SetA64("x0011110xx100011000000xxxxxxxxxx", AInstEmit.Ucvtf_Gp,      typeof(AOpCodeSimdCvt));
            SetA64("011111100x100001110110xxxxxxxxxx", AInstEmit.Ucvtf_S,       typeof(AOpCodeSimd));
            SetA64("0x1011100x100001110110xxxxxxxxxx", AInstEmit.Ucvtf_V,       typeof(AOpCodeSimd));
            SetA64("0x101110<<1xxxxx000001xxxxxxxxxx", AInstEmit.Uhadd_V,       typeof(AOpCodeSimdReg));
            SetA64("0x101110<<1xxxxx001001xxxxxxxxxx", AInstEmit.Uhsub_V,       typeof(AOpCodeSimdReg));
            SetA64("0x101110<<1xxxxx011001xxxxxxxxxx", AInstEmit.Umax_V,        typeof(AOpCodeSimdReg));
            SetA64("0x101110<<1xxxxx101001xxxxxxxxxx", AInstEmit.Umaxp_V,       typeof(AOpCodeSimdReg));
            SetA64("0x101110<<1xxxxx011011xxxxxxxxxx", AInstEmit.Umin_V,        typeof(AOpCodeSimdReg));
            SetA64("0x101110<<1xxxxx101011xxxxxxxxxx", AInstEmit.Uminp_V,       typeof(AOpCodeSimdReg));
            SetA64("0x001110000xxxxx001111xxxxxxxxxx", AInstEmit.Umov_S,        typeof(AOpCodeSimdIns));
            SetA64("0x101110<<1xxxxx110000xxxxxxxxxx", AInstEmit.Umull_V,       typeof(AOpCodeSimdReg));
            SetA64("01111110xx1xxxxx000011xxxxxxxxxx", AInstEmit.Uqadd_S,       typeof(AOpCodeSimdReg));
            SetA64("0>101110<<1xxxxx000011xxxxxxxxxx", AInstEmit.Uqadd_V,       typeof(AOpCodeSimdReg));
            SetA64("01111110xx1xxxxx001011xxxxxxxxxx", AInstEmit.Uqsub_S,       typeof(AOpCodeSimdReg));
            SetA64("0>101110<<1xxxxx001011xxxxxxxxxx", AInstEmit.Uqsub_V,       typeof(AOpCodeSimdReg));
            SetA64("01111110<<100001010010xxxxxxxxxx", AInstEmit.Uqxtn_S,       typeof(AOpCodeSimd));
            SetA64("0x101110<<100001010010xxxxxxxxxx", AInstEmit.Uqxtn_V,       typeof(AOpCodeSimd));
            SetA64("0x101110<<1xxxxx000101xxxxxxxxxx", AInstEmit.Urhadd_V,      typeof(AOpCodeSimdReg));
            SetA64("0>101110<<1xxxxx010001xxxxxxxxxx", AInstEmit.Ushl_V,        typeof(AOpCodeSimdReg));
            SetA64("0x10111100>>>xxx101001xxxxxxxxxx", AInstEmit.Ushll_V,       typeof(AOpCodeSimdShImm));
            SetA64("0111111101xxxxxx000001xxxxxxxxxx", AInstEmit.Ushr_S,        typeof(AOpCodeSimdShImm));
            SetA64("0x10111100>>>xxx000001xxxxxxxxxx", AInstEmit.Ushr_V,        typeof(AOpCodeSimdShImm));
            SetA64("0110111101xxxxxx000001xxxxxxxxxx", AInstEmit.Ushr_V,        typeof(AOpCodeSimdShImm));
            SetA64("01111110xx100000001110xxxxxxxxxx", AInstEmit.Usqadd_S,      typeof(AOpCodeSimd));
            SetA64("0>101110<<100000001110xxxxxxxxxx", AInstEmit.Usqadd_V,      typeof(AOpCodeSimd));
            SetA64("0x10111100>>>xxx000101xxxxxxxxxx", AInstEmit.Usra_V,        typeof(AOpCodeSimdShImm));
            SetA64("0110111101xxxxxx000101xxxxxxxxxx", AInstEmit.Usra_V,        typeof(AOpCodeSimdShImm));
            SetA64("0x101110<<1xxxxx001100xxxxxxxxxx", AInstEmit.Usubw_V,       typeof(AOpCodeSimdReg));
            SetA64("0>001110<<0xxxxx000110xxxxxxxxxx", AInstEmit.Uzp1_V,        typeof(AOpCodeSimdReg));
            SetA64("0>001110<<0xxxxx010110xxxxxxxxxx", AInstEmit.Uzp2_V,        typeof(AOpCodeSimdReg));
            SetA64("0x001110<<100001001010xxxxxxxxxx", AInstEmit.Xtn_V,         typeof(AOpCodeSimd));
            SetA64("0>001110<<0xxxxx001110xxxxxxxxxx", AInstEmit.Zip1_V,        typeof(AOpCodeSimdReg));
            SetA64("0>001110<<0xxxxx011110xxxxxxxxxx", AInstEmit.Zip2_V,        typeof(AOpCodeSimdReg));
#endregion

#region "Generate InstA64FastLookup Table (AArch64)"
            var Tmp = new List<InstInfo>[FastLookupSize];
            for (int i = 0; i < FastLookupSize; i++)
            {
                Tmp[i] = new List<InstInfo>();
            }

            foreach (var Inst in AllInstA64)
            {
                int Mask = ToFastLookupIndex(Inst.Mask);
                int Value = ToFastLookupIndex(Inst.Value);

                for (int i = 0; i < FastLookupSize; i++)
                {
                    if ((i & Mask) == Value)
                    {
                        Tmp[i].Add(Inst);
                    }
                }
            }

            for (int i = 0; i < FastLookupSize; i++)
            {
                InstA64FastLookup[i] = Tmp[i].ToArray();
            }
#endregion
        }

        private class InstInfo
        {
            public int Mask;
            public int Value;

            public AInst Inst;

            public InstInfo(int Mask, int Value, AInst Inst)
            {
                this.Mask  = Mask;
                this.Value = Value;
                this.Inst  = Inst;
            }
        }

        private static List<InstInfo> AllInstA32 = new List<InstInfo>();
        private static List<InstInfo> AllInstA64 = new List<InstInfo>();

        private static int FastLookupSize = 0x1000;
        private static InstInfo[][] InstA64FastLookup = new InstInfo[FastLookupSize][];

        private static void SetA32(string Encoding, AInstInterpreter Interpreter, Type Type)
        {
            Set(Encoding, new AInst(Interpreter, null, Type), AExecutionMode.AArch32);
        }

        private static void SetA64(string Encoding, AInstEmitter Emitter, Type Type)
        {
            Set(Encoding, new AInst(null, Emitter, Type), AExecutionMode.AArch64);
        }

        private static void Set(string Encoding, AInst Inst, AExecutionMode Mode)
        {
            int Bit   = Encoding.Length - 1;
            int Value = 0;
            int XMask = 0;
            int XBits = 0;

            int[] XPos = new int[Encoding.Length];

            int Blacklisted = 0;

            for (int Index = 0; Index < Encoding.Length; Index++, Bit--)
            {
                //Note: < and > are used on special encodings.
                //The < means that we should never have ALL bits with the '<' set.
                //So, when the encoding has <<, it means that 00, 01, and 10 are valid,
                //but not 11. <<< is 000, 001, ..., 110 but NOT 111, and so on...
                //For >, the invalid value is zero. So, for >> 01, 10 and 11 are valid,
                //but 00 isn't.
                char Chr = Encoding[Index];

                if (Chr == '1')
                {
                    Value |= 1 << Bit;
                }
                else if (Chr == 'x')
                {
                    XMask |= 1 << Bit;
                }
                else if (Chr == '>')
                {
                    XPos[XBits++] = Bit;
                }
                else if (Chr == '<')
                {
                    XPos[XBits++] = Bit;

                    Blacklisted |= 1 << Bit;
                }
                else if (Chr != '0')
                {
                    throw new ArgumentException(nameof(Encoding));
                }
            }

            XMask = ~XMask;

            if (XBits == 0)
            {
                InsertInst(XMask, Value, Inst, Mode);

                return;
            }

            for (int Index = 0; Index < (1 << XBits); Index++)
            {
                int Mask = 0;

                for (int X = 0; X < XBits; X++)
                {
                    Mask |= ((Index >> X) & 1) << XPos[X];
                }

                if (Mask != Blacklisted)
                {
                    InsertInst(XMask, Value | Mask, Inst, Mode);
                }
            }
        }

        private static void InsertInst(
            int            XMask,
            int            Value,
            AInst          Inst,
            AExecutionMode Mode)
        {
            InstInfo Info = new InstInfo(XMask, Value, Inst);

            if (Mode == AExecutionMode.AArch64)
            {
                AllInstA64.Add(Info);
            }
            else
            {
                AllInstA32.Add(Info);
            }
        }

        public static AInst GetInstA32(int OpCode)
        {
            return GetInstFromList(AllInstA32, OpCode);
        }

        public static AInst GetInstA64(int OpCode)
        {
            return GetInstFromList(InstA64FastLookup[ToFastLookupIndex(OpCode)], OpCode);
        }

        private static int ToFastLookupIndex(int Value)
        {
            return ((Value >> 10) & 0x00F) | ((Value >> 18) & 0xFF0);
        }

        private static AInst GetInstFromList(IEnumerable<InstInfo> InstList, int OpCode)
        {
            foreach (var Node in InstList)
            {
                if ((OpCode & Node.Mask) == Node.Value)
                {
                    return Node.Inst;
                }
            }

            return AInst.Undefined;
        }
    }
}
