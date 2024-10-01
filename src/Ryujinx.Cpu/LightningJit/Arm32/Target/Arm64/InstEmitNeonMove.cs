using Ryujinx.Cpu.LightningJit.CodeGen;
using System;
using System.Diagnostics;
using System.Numerics;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonMove
    {
        public static void VdupR(CodeGenContext context, uint rd, uint rt, uint b, uint e, uint q)
        {
            uint size = 2 - (e | (b << 1));

            Debug.Assert(size < 3);

            Operand rtOperand = InstEmitCommon.GetInputGpr(context, rt);

            uint imm5 = InstEmitNeonCommon.GetImm5ForElementIndex(0, size);

            if (q == 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                context.Arm64Assembler.DupGen(tempRegister.Operand, rtOperand, imm5, q);

                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Debug.Assert((rd & 1) == 0);

                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));

                context.Arm64Assembler.DupGen(rdOperand, rtOperand, imm5, q);
            }
        }

        public static void VdupS(CodeGenContext context, uint rd, uint rm, uint imm4, uint q)
        {
            uint size = (uint)BitOperations.TrailingZeroCount(imm4);

            Debug.Assert(size < 3);

            uint index = imm4 >> (int)(size + 1);

            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            uint imm5 = InstEmitNeonCommon.GetImm5ForElementIndex(index | ((rm & 1) << (int)(3 - size)), size);

            if (q == 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                context.Arm64Assembler.DupEltVectorFromElement(tempRegister.Operand, rmOperand, imm5, q);

                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Debug.Assert((rd & 1) == 0);

                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));

                context.Arm64Assembler.DupEltVectorFromElement(rdOperand, rmOperand, imm5, q);
            }
        }

        public static void Vext(CodeGenContext context, uint rd, uint rn, uint rm, uint imm4, uint q)
        {
            if (q == 0)
            {
                using ScopedRegister rnReg = InstEmitNeonCommon.MoveScalarToSide(context, rn, false);
                using ScopedRegister rmReg = InstEmitNeonCommon.MoveScalarToSide(context, rm, false);

                using ScopedRegister tempRegister = InstEmitNeonCommon.PickSimdRegister(context.RegisterAllocator, rnReg, rmReg);

                context.Arm64Assembler.Ext(tempRegister.Operand, rnReg.Operand, imm4, rmReg.Operand, q);

                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Debug.Assert(((rd | rn | rm) & 1) == 0);

                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                context.Arm64Assembler.Ext(rdOperand, rnOperand, imm4, rmOperand, q);
            }
        }

        public static void Vmovl(CodeGenContext context, uint rd, uint rm, bool u, uint imm3h)
        {
            uint size = (uint)BitOperations.TrailingZeroCount(imm3h);
            Debug.Assert(size < 3);

            InstEmitNeonCommon.EmitVectorBinaryLongShift(
                context,
                rd,
                rm,
                0,
                size,
                isShl: true,
                u ? context.Arm64Assembler.Ushll : context.Arm64Assembler.Sshll);
        }

        public static void Vmovn(CodeGenContext context, uint rd, uint rm, uint size)
        {
            Debug.Assert(size < 3);

            InstEmitNeonCommon.EmitVectorUnaryNarrow(context, rd, rm, size, context.Arm64Assembler.Xtn);
        }

        public static void Vmovx(CodeGenContext context, uint rd, uint rm)
        {
            InstEmitNeonCommon.EmitScalarBinaryShift(context, rd, rm, 16, 2, isShl: false, context.Arm64Assembler.UshrS);
        }

        public static void VmovD(CodeGenContext context, uint rt, uint rt2, uint rm, bool op)
        {
            Operand rmReg = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            uint top = rm & 1;
            uint ftype = top + 1;

            if (op)
            {
                Operand rtOperand = InstEmitCommon.GetOutputGpr(context, rt);
                Operand rt2Operand = InstEmitCommon.GetOutputGpr(context, rt2);

                Operand rtOperand64 = new(OperandKind.Register, OperandType.I64, rtOperand.Value);
                Operand rt2Operand64 = new(OperandKind.Register, OperandType.I64, rt2Operand.Value);

                context.Arm64Assembler.FmovFloatGen(rtOperand64, rmReg, ftype, 1, 0, top);

                context.Arm64Assembler.Lsr(rt2Operand64, rtOperand64, InstEmitCommon.Const(32));
                context.Arm64Assembler.Mov(rtOperand, rtOperand); // Zero-extend.
            }
            else
            {
                Operand rtOperand = InstEmitCommon.GetInputGpr(context, rt);
                Operand rt2Operand = InstEmitCommon.GetInputGpr(context, rt2);

                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                Operand tempRegister64 = new(OperandKind.Register, OperandType.I64, tempRegister.Operand.Value);

                context.Arm64Assembler.Lsl(tempRegister64, rt2Operand, InstEmitCommon.Const(32));
                context.Arm64Assembler.Orr(tempRegister64, tempRegister64, rtOperand);

                if (top == 0)
                {
                    // Doing FMOV on Rm directly would clear the high bits if we are moving to the bottom.

                    using ScopedRegister tempRegister2 = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                    context.Arm64Assembler.FmovFloatGen(tempRegister2.Operand, tempRegister64, ftype, 1, 1, top);

                    InstEmitNeonCommon.InsertResult(context, tempRegister2.Operand, rm, false);
                }
                else
                {
                    context.Arm64Assembler.FmovFloatGen(rmReg, tempRegister64, ftype, 1, 1, top);
                }
            }
        }

        public static void VmovH(CodeGenContext context, uint rt, uint rn, bool op)
        {
            if (op)
            {
                Operand rtOperand = InstEmitCommon.GetOutputGpr(context, rt);

                using ScopedRegister tempRegister = InstEmitNeonCommon.MoveScalarToSide(context, rn, true);

                context.Arm64Assembler.FmovFloatGen(rtOperand, tempRegister.Operand, 3, 0, 0, 0);
            }
            else
            {
                Operand rtOperand = InstEmitCommon.GetInputGpr(context, rt);

                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                context.Arm64Assembler.FmovFloatGen(tempRegister.Operand, rtOperand, 3, 0, 1, 0);

                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rn, true);
            }
        }

        public static void VmovI(CodeGenContext context, uint rd, uint op, uint cmode, uint imm8, uint q)
        {
            (uint a, uint b, uint c, uint d, uint e, uint f, uint g, uint h) = Split(imm8);

            if (q == 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                context.Arm64Assembler.Movi(tempRegister.Operand, h, g, f, e, d, cmode, c, b, a, op, q);

                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));

                context.Arm64Assembler.Movi(rdOperand, h, g, f, e, d, cmode, c, b, a, op, q);
            }
        }

        public static void VmovFI(CodeGenContext context, uint rd, uint imm8, uint size)
        {
            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

            context.Arm64Assembler.FmovFloatImm(tempRegister.Operand, imm8, size ^ 2u);

            InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, size != 3);
        }

        public static void VmovR(CodeGenContext context, uint rd, uint rm, uint size)
        {
            bool singleRegister = size == 2;

            int shift = singleRegister ? 2 : 1;
            uint mask = singleRegister ? 3u : 1u;
            uint dstElt = rd & mask;
            uint srcElt = rm & mask;

            uint imm4 = srcElt << (singleRegister ? 2 : 3);
            uint imm5 = InstEmitNeonCommon.GetImm5ForElementIndex(dstElt, singleRegister);

            Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> shift));
            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> shift));

            context.Arm64Assembler.InsElt(rdOperand, rmOperand, imm4, imm5);
        }

        public static void VmovRs(CodeGenContext context, uint rd, uint rt, uint opc1, uint opc2)
        {
            uint index;
            uint size;

            if ((opc1 & 2u) != 0)
            {
                index = opc2 | ((opc1 & 1u) << 2);
                size = 0;
            }
            else if ((opc2 & 1u) != 0)
            {
                index = (opc2 >> 1) | ((opc1 & 1u) << 1);
                size = 1;
            }
            else
            {
                Debug.Assert(opc1 == 0 || opc1 == 1);
                Debug.Assert(opc2 == 0);

                index = opc1 & 1u;
                size = 2;
            }

            index |= (rd & 1u) << (int)(3 - size);

            Operand rtOperand = InstEmitCommon.GetInputGpr(context, rt);

            Operand rdReg = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));

            context.Arm64Assembler.InsGen(rdReg, rtOperand, InstEmitNeonCommon.GetImm5ForElementIndex(index, size));
        }

        public static void VmovS(CodeGenContext context, uint rt, uint rn, bool op)
        {
            if (op)
            {
                Operand rtOperand = InstEmitCommon.GetOutputGpr(context, rt);

                using ScopedRegister tempRegister = InstEmitNeonCommon.MoveScalarToSide(context, rn, true);

                context.Arm64Assembler.FmovFloatGen(rtOperand, tempRegister.Operand, 0, 0, 0, 0);
            }
            else
            {
                Operand rtOperand = InstEmitCommon.GetInputGpr(context, rt);

                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                context.Arm64Assembler.FmovFloatGen(tempRegister.Operand, rtOperand, 0, 0, 1, 0);

                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rn, true);
            }
        }

        public static void VmovSr(CodeGenContext context, uint rt, uint rn, bool u, uint opc1, uint opc2)
        {
            uint index;
            uint size;

            if ((opc1 & 2u) != 0)
            {
                index = opc2 | ((opc1 & 1u) << 2);
                size = 0;
            }
            else if ((opc2 & 1u) != 0)
            {
                index = (opc2 >> 1) | ((opc1 & 1u) << 1);
                size = 1;
            }
            else
            {
                Debug.Assert(opc1 == 0 || opc1 == 1);
                Debug.Assert(opc2 == 0);
                Debug.Assert(!u);

                index = opc1 & 1u;
                size = 2;
            }

            index |= (rn & 1u) << (int)(3 - size);

            Operand rtOperand = InstEmitCommon.GetOutputGpr(context, rt);

            Operand rnReg = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));

            if (u || size > 1)
            {
                context.Arm64Assembler.Umov(rtOperand, rnReg, (int)index, (int)size);
            }
            else
            {
                context.Arm64Assembler.Smov(rtOperand, rnReg, (int)index, (int)size);
            }
        }

        public static void VmovSs(CodeGenContext context, uint rt, uint rt2, uint rm, bool op)
        {
            if ((rm & 1) == 0)
            {
                // If we are moving an aligned pair of single-precision registers,
                // we can just move a single double-precision register.

                VmovD(context, rt, rt2, rm >> 1, op);

                return;
            }

            if (op)
            {
                Operand rtOperand = InstEmitCommon.GetOutputGpr(context, rt);
                Operand rt2Operand = InstEmitCommon.GetOutputGpr(context, rt2);

                using ScopedRegister rmReg = InstEmitNeonCommon.MoveScalarToSide(context, rm, true);
                using ScopedRegister rmReg2 = InstEmitNeonCommon.MoveScalarToSide(context, rm + 1, true);

                context.Arm64Assembler.FmovFloatGen(rtOperand, rmReg.Operand, 0, 0, 0, 0);
                context.Arm64Assembler.FmovFloatGen(rt2Operand, rmReg2.Operand, 0, 0, 0, 0);
            }
            else
            {
                Operand rtOperand = InstEmitCommon.GetInputGpr(context, rt);
                Operand rt2Operand = InstEmitCommon.GetInputGpr(context, rt2);

                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                context.Arm64Assembler.FmovFloatGen(tempRegister.Operand, rtOperand, 0, 0, 1, 0);
                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rm, true);

                context.Arm64Assembler.FmovFloatGen(tempRegister.Operand, rt2Operand, 0, 0, 1, 0);
                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rm + 1, true);
            }
        }

        public static void VmvnI(CodeGenContext context, uint rd, uint cmode, uint imm8, uint q)
        {
            (uint a, uint b, uint c, uint d, uint e, uint f, uint g, uint h) = Split(imm8);

            if (q == 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                context.Arm64Assembler.Mvni(tempRegister.Operand, h, g, f, e, d, cmode, c, b, a, q);

                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));

                context.Arm64Assembler.Mvni(rdOperand, h, g, f, e, d, cmode, c, b, a, q);
            }
        }

        public static void VmvnR(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            InstEmitNeonCommon.EmitVectorUnary(context, rd, rm, q, context.Arm64Assembler.Not);
        }

        public static void Vswp(CodeGenContext context, uint rd, uint rm, uint q)
        {
            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

            if (q == 0)
            {
                InstEmitNeonCommon.MoveScalarToSide(context, tempRegister.Operand, rd, false);
                using ScopedRegister rmReg = InstEmitNeonCommon.MoveScalarToSide(context, rm, false);

                InstEmitNeonCommon.InsertResult(context, rmReg.Operand, rd, false);
                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rm, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                context.Arm64Assembler.Orr(tempRegister.Operand, rdOperand, rdOperand); // Temp = Rd
                context.Arm64Assembler.Orr(rdOperand, rmOperand, rmOperand); // Rd = Rm
                context.Arm64Assembler.Orr(rmOperand, tempRegister.Operand, tempRegister.Operand); // Rm = Temp
            }
        }

        public static void Vtbl(CodeGenContext context, uint rd, uint rn, uint rm, bool op, uint len)
        {
            // On AArch64, TBL/TBX works with 128-bit vectors, while on AArch32 it works with 64-bit vectors.
            // We must combine the 64-bit vectors into a larger 128-bit one in some cases.

            // TODO: Peephole optimization to combine adjacent TBL instructions?

            Debug.Assert(len <= 3);

            bool isTbl = !op;

            len = Math.Min(len, 31 - rn);

            bool rangeMismatch = !isTbl && (len & 1) == 0;

            using ScopedRegister indicesReg = InstEmitNeonCommon.MoveScalarToSide(context, rm, false, rangeMismatch);

            if (rangeMismatch)
            {
                // Force any index >= 8 * regs to be the maximum value, since on AArch64 we are working with a full vector,
                // and the out of range value is 16 * regs, not 8 * regs.

                Debug.Assert(indicesReg.IsAllocated);

                using ScopedRegister tempRegister2 = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                if (len == 0)
                {
                    (uint immb, uint immh) = InstEmitNeonCommon.GetImmbImmhForShift(3, 0, isShl: false);

                    context.Arm64Assembler.UshrV(tempRegister2.Operand, indicesReg.Operand, immb, immh, 0);
                    context.Arm64Assembler.CmeqZeroV(tempRegister2.Operand, tempRegister2.Operand, 0, 0);
                    context.Arm64Assembler.Orn(indicesReg.Operand, indicesReg.Operand, tempRegister2.Operand, 0);
                }
                else
                {
                    (uint a, uint b, uint c, uint d, uint e, uint f, uint g, uint h) = Split(8u * (len + 1));

                    context.Arm64Assembler.Movi(tempRegister2.Operand, h, g, f, e, d, 0xe, c, b, a, 0, 0);
                    context.Arm64Assembler.CmgeRegV(tempRegister2.Operand, indicesReg.Operand, tempRegister2.Operand, 0, 0);
                    context.Arm64Assembler.OrrReg(indicesReg.Operand, indicesReg.Operand, tempRegister2.Operand, 0);
                }
            }

            ScopedRegister tableReg1 = default;
            ScopedRegister tableReg2 = default;

            switch (len)
            {
                case 0:
                    tableReg1 = MoveHalfToSideZeroUpper(context, rn);
                    break;
                case 1:
                    tableReg1 = MoveDoublewords(context, rn, rn + 1);
                    break;
                case 2:
                    tableReg1 = MoveDoublewords(context, rn, rn + 1, isOdd: true);
                    tableReg2 = MoveHalfToSideZeroUpper(context, rn + 2);
                    break;
                case 3:
                    tableReg1 = MoveDoublewords(context, rn, rn + 1);
                    tableReg2 = MoveDoublewords(context, rn + 2, rn + 3);
                    break;
            }

            // TBL works with consecutive registers, it is assumed that two consecutive calls to the register allocator
            // will return consecutive registers.

            Debug.Assert(len < 2 || tableReg1.Operand.GetRegister().Index + 1 == tableReg2.Operand.GetRegister().Index);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

            if (isTbl)
            {
                context.Arm64Assembler.Tbl(tempRegister.Operand, tableReg1.Operand, len >> 1, indicesReg.Operand, 0);
            }
            else
            {
                InstEmitNeonCommon.MoveScalarToSide(context, tempRegister.Operand, rd, false);

                context.Arm64Assembler.Tbx(tempRegister.Operand, tableReg1.Operand, len >> 1, indicesReg.Operand, 0);
            }

            InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, false);

            tableReg1.Dispose();

            if (len > 1)
            {
                tableReg2.Dispose();
            }
        }

        public static void Vtrn(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            EmitVectorBinaryInterleavedTrn(context, rd, rm, size, q, context.Arm64Assembler.Trn1, context.Arm64Assembler.Trn2);
        }

        public static void Vuzp(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            EmitVectorBinaryInterleaved(context, rd, rm, size, q, context.Arm64Assembler.Uzp1, context.Arm64Assembler.Uzp2);
        }

        public static void Vzip(CodeGenContext context, uint rd, uint rm, uint size, uint q)
        {
            EmitVectorBinaryInterleaved(context, rd, rm, size, q, context.Arm64Assembler.Zip1, context.Arm64Assembler.Zip2);
        }

        public static (uint, uint, uint, uint, uint, uint, uint, uint) Split(uint imm8)
        {
            uint a = (imm8 >> 7) & 1;
            uint b = (imm8 >> 6) & 1;
            uint c = (imm8 >> 5) & 1;
            uint d = (imm8 >> 4) & 1;
            uint e = (imm8 >> 3) & 1;
            uint f = (imm8 >> 2) & 1;
            uint g = (imm8 >> 1) & 1;
            uint h = imm8 & 1;

            return (a, b, c, d, e, f, g, h);
        }

        private static ScopedRegister MoveHalfToSideZeroUpper(CodeGenContext context, uint srcReg)
        {
            uint elt = srcReg & 1u;

            Operand source = context.RegisterAllocator.RemapSimdRegister((int)(srcReg >> 1));
            ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempFpRegisterScoped(false);

            uint imm5 = InstEmitNeonCommon.GetImm5ForElementIndex(elt, false);

            context.Arm64Assembler.DupEltScalarFromElement(tempRegister.Operand, source, imm5);

            return tempRegister;
        }

        private static ScopedRegister MoveDoublewords(CodeGenContext context, uint lowerReg, uint upperReg, bool isOdd = false)
        {
            if ((lowerReg & 1) == 0 && upperReg == lowerReg + 1 && !isOdd)
            {
                return new ScopedRegister(context.RegisterAllocator, context.RegisterAllocator.RemapSimdRegister((int)(lowerReg >> 1)), false);
            }

            Operand lowerSrc = context.RegisterAllocator.RemapSimdRegister((int)(lowerReg >> 1));
            Operand upperSrc = context.RegisterAllocator.RemapSimdRegister((int)(upperReg >> 1));
            ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempFpRegisterScoped(false);

            uint imm5 = InstEmitNeonCommon.GetImm5ForElementIndex(lowerReg & 1u, false);

            context.Arm64Assembler.DupEltScalarFromElement(tempRegister.Operand, lowerSrc, imm5);

            imm5 = InstEmitNeonCommon.GetImm5ForElementIndex(1, false);

            context.Arm64Assembler.InsElt(tempRegister.Operand, upperSrc, (upperReg & 1u) << 3, imm5);

            return tempRegister;
        }

        private static void EmitVectorBinaryInterleavedTrn(
            CodeGenContext context,
            uint rd,
            uint rm,
            uint size,
            uint q,
            Action<Operand, Operand, Operand, uint, uint> action1,
            Action<Operand, Operand, Operand, uint, uint> action2)
        {
            if (rd == rm)
            {
                // The behaviour when the registers are the same is "unpredictable" according to the manual.

                if (q == 0)
                {
                    using ScopedRegister rdReg = InstEmitNeonCommon.MoveScalarToSide(context, rd, false);
                    using ScopedRegister rmReg = InstEmitNeonCommon.MoveScalarToSide(context, rm, false);

                    using ScopedRegister tempRegister1 = context.RegisterAllocator.AllocateTempSimdRegisterScoped();
                    using ScopedRegister tempRegister2 = InstEmitNeonCommon.PickSimdRegister(context.RegisterAllocator, rdReg, rmReg);

                    action1(tempRegister1.Operand, rdReg.Operand, rmReg.Operand, size, q);
                    action2(tempRegister2.Operand, rdReg.Operand, tempRegister1.Operand, size, q);

                    InstEmitNeonCommon.InsertResult(context, tempRegister2.Operand, rd, false);
                }
                else
                {
                    Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                    Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                    using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                    action1(tempRegister.Operand, rdOperand, rmOperand, size, q);
                    action2(rmOperand, rdOperand, tempRegister.Operand, size, q);
                }
            }
            else
            {
                EmitVectorBinaryInterleaved(context, rd, rm, size, q, action1, action2);
            }
        }

        private static void EmitVectorBinaryInterleaved(
            CodeGenContext context,
            uint rd,
            uint rm,
            uint size,
            uint q,
            Action<Operand, Operand, Operand, uint, uint> action1,
            Action<Operand, Operand, Operand, uint, uint> action2)
        {
            if (q == 0)
            {
                using ScopedRegister rdReg = InstEmitNeonCommon.MoveScalarToSide(context, rd, false);
                using ScopedRegister rmReg = InstEmitNeonCommon.MoveScalarToSide(context, rm, false);

                using ScopedRegister tempRegister1 = context.RegisterAllocator.AllocateTempSimdRegisterScoped();
                using ScopedRegister tempRegister2 = InstEmitNeonCommon.PickSimdRegister(context.RegisterAllocator, rdReg, rmReg);

                action1(tempRegister1.Operand, rdReg.Operand, rmReg.Operand, size, q);
                action2(tempRegister2.Operand, rdReg.Operand, rmReg.Operand, size, q);

                if (rd != rm)
                {
                    InstEmitNeonCommon.InsertResult(context, tempRegister1.Operand, rd, false);
                }

                InstEmitNeonCommon.InsertResult(context, tempRegister2.Operand, rm, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                action1(tempRegister.Operand, rdOperand, rmOperand, size, q);
                action2(rmOperand, rdOperand, rmOperand, size, q);

                if (rd != rm)
                {
                    context.Arm64Assembler.OrrReg(rdOperand, tempRegister.Operand, tempRegister.Operand, 1);
                }
            }
        }
    }
}
