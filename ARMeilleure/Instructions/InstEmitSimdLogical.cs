using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System.Diagnostics;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        public static void And_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Operand res = context.AddIntrinsic(Intrinsic.X86Pand, n, m);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorBinaryOpZx(context, (op1, op2) => context.BitwiseAnd(op1, op2));
            }
        }

        public static void Bic_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Operand res = context.AddIntrinsic(Intrinsic.X86Pandn, m, n);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorBinaryOpZx(context, (op1, op2) =>
                {
                    return context.BitwiseAnd(op1, context.BitwiseNot(op2));
                });
            }
        }

        public static void Bic_Vi(ArmEmitterContext context)
        {
            EmitVectorImmBinaryOp(context, (op1, op2) =>
            {
                return context.BitwiseAnd(op1, context.BitwiseNot(op2));
            });
        }

        public static void Bif_V(ArmEmitterContext context)
        {
            EmitBifBit(context, notRm: true);
        }

        public static void Bit_V(ArmEmitterContext context)
        {
            EmitBifBit(context, notRm: false);
        }

        private static void EmitBifBit(ArmEmitterContext context, bool notRm)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse2)
            {
                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Operand res = context.AddIntrinsic(Intrinsic.X86Pxor, n, d);

                if (notRm)
                {
                    res = context.AddIntrinsic(Intrinsic.X86Pandn, m, res);
                }
                else
                {
                    res = context.AddIntrinsic(Intrinsic.X86Pand, m, res);
                }

                res = context.AddIntrinsic(Intrinsic.X86Pxor, d, res);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(d, res);
            }
            else
            {
                Operand res = context.VectorZero();

                int elems = op.RegisterSize == RegisterSize.Simd128 ? 2 : 1;

                for (int index = 0; index < elems; index++)
                {
                    Operand d = EmitVectorExtractZx(context, op.Rd, index, 3);
                    Operand n = EmitVectorExtractZx(context, op.Rn, index, 3);
                    Operand m = EmitVectorExtractZx(context, op.Rm, index, 3);

                    if (notRm)
                    {
                        m = context.BitwiseNot(m);
                    }

                    Operand e = context.BitwiseExclusiveOr(d, n);

                    e = context.BitwiseAnd(e, m);
                    e = context.BitwiseExclusiveOr(e, d);

                    res = EmitVectorInsert(context, res, e, index, 3);
                }

                context.Copy(GetVec(op.Rd), res);
            }
        }

        public static void Bsl_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Operand res = context.AddIntrinsic(Intrinsic.X86Pxor, n, m);

                res = context.AddIntrinsic(Intrinsic.X86Pand, res, d);
                res = context.AddIntrinsic(Intrinsic.X86Pxor, res, m);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(d, res);
            }
            else
            {
                EmitVectorTernaryOpZx(context, (op1, op2, op3) =>
                {
                    return context.BitwiseExclusiveOr(
                        context.BitwiseAnd(op1,
                        context.BitwiseExclusiveOr(op2, op3)), op3);
                });
            }
        }

        public static void Eor_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Operand res = context.AddIntrinsic(Intrinsic.X86Pxor, n, m);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorBinaryOpZx(context, (op1, op2) => context.BitwiseExclusiveOr(op1, op2));
            }
        }

        public static void Not_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                Operand n = GetVec(op.Rn);

                Operand mask = X86GetAllElements(context, -1L);

                Operand res = context.AddIntrinsic(Intrinsic.X86Pandn, n, mask);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorUnaryOpZx(context, (op1) => context.BitwiseNot(op1));
            }
        }

        public static void Orn_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Operand mask = X86GetAllElements(context, -1L);

                Operand res = context.AddIntrinsic(Intrinsic.X86Pandn, m, mask);

                res = context.AddIntrinsic(Intrinsic.X86Por, res, n);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorBinaryOpZx(context, (op1, op2) =>
                {
                    return context.BitwiseOr(op1, context.BitwiseNot(op2));
                });
            }
        }

        public static void Orr_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Operand res = context.AddIntrinsic(Intrinsic.X86Por, n, m);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitVectorBinaryOpZx(context, (op1, op2) => context.BitwiseOr(op1, op2));
            }
        }

        public static void Orr_Vi(ArmEmitterContext context)
        {
            EmitVectorImmBinaryOp(context, (op1, op2) => context.BitwiseOr(op1, op2));
        }

        public static void Rbit_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.RegisterSize == RegisterSize.Simd128 ? 16 : 8;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtractZx(context, op.Rn, index, 0);

                Operand de = EmitReverseBits8Op(context, ne);

                res = EmitVectorInsert(context, res, de, index, 0);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        private static Operand EmitReverseBits8Op(ArmEmitterContext context, Operand op)
        {
            Debug.Assert(op.Type == OperandType.I64);

            Operand val = context.BitwiseOr(context.ShiftRightUI(context.BitwiseAnd(op, Const(0xaaul)), Const(1)),
                                            context.ShiftLeft   (context.BitwiseAnd(op, Const(0x55ul)), Const(1)));

            val = context.BitwiseOr(context.ShiftRightUI(context.BitwiseAnd(val, Const(0xccul)), Const(2)),
                                    context.ShiftLeft   (context.BitwiseAnd(val, Const(0x33ul)), Const(2)));

            return context.BitwiseOr(context.ShiftRightUI(val, Const(4)),
                                     context.ShiftLeft   (context.BitwiseAnd(val, Const(0x0ful)), Const(4)));
        }

        public static void Rev16_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSsse3)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                Operand n = GetVec(op.Rn);

                const long maskE0 = 06L << 56 | 07L << 48 | 04L << 40 | 05L << 32 | 02L << 24 | 03L << 16 | 00L << 8 | 01L << 0;
                const long maskE1 = 14L << 56 | 15L << 48 | 12L << 40 | 13L << 32 | 10L << 24 | 11L << 16 | 08L << 8 | 09L << 0;

                Operand mask = X86GetScalar(context, maskE0);

                mask = EmitVectorInsert(context, mask, Const(maskE1), 1, 3);

                Operand res = context.AddIntrinsic(Intrinsic.X86Pshufb, n, mask);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitRev_V(context, containerSize: 1);
            }
        }

        public static void Rev32_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSsse3)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                Operand n = GetVec(op.Rn);

                Operand mask;

                if (op.Size == 0)
                {
                    const long maskE0 = 04L << 56 | 05L << 48 | 06L << 40 | 07L << 32 | 00L << 24 | 01L << 16 | 02L << 8 | 03L << 0;
                    const long maskE1 = 12L << 56 | 13L << 48 | 14L << 40 | 15L << 32 | 08L << 24 | 09L << 16 | 10L << 8 | 11L << 0;

                    mask = X86GetScalar(context, maskE0);

                    mask = EmitVectorInsert(context, mask, Const(maskE1), 1, 3);
                }
                else /* if (op.Size == 1) */
                {
                    const long maskE0 = 05L << 56 | 04L << 48 | 07L << 40 | 06L << 32 | 01L << 24 | 00L << 16 | 03L << 8 | 02L << 0;
                    const long maskE1 = 13L << 56 | 12L << 48 | 15L << 40 | 14L << 32 | 09L << 24 | 08L << 16 | 11L << 8 | 10L << 0;

                    mask = X86GetScalar(context, maskE0);

                    mask = EmitVectorInsert(context, mask, Const(maskE1), 1, 3);
                }

                Operand res = context.AddIntrinsic(Intrinsic.X86Pshufb, n, mask);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitRev_V(context, containerSize: 2);
            }
        }

        public static void Rev64_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSsse3)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                Operand n = GetVec(op.Rn);

                Operand mask;

                if (op.Size == 0)
                {
                    const long maskE0 = 00L << 56 | 01L << 48 | 02L << 40 | 03L << 32 | 04L << 24 | 05L << 16 | 06L << 8 | 07L << 0;
                    const long maskE1 = 08L << 56 | 09L << 48 | 10L << 40 | 11L << 32 | 12L << 24 | 13L << 16 | 14L << 8 | 15L << 0;

                    mask = X86GetScalar(context, maskE0);

                    mask = EmitVectorInsert(context, mask, Const(maskE1), 1, 3);
                }
                else if (op.Size == 1)
                {
                    const long maskE0 = 01L << 56 | 00L << 48 | 03L << 40 | 02L << 32 | 05L << 24 | 04L << 16 | 07L << 8 | 06L << 0;
                    const long maskE1 = 09L << 56 | 08L << 48 | 11L << 40 | 10L << 32 | 13L << 24 | 12L << 16 | 15L << 8 | 14L << 0;

                    mask = X86GetScalar(context, maskE0);

                    mask = EmitVectorInsert(context, mask, Const(maskE1), 1, 3);
                }
                else /* if (op.Size == 2) */
                {
                    const long maskE0 = 03L << 56 | 02L << 48 | 01L << 40 | 00L << 32 | 07L << 24 | 06L << 16 | 05L << 8 | 04L << 0;
                    const long maskE1 = 11L << 56 | 10L << 48 | 09L << 40 | 08L << 32 | 15L << 24 | 14L << 16 | 13L << 8 | 12L << 0;

                    mask = X86GetScalar(context, maskE0);

                    mask = EmitVectorInsert(context, mask, Const(maskE1), 1, 3);
                }

                Operand res = context.AddIntrinsic(Intrinsic.X86Pshufb, n, mask);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitRev_V(context, containerSize: 3);
            }
        }

        private static void EmitRev_V(ArmEmitterContext context, int containerSize)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = op.GetBytesCount() >> op.Size;

            int containerMask = (1 << (containerSize - op.Size)) - 1;

            for (int index = 0; index < elems; index++)
            {
                int revIndex = index ^ containerMask;

                Operand ne = EmitVectorExtractZx(context, op.Rn, revIndex, op.Size);

                res = EmitVectorInsert(context, res, ne, index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }
    }
}
