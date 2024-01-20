using Ryujinx.Cpu.LightningJit.CodeGen.Arm64;
using System;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    class InstEmit : IInstEmit
    {
        public static void AdcIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.AdcI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm12), inst.S != 0);
        }

        public static void AdcIT1(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.AdcI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void AdcRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.AdcR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void AdcRT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdnb16w3 inst = new(encoding);

            InstEmitAlu.AdcR(context, inst.Rdn, inst.Rdn, inst.Rm, 0, 0, !context.InITBlock);
        }

        public static void AdcRT2(CodeGenContext context, uint encoding)
        {
            InstSb20w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.AdcR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.S != 0);
        }

        public static void AdcRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.AdcRr(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Rs, inst.S != 0);
        }

        public static void AddIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.AddI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm12), inst.S != 0);
        }

        public static void AddIT1(CodeGenContext context, uint encoding)
        {
            InstImm3b22w3Rnb19w3Rdb16w3 inst = new(encoding);

            InstEmitAlu.AddI(context, inst.Rd, inst.Rn, inst.Imm3, !context.InITBlock);
        }

        public static void AddIT2(CodeGenContext context, uint encoding)
        {
            InstRdnb24w3Imm8b16w8 inst = new(encoding);

            InstEmitAlu.AddI(context, inst.Rdn, inst.Rdn, inst.Imm8, !context.InITBlock);
        }

        public static void AddIT3(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.AddI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void AddIT4(CodeGenContext context, uint encoding)
        {
            InstIb26w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.AddI(context, inst.Rd, inst.Rn, ImmUtils.CombineImmU12(inst.Imm8, inst.Imm3, inst.I), false);
        }

        public static void AddRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.AddR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void AddRT1(CodeGenContext context, uint encoding)
        {
            InstRmb22w3Rnb19w3Rdb16w3 inst = new(encoding);

            InstEmitAlu.AddR(context, inst.Rd, inst.Rn, inst.Rm, 0, 0, !context.InITBlock);
        }

        public static void AddRT2(CodeGenContext context, uint encoding)
        {
            InstDnb23w1Rmb19w4Rdnb16w3 inst = new(encoding);

            uint rdn = (inst.Dn << 3) | inst.Rdn;

            InstEmitAlu.AddR(context, rdn, rdn, inst.Rm, 0, 0, false);
        }

        public static void AddRT3(CodeGenContext context, uint encoding)
        {
            InstSb20w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.AddR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.S != 0);
        }

        public static void AddRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.AddRr(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Rs, inst.S != 0);
        }

        public static void AddSpIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.AddI(context, inst.Rd, RegisterUtils.SpRegister, ImmUtils.ExpandImm(inst.Imm12), inst.S != 0);
        }

        public static void AddSpIT1(CodeGenContext context, uint encoding)
        {
            InstRdb24w3Imm8b16w8 inst = new(encoding);

            InstEmitAlu.AddI(context, inst.Rd, RegisterUtils.SpRegister, inst.Imm8 << 2, false);
        }

        public static void AddSpIT2(CodeGenContext context, uint encoding)
        {
            InstImm7b16w7 inst = new(encoding);

            InstEmitAlu.AddI(context, RegisterUtils.SpRegister, RegisterUtils.SpRegister, inst.Imm7 << 2, false);
        }

        public static void AddSpIT3(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.AddI(context, inst.Rd, RegisterUtils.SpRegister, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void AddSpIT4(CodeGenContext context, uint encoding)
        {
            InstIb26w1Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.AddI(context, inst.Rd, RegisterUtils.SpRegister, ImmUtils.CombineImmU12(inst.Imm8, inst.Imm3, inst.I), false);
        }

        public static void AddSpRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.AddR(context, inst.Rd, RegisterUtils.SpRegister, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void AddSpRT1(CodeGenContext context, uint encoding)
        {
            InstDmb23w1Rdmb16w3 inst = new(encoding);

            uint rdm = inst.Rdm | (inst.Dm << 3);

            InstEmitAlu.AddR(context, rdm, RegisterUtils.SpRegister, rdm, 0, 0, false);
        }

        public static void AddSpRT2(CodeGenContext context, uint encoding)
        {
            InstRmb19w4 inst = new(encoding);

            InstEmitAlu.AddR(context, RegisterUtils.SpRegister, RegisterUtils.SpRegister, inst.Rm, 0, 0, false);
        }

        public static void AddSpRT3(CodeGenContext context, uint encoding)
        {
            InstSb20w1Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.AddR(context, inst.Rd, RegisterUtils.SpRegister, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.S != 0);
        }

        public static void AdrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.Adr(context, inst.Rd, ImmUtils.ExpandImm(inst.Imm12), true);
        }

        public static void AdrA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.Adr(context, inst.Rd, ImmUtils.ExpandImm(inst.Imm12), false);
        }

        public static void AdrT1(CodeGenContext context, uint encoding)
        {
            InstRdb24w3Imm8b16w8 inst = new(encoding);

            InstEmitAlu.Adr(context, inst.Rd, inst.Imm8 << 2, true);
        }

        public static void AdrT2(CodeGenContext context, uint encoding)
        {
            InstIb26w1Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.Adr(context, inst.Rd, ImmUtils.CombineImmU12(inst.Imm8, inst.Imm3, inst.I), false);
        }

        public static void AdrT3(CodeGenContext context, uint encoding)
        {
            InstIb26w1Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.Adr(context, inst.Rd, ImmUtils.CombineImmU12(inst.Imm8, inst.Imm3, inst.I), true);
        }

        public static void AesdA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCrypto.Aesd(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void AesdT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCrypto.Aesd(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void AeseA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCrypto.Aese(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void AeseT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCrypto.Aese(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void AesimcA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCrypto.Aesimc(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void AesimcT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCrypto.Aesimc(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void AesmcA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCrypto.Aesmc(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void AesmcT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCrypto.Aesmc(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void AndIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.AndI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm12), ImmUtils.ExpandedImmRotated(inst.Imm12), inst.S != 0);
        }

        public static void AndIT1(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.AndI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), ImmUtils.ExpandedImmRotated(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void AndRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.AndR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void AndRT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdnb16w3 inst = new(encoding);

            InstEmitAlu.AndR(context, inst.Rdn, inst.Rdn, inst.Rm, 0, 0, !context.InITBlock);
        }

        public static void AndRT2(CodeGenContext context, uint encoding)
        {
            InstSb20w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.AndR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.S != 0);
        }

        public static void AndRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.AndRr(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Rs, inst.S != 0);
        }

        public static void BA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Imm24b0w24 inst = new(encoding);

            InstEmitFlow.B(context, ImmUtils.ExtractSImm24Times4(inst.Imm24), (ArmCondition)inst.Cond);
        }

        public static void BT1(CodeGenContext context, uint encoding)
        {
            InstCondb24w4Imm8b16w8 inst = new(encoding);

            InstEmitFlow.B(context, ImmUtils.ExtractT16SImm8Times2(inst.Imm8), (ArmCondition)inst.Cond);
        }

        public static void BT2(CodeGenContext context, uint encoding)
        {
            InstImm11b16w11 inst = new(encoding);

            InstEmitFlow.B(context, ImmUtils.ExtractT16SImm11Times2(inst.Imm11), ArmCondition.Al);
        }

        public static void BT3(CodeGenContext context, uint encoding)
        {
            InstSb26w1Condb22w4Imm6b16w6J1b13w1J2b11w1Imm11b0w11 inst = new(encoding);

            InstEmitFlow.B(context, ImmUtils.CombineSImm20Times2(inst.Imm11, inst.Imm6, inst.J1, inst.J2, inst.S), (ArmCondition)inst.Cond);
        }

        public static void BT4(CodeGenContext context, uint encoding)
        {
            InstSb26w1Imm10b16w10J1b13w1J2b11w1Imm11b0w11 inst = new(encoding);

            InstEmitFlow.B(context, ImmUtils.CombineSImm24Times2(inst.Imm11, inst.Imm10, inst.J1, inst.J2, inst.S), ArmCondition.Al);
        }

        public static void BfcA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Msbb16w5Rdb12w4Lsbb7w5 inst = new(encoding);

            InstEmitBit.Bfc(context, inst.Rd, inst.Lsb, inst.Msb);
        }

        public static void BfcT1(CodeGenContext context, uint encoding)
        {
            InstImm3b12w3Rdb8w4Imm2b6w2Msbb0w5 inst = new(encoding);

            InstEmitBit.Bfc(context, inst.Rd, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.Msb);
        }

        public static void BfiA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Msbb16w5Rdb12w4Lsbb7w5Rnb0w4 inst = new(encoding);

            InstEmitBit.Bfi(context, inst.Rd, inst.Rn, inst.Lsb, inst.Msb);
        }

        public static void BfiT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Imm3b12w3Rdb8w4Imm2b6w2Msbb0w5 inst = new(encoding);

            InstEmitBit.Bfi(context, inst.Rd, inst.Rn, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.Msb);
        }

        public static void BicIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.BicI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm12), ImmUtils.ExpandedImmRotated(inst.Imm12), inst.S != 0);
        }

        public static void BicIT1(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.BicI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), ImmUtils.ExpandedImmRotated(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void BicRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.BicR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void BicRT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdnb16w3 inst = new(encoding);

            InstEmitAlu.BicR(context, inst.Rdn, inst.Rdn, inst.Rm, 0, 0, !context.InITBlock);
        }

        public static void BicRT2(CodeGenContext context, uint encoding)
        {
            InstSb20w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.BicR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.S != 0);
        }

        public static void BicRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.BicRr(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Rs, inst.S != 0);
        }

        public static void BkptA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Imm12b8w12Imm4b0w4 inst = new(encoding);

            InstEmitSystem.Bkpt(context, ImmUtils.CombineImmU16(inst.Imm12, inst.Imm4));
        }

        public static void BkptT1(CodeGenContext context, uint encoding)
        {
            InstImm8b16w8 inst = new(encoding);

            InstEmitSystem.Bkpt(context, inst.Imm8);
        }

        public static void BlxRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rmb0w4 inst = new(encoding);

            InstEmitFlow.Blx(context, inst.Rm, false);
        }

        public static void BlxRT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w4 inst = new(encoding);

            InstEmitFlow.Blx(context, inst.Rm, true);
        }

        public static void BlIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Imm24b0w24 inst = new(encoding);

            InstEmitFlow.Bl(context, ImmUtils.ExtractSImm24Times4(inst.Imm24), false, false);
        }

        public static void BlIA2(CodeGenContext context, uint encoding)
        {
            InstHb24w1Imm24b0w24 inst = new(encoding);

            InstEmitFlow.Bl(context, ImmUtils.ExtractSImm24Times4(inst.Imm24) | ((int)inst.H << 1), false, true);
        }

        public static void BlIT1(CodeGenContext context, uint encoding)
        {
            InstSb26w1Imm10b16w10J1b13w1J2b11w1Imm11b0w11 inst = new(encoding);

            InstEmitFlow.Bl(context, ImmUtils.CombineSImm24Times2(inst.Imm11, inst.Imm10, inst.J1, inst.J2, inst.S), true, true);
        }

        public static void BlIT2(CodeGenContext context, uint encoding)
        {
            InstSb26w1Imm10hb16w10J1b13w1J2b11w1Imm10lb1w10Hb0w1 inst = new(encoding);

            InstEmitFlow.Bl(context, ImmUtils.CombineSImm24Times4(inst.Imm10l, inst.Imm10h, inst.J1, inst.J2, inst.S), true, false);
        }

        public static void BxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rmb0w4 inst = new(encoding);

            InstEmitFlow.Bx(context, inst.Rm);
        }

        public static void BxT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w4 inst = new(encoding);

            InstEmitFlow.Bx(context, inst.Rm);
        }

        public static void BxjA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rmb0w4 inst = new(encoding);

            InstEmitFlow.Bx(context, inst.Rm);
        }

        public static void BxjT1(CodeGenContext context, uint encoding)
        {
            InstRmb16w4 inst = new(encoding);

            InstEmitFlow.Bx(context, inst.Rm);
        }

        public static void CbnzT1(CodeGenContext context, uint encoding)
        {
            InstOpb27w1Ib25w1Imm5b19w5Rnb16w3 inst = new(encoding);

            InstEmitFlow.Cbnz(context, inst.Rn, (int)((inst.Imm5 << 1) | (inst.I << 6)), inst.Op != 0);
        }

        public static void ClrbhbA1(CodeGenContext context, uint encoding)
        {
            _ = new InstCondb28w4(encoding);

            throw new NotImplementedException();
        }

        public static void ClrbhbT1(CodeGenContext context, uint encoding)
        {
            throw new NotImplementedException();
        }

        public static void ClrexA1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Clrex();
        }

        public static void ClrexT1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Clrex();
        }

        public static void ClzA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitBit.Clz(context, inst.Rd, inst.Rm);
        }

        public static void ClzT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitBit.Clz(context, inst.Rd, inst.Rm);
        }

        public static void CmnIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.CmnI(context, inst.Rn, ImmUtils.ExpandImm(inst.Imm12));
        }

        public static void CmnIT1(CodeGenContext context, uint encoding)
        {
            InstIb26w1Rnb16w4Imm3b12w3Imm8b0w8 inst = new(encoding);

            InstEmitAlu.CmnI(context, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I));
        }

        public static void CmnRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.CmnR(context, inst.Rn, inst.Rm, inst.Stype, inst.Imm5);
        }

        public static void CmnRT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rnb16w3 inst = new(encoding);

            InstEmitAlu.CmnR(context, inst.Rn, inst.Rm, 0, 0);
        }

        public static void CmnRT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Imm3b12w3Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.CmnR(context, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3));
        }

        public static void CmnRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.CmnRr(context, inst.Rn, inst.Rm, inst.Stype, inst.Rs);
        }

        public static void CmpIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.CmpI(context, inst.Rn, ImmUtils.ExpandImm(inst.Imm12));
        }

        public static void CmpIT1(CodeGenContext context, uint encoding)
        {
            InstRnb24w3Imm8b16w8 inst = new(encoding);

            InstEmitAlu.CmpI(context, inst.Rn, inst.Imm8);
        }

        public static void CmpIT2(CodeGenContext context, uint encoding)
        {
            InstIb26w1Rnb16w4Imm3b12w3Imm8b0w8 inst = new(encoding);

            InstEmitAlu.CmpI(context, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I));
        }

        public static void CmpRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.CmpR(context, inst.Rn, inst.Rm, inst.Stype, inst.Imm5);
        }

        public static void CmpRT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rnb16w3 inst = new(encoding);

            InstEmitAlu.CmpR(context, inst.Rn, inst.Rm, 0, 0);
        }

        public static void CmpRT2(CodeGenContext context, uint encoding)
        {
            InstNb23w1Rmb19w4Rnb16w3 inst = new(encoding);

            InstEmitAlu.CmpR(context, inst.Rn | (inst.N << 3), inst.Rm, 0, 0);
        }

        public static void CmpRT3(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Imm3b12w3Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.CmpR(context, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3));
        }

        public static void CmpRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.CmpRr(context, inst.Rn, inst.Rm, inst.Stype, inst.Rs);
        }

        public static void CpsA1(CodeGenContext context, uint encoding)
        {
            InstImodb18w2Mb17w1Ab8w1Ib7w1Fb6w1Modeb0w5 inst = new(encoding);

            InstEmitSystem.Cps(context, inst.Imod, inst.M, inst.A, inst.I, inst.F, inst.Mode);
        }

        public static void CpsT1(CodeGenContext context, uint encoding)
        {
            InstImb20w1Ab18w1Ib17w1Fb16w1 inst = new(encoding);

            InstEmitSystem.Cps(context, inst.Im, 0, inst.A, inst.I, inst.F, 0);
        }

        public static void CpsT2(CodeGenContext context, uint encoding)
        {
            InstImodb9w2Mb8w1Ab7w1Ib6w1Fb5w1Modeb0w5 inst = new(encoding);

            InstEmitSystem.Cps(context, inst.Imod, inst.M, inst.A, inst.I, inst.F, inst.Mode);
        }

        public static void Crc32A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Szb21w2Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitCrc32.Crc32(context, inst.Rd, inst.Rn, inst.Rm, inst.Sz);
        }

        public static void Crc32T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Szb4w2Rmb0w4 inst = new(encoding);

            InstEmitCrc32.Crc32(context, inst.Rd, inst.Rn, inst.Rm, inst.Sz);
        }

        public static void Crc32cA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Szb21w2Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitCrc32.Crc32c(context, inst.Rd, inst.Rn, inst.Rm, inst.Sz);
        }

        public static void Crc32cT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Szb4w2Rmb0w4 inst = new(encoding);

            InstEmitCrc32.Crc32c(context, inst.Rd, inst.Rn, inst.Rm, inst.Sz);
        }

        public static void CsdbA1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Csdb();
        }

        public static void CsdbT1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Csdb();
        }

        public static void DbgA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Optionb0w4 inst = new(encoding);

            InstEmitSystem.Dbg(context, inst.Option);
        }

        public static void DbgT1(CodeGenContext context, uint encoding)
        {
            InstOptionb0w4 inst = new(encoding);

            InstEmitSystem.Dbg(context, inst.Option);
        }

        public static void Dcps1T1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void Dcps2T1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void Dcps3T1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void DmbA1(CodeGenContext context, uint encoding)
        {
            InstOptionb0w4 inst = new(encoding);

            context.Arm64Assembler.Dmb(inst.Option);
        }

        public static void DmbT1(CodeGenContext context, uint encoding)
        {
            InstOptionb0w4 inst = new(encoding);

            context.Arm64Assembler.Dmb(inst.Option);
        }

        public static void DsbA1(CodeGenContext context, uint encoding)
        {
            InstOptionb0w4 inst = new(encoding);

            context.Arm64Assembler.Dsb(inst.Option);
        }

        public static void DsbT1(CodeGenContext context, uint encoding)
        {
            InstOptionb0w4 inst = new(encoding);

            context.Arm64Assembler.Dsb(inst.Option);
        }

        public static void EorIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.EorI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm12), ImmUtils.ExpandedImmRotated(inst.Imm12), inst.S != 0);
        }

        public static void EorIT1(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.EorI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), ImmUtils.ExpandedImmRotated(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void EorRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.EorR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void EorRT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdnb16w3 inst = new(encoding);

            InstEmitAlu.EorR(context, inst.Rdn, inst.Rdn, inst.Rm, 0, 0, !context.InITBlock);
        }

        public static void EorRT2(CodeGenContext context, uint encoding)
        {
            InstSb20w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.EorR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.S != 0);
        }

        public static void EorRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.EorRr(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Rs, inst.S != 0);
        }

        public static void EretA1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void EretT1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void EsbA1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Esb();
        }

        public static void EsbT1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Esb();
        }

        public static void FldmxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm871b1w7 inst = new(encoding);

            InstEmitNeonMemory.Vldm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Imm871, inst.U != 0, inst.W != 0, singleRegs: false);
        }

        public static void FldmxT1(CodeGenContext context, uint encoding)
        {
            InstPb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm871b1w7 inst = new(encoding);

            InstEmitNeonMemory.Vldm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Imm871, inst.U != 0, inst.W != 0, singleRegs: false);
        }

        public static void FstmxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm871b1w7 inst = new(encoding);

            InstEmitNeonMemory.Vstm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Imm871, inst.U != 0, inst.W != 0, singleRegs: false);
        }

        public static void FstmxT1(CodeGenContext context, uint encoding)
        {
            InstPb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm871b1w7 inst = new(encoding);

            InstEmitNeonMemory.Vstm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Imm871, inst.U != 0, inst.W != 0, singleRegs: false);
        }

        public static void HltA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Imm12b8w12Imm4b0w4 inst = new(encoding);

            InstEmitSystem.Hlt(context, ImmUtils.CombineImmU16(inst.Imm12, inst.Imm4));
        }

        public static void HltT1(CodeGenContext context, uint encoding)
        {
            InstImm6b16w6 inst = new(encoding);

            InstEmitSystem.Hlt(context, inst.Imm6);
        }

        public static void HvcA1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void HvcT1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void IsbA1(CodeGenContext context, uint encoding)
        {
            InstOptionb0w4 inst = new(encoding);

            context.Arm64Assembler.Isb(inst.Option);
        }

        public static void IsbT1(CodeGenContext context, uint encoding)
        {
            InstOptionb0w4 inst = new(encoding);

            context.Arm64Assembler.Isb(inst.Option);
        }

        public static void ItT1(CodeGenContext context, uint encoding)
        {
            InstFirstcondb20w4Maskb16w4 inst = new(encoding);

            InstEmitFlow.It(context, inst.Firstcond, inst.Mask);
        }

        public static void LdaA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Lda(context, inst.Rt, inst.Rn);
        }

        public static void LdaT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Lda(context, inst.Rt, inst.Rn);
        }

        public static void LdabA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldab(context, inst.Rt, inst.Rn);
        }

        public static void LdabT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldab(context, inst.Rt, inst.Rn);
        }

        public static void LdaexA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldaex(context, inst.Rt, inst.Rn);
        }

        public static void LdaexT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldaex(context, inst.Rt, inst.Rn);
        }

        public static void LdaexbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldaexb(context, inst.Rt, inst.Rn);
        }

        public static void LdaexbT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldaexb(context, inst.Rt, inst.Rn);
        }

        public static void LdaexdA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldaexd(context, inst.Rt, RegisterUtils.GetRt2(inst.Rt), inst.Rn);
        }

        public static void LdaexdT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Rt2b8w4 inst = new(encoding);

            InstEmitMemory.Ldaexd(context, inst.Rt, inst.Rt2, inst.Rn);
        }

        public static void LdaexhA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldaexh(context, inst.Rt, inst.Rn);
        }

        public static void LdaexhT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldaexh(context, inst.Rt, inst.Rn);
        }

        public static void LdahA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldah(context, inst.Rt, inst.Rn);
        }

        public static void LdahT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldah(context, inst.Rt, inst.Rn);
        }

        public static void LdcIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdcI(context, inst.Rn, (int)inst.Imm8 << 2, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdcIT1(CodeGenContext context, uint encoding)
        {
            InstPb24w1Ub23w1Wb21w1Rnb16w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdcI(context, inst.Rn, (int)inst.Imm8 << 2, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdcLA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdcL(context, inst.Imm8 << 2, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdcLT1(CodeGenContext context, uint encoding)
        {
            InstPb24w1Ub23w1Wb21w1Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdcL(context, inst.Imm8 << 2, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdmA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Wb21w1Rnb16w4RegisterListb0w16 inst = new(encoding);

            InstEmitMemory.Ldm(context, inst.Rn, inst.RegisterList, inst.W != 0);
        }

        public static void LdmT1(CodeGenContext context, uint encoding)
        {
            InstRnb24w3RegisterListb16w8 inst = new(encoding);

            InstEmitMemory.Ldm(context, inst.Rn, inst.RegisterList, false);
        }

        public static void LdmT2(CodeGenContext context, uint encoding)
        {
            InstWb21w1Rnb16w4Pb15w1Mb14w1RegisterListb0w14 inst = new(encoding);

            InstEmitMemory.Ldm(context, inst.Rn, ImmUtils.CombineRegisterList(inst.RegisterList, inst.M, inst.P), inst.W != 0);
        }

        public static void LdmdaA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Wb21w1Rnb16w4RegisterListb0w16 inst = new(encoding);

            InstEmitMemory.Ldmda(context, inst.Rn, inst.RegisterList, inst.W != 0);
        }

        public static void LdmdbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Wb21w1Rnb16w4RegisterListb0w16 inst = new(encoding);

            InstEmitMemory.Ldmdb(context, inst.Rn, inst.RegisterList, inst.W != 0);
        }

        public static void LdmdbT1(CodeGenContext context, uint encoding)
        {
            InstWb21w1Rnb16w4Pb15w1Mb14w1RegisterListb0w14 inst = new(encoding);

            InstEmitMemory.Ldmdb(context, inst.Rn, ImmUtils.CombineRegisterList(inst.RegisterList, inst.M, inst.P), inst.W != 0);
        }

        public static void LdmibA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Wb21w1Rnb16w4RegisterListb0w16 inst = new(encoding);

            InstEmitMemory.Ldmib(context, inst.Rn, inst.RegisterList, inst.W != 0);
        }

        public static void LdmEA1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void LdmUA1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void LdrbtA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrbtI(context, inst.Rt, inst.Rn, (int)inst.Imm12, postIndex: true, inst.U != 0);
        }

        public static void LdrbtA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrbtR(context, inst.Rt, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, postIndex: true, inst.U != 0);
        }

        public static void LdrbtT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdrbtI(context, inst.Rt, inst.Rn, (int)inst.Imm8, postIndex: false, true);
        }

        public static void LdrbIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrbI(context, inst.Rt, inst.Rn, (int)inst.Imm12, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrbIT1(CodeGenContext context, uint encoding)
        {
            InstImm5b22w5Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.LdrbI(context, inst.Rt, inst.Rn, (int)inst.Imm5, true, true, false);
        }

        public static void LdrbIT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrbI(context, inst.Rt, inst.Rn, (int)inst.Imm12, true, true, false);
        }

        public static void LdrbIT3(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Pb10w1Ub9w1Wb8w1Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdrbI(context, inst.Rt, inst.Rn, (int)inst.Imm8, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrbLA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrbL(context, inst.Rt, inst.Imm12, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrbLT1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrbL(context, inst.Rt, inst.Imm12, true, inst.U != 0, false);
        }

        public static void LdrbRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrbR(context, inst.Rt, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrbRT1(CodeGenContext context, uint encoding)
        {
            InstRmb22w3Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.LdrbR(context, inst.Rt, inst.Rn, inst.Rm, 0, 0, true, true, false);
        }

        public static void LdrbRT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm2b4w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrbR(context, inst.Rt, inst.Rn, inst.Rm, 0, inst.Imm2, true, true, false);
        }

        public static void LdrdIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.LdrdI(context, inst.Rt, RegisterUtils.GetRt2(inst.Rt), inst.Rn, ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrdIT1(CodeGenContext context, uint encoding)
        {
            InstPb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Rt2b8w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdrdI(context, inst.Rt, inst.Rt2, inst.Rn, inst.Imm8 << 2, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrdLA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.LdrdL(context, inst.Rt, RegisterUtils.GetRt2(inst.Rt), ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), true, inst.U != 0, false);
        }

        public static void LdrdLT1(CodeGenContext context, uint encoding)
        {
            InstPb24w1Ub23w1Wb21w1Rtb12w4Rt2b8w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdrdL(context, inst.Rt, inst.Rt2, inst.Imm8, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrdRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrdR(context, inst.Rt, RegisterUtils.GetRt2(inst.Rt), inst.Rn, inst.Rm, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrexA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldrex(context, inst.Rt, inst.Rn);
        }

        public static void LdrexT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.Ldrex(context, inst.Rt, inst.Rn);
        }

        public static void LdrexbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldrexb(context, inst.Rt, inst.Rn);
        }

        public static void LdrexbT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldrexb(context, inst.Rt, inst.Rn);
        }

        public static void LdrexdA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldrexd(context, inst.Rt, RegisterUtils.GetRt2(inst.Rt), inst.Rn);
        }

        public static void LdrexdT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Rt2b8w4 inst = new(encoding);

            InstEmitMemory.Ldrexd(context, inst.Rt, inst.Rt2, inst.Rn);
        }

        public static void LdrexhA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldrexh(context, inst.Rt, inst.Rn);
        }

        public static void LdrexhT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Ldrexh(context, inst.Rt, inst.Rn);
        }

        public static void LdrhtA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.LdrhtI(context, inst.Rt, inst.Rn, (int)ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), postIndex: true, inst.U != 0);
        }

        public static void LdrhtA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrhtR(context, inst.Rt, inst.Rn, inst.Rm, postIndex: true, inst.U != 0);
        }

        public static void LdrhtT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdrhtI(context, inst.Rt, inst.Rn, (int)inst.Imm8, postIndex: false, true);
        }

        public static void LdrhIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.LdrhI(context, inst.Rt, inst.Rn, (int)ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrhIT1(CodeGenContext context, uint encoding)
        {
            InstImm5b22w5Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.LdrhI(context, inst.Rt, inst.Rn, (int)inst.Imm5 << 1, true, true, false);
        }

        public static void LdrhIT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrhI(context, inst.Rt, inst.Rn, (int)inst.Imm12, true, true, false);
        }

        public static void LdrhIT3(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Pb10w1Ub9w1Wb8w1Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdrhI(context, inst.Rt, inst.Rn, (int)inst.Imm8, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrhLA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.LdrhL(context, inst.Rt, ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrhLT1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrhL(context, inst.Rt, inst.Imm12, true, inst.U != 0, false);
        }

        public static void LdrhRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrhR(context, inst.Rt, inst.Rn, inst.Rm, 0, 0, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrhRT1(CodeGenContext context, uint encoding)
        {
            InstRmb22w3Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.LdrhR(context, inst.Rt, inst.Rn, inst.Rm, 0, 0, true, true, false);
        }

        public static void LdrhRT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm2b4w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrhR(context, inst.Rt, inst.Rn, inst.Rm, 0, inst.Imm2, true, true, false);
        }

        public static void LdrsbtA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.LdrsbtI(context, inst.Rt, inst.Rn, (int)ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), postIndex: true, inst.U != 0);
        }

        public static void LdrsbtA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrsbtR(context, inst.Rt, inst.Rn, inst.Rm, postIndex: true, inst.U != 0);
        }

        public static void LdrsbtT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdrsbtI(context, inst.Rt, inst.Rn, (int)inst.Imm8, postIndex: false, true);
        }

        public static void LdrsbIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.LdrsbI(context, inst.Rt, inst.Rn, (int)ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrsbIT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrsbI(context, inst.Rt, inst.Rn, (int)inst.Imm12, true, true, false);
        }

        public static void LdrsbIT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Pb10w1Ub9w1Wb8w1Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdrsbI(context, inst.Rt, inst.Rn, (int)inst.Imm8, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrsbLA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.LdrsbL(context, inst.Rt, ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrsbLT1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrsbL(context, inst.Rt, inst.Imm12, true, inst.U != 0, false);
        }

        public static void LdrsbRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrsbR(context, inst.Rt, inst.Rn, inst.Rm, 0, 0, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrsbRT1(CodeGenContext context, uint encoding)
        {
            InstRmb22w3Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.LdrsbR(context, inst.Rt, inst.Rn, inst.Rm, 0, 0, true, true, false);
        }

        public static void LdrsbRT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm2b4w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrsbR(context, inst.Rt, inst.Rn, inst.Rm, 0, inst.Imm2, true, true, false);
        }

        public static void LdrshtA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.LdrshtI(context, inst.Rt, inst.Rn, (int)ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), postIndex: true, inst.U != 0);
        }

        public static void LdrshtA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrshtR(context, inst.Rt, inst.Rn, inst.Rm, postIndex: true, inst.U != 0);
        }

        public static void LdrshtT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdrshtI(context, inst.Rt, inst.Rn, (int)inst.Imm8, postIndex: false, true);
        }

        public static void LdrshIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.LdrshI(context, inst.Rt, inst.Rn, (int)ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrshIT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrshI(context, inst.Rt, inst.Rn, (int)inst.Imm12, true, true, false);
        }

        public static void LdrshIT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Pb10w1Ub9w1Wb8w1Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdrshI(context, inst.Rt, inst.Rn, (int)inst.Imm8, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrshLA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.LdrshL(context, inst.Rt, ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrshLT1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrshL(context, inst.Rt, inst.Imm12, true, inst.U != 0, false);
        }

        public static void LdrshRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrshR(context, inst.Rt, inst.Rn, inst.Rm, 0, 0, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrshRT1(CodeGenContext context, uint encoding)
        {
            InstRmb22w3Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.LdrshR(context, inst.Rt, inst.Rn, inst.Rm, 0, 0, true, true, false);
        }

        public static void LdrshRT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm2b4w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrshR(context, inst.Rt, inst.Rn, inst.Rm, 0, inst.Imm2, true, true, false);
        }

        public static void LdrtA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrtI(context, inst.Rt, inst.Rn, (int)inst.Imm12, postIndex: true, inst.U != 0);
        }

        public static void LdrtA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrtR(context, inst.Rt, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, postIndex: true, inst.U != 0);
        }

        public static void LdrtT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdrtI(context, inst.Rt, inst.Rn, (int)inst.Imm8, postIndex: false, true);
        }

        public static void LdrIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrI(context, inst.Rt, inst.Rn, (int)inst.Imm12, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrIT1(CodeGenContext context, uint encoding)
        {
            InstImm5b22w5Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.LdrI(context, inst.Rt, inst.Rn, (int)inst.Imm5 << 2, true, true, false);
        }

        public static void LdrIT2(CodeGenContext context, uint encoding)
        {
            InstRtb24w3Imm8b16w8 inst = new(encoding);

            InstEmitMemory.LdrI(context, inst.Rt, RegisterUtils.SpRegister, (int)inst.Imm8 << 2, true, true, false);
        }

        public static void LdrIT3(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrI(context, inst.Rt, inst.Rn, (int)inst.Imm12, true, true, false);
        }

        public static void LdrIT4(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Pb10w1Ub9w1Wb8w1Imm8b0w8 inst = new(encoding);

            InstEmitMemory.LdrI(context, inst.Rt, inst.Rn, (int)inst.Imm8, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrLA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrL(context, inst.Rt, inst.Imm12, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrLT1(CodeGenContext context, uint encoding)
        {
            InstRtb24w3Imm8b16w8 inst = new(encoding);

            InstEmitMemory.LdrL(context, inst.Rt, inst.Imm8 << 2, true, true, false);
        }

        public static void LdrLT2(CodeGenContext context, uint encoding)
        {
            InstUb23w1Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.LdrL(context, inst.Rt, inst.Imm12, true, inst.U != 0, false);
        }

        public static void LdrRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrR(context, inst.Rt, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void LdrRT1(CodeGenContext context, uint encoding)
        {
            InstRmb22w3Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.LdrR(context, inst.Rt, inst.Rn, inst.Rm, 0, 0, true, true, false);
        }

        public static void LdrRT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm2b4w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.LdrR(context, inst.Rt, inst.Rn, inst.Rm, 0, inst.Imm2, true, true, false);
        }

        public static void McrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Opc1b21w3Crnb16w4Rtb12w4Coproc0b8w1Opc2b5w3Crmb0w4 inst = new(encoding);

            InstEmitSystem.Mcr(context, encoding, inst.Coproc0 | 0xe, inst.Opc1, inst.Rt, inst.Crn, inst.Crm, inst.Opc2);
        }

        public static void McrT1(CodeGenContext context, uint encoding)
        {
            InstOpc1b21w3Crnb16w4Rtb12w4Coproc0b8w1Opc2b5w3Crmb0w4 inst = new(encoding);

            InstEmitSystem.Mcr(context, encoding, inst.Coproc0 | 0xe, inst.Opc1, inst.Rt, inst.Crn, inst.Crm, inst.Opc2);
        }

        public static void McrrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rt2b16w4Rtb12w4Coproc0b8w1Opc1b4w4Crmb0w4 inst = new(encoding);

            InstEmitSystem.Mcrr(context, encoding, inst.Coproc0 | 0xe, inst.Opc1, inst.Rt, inst.Crm);
        }

        public static void McrrT1(CodeGenContext context, uint encoding)
        {
            InstRt2b16w4Rtb12w4Coproc0b8w1Opc1b4w4Crmb0w4 inst = new(encoding);

            InstEmitSystem.Mcrr(context, encoding, inst.Coproc0 | 0xe, inst.Opc1, inst.Rt, inst.Crm);
        }

        public static void MlaA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdb16w4Rab12w4Rmb8w4Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Mla(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra);
        }

        public static void MlaT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rab12w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Mla(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra);
        }

        public static void MlsA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rab12w4Rmb8w4Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Mls(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra);
        }

        public static void MlsT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rab12w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Mls(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra);
        }

        public static void MovtA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Imm4b16w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMove.Movt(context, inst.Rd, ImmUtils.CombineImmU16(inst.Imm12, inst.Imm4));
        }

        public static void MovtT1(CodeGenContext context, uint encoding)
        {
            InstIb26w1Imm4b16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitMove.Movt(context, inst.Rd, ImmUtils.CombineImmU16(inst.Imm8, inst.Imm3, inst.I, inst.Imm4));
        }

        public static void MovIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMove.MovI(context, inst.Rd, ImmUtils.ExpandImm(inst.Imm12), ImmUtils.ExpandedImmRotated(inst.Imm12), inst.S != 0);
        }

        public static void MovIA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Imm4b16w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMove.MovI(context, inst.Rd, ImmUtils.CombineImmU16(inst.Imm12, inst.Imm4), false, false);
        }

        public static void MovIT1(CodeGenContext context, uint encoding)
        {
            InstRdb24w3Imm8b16w8 inst = new(encoding);

            InstEmitMove.MovI(context, inst.Rd, inst.Imm8, false, !context.InITBlock);
        }

        public static void MovIT2(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitMove.MovI(context, inst.Rd, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), ImmUtils.ExpandedImmRotated(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void MovIT3(CodeGenContext context, uint encoding)
        {
            InstIb26w1Imm4b16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitMove.MovI(context, inst.Rd, ImmUtils.CombineImmU16(inst.Imm8, inst.Imm3, inst.I, inst.Imm4), false, false);
        }

        public static void MovRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMove.MovR(context, inst.Cond, inst.Rd, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void MovRT1(CodeGenContext context, uint encoding)
        {
            InstDb23w1Rmb19w4Rdb16w3 inst = new(encoding);

            InstEmitMove.MovR(context, inst.Rd | (inst.D << 3), inst.Rm, 0, 0, false);
        }

        public static void MovRT2(CodeGenContext context, uint encoding)
        {
            InstOpb27w2Imm5b22w5Rmb19w3Rdb16w3 inst = new(encoding);

            InstEmitMove.MovR(context, inst.Rd, inst.Rm, inst.Op, inst.Imm5, !context.InITBlock);
        }

        public static void MovRT3(CodeGenContext context, uint encoding)
        {
            InstSb20w1Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitMove.MovR(context, inst.Rd, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.S != 0);
        }

        public static void MovRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdb12w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMove.MovRr(context, inst.Rd, inst.Rm, inst.Stype, inst.Rs, inst.S != 0);
        }

        public static void MovRrT1(CodeGenContext context, uint encoding)
        {
            InstRsb19w3Rdmb16w3 inst = new(encoding);

            InstEmitMove.MovRr(context, inst.Rdm, inst.Rdm, ((encoding >> 7) & 2) | ((encoding >> 6) & 1), inst.Rs, !context.InITBlock);
        }

        public static void MovRrT2(CodeGenContext context, uint encoding)
        {
            InstStypeb21w2Sb20w1Rmb16w4Rdb8w4Rsb0w4 inst = new(encoding);

            InstEmitMove.MovRr(context, inst.Rd, inst.Rm, inst.Stype, inst.Rs, inst.S != 0);
        }

        public static void MrcA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Opc1b21w3Crnb16w4Rtb12w4Coproc0b8w1Opc2b5w3Crmb0w4 inst = new(encoding);

            InstEmitSystem.Mrc(context, encoding, inst.Coproc0 | 0xe, inst.Opc1, inst.Rt, inst.Crn, inst.Crm, inst.Opc2);
        }

        public static void MrcT1(CodeGenContext context, uint encoding)
        {
            InstOpc1b21w3Crnb16w4Rtb12w4Coproc0b8w1Opc2b5w3Crmb0w4 inst = new(encoding);

            InstEmitSystem.Mrc(context, encoding, inst.Coproc0 | 0xe, inst.Opc1, inst.Rt, inst.Crn, inst.Crm, inst.Opc2);
        }

        public static void MrrcA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rt2b16w4Rtb12w4Coproc0b8w1Opc1b4w4Crmb0w4 inst = new(encoding);

            InstEmitSystem.Mrrc(context, encoding, inst.Coproc0 | 0xe, inst.Opc1, inst.Rt, inst.Rt2, inst.Crm);
        }

        public static void MrrcT1(CodeGenContext context, uint encoding)
        {
            InstRt2b16w4Rtb12w4Coproc0b8w1Opc1b4w4Crmb0w4 inst = new(encoding);

            InstEmitSystem.Mrrc(context, encoding, inst.Coproc0 | 0xe, inst.Opc1, inst.Rt, inst.Rt2, inst.Crm);
        }

        public static void MrsA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rb22w1Rdb12w4 inst = new(encoding);

            InstEmitSystem.Mrs(context, inst.Rd, inst.R != 0);
        }

        public static void MrsT1(CodeGenContext context, uint encoding)
        {
            InstRb20w1Rdb8w4 inst = new(encoding);

            InstEmitSystem.Mrs(context, inst.Rd, inst.R != 0);
        }

        public static void MrsBrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rb22w1M1b16w4Rdb12w4Mb8w1 inst = new(encoding);

            InstEmitSystem.MrsBr(context, inst.Rd, inst.M1 | (inst.M << 4), inst.R != 0);
        }

        public static void MrsBrT1(CodeGenContext context, uint encoding)
        {
            InstRb20w1M1b16w4Rdb8w4Mb4w1 inst = new(encoding);

            InstEmitSystem.MrsBr(context, inst.Rd, inst.M1 | (inst.M << 4), inst.R != 0);
        }

        public static void MsrBrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rb22w1M1b16w4Mb8w1Rnb0w4 inst = new(encoding);

            InstEmitSystem.MsrBr(context, inst.Rn, inst.M1 | (inst.M << 4), inst.R != 0);
        }

        public static void MsrBrT1(CodeGenContext context, uint encoding)
        {
            InstRb20w1Rnb16w4M1b8w4Mb4w1 inst = new(encoding);

            InstEmitSystem.MsrBr(context, inst.Rn, inst.M1 | (inst.M << 4), inst.R != 0);
        }

        public static void MsrIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rb22w1Maskb16w4Imm12b0w12 inst = new(encoding);

            InstEmitSystem.MsrI(context, inst.Imm12, inst.Mask, inst.R != 0);
        }

        public static void MsrRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rb22w1Maskb16w4Rnb0w4 inst = new(encoding);

            InstEmitSystem.MsrR(context, inst.Rn, inst.Mask, inst.R != 0);
        }

        public static void MsrRT1(CodeGenContext context, uint encoding)
        {
            InstRb20w1Rnb16w4Maskb8w4 inst = new(encoding);

            InstEmitSystem.MsrR(context, inst.Rn, inst.Mask, inst.R != 0);
        }

        public static void MulA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdb16w4Rmb8w4Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Mul(context, inst.Rd, inst.Rn, inst.Rm, inst.S != 0);
        }

        public static void MulT1(CodeGenContext context, uint encoding)
        {
            InstRnb19w3Rdmb16w3 inst = new(encoding);

            InstEmitMultiply.Mul(context, inst.Rdm, inst.Rn, inst.Rdm, !context.InITBlock);
        }

        public static void MulT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Mul(context, inst.Rd, inst.Rn, inst.Rm, false);
        }

        public static void MvnIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMove.MvnI(context, inst.Rd, ImmUtils.ExpandImm(inst.Imm12), ImmUtils.ExpandedImmRotated(inst.Imm12), inst.S != 0);
        }

        public static void MvnIT1(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitMove.MvnI(context, inst.Rd, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), ImmUtils.ExpandedImmRotated(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void MvnRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMove.MvnR(context, inst.Rd, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void MvnRT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdb16w3 inst = new(encoding);

            InstEmitMove.MvnR(context, inst.Rd, inst.Rm, 0, 0, !context.InITBlock);
        }

        public static void MvnRT2(CodeGenContext context, uint encoding)
        {
            InstSb20w1Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitMove.MvnR(context, inst.Rd, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.S != 0);
        }

        public static void MvnRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdb12w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMove.MvnRr(context, inst.Rd, inst.Rm, inst.Stype, inst.Rs, inst.S != 0);
        }

        public static void NopA1(CodeGenContext context, uint encoding)
        {
        }

        public static void NopT1(CodeGenContext context, uint encoding)
        {
        }

        public static void NopT2(CodeGenContext context, uint encoding)
        {
        }

        public static void OrnIT1(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.OrnI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), ImmUtils.ExpandedImmRotated(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void OrnRT1(CodeGenContext context, uint encoding)
        {
            InstSb20w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.OrnR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.S != 0);
        }

        public static void OrrIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.OrrI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm12), ImmUtils.ExpandedImmRotated(inst.Imm12), inst.S != 0);
        }

        public static void OrrIT1(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.OrrI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), ImmUtils.ExpandedImmRotated(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void OrrRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.OrrR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void OrrRT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdnb16w3 inst = new(encoding);

            InstEmitAlu.OrrR(context, inst.Rdn, inst.Rdn, inst.Rm, 0, 0, !context.InITBlock);
        }

        public static void OrrRT2(CodeGenContext context, uint encoding)
        {
            InstSb20w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.OrrR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.S != 0);
        }

        public static void OrrRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.OrrRr(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Rs, inst.S != 0);
        }

        public static void PkhA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Imm5b7w5Tbb6w1Rmb0w4 inst = new(encoding);

            InstEmitMove.Pkh(context, inst.Rd, inst.Rn, inst.Rm, inst.Tb != 0, inst.Imm5);
        }

        public static void PkhT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Imm3b12w3Rdb8w4Imm2b6w2Tbb5w1Rmb0w4 inst = new(encoding);

            InstEmitMove.Pkh(context, inst.Rd, inst.Rn, inst.Rm, inst.Tb != 0, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3));
        }

        public static void PldIA1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Rb22w1Rnb16w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.PldI(context, inst.Rn, inst.Imm12, inst.U != 0, inst.R != 0);
        }

        public static void PldIT1(CodeGenContext context, uint encoding)
        {
            InstWb21w1Rnb16w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.PldI(context, inst.Rn, inst.Imm12, true, inst.W == 0);
        }

        public static void PldIT2(CodeGenContext context, uint encoding)
        {
            InstWb21w1Rnb16w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.PldI(context, inst.Rn, inst.Imm8, false, inst.W == 0);
        }

        public static void PldLA1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Imm12b0w12 inst = new(encoding);

            InstEmitMemory.PldL(context, inst.Imm12, inst.U != 0);
        }

        public static void PldLT1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Imm12b0w12 inst = new(encoding);

            InstEmitMemory.PldL(context, inst.Imm12, inst.U != 0);
        }

        public static void PldRA1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Rb22w1Rnb16w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.PldR(context, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.U != 0, inst.R != 0);
        }

        public static void PldRT1(CodeGenContext context, uint encoding)
        {
            InstWb21w1Rnb16w4Imm2b4w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.PldR(context, inst.Rn, inst.Rm, 0, inst.Imm2, true, inst.W == 0);
        }

        public static void PliIA1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Rnb16w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.PliI(context, inst.Rn, inst.Imm12, inst.U != 0);
        }

        public static void PliIT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.PliI(context, inst.Rn, inst.Imm12, true);
        }

        public static void PliIT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.PliI(context, inst.Rn, inst.Imm8, false);
        }

        public static void PliIT3(CodeGenContext context, uint encoding)
        {
            InstUb23w1Imm12b0w12 inst = new(encoding);

            InstEmitMemory.PliL(context, inst.Imm12, inst.U != 0);
        }

        public static void PliRA1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Rnb16w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.PliR(context, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.U != 0);
        }

        public static void PliRT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Imm2b4w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.PliR(context, inst.Rn, inst.Rm, 0, inst.Imm2, true);
        }

        public static void PopT1(CodeGenContext context, uint encoding)
        {
            InstPb24w1RegisterListb16w8 inst = new(encoding);

            InstEmitMemory.Ldm(context, RegisterUtils.SpRegister, inst.RegisterList | (inst.P << RegisterUtils.PcRegister), true);
        }

        public static void PssbbA1(CodeGenContext context, uint encoding)
        {
            throw new NotImplementedException();
        }

        public static void PssbbT1(CodeGenContext context, uint encoding)
        {
            throw new NotImplementedException();
        }

        public static void PushT1(CodeGenContext context, uint encoding)
        {
            InstMb24w1RegisterListb16w8 inst = new(encoding);

            InstEmitMemory.Stmdb(context, RegisterUtils.SpRegister, inst.RegisterList | (inst.M << RegisterUtils.LrRegister), true);
        }

        public static void QaddA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qadd(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void QaddT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qadd(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Qadd16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qadd16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Qadd16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qadd16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Qadd8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qadd8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Qadd8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qadd8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void QasxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qasx(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void QasxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qasx(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void QdaddA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qdadd(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void QdaddT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qdadd(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void QdsubA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qdsub(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void QdsubT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qdsub(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void QsaxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qsax(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void QsaxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qsax(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void QsubA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qsub(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void QsubT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qsub(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Qsub16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qsub16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Qsub16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qsub16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Qsub8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qsub8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Qsub8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Qsub8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void RbitA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitBit.Rbit(context, inst.Rd, inst.Rm);
        }

        public static void RbitT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitBit.Rbit(context, inst.Rd, inst.Rm);
        }

        public static void RevA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitBit.Rev(context, inst.Rd, inst.Rm);
        }

        public static void RevT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdb16w3 inst = new(encoding);

            InstEmitBit.Rev(context, inst.Rd, inst.Rm);
        }

        public static void RevT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitBit.Rev(context, inst.Rd, inst.Rm);
        }

        public static void Rev16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitBit.Rev16(context, inst.Rd, inst.Rm);
        }

        public static void Rev16T1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdb16w3 inst = new(encoding);

            InstEmitBit.Rev16(context, inst.Rd, inst.Rm);
        }

        public static void Rev16T2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitBit.Rev16(context, inst.Rd, inst.Rm);
        }

        public static void RevshA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitBit.Revsh(context, inst.Rd, inst.Rm);
        }

        public static void RevshT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdb16w3 inst = new(encoding);

            InstEmitBit.Revsh(context, inst.Rd, inst.Rm);
        }

        public static void RevshT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitBit.Revsh(context, inst.Rd, inst.Rm);
        }

        public static void RfeA1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void RfeT1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void RfeT2(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void RsbIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.RsbI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm12), inst.S != 0);
        }

        public static void RsbIT1(CodeGenContext context, uint encoding)
        {
            InstRnb19w3Rdb16w3 inst = new(encoding);

            InstEmitAlu.RsbI(context, inst.Rd, inst.Rn, 0, !context.InITBlock);
        }

        public static void RsbIT2(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.RsbI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void RsbRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.RsbR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void RsbRT1(CodeGenContext context, uint encoding)
        {
            InstSb20w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.RsbR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.S != 0);
        }

        public static void RsbRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.RsbRr(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Rs, inst.S != 0);
        }

        public static void RscIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.RscI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm12), inst.S != 0);
        }

        public static void RscRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.RscR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void RscRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.RscRr(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Rs, inst.S != 0);
        }

        public static void Sadd16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Sadd16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Sadd16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Sadd16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Sadd8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Sadd8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Sadd8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Sadd8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void SasxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Sasx(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void SasxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Sasx(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void SbA1(CodeGenContext context, uint encoding)
        {
            throw new NotImplementedException();
        }

        public static void SbT1(CodeGenContext context, uint encoding)
        {
            throw new NotImplementedException();
        }

        public static void SbcIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.SbcI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm12), inst.S != 0);
        }

        public static void SbcIT1(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.SbcI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void SbcRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.SbcR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void SbcRT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdnb16w3 inst = new(encoding);

            InstEmitAlu.SbcR(context, inst.Rdn, inst.Rdn, inst.Rm, 0, 0, !context.InITBlock);
        }

        public static void SbcRT2(CodeGenContext context, uint encoding)
        {
            InstSb20w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.SbcR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.S != 0);
        }

        public static void SbcRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.SbcRr(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Rs, inst.S != 0);
        }

        public static void SbfxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Widthm1b16w5Rdb12w4Lsbb7w5Rnb0w4 inst = new(encoding);

            InstEmitBit.Sbfx(context, inst.Rd, inst.Rn, inst.Lsb, inst.Widthm1);
        }

        public static void SbfxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Imm3b12w3Rdb8w4Imm2b6w2Widthm1b0w5 inst = new(encoding);

            InstEmitBit.Sbfx(context, inst.Rd, inst.Rn, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.Widthm1);
        }

        public static void SdivA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rmb8w4Rnb0w4 inst = new(encoding);

            InstEmitDivide.Sdiv(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void SdivT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitDivide.Sdiv(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void SelA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Sel(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void SelT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Sel(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void SetendA1(CodeGenContext context, uint encoding)
        {
            InstEb9w1 inst = new(encoding);

            InstEmitSystem.Setend(context, inst.E != 0);
        }

        public static void SetendT1(CodeGenContext context, uint encoding)
        {
            InstEb19w1 inst = new(encoding);

            InstEmitSystem.Setend(context, inst.E != 0);
        }

        public static void SetpanA1(CodeGenContext context, uint encoding)
        {
            _ = new InstImm1b9w1(encoding);

            throw new NotImplementedException();
        }

        public static void SetpanT1(CodeGenContext context, uint encoding)
        {
            _ = new InstImm1b19w1(encoding);

            throw new NotImplementedException();
        }

        public static void SevA1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Sev();
        }

        public static void SevT1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Sev();
        }

        public static void SevT2(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Sev();
        }

        public static void SevlA1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Sevl();
        }

        public static void SevlT1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Sevl();
        }

        public static void SevlT2(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Sevl();
        }

        public static void Sha1cA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha1c(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Sha1cT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha1c(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Sha1hA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha1h(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void Sha1hT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha1h(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void Sha1mA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha1m(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Sha1mT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha1m(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Sha1pA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha1p(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Sha1pT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha1p(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Sha1su0A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha1su0(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Sha1su0T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha1su0(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Sha1su1A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha1su1(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void Sha1su1T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha1su1(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void Sha256hA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha256h(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Sha256hT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha256h(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Sha256h2A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha256h2(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Sha256h2T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha256h2(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Sha256su0A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha256su0(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void Sha256su0T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha256su0(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void Sha256su1A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha256su1(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Sha256su1T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonHash.Sha256su1(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void Shadd16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Shadd16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Shadd16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Shadd16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Shadd8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Shadd8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Shadd8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Shadd8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void ShasxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Shasx(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void ShasxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Shasx(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void ShsaxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Shsax(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void ShsaxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Shsax(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Shsub16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Shsub16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Shsub16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Shsub16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Shsub8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Shsub8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Shsub8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Shsub8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void SmcA1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void SmcT1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void SmlabbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rab12w4Rmb8w4Mb6w1Nb5w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smlabb(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra, inst.N != 0, inst.M != 0);
        }

        public static void SmlabbT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rab12w4Rdb8w4Nb5w1Mb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smlabb(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra, inst.N != 0, inst.M != 0);
        }

        public static void SmladA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rab12w4Rmb8w4Mb5w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smlad(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra, inst.M != 0);
        }

        public static void SmladT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rab12w4Rdb8w4Mb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smlad(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra, inst.M != 0);
        }

        public static void SmlalA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdhib16w4Rdlob12w4Rmb8w4Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smlal(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, inst.S != 0);
        }

        public static void SmlalT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdlob12w4Rdhib8w4Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smlal(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, false);
        }

        public static void SmlalbbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdhib16w4Rdlob12w4Rmb8w4Mb6w1Nb5w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smlalbb(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, inst.N != 0, inst.M != 0);
        }

        public static void SmlalbbT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdlob12w4Rdhib8w4Nb5w1Mb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smlalbb(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, inst.N != 0, inst.M != 0);
        }

        public static void SmlaldA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdhib16w4Rdlob12w4Rmb8w4Mb5w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smlald(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, inst.M != 0);
        }

        public static void SmlaldT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdlob12w4Rdhib8w4Mb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smlald(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, inst.M != 0);
        }

        public static void SmlawbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rab12w4Rmb8w4Mb6w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smlawb(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra, inst.M != 0);
        }

        public static void SmlawbT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rab12w4Rdb8w4Mb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smlawb(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra, inst.M != 0);
        }

        public static void SmlsdA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rab12w4Rmb8w4Mb5w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smlsd(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra, inst.M != 0);
        }

        public static void SmlsdT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rab12w4Rdb8w4Mb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smlsd(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra, inst.M != 0);
        }

        public static void SmlsldA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdhib16w4Rdlob12w4Rmb8w4Mb5w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smlsld(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, inst.M != 0);
        }

        public static void SmlsldT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdlob12w4Rdhib8w4Mb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smlsld(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, inst.M != 0);
        }

        public static void SmmlaA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rab12w4Rmb8w4Rb5w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smmla(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra, inst.R != 0);
        }

        public static void SmmlaT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rab12w4Rdb8w4Rb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smmla(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra, inst.R != 0);
        }

        public static void SmmlsA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rab12w4Rmb8w4Rb5w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smmls(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra, inst.R != 0);
        }

        public static void SmmlsT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rab12w4Rdb8w4Rb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smmls(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra, inst.R != 0);
        }

        public static void SmmulA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rmb8w4Rb5w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smmul(context, inst.Rd, inst.Rn, inst.Rm, inst.R != 0);
        }

        public static void SmmulT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smmul(context, inst.Rd, inst.Rn, inst.Rm, inst.R != 0);
        }

        public static void SmuadA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rmb8w4Mb5w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smuad(context, inst.Rd, inst.Rn, inst.Rm, inst.M != 0);
        }

        public static void SmuadT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Mb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smuad(context, inst.Rd, inst.Rn, inst.Rm, inst.M != 0);
        }

        public static void SmulbbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rmb8w4Mb6w1Nb5w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smulbb(context, inst.Rd, inst.Rn, inst.Rm, inst.N != 0, inst.M != 0);
        }

        public static void SmulbbT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Nb5w1Mb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smulbb(context, inst.Rd, inst.Rn, inst.Rm, inst.N != 0, inst.M != 0);
        }

        public static void SmullA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdhib16w4Rdlob12w4Rmb8w4Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smull(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, inst.S != 0);
        }

        public static void SmullT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdlob12w4Rdhib8w4Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smull(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, false);
        }

        public static void SmulwbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rmb8w4Mb6w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smulwb(context, inst.Rd, inst.Rn, inst.Rm, inst.M != 0);
        }

        public static void SmulwbT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Mb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smulwb(context, inst.Rd, inst.Rn, inst.Rm, inst.M != 0);
        }

        public static void SmusdA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rmb8w4Mb5w1Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Smusd(context, inst.Rd, inst.Rn, inst.Rm, inst.M != 0);
        }

        public static void SmusdT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Mb4w1Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Smusd(context, inst.Rd, inst.Rn, inst.Rm, inst.M != 0);
        }

        public static void SrsA1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void SrsT1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void SrsT2(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void SsatA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4SatImmb16w5Rdb12w4Imm5b7w5Shb6w1Rnb0w4 inst = new(encoding);

            InstEmitSaturate.Ssat(context, inst.Rd, inst.SatImm, inst.Rn, inst.Sh != 0, inst.Imm5);
        }

        public static void SsatT1(CodeGenContext context, uint encoding)
        {
            InstShb21w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2SatImmb0w5 inst = new(encoding);

            InstEmitSaturate.Ssat(context, inst.Rd, inst.SatImm, inst.Rn, inst.Sh != 0, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3));
        }

        public static void Ssat16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4SatImmb16w4Rdb12w4Rnb0w4 inst = new(encoding);

            InstEmitSaturate.Ssat16(context, inst.Rd, inst.SatImm, inst.Rn);
        }

        public static void Ssat16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4SatImmb0w4 inst = new(encoding);

            InstEmitSaturate.Ssat16(context, inst.Rd, inst.SatImm, inst.Rn);
        }

        public static void SsaxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Ssax(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void SsaxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Ssax(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void SsbbA1(CodeGenContext context, uint encoding)
        {
            throw new NotImplementedException();
        }

        public static void SsbbT1(CodeGenContext context, uint encoding)
        {
            throw new NotImplementedException();
        }

        public static void Ssub16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Ssub16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Ssub16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Ssub16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Ssub8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Ssub8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Ssub8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Ssub8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void StcA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.Stc(context, inst.Rn, (int)inst.Imm8 << 2, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StcT1(CodeGenContext context, uint encoding)
        {
            InstPb24w1Ub23w1Wb21w1Rnb16w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.Stc(context, inst.Rn, (int)inst.Imm8 << 2, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StlA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb0w4 inst = new(encoding);

            InstEmitMemory.Stl(context, inst.Rt, inst.Rn);
        }

        public static void StlT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Stl(context, inst.Rt, inst.Rn);
        }

        public static void StlbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb0w4 inst = new(encoding);

            InstEmitMemory.Stlb(context, inst.Rt, inst.Rn);
        }

        public static void StlbT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Stlb(context, inst.Rt, inst.Rn);
        }

        public static void StlexA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rtb0w4 inst = new(encoding);

            InstEmitMemory.Stlex(context, inst.Rd, inst.Rt, inst.Rn);
        }

        public static void StlexT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Rdb0w4 inst = new(encoding);

            InstEmitMemory.Stlex(context, inst.Rd, inst.Rt, inst.Rn);
        }

        public static void StlexbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rtb0w4 inst = new(encoding);

            InstEmitMemory.Stlexb(context, inst.Rd, inst.Rt, inst.Rn);
        }

        public static void StlexbT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Rdb0w4 inst = new(encoding);

            InstEmitMemory.Stlexb(context, inst.Rd, inst.Rt, inst.Rn);
        }

        public static void StlexdA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rtb0w4 inst = new(encoding);

            InstEmitMemory.Stlexd(context, inst.Rd, inst.Rt, RegisterUtils.GetRt2(inst.Rt), inst.Rn);
        }

        public static void StlexdT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Rt2b8w4Rdb0w4 inst = new(encoding);

            InstEmitMemory.Stlexd(context, inst.Rd, inst.Rt, inst.Rt2, inst.Rn);
        }

        public static void StlexhA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rtb0w4 inst = new(encoding);

            InstEmitMemory.Stlexh(context, inst.Rd, inst.Rt, inst.Rn);
        }

        public static void StlexhT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Rdb0w4 inst = new(encoding);

            InstEmitMemory.Stlexh(context, inst.Rd, inst.Rt, inst.Rn);
        }

        public static void StlhA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rtb0w4 inst = new(encoding);

            InstEmitMemory.Stlh(context, inst.Rt, inst.Rn);
        }

        public static void StlhT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4 inst = new(encoding);

            InstEmitMemory.Stlh(context, inst.Rt, inst.Rn);
        }

        public static void StmA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Wb21w1Rnb16w4RegisterListb0w16 inst = new(encoding);

            InstEmitMemory.Stm(context, inst.Rn, inst.RegisterList, inst.W != 0);
        }

        public static void StmT1(CodeGenContext context, uint encoding)
        {
            InstRnb24w3RegisterListb16w8 inst = new(encoding);

            InstEmitMemory.Stm(context, inst.Rn, inst.RegisterList, false);
        }

        public static void StmT2(CodeGenContext context, uint encoding)
        {
            InstWb21w1Rnb16w4Mb14w1RegisterListb0w14 inst = new(encoding);

            InstEmitMemory.Stm(context, inst.Rn, ImmUtils.CombineRegisterList(inst.RegisterList, inst.M), inst.W != 0);
        }

        public static void StmdaA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Wb21w1Rnb16w4RegisterListb0w16 inst = new(encoding);

            InstEmitMemory.Stmda(context, inst.Rn, inst.RegisterList, inst.W != 0);
        }

        public static void StmdbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Wb21w1Rnb16w4RegisterListb0w16 inst = new(encoding);

            InstEmitMemory.Stmdb(context, inst.Rn, inst.RegisterList, inst.W != 0);
        }

        public static void StmdbT1(CodeGenContext context, uint encoding)
        {
            InstWb21w1Rnb16w4Mb14w1RegisterListb0w14 inst = new(encoding);

            InstEmitMemory.Stmdb(context, inst.Rn, ImmUtils.CombineRegisterList(inst.RegisterList, inst.M), inst.W != 0);
        }

        public static void StmibA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Wb21w1Rnb16w4RegisterListb0w16 inst = new(encoding);

            InstEmitMemory.Stmib(context, inst.Rn, inst.RegisterList, inst.W != 0);
        }

        public static void StmUA1(CodeGenContext context, uint encoding)
        {
            InstEmitSystem.PrivilegedInstruction(context, encoding);
        }

        public static void StrbtA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.StrbtI(context, inst.Rt, inst.Rn, (int)inst.Imm12, postIndex: true, inst.U != 0);
        }

        public static void StrbtA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.StrbtR(context, inst.Rt, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, postIndex: true, inst.U != 0);
        }

        public static void StrbtT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.StrbtI(context, inst.Rt, inst.Rn, (int)inst.Imm8, postIndex: false, true);
        }

        public static void StrbIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.StrbI(context, inst.Rt, inst.Rn, (int)inst.Imm12, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StrbIT1(CodeGenContext context, uint encoding)
        {
            InstImm5b22w5Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.StrbI(context, inst.Rt, inst.Rn, (int)inst.Imm5, true, true, false);
        }

        public static void StrbIT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.StrbI(context, inst.Rt, inst.Rn, (int)inst.Imm12, true, true, false);
        }

        public static void StrbIT3(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Pb10w1Ub9w1Wb8w1Imm8b0w8 inst = new(encoding);

            InstEmitMemory.StrbI(context, inst.Rt, inst.Rn, (int)inst.Imm8, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StrbRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.StrbR(context, inst.Rt, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StrbRT1(CodeGenContext context, uint encoding)
        {
            InstRmb22w3Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.StrbR(context, inst.Rt, inst.Rn, inst.Rm, 0, 0, true, true, false);
        }

        public static void StrbRT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm2b4w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.StrbR(context, inst.Rt, inst.Rn, inst.Rm, 0, inst.Imm2, true, true, false);
        }

        public static void StrdIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.StrdI(context, inst.Rt, RegisterUtils.GetRt2(inst.Rt), inst.Rn, ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StrdIT1(CodeGenContext context, uint encoding)
        {
            InstPb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Rt2b8w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.StrdI(context, inst.Rt, inst.Rt2, inst.Rn, inst.Imm8 << 2, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StrdRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Rmb0w4 inst = new(encoding);

            InstEmitMemory.StrdR(context, inst.Rt, RegisterUtils.GetRt2(inst.Rt), inst.Rn, inst.Rm, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StrexA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rtb0w4 inst = new(encoding);

            InstEmitMemory.Strex(context, inst.Rd, inst.Rt, inst.Rn);
        }

        public static void StrexT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.Strex(context, inst.Rd, inst.Rt, inst.Rn);
        }

        public static void StrexbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rtb0w4 inst = new(encoding);

            InstEmitMemory.Strexb(context, inst.Rd, inst.Rt, inst.Rn);
        }

        public static void StrexbT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Rdb0w4 inst = new(encoding);

            InstEmitMemory.Strexb(context, inst.Rd, inst.Rt, inst.Rn);
        }

        public static void StrexdA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rtb0w4 inst = new(encoding);

            InstEmitMemory.Strexd(context, inst.Rd, inst.Rt, RegisterUtils.GetRt2(inst.Rt), inst.Rn);
        }

        public static void StrexdT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Rt2b8w4Rdb0w4 inst = new(encoding);

            InstEmitMemory.Strexd(context, inst.Rd, inst.Rt, inst.Rt2, inst.Rn);
        }

        public static void StrexhA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rtb0w4 inst = new(encoding);

            InstEmitMemory.Strexh(context, inst.Rd, inst.Rt, inst.Rn);
        }

        public static void StrexhT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Rdb0w4 inst = new(encoding);

            InstEmitMemory.Strexh(context, inst.Rd, inst.Rt, inst.Rn);
        }

        public static void StrhtA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.StrhtI(context, inst.Rt, inst.Rn, (int)ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), postIndex: true, inst.U != 0);
        }

        public static void StrhtA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Rmb0w4 inst = new(encoding);

            InstEmitMemory.StrhtR(context, inst.Rt, inst.Rn, inst.Rm, postIndex: true, inst.U != 0);
        }

        public static void StrhtT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.StrhtI(context, inst.Rt, inst.Rn, (int)inst.Imm8, postIndex: false, true);
        }

        public static void StrhIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm4hb8w4Imm4lb0w4 inst = new(encoding);

            InstEmitMemory.StrhI(context, inst.Rt, inst.Rn, (int)ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StrhIT1(CodeGenContext context, uint encoding)
        {
            InstImm5b22w5Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.StrhI(context, inst.Rt, inst.Rn, (int)inst.Imm5 << 1, true, true, false);
        }

        public static void StrhIT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.StrhI(context, inst.Rt, inst.Rn, (int)inst.Imm12, true, true, false);
        }

        public static void StrhIT3(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Pb10w1Ub9w1Wb8w1Imm8b0w8 inst = new(encoding);

            InstEmitMemory.StrhI(context, inst.Rt, inst.Rn, (int)inst.Imm8, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StrhRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Rmb0w4 inst = new(encoding);

            InstEmitMemory.StrhR(context, inst.Rt, inst.Rn, inst.Rm, 0, 0, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StrhRT1(CodeGenContext context, uint encoding)
        {
            InstRmb22w3Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.StrhR(context, inst.Rt, inst.Rn, inst.Rm, 0, 0, true, true, false);
        }

        public static void StrhRT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm2b4w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.StrhR(context, inst.Rt, inst.Rn, inst.Rm, 0, inst.Imm2, true, true, false);
        }

        public static void StrtA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.StrtI(context, inst.Rt, inst.Rn, (int)inst.Imm12, postIndex: true, inst.U != 0);
        }

        public static void StrtA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Rnb16w4Rtb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.StrtR(context, inst.Rt, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, postIndex: true, inst.U != 0);
        }

        public static void StrtT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm8b0w8 inst = new(encoding);

            InstEmitMemory.StrtI(context, inst.Rt, inst.Rn, (int)inst.Imm8, postIndex: false, true);
        }

        public static void StrIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.StrI(context, inst.Rt, inst.Rn, (int)inst.Imm12, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StrIT1(CodeGenContext context, uint encoding)
        {
            InstImm5b22w5Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.StrI(context, inst.Rt, inst.Rn, (int)inst.Imm5 << 2, true, true, false);
        }

        public static void StrIT2(CodeGenContext context, uint encoding)
        {
            InstRtb24w3Imm8b16w8 inst = new(encoding);

            InstEmitMemory.StrI(context, inst.Rt, RegisterUtils.SpRegister, (int)inst.Imm8 << 2, true, true, false);
        }

        public static void StrIT3(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm12b0w12 inst = new(encoding);

            InstEmitMemory.StrI(context, inst.Rt, inst.Rn, (int)inst.Imm12, true, true, false);
        }

        public static void StrIT4(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Pb10w1Ub9w1Wb8w1Imm8b0w8 inst = new(encoding);

            InstEmitMemory.StrI(context, inst.Rt, inst.Rn, (int)inst.Imm8, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StrRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Wb21w1Rnb16w4Rtb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.StrR(context, inst.Rt, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.P != 0, inst.U != 0, inst.W != 0);
        }

        public static void StrRT1(CodeGenContext context, uint encoding)
        {
            InstRmb22w3Rnb19w3Rtb16w3 inst = new(encoding);

            InstEmitMemory.StrR(context, inst.Rt, inst.Rn, inst.Rm, 0, 0, true, true, false);
        }

        public static void StrRT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rtb12w4Imm2b4w2Rmb0w4 inst = new(encoding);

            InstEmitMemory.StrR(context, inst.Rt, inst.Rn, inst.Rm, 0, inst.Imm2, true, true, false);
        }

        public static void SubIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.SubI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm12), inst.S != 0);
        }

        public static void SubIT1(CodeGenContext context, uint encoding)
        {
            InstImm3b22w3Rnb19w3Rdb16w3 inst = new(encoding);

            InstEmitAlu.SubI(context, inst.Rd, inst.Rn, inst.Imm3, !context.InITBlock);
        }

        public static void SubIT2(CodeGenContext context, uint encoding)
        {
            InstRdnb24w3Imm8b16w8 inst = new(encoding);

            InstEmitAlu.SubI(context, inst.Rdn, inst.Rdn, inst.Imm8, !context.InITBlock);
        }

        public static void SubIT3(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.SubI(context, inst.Rd, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void SubIT4(CodeGenContext context, uint encoding)
        {
            InstIb26w1Rnb16w4Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.SubI(context, inst.Rd, inst.Rn, ImmUtils.CombineImmU12(inst.Imm8, inst.Imm3, inst.I), false);
        }

        public static void SubIT5(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.SubI(context, RegisterUtils.PcRegister, inst.Rn, inst.Imm8, true);
        }

        public static void SubRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.SubR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void SubRT1(CodeGenContext context, uint encoding)
        {
            InstRmb22w3Rnb19w3Rdb16w3 inst = new(encoding);

            InstEmitAlu.SubR(context, inst.Rd, inst.Rn, inst.Rm, 0, 0, !context.InITBlock);
        }

        public static void SubRT2(CodeGenContext context, uint encoding)
        {
            InstSb20w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.SubR(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), !context.InITBlock);
        }

        public static void SubRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rnb16w4Rdb12w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.SubRr(context, inst.Rd, inst.Rn, inst.Rm, inst.Stype, inst.Rs, inst.S != 0);
        }

        public static void SubSpIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdb12w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.SubI(context, inst.Rd, RegisterUtils.SpRegister, ImmUtils.ExpandImm(inst.Imm12), inst.S != 0);
        }

        public static void SubSpIT1(CodeGenContext context, uint encoding)
        {
            InstImm7b16w7 inst = new(encoding);

            InstEmitAlu.SubI(context, RegisterUtils.SpRegister, RegisterUtils.SpRegister, inst.Imm7 << 2, false);
        }

        public static void SubSpIT2(CodeGenContext context, uint encoding)
        {
            InstIb26w1Sb20w1Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.SubI(context, inst.Rd, RegisterUtils.SpRegister, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), inst.S != 0);
        }

        public static void SubSpIT3(CodeGenContext context, uint encoding)
        {
            InstIb26w1Imm3b12w3Rdb8w4Imm8b0w8 inst = new(encoding);

            InstEmitAlu.SubI(context, inst.Rd, RegisterUtils.SpRegister, ImmUtils.CombineImmU12(inst.Imm8, inst.Imm3, inst.I), false);
        }

        public static void SubSpRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdb12w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.SubR(context, inst.Rd, RegisterUtils.SpRegister, inst.Rm, inst.Stype, inst.Imm5, inst.S != 0);
        }

        public static void SubSpRT1(CodeGenContext context, uint encoding)
        {
            InstSb20w1Imm3b12w3Rdb8w4Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.SubR(context, inst.Rd, RegisterUtils.SpRegister, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.S != 0);
        }

        public static void SvcA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Imm24b0w24 inst = new(encoding);

            InstEmitSystem.Svc(context, inst.Imm24);
        }

        public static void SvcT1(CodeGenContext context, uint encoding)
        {
            InstImm8b16w8 inst = new(encoding);

            InstEmitSystem.Svc(context, inst.Imm8);
        }

        public static void SxtabA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rotateb10w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Sxtab(context, inst.Rd, inst.Rn, inst.Rm, inst.Rotate);
        }

        public static void SxtabT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rotateb4w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Sxtab(context, inst.Rd, inst.Rn, inst.Rm, inst.Rotate);
        }

        public static void Sxtab16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rotateb10w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Sxtab16(context, inst.Rd, inst.Rn, inst.Rm, inst.Rotate);
        }

        public static void Sxtab16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rotateb4w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Sxtab16(context, inst.Rd, inst.Rn, inst.Rm, inst.Rotate);
        }

        public static void SxtahA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rotateb10w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Sxtah(context, inst.Rd, inst.Rn, inst.Rm, inst.Rotate);
        }

        public static void SxtahT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rotateb4w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Sxtah(context, inst.Rd, inst.Rn, inst.Rm, inst.Rotate);
        }

        public static void SxtbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb12w4Rotateb10w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Sxtb(context, inst.Rd, inst.Rm, inst.Rotate);
        }

        public static void SxtbT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdb16w3 inst = new(encoding);

            InstEmitExtension.Sxtb(context, inst.Rd, inst.Rm, 0);
        }

        public static void SxtbT2(CodeGenContext context, uint encoding)
        {
            InstRdb8w4Rotateb4w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Sxtb(context, inst.Rd, inst.Rm, inst.Rotate);
        }

        public static void Sxtb16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb12w4Rotateb10w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Sxtb16(context, inst.Rd, inst.Rm, inst.Rotate);
        }

        public static void Sxtb16T1(CodeGenContext context, uint encoding)
        {
            InstRdb8w4Rotateb4w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Sxtb16(context, inst.Rd, inst.Rm, inst.Rotate);
        }

        public static void SxthA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb12w4Rotateb10w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Sxth(context, inst.Rd, inst.Rm, inst.Rotate);
        }

        public static void SxthT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdb16w3 inst = new(encoding);

            InstEmitExtension.Sxth(context, inst.Rd, inst.Rm, 0);
        }

        public static void SxthT2(CodeGenContext context, uint encoding)
        {
            InstRdb8w4Rotateb4w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Sxth(context, inst.Rd, inst.Rm, inst.Rotate);
        }

        public static void TbbT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Hb4w1Rmb0w4 inst = new(encoding);

            InstEmitFlow.Tbb(context, inst.Rn, inst.Rm, inst.H != 0);
        }

        public static void TeqIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.TeqI(context, inst.Rn, ImmUtils.ExpandImm(inst.Imm12), ImmUtils.ExpandedImmRotated(inst.Imm12));
        }

        public static void TeqIT1(CodeGenContext context, uint encoding)
        {
            InstIb26w1Rnb16w4Imm3b12w3Imm8b0w8 inst = new(encoding);

            InstEmitAlu.TeqI(context, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), ImmUtils.ExpandedImmRotated(inst.Imm8, inst.Imm3, inst.I));
        }

        public static void TeqRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.TeqR(context, inst.Rn, inst.Rm, inst.Stype, inst.Imm5);
        }

        public static void TeqRT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Imm3b12w3Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.TeqR(context, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3));
        }

        public static void TeqRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.TeqRr(context, inst.Rn, inst.Rm, inst.Stype, inst.Rs);
        }

        public static void TsbA1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Tsb();
        }

        public static void TsbT1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Tsb();
        }

        public static void TstIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Imm12b0w12 inst = new(encoding);

            InstEmitAlu.TstI(context, inst.Rn, ImmUtils.ExpandImm(inst.Imm12), ImmUtils.ExpandedImmRotated(inst.Imm12));
        }

        public static void TstIT1(CodeGenContext context, uint encoding)
        {
            InstIb26w1Rnb16w4Imm3b12w3Imm8b0w8 inst = new(encoding);

            InstEmitAlu.TstI(context, inst.Rn, ImmUtils.ExpandImm(inst.Imm8, inst.Imm3, inst.I), ImmUtils.ExpandedImmRotated(inst.Imm8, inst.Imm3, inst.I));
        }

        public static void TstRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Imm5b7w5Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.TstR(context, inst.Rn, inst.Rm, inst.Stype, inst.Imm5);
        }

        public static void TstRT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rnb16w3 inst = new(encoding);

            InstEmitAlu.TstR(context, inst.Rn, inst.Rm, 0, 0);
        }

        public static void TstRT2(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Imm3b12w3Imm2b6w2Stypeb4w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.TstR(context, inst.Rn, inst.Rm, inst.Stype, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3));
        }

        public static void TstRrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rsb8w4Stypeb5w2Rmb0w4 inst = new(encoding);

            InstEmitAlu.TstRr(context, inst.Rn, inst.Rm, inst.Stype, inst.Rs);
        }

        public static void Uadd16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Uadd16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uadd16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Uadd16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uadd8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Uadd8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uadd8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Uadd8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UasxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Uasx(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UasxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Uasx(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UbfxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Widthm1b16w5Rdb12w4Lsbb7w5Rnb0w4 inst = new(encoding);

            InstEmitBit.Ubfx(context, inst.Rd, inst.Rn, inst.Lsb, inst.Widthm1);
        }

        public static void UbfxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Imm3b12w3Rdb8w4Imm2b6w2Widthm1b0w5 inst = new(encoding);

            InstEmitBit.Ubfx(context, inst.Rd, inst.Rn, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3), inst.Widthm1);
        }

        public static void UdfA1(CodeGenContext context, uint encoding)
        {
            InstImm12b8w12Imm4b0w4 inst = new(encoding);

            InstEmitSystem.Udf(context, encoding, ImmUtils.CombineImmU16(inst.Imm12, inst.Imm4));
        }

        public static void UdfT1(CodeGenContext context, uint encoding)
        {
            InstImm8b16w8 inst = new(encoding);

            InstEmitSystem.Udf(context, encoding, inst.Imm8);
        }

        public static void UdfT2(CodeGenContext context, uint encoding)
        {
            InstImm4b16w4Imm12b0w12 inst = new(encoding);

            InstEmitSystem.Udf(context, encoding, ImmUtils.CombineImmU16(inst.Imm12, inst.Imm4));
        }

        public static void UdivA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rmb8w4Rnb0w4 inst = new(encoding);

            InstEmitDivide.Udiv(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UdivT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitDivide.Udiv(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uhadd16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Uhadd16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uhadd16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Uhadd16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uhadd8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Uhadd8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uhadd8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Uhadd8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UhasxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Uhasx(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UhasxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Uhasx(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UhsaxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Uhsax(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UhsaxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Uhsax(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uhsub16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Uhsub16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uhsub16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Uhsub16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uhsub8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Uhsub8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uhsub8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitHalve.Uhsub8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UmaalA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdhib16w4Rdlob12w4Rmb8w4Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Umaal(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm);
        }

        public static void UmaalT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdlob12w4Rdhib8w4Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Umaal(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm);
        }

        public static void UmlalA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdhib16w4Rdlob12w4Rmb8w4Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Umlal(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, inst.S != 0);
        }

        public static void UmlalT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdlob12w4Rdhib8w4Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Umlal(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, false);
        }

        public static void UmullA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Sb20w1Rdhib16w4Rdlob12w4Rmb8w4Rnb0w4 inst = new(encoding);

            InstEmitMultiply.Umull(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, inst.S != 0);
        }

        public static void UmullT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdlob12w4Rdhib8w4Rmb0w4 inst = new(encoding);

            InstEmitMultiply.Umull(context, inst.Rdlo, inst.Rdhi, inst.Rn, inst.Rm, false);
        }

        public static void Uqadd16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Uqadd16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uqadd16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Uqadd16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uqadd8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Uqadd8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uqadd8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Uqadd8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UqasxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Uqasx(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UqasxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Uqasx(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UqsaxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Uqsax(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UqsaxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Uqsax(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uqsub16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Uqsub16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uqsub16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Uqsub16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uqsub8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Uqsub8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Uqsub8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitSaturate.Uqsub8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Usad8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rmb8w4Rnb0w4 inst = new(encoding);

            InstEmitAbsDiff.Usad8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Usad8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitAbsDiff.Usad8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Usada8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb16w4Rab12w4Rmb8w4Rnb0w4 inst = new(encoding);

            InstEmitAbsDiff.Usada8(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra);
        }

        public static void Usada8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rab12w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitAbsDiff.Usada8(context, inst.Rd, inst.Rn, inst.Rm, inst.Ra);
        }

        public static void UsatA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4SatImmb16w5Rdb12w4Imm5b7w5Shb6w1Rnb0w4 inst = new(encoding);

            InstEmitSaturate.Usat(context, inst.Rd, inst.SatImm, inst.Rn, inst.Sh != 0, inst.Imm5);
        }

        public static void UsatT1(CodeGenContext context, uint encoding)
        {
            InstShb21w1Rnb16w4Imm3b12w3Rdb8w4Imm2b6w2SatImmb0w5 inst = new(encoding);

            InstEmitSaturate.Usat(context, inst.Rd, inst.SatImm, inst.Rn, inst.Sh != 0, ImmUtils.CombineImmU5(inst.Imm2, inst.Imm3));
        }

        public static void Usat16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4SatImmb16w4Rdb12w4Rnb0w4 inst = new(encoding);

            InstEmitSaturate.Usat16(context, inst.Rd, inst.SatImm, inst.Rn);
        }

        public static void Usat16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4SatImmb0w4 inst = new(encoding);

            InstEmitSaturate.Usat16(context, inst.Rd, inst.SatImm, inst.Rn);
        }

        public static void UsaxA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Usax(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UsaxT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Usax(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Usub16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Usub16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Usub16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Usub16(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Usub8A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Usub8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void Usub8T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rmb0w4 inst = new(encoding);

            InstEmitGE.Usub8(context, inst.Rd, inst.Rn, inst.Rm);
        }

        public static void UxtabA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rotateb10w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Uxtab(context, inst.Rd, inst.Rn, inst.Rm, inst.Rotate);
        }

        public static void UxtabT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rotateb4w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Uxtab(context, inst.Rd, inst.Rn, inst.Rm, inst.Rotate);
        }

        public static void Uxtab16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rotateb10w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Uxtab16(context, inst.Rd, inst.Rn, inst.Rm, inst.Rotate);
        }

        public static void Uxtab16T1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rotateb4w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Uxtab16(context, inst.Rd, inst.Rn, inst.Rm, inst.Rotate);
        }

        public static void UxtahA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rnb16w4Rdb12w4Rotateb10w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Uxtah(context, inst.Rd, inst.Rn, inst.Rm, inst.Rotate);
        }

        public static void UxtahT1(CodeGenContext context, uint encoding)
        {
            InstRnb16w4Rdb8w4Rotateb4w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Uxtah(context, inst.Rd, inst.Rn, inst.Rm, inst.Rotate);
        }

        public static void UxtbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb12w4Rotateb10w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Uxtb(context, inst.Rd, inst.Rm, inst.Rotate);
        }

        public static void UxtbT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdb16w3 inst = new(encoding);

            InstEmitExtension.Uxtb(context, inst.Rd, inst.Rm, 0);
        }

        public static void UxtbT2(CodeGenContext context, uint encoding)
        {
            InstRdb8w4Rotateb4w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Uxtb(context, inst.Rd, inst.Rm, inst.Rotate);
        }

        public static void Uxtb16A1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb12w4Rotateb10w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Uxtb16(context, inst.Rd, inst.Rm, inst.Rotate);
        }

        public static void Uxtb16T1(CodeGenContext context, uint encoding)
        {
            InstRdb8w4Rotateb4w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Uxtb16(context, inst.Rd, inst.Rm, inst.Rotate);
        }

        public static void UxthA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Rdb12w4Rotateb10w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Uxth(context, inst.Rd, inst.Rm, inst.Rotate);
        }

        public static void UxthT1(CodeGenContext context, uint encoding)
        {
            InstRmb19w3Rdb16w3 inst = new(encoding);

            InstEmitExtension.Uxth(context, inst.Rd, inst.Rm, 0);
        }

        public static void UxthT2(CodeGenContext context, uint encoding)
        {
            InstRdb8w4Rotateb4w2Rmb0w4 inst = new(encoding);

            InstEmitExtension.Uxth(context, inst.Rd, inst.Rm, inst.Rotate);
        }

        public static void VabaA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vaba(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VabaT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vaba(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VabalA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vabal(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VabalT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vabal(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VabdlIA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vabdl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VabdlIT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vabdl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VabdFA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VabdF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VabdFT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VabdF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VabdIA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VabdI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VabdIT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VabdI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VabsA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vabs(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VabsA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VabsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VabsT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vabs(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VabsT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VabsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VacgeA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.Vacge(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VacgeT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.Vacge(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VacgtA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.Vacgt(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VacgtT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.Vacgt(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VaddhnA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vaddhn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VaddhnT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vaddhn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VaddlA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vaddl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VaddlT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vaddl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VaddwA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vaddw(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VaddwT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vaddw(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VaddFA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VaddF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VaddFA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VaddF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VaddFT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VaddF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VaddFT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VaddF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VaddIA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VaddI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VaddIT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VaddI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VandRA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VandR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VandRT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VandR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VbicIA1(CodeGenContext context, uint encoding)
        {
            InstIb24w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonLogical.VbicI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VbicIA2(CodeGenContext context, uint encoding)
        {
            InstIb24w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonLogical.VbicI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VbicIT1(CodeGenContext context, uint encoding)
        {
            InstIb28w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonLogical.VbicI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VbicIT2(CodeGenContext context, uint encoding)
        {
            InstIb28w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonLogical.VbicI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VbicRA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VbicR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VbicRT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VbicR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VbifA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VbifR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VbifT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VbifR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VbitA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VbitR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VbitT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VbitR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VbslA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VbslR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VbslT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VbslR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VcaddA1(CodeGenContext context, uint encoding)
        {
            _ = new InstRotb24w1Db22w1Sb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VcaddT1(CodeGenContext context, uint encoding)
        {
            _ = new InstRotb24w1Db22w1Sb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VceqIA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VceqI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VceqIT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VceqI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VceqRA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VceqR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VceqRA2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VceqFR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VceqRT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VceqR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VceqRT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VceqFR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VcgeIA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcgeI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VcgeIT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcgeI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VcgeRA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcgeR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VcgeRA2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcgeFR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VcgeRT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcgeR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VcgeRT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcgeFR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VcgtIA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcgtI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VcgtIT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcgtI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VcgtRA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcgtR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VcgtRA2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcgtFR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VcgtRT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcgtR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VcgtRT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcgtFR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VcleIA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcleI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VcleIT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcleI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VclsA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonBit.Vcls(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VclsT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonBit.Vcls(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VcltIA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcltI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VcltIT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.VcltI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VclzA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonBit.Vclz(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VclzT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonBit.Vclz(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VcmlaA1(CodeGenContext context, uint encoding)
        {
            _ = new InstRotb23w2Db22w1Sb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VcmlaT1(CodeGenContext context, uint encoding)
        {
            _ = new InstRotb23w2Db22w1Sb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VcmlaSA1(CodeGenContext context, uint encoding)
        {
            _ = new InstSb23w1Db22w1Rotb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VcmlaST1(CodeGenContext context, uint encoding)
        {
            _ = new InstSb23w1Db22w1Rotb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VcmpA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpCompare.VcmpR(context, inst.Cond, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VcmpA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Sizeb8w2 inst = new(encoding);

            InstEmitVfpCompare.VcmpI(context, inst.Cond, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), inst.Size);
        }

        public static void VcmpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpCompare.VcmpR(context, 0xe, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VcmpT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2 inst = new(encoding);

            InstEmitVfpCompare.VcmpI(context, 0xe, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), inst.Size);
        }

        public static void VcmpeA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpCompare.VcmpeR(context, inst.Cond, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VcmpeA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Sizeb8w2 inst = new(encoding);

            InstEmitVfpCompare.VcmpeI(context, inst.Cond, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), inst.Size);
        }

        public static void VcmpeT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpCompare.VcmpeR(context, 0xe, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VcmpeT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2 inst = new(encoding);

            InstEmitVfpCompare.VcmpeI(context, 0xe, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), inst.Size);
        }

        public static void VcntA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonBit.Vcnt(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VcntT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonBit.Vcnt(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VcvtaAsimdA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.Vcvta(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VcvtaAsimdT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.Vcvta(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VcvtaVfpA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.Vcvta(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Op != 0, inst.Size);
        }

        public static void VcvtaVfpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.Vcvta(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Op != 0, inst.Size);
        }

        public static void VcvtbA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Opb16w1Vdb12w4Szb8w1Mb5w1Vmb0w4 inst = new(encoding);

            uint dSize = inst.Sz == 1 && inst.Op == 0 ? 3u : 2u;
            uint mSize = inst.Sz == 1 && inst.Op == 1 ? 3u : 2u;

            InstEmitVfpConvert.Vcvtb(context, InstEmitCommon.CombineV(inst.Vd, inst.D, dSize), InstEmitCommon.CombineV(inst.Vm, inst.M, mSize), inst.Sz, inst.Op);
        }

        public static void VcvtbT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Opb16w1Vdb12w4Szb8w1Mb5w1Vmb0w4 inst = new(encoding);

            uint dSize = inst.Sz == 1 && inst.Op == 0 ? 3u : 2u;
            uint mSize = inst.Sz == 1 && inst.Op == 1 ? 3u : 2u;

            InstEmitVfpConvert.Vcvtb(context, InstEmitCommon.CombineV(inst.Vd, inst.D, dSize), InstEmitCommon.CombineV(inst.Vm, inst.M, mSize), inst.Sz, inst.Op);
        }

        public static void VcvtbBfsA1(CodeGenContext context, uint encoding)
        {
            _ = new InstCondb28w4Db22w1Vdb12w4Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VcvtbBfsT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vdb12w4Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VcvtmAsimdA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.Vcvtm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VcvtmAsimdT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.Vcvtm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VcvtmVfpA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.Vcvtm(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Op != 0, inst.Size);
        }

        public static void VcvtmVfpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.Vcvtm(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Op != 0, inst.Size);
        }

        public static void VcvtnAsimdA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.Vcvtn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VcvtnAsimdT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.Vcvtn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VcvtnVfpA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.Vcvtn(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Op != 0, inst.Size);
        }

        public static void VcvtnVfpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.Vcvtn(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Op != 0, inst.Size);
        }

        public static void VcvtpAsimdA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.Vcvtp(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VcvtpAsimdT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.Vcvtp(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VcvtpVfpA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.Vcvtp(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Op != 0, inst.Size);
        }

        public static void VcvtpVfpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.Vcvtp(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Op != 0, inst.Size);
        }

        public static void VcvtrIvA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.VcvtrIv(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), (encoding >> 16) & 7, inst.Size);
        }

        public static void VcvtrIvT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.VcvtrIv(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), (encoding >> 16) & 7, inst.Size);
        }

        public static void VcvttA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Opb16w1Vdb12w4Szb8w1Mb5w1Vmb0w4 inst = new(encoding);

            uint dSize = inst.Sz == 1 && inst.Op == 0 ? 3u : 2u;
            uint mSize = inst.Sz == 1 && inst.Op == 1 ? 3u : 2u;

            InstEmitVfpConvert.Vcvtt(context, InstEmitCommon.CombineV(inst.Vd, inst.D, dSize), InstEmitCommon.CombineV(inst.Vm, inst.M, mSize), inst.Sz, inst.Op);
        }

        public static void VcvttT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Opb16w1Vdb12w4Szb8w1Mb5w1Vmb0w4 inst = new(encoding);

            uint dSize = inst.Sz == 1 && inst.Op == 0 ? 3u : 2u;
            uint mSize = inst.Sz == 1 && inst.Op == 1 ? 3u : 2u;

            InstEmitVfpConvert.Vcvtt(context, InstEmitCommon.CombineV(inst.Vd, inst.D, dSize), InstEmitCommon.CombineV(inst.Vm, inst.M, mSize), inst.Sz, inst.Op);
        }

        public static void VcvttBfsA1(CodeGenContext context, uint encoding)
        {
            _ = new InstCondb28w4Db22w1Vdb12w4Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VcvttBfsT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vdb12w4Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VcvtBfsA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vdb12w4Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VcvtBfsT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vdb12w4Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VcvtDsA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            uint size = (encoding >> 8) & 3;

            InstEmitVfpConvert.VcvtDs(context, InstEmitCommon.CombineV(inst.Vd, inst.D, size ^ 1u), InstEmitCommon.CombineV(inst.Vm, inst.M, size), size);
        }

        public static void VcvtDsT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            uint size = (encoding >> 8) & 3;

            InstEmitVfpConvert.VcvtDs(context, InstEmitCommon.CombineV(inst.Vd, inst.D, size ^ 1u), InstEmitCommon.CombineV(inst.Vm, inst.M, size), size);
        }

        public static void VcvtHsA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb8w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.VcvtHs(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0);
        }

        public static void VcvtHsT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb8w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.VcvtHs(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0);
        }

        public static void VcvtIsA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w2Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.VcvtIs(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op, inst.Size, inst.Q);
        }

        public static void VcvtIsT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w2Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.VcvtIs(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op, inst.Size, inst.Q);
        }

        public static void VcvtIvA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.VcvtIv(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), (encoding & (1u << 16)) == 0, inst.Size);
        }

        public static void VcvtIvT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.VcvtIv(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), (encoding & (1u << 16)) == 0, inst.Size);
        }

        public static void VcvtViA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.VcvtVi(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineVF(inst.M, inst.Vm), inst.Op == 0, inst.Size);
        }

        public static void VcvtViT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Opb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpConvert.VcvtVi(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineVF(inst.M, inst.Vm), inst.Op == 0, inst.Size);
        }

        public static void VcvtXsA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Imm6b16w6Vdb12w4Opb8w2Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.VcvtXs(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Imm6, inst.Op, inst.U != 0, inst.Q);
        }

        public static void VcvtXsT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Imm6b16w6Vdb12w4Opb8w2Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonConvert.VcvtXs(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Imm6, inst.Op, inst.U != 0, inst.Q);
        }

        public static void VcvtXvA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Opb18w1Ub16w1Vdb12w4Sfb8w2Sxb7w1Ib5w1Imm4b0w4 inst = new(encoding);

            InstEmitVfpConvert.VcvtXv(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Sf), ImmUtils.CombineImmU5IImm4(inst.I, inst.Imm4), inst.Sx != 0, inst.Sf, inst.Op, inst.U != 0);
        }

        public static void VcvtXvT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Opb18w1Ub16w1Vdb12w4Sfb8w2Sxb7w1Ib5w1Imm4b0w4 inst = new(encoding);

            InstEmitVfpConvert.VcvtXv(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Sf), ImmUtils.CombineImmU5IImm4(inst.I, inst.Imm4), inst.Sx != 0, inst.Sf, inst.Op, inst.U != 0);
        }

        public static void VdivA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VdivF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VdivT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VdivF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VdotA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VdotT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VdotSA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VdotST1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VdupRA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Bb22w1Qb21w1Vdb16w4Rtb12w4Db7w1Eb5w1 inst = new(encoding);

            InstEmitNeonMove.VdupR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rt, inst.B, inst.E, inst.Q);
        }

        public static void VdupRT1(CodeGenContext context, uint encoding)
        {
            InstBb22w1Qb21w1Vdb16w4Rtb12w4Db7w1Eb5w1 inst = new(encoding);

            InstEmitNeonMove.VdupR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rt, inst.B, inst.E, inst.Q);
        }

        public static void VdupSA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Imm4b16w4Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.VdupS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Imm4, inst.Q);
        }

        public static void VdupST1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Imm4b16w4Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.VdupS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Imm4, inst.Q);
        }

        public static void VeorA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VeorR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VeorT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VeorR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VextA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Imm4b8w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vext(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Imm4, inst.Q);
        }

        public static void VextT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Imm4b8w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vext(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Imm4, inst.Q);
        }

        public static void VfmaA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VfmaF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VfmaA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VfmaF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VfmaT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VfmaF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VfmaT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VfmaF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VfmalA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VfmalT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VfmalSA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VfmalST1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VfmaBfA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VfmaBfT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VfmaBfsA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VfmaBfsT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VfmsA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VfmsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VfmsA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VfmsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VfmsT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VfmsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VfmsT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VfmsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VfmslA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VfmslT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VfmslSA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VfmslST1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VfnmaA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VfnmaF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VfnmaT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VfnmaF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VfnmsA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VfnmsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VfnmsT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VfnmsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VhaddA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vhadd(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VhaddT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vhadd(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VhsubA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vhsub(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VhsubT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vhsub(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VinsA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vdb12w4Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VinsT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vdb12w4Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VjcvtA1(CodeGenContext context, uint encoding)
        {
            _ = new InstCondb28w4Db22w1Vdb12w4Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VjcvtT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vdb12w4Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void Vld11A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld11(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vld11A2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld11(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vld11A3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld11(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vld11T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld11(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vld11T2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld11(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vld11T3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld11(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vld1AA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Tb5w1Ab4w1Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld1A(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.A, inst.T, inst.Size);
        }

        public static void Vld1AT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Tb5w1Ab4w1Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld1A(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.A, inst.T, inst.Size);
        }

        public static void Vld1MA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 1, inst.Align, inst.Size);
        }

        public static void Vld1MA2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 2, inst.Align, inst.Size);
        }

        public static void Vld1MA3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 3, inst.Align, inst.Size);
        }

        public static void Vld1MA4(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 4, inst.Align, inst.Size);
        }

        public static void Vld1MT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 1, inst.Align, inst.Size);
        }

        public static void Vld1MT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 2, inst.Align, inst.Size);
        }

        public static void Vld1MT3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 3, inst.Align, inst.Size);
        }

        public static void Vld1MT4(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 4, inst.Align, inst.Size);
        }

        public static void Vld21A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld21(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vld21A2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld21(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vld21A3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld21(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vld21T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld21(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vld21T2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld21(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vld21T3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld21(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vld2AA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Tb5w1Ab4w1Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld2A(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.A, inst.T, inst.Size);
        }

        public static void Vld2AT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Tb5w1Ab4w1Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld2A(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.A, inst.T, inst.Size);
        }

        public static void Vld2MA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld2M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, (encoding >> 8) & 0xf, inst.Align, inst.Size);
        }

        public static void Vld2MA2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld2M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.Align, inst.Size);
        }

        public static void Vld2MT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld2M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, (encoding >> 8) & 0xf, inst.Align, inst.Size);
        }

        public static void Vld2MT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld2M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.Align, inst.Size);
        }

        public static void Vld31A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld31(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vld31A2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld31(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vld31A3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld31(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vld31T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld31(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vld31T2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld31(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vld31T3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld31(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vld3AA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Tb5w1Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld3A(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 0, inst.T, inst.Size);
        }

        public static void Vld3AT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Tb5w1Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld3A(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 0, inst.T, inst.Size);
        }

        public static void Vld3MA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld3M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, (encoding >> 8) & 0xf, inst.Align, inst.Size);
        }

        public static void Vld3MT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld3M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, (encoding >> 8) & 0xf, inst.Align, inst.Size);
        }

        public static void Vld41A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld41(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vld41A2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld41(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vld41A3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld41(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vld41T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld41(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vld41T2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld41(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vld41T3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld41(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vld4AA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Tb5w1Ab4w1Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld4A(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.A, inst.T, inst.Size);
        }

        public static void Vld4AT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Tb5w1Ab4w1Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld4A(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.A, inst.T, inst.Size);
        }

        public static void Vld4MA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld4M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, (encoding >> 8) & 0xf, inst.Align, inst.Size);
        }

        public static void Vld4MT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vld4M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, (encoding >> 8) & 0xf, inst.Align, inst.Size);
        }

        public static void VldmA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm871b1w7 inst = new(encoding);

            InstEmitNeonMemory.Vldm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Imm871, inst.U != 0, inst.W != 0, singleRegs: false);
        }

        public static void VldmA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm8b0w8 inst = new(encoding);

            InstEmitNeonMemory.Vldm(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), inst.Rn, inst.Imm8, inst.U != 0, inst.W != 0, singleRegs: true);
        }

        public static void VldmT1(CodeGenContext context, uint encoding)
        {
            InstPb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm871b1w7 inst = new(encoding);

            InstEmitNeonMemory.Vldm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Imm871, inst.U != 0, inst.W != 0, singleRegs: false);
        }

        public static void VldmT2(CodeGenContext context, uint encoding)
        {
            InstPb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm8b0w8 inst = new(encoding);

            InstEmitNeonMemory.Vldm(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), inst.Rn, inst.Imm8, inst.U != 0, inst.W != 0, singleRegs: true);
        }

        public static void VldrIA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Db22w1Rnb16w4Vdb12w4Sizeb8w2Imm8b0w8 inst = new(encoding);

            InstEmitNeonMemory.Vldr(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), inst.Rn, inst.Imm8, inst.U != 0, inst.Size);
        }

        public static void VldrIT1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Db22w1Rnb16w4Vdb12w4Sizeb8w2Imm8b0w8 inst = new(encoding);

            InstEmitNeonMemory.Vldr(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), inst.Rn, inst.Imm8, inst.U != 0, inst.Size);
        }

        public static void VldrLA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Db22w1Vdb12w4Sizeb8w2Imm8b0w8 inst = new(encoding);

            InstEmitNeonMemory.Vldr(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), RegisterUtils.PcRegister, inst.Imm8, inst.U != 0, inst.Size);
        }

        public static void VldrLT1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Db22w1Vdb12w4Sizeb8w2Imm8b0w8 inst = new(encoding);

            InstEmitNeonMemory.Vldr(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), RegisterUtils.PcRegister, inst.Imm8, inst.U != 0, inst.Size);
        }

        public static void VmaxnmA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vmaxnm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VmaxnmA2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.Vmaxnm(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VmaxnmT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vmaxnm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VmaxnmT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.Vmaxnm(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VmaxFA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmaxF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VmaxFT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmaxF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VmaxIA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmaxI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VmaxIT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmaxI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VminnmA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vminnm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VminnmA2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.Vminnm(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VminnmT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vminnm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VminnmT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.Vminnm(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VminFA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VminF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VminFT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VminF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VminIA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VminI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VminIT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VminI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VmlalIA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlalI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VmlalIT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlalI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VmlalSA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlalS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VmlalST1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlalS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VmlaFA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlaF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VmlaFA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VmlaF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VmlaFT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlaF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VmlaFT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VmlaF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VmlaIA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlaI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VmlaIT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlaI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VmlaSA1(CodeGenContext context, uint encoding)
        {
            InstQb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Fb8w1Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlaS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VmlaST1(CodeGenContext context, uint encoding)
        {
            InstQb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Fb8w1Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlaS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VmlslIA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlslI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VmlslIT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlslI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VmlslSA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlslS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VmlslST1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlslS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VmlsFA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VmlsFA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VmlsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VmlsFT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VmlsFT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VmlsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VmlsIA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlsI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VmlsIT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlsI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VmlsSA1(CodeGenContext context, uint encoding)
        {
            InstQb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Fb8w1Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlsS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VmlsST1(CodeGenContext context, uint encoding)
        {
            InstQb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Fb8w1Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmlsS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VmmlaA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VmmlaT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VmovlA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Imm3hb19w3Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vmovl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Imm3h);
        }

        public static void VmovlT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Imm3hb19w3Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vmovl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Imm3h);
        }

        public static void VmovnA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vmovn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VmovnT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vmovn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VmovxA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vmovx(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M));
        }

        public static void VmovxT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vmovx(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M));
        }

        public static void VmovDA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Opb20w1Rt2b16w4Rtb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.VmovD(context, inst.Rt, inst.Rt2, InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0);
        }

        public static void VmovDT1(CodeGenContext context, uint encoding)
        {
            InstOpb20w1Rt2b16w4Rtb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.VmovD(context, inst.Rt, inst.Rt2, InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0);
        }

        public static void VmovHA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Opb20w1Vnb16w4Rtb12w4Nb7w1 inst = new(encoding);

            InstEmitNeonMove.VmovH(context, inst.Rt, InstEmitCommon.CombineVF(inst.N, inst.Vn), inst.Op != 0);
        }

        public static void VmovHT1(CodeGenContext context, uint encoding)
        {
            InstOpb20w1Vnb16w4Rtb12w4Nb7w1 inst = new(encoding);

            InstEmitNeonMove.VmovH(context, inst.Rt, InstEmitCommon.CombineVF(inst.N, inst.Vn), inst.Op != 0);
        }

        public static void VmovIA1(CodeGenContext context, uint encoding)
        {
            InstIb24w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmovI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), 0, (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmovIA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Imm4hb16w4Vdb12w4Sizeb8w2Imm4lb0w4 inst = new(encoding);

            InstEmitNeonMove.VmovFI(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), inst.Size);
        }

        public static void VmovIA3(CodeGenContext context, uint encoding)
        {
            InstIb24w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmovI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), 0, (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmovIA4(CodeGenContext context, uint encoding)
        {
            InstIb24w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmovI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), 0, (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmovIA5(CodeGenContext context, uint encoding)
        {
            InstIb24w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmovI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), 1, (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmovIT1(CodeGenContext context, uint encoding)
        {
            InstIb28w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmovI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), 0, (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmovIT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Imm4hb16w4Vdb12w4Sizeb8w2Imm4lb0w4 inst = new(encoding);

            InstEmitNeonMove.VmovFI(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), ImmUtils.CombineImmU8(inst.Imm4l, inst.Imm4h), inst.Size);
        }

        public static void VmovIT3(CodeGenContext context, uint encoding)
        {
            InstIb28w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmovI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), 0, (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmovIT4(CodeGenContext context, uint encoding)
        {
            InstIb28w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmovI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), 0, (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmovIT5(CodeGenContext context, uint encoding)
        {
            InstIb28w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmovI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), 1, (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmovRA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            uint size = (encoding >> 8) & 3;

            InstEmitNeonMove.VmovR(context, InstEmitCommon.CombineV(inst.Vd, inst.D, size), InstEmitCommon.CombineV(inst.Vm, inst.M, size), size);
        }

        public static void VmovRT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            uint size = (encoding >> 8) & 3;

            InstEmitNeonMove.VmovR(context, InstEmitCommon.CombineV(inst.Vd, inst.D, size), InstEmitCommon.CombineV(inst.Vm, inst.M, size), size);
        }

        public static void VmovRsA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Opc1b21w2Vdb16w4Rtb12w4Db7w1Opc2b5w2 inst = new(encoding);

            InstEmitNeonMove.VmovRs(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rt, inst.Opc1, inst.Opc2);
        }

        public static void VmovRsT1(CodeGenContext context, uint encoding)
        {
            InstOpc1b21w2Vdb16w4Rtb12w4Db7w1Opc2b5w2 inst = new(encoding);

            InstEmitNeonMove.VmovRs(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rt, inst.Opc1, inst.Opc2);
        }

        public static void VmovSA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Opb20w1Vnb16w4Rtb12w4Nb7w1 inst = new(encoding);

            InstEmitNeonMove.VmovS(context, inst.Rt, InstEmitCommon.CombineVF(inst.N, inst.Vn), inst.Op != 0);
        }

        public static void VmovST1(CodeGenContext context, uint encoding)
        {
            InstOpb20w1Vnb16w4Rtb12w4Nb7w1 inst = new(encoding);

            InstEmitNeonMove.VmovS(context, inst.Rt, InstEmitCommon.CombineVF(inst.N, inst.Vn), inst.Op != 0);
        }

        public static void VmovSrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Opc1b21w2Vnb16w4Rtb12w4Nb7w1Opc2b5w2 inst = new(encoding);

            InstEmitNeonMove.VmovSr(context, inst.Rt, InstEmitCommon.CombineV(inst.Vn, inst.N), inst.U != 0, inst.Opc1, inst.Opc2);
        }

        public static void VmovSrT1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Opc1b21w2Vnb16w4Rtb12w4Nb7w1Opc2b5w2 inst = new(encoding);

            InstEmitNeonMove.VmovSr(context, inst.Rt, InstEmitCommon.CombineV(inst.Vn, inst.N), inst.U != 0, inst.Opc1, inst.Opc2);
        }

        public static void VmovSsA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Opb20w1Rt2b16w4Rtb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.VmovSs(context, inst.Rt, inst.Rt2, InstEmitCommon.CombineVF(inst.M, inst.Vm), inst.Op != 0);
        }

        public static void VmovSsT1(CodeGenContext context, uint encoding)
        {
            InstOpb20w1Rt2b16w4Rtb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.VmovSs(context, inst.Rt, inst.Rt2, InstEmitCommon.CombineVF(inst.M, inst.Vm), inst.Op != 0);
        }

        public static void VmrsA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Regb16w4Rtb12w4 inst = new(encoding);

            InstEmitNeonSystem.Vmrs(context, inst.Rt, inst.Reg);
        }

        public static void VmrsT1(CodeGenContext context, uint encoding)
        {
            InstRegb16w4Rtb12w4 inst = new(encoding);

            InstEmitNeonSystem.Vmrs(context, inst.Rt, inst.Reg);
        }

        public static void VmsrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Regb16w4Rtb12w4 inst = new(encoding);

            InstEmitNeonSystem.Vmsr(context, inst.Rt, inst.Reg);
        }

        public static void VmsrT1(CodeGenContext context, uint encoding)
        {
            InstRegb16w4Rtb12w4 inst = new(encoding);

            InstEmitNeonSystem.Vmsr(context, inst.Rt, inst.Reg);
        }

        public static void VmullIA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Opb9w1Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmullI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.U != 0, inst.Size);
        }

        public static void VmullIT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Opb9w1Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmullI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.U != 0, inst.Size);
        }

        public static void VmullSA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmullS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VmullST1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmullS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VmulFA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmulF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VmulFA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VmulF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VmulFT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmulF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VmulFT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VmulF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VmulIA1(CodeGenContext context, uint encoding)
        {
            InstOpb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmulI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VmulIT1(CodeGenContext context, uint encoding)
        {
            InstOpb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmulI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VmulSA1(CodeGenContext context, uint encoding)
        {
            InstQb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Fb8w1Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmulS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VmulST1(CodeGenContext context, uint encoding)
        {
            InstQb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Fb8w1Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VmulS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VmvnIA1(CodeGenContext context, uint encoding)
        {
            InstIb24w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmvnI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmvnIA2(CodeGenContext context, uint encoding)
        {
            InstIb24w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmvnI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmvnIA3(CodeGenContext context, uint encoding)
        {
            InstIb24w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmvnI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmvnIT1(CodeGenContext context, uint encoding)
        {
            InstIb28w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmvnI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmvnIT2(CodeGenContext context, uint encoding)
        {
            InstIb28w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmvnI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmvnIT3(CodeGenContext context, uint encoding)
        {
            InstIb28w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonMove.VmvnI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VmvnRA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.VmvnR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VmvnRT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.VmvnR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VnegA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vneg(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VnegA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VnegF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VnegT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb10w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vneg(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VnegT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VnegF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VnmlaA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VnmlaF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VnmlaT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VnmlaF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VnmlsA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VnmlsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VnmlsT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VnmlsF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VnmulA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VnmulF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VnmulT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VnmulF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VornRA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VornR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VornRT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VornR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VorrIA1(CodeGenContext context, uint encoding)
        {
            InstIb24w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonLogical.VorrI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VorrIA2(CodeGenContext context, uint encoding)
        {
            InstIb24w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonLogical.VorrI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VorrIT1(CodeGenContext context, uint encoding)
        {
            InstIb28w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonLogical.VorrI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VorrIT2(CodeGenContext context, uint encoding)
        {
            InstIb28w1Db22w1Imm3b16w3Vdb12w4Qb6w1Imm4b0w4 inst = new(encoding);

            InstEmitNeonLogical.VorrI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), (encoding >> 8) & 0xf, ImmUtils.CombineImmU8(inst.Imm4, inst.Imm3, inst.I), inst.Q);
        }

        public static void VorrRA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VorrR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VorrRT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonLogical.VorrR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VpadalA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vpadal(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VpadalT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vpadal(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VpaddlA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vpaddl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VpaddlT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vpaddl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Size, inst.Q);
        }

        public static void VpaddFA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VpaddF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VpaddFT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VpaddF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VpaddIA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VpaddI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VpaddIT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VpaddI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VpmaxFA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VpmaxF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, 0);
        }

        public static void VpmaxFT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VpmaxF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, 0);
        }

        public static void VpmaxIA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VpmaxI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, 0);
        }

        public static void VpmaxIT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VpmaxI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, 0);
        }

        public static void VpminFA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VpminF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, 0);
        }

        public static void VpminFT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VpminF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, 0);
        }

        public static void VpminIA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VpminI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, 0);
        }

        public static void VpminIT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VpminI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, 0);
        }

        public static void VqabsA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqabs(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqabsT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqabs(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqaddA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqadd(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VqaddT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqadd(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VqdmlalA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqdmlal(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VqdmlalA2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqdmlalS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VqdmlalT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqdmlal(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VqdmlalT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqdmlalS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VqdmlslA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqdmlsl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VqdmlslA2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqdmlslS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VqdmlslT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqdmlsl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VqdmlslT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqdmlslS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VqdmulhA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqdmulh(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqdmulhA2(CodeGenContext context, uint encoding)
        {
            InstQb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqdmulhS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqdmulhT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqdmulh(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqdmulhT2(CodeGenContext context, uint encoding)
        {
            InstQb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqdmulhS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqdmullA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqdmull(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VqdmullA2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqdmullS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VqdmullT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqdmull(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VqdmullT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqdmullS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VqmovnA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb6w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqmovn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op, inst.Size);
        }

        public static void VqmovnT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Opb6w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqmovn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op, inst.Size);
        }

        public static void VqnegA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqneg(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqnegT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqneg(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrdmlahA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqrdmlah(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrdmlahA2(CodeGenContext context, uint encoding)
        {
            InstQb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqrdmlahS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrdmlahT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqrdmlah(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrdmlahT2(CodeGenContext context, uint encoding)
        {
            InstQb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqrdmlahS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrdmlshA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqrdmlsh(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrdmlshA2(CodeGenContext context, uint encoding)
        {
            InstQb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqrdmlshS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrdmlshT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqrdmlsh(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrdmlshT2(CodeGenContext context, uint encoding)
        {
            InstQb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqrdmlshS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrdmulhA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqrdmulh(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrdmulhA2(CodeGenContext context, uint encoding)
        {
            InstQb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqrdmulhS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrdmulhT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqrdmulh(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrdmulhT2(CodeGenContext context, uint encoding)
        {
            InstQb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqrdmulhS(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrshlA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqrshl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrshlT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqrshl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VqrshrnA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Imm6b16w6Vdb12w4Opb8w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqrshrn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Op, inst.Imm6);
        }

        public static void VqrshrnT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Imm6b16w6Vdb12w4Opb8w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqrshrn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Op, inst.Imm6);
        }

        public static void VqshlIA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Imm6b16w6Vdb12w4Opb8w1Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqshlI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Op, inst.L, inst.Imm6, inst.Q);
        }

        public static void VqshlIT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Imm6b16w6Vdb12w4Opb8w1Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqshlI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Op, inst.L, inst.Imm6, inst.Q);
        }

        public static void VqshlRA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqshlR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VqshlRT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.VqshlR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VqshrnA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Imm6b16w6Vdb12w4Opb8w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqshrn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Op, inst.Imm6);
        }

        public static void VqshrnT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Imm6b16w6Vdb12w4Opb8w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqshrn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Op, inst.Imm6);
        }

        public static void VqsubA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqsub(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VqsubT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonSaturate.Vqsub(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VraddhnA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vraddhn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VraddhnT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vraddhn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VrecpeA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb8w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vrecpe(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VrecpeT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb8w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vrecpe(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VrecpsA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vrecps(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VrecpsT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vrecps(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void Vrev16A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonBit.Vrev16(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void Vrev16T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonBit.Vrev16(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void Vrev32A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonBit.Vrev32(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void Vrev32T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonBit.Vrev32(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void Vrev64A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonBit.Vrev64(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void Vrev64T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonBit.Vrev64(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VrhaddA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrhadd(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VrhaddT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrhadd(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VrintaAsimdA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrinta(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VrintaAsimdT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrinta(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VrintaVfpA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrinta(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrintaVfpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrinta(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrintmAsimdA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrintm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VrintmAsimdT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrintm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VrintmVfpA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrintm(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrintmVfpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrintm(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrintnAsimdA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrintn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VrintnAsimdT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrintn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VrintnVfpA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrintn(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrintnVfpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrintn(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrintpAsimdA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrintp(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VrintpAsimdT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrintp(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VrintpVfpA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrintp(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrintpVfpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrintp(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrintrVfpA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrintr(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrintrVfpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrintr(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrintxAsimdA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrintx(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VrintxAsimdT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrintx(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VrintxVfpA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrintx(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrintxVfpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrintx(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrintzAsimdA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrintz(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VrintzAsimdT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrintz(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VrintzVfpA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrintz(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrintzVfpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpRound.Vrintz(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VrshlA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrshl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VrshlT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrshl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VrshrA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrshr(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.L, inst.Imm6, inst.Q);
        }

        public static void VrshrT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrshr(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.L, inst.Imm6, inst.Q);
        }

        public static void VrshrnA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Imm6b16w6Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrshrn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Imm6);
        }

        public static void VrshrnT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Imm6b16w6Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrshrn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Imm6);
        }

        public static void VrsqrteA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb8w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vrsqrte(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VrsqrteT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Fb8w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vrsqrte(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.F != 0, inst.Size, inst.Q);
        }

        public static void VrsqrtsA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vrsqrts(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VrsqrtsT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vrsqrts(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VrsraA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrsra(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.L, inst.Imm6, inst.Q);
        }

        public static void VrsraT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrsra(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.L, inst.Imm6, inst.Q);
        }

        public static void VrsubhnA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrsubhn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VrsubhnT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonRound.Vrsubhn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VsdotA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VsdotT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VsdotSA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VsdotST1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VselA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Ccb20w2Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpMove.Vsel(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Cc, inst.Size);
        }

        public static void VselT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Ccb20w2Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpMove.Vsel(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Cc, inst.Size);
        }

        public static void VshllA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Imm6b16w6Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vshll(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Imm6, inst.U != 0);
        }

        public static void VshllA2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vshll2(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VshllT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Imm6b16w6Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vshll(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Imm6, inst.U != 0);
        }

        public static void VshllT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vshll2(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VshlIA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.VshlI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.L, inst.Imm6, inst.Q);
        }

        public static void VshlIT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.VshlI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.L, inst.Imm6, inst.Q);
        }

        public static void VshlRA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.VshlR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VshlRT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.VshlR(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size, inst.Q);
        }

        public static void VshrA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vshr(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.L, inst.Imm6, inst.Q);
        }

        public static void VshrT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vshr(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.L, inst.Imm6, inst.Q);
        }

        public static void VshrnA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Imm6b16w6Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vshrn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Imm6);
        }

        public static void VshrnT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Imm6b16w6Vdb12w4Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vshrn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Imm6);
        }

        public static void VsliA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vsli(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.L, inst.Imm6, inst.Q);
        }

        public static void VsliT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vsli(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.L, inst.Imm6, inst.Q);
        }

        public static void VsmmlaA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VsmmlaT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VsqrtA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VsqrtF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VsqrtT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Sizeb8w2Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VsqrtF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VsraA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vsra(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.L, inst.Imm6, inst.Q);
        }

        public static void VsraT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vsra(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.L, inst.Imm6, inst.Q);
        }

        public static void VsriA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vsri(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.L, inst.Imm6, inst.Q);
        }

        public static void VsriT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Imm6b16w6Vdb12w4Lb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonShift.Vsri(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.L, inst.Imm6, inst.Q);
        }

        public static void Vst11A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst11(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vst11A2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst11(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vst11A3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst11(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vst11T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst11(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vst11T2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst11(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vst11T3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst11(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vst1MA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 1, inst.Align, inst.Size);
        }

        public static void Vst1MA2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 2, inst.Align, inst.Size);
        }

        public static void Vst1MA3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 3, inst.Align, inst.Size);
        }

        public static void Vst1MA4(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 4, inst.Align, inst.Size);
        }

        public static void Vst1MT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 1, inst.Align, inst.Size);
        }

        public static void Vst1MT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 2, inst.Align, inst.Size);
        }

        public static void Vst1MT3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 3, inst.Align, inst.Size);
        }

        public static void Vst1MT4(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst1M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, 4, inst.Align, inst.Size);
        }

        public static void Vst21A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst21(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vst21A2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst21(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vst21A3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst21(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vst21T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst21(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vst21T2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst21(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vst21T3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst21(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vst2MA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst2M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, (encoding >> 8) & 0xf, inst.Align, inst.Size);
        }

        public static void Vst2MA2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst2M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.Align, inst.Size);
        }

        public static void Vst2MT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst2M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, (encoding >> 8) & 0xf, inst.Align, inst.Size);
        }

        public static void Vst2MT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst2M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.Align, inst.Size);
        }

        public static void Vst31A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst31(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vst31A2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst31(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vst31A3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst31(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vst31T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst31(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vst31T2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst31(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vst31T3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst31(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vst3MA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst3M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, (encoding >> 8) & 0xf, inst.Align, inst.Size);
        }

        public static void Vst3MT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst3M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, (encoding >> 8) & 0xf, inst.Align, inst.Size);
        }

        public static void Vst41A1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst41(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vst41A2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst41(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vst41A3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst41(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vst41T1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst41(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 0);
        }

        public static void Vst41T2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst41(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 1);
        }

        public static void Vst41T3(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4IndexAlignb4w4Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst41(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, inst.IndexAlign, 2);
        }

        public static void Vst4MA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst4M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, (encoding >> 8) & 0xf, inst.Align, inst.Size);
        }

        public static void Vst4MT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Rnb16w4Vdb12w4Sizeb6w2Alignb4w2Rmb0w4 inst = new(encoding);

            InstEmitNeonMemory.Vst4M(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Rm, (encoding >> 8) & 0xf, inst.Align, inst.Size);
        }

        public static void VstmA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm871b1w7 inst = new(encoding);

            InstEmitNeonMemory.Vstm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Imm871, inst.U != 0, inst.W != 0, singleRegs: false);
        }

        public static void VstmA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Pb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm8b0w8 inst = new(encoding);

            InstEmitNeonMemory.Vstm(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), inst.Rn, inst.Imm8, inst.U != 0, inst.W != 0, singleRegs: true);
        }

        public static void VstmT1(CodeGenContext context, uint encoding)
        {
            InstPb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm871b1w7 inst = new(encoding);

            InstEmitNeonMemory.Vstm(context, InstEmitCommon.CombineV(inst.Vd, inst.D), inst.Rn, inst.Imm871, inst.U != 0, inst.W != 0, singleRegs: false);
        }

        public static void VstmT2(CodeGenContext context, uint encoding)
        {
            InstPb24w1Ub23w1Db22w1Wb21w1Rnb16w4Vdb12w4Imm8b0w8 inst = new(encoding);

            InstEmitNeonMemory.Vstm(context, InstEmitCommon.CombineVF(inst.D, inst.Vd), inst.Rn, inst.Imm8, inst.U != 0, inst.W != 0, singleRegs: true);
        }

        public static void VstrA1(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Ub23w1Db22w1Rnb16w4Vdb12w4Sizeb8w2Imm8b0w8 inst = new(encoding);

            InstEmitNeonMemory.Vstr(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), inst.Rn, inst.Imm8, inst.U != 0, inst.Size);
        }

        public static void VstrT1(CodeGenContext context, uint encoding)
        {
            InstUb23w1Db22w1Rnb16w4Vdb12w4Sizeb8w2Imm8b0w8 inst = new(encoding);

            InstEmitNeonMemory.Vstr(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), inst.Rn, inst.Imm8, inst.U != 0, inst.Size);
        }

        public static void VsubhnA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vsubhn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VsubhnT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vsubhn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size);
        }

        public static void VsublA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vsubl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VsublT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vsubl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VsubwA1(CodeGenContext context, uint encoding)
        {
            InstUb24w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vsubw(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VsubwT1(CodeGenContext context, uint encoding)
        {
            InstUb28w1Db22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.Vsubw(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.U != 0, inst.Size);
        }

        public static void VsubFA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VsubF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VsubFA2(CodeGenContext context, uint encoding)
        {
            InstCondb28w4Db22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VsubF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VsubFT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Szb20w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VsubF(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Sz, inst.Q);
        }

        public static void VsubFT2(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Sizeb8w2Nb7w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitVfpArithmetic.VsubF(context, InstEmitCommon.CombineV(inst.Vd, inst.D, inst.Size), InstEmitCommon.CombineV(inst.Vn, inst.N, inst.Size), InstEmitCommon.CombineV(inst.Vm, inst.M, inst.Size), inst.Size);
        }

        public static void VsubIA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VsubI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VsubIT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonArithmetic.VsubI(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VsudotSA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VsudotST1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VswpA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vswp(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VswpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vswp(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Q);
        }

        public static void VtblA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Lenb8w2Nb7w1Opb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vtbl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Len);
        }

        public static void VtblT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Vnb16w4Vdb12w4Lenb8w2Nb7w1Opb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vtbl(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Op != 0, inst.Len);
        }

        public static void VtrnA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vtrn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VtrnT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vtrn(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VtstA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.Vtst(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VtstT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb20w2Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonCompare.Vtst(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vn, inst.N), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VudotA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VudotT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VudotSA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VudotST1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VummlaA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VummlaT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VusdotA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VusdotT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VusdotSA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VusdotST1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Qb6w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VusmmlaA1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VusmmlaT1(CodeGenContext context, uint encoding)
        {
            _ = new InstDb22w1Vnb16w4Vdb12w4Nb7w1Mb5w1Vmb0w4(encoding);

            throw new NotImplementedException();
        }

        public static void VuzpA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vuzp(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VuzpT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vuzp(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VzipA1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vzip(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void VzipT1(CodeGenContext context, uint encoding)
        {
            InstDb22w1Sizeb18w2Vdb12w4Qb6w1Mb5w1Vmb0w4 inst = new(encoding);

            InstEmitNeonMove.Vzip(context, InstEmitCommon.CombineV(inst.Vd, inst.D), InstEmitCommon.CombineV(inst.Vm, inst.M), inst.Size, inst.Q);
        }

        public static void WfeA1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Wfe();
        }

        public static void WfeT1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Wfe();
        }

        public static void WfeT2(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Wfe();
        }

        public static void WfiA1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Wfi();
        }

        public static void WfiT1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Wfi();
        }

        public static void WfiT2(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Wfi();
        }

        public static void YieldA1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Yield();
        }

        public static void YieldT1(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Yield();
        }

        public static void YieldT2(CodeGenContext context, uint encoding)
        {
            context.Arm64Assembler.Yield();
        }
    }
}
