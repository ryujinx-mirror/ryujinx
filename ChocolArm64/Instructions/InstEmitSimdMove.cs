using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using static ChocolArm64.Instructions.InstEmitSimdHelper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
#region "Masks"
        private static readonly long[] _masksE0_TrnUzpXtn = new long[]
        {
            14L << 56 | 12L << 48 | 10L << 40 | 08L << 32 | 06L << 24 | 04L << 16 | 02L << 8 | 00L << 0,
            13L << 56 | 12L << 48 | 09L << 40 | 08L << 32 | 05L << 24 | 04L << 16 | 01L << 8 | 00L << 0,
            11L << 56 | 10L << 48 | 09L << 40 | 08L << 32 | 03L << 24 | 02L << 16 | 01L << 8 | 00L << 0
        };

        private static readonly long[] _masksE1_TrnUzp = new long[]
        {
            15L << 56 | 13L << 48 | 11L << 40 | 09L << 32 | 07L << 24 | 05L << 16 | 03L << 8 | 01L << 0,
            15L << 56 | 14L << 48 | 11L << 40 | 10L << 32 | 07L << 24 | 06L << 16 | 03L << 8 | 02L << 0,
            15L << 56 | 14L << 48 | 13L << 40 | 12L << 32 | 07L << 24 | 06L << 16 | 05L << 8 | 04L << 0
        };

        private static readonly long[] _masksE0_Uzp = new long[]
        {
            13L << 56 | 09L << 48 | 05L << 40 | 01L << 32 | 12L << 24 | 08L << 16 | 04L << 8 | 00L << 0,
            11L << 56 | 10L << 48 | 03L << 40 | 02L << 32 | 09L << 24 | 08L << 16 | 01L << 8 | 00L << 0
        };

        private static readonly long[] _masksE1_Uzp = new long[]
        {
            15L << 56 | 11L << 48 | 07L << 40 | 03L << 32 | 14L << 24 | 10L << 16 | 06L << 8 | 02L << 0,
            15L << 56 | 14L << 48 | 07L << 40 | 06L << 32 | 13L << 24 | 12L << 16 | 05L << 8 | 04L << 0
        };
