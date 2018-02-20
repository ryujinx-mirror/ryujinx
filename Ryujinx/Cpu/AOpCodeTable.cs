using ChocolArm64.Decoder;
using ChocolArm64.Instruction;
using System;

namespace ChocolArm64
{
    static class AOpCodeTable
    {
        static AOpCodeTable()
        {
 #region "OpCode Table"
            //Integer
            Set("x0011010000xxxxx000000xxxxxxxxxx", AInstEmit.Adc,           typeof(AOpCodeAluRs));
            Set("x00100010xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Add,           typeof(AOpCodeAluImm));
            Set("x0001011<<0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Add,           typeof(AOpCodeAluRs));
            Set("x0001011001xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Add,           typeof(AOpCodeAluRx));
            Set("x01100010xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Adds,          typeof(AOpCodeAluImm));
            Set("x0101011<<0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Adds,          typeof(AOpCodeAluRs));
            Set("x0101011001xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Adds,          typeof(AOpCodeAluRx));
            Set("0xx10000xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Adr,           typeof(AOpCodeAdr));
            Set("1xx10000xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Adrp,          typeof(AOpCodeAdr));
            Set("x00100100xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.And,           typeof(AOpCodeAluImm));
            Set("x0001010xx0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.And,           typeof(AOpCodeAluRs));
            Set("x11100100xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ands,          typeof(AOpCodeAluImm));
            Set("x1101010xx0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ands,          typeof(AOpCodeAluRs));
            Set("x0011010110xxxxx001010xxxxxxxxxx", AInstEmit.Asrv,          typeof(AOpCodeAluRs));
            Set("000101xxxxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.B,             typeof(AOpCodeBImmAl));
            Set("01010100xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.B_Cond,        typeof(AOpCodeBImmCond));
            Set("x01100110xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Bfm,           typeof(AOpCodeBfm));
            Set("x0001010xx1xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Bic,           typeof(AOpCodeAluRs));
            Set("x1101010xx1xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Bics,          typeof(AOpCodeAluRs));
            Set("100101xxxxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Bl,            typeof(AOpCodeBImmAl));
            Set("11010110001xxxxx000000xxxxxxxxxx", AInstEmit.Blr,           typeof(AOpCodeBReg));
            Set("11010110000xxxxx000000xxxxxxxxxx", AInstEmit.Br,            typeof(AOpCodeBReg));
            Set("11010100001xxxxxxxxxxxxxxxx00000", AInstEmit.Brk,           typeof(AOpCodeException));
            Set("x0110101xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Cbnz,          typeof(AOpCodeBImmCmp));
            Set("x0110100xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Cbz,           typeof(AOpCodeBImmCmp));
            Set("x0111010010xxxxxxxxx10xxxxxxxxxx", AInstEmit.Ccmn,          typeof(AOpCodeCcmpImm));
            Set("x0111010010xxxxxxxxx00xxxxxxxxxx", AInstEmit.Ccmn,          typeof(AOpCodeCcmpReg));
            Set("x1111010010xxxxxxxxx10xxxxxxxxxx", AInstEmit.Ccmp,          typeof(AOpCodeCcmpImm));
            Set("x1111010010xxxxxxxxx00xxxxxxxxxx", AInstEmit.Ccmp,          typeof(AOpCodeCcmpReg));
            Set("11010101000000110011xxxx01011111", AInstEmit.Clrex,         typeof(AOpCodeSystem));
            Set("x101101011000000000100xxxxxxxxxx", AInstEmit.Clz,           typeof(AOpCodeAlu));
            Set("x0011010100xxxxxxxxx00xxxxxxxxxx", AInstEmit.Csel,          typeof(AOpCodeCsel));
            Set("x0011010100xxxxxxxxx01xxxxxxxxxx", AInstEmit.Csinc,         typeof(AOpCodeCsel));
            Set("x1011010100xxxxxxxxx00xxxxxxxxxx", AInstEmit.Csinv,         typeof(AOpCodeCsel));
            Set("x1011010100xxxxxxxxx01xxxxxxxxxx", AInstEmit.Csneg,         typeof(AOpCodeCsel));
            Set("11010101000000110011xxxx10111111", AInstEmit.Dmb,           typeof(AOpCodeSystem));
            Set("11010101000000110011xxxx10011111", AInstEmit.Dsb,           typeof(AOpCodeSystem));
            Set("x1001010xx1xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Eon,           typeof(AOpCodeAluRs));
            Set("x10100100xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Eor,           typeof(AOpCodeAluImm));
            Set("x1001010xx0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Eor,           typeof(AOpCodeAluRs));
            Set("x00100111x0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Extr,          typeof(AOpCodeAluRs));
            Set("xx001000110xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Ldar,          typeof(AOpCodeMemEx));
            Set("1x001000011xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Ldaxp,         typeof(AOpCodeMemEx));
            Set("xx001000010xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Ldaxr,         typeof(AOpCodeMemEx));
            Set("<<10100xx1xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldp,           typeof(AOpCodeMemPair));
            Set("xx111000010xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeMemImm));
            Set("xx11100101xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeMemImm));
            Set("xx111000011xxxxxxxxx10xxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeMemReg));
            Set("xx011000xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.LdrLit,        typeof(AOpCodeMemLit));            
            Set("0x1110001x0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldrs,          typeof(AOpCodeMemImm));
            Set("0x1110011xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldrs,          typeof(AOpCodeMemImm));
            Set("10111000100xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldrs,          typeof(AOpCodeMemImm));
            Set("1011100110xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldrs,          typeof(AOpCodeMemImm));
            Set("0x1110001x1xxxxxxxxx10xxxxxxxxxx", AInstEmit.Ldrs,          typeof(AOpCodeMemReg));
            Set("10111000101xxxxxxxxx10xxxxxxxxxx", AInstEmit.Ldrs,          typeof(AOpCodeMemReg));
            Set("xx001000010xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Ldxr,          typeof(AOpCodeMemEx));
            Set("1x001000011xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Ldxp,          typeof(AOpCodeMemEx));
            Set("x0011010110xxxxx001000xxxxxxxxxx", AInstEmit.Lslv,          typeof(AOpCodeAluRs));
            Set("x0011010110xxxxx001001xxxxxxxxxx", AInstEmit.Lsrv,          typeof(AOpCodeAluRs));
            Set("x0011011000xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Madd,          typeof(AOpCodeMul));
            Set("x11100101xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Movk,          typeof(AOpCodeMov));
            Set("x00100101xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Movn,          typeof(AOpCodeMov));
            Set("x10100101xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Movz,          typeof(AOpCodeMov));
            Set("110101010011xxxxxxxxxxxxxxxxxxxx", AInstEmit.Mrs,           typeof(AOpCodeSystem));
            Set("110101010001xxxxxxxxxxxxxxxxxxxx", AInstEmit.Msr,           typeof(AOpCodeSystem));
            Set("x0011011000xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Msub,          typeof(AOpCodeMul));
            Set("11010101000000110010000000011111", AInstEmit.Nop,           typeof(AOpCodeSystem));
            Set("x0101010xx1xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Orn,           typeof(AOpCodeAluRs));
            Set("x01100100xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Orr,           typeof(AOpCodeAluImm));
            Set("x0101010xx0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Orr,           typeof(AOpCodeAluRs));
            Set("1111100110xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Pfrm,          typeof(AOpCodeMemImm));
            Set("11011000xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Pfrm,          typeof(AOpCodeMemLit));
            Set("x101101011000000000000xxxxxxxxxx", AInstEmit.Rbit,          typeof(AOpCodeAlu));
            Set("11010110010xxxxx000000xxxxxxxxxx", AInstEmit.Ret,           typeof(AOpCodeBReg));
            Set("x101101011000000000001xxxxxxxxxx", AInstEmit.Rev16,         typeof(AOpCodeAlu));
            Set("x101101011000000000010xxxxxxxxxx", AInstEmit.Rev32,         typeof(AOpCodeAlu));
            Set("1101101011000000000011xxxxxxxxxx", AInstEmit.Rev64,         typeof(AOpCodeAlu));
            Set("x0011010110xxxxx001011xxxxxxxxxx", AInstEmit.Rorv,          typeof(AOpCodeAluRs));
            Set("x1011010000xxxxx000000xxxxxxxxxx", AInstEmit.Sbc,           typeof(AOpCodeAluRs));
            Set("x00100110xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Sbfm,          typeof(AOpCodeBfm));
            Set("x0011010110xxxxx000011xxxxxxxxxx", AInstEmit.Sdiv,          typeof(AOpCodeAluRs));
            Set("10011011001xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Smaddl,        typeof(AOpCodeMul));
            Set("10011011001xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Smsubl,        typeof(AOpCodeMul));
            Set("10011011010xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Smulh,         typeof(AOpCodeMul));
            Set("xx001000100xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Stlr,          typeof(AOpCodeMemEx));
            Set("1x001000001xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Stlxp,         typeof(AOpCodeMemEx));
            Set("xx001000000xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Stlxr,         typeof(AOpCodeMemEx));
            Set("x010100xx0xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Stp,           typeof(AOpCodeMemPair));
            Set("xx111000000xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeMemImm));
            Set("xx11100100xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeMemImm));
            Set("xx111000001xxxxxxxxx10xxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeMemReg));
            Set("1x001000001xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Stxp,          typeof(AOpCodeMemEx));
            Set("xx001000000xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Stxr,          typeof(AOpCodeMemEx));
            Set("x10100010xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Sub,           typeof(AOpCodeAluImm));
            Set("x1001011<<0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Sub,           typeof(AOpCodeAluRs));
            Set("x1001011001xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Sub,           typeof(AOpCodeAluRx));
            Set("x11100010xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Subs,          typeof(AOpCodeAluImm));
            Set("x1101011<<0xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Subs,          typeof(AOpCodeAluRs));
            Set("x1101011001xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Subs,          typeof(AOpCodeAluRx));
            Set("11010100000xxxxxxxxxxxxxxxx00001", AInstEmit.Svc,           typeof(AOpCodeException));
            Set("1101010100001xxxxxxxxxxxxxxxxxxx", AInstEmit.Sys,           typeof(AOpCodeSystem));
            Set("x0110111xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Tbnz,          typeof(AOpCodeBImmTest));
            Set("x0110110xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Tbz,           typeof(AOpCodeBImmTest));
            Set("x10100110xxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ubfm,          typeof(AOpCodeBfm));
            Set("x0011010110xxxxx000010xxxxxxxxxx", AInstEmit.Udiv,          typeof(AOpCodeAluRs));
            Set("10011011101xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Umaddl,        typeof(AOpCodeMul));
            Set("10011011101xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Umsubl,        typeof(AOpCodeMul));
            Set("10011011110xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Umulh,         typeof(AOpCodeMul));

            //Vector
            Set("0>001110<<1xxxxx100001xxxxxxxxxx", AInstEmit.Add_V,         typeof(AOpCodeSimdReg));
            Set("01011110xx110001101110xxxxxxxxxx", AInstEmit.Addp_S,        typeof(AOpCodeSimd));
            Set("0>001110<<1xxxxx101111xxxxxxxxxx", AInstEmit.Addp_V,        typeof(AOpCodeSimdReg));
            Set("000011100x110001101110xxxxxxxxxx", AInstEmit.Addv_V,        typeof(AOpCodeSimd));
            Set("01001110<<110001101110xxxxxxxxxx", AInstEmit.Addv_V,        typeof(AOpCodeSimd));
            Set("0x001110001xxxxx000111xxxxxxxxxx", AInstEmit.And_V,         typeof(AOpCodeSimdReg));
            Set("0x001110011xxxxx000111xxxxxxxxxx", AInstEmit.Bic_V,         typeof(AOpCodeSimdReg));
            Set("0x10111100000xxx<<x101xxxxxxxxxx", AInstEmit.Bic_Vi,        typeof(AOpCodeSimdImm));
            Set("0x101110011xxxxx000111xxxxxxxxxx", AInstEmit.Bsl_V,         typeof(AOpCodeSimdReg));
            Set("0>101110<<1xxxxx100011xxxxxxxxxx", AInstEmit.Cmeq_V,        typeof(AOpCodeSimdReg));
            Set("0>001110<<100000100110xxxxxxxxxx", AInstEmit.Cmeq_V,        typeof(AOpCodeSimd));
            Set("0>001110<<1xxxxx001111xxxxxxxxxx", AInstEmit.Cmge_V,        typeof(AOpCodeSimdReg));
            Set("0>101110<<100000100010xxxxxxxxxx", AInstEmit.Cmge_V,        typeof(AOpCodeSimd));
            Set("0>001110<<1xxxxx001101xxxxxxxxxx", AInstEmit.Cmgt_V,        typeof(AOpCodeSimdReg));
            Set("0>001110<<100000100010xxxxxxxxxx", AInstEmit.Cmgt_V,        typeof(AOpCodeSimd));
            Set("0>101110<<1xxxxx001101xxxxxxxxxx", AInstEmit.Cmhi_V,        typeof(AOpCodeSimdReg));
            Set("0>101110<<1xxxxx001111xxxxxxxxxx", AInstEmit.Cmhs_V,        typeof(AOpCodeSimdReg));
            Set("0>101110<<100000100110xxxxxxxxxx", AInstEmit.Cmle_V,        typeof(AOpCodeSimd));
            Set("0>001110<<100000101010xxxxxxxxxx", AInstEmit.Cmlt_V,        typeof(AOpCodeSimd));
            Set("0x00111000100000010110xxxxxxxxxx", AInstEmit.Cnt_V,         typeof(AOpCodeSimd));
            Set("0x001110000xxxxx000011xxxxxxxxxx", AInstEmit.Dup_Gp,        typeof(AOpCodeSimdIns));
            Set("01011110000xxxxx000001xxxxxxxxxx", AInstEmit.Dup_S,         typeof(AOpCodeSimdIns));
            Set("0x001110000xxxxx000001xxxxxxxxxx", AInstEmit.Dup_V,         typeof(AOpCodeSimdIns));
            Set("0x101110001xxxxx000111xxxxxxxxxx", AInstEmit.Eor_V,         typeof(AOpCodeSimdReg));
            Set("00011110xx100000110000xxxxxxxxxx", AInstEmit.Fabs_S,        typeof(AOpCodeSimd));
            Set("00011110xx1xxxxx001010xxxxxxxxxx", AInstEmit.Fadd_S,        typeof(AOpCodeSimdReg));
            Set("0x0011100x1xxxxx110101xxxxxxxxxx", AInstEmit.Fadd_V,        typeof(AOpCodeSimdReg));
            Set("00011110xx1xxxxxxxxx01xxxxx0xxxx", AInstEmit.Fccmp_S,       typeof(AOpCodeSimdFcond));
            Set("00011110xx1xxxxxxxxx01xxxxx1xxxx", AInstEmit.Fccmpe_S,      typeof(AOpCodeSimdFcond));
            Set("00011110xx1xxxxx001000xxxxx0x000", AInstEmit.Fcmp_S,        typeof(AOpCodeSimdReg));
            Set("00011110xx1xxxxx001000xxxxx1x000", AInstEmit.Fcmpe_S,       typeof(AOpCodeSimdReg));
            Set("00011110xx1xxxxxxxxx11xxxxxxxxxx", AInstEmit.Fcsel_S,       typeof(AOpCodeSimdFcond));
            Set("00011110xx10001xx10000xxxxxxxxxx", AInstEmit.Fcvt_S,        typeof(AOpCodeSimd));
            Set("x0011110xx100100000000xxxxxxxxxx", AInstEmit.Fcvtas_Gp,     typeof(AOpCodeSimdCvt));
            Set("x0011110xx100101000000xxxxxxxxxx", AInstEmit.Fcvtau_Gp,     typeof(AOpCodeSimdCvt));
            Set("x0011110xx110000000000xxxxxxxxxx", AInstEmit.Fcvtms_Gp,     typeof(AOpCodeSimdCvt));
            Set("x0011110xx101000000000xxxxxxxxxx", AInstEmit.Fcvtps_Gp,     typeof(AOpCodeSimdCvt));
            Set("x0011110xx111000000000xxxxxxxxxx", AInstEmit.Fcvtzs_Gp,     typeof(AOpCodeSimdCvt));
            Set("x0011110xx011000xxxxxxxxxxxxxxxx", AInstEmit.Fcvtzs_Gp_Fix, typeof(AOpCodeSimdCvt));
            Set("0x0011101x100001101110xxxxxxxxxx", AInstEmit.Fcvtzs_V,      typeof(AOpCodeSimd));
            Set("0x0011110>>xxxxx111111xxxxxxxxxx", AInstEmit.Fcvtzs_V,      typeof(AOpCodeSimdShImm));
            Set("x0011110xx111001000000xxxxxxxxxx", AInstEmit.Fcvtzu_Gp,     typeof(AOpCodeSimdCvt));
            Set("x0011110xx011001xxxxxxxxxxxxxxxx", AInstEmit.Fcvtzu_Gp_Fix, typeof(AOpCodeSimdCvt));
            Set("0x1011101x100001101110xxxxxxxxxx", AInstEmit.Fcvtzu_V,      typeof(AOpCodeSimd));
            Set("0x1011110>>xxxxx111111xxxxxxxxxx", AInstEmit.Fcvtzu_V,      typeof(AOpCodeSimdShImm));            
            Set("00011110xx1xxxxx000110xxxxxxxxxx", AInstEmit.Fdiv_S,        typeof(AOpCodeSimdReg));
            Set("00011111xx0xxxxx0xxxxxxxxxxxxxxx", AInstEmit.Fmadd_S,       typeof(AOpCodeSimdReg));
            Set("00011110xx1xxxxx010010xxxxxxxxxx", AInstEmit.Fmax_S,        typeof(AOpCodeSimdReg));
            Set("00011110xx1xxxxx011010xxxxxxxxxx", AInstEmit.Fmaxnm_S,      typeof(AOpCodeSimdReg));
            Set("00011110xx1xxxxx010110xxxxxxxxxx", AInstEmit.Fmin_S,        typeof(AOpCodeSimdReg));
            Set("00011110xx1xxxxx011110xxxxxxxxxx", AInstEmit.Fminnm_S,      typeof(AOpCodeSimdReg));
            Set("0x0011100x1xxxxx110011xxxxxxxxxx", AInstEmit.Fmla_V,        typeof(AOpCodeSimdReg));
            Set("0x0011111<<xxxxx0001x0xxxxxxxxxx", AInstEmit.Fmla_Ve,       typeof(AOpCodeSimdRegElem));
            Set("00011110xx100000010000xxxxxxxxxx", AInstEmit.Fmov_S,        typeof(AOpCodeSimd));
            Set("00011110xx1xxxxxxxx100xxxxxxxxxx", AInstEmit.Fmov_Si,       typeof(AOpCodeSimdFmov));
            Set("0xx0111100000xxx111101xxxxxxxxxx", AInstEmit.Fmov_V,        typeof(AOpCodeSimdImm));
            Set("x0011110xx100110000000xxxxxxxxxx", AInstEmit.Fmov_Ftoi,     typeof(AOpCodeSimdCvt));
            Set("x0011110xx100111000000xxxxxxxxxx", AInstEmit.Fmov_Itof,     typeof(AOpCodeSimdCvt));
            Set("x0011110xx101110000000xxxxxxxxxx", AInstEmit.Fmov_Ftoi1,    typeof(AOpCodeSimdCvt));
            Set("x0011110xx101111000000xxxxxxxxxx", AInstEmit.Fmov_Itof1,    typeof(AOpCodeSimdCvt));
            Set("00011111xx0xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Fmsub_S,       typeof(AOpCodeSimdReg));
            Set("00011110xx1xxxxx000010xxxxxxxxxx", AInstEmit.Fmul_S,        typeof(AOpCodeSimdReg));
            Set("0x1011100x1xxxxx110111xxxxxxxxxx", AInstEmit.Fmul_V,        typeof(AOpCodeSimdReg));
            Set("0x0011111<<xxxxx1001x0xxxxxxxxxx", AInstEmit.Fmul_Ve,       typeof(AOpCodeSimdRegElem));
            Set("00011110xx100001010000xxxxxxxxxx", AInstEmit.Fneg_S,        typeof(AOpCodeSimdReg));
            Set("00011111xx1xxxxx1xxxxxxxxxxxxxxx", AInstEmit.Fnmsub_S,      typeof(AOpCodeSimdReg));
            Set("00011110xx1xxxxx100010xxxxxxxxxx", AInstEmit.Fnmul_S,       typeof(AOpCodeSimdReg));
            Set("00011110xx100110010000xxxxxxxxxx", AInstEmit.Frinta_S,      typeof(AOpCodeSimd));
            Set("00011110xx100101010000xxxxxxxxxx", AInstEmit.Frintm_S,      typeof(AOpCodeSimd));
            Set("00011110xx100001110000xxxxxxxxxx", AInstEmit.Fsqrt_S,       typeof(AOpCodeSimd));
            Set("00011110xx1xxxxx001110xxxxxxxxxx", AInstEmit.Fsub_S,        typeof(AOpCodeSimdReg));
            Set("0x0011101x1xxxxx110101xxxxxxxxxx", AInstEmit.Fsub_V,        typeof(AOpCodeSimdReg));
            Set("01001110000xxxxx000111xxxxxxxxxx", AInstEmit.Ins_Gp,        typeof(AOpCodeSimdIns));
            Set("01101110000xxxxx0xxxx1xxxxxxxxxx", AInstEmit.Ins_V,         typeof(AOpCodeSimdIns));
            Set("0x00110001000000xxxxxxxxxxxxxxxx", AInstEmit.Ld__Vms,       typeof(AOpCodeSimdMemMs));
            Set("0x001100110xxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ld__Vms,       typeof(AOpCodeSimdMemMs));
            Set("0x00110101000000xx0xxxxxxxxxxxxx", AInstEmit.Ld__Vss,       typeof(AOpCodeSimdMemSs));
            Set("0x001101110xxxxxxx0xxxxxxxxxxxxx", AInstEmit.Ld__Vss,       typeof(AOpCodeSimdMemSs));
            Set("xx10110xx1xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldp,           typeof(AOpCodeSimdMemPair));
            Set("xx111100x10xxxxxxxxx00xxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeSimdMemImm));
            Set("xx111100x10xxxxxxxxx01xxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeSimdMemImm));
            Set("xx111100x10xxxxxxxxx11xxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeSimdMemImm));
            Set("xx111101x1xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeSimdMemImm));
            Set("xx111100x11xxxxxxxxx10xxxxxxxxxx", AInstEmit.Ldr,           typeof(AOpCodeSimdMemReg));
            Set("xx011100xxxxxxxxxxxxxxxxxxxxxxxx", AInstEmit.LdrLit,        typeof(AOpCodeSimdMemLit));
            Set("0x001110<<1xxxxx100101xxxxxxxxxx", AInstEmit.Mla_V,         typeof(AOpCodeSimdReg));
            Set("0x101110<<1xxxxx100101xxxxxxxxxx", AInstEmit.Mls_V,         typeof(AOpCodeSimdReg));
            Set("0x00111100000xxx0xx001xxxxxxxxxx", AInstEmit.Movi_V,        typeof(AOpCodeSimdImm));
            Set("0x00111100000xxx10x001xxxxxxxxxx", AInstEmit.Movi_V,        typeof(AOpCodeSimdImm));
            Set("0x00111100000xxx110x01xxxxxxxxxx", AInstEmit.Movi_V,        typeof(AOpCodeSimdImm));
            Set("0xx0111100000xxx111001xxxxxxxxxx", AInstEmit.Movi_V,        typeof(AOpCodeSimdImm));
            Set("0x001110<<1xxxxx100111xxxxxxxxxx", AInstEmit.Mul_V,         typeof(AOpCodeSimdReg));
            Set("0x10111100000xxx0xx001xxxxxxxxxx", AInstEmit.Mvni_V,        typeof(AOpCodeSimdImm));
            Set("0x10111100000xxx10x001xxxxxxxxxx", AInstEmit.Mvni_V,        typeof(AOpCodeSimdImm));
            Set("0x10111100000xxx110x01xxxxxxxxxx", AInstEmit.Mvni_V,        typeof(AOpCodeSimdImm));
            Set("0>101110<<100000101110xxxxxxxxxx", AInstEmit.Neg_V,         typeof(AOpCodeSimdReg));
            Set("0x10111000100000010110xxxxxxxxxx", AInstEmit.Not_V,         typeof(AOpCodeSimd));
            Set("0x001110101xxxxx000111xxxxxxxxxx", AInstEmit.Orr_V,         typeof(AOpCodeSimdReg));
            Set("0x00111100000xxx<<x101xxxxxxxxxx", AInstEmit.Orr_Vi,        typeof(AOpCodeSimdImm));
            Set("0x001110<<1xxxxx000100xxxxxxxxxx", AInstEmit.Saddw_V,       typeof(AOpCodeSimdReg));
            Set("x0011110xx100010000000xxxxxxxxxx", AInstEmit.Scvtf_Gp,      typeof(AOpCodeSimdCvt));
            Set("010111100x100001110110xxxxxxxxxx", AInstEmit.Scvtf_S,       typeof(AOpCodeSimd));
            Set("0x0011100x100001110110xxxxxxxxxx", AInstEmit.Scvtf_V,       typeof(AOpCodeSimd));
            Set("010111110>>>>xxx010101xxxxxxxxxx", AInstEmit.Shl_S,         typeof(AOpCodeSimdShImm));
            Set("0x0011110>>>>xxx010101xxxxxxxxxx", AInstEmit.Shl_V,         typeof(AOpCodeSimdShImm));
            Set("0x00111100>>>xxx100001xxxxxxxxxx", AInstEmit.Shrn_V,        typeof(AOpCodeSimdShImm));
            Set("0x001110<<1xxxxx011001xxxxxxxxxx", AInstEmit.Smax_V,        typeof(AOpCodeSimdReg));
            Set("0x001110<<1xxxxx011011xxxxxxxxxx", AInstEmit.Smin_V,        typeof(AOpCodeSimdReg));
            Set("0x001110<<1xxxxx110000xxxxxxxxxx", AInstEmit.Smull_V,       typeof(AOpCodeSimdReg));
            Set("0>001110<<1xxxxx010001xxxxxxxxxx", AInstEmit.Sshl_V,        typeof(AOpCodeSimdReg));
            Set("0x00111100>>>xxx101001xxxxxxxxxx", AInstEmit.Sshll_V,       typeof(AOpCodeSimdShImm));
            Set("010111110>>>>xxx000001xxxxxxxxxx", AInstEmit.Sshr_S,        typeof(AOpCodeSimdShImm));
            Set("0x0011110>>>>xxx000001xxxxxxxxxx", AInstEmit.Sshr_V,        typeof(AOpCodeSimdShImm));
            Set("0x00110000000000xxxxxxxxxxxxxxxx", AInstEmit.St__Vms,       typeof(AOpCodeSimdMemMs));
            Set("0x001100100xxxxxxxxxxxxxxxxxxxxx", AInstEmit.St__Vms,       typeof(AOpCodeSimdMemMs));
            Set("0x00110100000000xx0xxxxxxxxxxxxx", AInstEmit.St__Vss,       typeof(AOpCodeSimdMemSs));
            Set("0x001101100xxxxxxx0xxxxxxxxxxxxx", AInstEmit.St__Vss,       typeof(AOpCodeSimdMemSs));
            Set("xx10110xx0xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Stp,           typeof(AOpCodeSimdMemPair));
            Set("xx111100x00xxxxxxxxx00xxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeSimdMemImm));
            Set("xx111100x00xxxxxxxxx01xxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeSimdMemImm));
            Set("xx111100x00xxxxxxxxx11xxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeSimdMemImm));
            Set("xx111101x0xxxxxxxxxxxxxxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeSimdMemImm));
            Set("xx111100x01xxxxxxxxx10xxxxxxxxxx", AInstEmit.Str,           typeof(AOpCodeSimdMemReg));
            Set("01111110xx1xxxxx100001xxxxxxxxxx", AInstEmit.Sub_S,         typeof(AOpCodeSimdReg));
            Set("0>101110<<1xxxxx100001xxxxxxxxxx", AInstEmit.Sub_V,         typeof(AOpCodeSimdReg));
            Set("0x001110000xxxxx0xx000xxxxxxxxxx", AInstEmit.Tbl_V,         typeof(AOpCodeSimdTbl));
            Set("001011100x110000001110xxxxxxxxxx", AInstEmit.Uaddlv_V,      typeof(AOpCodeSimd));
            Set("01101110<<110000001110xxxxxxxxxx", AInstEmit.Uaddlv_V,      typeof(AOpCodeSimd));
            Set("0x101110<<1xxxxx000100xxxxxxxxxx", AInstEmit.Uaddw_V,       typeof(AOpCodeSimdReg));
            Set("x0011110xx100011000000xxxxxxxxxx", AInstEmit.Ucvtf_Gp,      typeof(AOpCodeSimdCvt));
            Set("011111100x100001110110xxxxxxxxxx", AInstEmit.Ucvtf_S,       typeof(AOpCodeSimd));
            Set("0x1011100x100001110110xxxxxxxxxx", AInstEmit.Ucvtf_V,       typeof(AOpCodeSimd));
            Set("0x001110000xxxxx001111xxxxxxxxxx", AInstEmit.Umov_S,        typeof(AOpCodeSimdIns));
            Set("0>101110<<1xxxxx010001xxxxxxxxxx", AInstEmit.Ushl_V,        typeof(AOpCodeSimdReg));
            Set("0x10111100>>>xxx101001xxxxxxxxxx", AInstEmit.Ushll_V,       typeof(AOpCodeSimdShImm));
            Set("011111110>>>>xxx000001xxxxxxxxxx", AInstEmit.Ushr_S,        typeof(AOpCodeSimdShImm));
            Set("0x1011110>>>>xxx000001xxxxxxxxxx", AInstEmit.Ushr_V,        typeof(AOpCodeSimdShImm));
            Set("0x1011110>>>>xxx000101xxxxxxxxxx", AInstEmit.Usra_V,        typeof(AOpCodeSimdShImm));
            Set("0x001110xx0xxxxx000110xxxxxxxxxx", AInstEmit.Uzp1_V,        typeof(AOpCodeSimdReg));
            Set("0x001110xx0xxxxx010110xxxxxxxxxx", AInstEmit.Uzp2_V,        typeof(AOpCodeSimdReg));
            Set("0x001110<<100001001010xxxxxxxxxx", AInstEmit.Xtn_V,         typeof(AOpCodeSimd));
            Set("0x001110xx0xxxxx001110xxxxxxxxxx", AInstEmit.Zip1_V,        typeof(AOpCodeSimdReg));
            Set("0x001110xx0xxxxx011110xxxxxxxxxx", AInstEmit.Zip2_V,        typeof(AOpCodeSimdReg));
#endregion
        }

        private class TreeNode
        {
            public int Mask;
            public int Value;

            public TreeNode Next;
            public TreeNode Child;

            public AInst Inst;

            public TreeNode(int Mask, int Value, AInst Inst)
            {
                this.Mask  = Mask;
                this.Value = Value;
                this.Inst  = Inst;
            }
        }

        private static TreeNode Root;

        private static void Set(string Encoding, AInstEmitter Emitter, Type Type)
        {
            Set(Encoding, new AInst(Emitter, Type));
        }

        private static void Set(string Encoding, AInst Inst)
        {
            int Bit    = Encoding.Length - 1;
            int Value  = 0;
            int XMask  = 0;
            int ZCount = 0;
            int OCount = 0;

            int[] ZPos = new int[Encoding.Length];
            int[] OPos = new int[Encoding.Length];

            for (int Index = 0; Index < Encoding.Length; Index++, Bit--)
            {
                //Note: < and > are used on special encodings.
                //The < means that we should never have ALL bits with the '<' set.
                //So, when the encoding has <<, it means that 00, 01, and 10 are valid,
                //but not 11. <<< is 000, 001, ..., 110 but NOT 111, and so on...
                //For >, the invalid value is zero. So, for >> 01, 10 and 11 are valid,
                //but 00 isn't.
                switch (Encoding[Index])
                {
                    case '0': /* Do nothing. */  break;
                    case '1': Value |= 1 << Bit; break;
                    case 'x': XMask |= 1 << Bit; break;

                    case '<': OPos[OCount++] = Bit; break;
                    case '>': ZPos[ZCount++] = Bit; break;

                    default: throw new ArgumentException(nameof(Encoding));
                }
            }

            if (ZCount + OCount == 0)
            {
                InsertTop(XMask, Value, Inst);
            }
            else if (ZCount != 0 && OCount != 0)
            {
                //When both the > and the < are used, then a value is blacklisted,
                //with > indicating 0, and < indicating 1. So, for example, ><<
                //blacklists the pattern 011, but 000, 001, 010, 100, 101,
                //110 and 111 are valid.
                for (int OCtr = 0; (uint)OCtr < (1 << OCount); OCtr++)
                {
                    int OVal = Value;

                    for (int O = 0; O < OCount; O++)
                    {
                        OVal |= ((OCtr >> O) & 1) << OPos[O];
                    }

                    int ZStart = OCtr == (1 << OCount) ? 1 : 0;

                    InsertWithCtr(ZStart, 1 << ZCount, ZCount, ZPos, XMask, OVal, Inst);
                }
            }
            else if (ZCount != 0)
            {
                InsertWithCtr(1,  1 << ZCount,      ZCount, ZPos, XMask, Value, Inst);
            }
            else if (OCount != 0)
            {
                InsertWithCtr(0, (1 << OCount) - 1, OCount, OPos, XMask, Value, Inst);
            }
        }

        private static void InsertWithCtr(
            int   Start,
            int   End,
            int   Cnt,
            int[] Pos,
            int   XMask,
            int   Value,
            AInst Inst)
        {
            for (int Ctr = Start; (uint)Ctr < End; Ctr++)
            {
                int Val = Value;

                for (int Index = 0; Index < Cnt; Index++)
                {
                    Val |= ((Ctr >> Index) & 1) << Pos[Index];
                }

                InsertTop(XMask, Val, Inst);
            }
        }

        private static void InsertTop(int XMask, int Value, AInst Inst)
        {
            TreeNode Next = Root;

            Root = new TreeNode(~XMask, Value, Inst);

            Root.Next = Next;
        }

        public static AInst GetInst(int OpCode)
        {
            TreeNode Node = Root;

            do
            {
                if ((OpCode & Node.Mask) == Node.Value)
                {
                    return Node.Inst;
                }
            }
            while ((Node = Node.Next) != null);

            return AInst.Undefined;
        }
    }
}