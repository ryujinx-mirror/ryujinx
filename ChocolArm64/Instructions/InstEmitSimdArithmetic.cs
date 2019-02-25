// https://github.com/intel/ARM_NEON_2_x86_SSE/blob/master/NEON_2_SSE.h

using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using static ChocolArm64.Instructions.InstEmitSimdHelper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Abs_S(ILEmitterCtx context)
        {
            EmitScalarUnaryOpSx(context, () => EmitAbs(context));
        }

        public static void Abs_V(ILEmitterCtx context)
        {
            EmitVectorUnaryOpSx(context, () => EmitAbs(context));
        }

        public static void Add_S(ILEmitterCtx context)
        {
            EmitScalarBinaryOpZx(context, () => context.Emit(OpCodes.Add));
        }

        public static void Add_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                EmitSse2Op(context, nameof(Sse2.Add));
            }
            else
            {
                EmitVectorBinaryOpZx(context, () => context.Emit(OpCodes.Add));
            }
        }

        public static void Addhn_V(ILEmitterCtx context)
        {
            EmitHighNarrow(context, () => context.Emit(OpCodes.Add), round: false);
        }

        public static void Addp_S(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            EmitVectorExtractZx(context, op.Rn, 0, op.Size);
            EmitVectorExtractZx(context, op.Rn, 1, op.Size);

            context.Emit(OpCodes.Add);

            EmitScalarSet(context, op.Rd, op.Size);
        }

        public static void Addp_V(ILEmitterCtx context)
        {
            EmitVectorPairwiseOpZx(context, () => context.Emit(OpCodes.Add));
        }

        public static void Addv_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int bytes = op.GetBitsCount() >> 3;
            int elems = bytes >> op.Size;

            EmitVectorExtractZx(context, op.Rn, 0, op.Size);

            for (int index = 1; index < elems; index++)
            {
                EmitVectorExtractZx(context, op.Rn, index, op.Size);

                context.Emit(OpCodes.Add);
            }

            EmitScalarSet(context, op.Rd, op.Size);
        }

        public static void Cls_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int bytes = op.GetBitsCount() >> 3;
            int elems = bytes >> op.Size;

            int eSize = 8 << op.Size;

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractZx(context, op.Rn, index, op.Size);

                context.EmitLdc_I4(eSize);

                SoftFallback.EmitCall(context, nameof(SoftFallback.CountLeadingSigns));

                EmitVectorInsert(context, op.Rd, index, op.Size);
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void Clz_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int bytes = op.GetBitsCount() >> 3;
            int elems = bytes >> op.Size;

            int eSize = 8 << op.Size;

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractZx(context, op.Rn, index, op.Size);

                if (Lzcnt.IsSupported && eSize == 32)
                {
                    context.Emit(OpCodes.Conv_U4);

                    context.EmitCall(typeof(Lzcnt).GetMethod(nameof(Lzcnt.LeadingZeroCount), new Type[] { typeof(uint) }));

                    context.Emit(OpCodes.Conv_U8);
                }
                else
                {
                    context.EmitLdc_I4(eSize);

                    SoftFallback.EmitCall(context, nameof(SoftFallback.CountLeadingZeros));
                }

                EmitVectorInsert(context, op.Rd, index, op.Size);
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void Cnt_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int elems = op.RegisterSize == RegisterSize.Simd128 ? 16 : 8;

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractZx(context, op.Rn, index, 0);

                if (Popcnt.IsSupported)
                {
                    context.EmitCall(typeof(Popcnt).GetMethod(nameof(Popcnt.PopCount), new Type[] { typeof(ulong) }));
                }
                else
                {
                    SoftFallback.EmitCall(context, nameof(SoftFallback.CountSetBits8));
                }

                EmitVectorInsert(context, op.Rd, index, 0);
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void Fabd_S(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Type[] typesSsv       = new Type[] { typeof(float) };
                    Type[] typesSubAndNot = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdc_R4(-0f);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SetScalarVector128), typesSsv));

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SubtractScalar), typesSubAndNot));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.AndNot),         typesSubAndNot));

                    context.EmitStvec(op.Rd);

                    EmitVectorZero32_128(context, op.Rd);
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesSsv       = new Type[] { typeof(double) };
                    Type[] typesSubAndNot = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdc_R8(-0d);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetScalarVector128), typesSsv));

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SubtractScalar), typesSubAndNot));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.AndNot),         typesSubAndNot));

                    context.EmitStvec(op.Rd);

                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitScalarBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPSub));

                    EmitUnaryMathCall(context, nameof(Math.Abs));
                });
            }
        }

        public static void Fabd_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Type[] typesSav       = new Type[] { typeof(float) };
                    Type[] typesSubAndNot = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdc_R4(-0f);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SetAllVector128), typesSav));

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Subtract), typesSubAndNot));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.AndNot),   typesSubAndNot));

                    context.EmitStvec(op.Rd);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        EmitVectorZeroUpper(context, op.Rd);
                    }
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesSav       = new Type[] { typeof(double) };
                    Type[] typesSubAndNot = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdc_R8(-0d);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesSubAndNot));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.AndNot),   typesSubAndNot));

                    context.EmitStvec(op.Rd);
                }
            }
            else
            {
                EmitVectorBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPSub));

                    EmitUnaryMathCall(context, nameof(Math.Abs));
                });
            }
        }

        public static void Fabs_S(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

                if (op.Size == 0)
                {
                    Type[] typesSsv    = new Type[] { typeof(float) };
                    Type[] typesAndNot = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdc_R4(-0f);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SetScalarVector128), typesSsv));

                    context.EmitLdvec(op.Rn);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.AndNot), typesAndNot));

                    context.EmitStvec(op.Rd);

                    EmitVectorZero32_128(context, op.Rd);
                }
                else /* if (op.Size == 1) */
                {
                    Type[] typesSsv    = new Type[] { typeof(double) };
                    Type[] typesAndNot = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdc_R8(-0d);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetScalarVector128), typesSsv));

                    context.EmitLdvec(op.Rn);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.AndNot), typesAndNot));

                    context.EmitStvec(op.Rd);

                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitScalarUnaryOpF(context, () =>
                {
                    EmitUnaryMathCall(context, nameof(Math.Abs));
                });
            }
        }

        public static void Fabs_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Type[] typesSav    = new Type[] { typeof(float) };
                    Type[] typesAndNot = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdc_R4(-0f);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SetAllVector128), typesSav));

                    context.EmitLdvec(op.Rn);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.AndNot), typesAndNot));

                    context.EmitStvec(op.Rd);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        EmitVectorZeroUpper(context, op.Rd);
                    }
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesSav    = new Type[] { typeof(double) };
                    Type[] typesAndNot = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdc_R8(-0d);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                    context.EmitLdvec(op.Rn);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.AndNot), typesAndNot));

                    context.EmitStvec(op.Rd);
                }
            }
            else
            {
                EmitVectorUnaryOpF(context, () =>
                {
                    EmitUnaryMathCall(context, nameof(Math.Abs));
                });
            }
        }

        public static void Fadd_S(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarSseOrSse2OpF(context, nameof(Sse.AddScalar));
            }
            else
            {
                EmitScalarBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPAdd));
                });
            }
        }

        public static void Fadd_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorSseOrSse2OpF(context, nameof(Sse.Add));
            }
            else
            {
                EmitVectorBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPAdd));
                });
            }
        }

        public static void Faddp_S(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.FastFP && Optimizations.UseSse3)
            {
                if (sizeF == 0)
                {
                    Type[] typesAddH = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdvec(op.Rn);
                    context.Emit(OpCodes.Dup);

                    context.EmitCall(typeof(Sse3).GetMethod(nameof(Sse3.HorizontalAdd), typesAddH));

                    context.EmitStvec(op.Rd);

                    EmitVectorZero32_128(context, op.Rd);
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesAddH = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdvec(op.Rn);
                    context.Emit(OpCodes.Dup);

                    context.EmitCall(typeof(Sse3).GetMethod(nameof(Sse3.HorizontalAdd), typesAddH));

                    context.EmitStvec(op.Rd);

                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorExtractF(context, op.Rn, 0, sizeF);
                EmitVectorExtractF(context, op.Rn, 1, sizeF);

                EmitSoftFloatCall(context, nameof(SoftFloat32.FPAdd));

                EmitScalarSetF(context, op.Rd, sizeF);
            }
        }

        public static void Faddp_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorPairwiseSseOrSse2OpF(context, nameof(Sse.Add));
            }
            else
            {
                EmitVectorPairwiseOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPAdd));
                });
            }
        }

        public static void Fdiv_S(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarSseOrSse2OpF(context, nameof(Sse.DivideScalar));
            }
            else
            {
                EmitScalarBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPDiv));
                });
            }
        }

        public static void Fdiv_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorSseOrSse2OpF(context, nameof(Sse.Divide));
            }
            else
            {
                EmitVectorBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPDiv));
                });
            }
        }

        public static void Fmadd_S(ILEmitterCtx context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                if (op.Size == 0)
                {
                    Type[] typesMulAdd = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdvec(op.Ra);
                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MultiplyScalar), typesMulAdd));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.AddScalar),      typesMulAdd));

                    context.EmitStvec(op.Rd);

                    EmitVectorZero32_128(context, op.Rd);
                }
                else /* if (op.Size == 1) */
                {
                    Type[] typesMulAdd = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdvec(op.Ra);
                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.MultiplyScalar), typesMulAdd));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.AddScalar),      typesMulAdd));

                    context.EmitStvec(op.Rd);

                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitScalarTernaryRaOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMulAdd));
                });
            }
        }

        public static void Fmax_S(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarSseOrSse2OpF(context, nameof(Sse.MaxScalar));
            }
            else
            {
                EmitScalarBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMax));
                });
            }
        }

        public static void Fmax_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorSseOrSse2OpF(context, nameof(Sse.Max));
            }
            else
            {
                EmitVectorBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMax));
                });
            }
        }

        public static void Fmaxnm_S(ILEmitterCtx context)
        {
            EmitScalarBinaryOpF(context, () =>
            {
                EmitSoftFloatCall(context, nameof(SoftFloat32.FPMaxNum));
            });
        }

        public static void Fmaxnm_V(ILEmitterCtx context)
        {
            EmitVectorBinaryOpF(context, () =>
            {
                EmitSoftFloatCall(context, nameof(SoftFloat32.FPMaxNum));
            });
        }

        public static void Fmaxp_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorPairwiseSseOrSse2OpF(context, nameof(Sse.Max));
            }
            else
            {
                EmitVectorPairwiseOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMax));
                });
            }
        }

        public static void Fmin_S(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarSseOrSse2OpF(context, nameof(Sse.MinScalar));
            }
            else
            {
                EmitScalarBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMin));
                });
            }
        }

        public static void Fmin_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorSseOrSse2OpF(context, nameof(Sse.Min));
            }
            else
            {
                EmitVectorBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMin));
                });
            }
        }

        public static void Fminnm_S(ILEmitterCtx context)
        {
            EmitScalarBinaryOpF(context, () =>
            {
                EmitSoftFloatCall(context, nameof(SoftFloat32.FPMinNum));
            });
        }

        public static void Fminnm_V(ILEmitterCtx context)
        {
            EmitVectorBinaryOpF(context, () =>
            {
                EmitSoftFloatCall(context, nameof(SoftFloat32.FPMinNum));
            });
        }

        public static void Fminp_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorPairwiseSseOrSse2OpF(context, nameof(Sse.Min));
            }
            else
            {
                EmitVectorPairwiseOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMin));
                });
            }
        }

        public static void Fmla_Se(ILEmitterCtx context)
        {
            EmitScalarTernaryOpByElemF(context, () =>
            {
                context.Emit(OpCodes.Mul);
                context.Emit(OpCodes.Add);
            });
        }

        public static void Fmla_V(ILEmitterCtx context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Type[] typesMulAdd = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdvec(op.Rd);
                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Multiply), typesMulAdd));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Add),      typesMulAdd));

                    context.EmitStvec(op.Rd);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        EmitVectorZeroUpper(context, op.Rd);
                    }
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesMulAdd = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdvec(op.Rd);
                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Multiply), typesMulAdd));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add),      typesMulAdd));

                    context.EmitStvec(op.Rd);
                }
            }
            else
            {
                EmitVectorTernaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMulAdd));
                });
            }
        }

        public static void Fmla_Ve(ILEmitterCtx context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdRegElemF64 op = (OpCodeSimdRegElemF64)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Type[] typesSfl    = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>), typeof(byte) };
                    Type[] typesMulAdd = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdvec(op.Rd);
                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);
                    context.Emit(OpCodes.Dup);

                    context.EmitLdc_I4(op.Index | op.Index << 2 | op.Index << 4 | op.Index << 6);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Shuffle),  typesSfl));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Multiply), typesMulAdd));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Add),      typesMulAdd));

                    context.EmitStvec(op.Rd);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        EmitVectorZeroUpper(context, op.Rd);
                    }
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesSfl    = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>), typeof(byte) };
                    Type[] typesMulAdd = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdvec(op.Rd);
                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);
                    context.Emit(OpCodes.Dup);

                    context.EmitLdc_I4(op.Index | op.Index << 1);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Shuffle),  typesSfl));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Multiply), typesMulAdd));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add),      typesMulAdd));

                    context.EmitStvec(op.Rd);
                }
            }
            else
            {
                EmitVectorTernaryOpByElemF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMulAdd));
                });
            }
        }

        public static void Fmls_Se(ILEmitterCtx context)
        {
            EmitScalarTernaryOpByElemF(context, () =>
            {
                context.Emit(OpCodes.Mul);
                context.Emit(OpCodes.Sub);
            });
        }

        public static void Fmls_V(ILEmitterCtx context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Type[] typesMulSub = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdvec(op.Rd);
                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Multiply), typesMulSub));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Subtract), typesMulSub));

                    context.EmitStvec(op.Rd);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        EmitVectorZeroUpper(context, op.Rd);
                    }
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesMulSub = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdvec(op.Rd);
                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Multiply), typesMulSub));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesMulSub));

                    context.EmitStvec(op.Rd);
                }
            }
            else
            {
                EmitVectorTernaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMulSub));
                });
            }
        }

        public static void Fmls_Ve(ILEmitterCtx context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdRegElemF64 op = (OpCodeSimdRegElemF64)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Type[] typesSfl    = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>), typeof(byte) };
                    Type[] typesMulSub = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdvec(op.Rd);
                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);
                    context.Emit(OpCodes.Dup);

                    context.EmitLdc_I4(op.Index | op.Index << 2 | op.Index << 4 | op.Index << 6);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Shuffle),  typesSfl));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Multiply), typesMulSub));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Subtract), typesMulSub));

                    context.EmitStvec(op.Rd);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        EmitVectorZeroUpper(context, op.Rd);
                    }
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesSfl    = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>), typeof(byte) };
                    Type[] typesMulSub = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdvec(op.Rd);
                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);
                    context.Emit(OpCodes.Dup);

                    context.EmitLdc_I4(op.Index | op.Index << 1);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Shuffle),  typesSfl));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Multiply), typesMulSub));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesMulSub));

                    context.EmitStvec(op.Rd);
                }
            }
            else
            {
                EmitVectorTernaryOpByElemF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMulSub));
                });
            }
        }

        public static void Fmsub_S(ILEmitterCtx context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                if (op.Size == 0)
                {
                    Type[] typesMulSub = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdvec(op.Ra);
                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MultiplyScalar), typesMulSub));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SubtractScalar), typesMulSub));

                    context.EmitStvec(op.Rd);

                    EmitVectorZero32_128(context, op.Rd);
                }
                else /* if (op.Size == 1) */
                {
                    Type[] typesMulSub = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdvec(op.Ra);
                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.MultiplyScalar), typesMulSub));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SubtractScalar), typesMulSub));

                    context.EmitStvec(op.Rd);

                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitScalarTernaryRaOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMulSub));
                });
            }
        }

        public static void Fmul_S(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarSseOrSse2OpF(context, nameof(Sse.MultiplyScalar));
            }
            else
            {
                EmitScalarBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMul));
                });
            }
        }

        public static void Fmul_Se(ILEmitterCtx context)
        {
            EmitScalarBinaryOpByElemF(context, () => context.Emit(OpCodes.Mul));
        }

        public static void Fmul_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorSseOrSse2OpF(context, nameof(Sse.Multiply));
            }
            else
            {
                EmitVectorBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMul));
                });
            }
        }

        public static void Fmul_Ve(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdRegElemF64 op = (OpCodeSimdRegElemF64)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Type[] typesSfl = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>), typeof(byte) };
                    Type[] typesMul = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);
                    context.Emit(OpCodes.Dup);

                    context.EmitLdc_I4(op.Index | op.Index << 2 | op.Index << 4 | op.Index << 6);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Shuffle),  typesSfl));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Multiply), typesMul));

                    context.EmitStvec(op.Rd);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        EmitVectorZeroUpper(context, op.Rd);
                    }
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesSfl = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>), typeof(byte) };
                    Type[] typesMul = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);
                    context.Emit(OpCodes.Dup);

                    context.EmitLdc_I4(op.Index | op.Index << 1);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Shuffle),  typesSfl));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Multiply), typesMul));

                    context.EmitStvec(op.Rd);
                }
            }
            else
            {
                EmitVectorBinaryOpByElemF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPMul));
                });
            }
        }

        public static void Fmulx_S(ILEmitterCtx context)
        {
            EmitScalarBinaryOpF(context, () =>
            {
                EmitSoftFloatCall(context, nameof(SoftFloat32.FPMulX));
            });
        }

        public static void Fmulx_Se(ILEmitterCtx context)
        {
            EmitScalarBinaryOpByElemF(context, () =>
            {
                EmitSoftFloatCall(context, nameof(SoftFloat32.FPMulX));
            });
        }

        public static void Fmulx_V(ILEmitterCtx context)
        {
            EmitVectorBinaryOpF(context, () =>
            {
                EmitSoftFloatCall(context, nameof(SoftFloat32.FPMulX));
            });
        }

        public static void Fmulx_Ve(ILEmitterCtx context)
        {
            EmitVectorBinaryOpByElemF(context, () =>
            {
                EmitSoftFloatCall(context, nameof(SoftFloat32.FPMulX));
            });
        }

        public static void Fneg_S(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

                if (op.Size == 0)
                {
                    Type[] typesSsv = new Type[] { typeof(float) };
                    Type[] typesXor = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdc_R4(-0f);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SetScalarVector128), typesSsv));

                    context.EmitLdvec(op.Rn);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Xor), typesXor));

                    context.EmitStvec(op.Rd);

                    EmitVectorZero32_128(context, op.Rd);
                }
                else /* if (op.Size == 1) */
                {
                    Type[] typesSsv = new Type[] { typeof(double) };
                    Type[] typesXor = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdc_R8(-0d);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetScalarVector128), typesSsv));

                    context.EmitLdvec(op.Rn);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), typesXor));

                    context.EmitStvec(op.Rd);

                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitScalarUnaryOpF(context, () => context.Emit(OpCodes.Neg));
            }
        }

        public static void Fneg_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Type[] typesSav = new Type[] { typeof(float) };
                    Type[] typesXor = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdc_R4(-0f);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SetAllVector128), typesSav));

                    context.EmitLdvec(op.Rn);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Xor), typesXor));

                    context.EmitStvec(op.Rd);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        EmitVectorZeroUpper(context, op.Rd);
                    }
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesSav = new Type[] { typeof(double) };
                    Type[] typesXor = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdc_R8(-0d);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                    context.EmitLdvec(op.Rn);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), typesXor));

                    context.EmitStvec(op.Rd);
                }
            }
            else
            {
                EmitVectorUnaryOpF(context, () => context.Emit(OpCodes.Neg));
            }
        }

        public static void Fnmadd_S(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            int sizeF = op.Size & 1;

            EmitVectorExtractF(context, op.Rn, 0, sizeF);

            context.Emit(OpCodes.Neg);

            EmitVectorExtractF(context, op.Rm, 0, sizeF);

            context.Emit(OpCodes.Mul);

            EmitVectorExtractF(context, op.Ra, 0, sizeF);

            context.Emit(OpCodes.Sub);

            EmitScalarSetF(context, op.Rd, sizeF);
        }

        public static void Fnmsub_S(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            int sizeF = op.Size & 1;

            EmitVectorExtractF(context, op.Rn, 0, sizeF);
            EmitVectorExtractF(context, op.Rm, 0, sizeF);

            context.Emit(OpCodes.Mul);

            EmitVectorExtractF(context, op.Ra, 0, sizeF);

            context.Emit(OpCodes.Sub);

            EmitScalarSetF(context, op.Rd, sizeF);
        }

        public static void Fnmul_S(ILEmitterCtx context)
        {
            EmitScalarBinaryOpF(context, () =>
            {
                context.Emit(OpCodes.Mul);
                context.Emit(OpCodes.Neg);
            });
        }

        public static void Frecpe_S(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.FastFP && Optimizations.UseSse && sizeF == 0)
            {
                EmitScalarSseOrSse2OpF(context, nameof(Sse.ReciprocalScalar));
            }
            else
            {
                EmitScalarUnaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPRecipEstimate));
                });
            }
        }

        public static void Frecpe_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.FastFP && Optimizations.UseSse && sizeF == 0)
            {
                EmitVectorSseOrSse2OpF(context, nameof(Sse.Reciprocal));
            }
            else
            {
                EmitVectorUnaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPRecipEstimate));
                });
            }
        }

        public static void Frecps_S(ILEmitterCtx context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Type[] typesSsv    = new Type[] { typeof(float) };
                    Type[] typesMulSub = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdc_R4(2f);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SetScalarVector128), typesSsv));

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MultiplyScalar), typesMulSub));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SubtractScalar), typesMulSub));

                    context.EmitStvec(op.Rd);

                    EmitVectorZero32_128(context, op.Rd);
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesSsv    = new Type[] { typeof(double) };
                    Type[] typesMulSub = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdc_R8(2d);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetScalarVector128), typesSsv));

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.MultiplyScalar), typesMulSub));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SubtractScalar), typesMulSub));

                    context.EmitStvec(op.Rd);

                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitScalarBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPRecipStepFused));
                });
            }
        }

        public static void Frecps_V(ILEmitterCtx context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Type[] typesSav    = new Type[] { typeof(float) };
                    Type[] typesMulSub = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdc_R4(2f);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SetAllVector128), typesSav));

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Multiply), typesMulSub));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Subtract), typesMulSub));

                    context.EmitStvec(op.Rd);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        EmitVectorZeroUpper(context, op.Rd);
                    }
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesSav    = new Type[] { typeof(double) };
                    Type[] typesMulSub = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdc_R8(2d);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Multiply), typesMulSub));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesMulSub));

                    context.EmitStvec(op.Rd);
                }
            }
            else
            {
                EmitVectorBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPRecipStepFused));
                });
            }
        }

        public static void Frecpx_S(ILEmitterCtx context)
        {
            EmitScalarUnaryOpF(context, () =>
            {
                EmitSoftFloatCall(context, nameof(SoftFloat32.FPRecpX));
            });
        }

        public static void Frinta_S(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            EmitVectorExtractF(context, op.Rn, 0, op.Size);

            EmitRoundMathCall(context, MidpointRounding.AwayFromZero);

            EmitScalarSetF(context, op.Rd, op.Size);
        }

        public static void Frinta_V(ILEmitterCtx context)
        {
            EmitVectorUnaryOpF(context, () =>
            {
                EmitRoundMathCall(context, MidpointRounding.AwayFromZero);
            });
        }

        public static void Frinti_S(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            EmitScalarUnaryOpF(context, () =>
            {
                context.EmitLdarg(TranslatedSub.StateArgIdx);

                if (op.Size == 0)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.RoundF));
                }
                else if (op.Size == 1)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.Round));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
        }

        public static void Frinti_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            EmitVectorUnaryOpF(context, () =>
            {
                context.EmitLdarg(TranslatedSub.StateArgIdx);

                if (sizeF == 0)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.RoundF));
                }
                else if (sizeF == 1)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.Round));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
        }

        public static void Frintm_S(ILEmitterCtx context)
        {
            EmitScalarUnaryOpF(context, () =>
            {
                EmitUnaryMathCall(context, nameof(Math.Floor));
            });
        }

        public static void Frintm_V(ILEmitterCtx context)
        {
            EmitVectorUnaryOpF(context, () =>
            {
                EmitUnaryMathCall(context, nameof(Math.Floor));
            });
        }

        public static void Frintn_S(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            EmitVectorExtractF(context, op.Rn, 0, op.Size);

            EmitRoundMathCall(context, MidpointRounding.ToEven);

            EmitScalarSetF(context, op.Rd, op.Size);
        }

        public static void Frintn_V(ILEmitterCtx context)
        {
            EmitVectorUnaryOpF(context, () =>
            {
                EmitRoundMathCall(context, MidpointRounding.ToEven);
            });
        }

        public static void Frintp_S(ILEmitterCtx context)
        {
            EmitScalarUnaryOpF(context, () =>
            {
                EmitUnaryMathCall(context, nameof(Math.Ceiling));
            });
        }

        public static void Frintp_V(ILEmitterCtx context)
        {
            EmitVectorUnaryOpF(context, () =>
            {
                EmitUnaryMathCall(context, nameof(Math.Ceiling));
            });
        }

        public static void Frintx_S(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            EmitScalarUnaryOpF(context, () =>
            {
                context.EmitLdarg(TranslatedSub.StateArgIdx);

                if (op.Size == 0)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.RoundF));
                }
                else if (op.Size == 1)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.Round));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
        }

        public static void Frintx_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            EmitVectorUnaryOpF(context, () =>
            {
                context.EmitLdarg(TranslatedSub.StateArgIdx);

                if (op.Size == 0)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.RoundF));
                }
                else if (op.Size == 1)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.Round));
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
        }

        public static void Frintz_S(ILEmitterCtx context)
        {
            EmitScalarUnaryOpF(context, () =>
            {
                EmitUnaryMathCall(context, nameof(Math.Truncate));
            });
        }

        public static void Frintz_V(ILEmitterCtx context)
        {
            EmitVectorUnaryOpF(context, () =>
            {
                EmitUnaryMathCall(context, nameof(Math.Truncate));
            });
        }

        public static void Frsqrte_S(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.FastFP && Optimizations.UseSse && sizeF == 0)
            {
                EmitScalarSseOrSse2OpF(context, nameof(Sse.ReciprocalSqrtScalar));
            }
            else
            {
                EmitScalarUnaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPRSqrtEstimate));
                });
            }
        }

        public static void Frsqrte_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.FastFP && Optimizations.UseSse && sizeF == 0)
            {
                EmitVectorSseOrSse2OpF(context, nameof(Sse.ReciprocalSqrt));
            }
            else
            {
                EmitVectorUnaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPRSqrtEstimate));
                });
            }
        }

        public static void Frsqrts_S(ILEmitterCtx context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Type[] typesSsv    = new Type[] { typeof(float) };
                    Type[] typesMulSub = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdc_R4(0.5f);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SetScalarVector128), typesSsv));

                    context.EmitLdc_R4(3f);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SetScalarVector128), typesSsv));

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MultiplyScalar), typesMulSub));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SubtractScalar), typesMulSub));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MultiplyScalar), typesMulSub));

                    context.EmitStvec(op.Rd);

                    EmitVectorZero32_128(context, op.Rd);
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesSsv    = new Type[] { typeof(double) };
                    Type[] typesMulSub = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdc_R8(0.5d);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetScalarVector128), typesSsv));

                    context.EmitLdc_R8(3d);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetScalarVector128), typesSsv));

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.MultiplyScalar), typesMulSub));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SubtractScalar), typesMulSub));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.MultiplyScalar), typesMulSub));

                    context.EmitStvec(op.Rd);

                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitScalarBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPRSqrtStepFused));
                });
            }
        }

        public static void Frsqrts_V(ILEmitterCtx context) // Fused.
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Type[] typesSav    = new Type[] { typeof(float) };
                    Type[] typesMulSub = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    context.EmitLdc_R4(0.5f);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SetAllVector128), typesSav));

                    context.EmitLdc_R4(3f);
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.SetAllVector128), typesSav));

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Multiply), typesMulSub));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Subtract), typesMulSub));
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Multiply), typesMulSub));

                    context.EmitStvec(op.Rd);

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        EmitVectorZeroUpper(context, op.Rd);
                    }
                }
                else /* if (sizeF == 1) */
                {
                    Type[] typesSav    = new Type[] { typeof(double) };
                    Type[] typesMulSub = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    context.EmitLdc_R8(0.5d);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                    context.EmitLdc_R8(3d);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                    context.EmitLdvec(op.Rn);
                    context.EmitLdvec(op.Rm);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Multiply), typesMulSub));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesMulSub));
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Multiply), typesMulSub));

                    context.EmitStvec(op.Rd);
                }
            }
            else
            {
                EmitVectorBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPRSqrtStepFused));
                });
            }
        }

        public static void Fsqrt_S(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarSseOrSse2OpF(context, nameof(Sse.SqrtScalar));
            }
            else
            {
                EmitScalarUnaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPSqrt));
                });
            }
        }

        public static void Fsqrt_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorSseOrSse2OpF(context, nameof(Sse.Sqrt));
            }
            else
            {
                EmitVectorUnaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPSqrt));
                });
            }
        }

        public static void Fsub_S(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitScalarSseOrSse2OpF(context, nameof(Sse.SubtractScalar));
            }
            else
            {
                EmitScalarBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPSub));
                });
            }
        }

        public static void Fsub_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitVectorSseOrSse2OpF(context, nameof(Sse.Subtract));
            }
            else
            {
                EmitVectorBinaryOpF(context, () =>
                {
                    EmitSoftFloatCall(context, nameof(SoftFloat32.FPSub));
                });
            }
        }

        public static void Mla_V(ILEmitterCtx context)
        {
            EmitVectorTernaryOpZx(context, () =>
            {
                context.Emit(OpCodes.Mul);
                context.Emit(OpCodes.Add);
            });
        }

        public static void Mla_Ve(ILEmitterCtx context)
        {
            EmitVectorTernaryOpByElemZx(context, () =>
            {
                context.Emit(OpCodes.Mul);
                context.Emit(OpCodes.Add);
            });
        }

        public static void Mls_V(ILEmitterCtx context)
        {
            EmitVectorTernaryOpZx(context, () =>
            {
                context.Emit(OpCodes.Mul);
                context.Emit(OpCodes.Sub);
            });
        }

        public static void Mls_Ve(ILEmitterCtx context)
        {
            EmitVectorTernaryOpByElemZx(context, () =>
            {
                context.Emit(OpCodes.Mul);
                context.Emit(OpCodes.Sub);
            });
        }

        public static void Mul_V(ILEmitterCtx context)
        {
            EmitVectorBinaryOpZx(context, () => context.Emit(OpCodes.Mul));
        }

        public static void Mul_Ve(ILEmitterCtx context)
        {
            EmitVectorBinaryOpByElemZx(context, () => context.Emit(OpCodes.Mul));
        }

        public static void Neg_S(ILEmitterCtx context)
        {
            EmitScalarUnaryOpSx(context, () => context.Emit(OpCodes.Neg));
        }

        public static void Neg_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

                Type[] typesSub = new Type[] { VectorIntTypesPerSizeLog2[op.Size], VectorIntTypesPerSizeLog2[op.Size] };

                string[] namesSzv = new string[] { nameof(VectorHelper.VectorSByteZero),
                                                   nameof(VectorHelper.VectorInt16Zero),
                                                   nameof(VectorHelper.VectorInt32Zero),
                                                   nameof(VectorHelper.VectorInt64Zero) };

                VectorHelper.EmitCall(context, namesSzv[op.Size]);

                context.EmitLdvec(op.Rn);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesSub));

                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorUnaryOpSx(context, () => context.Emit(OpCodes.Neg));
            }
        }

        public static void Raddhn_V(ILEmitterCtx context)
        {
            EmitHighNarrow(context, () => context.Emit(OpCodes.Add), round: true);
        }

        public static void Rsubhn_V(ILEmitterCtx context)
        {
            EmitHighNarrow(context, () => context.Emit(OpCodes.Sub), round: true);
        }

        public static void Saba_V(ILEmitterCtx context)
        {
            EmitVectorTernaryOpSx(context, () =>
            {
                context.Emit(OpCodes.Sub);
                EmitAbs(context);

                context.Emit(OpCodes.Add);
            });
        }

        public static void Sabal_V(ILEmitterCtx context)
        {
            EmitVectorWidenRnRmTernaryOpSx(context, () =>
            {
                context.Emit(OpCodes.Sub);
                EmitAbs(context);

                context.Emit(OpCodes.Add);
            });
        }

        public static void Sabd_V(ILEmitterCtx context)
        {
            EmitVectorBinaryOpSx(context, () =>
            {
                context.Emit(OpCodes.Sub);
                EmitAbs(context);
            });
        }

        public static void Sabdl_V(ILEmitterCtx context)
        {
            EmitVectorWidenRnRmBinaryOpSx(context, () =>
            {
                context.Emit(OpCodes.Sub);
                EmitAbs(context);
            });
        }

        public static void Sadalp_V(ILEmitterCtx context)
        {
            EmitAddLongPairwise(context, signed: true, accumulate: true);
        }

        public static void Saddl_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesSrl = new Type[] { VectorIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesCvt = new Type[] { VectorIntTypesPerSizeLog2[op.Size] };
                Type[] typesAdd = new Type[] { VectorIntTypesPerSizeLog2[op.Size + 1],
                                               VectorIntTypesPerSizeLog2[op.Size + 1] };

                string[] namesCvt = new string[] { nameof(Sse41.ConvertToVector128Int16),
                                                   nameof(Sse41.ConvertToVector128Int32),
                                                   nameof(Sse41.ConvertToVector128Int64) };

                int numBytes = op.RegisterSize == RegisterSize.Simd128 ? 8 : 0;

                context.EmitLdvec(op.Rn);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(namesCvt[op.Size], typesCvt));

                context.EmitLdvec(op.Rm);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(namesCvt[op.Size], typesCvt));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));

                context.EmitStvec(op.Rd);
            }
            else
            {
                EmitVectorWidenRnRmBinaryOpSx(context, () => context.Emit(OpCodes.Add));
            }
        }

        public static void Saddlp_V(ILEmitterCtx context)
        {
            EmitAddLongPairwise(context, signed: true, accumulate: false);
        }

        public static void Saddw_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesSrl = new Type[] { VectorIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesCvt = new Type[] { VectorIntTypesPerSizeLog2[op.Size] };
                Type[] typesAdd = new Type[] { VectorIntTypesPerSizeLog2[op.Size + 1],
                                               VectorIntTypesPerSizeLog2[op.Size + 1] };

                string[] namesCvt = new string[] { nameof(Sse41.ConvertToVector128Int16),
                                                   nameof(Sse41.ConvertToVector128Int32),
                                                   nameof(Sse41.ConvertToVector128Int64) };

                int numBytes = op.RegisterSize == RegisterSize.Simd128 ? 8 : 0;

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rm);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(namesCvt[op.Size], typesCvt));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));

                context.EmitStvec(op.Rd);
            }
            else
            {
                EmitVectorWidenRmBinaryOpSx(context, () => context.Emit(OpCodes.Add));
            }
        }

        public static void Shadd_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                Type[] typesSra       = new Type[] { VectorIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesAndXorAdd = new Type[] { VectorIntTypesPerSizeLog2[op.Size], VectorIntTypesPerSizeLog2[op.Size] };

                context.EmitLdvec(op.Rn);

                context.Emit(OpCodes.Dup);
                context.EmitStvectmp();

                context.EmitLdvec(op.Rm);

                context.Emit(OpCodes.Dup);
                context.EmitStvectmp2();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.And), typesAndXorAdd));

                context.EmitLdvectmp();
                context.EmitLdvectmp2();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), typesAndXorAdd));

                context.EmitLdc_I4(1);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightArithmetic), typesSra));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add),                  typesAndXorAdd));

                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorBinaryOpSx(context, () =>
                {
                    context.Emit(OpCodes.Add);

                    context.Emit(OpCodes.Ldc_I4_1);
                    context.Emit(OpCodes.Shr);
                });
            }
        }

        public static void Shsub_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size < 2)
            {
                Type[] typesSav    = new Type[] { IntTypesPerSizeLog2[op.Size] };
                Type[] typesAddSub = new Type[] { VectorIntTypesPerSizeLog2 [op.Size], VectorIntTypesPerSizeLog2 [op.Size] };
                Type[] typesAvg    = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], VectorUIntTypesPerSizeLog2[op.Size] };

                context.EmitLdc_I4(op.Size == 0 ? sbyte.MinValue : short.MinValue);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                context.EmitStvectmp();

                context.EmitLdvec(op.Rn);
                context.EmitLdvectmp();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAddSub));

                context.Emit(OpCodes.Dup);

                context.EmitLdvec(op.Rm);
                context.EmitLdvectmp();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add),      typesAddSub));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Average),  typesAvg));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesAddSub));

                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorBinaryOpSx(context, () =>
                {
                    context.Emit(OpCodes.Sub);

                    context.Emit(OpCodes.Ldc_I4_1);
                    context.Emit(OpCodes.Shr);
                });
            }
        }

        public static void Smax_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesMax = new Type[] { VectorIntTypesPerSizeLog2[op.Size], VectorIntTypesPerSizeLog2[op.Size] };

                Type typeSse = op.Size == 1 ? typeof(Sse2) : typeof(Sse41);

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rm);

                context.EmitCall(typeSse.GetMethod(nameof(Sse2.Max), typesMax));

                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                Type[] types = new Type[] { typeof(long), typeof(long) };

                MethodInfo mthdInfo = typeof(Math).GetMethod(nameof(Math.Max), types);

                EmitVectorBinaryOpSx(context, () => context.EmitCall(mthdInfo));
            }
        }

        public static void Smaxp_V(ILEmitterCtx context)
        {
            Type[] types = new Type[] { typeof(long), typeof(long) };

            MethodInfo mthdInfo = typeof(Math).GetMethod(nameof(Math.Max), types);

            EmitVectorPairwiseOpSx(context, () => context.EmitCall(mthdInfo));
        }

        public static void Smin_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesMin = new Type[] { VectorIntTypesPerSizeLog2[op.Size], VectorIntTypesPerSizeLog2[op.Size] };

                Type typeSse = op.Size == 1 ? typeof(Sse2) : typeof(Sse41);

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rm);

                context.EmitCall(typeSse.GetMethod(nameof(Sse2.Min), typesMin));

                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                Type[] types = new Type[] { typeof(long), typeof(long) };

                MethodInfo mthdInfo = typeof(Math).GetMethod(nameof(Math.Min), types);

                EmitVectorBinaryOpSx(context, () => context.EmitCall(mthdInfo));
            }
        }

        public static void Sminp_V(ILEmitterCtx context)
        {
            Type[] types = new Type[] { typeof(long), typeof(long) };

            MethodInfo mthdInfo = typeof(Math).GetMethod(nameof(Math.Min), types);

            EmitVectorPairwiseOpSx(context, () => context.EmitCall(mthdInfo));
        }

        public static void Smlal_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse41 && op.Size < 2)
            {
                Type[] typesSrl    = new Type[] { VectorIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesCvt    = new Type[] { VectorIntTypesPerSizeLog2[op.Size] };
                Type[] typesMulAdd = new Type[] { VectorIntTypesPerSizeLog2[op.Size + 1],
                                                  VectorIntTypesPerSizeLog2[op.Size + 1] };

                Type typeSse = op.Size == 0 ? typeof(Sse2) : typeof(Sse41);

                string nameCvt = op.Size == 0
                    ? nameof(Sse41.ConvertToVector128Int16)
                    : nameof(Sse41.ConvertToVector128Int32);

                int numBytes = op.RegisterSize == RegisterSize.Simd128 ? 8 : 0;

                context.EmitLdvec(op.Rd);
                context.EmitLdvec(op.Rn);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(nameCvt, typesCvt));

                context.EmitLdvec(op.Rm);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(nameCvt, typesCvt));

                context.EmitCall(typeSse.GetMethod(nameof(Sse2.MultiplyLow), typesMulAdd));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesMulAdd));

                context.EmitStvec(op.Rd);
            }
            else
            {
                EmitVectorWidenRnRmTernaryOpSx(context, () =>
                {
                    context.Emit(OpCodes.Mul);
                    context.Emit(OpCodes.Add);
                });
            }
        }

        public static void Smlal_Ve(ILEmitterCtx context)
        {
            EmitVectorWidenTernaryOpByElemSx(context, () =>
            {
                context.Emit(OpCodes.Mul);
                context.Emit(OpCodes.Add);
            });
        }

        public static void Smlsl_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse41 && op.Size < 2)
            {
                Type[] typesSrl    = new Type[] { VectorIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesCvt    = new Type[] { VectorIntTypesPerSizeLog2[op.Size] };
                Type[] typesMulSub = new Type[] { VectorIntTypesPerSizeLog2[op.Size + 1],
                                                  VectorIntTypesPerSizeLog2[op.Size + 1] };

                Type typeSse = op.Size == 0 ? typeof(Sse2) : typeof(Sse41);

                string nameCvt = op.Size == 0
                    ? nameof(Sse41.ConvertToVector128Int16)
                    : nameof(Sse41.ConvertToVector128Int32);

                int numBytes = op.RegisterSize == RegisterSize.Simd128 ? 8 : 0;

                context.EmitLdvec(op.Rd);
                context.EmitLdvec(op.Rn);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(nameCvt, typesCvt));

                context.EmitLdvec(op.Rm);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(nameCvt, typesCvt));

                context.EmitCall(typeSse.GetMethod(nameof(Sse2.MultiplyLow), typesMulSub));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesMulSub));

                context.EmitStvec(op.Rd);
            }
            else
            {
                EmitVectorWidenRnRmTernaryOpSx(context, () =>
                {
                    context.Emit(OpCodes.Mul);
                    context.Emit(OpCodes.Sub);
                });
            }
        }

        public static void Smlsl_Ve(ILEmitterCtx context)
        {
            EmitVectorWidenTernaryOpByElemSx(context, () =>
            {
                context.Emit(OpCodes.Mul);
                context.Emit(OpCodes.Sub);
            });
        }

        public static void Smull_V(ILEmitterCtx context)
        {
            EmitVectorWidenRnRmBinaryOpSx(context, () => context.Emit(OpCodes.Mul));
        }

        public static void Smull_Ve(ILEmitterCtx context)
        {
            EmitVectorWidenBinaryOpByElemSx(context, () => context.Emit(OpCodes.Mul));
        }

        public static void Sqabs_S(ILEmitterCtx context)
        {
            EmitScalarSaturatingUnaryOpSx(context, () => EmitAbs(context));
        }

        public static void Sqabs_V(ILEmitterCtx context)
        {
            EmitVectorSaturatingUnaryOpSx(context, () => EmitAbs(context));
        }

        public static void Sqadd_S(ILEmitterCtx context)
        {
            EmitScalarSaturatingBinaryOpSx(context, SaturatingFlags.Add);
        }

        public static void Sqadd_V(ILEmitterCtx context)
        {
            EmitVectorSaturatingBinaryOpSx(context, SaturatingFlags.Add);
        }

        public static void Sqdmulh_S(ILEmitterCtx context)
        {
            EmitSaturatingBinaryOp(context, () => EmitDoublingMultiplyHighHalf(context, round: false), SaturatingFlags.ScalarSx);
        }

        public static void Sqdmulh_V(ILEmitterCtx context)
        {
            EmitSaturatingBinaryOp(context, () => EmitDoublingMultiplyHighHalf(context, round: false), SaturatingFlags.VectorSx);
        }

        public static void Sqneg_S(ILEmitterCtx context)
        {
            EmitScalarSaturatingUnaryOpSx(context, () => context.Emit(OpCodes.Neg));
        }

        public static void Sqneg_V(ILEmitterCtx context)
        {
            EmitVectorSaturatingUnaryOpSx(context, () => context.Emit(OpCodes.Neg));
        }

        public static void Sqrdmulh_S(ILEmitterCtx context)
        {
            EmitSaturatingBinaryOp(context, () => EmitDoublingMultiplyHighHalf(context, round: true), SaturatingFlags.ScalarSx);
        }

        public static void Sqrdmulh_V(ILEmitterCtx context)
        {
            EmitSaturatingBinaryOp(context, () => EmitDoublingMultiplyHighHalf(context, round: true), SaturatingFlags.VectorSx);
        }

        public static void Sqsub_S(ILEmitterCtx context)
        {
            EmitScalarSaturatingBinaryOpSx(context, SaturatingFlags.Sub);
        }

        public static void Sqsub_V(ILEmitterCtx context)
        {
            EmitVectorSaturatingBinaryOpSx(context, SaturatingFlags.Sub);
        }

        public static void Sqxtn_S(ILEmitterCtx context)
        {
            EmitSaturatingNarrowOp(context, SaturatingNarrowFlags.ScalarSxSx);
        }

        public static void Sqxtn_V(ILEmitterCtx context)
        {
            EmitSaturatingNarrowOp(context, SaturatingNarrowFlags.VectorSxSx);
        }

        public static void Sqxtun_S(ILEmitterCtx context)
        {
            EmitSaturatingNarrowOp(context, SaturatingNarrowFlags.ScalarSxZx);
        }

        public static void Sqxtun_V(ILEmitterCtx context)
        {
            EmitSaturatingNarrowOp(context, SaturatingNarrowFlags.VectorSxZx);
        }

        public static void Srhadd_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size < 2)
            {
                Type[] typesSav    = new Type[] { IntTypesPerSizeLog2[op.Size] };
                Type[] typesSubAdd = new Type[] { VectorIntTypesPerSizeLog2 [op.Size], VectorIntTypesPerSizeLog2 [op.Size] };
                Type[] typesAvg    = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], VectorUIntTypesPerSizeLog2[op.Size] };

                context.EmitLdc_I4(op.Size == 0 ? sbyte.MinValue : short.MinValue);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                context.Emit(OpCodes.Dup);
                context.EmitStvectmp();

                context.EmitLdvec(op.Rn);
                context.EmitLdvectmp();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesSubAdd));

                context.EmitLdvec(op.Rm);
                context.EmitLdvectmp();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesSubAdd));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Average),  typesAvg));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add),      typesSubAdd));

                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorBinaryOpSx(context, () =>
                {
                    context.Emit(OpCodes.Add);

                    context.Emit(OpCodes.Ldc_I4_1);
                    context.Emit(OpCodes.Add);

                    context.Emit(OpCodes.Ldc_I4_1);
                    context.Emit(OpCodes.Shr);
                });
            }
        }

        public static void Ssubl_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesSrl = new Type[] { VectorIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesCvt = new Type[] { VectorIntTypesPerSizeLog2[op.Size] };
                Type[] typesSub = new Type[] { VectorIntTypesPerSizeLog2[op.Size + 1],
                                               VectorIntTypesPerSizeLog2[op.Size + 1] };

                string[] namesCvt = new string[] { nameof(Sse41.ConvertToVector128Int16),
                                                   nameof(Sse41.ConvertToVector128Int32),
                                                   nameof(Sse41.ConvertToVector128Int64) };

                int numBytes = op.RegisterSize == RegisterSize.Simd128 ? 8 : 0;

                context.EmitLdvec(op.Rn);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(namesCvt[op.Size], typesCvt));

                context.EmitLdvec(op.Rm);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(namesCvt[op.Size], typesCvt));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesSub));

                context.EmitStvec(op.Rd);
            }
            else
            {
                EmitVectorWidenRnRmBinaryOpSx(context, () => context.Emit(OpCodes.Sub));
            }
        }

        public static void Ssubw_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesSrl = new Type[] { VectorIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesCvt = new Type[] { VectorIntTypesPerSizeLog2[op.Size] };
                Type[] typesSub = new Type[] { VectorIntTypesPerSizeLog2[op.Size + 1],
                                               VectorIntTypesPerSizeLog2[op.Size + 1] };

                string[] namesCvt = new string[] { nameof(Sse41.ConvertToVector128Int16),
                                                   nameof(Sse41.ConvertToVector128Int32),
                                                   nameof(Sse41.ConvertToVector128Int64) };

                int numBytes = op.RegisterSize == RegisterSize.Simd128 ? 8 : 0;

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rm);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(namesCvt[op.Size], typesCvt));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesSub));

                context.EmitStvec(op.Rd);
            }
            else
            {
                EmitVectorWidenRmBinaryOpSx(context, () => context.Emit(OpCodes.Sub));
            }
        }

        public static void Sub_S(ILEmitterCtx context)
        {
            EmitScalarBinaryOpZx(context, () => context.Emit(OpCodes.Sub));
        }

        public static void Sub_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                EmitSse2Op(context, nameof(Sse2.Subtract));
            }
            else
            {
                EmitVectorBinaryOpZx(context, () => context.Emit(OpCodes.Sub));
            }
        }

        public static void Subhn_V(ILEmitterCtx context)
        {
            EmitHighNarrow(context, () => context.Emit(OpCodes.Sub), round: false);
        }

        public static void Suqadd_S(ILEmitterCtx context)
        {
            EmitScalarSaturatingBinaryOpSx(context, SaturatingFlags.Accumulate);
        }

        public static void Suqadd_V(ILEmitterCtx context)
        {
            EmitVectorSaturatingBinaryOpSx(context, SaturatingFlags.Accumulate);
        }

        public static void Uaba_V(ILEmitterCtx context)
        {
            EmitVectorTernaryOpZx(context, () =>
            {
                context.Emit(OpCodes.Sub);
                EmitAbs(context);

                context.Emit(OpCodes.Add);
            });
        }

        public static void Uabal_V(ILEmitterCtx context)
        {
            EmitVectorWidenRnRmTernaryOpZx(context, () =>
            {
                context.Emit(OpCodes.Sub);
                EmitAbs(context);

                context.Emit(OpCodes.Add);
            });
        }

        public static void Uabd_V(ILEmitterCtx context)
        {
            EmitVectorBinaryOpZx(context, () =>
            {
                context.Emit(OpCodes.Sub);
                EmitAbs(context);
            });
        }

        public static void Uabdl_V(ILEmitterCtx context)
        {
            EmitVectorWidenRnRmBinaryOpZx(context, () =>
            {
                context.Emit(OpCodes.Sub);
                EmitAbs(context);
            });
        }

        public static void Uadalp_V(ILEmitterCtx context)
        {
            EmitAddLongPairwise(context, signed: false, accumulate: true);
        }

        public static void Uaddl_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesSrl = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesCvt = new Type[] { VectorUIntTypesPerSizeLog2[op.Size] };
                Type[] typesAdd = new Type[] { VectorUIntTypesPerSizeLog2[op.Size + 1],
                                               VectorUIntTypesPerSizeLog2[op.Size + 1] };

                string[] namesCvt = new string[] { nameof(Sse41.ConvertToVector128Int16),
                                                   nameof(Sse41.ConvertToVector128Int32),
                                                   nameof(Sse41.ConvertToVector128Int64) };

                int numBytes = op.RegisterSize == RegisterSize.Simd128 ? 8 : 0;

                context.EmitLdvec(op.Rn);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(namesCvt[op.Size], typesCvt));

                context.EmitLdvec(op.Rm);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(namesCvt[op.Size], typesCvt));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));

                context.EmitStvec(op.Rd);
            }
            else
            {
                EmitVectorWidenRnRmBinaryOpZx(context, () => context.Emit(OpCodes.Add));
            }
        }

        public static void Uaddlp_V(ILEmitterCtx context)
        {
            EmitAddLongPairwise(context, signed: false, accumulate: false);
        }

        public static void Uaddlv_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int bytes = op.GetBitsCount() >> 3;
            int elems = bytes >> op.Size;

            EmitVectorExtractZx(context, op.Rn, 0, op.Size);

            for (int index = 1; index < elems; index++)
            {
                EmitVectorExtractZx(context, op.Rn, index, op.Size);

                context.Emit(OpCodes.Add);
            }

            EmitScalarSet(context, op.Rd, op.Size + 1);
        }

        public static void Uaddw_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesSrl = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesCvt = new Type[] { VectorUIntTypesPerSizeLog2[op.Size] };
                Type[] typesAdd = new Type[] { VectorUIntTypesPerSizeLog2[op.Size + 1],
                                               VectorUIntTypesPerSizeLog2[op.Size + 1] };

                string[] namesCvt = new string[] { nameof(Sse41.ConvertToVector128Int16),
                                                   nameof(Sse41.ConvertToVector128Int32),
                                                   nameof(Sse41.ConvertToVector128Int64) };

                int numBytes = op.RegisterSize == RegisterSize.Simd128 ? 8 : 0;

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rm);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(namesCvt[op.Size], typesCvt));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));

                context.EmitStvec(op.Rd);
            }
            else
            {
                EmitVectorWidenRmBinaryOpZx(context, () => context.Emit(OpCodes.Add));
            }
        }

        public static void Uhadd_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                Type[] typesSrl       = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesAndXorAdd = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], VectorUIntTypesPerSizeLog2[op.Size] };

                context.EmitLdvec(op.Rn);

                context.Emit(OpCodes.Dup);
                context.EmitStvectmp();

                context.EmitLdvec(op.Rm);

                context.Emit(OpCodes.Dup);
                context.EmitStvectmp2();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.And), typesAndXorAdd));

                context.EmitLdvectmp();
                context.EmitLdvectmp2();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), typesAndXorAdd));

                context.EmitLdc_I4(1);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical), typesSrl));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add),               typesAndXorAdd));

                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorBinaryOpZx(context, () =>
                {
                    context.Emit(OpCodes.Add);

                    context.Emit(OpCodes.Ldc_I4_1);
                    context.Emit(OpCodes.Shr_Un);
                });
            }
        }

        public static void Uhsub_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size < 2)
            {
                Type[] typesAvgSub = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], VectorUIntTypesPerSizeLog2[op.Size] };

                context.EmitLdvec(op.Rn);
                context.Emit(OpCodes.Dup);

                context.EmitLdvec(op.Rm);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Average),  typesAvgSub));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesAvgSub));

                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorBinaryOpZx(context, () =>
                {
                    context.Emit(OpCodes.Sub);

                    context.Emit(OpCodes.Ldc_I4_1);
                    context.Emit(OpCodes.Shr_Un);
                });
            }
        }

        public static void Umax_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesMax = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], VectorUIntTypesPerSizeLog2[op.Size] };

                Type typeSse = op.Size == 0 ? typeof(Sse2) : typeof(Sse41);

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rm);

                context.EmitCall(typeSse.GetMethod(nameof(Sse2.Max), typesMax));

                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                Type[] types = new Type[] { typeof(ulong), typeof(ulong) };

                MethodInfo mthdInfo = typeof(Math).GetMethod(nameof(Math.Max), types);

                EmitVectorBinaryOpZx(context, () => context.EmitCall(mthdInfo));
            }
        }

        public static void Umaxp_V(ILEmitterCtx context)
        {
            Type[] types = new Type[] { typeof(ulong), typeof(ulong) };

            MethodInfo mthdInfo = typeof(Math).GetMethod(nameof(Math.Max), types);

            EmitVectorPairwiseOpZx(context, () => context.EmitCall(mthdInfo));
        }

        public static void Umin_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesMin = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], VectorUIntTypesPerSizeLog2[op.Size] };

                Type typeSse = op.Size == 0 ? typeof(Sse2) : typeof(Sse41);

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rm);

                context.EmitCall(typeSse.GetMethod(nameof(Sse2.Min), typesMin));

                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                Type[] types = new Type[] { typeof(ulong), typeof(ulong) };

                MethodInfo mthdInfo = typeof(Math).GetMethod(nameof(Math.Min), types);

                EmitVectorBinaryOpZx(context, () => context.EmitCall(mthdInfo));
            }
        }

        public static void Uminp_V(ILEmitterCtx context)
        {
            Type[] types = new Type[] { typeof(ulong), typeof(ulong) };

            MethodInfo mthdInfo = typeof(Math).GetMethod(nameof(Math.Min), types);

            EmitVectorPairwiseOpZx(context, () => context.EmitCall(mthdInfo));
        }

        public static void Umlal_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse41 && op.Size < 2)
            {
                Type[] typesSrl    = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesCvt    = new Type[] { VectorUIntTypesPerSizeLog2[op.Size] };
                Type[] typesMulAdd = new Type[] { VectorIntTypesPerSizeLog2 [op.Size + 1],
                                                  VectorIntTypesPerSizeLog2 [op.Size + 1] };

                Type typeSse = op.Size == 0 ? typeof(Sse2) : typeof(Sse41);

                string nameCvt = op.Size == 0
                    ? nameof(Sse41.ConvertToVector128Int16)
                    : nameof(Sse41.ConvertToVector128Int32);

                int numBytes = op.RegisterSize == RegisterSize.Simd128 ? 8 : 0;

                context.EmitLdvec(op.Rd);
                context.EmitLdvec(op.Rn);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(nameCvt, typesCvt));

                context.EmitLdvec(op.Rm);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(nameCvt, typesCvt));

                context.EmitCall(typeSse.GetMethod(nameof(Sse2.MultiplyLow), typesMulAdd));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesMulAdd));

                context.EmitStvec(op.Rd);
            }
            else
            {
                EmitVectorWidenRnRmTernaryOpZx(context, () =>
                {
                    context.Emit(OpCodes.Mul);
                    context.Emit(OpCodes.Add);
                });
            }
        }

        public static void Umlal_Ve(ILEmitterCtx context)
        {
            EmitVectorWidenTernaryOpByElemZx(context, () =>
            {
                context.Emit(OpCodes.Mul);
                context.Emit(OpCodes.Add);
            });
        }

        public static void Umlsl_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse41 && op.Size < 2)
            {
                Type[] typesSrl    = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesCvt    = new Type[] { VectorUIntTypesPerSizeLog2[op.Size] };
                Type[] typesMulSub = new Type[] { VectorIntTypesPerSizeLog2 [op.Size + 1],
                                                  VectorIntTypesPerSizeLog2 [op.Size + 1] };

                Type typeSse = op.Size == 0 ? typeof(Sse2) : typeof(Sse41);

                string nameCvt = op.Size == 0
                    ? nameof(Sse41.ConvertToVector128Int16)
                    : nameof(Sse41.ConvertToVector128Int32);

                int numBytes = op.RegisterSize == RegisterSize.Simd128 ? 8 : 0;

                context.EmitLdvec(op.Rd);
                context.EmitLdvec(op.Rn);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(nameCvt, typesCvt));

                context.EmitLdvec(op.Rm);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(nameCvt, typesCvt));

                context.EmitCall(typeSse.GetMethod(nameof(Sse2.MultiplyLow), typesMulSub));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesMulSub));

                context.EmitStvec(op.Rd);
            }
            else
            {
                EmitVectorWidenRnRmTernaryOpZx(context, () =>
                {
                    context.Emit(OpCodes.Mul);
                    context.Emit(OpCodes.Sub);
                });
            }
        }

        public static void Umlsl_Ve(ILEmitterCtx context)
        {
            EmitVectorWidenTernaryOpByElemZx(context, () =>
            {
                context.Emit(OpCodes.Mul);
                context.Emit(OpCodes.Sub);
            });
        }

        public static void Umull_V(ILEmitterCtx context)
        {
            EmitVectorWidenRnRmBinaryOpZx(context, () => context.Emit(OpCodes.Mul));
        }

        public static void Umull_Ve(ILEmitterCtx context)
        {
            EmitVectorWidenBinaryOpByElemZx(context, () => context.Emit(OpCodes.Mul));
        }

        public static void Uqadd_S(ILEmitterCtx context)
        {
            EmitScalarSaturatingBinaryOpZx(context, SaturatingFlags.Add);
        }

        public static void Uqadd_V(ILEmitterCtx context)
        {
            EmitVectorSaturatingBinaryOpZx(context, SaturatingFlags.Add);
        }

        public static void Uqsub_S(ILEmitterCtx context)
        {
            EmitScalarSaturatingBinaryOpZx(context, SaturatingFlags.Sub);
        }

        public static void Uqsub_V(ILEmitterCtx context)
        {
            EmitVectorSaturatingBinaryOpZx(context, SaturatingFlags.Sub);
        }

        public static void Uqxtn_S(ILEmitterCtx context)
        {
            EmitSaturatingNarrowOp(context, SaturatingNarrowFlags.ScalarZxZx);
        }

        public static void Uqxtn_V(ILEmitterCtx context)
        {
            EmitSaturatingNarrowOp(context, SaturatingNarrowFlags.VectorZxZx);
        }

        public static void Urhadd_V(ILEmitterCtx context)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size < 2)
            {
                Type[] typesAvg = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], VectorUIntTypesPerSizeLog2[op.Size] };

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rm);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Average), typesAvg));

                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorBinaryOpZx(context, () =>
                {
                    context.Emit(OpCodes.Add);

                    context.Emit(OpCodes.Ldc_I4_1);
                    context.Emit(OpCodes.Add);

                    context.Emit(OpCodes.Ldc_I4_1);
                    context.Emit(OpCodes.Shr_Un);
                });
            }
        }

        public static void Usqadd_S(ILEmitterCtx context)
        {
            EmitScalarSaturatingBinaryOpZx(context, SaturatingFlags.Accumulate);
        }

        public static void Usqadd_V(ILEmitterCtx context)
        {
            EmitVectorSaturatingBinaryOpZx(context, SaturatingFlags.Accumulate);
        }

        public static void Usubl_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesSrl = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesCvt = new Type[] { VectorUIntTypesPerSizeLog2[op.Size] };
                Type[] typesSub = new Type[] { VectorUIntTypesPerSizeLog2[op.Size + 1],
                                               VectorUIntTypesPerSizeLog2[op.Size + 1] };

                string[] namesCvt = new string[] { nameof(Sse41.ConvertToVector128Int16),
                                                   nameof(Sse41.ConvertToVector128Int32),
                                                   nameof(Sse41.ConvertToVector128Int64) };

                int numBytes = op.RegisterSize == RegisterSize.Simd128 ? 8 : 0;

                context.EmitLdvec(op.Rn);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(namesCvt[op.Size], typesCvt));

                context.EmitLdvec(op.Rm);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(namesCvt[op.Size], typesCvt));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesSub));

                context.EmitStvec(op.Rd);
            }
            else
            {
                EmitVectorWidenRnRmBinaryOpZx(context, () => context.Emit(OpCodes.Sub));
            }
        }

        public static void Usubw_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesSrl = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesCvt = new Type[] { VectorUIntTypesPerSizeLog2[op.Size] };
                Type[] typesSub = new Type[] { VectorUIntTypesPerSizeLog2[op.Size + 1],
                                               VectorUIntTypesPerSizeLog2[op.Size + 1] };

                string[] namesCvt = new string[] { nameof(Sse41.ConvertToVector128Int16),
                                                   nameof(Sse41.ConvertToVector128Int32),
                                                   nameof(Sse41.ConvertToVector128Int64) };

                int numBytes = op.RegisterSize == RegisterSize.Simd128 ? 8 : 0;

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rm);

                context.EmitLdc_I4(numBytes);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical128BitLane), typesSrl));

                context.EmitCall(typeof(Sse41).GetMethod(namesCvt[op.Size], typesCvt));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), typesSub));

                context.EmitStvec(op.Rd);
            }
            else
            {
                EmitVectorWidenRmBinaryOpZx(context, () => context.Emit(OpCodes.Sub));
            }
        }

        private static void EmitAbs(ILEmitterCtx context)
        {
            ILLabel lblTrue = new ILLabel();

            context.Emit(OpCodes.Dup);
            context.Emit(OpCodes.Ldc_I4_0);
            context.Emit(OpCodes.Bge_S, lblTrue);

            context.Emit(OpCodes.Neg);

            context.MarkLabel(lblTrue);
        }

        private static void EmitAddLongPairwise(ILEmitterCtx context, bool signed, bool accumulate)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int words = op.GetBitsCount() >> 4;
            int pairs = words >> op.Size;

            for (int index = 0; index < pairs; index++)
            {
                int idx = index << 1;

                EmitVectorExtract(context, op.Rn, idx,     op.Size, signed);
                EmitVectorExtract(context, op.Rn, idx + 1, op.Size, signed);

                context.Emit(OpCodes.Add);

                if (accumulate)
                {
                    EmitVectorExtract(context, op.Rd, index, op.Size + 1, signed);

                    context.Emit(OpCodes.Add);
                }

                EmitVectorInsertTmp(context, index, op.Size + 1);
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        private static void EmitDoublingMultiplyHighHalf(ILEmitterCtx context, bool round)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            int eSize = 8 << op.Size;

            context.Emit(OpCodes.Mul);

            if (!round)
            {
                context.EmitAsr(eSize - 1);
            }
            else
            {
                long roundConst = 1L << (eSize - 1);

                ILLabel lblTrue = new ILLabel();

                context.EmitLsl(1);

                context.EmitLdc_I8(roundConst);

                context.Emit(OpCodes.Add);

                context.EmitAsr(eSize);

                context.Emit(OpCodes.Dup);
                context.EmitLdc_I8((long)int.MinValue);
                context.Emit(OpCodes.Bne_Un_S, lblTrue);

                context.Emit(OpCodes.Neg);

                context.MarkLabel(lblTrue);
            }
        }

        private static void EmitHighNarrow(ILEmitterCtx context, Action emit, bool round)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            int elems = 8 >> op.Size;

            int eSize = 8 << op.Size;

            int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

            long roundConst = 1L << (eSize - 1);

            if (part != 0)
            {
                context.EmitLdvec(op.Rd);
                context.EmitStvectmp();
            }

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractZx(context, op.Rn, index, op.Size + 1);
                EmitVectorExtractZx(context, op.Rm, index, op.Size + 1);

                emit();

                if (round)
                {
                    context.EmitLdc_I8(roundConst);

                    context.Emit(OpCodes.Add);
                }

                context.EmitLsr(eSize);

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
}
