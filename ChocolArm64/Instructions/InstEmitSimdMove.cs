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

                EmitStvecWithUnsignedCast(context, op.Rd, op.Size);
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

                EmitStvecWithUnsignedCast(context, op.Rd, op.Size);
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

                EmitLdvecWithUnsignedCast(context, op.Rn, 0);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MoveLowToHigh)));
                }

                context.EmitLdc_I4(op.Imm4);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesShs));

                EmitLdvecWithUnsignedCast(context, op.Rm, 0);

                context.EmitLdc_I4((op.RegisterSize == RegisterSize.Simd64 ? 8 : 16) - op.Imm4);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftLeftLogical128BitLane), typesShs));

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MoveLowToHigh)));
                }

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Or), typesOr));

                EmitStvecWithUnsignedCast(context, op.Rd, 0);
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
            EmitVectorImmUnaryOp(context, () => { });
        }

        public static void Mvni_V(ILEmitterCtx context)
        {
            EmitVectorImmUnaryOp(context, () => context.Emit(OpCodes.Not));
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

            context.EmitLdvec(op.Rm);

            for (int index = 0; index < op.Size; index++)
            {
                context.EmitLdvec((op.Rn + index) & 0x1f);
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

            int elems = 8 >> op.Size;

            int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

            if (Optimizations.UseSse41 && op.Size < 2)
            {
                void EmitZeroVector()
                {
                    switch (op.Size)
                    {
                        case 0: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInt16Zero)); break;
                        case 1: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInt32Zero)); break;
                    }
                }

                //For XTN, first operand is source, second operand is 0.
                //For XTN2, first operand is 0, second operand is source.
                if (part != 0)
                {
                    EmitZeroVector();
                }

                EmitLdvecWithSignedCast(context, op.Rn, op.Size + 1);

                //Set mask to discard the upper half of the wide elements.
                switch (op.Size)
                {
                    case 0: context.EmitLdc_I4(0x00ff);     break;
                    case 1: context.EmitLdc_I4(0x0000ffff); break;
                }

                Type wideType = IntTypesPerSizeLog2[op.Size + 1];

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), new Type[] { wideType }));

                wideType = VectorIntTypesPerSizeLog2[op.Size + 1];

                Type[] wideTypes = new Type[] { wideType, wideType };

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.And), wideTypes));

                if (part == 0)
                {
                    EmitZeroVector();
                }

                //Pack values with signed saturation, the signed saturation shouldn't
                //saturate anything since the upper bits were masked off.
                Type sseType = op.Size == 0 ? typeof(Sse2) : typeof(Sse41);

                context.EmitCall(sseType.GetMethod(nameof(Sse2.PackUnsignedSaturate), wideTypes));

                if (part != 0)
                {
                    //For XTN2, we additionally need to discard the upper bits
                    //of the target register and OR the result with it.
                    EmitVectorZeroUpper(context, op.Rd);

                    EmitLdvecWithUnsignedCast(context, op.Rd, op.Size);

                    Type narrowType = VectorUIntTypesPerSizeLog2[op.Size];

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Or), new Type[] { narrowType, narrowType }));
                }

                EmitStvecWithUnsignedCast(context, op.Rd, op.Size);
            }
            else
            {
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

        private static void EmitVectorTranspose(ILEmitterCtx context, int part)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

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

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        private static void EmitVectorUnzip(ILEmitterCtx context, int part)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

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

        private static void EmitVectorZip(ILEmitterCtx context, int part)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse2)
            {
                EmitLdvecWithUnsignedCast(context, op.Rn, op.Size);
                EmitLdvecWithUnsignedCast(context, op.Rm, op.Size);

                Type[] types = new Type[]
                {
                    VectorUIntTypesPerSizeLog2[op.Size],
                    VectorUIntTypesPerSizeLog2[op.Size]
                };

                string name = part == 0 || (part != 0 && op.RegisterSize == RegisterSize.Simd64)
                    ? nameof(Sse2.UnpackLow)
                    : nameof(Sse2.UnpackHigh);

                context.EmitCall(typeof(Sse2).GetMethod(name, types));

                if (op.RegisterSize == RegisterSize.Simd64 && part != 0)
                {
                    context.EmitLdc_I4(8);

                    Type[] shTypes = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], typeof(byte) };

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), shTypes));
                }

                EmitStvecWithUnsignedCast(context, op.Rd, op.Size);

                if (op.RegisterSize == RegisterSize.Simd64 && part == 0)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
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
    }
}