#endregion

        public static void Dup_Gp(ILEmitterCtx context)
        {
            OpCodeSimdIns64 op = (OpCodeSimdIns64)context.CurrOp;

            if (Optimizations.UseSse2)
            {
                Type[] typesSav = new Type[] { UIntTypesPerSizeLog2[op.Size] };

                context.EmitLdintzr(op.Rn);

                switch (op.Size)
                {
                    case 0: context.Emit(OpCodes.Conv_U1); break;
                    case 1: context.Emit(OpCodes.Conv_U2); break;
                    case 2: context.Emit(OpCodes.Conv_U4); break;
                }

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                context.EmitStvec(op.Rd);
            }
            else
            {
                int bytes = op.GetBitsCount() >> 3;
                int elems = bytes >> op.Size;

                for (int index = 0; index < elems; index++)
                {
                    context.EmitLdintzr(op.Rn);

                    EmitVectorInsert(context, op.Rd, index, op.Size);
                }
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void Dup_S(ILEmitterCtx context)
        {
            OpCodeSimdIns64 op = (OpCodeSimdIns64)context.CurrOp;

            EmitVectorExtractZx(context, op.Rn, op.DstIndex, op.Size);

            EmitScalarSet(context, op.Rd, op.Size);
        }

        public static void Dup_V(ILEmitterCtx context)
        {
            OpCodeSimdIns64 op = (OpCodeSimdIns64)context.CurrOp;

            if (Optimizations.UseSse2)
            {
                Type[] typesSav = new Type[] { UIntTypesPerSizeLog2[op.Size] };

                EmitVectorExtractZx(context, op.Rn, op.DstIndex, op.Size);

                switch (op.Size)
                {
                    case 0: context.Emit(OpCodes.Conv_U1); break;
                    case 1: context.Emit(OpCodes.Conv_U2); break;
                    case 2: context.Emit(OpCodes.Conv_U4); break;
                }

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                context.EmitStvec(op.Rd);
            }
            else
            {
                int bytes = op.GetBitsCount() >> 3;
                int elems = bytes >> op.Size;

                for (int index = 0; index < elems; index++)
                {
                    EmitVectorExtractZx(context, op.Rn, op.DstIndex, op.Size);

                    EmitVectorInsert(context, op.Rd, index, op.Size);
                }
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void Ext_V(ILEmitterCtx context)
        {
            OpCodeSimdExt64 op = (OpCodeSimdExt64)context.CurrOp;

            if (Optimizations.UseSse2)
            {
                Type[] typesShs = new Type[] { typeof(Vector128<byte>), typeof(byte) };
                Type[] typesOr  = new Type[] { typeof(Vector128<byte>), typeof(Vector128<byte>) };

                context.EmitLdvec(op.Rn);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MoveLowToHigh)));
                }

                context.EmitLdc_I4(op.Imm4);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesShs));

                context.EmitLdvec(op.Rm);

                context.EmitLdc_I4((op.RegisterSize == RegisterSize.Simd64 ? 8 : 16) - op.Imm4);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftLeftLogical128BitLane), typesShs));

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MoveLowToHigh)));
                }

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Or), typesOr));

                context.EmitStvec(op.Rd);
            }
            else
            {
                int bytes = op.GetBitsCount() >> 3;

                int position = op.Imm4;

                for (int index = 0; index < bytes; index++)
                {
                    int reg = op.Imm4 + index < bytes ? op.Rn : op.Rm;

                    if (position == bytes)
                    {
                        position = 0;
                    }

                    EmitVectorExtractZx(context, reg, position++, 0);
                    EmitVectorInsertTmp(context, index, 0);
                }

                context.EmitLdvectmp();
                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
        }

        public static void Fcsel_S(ILEmitterCtx context)
        {
            OpCodeSimdFcond64 op = (OpCodeSimdFcond64)context.CurrOp;

            ILLabel lblTrue = new ILLabel();
            ILLabel lblEnd  = new ILLabel();

            context.EmitCondBranch(lblTrue, op.Cond);

            EmitVectorExtractF(context, op.Rm, 0, op.Size);

            context.Emit(OpCodes.Br_S, lblEnd);

            context.MarkLabel(lblTrue);

            EmitVectorExtractF(context, op.Rn, 0, op.Size);

            context.MarkLabel(lblEnd);

            EmitScalarSetF(context, op.Rd, op.Size);
        }

        public static void Fmov_Ftoi(ILEmitterCtx context)
        {
            OpCodeSimdCvt64 op = (OpCodeSimdCvt64)context.CurrOp;

            EmitVectorExtractZx(context, op.Rn, 0, 3);

            EmitIntZeroUpperIfNeeded(context);

            context.EmitStintzr(op.Rd);
        }

        public static void Fmov_Ftoi1(ILEmitterCtx context)
        {
            OpCodeSimdCvt64 op = (OpCodeSimdCvt64)context.CurrOp;

            EmitVectorExtractZx(context, op.Rn, 1, 3);

            EmitIntZeroUpperIfNeeded(context);

            context.EmitStintzr(op.Rd);
        }

        public static void Fmov_Itof(ILEmitterCtx context)
        {
            OpCodeSimdCvt64 op = (OpCodeSimdCvt64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            EmitIntZeroUpperIfNeeded(context);

            EmitScalarSet(context, op.Rd, 3);
        }

        public static void Fmov_Itof1(ILEmitterCtx context)
        {
            OpCodeSimdCvt64 op = (OpCodeSimdCvt64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            EmitIntZeroUpperIfNeeded(context);

            EmitVectorInsert(context, op.Rd, 1, 3);
        }

        public static void Fmov_S(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            EmitVectorExtractF(context, op.Rn, 0, op.Size);

            EmitScalarSetF(context, op.Rd, op.Size);
        }

        public static void Fmov_Si(ILEmitterCtx context)
        {
            OpCodeSimdFmov64 op = (OpCodeSimdFmov64)context.CurrOp;

            context.EmitLdc_I8(op.Imm);

            EmitScalarSet(context, op.Rd, op.Size + 2);
        }

        public static void Fmov_V(ILEmitterCtx context)
        {
            OpCodeSimdImm64 op = (OpCodeSimdImm64)context.CurrOp;

            int elems = op.RegisterSize == RegisterSize.Simd128 ? 4 : 2;

            for (int index = 0; index < (elems >> op.Size); index++)
            {
                context.EmitLdc_I8(op.Imm);

                EmitVectorInsert(context, op.Rd, index, op.Size + 2);
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void Ins_Gp(ILEmitterCtx context)
        {
            OpCodeSimdIns64 op = (OpCodeSimdIns64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            EmitVectorInsert(context, op.Rd, op.DstIndex, op.Size);
        }

        public static void Ins_V(ILEmitterCtx context)
        {
            OpCodeSimdIns64 op = (OpCodeSimdIns64)context.CurrOp;

            EmitVectorExtractZx(context, op.Rn, op.SrcIndex, op.Size);

            EmitVectorInsert(context, op.Rd, op.DstIndex, op.Size);
        }

        public static void Movi_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                EmitMoviMvni(context, not: false);
            }
            else
            {
                EmitVectorImmUnaryOp(context, () => { });
            }
        }

        public static void Mvni_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                EmitMoviMvni(context, not: true);
            }
            else
            {
                EmitVectorImmUnaryOp(context, () => context.Emit(OpCodes.Not));
            }
        }

        public static void Smov_S(ILEmitterCtx context)
        {
            OpCodeSimdIns64 op = (OpCodeSimdIns64)context.CurrOp;

            EmitVectorExtractSx(context, op.Rn, op.DstIndex, op.Size);

            EmitIntZeroUpperIfNeeded(context);

            context.EmitStintzr(op.Rd);
        }

        public static void Tbl_V(ILEmitterCtx context)
        {
            OpCodeSimdTbl64 op = (OpCodeSimdTbl64)context.CurrOp;

            if (Optimizations.UseSsse3)
            {
                Type[] typesCmpSflSub = new Type[] { typeof(Vector128<sbyte>), typeof(Vector128<sbyte>) };
                Type[] typesOr        = new Type[] { typeof(Vector128<long> ), typeof(Vector128<long> ) };
                Type[] typesSav       = new Type[] { typeof(long) };

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rm);

                context.EmitLdc_I8(0x0F0F0F0F0F0F0F0FL);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                context.EmitStvectmp2();
                context.EmitLdvectmp2();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.CompareGreaterThan), typesCmpSflSub));

                context.EmitLdvec(op.Rm);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Or), typesOr));

                context.EmitCall(typeof(Ssse3).GetMethod(nameof(Ssse3.Shuffle), typesCmpSflSub));

                for (int index = 1; index < op.Size; index++)
                {
                    context.EmitLdvec((op.Rn + index) & 0x1F);
                    context.EmitLdvec(op.Rm);

                    context.EmitLdc_I8(0x1010101010101010L * index);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesCmpSflSub));

                    context.EmitStvectmp();
                    context.EmitLdvectmp();

                    context.EmitLdvectmp2();

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.CompareGreaterThan), typesCmpSflSub));

                    context.EmitLdvectmp();

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Or), typesOr));

                    context.EmitCall(typeof(Ssse3).GetMethod(nameof(Ssse3.Shuffle), typesCmpSflSub));

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Or), typesOr));
                }

                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                context.EmitLdvec(op.Rm);

                for (int index = 0; index < op.Size; index++)
                {
                    context.EmitLdvec((op.Rn + index) & 0x1F);
                }

                switch (op.Size)
                {
                    case 1: VectorHelper.EmitCall(context,
                        nameof(VectorHelper.Tbl1_V64),
                        nameof(VectorHelper.Tbl1_V128)); break;

                    case 2: VectorHelper.EmitCall(context,
                        nameof(VectorHelper.Tbl2_V64),
                        nameof(VectorHelper.Tbl2_V128)); break;

                    case 3: VectorHelper.EmitCall(context,
                        nameof(VectorHelper.Tbl3_V64),
                        nameof(VectorHelper.Tbl3_V128)); break;

                    case 4: VectorHelper.EmitCall(context,
                        nameof(VectorHelper.Tbl4_V64),
                        nameof(VectorHelper.Tbl4_V128)); break;

                    default: throw new InvalidOperationException();
                }

                context.EmitStvec(op.Rd);
            }
        }

        public static void Trn1_V(ILEmitterCtx context)
        {
            EmitVectorTranspose(context, part: 0);
        }

        public static void Trn2_V(ILEmitterCtx context)
        {
            EmitVectorTranspose(context, part: 1);
        }

        public static void Umov_S(ILEmitterCtx context)
        {
            OpCodeSimdIns64 op = (OpCodeSimdIns64)context.CurrOp;

            EmitVectorExtractZx(context, op.Rn, op.DstIndex, op.Size);

            context.EmitStintzr(op.Rd);
        }

        public static void Uzp1_V(ILEmitterCtx context)
        {
            EmitVectorUnzip(context, part: 0);
        }

        public static void Uzp2_V(ILEmitterCtx context)
        {
            EmitVectorUnzip(context, part: 1);
        }

        public static void Xtn_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            if (Optimizations.UseSsse3)
            {
                Type[] typesSve = new Type[] { typeof(long), typeof(long) };

                string nameMov = op.RegisterSize == RegisterSize.Simd128
                    ? nameof(Sse.MoveLowToHigh)
                    : nameof(Sse.MoveHighToLow);

                context.EmitLdvec(op.Rd);
                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MoveLowToHigh)));

                context.EmitLdvec(op.Rn); // value

                context.EmitLdc_I8(_masksE0_TrnUzpXtn[op.Size]); // mask
                context.Emit(OpCodes.Dup); // mask

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetVector128), typesSve));

                context.EmitCall(typeof(Ssse3).GetMethod(nameof(Ssse3.Shuffle), GetTypesSflUpk(0)));

                context.EmitCall(typeof(Sse).GetMethod(nameMov));

                context.EmitStvec(op.Rd);
            }
            else
            {
                int elems = 8 >> op.Size;

                int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

                if (part != 0)
                {
                    context.EmitLdvec(op.Rd);
                    context.EmitStvectmp();
                }

                for (int index = 0; index < elems; index++)
                {
                    EmitVectorExtractZx(context, op.Rn, index, op.Size + 1);

                    EmitVectorInsertTmp(context, part + index, op.Size);
                }

                context.EmitLdvectmp();
                context.EmitStvec(op.Rd);

                if (part == 0)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
        }

        public static void Zip1_V(ILEmitterCtx context)
        {
            EmitVectorZip(context, part: 0);
        }

        public static void Zip2_V(ILEmitterCtx context)
        {
            EmitVectorZip(context, part: 1);
        }

        private static void EmitIntZeroUpperIfNeeded(ILEmitterCtx context)
        {
            if (context.CurrOp.RegisterSize == RegisterSize.Int32 ||
                context.CurrOp.RegisterSize == RegisterSize.Simd64)
            {
                context.Emit(OpCodes.Conv_U4);
                context.Emit(OpCodes.Conv_U8);
            }
        }

        private static void EmitMoviMvni(ILEmitterCtx context, bool not)
        {
            OpCodeSimdImm64 op = (OpCodeSimdImm64)context.CurrOp;

            Type[] typesSav = new Type[] { UIntTypesPerSizeLog2[op.Size] };

            long imm = op.Imm;

            if (not)
            {
                imm = ~imm;
            }

            if (op.Size < 3)
            {
                context.EmitLdc_I4((int)imm);
            }
            else
            {
                context.EmitLdc_I8(imm);
            }

            context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

            context.EmitStvec(op.Rd);

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        private static void EmitVectorTranspose(ILEmitterCtx context, int part)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSsse3)
            {
                Type[] typesSve = new Type[] { typeof(long), typeof(long) };

                string nameUpk = part == 0
                    ? nameof(Sse2.UnpackLow)
                    : nameof(Sse2.UnpackHigh);

                context.EmitLdvec(op.Rn); // value

                if (op.Size < 3)
                {
                    context.EmitLdc_I8(_masksE1_TrnUzp   [op.Size]); // maskE1
                    context.EmitLdc_I8(_masksE0_TrnUzpXtn[op.Size]); // maskE0

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetVector128), typesSve));

                    context.EmitCall(typeof(Ssse3).GetMethod(nameof(Ssse3.Shuffle), GetTypesSflUpk(0)));
                }

                context.EmitLdvec(op.Rm); // value

                if (op.Size < 3)
                {
                    context.EmitLdc_I8(_masksE1_TrnUzp   [op.Size]); // maskE1
                    context.EmitLdc_I8(_masksE0_TrnUzpXtn[op.Size]); // maskE0

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetVector128), typesSve));

                    context.EmitCall(typeof(Ssse3).GetMethod(nameof(Ssse3.Shuffle), GetTypesSflUpk(0)));
                }

                context.EmitCall(typeof(Sse2).GetMethod(nameUpk, GetTypesSflUpk(op.Size)));

                context.EmitStvec(op.Rd);
            }
            else
            {
                int words = op.GetBitsCount() >> 4;
                int pairs = words >> op.Size;

                for (int index = 0; index < pairs; index++)
                {
                    int idx = index << 1;

                    EmitVectorExtractZx(context, op.Rn, idx + part, op.Size);
                    EmitVectorExtractZx(context, op.Rm, idx + part, op.Size);

                    EmitVectorInsertTmp(context, idx + 1, op.Size);
                    EmitVectorInsertTmp(context, idx,     op.Size);
                }

                context.EmitLdvectmp();
                context.EmitStvec(op.Rd);
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        private static void EmitVectorUnzip(ILEmitterCtx context, int part)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSsse3)
            {
                Type[] typesSve = new Type[] { typeof(long), typeof(long) };

                string nameUpk = part == 0
                    ? nameof(Sse2.UnpackLow)
                    : nameof(Sse2.UnpackHigh);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    context.EmitLdvec(op.Rn); // value

                    if (op.Size < 3)
                    {
                        context.EmitLdc_I8(_masksE1_TrnUzp   [op.Size]); // maskE1
                        context.EmitLdc_I8(_masksE0_TrnUzpXtn[op.Size]); // maskE0

                        context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetVector128), typesSve));

                        context.EmitCall(typeof(Ssse3).GetMethod(nameof(Ssse3.Shuffle), GetTypesSflUpk(0)));
                    }

                    context.EmitLdvec(op.Rm); // value

                    if (op.Size < 3)
                    {
                        context.EmitLdc_I8(_masksE1_TrnUzp   [op.Size]); // maskE1
                        context.EmitLdc_I8(_masksE0_TrnUzpXtn[op.Size]); // maskE0

                        context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetVector128), typesSve));

                        context.EmitCall(typeof(Ssse3).GetMethod(nameof(Ssse3.Shuffle), GetTypesSflUpk(0)));
                    }

                    context.EmitCall(typeof(Sse2).GetMethod(nameUpk, GetTypesSflUpk(3)));

                    context.EmitStvec(op.Rd);
                }
                else
                {
                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.UnpackLow), GetTypesSflUpk(op.Size))); // value

                    if (op.Size < 2)
                    {
                        context.EmitLdc_I8(_masksE1_Uzp[op.Size]); // maskE1
                        context.EmitLdc_I8(_masksE0_Uzp[op.Size]); // maskE0

                        context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetVector128), typesSve));

                        context.EmitCall(typeof(Ssse3).GetMethod(nameof(Ssse3.Shuffle), GetTypesSflUpk(0)));
                    }

                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInt64Zero));

                    context.EmitCall(typeof(Sse2).GetMethod(nameUpk, GetTypesSflUpk(3)));

                    context.EmitStvec(op.Rd);
                }
            }
            else
            {
                int words = op.GetBitsCount() >> 4;
                int pairs = words >> op.Size;

                for (int index = 0; index < pairs; index++)
                {
                    int idx = index << 1;

                    EmitVectorExtractZx(context, op.Rn, idx + part, op.Size);
                    EmitVectorExtractZx(context, op.Rm, idx + part, op.Size);

                    EmitVectorInsertTmp(context, pairs + index, op.Size);
                    EmitVectorInsertTmp(context,         index, op.Size);
                }

                context.EmitLdvectmp();
                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
        }

        private static void EmitVectorZip(ILEmitterCtx context, int part)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse2)
            {
                string nameUpk = part == 0
                    ? nameof(Sse2.UnpackLow)
                    : nameof(Sse2.UnpackHigh);

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rm);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    context.EmitCall(typeof(Sse2).GetMethod(nameUpk, GetTypesSflUpk(op.Size)));
                }
                else
                {
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.UnpackLow), GetTypesSflUpk(op.Size)));
                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInt64Zero));

                    context.EmitCall(typeof(Sse2).GetMethod(nameUpk, GetTypesSflUpk(3)));
                }

                context.EmitStvec(op.Rd);
            }
            else
            {
                int words = op.GetBitsCount() >> 4;
                int pairs = words >> op.Size;

                int Base = part != 0 ? pairs : 0;

                for (int index = 0; index < pairs; index++)
                {
                    int idx = index << 1;

                    EmitVectorExtractZx(context, op.Rn, Base + index, op.Size);
                    EmitVectorExtractZx(context, op.Rm, Base + index, op.Size);

                    EmitVectorInsertTmp(context, idx + 1, op.Size);
                    EmitVectorInsertTmp(context, idx,     op.Size);
                }

                context.EmitLdvectmp();
                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
        }

        private static Type[] GetTypesSflUpk(int size)
        {
            return new Type[] { VectorIntTypesPerSizeLog2[size], VectorIntTypesPerSizeLog2[size] };
        }
    }
}
