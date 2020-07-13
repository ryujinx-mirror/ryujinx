// https://github.com/intel/ARM_NEON_2_x86_SSE/blob/master/NEON_2_SSE.h

using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;
using System.Diagnostics;
using System.Reflection;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    using Func2I = Func<Operand, Operand, Operand>;

    static partial class InstEmit
    {
#region "Masks"
        private static readonly long[] _masks_SliSri = new long[] // Replication masks.
        {
            0x0101010101010101L, 0x0001000100010001L, 0x0000000100000001L, 0x0000000000000001L
        };
#endregion

        public static void Rshrn_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSsse3)
            {
                OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

                int shift = GetImmShr(op);

                long roundConst = 1L << (shift - 1);

                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);

                Operand dLow = context.VectorZeroUpper64(d);

                Operand mask = null;

                switch (op.Size + 1)
                {
                    case 1: mask = X86GetAllElements(context, (int)roundConst * 0x00010001); break;
                    case 2: mask = X86GetAllElements(context, (int)roundConst); break;
                    case 3: mask = X86GetAllElements(context,      roundConst); break;
                }

                Intrinsic addInst = X86PaddInstruction[op.Size + 1];

                Operand res = context.AddIntrinsic(addInst, n, mask);

                Intrinsic srlInst = X86PsrlInstruction[op.Size + 1];

                res = context.AddIntrinsic(srlInst, res, Const(shift));

                Operand mask2 = X86GetAllElements(context, EvenMasks[op.Size]);

                res = context.AddIntrinsic(Intrinsic.X86Pshufb, res, mask2);

                Intrinsic movInst = op.RegisterSize == RegisterSize.Simd128
                    ? Intrinsic.X86Movlhps
                    : Intrinsic.X86Movhlps;

                res = context.AddIntrinsic(movInst, dLow, res);

                context.Copy(d, res);
            }
            else
            {
                EmitVectorShrImmNarrowOpZx(context, round: true);
            }
        }

        public static void Shl_S(ArmEmitterContext context)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            int shift = GetImmShl(op);

            EmitScalarUnaryOpZx(context, (op1) => context.ShiftLeft(op1, Const(shift)));
        }

        public static void Shl_V(ArmEmitterContext context)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            int shift = GetImmShl(op);

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                Operand n = GetVec(op.Rn);

                Intrinsic sllInst = X86PsllInstruction[op.Size];

                Operand res = context.AddIntrinsic(sllInst, n, Const(shift));

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorUnaryOpZx(context, (op1) => context.ShiftLeft(op1, Const(shift)));
            }
        }

        public static void Shll_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int shift = 8 << op.Size;

            if (Optimizations.UseSse41)
            {
                Operand n = GetVec(op.Rn);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    n = context.AddIntrinsic(Intrinsic.X86Psrldq, n, Const(8));
                }

                Intrinsic movsxInst = X86PmovsxInstruction[op.Size];

                Operand res = context.AddIntrinsic(movsxInst, n);

                Intrinsic sllInst = X86PsllInstruction[op.Size + 1];

                res = context.AddIntrinsic(sllInst, res, Const(shift));

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorShImmWidenBinaryZx(context, (op1, op2) => context.ShiftLeft(op1, op2), shift);
            }
        }

        public static void Shrn_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSsse3)
            {
                OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

                int shift = GetImmShr(op);

                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);

                Operand dLow = context.VectorZeroUpper64(d);

                Intrinsic srlInst = X86PsrlInstruction[op.Size + 1];

                Operand nShifted = context.AddIntrinsic(srlInst, n, Const(shift));

                Operand mask = X86GetAllElements(context, EvenMasks[op.Size]);

                Operand res = context.AddIntrinsic(Intrinsic.X86Pshufb, nShifted, mask);

                Intrinsic movInst = op.RegisterSize == RegisterSize.Simd128
                    ? Intrinsic.X86Movlhps
                    : Intrinsic.X86Movhlps;

                res = context.AddIntrinsic(movInst, dLow, res);

                context.Copy(d, res);
            }
            else
            {
                EmitVectorShrImmNarrowOpZx(context, round: false);
            }
        }

        public static void Sli_S(ArmEmitterContext context)
        {
            EmitSli(context, scalar: true);
        }

        public static void Sli_V(ArmEmitterContext context)
        {
            EmitSli(context, scalar: false);
        }

        public static void Sqrshl_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtractSx(context, op.Rn, index, op.Size);
                Operand me = EmitVectorExtractSx(context, op.Rm, index, op.Size);

                Operand e = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SignedShlRegSatQ)), ne, me, Const(1), Const(op.Size));

                res = EmitVectorInsert(context, res, e, index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sqrshrn_S(ArmEmitterContext context)
        {
            EmitRoundShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.ScalarSxSx);
        }

        public static void Sqrshrn_V(ArmEmitterContext context)
        {
            EmitRoundShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.VectorSxSx);
        }

        public static void Sqrshrun_S(ArmEmitterContext context)
        {
            EmitRoundShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.ScalarSxZx);
        }

        public static void Sqrshrun_V(ArmEmitterContext context)
        {
            EmitRoundShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.VectorSxZx);
        }

        public static void Sqshl_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtractSx(context, op.Rn, index, op.Size);
                Operand me = EmitVectorExtractSx(context, op.Rm, index, op.Size);

                Operand e = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SignedShlRegSatQ)), ne, me, Const(0), Const(op.Size));

                res = EmitVectorInsert(context, res, e, index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Sqshrn_S(ArmEmitterContext context)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.ScalarSxSx);
        }

        public static void Sqshrn_V(ArmEmitterContext context)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.VectorSxSx);
        }

        public static void Sqshrun_S(ArmEmitterContext context)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.ScalarSxZx);
        }

        public static void Sqshrun_V(ArmEmitterContext context)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.VectorSxZx);
        }

        public static void Sri_S(ArmEmitterContext context)
        {
            EmitSri(context, scalar: true);
        }

        public static void Sri_V(ArmEmitterContext context)
        {
            EmitSri(context, scalar: false);
        }

        public static void Srshl_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtractSx(context, op.Rn, index, op.Size);
                Operand me = EmitVectorExtractSx(context, op.Rm, index, op.Size);

                Operand e = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.SignedShlReg)), ne, me, Const(1), Const(op.Size));

                res = EmitVectorInsert(context, res, e, index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Srshr_S(ArmEmitterContext context)
        {
            EmitScalarShrImmOpSx(context, ShrImmFlags.Round);
        }

        public static void Srshr_V(ArmEmitterContext context)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0 && op.Size < 3)
            {
                int shift = GetImmShr(op);
                int eSize = 8 << op.Size;

                Operand n = GetVec(op.Rn);

                Intrinsic sllInst = X86PsllInstruction[op.Size];

                Operand res = context.AddIntrinsic(sllInst, n, Const(eSize - shift));

                Intrinsic srlInst = X86PsrlInstruction[op.Size];

                res = context.AddIntrinsic(srlInst, res, Const(eSize - 1));

                Intrinsic sraInst = X86PsraInstruction[op.Size];

                Operand nSra = context.AddIntrinsic(sraInst, n, Const(shift));

                Intrinsic addInst = X86PaddInstruction[op.Size];

                res = context.AddIntrinsic(addInst, res, nSra);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorShrImmOpSx(context, ShrImmFlags.Round);
            }
        }

        public static void Srsra_S(ArmEmitterContext context)
        {
            EmitScalarShrImmOpSx(context, ShrImmFlags.Round | ShrImmFlags.Accumulate);
        }

        public static void Srsra_V(ArmEmitterContext context)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0 && op.Size < 3)
            {
                int shift = GetImmShr(op);
                int eSize = 8 << op.Size;

                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);

                Intrinsic sllInst = X86PsllInstruction[op.Size];

                Operand res = context.AddIntrinsic(sllInst, n, Const(eSize - shift));

                Intrinsic srlInst = X86PsrlInstruction[op.Size];

                res = context.AddIntrinsic(srlInst, res, Const(eSize - 1));

                Intrinsic sraInst = X86PsraInstruction[op.Size];

                Operand nSra = context.AddIntrinsic(sraInst, n, Const(shift));

                Intrinsic addInst = X86PaddInstruction[op.Size];

                res = context.AddIntrinsic(addInst, res, nSra);
                res = context.AddIntrinsic(addInst, res, d);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(d, res);
            }
            else
            {
                EmitVectorShrImmOpSx(context, ShrImmFlags.Round | ShrImmFlags.Accumulate);
            }
        }

        public static void Sshl_S(ArmEmitterContext context)
        {
            EmitSshlOrUshl(context, signed: true, scalar: true);
        }

        public static void Sshl_V(ArmEmitterContext context)
        {
            EmitSshlOrUshl(context, signed: true, scalar: false);
        }

        public static void Sshll_V(ArmEmitterContext context)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            int shift = GetImmShl(op);

            if (Optimizations.UseSse41)
            {
                Operand n = GetVec(op.Rn);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    n = context.AddIntrinsic(Intrinsic.X86Psrldq, n, Const(8));
                }

                Intrinsic movsxInst = X86PmovsxInstruction[op.Size];

                Operand res = context.AddIntrinsic(movsxInst, n);

                if (shift != 0)
                {
                    Intrinsic sllInst = X86PsllInstruction[op.Size + 1];

                    res = context.AddIntrinsic(sllInst, res, Const(shift));
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorShImmWidenBinarySx(context, (op1, op2) => context.ShiftLeft(op1, op2), shift);
            }
        }

        public static void Sshr_S(ArmEmitterContext context)
        {
            EmitShrImmOp(context, ShrImmFlags.ScalarSx);
        }

        public static void Sshr_V(ArmEmitterContext context)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0 && op.Size < 3)
            {
                int shift = GetImmShr(op);

                Operand n = GetVec(op.Rn);

                Intrinsic sraInst = X86PsraInstruction[op.Size];

                Operand res = context.AddIntrinsic(sraInst, n, Const(shift));

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitShrImmOp(context, ShrImmFlags.VectorSx);
            }
        }

        public static void Ssra_S(ArmEmitterContext context)
        {
            EmitScalarShrImmOpSx(context, ShrImmFlags.Accumulate);
        }

        public static void Ssra_V(ArmEmitterContext context)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0 && op.Size < 3)
            {
                int shift = GetImmShr(op);

                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);

                Intrinsic sraInst = X86PsraInstruction[op.Size];

                Operand res = context.AddIntrinsic(sraInst, n, Const(shift));

                Intrinsic addInst = X86PaddInstruction[op.Size];

                res = context.AddIntrinsic(addInst, res, d);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(d, res);
            }
            else
            {
                EmitVectorShrImmOpSx(context, ShrImmFlags.Accumulate);
            }
        }

        public static void Uqrshl_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtractZx(context, op.Rn, index, op.Size);
                Operand me = EmitVectorExtractZx(context, op.Rm, index, op.Size);

                Operand e = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.UnsignedShlRegSatQ)), ne, me, Const(1), Const(op.Size));

                res = EmitVectorInsert(context, res, e, index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Uqrshrn_S(ArmEmitterContext context)
        {
            EmitRoundShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.ScalarZxZx);
        }

        public static void Uqrshrn_V(ArmEmitterContext context)
        {
            EmitRoundShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.VectorZxZx);
        }

        public static void Uqshl_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtractZx(context, op.Rn, index, op.Size);
                Operand me = EmitVectorExtractZx(context, op.Rm, index, op.Size);

                Operand e = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.UnsignedShlRegSatQ)), ne, me, Const(0), Const(op.Size));

                res = EmitVectorInsert(context, res, e, index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Uqshrn_S(ArmEmitterContext context)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.ScalarZxZx);
        }

        public static void Uqshrn_V(ArmEmitterContext context)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.VectorZxZx);
        }

        public static void Urshl_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.GetBytesCount() >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtractZx(context, op.Rn, index, op.Size);
                Operand me = EmitVectorExtractZx(context, op.Rm, index, op.Size);

                Operand e = context.Call(typeof(SoftFallback).GetMethod(nameof(SoftFallback.UnsignedShlReg)), ne, me, Const(1), Const(op.Size));

                res = EmitVectorInsert(context, res, e, index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        public static void Urshr_S(ArmEmitterContext context)
        {
            EmitScalarShrImmOpZx(context, ShrImmFlags.Round);
        }

        public static void Urshr_V(ArmEmitterContext context)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                int shift = GetImmShr(op);
                int eSize = 8 << op.Size;

                Operand n = GetVec(op.Rn);

                Intrinsic sllInst = X86PsllInstruction[op.Size];

                Operand res = context.AddIntrinsic(sllInst, n, Const(eSize - shift));

                Intrinsic srlInst = X86PsrlInstruction[op.Size];

                res = context.AddIntrinsic(srlInst, res, Const(eSize - 1));

                Operand nSrl = context.AddIntrinsic(srlInst, n, Const(shift));

                Intrinsic addInst = X86PaddInstruction[op.Size];

                res = context.AddIntrinsic(addInst, res, nSrl);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorShrImmOpZx(context, ShrImmFlags.Round);
            }
        }

        public static void Ursra_S(ArmEmitterContext context)
        {
            EmitScalarShrImmOpZx(context, ShrImmFlags.Round | ShrImmFlags.Accumulate);
        }

        public static void Ursra_V(ArmEmitterContext context)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                int shift = GetImmShr(op);
                int eSize = 8 << op.Size;

                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);

                Intrinsic sllInst = X86PsllInstruction[op.Size];

                Operand res = context.AddIntrinsic(sllInst, n, Const(eSize - shift));

                Intrinsic srlInst = X86PsrlInstruction[op.Size];

                res = context.AddIntrinsic(srlInst, res, Const(eSize - 1));

                Operand nSrl = context.AddIntrinsic(srlInst, n, Const(shift));

                Intrinsic addInst = X86PaddInstruction[op.Size];

                res = context.AddIntrinsic(addInst, res, nSrl);
                res = context.AddIntrinsic(addInst, res, d);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(d, res);
            }
            else
            {
                EmitVectorShrImmOpZx(context, ShrImmFlags.Round | ShrImmFlags.Accumulate);
            }
        }

        public static void Ushl_S(ArmEmitterContext context)
        {
            EmitSshlOrUshl(context, signed: false, scalar: true);
        }

        public static void Ushl_V(ArmEmitterContext context)
        {
            EmitSshlOrUshl(context, signed: false, scalar: false);
        }

        public static void Ushll_V(ArmEmitterContext context)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            int shift = GetImmShl(op);

            if (Optimizations.UseSse41)
            {
                Operand n = GetVec(op.Rn);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    n = context.AddIntrinsic(Intrinsic.X86Psrldq, n, Const(8));
                }

                Intrinsic movzxInst = X86PmovzxInstruction[op.Size];

                Operand res = context.AddIntrinsic(movzxInst, n);

                if (shift != 0)
                {
                    Intrinsic sllInst = X86PsllInstruction[op.Size + 1];

                    res = context.AddIntrinsic(sllInst, res, Const(shift));
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorShImmWidenBinaryZx(context, (op1, op2) => context.ShiftLeft(op1, op2), shift);
            }
        }

        public static void Ushr_S(ArmEmitterContext context)
        {
            EmitShrImmOp(context, ShrImmFlags.ScalarZx);
        }

        public static void Ushr_V(ArmEmitterContext context)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                int shift = GetImmShr(op);

                Operand n = GetVec(op.Rn);

                Intrinsic srlInst = X86PsrlInstruction[op.Size];

                Operand res = context.AddIntrinsic(srlInst, n, Const(shift));

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitShrImmOp(context, ShrImmFlags.VectorZx);
            }
        }

        public static void Usra_S(ArmEmitterContext context)
        {
            EmitScalarShrImmOpZx(context, ShrImmFlags.Accumulate);
        }

        public static void Usra_V(ArmEmitterContext context)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                int shift = GetImmShr(op);

                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);

                Intrinsic srlInst = X86PsrlInstruction[op.Size];

                Operand res = context.AddIntrinsic(srlInst, n, Const(shift));

                Intrinsic addInst = X86PaddInstruction[op.Size];

                res = context.AddIntrinsic(addInst, res, d);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(d, res);
            }
            else
            {
                EmitVectorShrImmOpZx(context, ShrImmFlags.Accumulate);
            }
        }

        [Flags]
        private enum ShrImmFlags
        {
            Scalar = 1 << 0,
            Signed = 1 << 1,

            Round      = 1 << 2,
            Accumulate = 1 << 3,

            ScalarSx = Scalar | Signed,
            ScalarZx = Scalar,

            VectorSx = Signed,
            VectorZx = 0
        }

        private static void EmitScalarShrImmOpSx(ArmEmitterContext context, ShrImmFlags flags)
        {
            EmitShrImmOp(context, ShrImmFlags.ScalarSx | flags);
        }

        private static void EmitScalarShrImmOpZx(ArmEmitterContext context, ShrImmFlags flags)
        {
            EmitShrImmOp(context, ShrImmFlags.ScalarZx | flags);
        }

        private static void EmitVectorShrImmOpSx(ArmEmitterContext context, ShrImmFlags flags)
        {
            EmitShrImmOp(context, ShrImmFlags.VectorSx | flags);
        }

        private static void EmitVectorShrImmOpZx(ArmEmitterContext context, ShrImmFlags flags)
        {
            EmitShrImmOp(context, ShrImmFlags.VectorZx | flags);
        }

        private static void EmitShrImmOp(ArmEmitterContext context, ShrImmFlags flags)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            Operand res = context.VectorZero();

            bool scalar     = (flags & ShrImmFlags.Scalar)     != 0;
            bool signed     = (flags & ShrImmFlags.Signed)     != 0;
            bool round      = (flags & ShrImmFlags.Round)      != 0;
            bool accumulate = (flags & ShrImmFlags.Accumulate) != 0;

            int shift = GetImmShr(op);

            long roundConst = 1L << (shift - 1);

            int elems = !scalar ? op.GetBytesCount() >> op.Size : 1;

            for (int index = 0; index < elems; index++)
            {
                Operand e = EmitVectorExtract(context, op.Rn, index, op.Size, signed);

                if (op.Size <= 2)
                {
                    if (round)
                    {
                        e = context.Add(e, Const(roundConst));
                    }

                    e = signed ? context.ShiftRightSI(e, Const(shift)) : context.ShiftRightUI(e, Const(shift));
                }
                else /* if (op.Size == 3) */
                {
                    e = EmitShrImm64(context, e, signed, round ? roundConst : 0L, shift);
                }

                if (accumulate)
                {
                    Operand de = EmitVectorExtract(context, op.Rd, index, op.Size, signed);

                    e = context.Add(e, de);
                }

                res = EmitVectorInsert(context, res, e, index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        private static Operand EmitShlRegOp(ArmEmitterContext context, Operand op, Operand shiftLsB, int size, bool signed)
        {
            Debug.Assert(op.Type       == OperandType.I64);
            Debug.Assert(shiftLsB.Type == OperandType.I32);
            Debug.Assert((uint)size < 4u);

            Operand negShiftLsB = context.Negate(shiftLsB);

            Operand isInRange = context.BitwiseAnd(
                context.ICompareLess(shiftLsB,    Const(8 << size)),
                context.ICompareLess(negShiftLsB, Const(8 << size)));

            Operand isPositive = context.ICompareGreaterOrEqual(shiftLsB, Const(0));

            Operand shl = context.ShiftLeft(op, shiftLsB);

            Operand sarOrShr = signed
                ? context.ShiftRightSI(op, negShiftLsB)
                : context.ShiftRightUI(op, negShiftLsB);

            Operand res = context.ConditionalSelect(isPositive, shl, sarOrShr);

            if (signed)
            {
                Operand isPositive2 = context.ICompareGreaterOrEqual(op, Const(0L));

                Operand res2 = context.ConditionalSelect(isPositive2, Const(0L), Const(-1L));
                        res2 = context.ConditionalSelect(isPositive,  Const(0L), res2);

                return context.ConditionalSelect(isInRange, res, res2);
            }
            else
            {
                return context.ConditionalSelect(isInRange, res, Const(0UL));
            }
        }

        private static void EmitVectorShrImmNarrowOpZx(ArmEmitterContext context, bool round)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            int shift = GetImmShr(op);

            long roundConst = 1L << (shift - 1);

            int elems = 8 >> op.Size;

            int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

            Operand d = GetVec(op.Rd);

            Operand res = part == 0 ? context.VectorZero() : context.Copy(d);

            for (int index = 0; index < elems; index++)
            {
                Operand e = EmitVectorExtractZx(context, op.Rn, index, op.Size + 1);

                if (round)
                {
                    e = context.Add(e, Const(roundConst));
                }

                e = context.ShiftRightUI(e, Const(shift));

                res = EmitVectorInsert(context, res, e, part + index, op.Size);
            }

            context.Copy(d, res);
        }

        [Flags]
        private enum ShrImmSaturatingNarrowFlags
        {
            Scalar    = 1 << 0,
            SignedSrc = 1 << 1,
            SignedDst = 1 << 2,

            Round = 1 << 3,

            ScalarSxSx = Scalar | SignedSrc | SignedDst,
            ScalarSxZx = Scalar | SignedSrc,
            ScalarZxZx = Scalar,

            VectorSxSx = SignedSrc | SignedDst,
            VectorSxZx = SignedSrc,
            VectorZxZx = 0
        }

        private static void EmitRoundShrImmSaturatingNarrowOp(ArmEmitterContext context, ShrImmSaturatingNarrowFlags flags)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.Round | flags);
        }

        private static void EmitShrImmSaturatingNarrowOp(ArmEmitterContext context, ShrImmSaturatingNarrowFlags flags)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            bool scalar    = (flags & ShrImmSaturatingNarrowFlags.Scalar)    != 0;
            bool signedSrc = (flags & ShrImmSaturatingNarrowFlags.SignedSrc) != 0;
            bool signedDst = (flags & ShrImmSaturatingNarrowFlags.SignedDst) != 0;
            bool round     = (flags & ShrImmSaturatingNarrowFlags.Round)     != 0;

            int shift = GetImmShr(op);

            long roundConst = 1L << (shift - 1);

            int elems = !scalar ? 8 >> op.Size : 1;

            int part = !scalar && (op.RegisterSize == RegisterSize.Simd128) ? elems : 0;

            Operand d = GetVec(op.Rd);

            Operand res = part == 0 ? context.VectorZero() : context.Copy(d);

            for (int index = 0; index < elems; index++)
            {
                Operand e = EmitVectorExtract(context, op.Rn, index, op.Size + 1, signedSrc);

                if (op.Size <= 1 || !round)
                {
                    if (round)
                    {
                        e = context.Add(e, Const(roundConst));
                    }

                    e = signedSrc ? context.ShiftRightSI(e, Const(shift)) : context.ShiftRightUI(e, Const(shift));
                }
                else /* if (op.Size == 2 && round) */
                {
                    e = EmitShrImm64(context, e, signedSrc, roundConst, shift); // shift <= 32
                }

                e = EmitSatQ(context, e, op.Size, signedSrc, signedDst);

                res = EmitVectorInsert(context, res, e, part + index, op.Size);
            }

            context.Copy(d, res);
        }

        // dst64 = (Int(src64, signed) + roundConst) >> shift;
        private static Operand EmitShrImm64(
            ArmEmitterContext context,
            Operand value,
            bool signed,
            long roundConst,
            int shift)
        {
            MethodInfo info = signed
                ? typeof(SoftFallback).GetMethod(nameof(SoftFallback.SignedShrImm64))
                : typeof(SoftFallback).GetMethod(nameof(SoftFallback.UnsignedShrImm64));

            return context.Call(info, value, Const(roundConst), Const(shift));
        }

        private static void EmitVectorShImmWidenBinarySx(ArmEmitterContext context, Func2I emit, int imm)
        {
            EmitVectorShImmWidenBinaryOp(context, emit, imm, signed: true);
        }

        private static void EmitVectorShImmWidenBinaryZx(ArmEmitterContext context, Func2I emit, int imm)
        {
            EmitVectorShImmWidenBinaryOp(context, emit, imm, signed: false);
        }

        private static void EmitVectorShImmWidenBinaryOp(ArmEmitterContext context, Func2I emit, int imm, bool signed)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = 8 >> op.Size;

            int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtract(context, op.Rn, part + index, op.Size, signed);

                res = EmitVectorInsert(context, res, emit(ne, Const(imm)), index, op.Size + 1);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        private static void EmitSli(ArmEmitterContext context, bool scalar)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            int shift = GetImmShl(op);

            ulong mask = shift != 0 ? ulong.MaxValue >> (64 - shift) : 0UL;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);

                Intrinsic sllInst = X86PsllInstruction[op.Size];

                Operand nShifted = context.AddIntrinsic(sllInst, n, Const(shift));

                Operand dMask = X86GetAllElements(context, (long)mask * _masks_SliSri[op.Size]);

                Operand dMasked = context.AddIntrinsic(Intrinsic.X86Pand, d, dMask);

                Operand res = context.AddIntrinsic(Intrinsic.X86Por, nShifted, dMasked);

                if ((op.RegisterSize == RegisterSize.Simd64) || scalar)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(d, res);
            }
            else
            {
                Operand res = context.VectorZero();

                int elems = !scalar ? op.GetBytesCount() >> op.Size : 1;

                for (int index = 0; index < elems; index++)
                {
                    Operand ne = EmitVectorExtractZx(context, op.Rn, index, op.Size);

                    Operand neShifted = context.ShiftLeft(ne, Const(shift));

                    Operand de = EmitVectorExtractZx(context, op.Rd, index, op.Size);

                    Operand deMasked = context.BitwiseAnd(de, Const(mask));

                    Operand e = context.BitwiseOr(neShifted, deMasked);

                    res = EmitVectorInsert(context, res, e, index, op.Size);
                }

                context.Copy(GetVec(op.Rd), res);
            }
        }

        private static void EmitSri(ArmEmitterContext context, bool scalar)
        {
            OpCodeSimdShImm op = (OpCodeSimdShImm)context.CurrOp;

            int shift = GetImmShr(op);
            int eSize = 8 << op.Size;

            ulong mask = (ulong.MaxValue << (eSize - shift)) & (ulong.MaxValue >> (64 - eSize));

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);

                Intrinsic srlInst = X86PsrlInstruction[op.Size];

                Operand nShifted = context.AddIntrinsic(srlInst, n, Const(shift));

                Operand dMask = X86GetAllElements(context, (long)mask * _masks_SliSri[op.Size]);

                Operand dMasked = context.AddIntrinsic(Intrinsic.X86Pand, d, dMask);

                Operand res = context.AddIntrinsic(Intrinsic.X86Por, nShifted, dMasked);

                if ((op.RegisterSize == RegisterSize.Simd64) || scalar)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(d, res);
            }
            else
            {
                Operand res = context.VectorZero();

                int elems = !scalar ? op.GetBytesCount() >> op.Size : 1;

                for (int index = 0; index < elems; index++)
                {
                    Operand ne = EmitVectorExtractZx(context, op.Rn, index, op.Size);

                    Operand neShifted = shift != 64 ? context.ShiftRightUI(ne, Const(shift)) : Const(0UL);

                    Operand de = EmitVectorExtractZx(context, op.Rd, index, op.Size);

                    Operand deMasked = context.BitwiseAnd(de, Const(mask));

                    Operand e = context.BitwiseOr(neShifted, deMasked);

                    res = EmitVectorInsert(context, res, e, index, op.Size);
                }

                context.Copy(GetVec(op.Rd), res);
            }
        }

        private static void EmitSshlOrUshl(ArmEmitterContext context, bool signed, bool scalar)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = !scalar ? op.GetBytesCount() >> op.Size : 1;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtract  (context, op.Rn, index, op.Size, signed);
                Operand me = EmitVectorExtractSx(context, op.Rm, index << op.Size, 0);

                Operand e = EmitShlRegOp(context, ne, context.ConvertI64ToI32(me), op.Size, signed);

                res = EmitVectorInsert(context, res, e, index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }
    }
}
