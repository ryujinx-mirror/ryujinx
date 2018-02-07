using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Addp_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Addp_S));

            Context.EmitStvec(Op.Rd);
        }

        public static void Dup_S(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4(Op.DstIndex);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Dup_S));

            Context.EmitStvec(Op.Rd);
        }

        public static void Fabs_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvecsf(Op.Rn);

            MethodInfo MthdInfo;

            if (Op.Size == 0)
            {
                MthdInfo = typeof(MathF).GetMethod(nameof(MathF.Abs), new Type[] { typeof(float) });
            }
            else if (Op.Size == 1)
            {
                MthdInfo = typeof(Math).GetMethod(nameof(Math.Abs), new Type[] { typeof(double) });
            }
            else
            {
                throw new InvalidOperationException();
            }

            Context.EmitCall(MthdInfo);

            Context.EmitStvecsf(Op.Rd);
        }

        public static void Fadd_S(AILEmitterCtx Context) => EmitScalarOp(Context, OpCodes.Add);

        public static void Fccmp_S(AILEmitterCtx Context)
        {
            AOpCodeSimdFcond Op = (AOpCodeSimdFcond)Context.CurrOp;

            AILLabel LblTrue = new AILLabel();
            AILLabel LblEnd  = new AILLabel();

            Context.EmitCondBranch(LblTrue, Op.Cond);

            //TODO: Share this logic with Ccmp.
            Context.EmitLdc_I4((Op.NZCV >> 0) & 1);

            Context.EmitStflg((int)APState.VBit);

            Context.EmitLdc_I4((Op.NZCV >> 1) & 1);

            Context.EmitStflg((int)APState.CBit);

            Context.EmitLdc_I4((Op.NZCV >> 2) & 1);

            Context.EmitStflg((int)APState.ZBit);

            Context.EmitLdc_I4((Op.NZCV >> 3) & 1);

            Context.EmitStflg((int)APState.NBit);

            Context.Emit(OpCodes.Br_S, LblEnd);

            Context.MarkLabel(LblTrue);

            Fcmp_S(Context);

            Context.MarkLabel(LblEnd);
        }

        public static void Fcmp_S(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            bool CmpWithZero = !(Op is AOpCodeSimdFcond) ? Op.Bit3 : false;

            //todo
            //Context.TryMarkCondWithoutCmp();

            void EmitLoadOpers()
            {
                Context.EmitLdvecsf(Op.Rn);

                if (CmpWithZero)
                {
                    EmitLdcImmF(Context, 0, Op.Size);
                }
                else
                {
                    Context.EmitLdvecsf(Op.Rm);
                }
            }

            //Z = Rn == Rm
            EmitLoadOpers();
            
            Context.Emit(OpCodes.Ceq);
            Context.Emit(OpCodes.Dup);

            Context.EmitStflg((int)APState.ZBit);

            //C = Rn >= Rm
            EmitLoadOpers();

            Context.Emit(OpCodes.Cgt);
            Context.Emit(OpCodes.Or);

            Context.EmitStflg((int)APState.CBit);

            //N = Rn < Rm
            EmitLoadOpers();

            Context.Emit(OpCodes.Clt);

            Context.EmitStflg((int)APState.NBit);

            //Handle NaN case. If any number is NaN, then NZCV = 0011.
            AILLabel LblNotNaN = new AILLabel();

            if (CmpWithZero)
            {
                EmitNaNCheck(Context, Op.Rn);
            }
            else
            {
                EmitNaNCheck(Context, Op.Rn);
                EmitNaNCheck(Context, Op.Rm);

                Context.Emit(OpCodes.Or);
            }

            Context.Emit(OpCodes.Brfalse_S, LblNotNaN);

            Context.EmitLdc_I4(1);
            Context.EmitLdc_I4(1);

            Context.EmitStflg((int)APState.CBit);
            Context.EmitStflg((int)APState.VBit);

            Context.MarkLabel(LblNotNaN);
        }

        public static void Fcmpe_S(AILEmitterCtx Context)
        {
            //TODO: Raise exception if value is NaN, how to handle exceptions?
            Fcmp_S(Context);
        }

        public static void Fcsel_S(AILEmitterCtx Context)
        {
            AOpCodeSimdFcond Op = (AOpCodeSimdFcond)Context.CurrOp;

            AILLabel LblTrue = new AILLabel();
            AILLabel LblEnd  = new AILLabel();

            Context.EmitCondBranch(LblTrue, Op.Cond);
            Context.EmitLdvecsf(Op.Rm);
            Context.EmitStvecsf(Op.Rd);

            Context.Emit(OpCodes.Br_S, LblEnd);

            Context.MarkLabel(LblTrue);

            Context.EmitLdvecsf(Op.Rn);
            Context.EmitStvecsf(Op.Rd);

            Context.MarkLabel(LblEnd);
        }

        public static void Fcvt_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvecsf(Op.Rn);

            EmitFloatCast(Context, Op.Opc);

            Context.EmitStvecsf(Op.Rd);
        }

        public static void Fcvtms_S(AILEmitterCtx Context) => EmitMathOpCvtToInt(Context, nameof(Math.Floor));
        public static void Fcvtps_S(AILEmitterCtx Context) => EmitMathOpCvtToInt(Context, nameof(Math.Ceiling));

        public static void Fcvtzs_S(AILEmitterCtx Context) => EmitFcvtz_(Context, true);
        public static void Fcvtzu_S(AILEmitterCtx Context) => EmitFcvtz_(Context, false);

        private static void EmitFcvtz_(AILEmitterCtx Context, bool Signed)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            Context.EmitLdvecsf(Op.Rn);

            if (Signed)
            {
                EmitCvtToInt(Context, Op.Size);
            }
            else
            {
                EmitCvtToUInt(Context, Op.Size);
            }

            Context.EmitStintzr(Op.Rd);
        }

        public static void Fcvtzs_Fix(AILEmitterCtx Context) => EmitFcvtz__Fix(Context, true);
        public static void Fcvtzu_Fix(AILEmitterCtx Context) => EmitFcvtz__Fix(Context, false);

        private static void EmitFcvtz__Fix(AILEmitterCtx Context, bool Signed)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            Context.EmitLdvecsf(Op.Rn);

            EmitLdcImmF(Context, 1L << Op.FBits, Op.Size);

            Context.Emit(OpCodes.Mul);

            if (Signed)
            {
                EmitCvtToInt(Context, Op.Size);
            }
            else
            {
                EmitCvtToUInt(Context, Op.Size);
            }

            Context.EmitStintzr(Op.Rd);
        }

        public static void Fdiv_S(AILEmitterCtx Context) => EmitScalarOp(Context, OpCodes.Div);

        public static void Fmax_S(AILEmitterCtx Context) => EmitMathOp3(Context, nameof(Math.Max));
        public static void Fmin_S(AILEmitterCtx Context) => EmitMathOp3(Context, nameof(Math.Min));

        public static void Fmaxnm_S(AILEmitterCtx Context) => EmitMathOp3(Context, nameof(Math.Max));
        public static void Fminnm_S(AILEmitterCtx Context) => EmitMathOp3(Context, nameof(Math.Min));

        public static void Fmov_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvecsf(Op.Rn);
            Context.EmitStvecsf(Op.Rd);
        }

        public static void Fmov_Si(AILEmitterCtx Context)
        {
            AOpCodeSimdFmov Op = (AOpCodeSimdFmov)Context.CurrOp;

            Context.EmitLdc_I8(Op.Imm);
            Context.EmitLdc_I4(0);
            Context.EmitLdc_I4(Op.Size + 2);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Fmov_S));

            Context.EmitStvec(Op.Rd);
        }

        public static void Fmov_Ftoi(AILEmitterCtx Context)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            Context.EmitLdvecsi(Op.Rn);
            Context.EmitStintzr(Op.Rd);
        }

        public static void Fmov_Itof(AILEmitterCtx Context)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);
            Context.EmitStvecsi(Op.Rd);
        }

        public static void Fmov_Ftoi1(AILEmitterCtx Context)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4(1);
            Context.EmitLdc_I4(3);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.ExtractVec));

            Context.EmitStintzr(Op.Rd);
        }

        public static void Fmov_Itof1(AILEmitterCtx Context)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);
            Context.EmitLdc_I4(1);
            Context.EmitLdc_I4(3);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Fmov_S));

            Context.EmitStvec(Op.Rd);
        }

        public static void Fmul_S(AILEmitterCtx Context) => EmitScalarOp(Context, OpCodes.Mul);

        public static void Fneg_S(AILEmitterCtx Context) => EmitScalarOp(Context, OpCodes.Neg);

        public static void Fnmul_S(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvecsf(Op.Rn);
            Context.EmitLdvecsf(Op.Rm);

            Context.Emit(OpCodes.Mul);
            Context.Emit(OpCodes.Neg);

            Context.EmitStvecsf(Op.Rd);
        }

        public static void Frinta_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvecsf(Op.Rn);
            Context.EmitLdc_I4((int)MidpointRounding.AwayFromZero);

            MethodInfo MthdInfo;

            if (Op.Size == 0)
            {
                Type[] Types = new Type[] { typeof(float), typeof(MidpointRounding) };

                MthdInfo = typeof(MathF).GetMethod(nameof(MathF.Round), Types);
            }
            else if (Op.Size == 1)
            {
                Type[] Types = new Type[] { typeof(double), typeof(MidpointRounding) };

                MthdInfo = typeof(Math).GetMethod(nameof(Math.Round), Types);
            }
            else
            {
                throw new InvalidOperationException();
            }

            Context.EmitCall(MthdInfo);

            Context.EmitStvecsf(Op.Rd);
        }

        public static void Frintm_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvecsf(Op.Rn);

            MethodInfo MthdInfo;

            if (Op.Size == 0)
            {
                MthdInfo = typeof(MathF).GetMethod(nameof(MathF.Floor), new Type[] { typeof(float) });
            }
            else if (Op.Size == 1)
            {
                MthdInfo = typeof(Math).GetMethod(nameof(Math.Floor), new Type[] { typeof(double) });
            }
            else
            {
                throw new InvalidOperationException();
            }

            Context.EmitCall(MthdInfo);

            Context.EmitStvecsf(Op.Rd);
        }

        public static void Fsqrt_S(AILEmitterCtx Context) => EmitMathOp2(Context, nameof(Math.Sqrt));

        public static void Fsub_S(AILEmitterCtx Context) => EmitScalarOp(Context, OpCodes.Sub);

        public static void Scvtf_Gp(AILEmitterCtx Context)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            EmitFloatCast(Context, Op.Size);

            Context.EmitStvecsf(Op.Rd);
        }

        public static void Scvtf_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvecsi(Op.Rn);

            EmitFloatCast(Context, Op.Size);

            Context.EmitStvecsf(Op.Rd);
        }

        public static void Shl_S(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            Context.EmitLdvecsi(Op.Rn);
            Context.EmitLdc_I4(Op.Imm - (8 << Op.Size));

            Context.Emit(OpCodes.Shl);

            Context.EmitStvecsi(Op.Rd);
        }

        public static void Sshr_S(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            Context.EmitLdvecsi(Op.Rn);
            Context.EmitLdc_I4((8 << (Op.Size + 1)) - Op.Imm);

            Context.Emit(OpCodes.Shr);

            Context.EmitStvecsi(Op.Rd);
        }

        public static void Sub_S(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvecsi(Op.Rn);
            Context.EmitLdvecsi(Op.Rm);

            Context.Emit(OpCodes.Sub);

            Context.EmitStvecsi(Op.Rd);
        }

        public static void Ucvtf_Gp(AILEmitterCtx Context)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            Context.Emit(OpCodes.Conv_R_Un);

            EmitFloatCast(Context, Op.Size);

            Context.EmitStvecsf(Op.Rd);
        }

        private static void EmitScalarOp(AILEmitterCtx Context, OpCode ILOp)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvecsf(Op.Rn);

            //Negate and Not are the only unary operations supported on IL.
            //"Not" doesn't work with floats, so we don't need to compare it.
            if (ILOp != OpCodes.Neg)
            {
                Context.EmitLdvecsf(Op.Rm);
            }

            Context.Emit(ILOp);

            Context.EmitStvecsf(Op.Rd);
        }

        private static void EmitMathOp2(AILEmitterCtx Context, string Name)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvecsf(Op.Rn);

            EmitMathOpCall(Context, Name);

            Context.EmitStvecsf(Op.Rd);
        }

        private static void EmitMathOp3(AILEmitterCtx Context, string Name)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvecsf(Op.Rn);
            Context.EmitLdvecsf(Op.Rm);

            MethodInfo MthdInfo;

            if (Op.Size == 0)
            {
                MthdInfo = typeof(MathF).GetMethod(Name, new Type[] { typeof(float), typeof(float) });
            }
            else if (Op.Size == 1)
            {
                MthdInfo = typeof(Math).GetMethod(Name, new Type[] { typeof(double), typeof(double) });
            }
            else
            {
                throw new InvalidOperationException();
            }

            Context.EmitCall(MthdInfo);

            Context.EmitStvecsf(Op.Rd);
        }

        public static void EmitMathOpCvtToInt(AILEmitterCtx Context, string Name)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            Context.EmitLdvecsf(Op.Rn);

            EmitMathOpCall(Context, Name);

            EmitCvtToInt(Context, Op.Size);

            Context.EmitStintzr(Op.Rd);
        }

        private static void EmitMathOpCall(AILEmitterCtx Context, string Name)
        {
            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            MethodInfo MthdInfo;

            if (Op.Size == 0)
            {
                MthdInfo = typeof(MathF).GetMethod(Name);
            }
            else if (Op.Size == 1)
            {
                MthdInfo = typeof(Math).GetMethod(Name);
            }
            else
            {
                throw new InvalidOperationException();
            }

            Context.EmitCall(MthdInfo);
        }

        private static void EmitCvtToInt(AILEmitterCtx Context, int Size)
        {
            if (Size < 0 || Size > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            Context.EmitLdc_I4(0);

            if (Context.CurrOp.RegisterSize == ARegisterSize.Int32)
            {
                if (Size == 0)
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatSingleToInt32));
                }
                else /* if (Size == 1) */
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatDoubleToInt32));
                }
            }
            else
            {
                if (Size == 0)
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatSingleToInt64));
                }
                else /* if (Size == 1) */
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatDoubleToInt64));
                }
            }
        }

        private static void EmitCvtToUInt(AILEmitterCtx Context, int Size)
        {
            if (Size < 0 || Size > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            Context.EmitLdc_I4(0);

            if (Context.CurrOp.RegisterSize == ARegisterSize.Int32)
            {
                if (Size == 0)
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatSingleToUInt32));
                }
                else /* if (Size == 1) */
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatDoubleToUInt32));
                }
            }
            else
            {
                if (Size == 0)
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatSingleToUInt64));
                }
                else /* if (Size == 1) */
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatDoubleToUInt64));
                }
            }
        }

        private static void EmitFloatCast(AILEmitterCtx Context, int Size)
        {
            if (Size == 0)
            {
                Context.Emit(OpCodes.Conv_R4);
            }
            else if (Size == 1)
            {
                Context.Emit(OpCodes.Conv_R8);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }
        }

        private static void EmitLdcImmF(AILEmitterCtx Context, double ImmF, int Size)
        {
            if (Size == 0)
            {
                Context.EmitLdc_R4((float)ImmF);
            }
            else if (Size == 1)
            {
                Context.EmitLdc_R8(ImmF);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }
        }

        private static void EmitNaNCheck(AILEmitterCtx Context, int Index)
        {
            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            Context.EmitLdvecsf(Index);

            if (Op.Size == 0)
            {
                Context.EmitCall(typeof(float), nameof(float.IsNaN));
            }
            else if (Op.Size == 1)
            {
                Context.EmitCall(typeof(double), nameof(double.IsNaN));
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}