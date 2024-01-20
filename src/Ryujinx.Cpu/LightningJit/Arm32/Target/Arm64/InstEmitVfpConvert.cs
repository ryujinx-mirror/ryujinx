using System;
using System.Diagnostics;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitVfpConvert
    {
        public static void Vcvta(CodeGenContext context, uint rd, uint rm, bool op, uint size)
        {
            if (size == 3)
            {
                // F64 -> S32/U32 conversion on SIMD is not supported, so we convert it to a GPR, then insert it back into the SIMD register.

                if (op)
                {
                    InstEmitNeonCommon.EmitScalarUnaryToGprTempF(context, rd, rm, size, 0, context.Arm64Assembler.FcvtasFloat);
                }
                else
                {
                    InstEmitNeonCommon.EmitScalarUnaryToGprTempF(context, rd, rm, size, 0, context.Arm64Assembler.FcvtauFloat);
                }
            }
            else if (op)
            {
                InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FcvtasS, context.Arm64Assembler.FcvtasSH);
            }
            else
            {
                InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FcvtauS, context.Arm64Assembler.FcvtauSH);
            }
        }

        public static void Vcvtb(CodeGenContext context, uint rd, uint rm, uint sz, uint op)
        {
            EmitVcvtbVcvtt(context, rd, rm, sz, op, top: false);
        }

        public static void Vcvtm(CodeGenContext context, uint rd, uint rm, bool op, uint size)
        {
            if (size == 3)
            {
                // F64 -> S32/U32 conversion on SIMD is not supported, so we convert it to a GPR, then insert it back into the SIMD register.

                if (op)
                {
                    InstEmitNeonCommon.EmitScalarUnaryToGprTempF(context, rd, rm, size, 0, context.Arm64Assembler.FcvtmsFloat);
                }
                else
                {
                    InstEmitNeonCommon.EmitScalarUnaryToGprTempF(context, rd, rm, size, 0, context.Arm64Assembler.FcvtmuFloat);
                }
            }
            else if (op)
            {
                InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FcvtmsS, context.Arm64Assembler.FcvtmsSH);
            }
            else
            {
                InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FcvtmuS, context.Arm64Assembler.FcvtmuSH);
            }
        }

        public static void Vcvtn(CodeGenContext context, uint rd, uint rm, bool op, uint size)
        {
            if (size == 3)
            {
                // F64 -> S32/U32 conversion on SIMD is not supported, so we convert it to a GPR, then insert it back into the SIMD register.

                if (op)
                {
                    InstEmitNeonCommon.EmitScalarUnaryToGprTempF(context, rd, rm, size, 0, context.Arm64Assembler.FcvtnsFloat);
                }
                else
                {
                    InstEmitNeonCommon.EmitScalarUnaryToGprTempF(context, rd, rm, size, 0, context.Arm64Assembler.FcvtnuFloat);
                }
            }
            else if (op)
            {
                InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FcvtnsS, context.Arm64Assembler.FcvtnsSH);
            }
            else
            {
                InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FcvtnuS, context.Arm64Assembler.FcvtnuSH);
            }
        }

        public static void Vcvtp(CodeGenContext context, uint rd, uint rm, bool op, uint size)
        {
            if (size == 3)
            {
                // F64 -> S32/U32 conversion on SIMD is not supported, so we convert it to a GPR, then insert it back into the SIMD register.

                if (op)
                {
                    InstEmitNeonCommon.EmitScalarUnaryToGprTempF(context, rd, rm, size, 0, context.Arm64Assembler.FcvtpsFloat);
                }
                else
                {
                    InstEmitNeonCommon.EmitScalarUnaryToGprTempF(context, rd, rm, size, 0, context.Arm64Assembler.FcvtpuFloat);
                }
            }
            else if (op)
            {
                InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FcvtpsS, context.Arm64Assembler.FcvtpsSH);
            }
            else
            {
                InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FcvtpuS, context.Arm64Assembler.FcvtpuSH);
            }
        }

        public static void VcvtDs(CodeGenContext context, uint rd, uint rm, uint size)
        {
            bool doubleToSingle = size == 3;

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

            if (doubleToSingle)
            {
                // Double to single.

                using ScopedRegister rmReg = InstEmitNeonCommon.MoveScalarToSide(context, rm, false);

                context.Arm64Assembler.FcvtFloat(tempRegister.Operand, rmReg.Operand, 0, 1);

                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, true);
            }
            else
            {
                // Single to double.

                using ScopedRegister rmReg = InstEmitNeonCommon.MoveScalarToSide(context, rm, true);

                context.Arm64Assembler.FcvtFloat(tempRegister.Operand, rmReg.Operand, 1, 0);

                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, false);
            }
        }

        public static void VcvtIv(CodeGenContext context, uint rd, uint rm, bool unsigned, uint size)
        {
            if (size == 3)
            {
                // F64 -> S32/U32 conversion on SIMD is not supported, so we convert it to a GPR, then insert it back into the SIMD register.

                if (unsigned)
                {
                    InstEmitNeonCommon.EmitScalarUnaryToGprTempF(context, rd, rm, size, 0, context.Arm64Assembler.FcvtzuFloatInt);
                }
                else
                {
                    InstEmitNeonCommon.EmitScalarUnaryToGprTempF(context, rd, rm, size, 0, context.Arm64Assembler.FcvtzsFloatInt);
                }
            }
            else
            {
                if (unsigned)
                {
                    InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FcvtzuIntS, context.Arm64Assembler.FcvtzuIntSH);
                }
                else
                {
                    InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.FcvtzsIntS, context.Arm64Assembler.FcvtzsIntSH);
                }
            }
        }

        public static void VcvtVi(CodeGenContext context, uint rd, uint rm, bool unsigned, uint size)
        {
            if (size == 3)
            {
                // S32/U32 -> F64 conversion on SIMD is not supported, so we convert it to a GPR, then insert it back into the SIMD register.

                if (unsigned)
                {
                    InstEmitNeonCommon.EmitScalarUnaryFromGprTempF(context, rd, rm, size, 0, context.Arm64Assembler.UcvtfFloatInt);
                }
                else
                {
                    InstEmitNeonCommon.EmitScalarUnaryFromGprTempF(context, rd, rm, size, 0, context.Arm64Assembler.ScvtfFloatInt);
                }
            }
            else
            {
                if (unsigned)
                {
                    InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.UcvtfIntS, context.Arm64Assembler.UcvtfIntSH);
                }
                else
                {
                    InstEmitNeonCommon.EmitScalarUnaryF(context, rd, rm, size, context.Arm64Assembler.ScvtfIntS, context.Arm64Assembler.ScvtfIntSH);
                }
            }
        }

        public static void VcvtXv(CodeGenContext context, uint rd, uint imm5, bool sx, uint sf, uint op, bool u)
        {
            Debug.Assert(op >> 1 == 0);

            bool unsigned = u;
            bool toFixed = op == 1;
            uint size = sf;
            uint fbits = Math.Clamp((sx ? 32u : 16u) - imm5, 1, 8u << (int)size);

            if (toFixed)
            {
                if (unsigned)
                {
                    InstEmitNeonCommon.EmitScalarUnaryFixedF(context, rd, rd, fbits, size, is16Bit: false, context.Arm64Assembler.FcvtzuFixS);
                }
                else
                {
                    InstEmitNeonCommon.EmitScalarUnaryFixedF(context, rd, rd, fbits, size, is16Bit: false, context.Arm64Assembler.FcvtzsFixS);
                }
            }
            else
            {
                if (unsigned)
                {
                    InstEmitNeonCommon.EmitScalarUnaryFixedF(context, rd, rd, fbits, size, is16Bit: !sx, context.Arm64Assembler.UcvtfFixS);
                }
                else
                {
                    InstEmitNeonCommon.EmitScalarUnaryFixedF(context, rd, rd, fbits, size, is16Bit: !sx, context.Arm64Assembler.ScvtfFixS);
                }
            }
        }

        public static void VcvtrIv(CodeGenContext context, uint rd, uint rm, uint op, uint size)
        {
            bool unsigned = (op & 1) == 0;

            Debug.Assert(size == 1 || size == 2 || size == 3);

            bool singleRegs = size != 3;

            using ScopedRegister rmReg = InstEmitNeonCommon.MoveScalarToSide(context, rm, singleRegs);

            using ScopedRegister tempRegister = InstEmitNeonCommon.PickSimdRegister(context.RegisterAllocator, rmReg);

            // Round using the FPCR rounding mode first, since the FCVTZ instructions will use the round to zero mode.
            context.Arm64Assembler.FrintiFloat(tempRegister.Operand, rmReg.Operand, size ^ 2u);

            if (unsigned)
            {
                if (size == 1)
                {
                    context.Arm64Assembler.FcvtzuIntSH(tempRegister.Operand, tempRegister.Operand);
                }
                else
                {
                    context.Arm64Assembler.FcvtzuIntS(tempRegister.Operand, tempRegister.Operand, size & 1);
                }
            }
            else
            {
                if (size == 1)
                {
                    context.Arm64Assembler.FcvtzsIntSH(tempRegister.Operand, tempRegister.Operand);
                }
                else
                {
                    context.Arm64Assembler.FcvtzsIntS(tempRegister.Operand, tempRegister.Operand, size & 1);
                }
            }

            InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, singleRegs);
        }

        public static void Vcvtt(CodeGenContext context, uint rd, uint rm, uint sz, uint op)
        {
            EmitVcvtbVcvtt(context, rd, rm, sz, op, top: true);
        }

        public static void EmitVcvtbVcvtt(CodeGenContext context, uint rd, uint rm, uint sz, uint op, bool top)
        {
            bool usesDouble = sz == 1;
            bool convertFromHalf = op == 0;

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

            if (convertFromHalf)
            {
                // Half to single/double.

                using ScopedRegister rmReg = InstEmitNeonCommon.Move16BitScalarToSide(context, rm, top);

                context.Arm64Assembler.FcvtFloat(tempRegister.Operand, rmReg.Operand, usesDouble ? 1u : 0u, 3u);

                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, !usesDouble);
            }
            else
            {
                // Single/double to half.

                using ScopedRegister rmReg = InstEmitNeonCommon.MoveScalarToSide(context, rm, !usesDouble);

                context.Arm64Assembler.FcvtFloat(tempRegister.Operand, rmReg.Operand, 3u, usesDouble ? 1u : 0u);

                InstEmitNeonCommon.Insert16BitResult(context, tempRegister.Operand, rd, top);
            }
        }
    }
}
