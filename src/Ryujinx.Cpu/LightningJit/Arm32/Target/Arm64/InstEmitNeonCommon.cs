
using Ryujinx.Cpu.LightningJit.CodeGen;
using System;
using System.Diagnostics;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonCommon
    {
        public static ScopedRegister MoveScalarToSide(CodeGenContext context, uint srcReg, bool isFP32, bool forceAllocation = false)
        {
            int shift = isFP32 ? 2 : 1;
            uint mask = isFP32 ? 3u : 1u;
            uint elt = srcReg & mask;

            if (elt == 0 && !forceAllocation)
            {
                return new ScopedRegister(context.RegisterAllocator, context.RegisterAllocator.RemapFpRegister((int)(srcReg >> shift), isFP32), false);
            }

            Operand source = context.RegisterAllocator.RemapSimdRegister((int)(srcReg >> shift));
            ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempFpRegisterScoped(isFP32);

            uint imm5 = GetImm5ForElementIndex(elt, isFP32);

            context.Arm64Assembler.DupEltScalarFromElement(tempRegister.Operand, source, imm5);

            return tempRegister;
        }

        public static ScopedRegister Move16BitScalarToSide(CodeGenContext context, uint srcReg, bool top = false)
        {
            uint elt = srcReg & 3;

            Operand source = context.RegisterAllocator.RemapSimdRegister((int)(srcReg >> 2));
            ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempFpRegisterScoped(true);

            uint imm5 = GetImm5ForElementIndex((elt << 1) | (top ? 1u : 0u), 1);

            context.Arm64Assembler.DupEltScalarFromElement(tempRegister.Operand, source, imm5);

            return tempRegister;
        }

        public static void MoveScalarToSide(CodeGenContext context, Operand dest, uint srcReg, bool isFP32)
        {
            int shift = isFP32 ? 2 : 1;
            uint mask = isFP32 ? 3u : 1u;
            uint elt = srcReg & mask;

            Operand source = context.RegisterAllocator.RemapSimdRegister((int)(srcReg >> shift));

            uint imm5 = GetImm5ForElementIndex(elt, isFP32);

            context.Arm64Assembler.DupEltScalarFromElement(dest, source, imm5);
        }

        public static ScopedRegister MoveScalarToSideIntoGpr(CodeGenContext context, uint srcReg, bool isFP32)
        {
            int shift = isFP32 ? 2 : 1;
            uint mask = isFP32 ? 3u : 1u;
            uint elt = srcReg & mask;

            Operand source = context.RegisterAllocator.RemapSimdRegister((int)(srcReg >> shift));
            ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            context.Arm64Assembler.Umov(tempRegister.Operand, source, (int)elt, isFP32 ? 2 : 3);

            return tempRegister;
        }

        public static void InsertResult(CodeGenContext context, Operand source, uint dstReg, bool isFP32)
        {
            int shift = isFP32 ? 2 : 1;
            uint mask = isFP32 ? 3u : 1u;
            uint elt = dstReg & mask;

            uint imm5 = GetImm5ForElementIndex(elt, isFP32);

            Operand dest = context.RegisterAllocator.RemapSimdRegister((int)(dstReg >> shift));

            context.Arm64Assembler.InsElt(dest, source, 0, imm5);
        }

        public static void Insert16BitResult(CodeGenContext context, Operand source, uint dstReg, bool top = false)
        {
            uint elt = dstReg & 3u;

            uint imm5 = GetImm5ForElementIndex((elt << 1) | (top ? 1u : 0u), 1);

            Operand dest = context.RegisterAllocator.RemapSimdRegister((int)(dstReg >> 2));

            context.Arm64Assembler.InsElt(dest, source, 0, imm5);
        }

        public static void InsertResultFromGpr(CodeGenContext context, Operand source, uint dstReg, bool isFP32)
        {
            int shift = isFP32 ? 2 : 1;
            uint mask = isFP32 ? 3u : 1u;
            uint elt = dstReg & mask;

            uint imm5 = GetImm5ForElementIndex(elt, isFP32);

            Operand dest = context.RegisterAllocator.RemapSimdRegister((int)(dstReg >> shift));

            context.Arm64Assembler.InsGen(dest, source, imm5);
        }

        public static uint GetImm5ForElementIndex(uint elt, bool isFP32)
        {
            return isFP32 ? (4u | (elt << 3)) : (8u | (elt << 4));
        }

        public static uint GetImm5ForElementIndex(uint elt, uint size)
        {
            return (1u << (int)size) | (elt << ((int)size + 1));
        }

        public static void EmitScalarUnaryF(CodeGenContext context, uint rd, uint rm, uint size, Action<Operand, Operand, uint> action)
        {
            Debug.Assert(size == 1 || size == 2 || size == 3);

            bool singleRegs = size != 3;

            using ScopedRegister rmReg = MoveScalarToSide(context, rm, singleRegs);

            using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rmReg);

            action(tempRegister.Operand, rmReg.Operand, size ^ 2u);

            InsertResult(context, tempRegister.Operand, rd, singleRegs);
        }

        public static void EmitScalarUnaryF(CodeGenContext context, uint rd, uint rm, uint size, Action<Operand, Operand, uint> action, Action<Operand, Operand> actionHalf)
        {
            Debug.Assert(size == 1 || size == 2 || size == 3);

            bool singleRegs = size != 3;

            using ScopedRegister rmReg = MoveScalarToSide(context, rm, singleRegs);

            using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rmReg);

            if (size == 1)
            {
                actionHalf(tempRegister.Operand, rmReg.Operand);
            }
            else
            {
                action(tempRegister.Operand, rmReg.Operand, size & 1);
            }

            InsertResult(context, tempRegister.Operand, rd, singleRegs);
        }

        public static void EmitScalarUnaryToGprTempF(
            CodeGenContext context,
            uint rd,
            uint rm,
            uint size,
            uint sf,
            Action<Operand, Operand, uint, uint> action)
        {
            Debug.Assert(size == 1 || size == 2 || size == 3);

            bool singleRegs = size != 3;

            using ScopedRegister rmReg = MoveScalarToSide(context, rm, singleRegs);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            action(tempRegister.Operand, rmReg.Operand, size ^ 2u, sf);

            InsertResultFromGpr(context, tempRegister.Operand, rd, sf == 0);
        }

        public static void EmitScalarUnaryFromGprTempF(
            CodeGenContext context,
            uint rd,
            uint rm,
            uint size,
            uint sf,
            Action<Operand, Operand, uint, uint> action)
        {
            Debug.Assert(size == 1 || size == 2 || size == 3);

            bool singleRegs = size != 3;

            using ScopedRegister rmReg = MoveScalarToSideIntoGpr(context, rm, sf == 0);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

            action(tempRegister.Operand, rmReg.Operand, size ^ 2u, sf);

            InsertResult(context, tempRegister.Operand, rd, singleRegs);
        }

        public static void EmitScalarUnaryFixedF(CodeGenContext context, uint rd, uint rm, uint fbits, uint size, bool is16Bit, Action<Operand, Operand, uint, uint> action)
        {
            Debug.Assert(size == 1 || size == 2 || size == 3);

            bool singleRegs = size != 3;

            (uint immb, uint immh) = GetImmbImmh(fbits, size);

            using ScopedRegister rmReg = is16Bit ? Move16BitScalarToSide(context, rm) : MoveScalarToSide(context, rm, singleRegs);

            using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rmReg);

            action(tempRegister.Operand, rmReg.Operand, immb, immh);

            InsertResult(context, tempRegister.Operand, rd, singleRegs);
        }

        public static void EmitScalarBinaryF(CodeGenContext context, uint rd, uint rn, uint rm, uint size, Action<Operand, Operand, Operand, uint> action)
        {
            Debug.Assert(size == 1 || size == 2 || size == 3);

            bool singleRegs = size != 3;

            using ScopedRegister rnReg = MoveScalarToSide(context, rn, singleRegs);
            using ScopedRegister rmReg = MoveScalarToSide(context, rm, singleRegs);

            using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rnReg, rmReg);

            action(tempRegister.Operand, rnReg.Operand, rmReg.Operand, size ^ 2u);

            InsertResult(context, tempRegister.Operand, rd, singleRegs);
        }

        public static void EmitScalarBinaryShift(
            CodeGenContext context,
            uint rd,
            uint rm,
            uint shift,
            uint size,
            bool isShl,
            Action<Operand, Operand, uint, uint> action)
        {
            bool singleRegs = size != 3;

            (uint immb, uint immh) = GetImmbImmhForShift(shift, size, isShl);

            using ScopedRegister rmReg = MoveScalarToSide(context, rm, singleRegs);

            using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rmReg);

            action(tempRegister.Operand, rmReg.Operand, immb, immh);

            InsertResult(context, tempRegister.Operand, rd, singleRegs);
        }

        public static void EmitScalarTernaryRdF(CodeGenContext context, uint rd, uint rn, uint rm, uint size, Action<Operand, Operand, Operand, uint> action)
        {
            Debug.Assert(size == 1 || size == 2 || size == 3);

            bool singleRegs = size != 3;

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

            MoveScalarToSide(context, tempRegister.Operand, rd, singleRegs);

            using ScopedRegister rnReg = MoveScalarToSide(context, rn, singleRegs);
            using ScopedRegister rmReg = MoveScalarToSide(context, rm, singleRegs);

            action(tempRegister.Operand, rnReg.Operand, rmReg.Operand, size ^ 2u);

            InsertResult(context, tempRegister.Operand, rd, singleRegs);
        }

        public static void EmitScalarTernaryRdF(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint size,
            Action<Operand, Operand, Operand, Operand, uint> action)
        {
            Debug.Assert(size == 1 || size == 2 || size == 3);

            bool singleRegs = size != 3;

            using ScopedRegister rdReg = MoveScalarToSide(context, rd, singleRegs);
            using ScopedRegister rnReg = MoveScalarToSide(context, rn, singleRegs);
            using ScopedRegister rmReg = MoveScalarToSide(context, rm, singleRegs);

            using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rdReg, rnReg, rmReg);

            action(tempRegister.Operand, rnReg.Operand, rmReg.Operand, rdReg.Operand, size ^ 2u);

            InsertResult(context, tempRegister.Operand, rd, singleRegs);
        }

        public static void EmitScalarTernaryMulNegRdF(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint size,
            bool negD,
            bool negProduct)
        {
            Debug.Assert(size == 1 || size == 2 || size == 3);

            bool singleRegs = size != 3;

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

            MoveScalarToSide(context, tempRegister.Operand, rd, singleRegs);

            using ScopedRegister rnReg = MoveScalarToSide(context, rn, singleRegs);
            using ScopedRegister rmReg = MoveScalarToSide(context, rm, singleRegs);

            using ScopedRegister productRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

            uint ftype = size ^ 2u;

            context.Arm64Assembler.FmulFloat(productRegister.Operand, rnReg.Operand, rmReg.Operand, ftype);

            if (negD)
            {
                context.Arm64Assembler.FnegFloat(tempRegister.Operand, tempRegister.Operand, ftype);
            }

            if (negProduct)
            {
                context.Arm64Assembler.FnegFloat(productRegister.Operand, productRegister.Operand, ftype);
            }

            context.Arm64Assembler.FaddFloat(tempRegister.Operand, tempRegister.Operand, productRegister.Operand, ftype);

            InsertResult(context, tempRegister.Operand, rd, singleRegs);
        }

        public static void EmitVectorUnary(CodeGenContext context, uint rd, uint rm, Action<Operand, Operand> action)
        {
            Debug.Assert(((rd | rm) & 1) == 0);

            Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            action(rdOperand, rmOperand);
        }

        public static void EmitVectorUnary(CodeGenContext context, uint rd, uint rm, uint q, Action<Operand, Operand, uint> action)
        {
            if (q == 0)
            {
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rmReg);

                action(tempRegister.Operand, rmReg.Operand, q);

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                action(rdOperand, rmOperand, q);
            }
        }

        public static void EmitVectorUnary(CodeGenContext context, uint rd, uint rm, uint size, uint q, Action<Operand, Operand, uint, uint> action)
        {
            Debug.Assert(size < 3);

            if (q == 0)
            {
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rmReg);

                action(tempRegister.Operand, rmReg.Operand, size, q);

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                action(rdOperand, rmOperand, size, q);
            }
        }

        public static void EmitVectorUnaryLong(CodeGenContext context, uint rd, uint rm, uint size, Action<Operand, Operand, uint, uint> action)
        {
            Debug.Assert((rd & 1) == 0);

            Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            uint q = rm & 1;

            action(rdOperand, rmOperand, size, q);
        }

        public static void EmitVectorUnaryNarrow(CodeGenContext context, uint rd, uint rm, uint size, Action<Operand, Operand, uint, uint> action)
        {
            Debug.Assert((rm & 1) == 0);

            Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            uint q = rd & 1;

            if (q == 0)
            {
                // Writing to the lower half would clear the higher bits, we don't want that, so use a temp register and move the element.

                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                action(tempRegister.Operand, rmOperand, size, q);

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                action(rdOperand, rmOperand, size, q);
            }
        }

        public static void EmitVectorBinary(CodeGenContext context, uint rd, uint rn, uint rm, Action<Operand, Operand, Operand> action)
        {
            Debug.Assert(((rd | rn | rm) & 1) == 0);

            Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
            Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            action(rdOperand, rnOperand, rmOperand);
        }

        public static void EmitVectorBinary(CodeGenContext context, uint rd, uint rn, uint rm, uint q, Action<Operand, Operand, Operand, uint> action)
        {
            if (q == 0)
            {
                using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rnReg, rmReg);

                action(tempRegister.Operand, rnReg.Operand, rmReg.Operand, q);

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                action(rdOperand, rnOperand, rmOperand, q);
            }
        }

        public static void EmitVectorBinary(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint size,
            uint q,
            Action<Operand, Operand, Operand, uint, uint> action,
            Action<Operand, Operand, Operand, uint> actionScalar)
        {
            Debug.Assert(size <= 3);

            if (q == 0)
            {
                using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rnReg, rmReg);

                if (size == 3)
                {
                    actionScalar(tempRegister.Operand, rnReg.Operand, rmReg.Operand, size);
                }
                else
                {
                    action(tempRegister.Operand, rnReg.Operand, rmReg.Operand, size, q);
                }

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                action(rdOperand, rnOperand, rmOperand, size, q);
            }
        }

        public static void EmitVectorBinaryRd(CodeGenContext context, uint rd, uint rm, uint size, uint q, Action<Operand, Operand, uint, uint> action)
        {
            Debug.Assert(size < 3);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

            MoveScalarToSide(context, tempRegister.Operand, rd, false);

            using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

            action(tempRegister.Operand, rmReg.Operand, size, q);

            InsertResult(context, tempRegister.Operand, rd, false);
        }

        public static void EmitVectorBinaryShift(
            CodeGenContext context,
            uint rd,
            uint rm,
            uint shift,
            uint size,
            uint q,
            bool isShl,
            Action<Operand, Operand, uint, uint, uint> action,
            Action<Operand, Operand, uint, uint> actionScalar)
        {
            (uint immb, uint immh) = GetImmbImmhForShift(shift, size, isShl);

            if (q == 0)
            {
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rmReg);

                if (size == 3)
                {
                    actionScalar(tempRegister.Operand, rmReg.Operand, immb, immh);
                }
                else
                {
                    action(tempRegister.Operand, rmReg.Operand, immb, immh, q);
                }

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                action(rdOperand, rmOperand, immb, immh, q);
            }
        }

        public static void EmitVectorBinaryLong(CodeGenContext context, uint rd, uint rn, uint rm, uint size, Action<Operand, Operand, Operand, uint, uint> action)
        {
            Debug.Assert((rd & 1) == 0);

            Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));

            if ((rn & 1) == (rm & 1))
            {
                // Both inputs are on the same side of the vector, so we can use the variant that selects the half.

                Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                uint q = rn & 1;

                action(rdOperand, rnOperand, rmOperand, size, q);
            }
            else
            {
                // Inputs are on different sides of the vector, we have to move them.

                using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                action(rdOperand, rnReg.Operand, rmReg.Operand, size, 0);
            }
        }

        public static void EmitVectorBinaryLongShift(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint shift,
            uint size,
            bool isShl,
            Action<Operand, Operand, uint, uint, uint> action)
        {
            (uint immb, uint immh) = GetImmbImmhForShift(shift, size, isShl);

            Debug.Assert((rd & 1) == 0);

            Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
            Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));

            uint q = rn & 1;

            action(rdOperand, rnOperand, immb, immh, q);
        }

        public static void EmitVectorBinaryLongByScalar(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint size,
            Action<Operand, Operand, uint, Operand, uint, uint, uint, uint> action)
        {
            Debug.Assert((rd & 1) == 0);

            (uint h, uint l, uint m) = GetIndexForReg(ref rm, size);

            Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
            Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            uint q = rn & 1;

            action(rdOperand, rnOperand, h, rmOperand, m, l, size, q);
        }

        public static void EmitVectorBinaryNarrow(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint size,
            Action<Operand, Operand, Operand, uint, uint> action)
        {
            Debug.Assert((rn & 1) == 0);
            Debug.Assert((rm & 1) == 0);

            Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
            Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            uint q = rd & 1;

            if (q == 0)
            {
                // Writing to the lower half would clear the higher bits, we don't want that, so use a temp register and move the element.

                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                action(tempRegister.Operand, rnOperand, rmOperand, size, q);

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                action(rdOperand, rnOperand, rmOperand, size, q);
            }
        }

        public static void EmitVectorBinaryNarrowShift(
            CodeGenContext context,
            uint rd,
            uint rm,
            uint shift,
            uint size,
            bool isShl,
            Action<Operand, Operand, uint, uint, uint> action)
        {
            (uint immb, uint immh) = GetImmbImmhForShift(shift, size, isShl);

            Debug.Assert((rm & 1) == 0);

            Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            uint q = rd & 1;

            if (q == 0)
            {
                // Writing to the lower half would clear the higher bits, we don't want that, so use a temp register and move the element.

                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                action(tempRegister.Operand, rmOperand, immb, immh, q);

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                action(rdOperand, rmOperand, immb, immh, q);
            }
        }

        public static void EmitVectorBinaryWide(CodeGenContext context, uint rd, uint rn, uint rm, uint size, Action<Operand, Operand, Operand, uint, uint> action)
        {
            Debug.Assert(((rd | rn) & 1) == 0);

            Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
            Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            uint q = rm & 1;

            action(rdOperand, rnOperand, rmOperand, size, q);
        }

        public static void EmitVectorBinaryByScalar(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint size,
            uint q,
            Action<Operand, Operand, uint, Operand, uint, uint, uint, uint> action)
        {
            EmitVectorByScalarCore(context, rd, rn, rm, size, q, action, isTernary: false);
        }

        public static void EmitVectorTernaryRd(CodeGenContext context, uint rd, uint rn, uint rm, uint q, Action<Operand, Operand, Operand, uint> action)
        {
            if (q == 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                MoveScalarToSide(context, tempRegister.Operand, rd, false);

                using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                action(tempRegister.Operand, rnReg.Operand, rmReg.Operand, q);

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                action(rdOperand, rnOperand, rmOperand, q);
            }
        }

        public static void EmitVectorTernaryRd(CodeGenContext context, uint rd, uint rn, uint rm, uint size, uint q, Action<Operand, Operand, Operand, uint, uint> action)
        {
            if (q == 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                MoveScalarToSide(context, tempRegister.Operand, rd, false);

                using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                action(tempRegister.Operand, rnReg.Operand, rmReg.Operand, size, q);

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                action(rdOperand, rnOperand, rmOperand, size, q);
            }
        }

        public static void EmitVectorTernaryRdLong(CodeGenContext context, uint rd, uint rn, uint rm, uint size, Action<Operand, Operand, Operand, uint, uint> action)
        {
            Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));

            if ((rn & 1) == (rm & 1))
            {
                // Both inputs are on the same side of the vector, so we can use the variant that selects the half.

                Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                uint q = rn & 1;

                action(rdOperand, rnOperand, rmOperand, size, q);
            }
            else
            {
                // Inputs are on different sides of the vector, we have to move them.

                using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                action(rdOperand, rnReg.Operand, rmReg.Operand, size, 0);
            }
        }

        public static void EmitVectorTernaryRdLongByScalar(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint size,
            Action<Operand, Operand, uint, Operand, uint, uint, uint, uint> action)
        {
            (uint h, uint l, uint m) = GetIndexForReg(ref rm, size);

            Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
            Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            uint q = rn & 1;

            action(rdOperand, rnOperand, h, rmOperand, m, l, size, q);
        }

        public static void EmitVectorTernaryRdShift(
            CodeGenContext context,
            uint rd,
            uint rm,
            uint shift,
            uint size,
            uint q,
            bool isShl,
            Action<Operand, Operand, uint, uint, uint> action,
            Action<Operand, Operand, uint, uint> actionScalar)
        {
            (uint immb, uint immh) = GetImmbImmhForShift(shift, size, isShl);

            if (q == 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                MoveScalarToSide(context, tempRegister.Operand, rd, false);

                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                if (size == 3)
                {
                    actionScalar(tempRegister.Operand, rmReg.Operand, immb, immh);
                }
                else
                {
                    action(tempRegister.Operand, rmReg.Operand, immb, immh, q);
                }

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                action(rdOperand, rmOperand, immb, immh, q);
            }
        }

        public static void EmitVectorTernaryRdByScalar(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint size,
            uint q,
            Action<Operand, Operand, uint, Operand, uint, uint, uint, uint> action)
        {
            EmitVectorByScalarCore(context, rd, rn, rm, size, q, action, isTernary: true);
        }

        private static void EmitVectorByScalarCore(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint size,
            uint q,
            Action<Operand, Operand, uint, Operand, uint, uint, uint, uint> action,
            bool isTernary)
        {
            (uint h, uint l, uint m) = GetIndexForReg(ref rm, size);

            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            if (q == 0)
            {
                if (isTernary)
                {
                    using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                    MoveScalarToSide(context, tempRegister.Operand, rd, false);

                    using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);

                    action(tempRegister.Operand, rnReg.Operand, h, rmOperand, m, l, size, q);

                    InsertResult(context, tempRegister.Operand, rd, false);
                }
                else
                {
                    using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);

                    using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rnReg);

                    action(tempRegister.Operand, rnReg.Operand, h, rmOperand, m, l, size, q);

                    InsertResult(context, tempRegister.Operand, rd, false);
                }
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));

                action(rdOperand, rnOperand, h, rmOperand, m, l, size, q);
            }
        }

        public static void EmitVectorUnaryF(
            CodeGenContext context,
            uint rd,
            uint rm,
            uint sz,
            uint q,
            Action<Operand, Operand, uint, uint> action,
            Action<Operand, Operand, uint> actionHalf)
        {
            Debug.Assert(sz == 0 || sz == 1);

            if (q == 0)
            {
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rmReg);

                if (sz == 1)
                {
                    actionHalf(tempRegister.Operand, rmReg.Operand, q);
                }
                else
                {
                    action(tempRegister.Operand, rmReg.Operand, 0, q);
                }

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                if (sz == 1)
                {
                    actionHalf(rdOperand, rmOperand, q);
                }
                else
                {
                    action(rdOperand, rmOperand, 0, q);
                }
            }
        }

        public static void EmitVectorUnaryAnyF(
            CodeGenContext context,
            uint rd,
            uint rm,
            uint size,
            uint q,
            Action<Operand, Operand, uint, uint> action,
            Action<Operand, Operand, uint> actionHalf)
        {
            Debug.Assert(size == 1 || size == 2 || size == 3);
            Debug.Assert(size != 3 || q == 1);

            if (q == 0)
            {
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rmReg);

                if (size == 1)
                {
                    actionHalf(tempRegister.Operand, rmReg.Operand, q);
                }
                else
                {
                    action(tempRegister.Operand, rmReg.Operand, size ^ 2u, q);
                }

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                if (size == 1)
                {
                    actionHalf(rdOperand, rmOperand, q);
                }
                else
                {
                    action(rdOperand, rmOperand, size ^ 2u, q);
                }
            }
        }

        public static void EmitVectorUnaryFixedAnyF(
            CodeGenContext context,
            uint rd,
            uint rm,
            uint fbits,
            uint size,
            uint q,
            Action<Operand, Operand, uint, uint, uint> action)
        {
            Debug.Assert(size == 1 || size == 2 || size == 3);
            Debug.Assert(size != 3 || q == 1);

            (uint immb, uint immh) = GetImmbImmh(fbits, size);

            if (q == 0)
            {
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);
                using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rmReg);

                action(tempRegister.Operand, rmReg.Operand, immb, immh, q);

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                action(rdOperand, rmOperand, immb, immh, q);
            }
        }

        public static void EmitVectorBinaryF(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint sz,
            uint q,
            Action<Operand, Operand, Operand, uint, uint> action,
            Action<Operand, Operand, Operand, uint> actionHalf)
        {
            Debug.Assert(sz == 0 || sz == 1);

            if (q == 0)
            {
                using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rnReg, rmReg);

                if (sz == 1)
                {
                    actionHalf(tempRegister.Operand, rnReg.Operand, rmReg.Operand, q);
                }
                else
                {
                    action(tempRegister.Operand, rnReg.Operand, rmReg.Operand, 0, q);
                }

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                if (sz == 1)
                {
                    actionHalf(rdOperand, rnOperand, rmOperand, q);
                }
                else
                {
                    action(rdOperand, rnOperand, rmOperand, 0, q);
                }
            }
        }

        public static void EmitVectorBinaryByScalarAnyF(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint size,
            uint q,
            Action<Operand, Operand, uint, Operand, uint, uint, uint, uint> action,
            Action<Operand, Operand, uint, Operand, uint, uint, uint> actionHalf)
        {
            EmitVectorByScalarAnyFCore(context, rd, rn, rm, size, q, action, actionHalf, isTernary: false);
        }

        public static void EmitVectorTernaryRdF(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint sz,
            uint q,
            Action<Operand, Operand, Operand, uint, uint> action,
            Action<Operand, Operand, Operand, uint> actionHalf)
        {
            Debug.Assert(sz == 0 || sz == 1);

            if (q == 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                MoveScalarToSide(context, tempRegister.Operand, rd, false);

                using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                if (sz == 1)
                {
                    actionHalf(tempRegister.Operand, rnReg.Operand, rmReg.Operand, q);
                }
                else
                {
                    action(tempRegister.Operand, rnReg.Operand, rmReg.Operand, 0, q);
                }

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                if (sz == 1)
                {
                    actionHalf(rdOperand, rnOperand, rmOperand, q);
                }
                else
                {
                    action(rdOperand, rnOperand, rmOperand, 0, q);
                }
            }
        }

        public static void EmitVectorTernaryMulNegRdF(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint sz,
            uint q,
            bool negProduct)
        {
            Debug.Assert(sz == 0 || sz == 1);

            if (q == 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                MoveScalarToSide(context, tempRegister.Operand, rd, false);

                using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);
                using ScopedRegister rmReg = MoveScalarToSide(context, rm, false);

                EmitMulNegVector(context, tempRegister.Operand, rnReg.Operand, rmReg.Operand, sz, q, negProduct);

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));
                Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

                EmitMulNegVector(context, rdOperand, rnOperand, rmOperand, sz, q, negProduct);
            }
        }

        private static void EmitMulNegVector(
            CodeGenContext context,
            Operand rd,
            Operand rn,
            Operand rm,
            uint sz,
            uint q,
            bool negProduct)
        {
            using ScopedRegister productRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

            if (sz == 1)
            {
                context.Arm64Assembler.FmulVecHalf(productRegister.Operand, rn, rm, q);

                if (negProduct)
                {
                    context.Arm64Assembler.FnegHalf(productRegister.Operand, productRegister.Operand, q);
                }

                context.Arm64Assembler.FaddHalf(rd, rd, productRegister.Operand, q);
            }
            else
            {
                context.Arm64Assembler.FmulVecSingleAndDouble(productRegister.Operand, rn, rm, 0, q);

                if (negProduct)
                {
                    context.Arm64Assembler.FnegSingleAndDouble(productRegister.Operand, productRegister.Operand, 0, q);
                }

                context.Arm64Assembler.FaddSingleAndDouble(rd, rd, productRegister.Operand, 0, q);
            }
        }

        public static void EmitVectorTernaryRdByScalarAnyF(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint size,
            uint q,
            Action<Operand, Operand, uint, Operand, uint, uint, uint, uint> action,
            Action<Operand, Operand, uint, Operand, uint, uint, uint> actionHalf)
        {
            EmitVectorByScalarAnyFCore(context, rd, rn, rm, size, q, action, actionHalf, isTernary: true);
        }

        private static void EmitVectorByScalarAnyFCore(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint size,
            uint q,
            Action<Operand, Operand, uint, Operand, uint, uint, uint, uint> action,
            Action<Operand, Operand, uint, Operand, uint, uint, uint> actionHalf,
            bool isTernary)
        {
            (uint h, uint l, uint m) = GetIndexForReg(ref rm, size);

            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            if (q == 0)
            {
                if (isTernary)
                {
                    using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                    MoveScalarToSide(context, tempRegister.Operand, rd, false);

                    using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);

                    if (size == 1)
                    {
                        actionHalf(tempRegister.Operand, rnReg.Operand, h, rmOperand, m, l, q);
                    }
                    else
                    {
                        action(tempRegister.Operand, rnReg.Operand, h, rmOperand, m, l, 0, q);
                    }

                    InsertResult(context, tempRegister.Operand, rd, false);
                }
                else
                {
                    using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);

                    using ScopedRegister tempRegister = PickSimdRegister(context.RegisterAllocator, rnReg);

                    if (size == 1)
                    {
                        actionHalf(tempRegister.Operand, rnReg.Operand, h, rmOperand, m, l, q);
                    }
                    else
                    {
                        action(tempRegister.Operand, rnReg.Operand, h, rmOperand, m, l, 0, q);
                    }

                    InsertResult(context, tempRegister.Operand, rd, false);
                }
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));

                if (size == 1)
                {
                    actionHalf(rdOperand, rnOperand, h, rmOperand, m, l, q);
                }
                else
                {
                    action(rdOperand, rnOperand, h, rmOperand, m, l, 0, q);
                }
            }
        }

        public static void EmitVectorTernaryMulNegRdByScalarAnyF(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint rm,
            uint size,
            uint q,
            bool negProduct)
        {
            (uint h, uint l, uint m) = GetIndexForReg(ref rm, size);

            Operand rmOperand = context.RegisterAllocator.RemapSimdRegister((int)(rm >> 1));

            if (q == 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

                MoveScalarToSide(context, tempRegister.Operand, rd, false);

                using ScopedRegister rnReg = MoveScalarToSide(context, rn, false);

                EmitMulNegVectorByScalar(context, tempRegister.Operand, rnReg.Operand, rmOperand, h, l, m, size, q, negProduct);

                InsertResult(context, tempRegister.Operand, rd, false);
            }
            else
            {
                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1));
                Operand rnOperand = context.RegisterAllocator.RemapSimdRegister((int)(rn >> 1));

                EmitMulNegVectorByScalar(context, rdOperand, rnOperand, rmOperand, h, l, m, size, q, negProduct);
            }
        }

        private static void EmitMulNegVectorByScalar(
            CodeGenContext context,
            Operand rd,
            Operand rn,
            Operand rm,
            uint h,
            uint l,
            uint m,
            uint sz,
            uint q,
            bool negProduct)
        {
            using ScopedRegister productRegister = context.RegisterAllocator.AllocateTempSimdRegisterScoped();

            if (sz == 1)
            {
                context.Arm64Assembler.FmulElt2regElementHalf(productRegister.Operand, rn, h, rm, m, l, q);

                if (negProduct)
                {
                    context.Arm64Assembler.FnegHalf(productRegister.Operand, productRegister.Operand, q);
                }

                context.Arm64Assembler.FaddHalf(rd, rd, productRegister.Operand, q);
            }
            else
            {
                context.Arm64Assembler.FmulElt2regElementSingleAndDouble(productRegister.Operand, rn, h, rm, m, l, 0, q);

                if (negProduct)
                {
                    context.Arm64Assembler.FnegSingleAndDouble(productRegister.Operand, productRegister.Operand, 0, q);
                }

                context.Arm64Assembler.FaddSingleAndDouble(rd, rd, productRegister.Operand, 0, q);
            }
        }

        private static (uint, uint, uint) GetIndexForReg(ref uint reg, uint size)
        {
            int shift = (int)(size + 2);
            uint index = reg >> shift;
            reg &= (1u << shift) - 1;
            index |= (reg & 1) << (5 - shift);

            uint h, l, m;

            if (size == 1)
            {
                Debug.Assert((index >> 3) == 0);

                m = index & 1;
                l = (index >> 1) & 1;
                h = index >> 2;
            }
            else
            {
                Debug.Assert(size == 2);
                Debug.Assert((index >> 2) == 0);

                m = 0;
                l = index & 1;
                h = (index >> 1) & 1;
            }

            return (h, l, m);
        }

        private static (uint, uint) GetImmbImmh(uint value, uint size)
        {
            Debug.Assert(value > 0 && value <= (8u << (int)size));

            uint imm = (8u << (int)size) | ((8u << (int)size) - value);

            Debug.Assert((imm >> 7) == 0);

            uint immb = imm & 7;
            uint immh = imm >> 3;

            return (immb, immh);
        }

        public static (uint, uint) GetImmbImmhForShift(uint value, uint size, bool isShl)
        {
            if (isShl)
            {
                Debug.Assert(value >= 0 && value < (8u << (int)size));

                uint imm = (8u << (int)size) | (value & (0x3fu >> (int)(3 - size)));

                Debug.Assert((imm >> 7) == 0);

                uint immb = imm & 7;
                uint immh = imm >> 3;

                return (immb, immh);
            }
            else
            {
                return GetImmbImmh(value, size);
            }
        }

        public static uint GetSizeFromImm6(uint imm6)
        {
            if ((imm6 & 0b100000) != 0)
            {
                return 2;
            }
            else if ((imm6 & 0b10000) != 0)
            {
                return 1;
            }
            else
            {
                Debug.Assert((imm6 & 0b1000) != 0);

                return 0;
            }
        }

        public static uint GetSizeFromImm7(uint imm7)
        {
            if ((imm7 & 0b1000000) != 0)
            {
                return 3;
            }
            else if ((imm7 & 0b100000) != 0)
            {
                return 2;
            }
            else if ((imm7 & 0b10000) != 0)
            {
                return 1;
            }
            else
            {
                Debug.Assert((imm7 & 0b1000) != 0);

                return 0;
            }
        }

        public static ScopedRegister PickSimdRegister(RegisterAllocator registerAllocator, ScopedRegister option1)
        {
            if (option1.IsAllocated)
            {
                return option1;
            }

            return registerAllocator.AllocateTempSimdRegisterScoped();
        }

        public static ScopedRegister PickSimdRegister(RegisterAllocator registerAllocator, ScopedRegister option1, ScopedRegister option2)
        {
            if (option1.IsAllocated)
            {
                return option1;
            }
            else if (option2.IsAllocated)
            {
                return option2;
            }

            return registerAllocator.AllocateTempSimdRegisterScoped();
        }

        public static ScopedRegister PickSimdRegister(RegisterAllocator registerAllocator, ScopedRegister option1, ScopedRegister option2, ScopedRegister option3)
        {
            if (option1.IsAllocated)
            {
                return option1;
            }
            else if (option2.IsAllocated)
            {
                return option2;
            }
            else if (option3.IsAllocated)
            {
                return option3;
            }

            return registerAllocator.AllocateTempSimdRegisterScoped();
        }
    }
}
