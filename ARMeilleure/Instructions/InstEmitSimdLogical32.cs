using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper32;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        public static void Vand_I(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                EmitVectorBinaryOpSimd32(context, (n, m) => context.AddIntrinsic(Intrinsic.X86Pand, n, m));
            }
            else
            {
                EmitVectorBinaryOpZx32(context, (op1, op2) => context.BitwiseAnd(op1, op2));
            }
        }

        public static void Vbic_I(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                EmitVectorBinaryOpSimd32(context, (n, m) => context.AddIntrinsic(Intrinsic.X86Pandn, m, n));
            }
            else
            {
                EmitVectorBinaryOpZx32(context, (op1, op2) => context.BitwiseAnd(op1, context.BitwiseNot(op2)));
            }
        }

        public static void Vbic_II(ArmEmitterContext context)
        {
            OpCode32SimdImm op = (OpCode32SimdImm)context.CurrOp;

            long immediate = op.Immediate;

            // Replicate fields to fill the 64-bits, if size is < 64-bits.
            switch (op.Size)
            {
                case 0: immediate *= 0x0101010101010101L; break;
                case 1: immediate *= 0x0001000100010001L; break;
                case 2: immediate *= 0x0000000100000001L; break;
            }

            Operand imm = Const(immediate);
            Operand res = GetVecA32(op.Qd);

            if (op.Q)
            {
                for (int elem = 0; elem < 2; elem++)
                {
                    Operand de = EmitVectorExtractZx(context, op.Qd, elem, 3);

                    res = EmitVectorInsert(context, res, context.BitwiseAnd(de, context.BitwiseNot(imm)), elem, 3);
                }
            }
            else
            {
                Operand de = EmitVectorExtractZx(context, op.Qd, op.Vd & 1, 3);

                res = EmitVectorInsert(context, res, context.BitwiseAnd(de, context.BitwiseNot(imm)), op.Vd & 1, 3);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void Vbif(ArmEmitterContext context)
        {
            EmitBifBit(context, true);
        }

        public static void Vbit(ArmEmitterContext context)
        {
            EmitBifBit(context, false);
        }

        public static void Vbsl(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                EmitVectorTernaryOpSimd32(context, (d, n, m) =>
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Pxor, n, m);
                    res = context.AddIntrinsic(Intrinsic.X86Pand, res, d);
                    return context.AddIntrinsic(Intrinsic.X86Pxor, res, m);
                });
            }
            else
            {
                EmitVectorTernaryOpZx32(context, (op1, op2, op3) =>
                {
                    return context.BitwiseExclusiveOr(
                        context.BitwiseAnd(op1,
                        context.BitwiseExclusiveOr(op2, op3)), op3);
                });
            }
        }

        public static void Veor_I(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                EmitVectorBinaryOpSimd32(context, (n, m) => context.AddIntrinsic(Intrinsic.X86Pxor, n, m));
            }
            else
            {
                EmitVectorBinaryOpZx32(context, (op1, op2) => context.BitwiseExclusiveOr(op1, op2));
            }
        }

        public static void Vorr_I(ArmEmitterContext context)
        {
            if (Optimizations.UseSse2)
            {
                EmitVectorBinaryOpSimd32(context, (n, m) => context.AddIntrinsic(Intrinsic.X86Por, n, m));
            }
            else
            {
                EmitVectorBinaryOpZx32(context, (op1, op2) => context.BitwiseOr(op1, op2));
            }
        }

        public static void Vorr_II(ArmEmitterContext context)
        {
            OpCode32SimdImm op = (OpCode32SimdImm)context.CurrOp;

            long immediate = op.Immediate;

            // Replicate fields to fill the 64-bits, if size is < 64-bits.
            switch (op.Size)
            {
                case 0: immediate *= 0x0101010101010101L; break;
                case 1: immediate *= 0x0001000100010001L; break;
                case 2: immediate *= 0x0000000100000001L; break;
            }

            Operand imm = Const(immediate);
            Operand res = GetVecA32(op.Qd);

            if (op.Q)
            {
                for (int elem = 0; elem < 2; elem++)
                {
                    Operand de = EmitVectorExtractZx(context, op.Qd, elem, 3);

                    res = EmitVectorInsert(context, res, context.BitwiseOr(de, imm), elem, 3);
                }
            }
            else
            {
                Operand de = EmitVectorExtractZx(context, op.Qd, op.Vd & 1, 3);

                res = EmitVectorInsert(context, res, context.BitwiseOr(de, imm), op.Vd & 1, 3);
            }

            context.Copy(GetVecA32(op.Qd), res);
        }

        public static void Vtst(ArmEmitterContext context)
        {
            EmitVectorBinaryOpZx32(context, (op1, op2) =>
            {
                Operand isZero = context.ICompareEqual(context.BitwiseAnd(op1, op2), Const(0));
                return context.ConditionalSelect(isZero, Const(0), Const(-1));
            });
        }

        private static void EmitBifBit(ArmEmitterContext context, bool notRm)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            if (Optimizations.UseSse2)
            {
                EmitVectorTernaryOpSimd32(context, (d, n, m) =>
                {
                    Operand res = context.AddIntrinsic(Intrinsic.X86Pxor, n, d);
                    res = context.AddIntrinsic((notRm) ? Intrinsic.X86Pandn : Intrinsic.X86Pand, m, res);
                    return context.AddIntrinsic(Intrinsic.X86Pxor, d, res);
                });
            }
            else
            {
                EmitVectorTernaryOpZx32(context, (d, n, m) =>
                {
                    if (notRm)
                    {
                        m = context.BitwiseNot(m);
                    }
                    return context.BitwiseExclusiveOr(
                        context.BitwiseAnd(m,
                        context.BitwiseExclusiveOr(d, n)), d);
                });
            }
        }
    }
}
