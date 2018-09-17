using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace ChocolArm64.Instruction
{
    static class AInstEmitSimdHelper
    {
        [Flags]
        public enum OperFlags
        {
            Rd = 1 << 0,
            Rn = 1 << 1,
            Rm = 1 << 2,
            Ra = 1 << 3,

            RnRm   = Rn | Rm,
            RdRn   = Rd | Rn,
            RaRnRm = Ra | Rn | Rm,
            RdRnRm = Rd | Rn | Rm
        }

        public static int GetImmShl(AOpCodeSimdShImm Op)
        {
            return Op.Imm - (8 << Op.Size);
        }

        public static int GetImmShr(AOpCodeSimdShImm Op)
        {
            return (8 << (Op.Size + 1)) - Op.Imm;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EmitSse2Call(AILEmitterCtx Context, string Name)
        {
            EmitSseCall(Context, Name, typeof(Sse2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EmitSse41Call(AILEmitterCtx Context, string Name)
        {
            EmitSseCall(Context, Name, typeof(Sse41));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EmitSse42Call(AILEmitterCtx Context, string Name)
        {
            EmitSseCall(Context, Name, typeof(Sse42));
        }

        private static void EmitSseCall(AILEmitterCtx Context, string Name, Type Type)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            void Ldvec(int Reg)
            {
                Context.EmitLdvec(Reg);

                switch (Op.Size)
                {
                    case 0: AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorSingleToSByte)); break;
                    case 1: AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorSingleToInt16)); break;
                    case 2: AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorSingleToInt32)); break;
                    case 3: AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorSingleToInt64)); break;
                }
            }

            Ldvec(Op.Rn);

            Type BaseType = null;

            switch (Op.Size)
            {
                case 0: BaseType = typeof(Vector128<sbyte>); break;
                case 1: BaseType = typeof(Vector128<short>); break;
                case 2: BaseType = typeof(Vector128<int>);   break;
                case 3: BaseType = typeof(Vector128<long>);  break;
            }

            if (Op is AOpCodeSimdReg BinOp)
            {
                Ldvec(BinOp.Rm);

                Context.EmitCall(Type.GetMethod(Name, new Type[] { BaseType, BaseType }));
            }
            else
            {
                Context.EmitCall(Type.GetMethod(Name, new Type[] { BaseType }));
            }

            switch (Op.Size)
            {
                case 0: AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorSByteToSingle)); break;
                case 1: AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorInt16ToSingle)); break;
                case 2: AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorInt32ToSingle)); break;
                case 3: AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorInt64ToSingle)); break;
            }

            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void EmitScalarSseOrSse2CallF(AILEmitterCtx Context, string Name)
        {
            EmitSseOrSse2CallF(Context, Name, true);
        }

        public static void EmitVectorSseOrSse2CallF(AILEmitterCtx Context, string Name)
        {
            EmitSseOrSse2CallF(Context, Name, false);
        }

        public static void EmitSseOrSse2CallF(AILEmitterCtx Context, string Name, bool Scalar)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            void Ldvec(int Reg)
            {
                Context.EmitLdvec(Reg);

                if (SizeF == 1)
                {
                    AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorSingleToDouble));
                }
            }

            Ldvec(Op.Rn);

            Type Type;
            Type BaseType;

            if (SizeF == 0)
            {
                Type     = typeof(Sse);
                BaseType = typeof(Vector128<float>);
            }
            else /* if (SizeF == 1) */
            {
                Type     = typeof(Sse2);
                BaseType = typeof(Vector128<double>);
            }

            if (Op is AOpCodeSimdReg BinOp)
            {
                Ldvec(BinOp.Rm);

                Context.EmitCall(Type.GetMethod(Name, new Type[] { BaseType, BaseType }));
            }
            else
            {
                Context.EmitCall(Type.GetMethod(Name, new Type[] { BaseType }));
            }

            if (SizeF == 1)
            {
                AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorDoubleToSingle));
            }

            Context.EmitStvec(Op.Rd);

            if (Scalar)
            {
                if (SizeF == 0)
                {
                    EmitVectorZero32_128(Context, Op.Rd);
                }
                else /* if (SizeF == 1) */
                {
                    EmitVectorZeroUpper(Context, Op.Rd);
                }
            }
            else if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void EmitUnaryMathCall(AILEmitterCtx Context, string Name)
        {
            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            MethodInfo MthdInfo;

            if (SizeF == 0)
            {
                MthdInfo = typeof(MathF).GetMethod(Name, new Type[] { typeof(float) });
            }
            else /* if (SizeF == 1) */
            {
                MthdInfo = typeof(Math).GetMethod(Name, new Type[] { typeof(double) });
            }

            Context.EmitCall(MthdInfo);
        }

        public static void EmitBinaryMathCall(AILEmitterCtx Context, string Name)
        {
            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            MethodInfo MthdInfo;

            if (SizeF == 0)
            {
                MthdInfo = typeof(MathF).GetMethod(Name, new Type[] { typeof(float), typeof(float) });
            }
            else /* if (SizeF == 1) */
            {
                MthdInfo = typeof(Math).GetMethod(Name, new Type[] { typeof(double), typeof(double) });
            }

            Context.EmitCall(MthdInfo);
        }

        public static void EmitRoundMathCall(AILEmitterCtx Context, MidpointRounding RoundMode)
        {
            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            Context.EmitLdc_I4((int)RoundMode);

            MethodInfo MthdInfo;

            Type[] Types = new Type[] { null, typeof(MidpointRounding) };

            Types[0] = SizeF == 0
                ? typeof(float)
                : typeof(double);

            if (SizeF == 0)
            {
                MthdInfo = typeof(MathF).GetMethod(nameof(MathF.Round), Types);
            }
            else /* if (SizeF == 1) */
            {
                MthdInfo = typeof(Math).GetMethod(nameof(Math.Round), Types);
            }

            Context.EmitCall(MthdInfo);
        }

        public static void EmitUnarySoftFloatCall(AILEmitterCtx Context, string Name)
        {
            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            MethodInfo MthdInfo;

            if (SizeF == 0)
            {
                MthdInfo = typeof(ASoftFloat).GetMethod(Name, new Type[] { typeof(float) });
            }
            else /* if (SizeF == 1) */
            {
                MthdInfo = typeof(ASoftFloat).GetMethod(Name, new Type[] { typeof(double) });
            }

            Context.EmitCall(MthdInfo);
        }

        public static void EmitBinarySoftFloatCall(AILEmitterCtx Context, string Name)
        {
            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            MethodInfo MthdInfo;

            if (SizeF == 0)
            {
                MthdInfo = typeof(ASoftFloat).GetMethod(Name, new Type[] { typeof(float), typeof(float) });
            }
            else /* if (SizeF == 1) */
            {
                MthdInfo = typeof(ASoftFloat).GetMethod(Name, new Type[] { typeof(double), typeof(double) });
            }

            Context.EmitCall(MthdInfo);
        }

        public static void EmitScalarBinaryOpByElemF(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElemF Op = (AOpCodeSimdRegElemF)Context.CurrOp;

            EmitScalarOpByElemF(Context, Emit, Op.Index, Ternary: false);
        }

        public static void EmitScalarTernaryOpByElemF(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElemF Op = (AOpCodeSimdRegElemF)Context.CurrOp;

            EmitScalarOpByElemF(Context, Emit, Op.Index, Ternary: true);
        }

        public static void EmitScalarOpByElemF(AILEmitterCtx Context, Action Emit, int Elem, bool Ternary)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int SizeF = Op.Size & 1;

            if (Ternary)
            {
                EmitVectorExtractF(Context, Op.Rd, 0, SizeF);
            }

            EmitVectorExtractF(Context, Op.Rn, 0,    SizeF);
            EmitVectorExtractF(Context, Op.Rm, Elem, SizeF);

            Emit();

            EmitScalarSetF(Context, Op.Rd, SizeF);
        }

        public static void EmitScalarUnaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOp(Context, Emit, OperFlags.Rn, true);
        }

        public static void EmitScalarBinaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOp(Context, Emit, OperFlags.RnRm, true);
        }

        public static void EmitScalarUnaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOp(Context, Emit, OperFlags.Rn, false);
        }

        public static void EmitScalarBinaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOp(Context, Emit, OperFlags.RnRm, false);
        }

        public static void EmitScalarTernaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOp(Context, Emit, OperFlags.RdRnRm, false);
        }

        public static void EmitScalarOp(AILEmitterCtx Context, Action Emit, OperFlags Opers, bool Signed)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            bool Rd = (Opers & OperFlags.Rd) != 0;
            bool Rn = (Opers & OperFlags.Rn) != 0;
            bool Rm = (Opers & OperFlags.Rm) != 0;

            if (Rd)
            {
                EmitVectorExtract(Context, Op.Rd, 0, Op.Size, Signed);
            }

            if (Rn)
            {
                EmitVectorExtract(Context, Op.Rn, 0, Op.Size, Signed);
            }

            if (Rm)
            {
                EmitVectorExtract(Context, ((AOpCodeSimdReg)Op).Rm, 0, Op.Size, Signed);
            }

            Emit();

            EmitScalarSet(Context, Op.Rd, Op.Size);
        }

        public static void EmitScalarUnaryOpF(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOpF(Context, Emit, OperFlags.Rn);
        }

        public static void EmitScalarBinaryOpF(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOpF(Context, Emit, OperFlags.RnRm);
        }

        public static void EmitScalarTernaryRaOpF(AILEmitterCtx Context, Action Emit)
        {
            EmitScalarOpF(Context, Emit, OperFlags.RaRnRm);
        }

        public static void EmitScalarOpF(AILEmitterCtx Context, Action Emit, OperFlags Opers)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            bool Ra = (Opers & OperFlags.Ra) != 0;
            bool Rn = (Opers & OperFlags.Rn) != 0;
            bool Rm = (Opers & OperFlags.Rm) != 0;

            if (Ra)
            {
                EmitVectorExtractF(Context, ((AOpCodeSimdReg)Op).Ra, 0, SizeF);
            }

            if (Rn)
            {
                EmitVectorExtractF(Context, Op.Rn, 0, SizeF);
            }

            if (Rm)
            {
                EmitVectorExtractF(Context, ((AOpCodeSimdReg)Op).Rm, 0, SizeF);
            }

            Emit();

            EmitScalarSetF(Context, Op.Rd, SizeF);
        }

        public static void EmitVectorUnaryOpF(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOpF(Context, Emit, OperFlags.Rn);
        }

        public static void EmitVectorBinaryOpF(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOpF(Context, Emit, OperFlags.RnRm);
        }

        public static void EmitVectorTernaryOpF(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOpF(Context, Emit, OperFlags.RdRnRm);
        }

        public static void EmitVectorOpF(AILEmitterCtx Context, Action Emit, OperFlags Opers)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = Bytes >> SizeF + 2;

            bool Rd = (Opers & OperFlags.Rd) != 0;
            bool Rn = (Opers & OperFlags.Rn) != 0;
            bool Rm = (Opers & OperFlags.Rm) != 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                if (Rd)
                {
                    EmitVectorExtractF(Context, Op.Rd, Index, SizeF);
                }

                if (Rn)
                {
                    EmitVectorExtractF(Context, Op.Rn, Index, SizeF);
                }

                if (Rm)
                {
                    EmitVectorExtractF(Context, ((AOpCodeSimdReg)Op).Rm, Index, SizeF);
                }

                Emit();

                EmitVectorInsertF(Context, Op.Rd, Index, SizeF);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void EmitVectorBinaryOpByElemF(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElemF Op = (AOpCodeSimdRegElemF)Context.CurrOp;

            EmitVectorOpByElemF(Context, Emit, Op.Index, Ternary: false);
        }

        public static void EmitVectorTernaryOpByElemF(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElemF Op = (AOpCodeSimdRegElemF)Context.CurrOp;

            EmitVectorOpByElemF(Context, Emit, Op.Index, Ternary: true);
        }

        public static void EmitVectorOpByElemF(AILEmitterCtx Context, Action Emit, int Elem, bool Ternary)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int SizeF = Op.Size & 1;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = Bytes >> SizeF + 2;

            for (int Index = 0; Index < Elems; Index++)
            {
                if (Ternary)
                {
                    EmitVectorExtractF(Context, Op.Rd, Index, SizeF);
                }

                EmitVectorExtractF(Context, Op.Rn, Index, SizeF);
                EmitVectorExtractF(Context, Op.Rm, Elem,  SizeF);

                Emit();

                EmitVectorInsertTmpF(Context, Index, SizeF);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void EmitVectorUnaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.Rn, true);
        }

        public static void EmitVectorBinaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.RnRm, true);
        }

        public static void EmitVectorTernaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.RdRnRm, true);
        }

        public static void EmitVectorUnaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.Rn, false);
        }

        public static void EmitVectorBinaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.RnRm, false);
        }

        public static void EmitVectorTernaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.RdRnRm, false);
        }

        public static void EmitVectorOp(AILEmitterCtx Context, Action Emit, OperFlags Opers, bool Signed)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = Bytes >> Op.Size;

            bool Rd = (Opers & OperFlags.Rd) != 0;
            bool Rn = (Opers & OperFlags.Rn) != 0;
            bool Rm = (Opers & OperFlags.Rm) != 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                if (Rd)
                {
                    EmitVectorExtract(Context, Op.Rd, Index, Op.Size, Signed);
                }

                if (Rn)
                {
                    EmitVectorExtract(Context, Op.Rn, Index, Op.Size, Signed);
                }

                if (Rm)
                {
                    EmitVectorExtract(Context, ((AOpCodeSimdReg)Op).Rm, Index, Op.Size, Signed);
                }

                Emit();

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void EmitVectorBinaryOpByElemSx(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElem Op = (AOpCodeSimdRegElem)Context.CurrOp;

            EmitVectorOpByElem(Context, Emit, Op.Index, false, true);
        }

        public static void EmitVectorBinaryOpByElemZx(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElem Op = (AOpCodeSimdRegElem)Context.CurrOp;

            EmitVectorOpByElem(Context, Emit, Op.Index, false, false);
        }

        public static void EmitVectorTernaryOpByElemZx(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElem Op = (AOpCodeSimdRegElem)Context.CurrOp;

            EmitVectorOpByElem(Context, Emit, Op.Index, true, false);
        }

        public static void EmitVectorOpByElem(AILEmitterCtx Context, Action Emit, int Elem, bool Ternary, bool Signed)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = Bytes >> Op.Size;

            EmitVectorExtract(Context, Op.Rm, Elem, Op.Size, Signed);
            Context.EmitSttmp();

            for (int Index = 0; Index < Elems; Index++)
            {
                if (Ternary)
                {
                    EmitVectorExtract(Context, Op.Rd, Index, Op.Size, Signed);
                }

                EmitVectorExtract(Context, Op.Rn, Index, Op.Size, Signed);
                Context.EmitLdtmp();

                Emit();

                EmitVectorInsertTmp(Context, Index, Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void EmitVectorImmUnaryOp(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorImmOp(Context, Emit, false);
        }

        public static void EmitVectorImmBinaryOp(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorImmOp(Context, Emit, true);
        }

        public static void EmitVectorImmOp(AILEmitterCtx Context, Action Emit, bool Binary)
        {
            AOpCodeSimdImm Op = (AOpCodeSimdImm)Context.CurrOp;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = Bytes >> Op.Size;

            for (int Index = 0; Index < Elems; Index++)
            {
                if (Binary)
                {
                    EmitVectorExtractZx(Context, Op.Rd, Index, Op.Size);
                }

                Context.EmitLdc_I8(Op.Imm);

                Emit();

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void EmitVectorWidenRmBinaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenRmBinaryOp(Context, Emit, true);
        }

        public static void EmitVectorWidenRmBinaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenRmBinaryOp(Context, Emit, false);
        }

        public static void EmitVectorWidenRmBinaryOp(AILEmitterCtx Context, Action Emit, bool Signed)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Elems = 8 >> Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtract(Context, Op.Rn,        Index, Op.Size + 1, Signed);
                EmitVectorExtract(Context, Op.Rm, Part + Index, Op.Size,     Signed);

                Emit();

                EmitVectorInsertTmp(Context, Index, Op.Size + 1);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);
        }

        public static void EmitVectorWidenRnRmBinaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenRnRmOp(Context, Emit, false, true);
        }

        public static void EmitVectorWidenRnRmBinaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenRnRmOp(Context, Emit, false, false);
        }

        public static void EmitVectorWidenRnRmTernaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenRnRmOp(Context, Emit, true, true);
        }

        public static void EmitVectorWidenRnRmTernaryOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenRnRmOp(Context, Emit, true, false);
        }

        public static void EmitVectorWidenRnRmOp(AILEmitterCtx Context, Action Emit, bool Ternary, bool Signed)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Elems = 8 >> Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                if (Ternary)
                {
                    EmitVectorExtract(Context, Op.Rd, Index, Op.Size + 1, Signed);
                }

                EmitVectorExtract(Context, Op.Rn, Part + Index, Op.Size, Signed);
                EmitVectorExtract(Context, Op.Rm, Part + Index, Op.Size, Signed);

                Emit();

                EmitVectorInsertTmp(Context, Index, Op.Size + 1);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);
        }

        public static void EmitVectorPairwiseOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorPairwiseOp(Context, Emit, true);
        }

        public static void EmitVectorPairwiseOpZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorPairwiseOp(Context, Emit, false);
        }

        public static void EmitVectorPairwiseOp(AILEmitterCtx Context, Action Emit, bool Signed)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Words = Op.GetBitsCount() >> 4;
            int Pairs = Words >> Op.Size;

            for (int Index = 0; Index < Pairs; Index++)
            {
                int Idx = Index << 1;

                EmitVectorExtract(Context, Op.Rn, Idx,     Op.Size, Signed);
                EmitVectorExtract(Context, Op.Rn, Idx + 1, Op.Size, Signed);

                Emit();

                EmitVectorExtract(Context, Op.Rm, Idx,     Op.Size, Signed);
                EmitVectorExtract(Context, Op.Rm, Idx + 1, Op.Size, Signed);

                Emit();

                EmitVectorInsertTmp(Context, Pairs + Index, Op.Size);
                EmitVectorInsertTmp(Context,         Index, Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        [Flags]
        public enum SaturatingFlags
        {
            Scalar = 1 << 0,
            Signed = 1 << 1,

            Add = 1 << 2,
            Sub = 1 << 3,

            Accumulate = 1 << 4,

            ScalarSx = Scalar | Signed,
            ScalarZx = Scalar,

            VectorSx = Signed,
            VectorZx = 0
        }

        public static void EmitScalarSaturatingUnaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitSaturatingUnaryOpSx(Context, Emit, SaturatingFlags.ScalarSx);
        }

        public static void EmitVectorSaturatingUnaryOpSx(AILEmitterCtx Context, Action Emit)
        {
            EmitSaturatingUnaryOpSx(Context, Emit, SaturatingFlags.VectorSx);
        }

        public static void EmitSaturatingUnaryOpSx(AILEmitterCtx Context, Action Emit, SaturatingFlags Flags)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            bool Scalar = (Flags & SaturatingFlags.Scalar) != 0;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = !Scalar ? Bytes >> Op.Size : 1;

            if (Scalar)
            {
                EmitVectorZeroLowerTmp(Context);
            }

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractSx(Context, Op.Rn, Index, Op.Size);

                Emit();

                if (Op.Size <= 2)
                {
                    EmitSatQ(Context, Op.Size, true, true);
                }
                else /* if (Op.Size == 3) */
                {
                    EmitUnarySignedSatQAbsOrNeg(Context);
                }

                EmitVectorInsertTmp(Context, Index, Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if ((Op.RegisterSize == ARegisterSize.SIMD64) || Scalar)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void EmitScalarSaturatingBinaryOpSx(AILEmitterCtx Context, SaturatingFlags Flags)
        {
            EmitSaturatingBinaryOp(Context, () => { }, SaturatingFlags.ScalarSx | Flags);
        }

        public static void EmitScalarSaturatingBinaryOpZx(AILEmitterCtx Context, SaturatingFlags Flags)
        {
            EmitSaturatingBinaryOp(Context, () => { }, SaturatingFlags.ScalarZx | Flags);
        }

        public static void EmitVectorSaturatingBinaryOpSx(AILEmitterCtx Context, SaturatingFlags Flags)
        {
            EmitSaturatingBinaryOp(Context, () => { }, SaturatingFlags.VectorSx | Flags);
        }

        public static void EmitVectorSaturatingBinaryOpZx(AILEmitterCtx Context, SaturatingFlags Flags)
        {
            EmitSaturatingBinaryOp(Context, () => { }, SaturatingFlags.VectorZx | Flags);
        }

        public static void EmitSaturatingBinaryOp(AILEmitterCtx Context, Action Emit, SaturatingFlags Flags)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            bool Scalar = (Flags & SaturatingFlags.Scalar) != 0;
            bool Signed = (Flags & SaturatingFlags.Signed) != 0;

            bool Add = (Flags & SaturatingFlags.Add) != 0;
            bool Sub = (Flags & SaturatingFlags.Sub) != 0;

            bool Accumulate = (Flags & SaturatingFlags.Accumulate) != 0;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = !Scalar ? Bytes >> Op.Size : 1;

            if (Scalar)
            {
                EmitVectorZeroLowerTmp(Context);
            }

            if (Add || Sub)
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    EmitVectorExtract(Context,                   Op.Rn, Index, Op.Size, Signed);
                    EmitVectorExtract(Context, ((AOpCodeSimdReg)Op).Rm, Index, Op.Size, Signed);

                    if (Op.Size <= 2)
                    {
                        Context.Emit(Add ? OpCodes.Add : OpCodes.Sub);

                        EmitSatQ(Context, Op.Size, true, Signed);
                    }
                    else /* if (Op.Size == 3) */
                    {
                        if (Add)
                        {
                            EmitBinarySatQAdd(Context, Signed);
                        }
                        else /* if (Sub) */
                        {
                            EmitBinarySatQSub(Context, Signed);
                        }
                    }

                    EmitVectorInsertTmp(Context, Index, Op.Size);
                }
            }
            else if (Accumulate)
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    EmitVectorExtract(Context, Op.Rn, Index, Op.Size, !Signed);
                    EmitVectorExtract(Context, Op.Rd, Index, Op.Size,  Signed);

                    if (Op.Size <= 2)
                    {
                        Context.Emit(OpCodes.Add);

                        EmitSatQ(Context, Op.Size, true, Signed);
                    }
                    else /* if (Op.Size == 3) */
                    {
                        EmitBinarySatQAccumulate(Context, Signed);
                    }

                    EmitVectorInsertTmp(Context, Index, Op.Size);
                }
            }
            else
            {
                for (int Index = 0; Index < Elems; Index++)
                {
                    EmitVectorExtract(Context,                   Op.Rn, Index, Op.Size, Signed);
                    EmitVectorExtract(Context, ((AOpCodeSimdReg)Op).Rm, Index, Op.Size, Signed);

                    Emit();

                    EmitSatQ(Context, Op.Size, true, Signed);

                    EmitVectorInsertTmp(Context, Index, Op.Size);
                }
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if ((Op.RegisterSize == ARegisterSize.SIMD64) || Scalar)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        [Flags]
        public enum SaturatingNarrowFlags
        {
            Scalar    = 1 << 0,
            SignedSrc = 1 << 1,
            SignedDst = 1 << 2,

            ScalarSxSx = Scalar | SignedSrc | SignedDst,
            ScalarSxZx = Scalar | SignedSrc,
            ScalarZxZx = Scalar,

            VectorSxSx = SignedSrc | SignedDst,
            VectorSxZx = SignedSrc,
            VectorZxZx = 0
        }

        public static void EmitSaturatingNarrowOp(AILEmitterCtx Context, SaturatingNarrowFlags Flags)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            bool Scalar    = (Flags & SaturatingNarrowFlags.Scalar)    != 0;
            bool SignedSrc = (Flags & SaturatingNarrowFlags.SignedSrc) != 0;
            bool SignedDst = (Flags & SaturatingNarrowFlags.SignedDst) != 0;

            int Elems = !Scalar ? 8 >> Op.Size : 1;

            int Part = !Scalar && (Op.RegisterSize == ARegisterSize.SIMD128) ? Elems : 0;

            if (Scalar)
            {
                EmitVectorZeroLowerTmp(Context);
            }

            if (Part != 0)
            {
                Context.EmitLdvec(Op.Rd);
                Context.EmitStvectmp();
            }

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtract(Context, Op.Rn, Index, Op.Size + 1, SignedSrc);

                EmitSatQ(Context, Op.Size, SignedSrc, SignedDst);

                EmitVectorInsertTmp(Context, Part + Index, Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Part == 0)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        // TSrc (16bit, 32bit, 64bit; signed, unsigned) > TDst (8bit, 16bit, 32bit; signed, unsigned).
        public static void EmitSatQ(
            AILEmitterCtx Context,
            int  SizeDst,
            bool SignedSrc,
            bool SignedDst)
        {
            if (SizeDst > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(SizeDst));
            }

            Context.EmitLdc_I4(SizeDst);
            Context.EmitLdarg(ATranslatedSub.StateArgIdx);

            if (SignedSrc)
            {
                ASoftFallback.EmitCall(Context, SignedDst
                    ? nameof(ASoftFallback.SignedSrcSignedDstSatQ)
                    : nameof(ASoftFallback.SignedSrcUnsignedDstSatQ));
            }
            else
            {
                ASoftFallback.EmitCall(Context, SignedDst
                    ? nameof(ASoftFallback.UnsignedSrcSignedDstSatQ)
                    : nameof(ASoftFallback.UnsignedSrcUnsignedDstSatQ));
            }
        }

        // TSrc (64bit) == TDst (64bit); signed.
        public static void EmitUnarySignedSatQAbsOrNeg(AILEmitterCtx Context)
        {
            if (((AOpCodeSimd)Context.CurrOp).Size < 3)
            {
                throw new InvalidOperationException();
            }

            Context.EmitLdarg(ATranslatedSub.StateArgIdx);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.UnarySignedSatQAbsOrNeg));
        }

        // TSrcs (64bit) == TDst (64bit); signed, unsigned.
        public static void EmitBinarySatQAdd(AILEmitterCtx Context, bool Signed)
        {
            if (((AOpCodeSimdReg)Context.CurrOp).Size < 3)
            {
                throw new InvalidOperationException();
            }

            Context.EmitLdarg(ATranslatedSub.StateArgIdx);

            ASoftFallback.EmitCall(Context, Signed
                ? nameof(ASoftFallback.BinarySignedSatQAdd)
                : nameof(ASoftFallback.BinaryUnsignedSatQAdd));
        }

        // TSrcs (64bit) == TDst (64bit); signed, unsigned.
        public static void EmitBinarySatQSub(AILEmitterCtx Context, bool Signed)
        {
            if (((AOpCodeSimdReg)Context.CurrOp).Size < 3)
            {
                throw new InvalidOperationException();
            }

            Context.EmitLdarg(ATranslatedSub.StateArgIdx);

            ASoftFallback.EmitCall(Context, Signed
                ? nameof(ASoftFallback.BinarySignedSatQSub)
                : nameof(ASoftFallback.BinaryUnsignedSatQSub));
        }

        // TSrcs (64bit) == TDst (64bit); signed, unsigned.
        public static void EmitBinarySatQAccumulate(AILEmitterCtx Context, bool Signed)
        {
            if (((AOpCodeSimd)Context.CurrOp).Size < 3)
            {
                throw new InvalidOperationException();
            }

            Context.EmitLdarg(ATranslatedSub.StateArgIdx);

            ASoftFallback.EmitCall(Context, Signed
                ? nameof(ASoftFallback.BinarySignedSatQAcc)
                : nameof(ASoftFallback.BinaryUnsignedSatQAcc));
        }

        public static void EmitScalarSet(AILEmitterCtx Context, int Reg, int Size)
        {
            EmitVectorZeroAll(Context, Reg);
            EmitVectorInsert(Context, Reg, 0, Size);
        }

        public static void EmitScalarSetF(AILEmitterCtx Context, int Reg, int Size)
        {
            EmitVectorZeroAll(Context, Reg);
            EmitVectorInsertF(Context, Reg, 0, Size);
        }

        public static void EmitVectorExtractSx(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            EmitVectorExtract(Context, Reg, Index, Size, true);
        }

        public static void EmitVectorExtractZx(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            EmitVectorExtract(Context, Reg, Index, Size, false);
        }

        public static void EmitVectorExtract(AILEmitterCtx Context, int Reg, int Index, int Size, bool Signed)
        {
            ThrowIfInvalid(Index, Size);

            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);

            AVectorHelper.EmitCall(Context, Signed
                ? nameof(AVectorHelper.VectorExtractIntSx)
                : nameof(AVectorHelper.VectorExtractIntZx));
        }

        public static void EmitVectorExtractF(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            ThrowIfInvalidF(Index, Size);

            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);

            if (Size == 0)
            {
                AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorExtractSingle));
            }
            else if (Size == 1)
            {
                AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorExtractDouble));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }
        }

        public static void EmitVectorZeroAll(AILEmitterCtx Context, int Rd)
        {
            EmitVectorZeroLower(Context, Rd);
            EmitVectorZeroUpper(Context, Rd);
        }

        public static void EmitVectorZeroLower(AILEmitterCtx Context, int Rd)
        {
            EmitVectorInsert(Context, Rd, 0, 3, 0);
        }

        public static void EmitVectorZeroLowerTmp(AILEmitterCtx Context)
        {
            EmitVectorInsertTmp(Context, 0, 3, 0);
        }

        public static void EmitVectorZeroUpper(AILEmitterCtx Context, int Rd)
        {
            EmitVectorInsert(Context, Rd, 1, 3, 0);
        }

        public static void EmitVectorZero32_128(AILEmitterCtx Context, int Reg)
        {
            Context.EmitLdvec(Reg);

            AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorZero32_128));

            Context.EmitStvec(Reg);
        }

        public static void EmitVectorInsert(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            ThrowIfInvalid(Index, Size);

            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);

            AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorInsertInt));

            Context.EmitStvec(Reg);
        }

        public static void EmitVectorInsertTmp(AILEmitterCtx Context, int Index, int Size)
        {
            ThrowIfInvalid(Index, Size);

            Context.EmitLdvectmp();
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);

            AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorInsertInt));

            Context.EmitStvectmp();
        }

        public static void EmitVectorInsert(AILEmitterCtx Context, int Reg, int Index, int Size, long Value)
        {
            ThrowIfInvalid(Index, Size);

            Context.EmitLdc_I8(Value);
            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);

            AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorInsertInt));

            Context.EmitStvec(Reg);
        }

        public static void EmitVectorInsertTmp(AILEmitterCtx Context, int Index, int Size, long Value)
        {
            ThrowIfInvalid(Index, Size);

            Context.EmitLdc_I8(Value);
            Context.EmitLdvectmp();
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);

            AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorInsertInt));

            Context.EmitStvectmp();
        }

        public static void EmitVectorInsertF(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            ThrowIfInvalidF(Index, Size);

            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);

            if (Size == 0)
            {
                AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorInsertSingle));
            }
            else if (Size == 1)
            {
                AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorInsertDouble));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            Context.EmitStvec(Reg);
        }

        public static void EmitVectorInsertTmpF(AILEmitterCtx Context, int Index, int Size)
        {
            ThrowIfInvalidF(Index, Size);

            Context.EmitLdvectmp();
            Context.EmitLdc_I4(Index);

            if (Size == 0)
            {
                AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorInsertSingle));
            }
            else if (Size == 1)
            {
                AVectorHelper.EmitCall(Context, nameof(AVectorHelper.VectorInsertDouble));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            Context.EmitStvectmp();
        }

        private static void ThrowIfInvalid(int Index, int Size)
        {
            if ((uint)Size > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            if ((uint)Index >= 16 >> Size)
            {
                throw new ArgumentOutOfRangeException(nameof(Index));
            }
        }

        private static void ThrowIfInvalidF(int Index, int Size)
        {
            if ((uint)Size > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            if ((uint)Index >= 4 >> Size)
            {
                throw new ArgumentOutOfRangeException(nameof(Index));
            }
        }
    }
}
