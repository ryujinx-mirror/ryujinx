using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection;
using System.Reflection.Emit;

using static ChocolArm64.Instruction.AInstEmitMemoryHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Add_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryZx(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Addp_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            int Elems = Bytes >> Op.Size;
            int Half  = Elems >> 1;

            for (int Index = 0; Index < Elems; Index++)
            {
                int Elem = (Index & (Half - 1)) << 1;
                
                EmitVectorExtractZx(Context, Index < Half ? Op.Rn : Op.Rm, Elem + 0, Op.Size);
                EmitVectorExtractZx(Context, Index < Half ? Op.Rn : Op.Rm, Elem + 1, Op.Size);

                Context.Emit(OpCodes.Add);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Addv_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            int Results = 0;        

            for (int Size = Op.Size; Size < 4; Size++)
            {
                for (int Index = 0; Index < (Bytes >> Size); Index += 2)
                {
                    EmitVectorExtractZx(Context, Op.Rn, Index + 0, Size);
                    EmitVectorExtractZx(Context, Op.Rn, Index + 1, Size);

                    Context.Emit(OpCodes.Add);

                    Results++;
                }
            }

            while (--Results > 0)
            {
                Context.Emit(OpCodes.Add);
            }

            EmitVectorZeroLower(Context, Op.Rd);
            EmitVectorZeroUpper(Context, Op.Rd);

            EmitVectorInsert(Context, Op.Rd, 0, Op.Size);
        }

        public static void And_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryZx(Context, () => Context.Emit(OpCodes.And));
        }

        public static void Bic_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryZx(Context, () =>
            {
                Context.Emit(OpCodes.Not);
                Context.Emit(OpCodes.And);
            });
        }

        public static void Bic_Vi(AILEmitterCtx Context)
        {
            EmitVectorImmBinary(Context, () =>
            {
                Context.Emit(OpCodes.Not);
                Context.Emit(OpCodes.And);
            });
        }

        public static void Bsl_V(AILEmitterCtx Context)
        {
            EmitVectorTernaryZx(Context, () =>
            {
                Context.EmitSttmp();
                Context.EmitLdtmp();

                Context.Emit(OpCodes.Xor);
                Context.Emit(OpCodes.And);

                Context.EmitLdtmp();

                Context.Emit(OpCodes.Xor);
            });
        }

        public static void Cmeq_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Beq_S);
        }

        public static void Cmge_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Bge_S);
        }

        public static void Cmgt_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Bgt_S);
        }

        public static void Cmhi_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Bgt_Un_S);
        }

        public static void Cmhs_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Bge_Un_S);
        }

        public static void Cmle_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Ble_S);
        }

        public static void Cmlt_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Blt_S);
        }

        public static void Cnt_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Elems = Op.RegisterSize == ARegisterSize.SIMD128 ? 16 : 8;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, 0);

                Context.Emit(OpCodes.Conv_U1);

                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.CountSetBits8));

                Context.Emit(OpCodes.Conv_U8);

                EmitVectorInsert(Context, Op.Rd, Index, 0);
            }
        }

        public static void Dup_Gp(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                Context.EmitLdintzr(Op.Rn);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Dup_V(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Op.DstIndex, Op.Size);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Eor_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryZx(Context, () => Context.Emit(OpCodes.Xor));
        }

        public static void Fadd_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryF(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Fcvtzs_V(AILEmitterCtx Context)
        {
            EmitVectorFcvt(Context, Signed: true);
        }

        public static void Fcvtzu_V(AILEmitterCtx Context)
        {
            EmitVectorFcvt(Context, Signed: false);
        }

        public static void Fmla_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            EmitVectorTernaryF(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Add);
            });
        }

        public static void Fmla_Ve(AILEmitterCtx Context)
        {
            EmitVectorTernaryByElemF(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Add);
            });
        }

        public static void Fmov_V(AILEmitterCtx Context)
        {
            AOpCodeSimdImm Op = (AOpCodeSimdImm)Context.CurrOp;

            Context.EmitLdc_I8(Op.Imm);
            Context.EmitLdc_I4(Op.Size + 2);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Dup_Gp64),
                nameof(ASoftFallback.Dup_Gp128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Fmul_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryF(Context, () => Context.Emit(OpCodes.Mul));
        }
    
        public static void Fmul_Ve(AILEmitterCtx Context)
        {
            EmitVectorBinaryByElemF(Context, () => Context.Emit(OpCodes.Mul));
        }

        public static void Fsub_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryF(Context, () => Context.Emit(OpCodes.Sub));
        }

        public static void Ins_Gp(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            EmitVectorInsert(Context, Op.Rd, Op.DstIndex, Op.Size);
        }

        public static void Ins_V(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            EmitVectorExtractZx(Context, Op.Rn, Op.SrcIndex, Op.Size);

            EmitVectorInsert(Context, Op.Rd, Op.DstIndex, Op.Size);
        }

        public static void Ld__Vms(AILEmitterCtx Context)
        {
            EmitSimdMemMs(Context, IsLoad: true);
        }

        public static void Ld__Vss(AILEmitterCtx Context)
        {
            EmitSimdMemSs(Context, IsLoad: true);
        }

        public static void Mla_V(AILEmitterCtx Context)
        {
            EmitVectorTernaryZx(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Add);
            });
        }

        public static void Movi_V(AILEmitterCtx Context)
        {
            EmitVectorImmUnary(Context, () => { });
        }

        public static void Mul_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryZx(Context, () => Context.Emit(OpCodes.Mul));
        }

        public static void Mvni_V(AILEmitterCtx Context)
        {
            EmitVectorImmUnary(Context, () => Context.Emit(OpCodes.Not));
        }

        public static void Neg_V(AILEmitterCtx Context)
        {
            EmitVectorUnarySx(Context, () => Context.Emit(OpCodes.Neg));
        }

        public static void Not_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryZx(Context, () => Context.Emit(OpCodes.Not));
        }

        public static void Orr_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryZx(Context, () => Context.Emit(OpCodes.Or));
        }

        public static void Orr_Vi(AILEmitterCtx Context)
        {
            EmitVectorImmBinary(Context, () => Context.Emit(OpCodes.Or));
        }

        public static void Saddw_V(AILEmitterCtx Context)
        {
            EmitVectorWidenBinarySx(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Scvtf_V(AILEmitterCtx Context)
        {
            EmitVectorCvtf(Context, Signed: true);
        }

        public static void Shl_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Shift = Op.Imm - (8 << Op.Size);

            EmitVectorShImmBinaryZx(Context, () => Context.Emit(OpCodes.Shl), Shift);
        }

        public static void Shrn_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Shift = (8 << (Op.Size + 1)) - Op.Imm;

            EmitVectorShImmNarrowBinaryZx(Context, () => Context.Emit(OpCodes.Shr_Un), Shift);
        }

        public static void Smax_V(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(long), typeof(long) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Max), Types);

            EmitVectorBinarySx(Context, () => Context.EmitCall(MthdInfo));
        }

        public static void Smin_V(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(long), typeof(long) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Min), Types);

            EmitVectorBinarySx(Context, () => Context.EmitCall(MthdInfo));
        }

        public static void Sshl_V(AILEmitterCtx Context)
        {
            EmitVectorShl(Context, Signed: true);
        }

        public static void Sshll_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Shift = Op.Imm - (8 << Op.Size);

            EmitVectorShImmWidenBinarySx(Context, () => Context.Emit(OpCodes.Shl), Shift);
        }

        public static void Sshr_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Shift = (8 << (Op.Size + 1)) - Op.Imm;

            EmitVectorShImmBinarySx(Context, () => Context.Emit(OpCodes.Shr), Shift);
        }

        public static void St__Vms(AILEmitterCtx Context)
        {
            EmitSimdMemMs(Context, IsLoad: false);
        }

        public static void St__Vss(AILEmitterCtx Context)
        {
            EmitSimdMemSs(Context, IsLoad: false);
        }

        public static void Sub_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryZx(Context, () => Context.Emit(OpCodes.Sub));
        }

        public static void Tbl_V(AILEmitterCtx Context)
        {
            AOpCodeSimdTbl Op = (AOpCodeSimdTbl)Context.CurrOp;

            Context.EmitLdvec(Op.Rm);

            for (int Index = 0; Index < Op.Size; Index++)
            {
                Context.EmitLdvec((Op.Rn + Index) & 0x1f);
            }

            switch (Op.Size)
            {
                case 1: ASoftFallback.EmitCall(Context,
                    nameof(ASoftFallback.Tbl1_V64),
                    nameof(ASoftFallback.Tbl1_V128)); break;

                case 2: ASoftFallback.EmitCall(Context,
                    nameof(ASoftFallback.Tbl2_V64),
                    nameof(ASoftFallback.Tbl2_V128)); break;

                case 3: ASoftFallback.EmitCall(Context,
                    nameof(ASoftFallback.Tbl3_V64),
                    nameof(ASoftFallback.Tbl3_V128)); break;

                case 4: ASoftFallback.EmitCall(Context,
                    nameof(ASoftFallback.Tbl4_V64),
                    nameof(ASoftFallback.Tbl4_V128)); break;

                default: throw new InvalidOperationException();
            }

            Context.EmitStvec(Op.Rd);
        }

        public static void Uaddlv_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            EmitVectorExtractZx(Context, Op.Rn, 0, Op.Size);

            for (int Index = 1; Index < (Bytes >> Op.Size); Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size);

                Context.Emit(OpCodes.Add);
            }

            EmitVectorZeroLower(Context, Op.Rd);
            EmitVectorZeroUpper(Context, Op.Rd);

            EmitVectorInsert(Context, Op.Rd, 0, Op.Size);
        }

        public static void Uaddw_V(AILEmitterCtx Context)
        {
            EmitVectorWidenBinaryZx(Context, () => Context.Emit(OpCodes.Add));
        }

        public static void Ucvtf_V(AILEmitterCtx Context)
        {
            EmitVectorCvtf(Context, Signed: false);
        }

        public static void Ushl_V(AILEmitterCtx Context)
        {
            EmitVectorShl(Context, Signed: false);
        }

        public static void Ushll_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Shift = Op.Imm - (8 << Op.Size);

            EmitVectorShImmWidenBinaryZx(Context, () => Context.Emit(OpCodes.Shl), Shift);
        }

        public static void Ushr_V(AILEmitterCtx Context)
        {
            EmitVectorShr(Context, ShrFlags.None);
        }

        public static void Usra_V(AILEmitterCtx Context)
        {
            EmitVectorShr(Context, ShrFlags.Accumulate);
        }

        [Flags]
        private enum ShrFlags
        {
            None       = 0,
            Signed     = 1 << 0,
            Rounding   = 1 << 1,
            Accumulate = 1 << 2
        }

        private static void EmitVectorShr(AILEmitterCtx Context, ShrFlags Flags)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Shift = (8 << (Op.Size + 1)) - Op.Imm;

            if (Flags.HasFlag(ShrFlags.Accumulate))
            {
                Action Emit = () =>
                {
                    Context.EmitLdc_I4(Shift);

                    Context.Emit(OpCodes.Shr_Un);
                    Context.Emit(OpCodes.Add);
                };

                EmitVectorOp(Context, Emit, OperFlags.RdRn, Signed: false);
            }
            else
            {
                EmitVectorUnaryZx(Context, () =>
                {
                    Context.EmitLdc_I4(Shift);

                    Context.Emit(OpCodes.Shr_Un);
                });
            }
        }

        public static void Uzp1_V(AILEmitterCtx Context)
        {
            EmitVectorUnzip(Context, Part: 0);
        }

        public static void Uzp2_V(AILEmitterCtx Context)
        {
            EmitVectorUnzip(Context, Part: 1);
        }

        private static void EmitVectorUnzip(AILEmitterCtx Context, int Part)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            int Elems = Bytes >> Op.Size;
            int Half  = Elems >> 1;

            for (int Index = 0; Index < Elems; Index++)
            {
                int Elem = Part + ((Index & (Half - 1)) << 1);
                
                EmitVectorExtractZx(Context, Index < Half ? Op.Rn : Op.Rm, Elem, Op.Size);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Xtn_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Elems = 8 >> Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size + 1);

                EmitVectorInsert(Context, Op.Rd, Part + Index, Op.Size);
            }

            if (Part == 0)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitSimdMemMs(AILEmitterCtx Context, bool IsLoad)
        {
            AOpCodeSimdMemMs Op = (AOpCodeSimdMemMs)Context.CurrOp;

            int Offset = 0;

            for (int Rep   = 0; Rep   < Op.Reps;   Rep++)
            for (int Elem  = 0; Elem  < Op.Elems;  Elem++)
            for (int SElem = 0; SElem < Op.SElems; SElem++)
            {
                int Rtt = (Op.Rt + Rep + SElem) & 0x1f;

                if (IsLoad)
                {
                    Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
                    Context.EmitLdint(Op.Rn);
                    Context.EmitLdc_I8(Offset);

                    Context.Emit(OpCodes.Add);

                    EmitReadZxCall(Context, Op.Size);

                    EmitVectorInsert(Context, Rtt, Elem, Op.Size);

                    if (Op.RegisterSize == ARegisterSize.SIMD64 && Elem == Op.Elems - 1)
                    {
                        EmitVectorZeroUpper(Context, Rtt);
                    }
                }
                else
                {
                    Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
                    Context.EmitLdint(Op.Rn);
                    Context.EmitLdc_I8(Offset);

                    Context.Emit(OpCodes.Add);

                    EmitVectorExtractZx(Context, Rtt, Elem, Op.Size);

                    EmitWriteCall(Context, Op.Size);
                }

                Offset += 1 << Op.Size;
            }

            if (Op.WBack)
            {
                EmitSimdMemWBack(Context, Offset);
            }
        }

        private static void EmitSimdMemSs(AILEmitterCtx Context, bool IsLoad)
        {
            AOpCodeSimdMemSs Op = (AOpCodeSimdMemSs)Context.CurrOp;

            //TODO: Replicate mode.

            int Offset = 0;

            for (int SElem = 0; SElem < Op.SElems; SElem++)
            {
                int Rt = (Op.Rt + SElem) & 0x1f;

                if (IsLoad)
                {
                    Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
                    Context.EmitLdint(Op.Rn);
                    Context.EmitLdc_I8(Offset);

                    Context.Emit(OpCodes.Add);

                    EmitReadZxCall(Context, Op.Size);

                    EmitVectorInsert(Context, Rt, Op.Index, Op.Size);

                    if (Op.RegisterSize == ARegisterSize.SIMD64)
                    {
                        EmitVectorZeroUpper(Context, Rt);
                    }
                }
                else
                {
                    Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
                    Context.EmitLdint(Op.Rn);
                    Context.EmitLdc_I8(Offset);

                    Context.Emit(OpCodes.Add);

                    EmitVectorExtractZx(Context, Rt, Op.Index, Op.Size);

                    EmitWriteCall(Context, Op.Size);
                }

                Offset += 1 << Op.Size;
            }

            if (Op.WBack)
            {
                EmitSimdMemWBack(Context, Offset);
            }
        }

        private static void EmitSimdMemWBack(AILEmitterCtx Context, int Offset)
        {
            AOpCodeMemReg Op = (AOpCodeMemReg)Context.CurrOp;

            Context.EmitLdint(Op.Rn);

            if (Op.Rm != ARegisters.ZRIndex)
            {
                Context.EmitLdint(Op.Rm);
            }
            else
            {
                Context.EmitLdc_I8(Offset);
            }

            Context.Emit(OpCodes.Add);

            Context.EmitStint(Op.Rn);
        }

        private static void EmitVectorCmp(AILEmitterCtx Context, OpCode ILOp)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            ulong SzMask = ulong.MaxValue >> (64 - (8 << Op.Size));

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                EmitVectorExtractSx(Context, Op.Rn, Index, Op.Size);

                if (Op is AOpCodeSimdReg BinOp)
                {
                    EmitVectorExtractSx(Context, BinOp.Rm, Index, Op.Size);
                }
                else
                {
                    Context.EmitLdc_I8(0);
                }

                AILLabel LblTrue = new AILLabel();
                AILLabel LblEnd  = new AILLabel();

                Context.Emit(ILOp, LblTrue);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size, 0);

                Context.Emit(OpCodes.Br_S, LblEnd);

                Context.MarkLabel(LblTrue);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size, (long)SzMask);

                Context.MarkLabel(LblEnd);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitVectorShl(AILEmitterCtx Context, bool Signed)
        {
            //This instruction shifts the value on vector A by the number of bits
            //specified on the signed, lower 8 bits of vector B. If the shift value
            //is greater or equal to the data size of each lane, then the result is zero.
            //Additionally, negative shifts produces right shifts by the negated shift value.
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int MaxShift = 8 << Op.Size;

            Action Emit = () =>
            {
                AILLabel LblShl  = new AILLabel();
                AILLabel LblZero = new AILLabel();
                AILLabel LblEnd  = new AILLabel();

                void EmitShift(OpCode ILOp)
                {
                    Context.Emit(OpCodes.Dup);

                    Context.EmitLdc_I4(MaxShift);

                    Context.Emit(OpCodes.Bge_S, LblZero);
                    Context.Emit(ILOp);
                    Context.Emit(OpCodes.Br_S, LblEnd);
                }

                Context.Emit(OpCodes.Conv_I1);
                Context.Emit(OpCodes.Dup);

                Context.EmitLdc_I4(0);

                Context.Emit(OpCodes.Bge_S, LblShl);
                Context.Emit(OpCodes.Neg);

                EmitShift(Signed
                    ? OpCodes.Shr
                    : OpCodes.Shr_Un);

                Context.MarkLabel(LblShl);

                EmitShift(OpCodes.Shl);

                Context.MarkLabel(LblZero);

                Context.Emit(OpCodes.Pop);
                Context.Emit(OpCodes.Pop);

                Context.EmitLdc_I8(0);

                Context.MarkLabel(LblEnd);
            };

            if (Signed)
            {
                EmitVectorBinarySx(Context, Emit);
            }
            else
            {
                EmitVectorBinaryZx(Context, Emit);
            }
        }

        private static void EmitVectorFcvt(AILEmitterCtx Context, bool Signed)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;
            int SizeI = SizeF + 2;

            int FBits = GetFBits(Context);

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> SizeI); Index++)
            {
                EmitVectorExtractF(Context, Op.Rn, Index, SizeF);

                Context.EmitLdc_I4(FBits);

                if (SizeF == 0)
                {
                    ASoftFallback.EmitCall(Context, Signed
                        ? nameof(ASoftFallback.SatSingleToInt32)
                        : nameof(ASoftFallback.SatSingleToUInt32));
                }
                else if (SizeF == 1)
                {
                    ASoftFallback.EmitCall(Context, Signed
                        ? nameof(ASoftFallback.SatDoubleToInt64)
                        : nameof(ASoftFallback.SatDoubleToUInt64));
                }

                EmitVectorInsert(Context, Op.Rd, Index, SizeI);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitVectorCvtf(AILEmitterCtx Context, bool Signed)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;
            int SizeI = SizeF + 2;

            int FBits = GetFBits(Context);

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> SizeI); Index++)
            {
                EmitVectorExtract(Context, Op.Rn, Index, SizeI, Signed);

                Context.EmitLdc_I4(FBits);

                if (SizeF == 0)
                {
                    Context.Emit(OpCodes.Conv_I4);

                    ASoftFallback.EmitCall(Context, Signed
                        ? nameof(ASoftFallback.Int32ToSingle)
                        : nameof(ASoftFallback.UInt32ToSingle));
                }
                else if (SizeF == 1)
                {
                    ASoftFallback.EmitCall(Context, Signed
                        ? nameof(ASoftFallback.Int64ToDouble)
                        : nameof(ASoftFallback.UInt64ToDouble));
                }

                EmitVectorInsertF(Context, Op.Rd, Index, SizeF);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static int GetFBits(AILEmitterCtx Context)
        {
            if (Context.CurrOp is AOpCodeSimdShImm Op)
            {
                return (8 << (Op.Size + 1)) - Op.Imm;
            }

            return 0;
        }

        [Flags]
        private enum OperFlags
        {
            Rd = 1 << 0,
            Rn = 1 << 1,
            Rm = 1 << 2,

            RnRm   = Rn | Rm,
            RdRn   = Rd | Rn,
            RdRnRm = Rd | Rn | Rm
        }

        private static void EmitVectorBinaryF(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorFOp(Context, Emit, OperFlags.RnRm);
        }

        private static void EmitVectorTernaryF(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorFOp(Context, Emit, OperFlags.RdRnRm);
        }

        private static void EmitVectorBinaryByElemF(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElem Op = (AOpCodeSimdRegElem)Context.CurrOp;

            EmitVectorFOp(Context, Emit, OperFlags.RnRm, Op.Index);
        }

        private static void EmitVectorTernaryByElemF(AILEmitterCtx Context, Action Emit)
        {
            AOpCodeSimdRegElem Op = (AOpCodeSimdRegElem)Context.CurrOp;

            EmitVectorFOp(Context, Emit, OperFlags.RdRnRm, Op.Index);
        }

        private static void EmitVectorFOp(AILEmitterCtx Context, Action Emit, OperFlags Opers, int Elem = -1)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int SizeF = Op.Size & 1;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> SizeF + 2); Index++)
            {
                if (Opers.HasFlag(OperFlags.Rd))
                {
                    EmitVectorExtractF(Context, Op.Rd, Index, SizeF);
                }

                if (Opers.HasFlag(OperFlags.Rn))
                {
                    EmitVectorExtractF(Context, Op.Rn, Index, SizeF);
                }

                if (Opers.HasFlag(OperFlags.Rm))
                {
                    if (Elem != -1)
                    {
                        EmitVectorExtractF(Context, Op.Rm, Elem, SizeF);
                    }
                    else
                    {
                        EmitVectorExtractF(Context, Op.Rm, Index, SizeF);
                    }
                }

                Emit();

                EmitVectorInsertF(Context, Op.Rd, Index, SizeF);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitVectorUnarySx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.Rn, true);
        }

        private static void EmitVectorBinarySx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.RnRm, true);
        }

        private static void EmitVectorUnaryZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.Rn, false);
        }

        private static void EmitVectorBinaryZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.RnRm, false);
        }

        private static void EmitVectorTernaryZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, OperFlags.RdRnRm, false);
        }

        private static void EmitVectorOp(AILEmitterCtx Context, Action Emit, OperFlags Opers, bool Signed)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                if (Opers.HasFlag(OperFlags.Rd))
                {
                    EmitVectorExtract(Context, Op.Rd, Index, Op.Size, Signed);
                }

                if (Opers.HasFlag(OperFlags.Rn))
                {
                    EmitVectorExtract(Context, Op.Rn, Index, Op.Size, Signed);
                }

                if (Opers.HasFlag(OperFlags.Rm))
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

        private static void EmitVectorImmUnary(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorImmOp(Context, Emit, false);
        }

        private static void EmitVectorImmBinary(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorImmOp(Context, Emit, true);
        }

        private static void EmitVectorImmOp(AILEmitterCtx Context, Action Emit, bool Binary)
        {
            AOpCodeSimdImm Op = (AOpCodeSimdImm)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
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

        private static void EmitVectorShImmBinarySx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmBinaryOp(Context, Emit, Imm, true);
        }

        private static void EmitVectorShImmBinaryZx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmBinaryOp(Context, Emit, Imm, false);
        }

        private static void EmitVectorShImmBinaryOp(AILEmitterCtx Context, Action Emit, int Imm, bool Signed)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                EmitVectorExtract(Context, Op.Rn, Index, Op.Size, Signed);

                Context.EmitLdc_I4(Imm);

                Emit();

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitVectorShImmNarrowBinarySx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmNarrowBinaryOp(Context, Emit, Imm, true);
        }

        private static void EmitVectorShImmNarrowBinaryZx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmNarrowBinaryOp(Context, Emit, Imm, false);
        }

        private static void EmitVectorShImmNarrowBinaryOp(AILEmitterCtx Context, Action Emit, int Imm, bool Signed)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Elems = 8 >> Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtract(Context, Op.Rn, Index, Op.Size + 1, Signed);

                Context.EmitLdc_I4(Imm);

                Emit();

                EmitVectorInsert(Context, Op.Rd, Part + Index, Op.Size);
            }

            if (Part == 0)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitVectorShImmWidenBinarySx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmWidenBinaryOp(Context, Emit, Imm, true);
        }

        private static void EmitVectorShImmWidenBinaryZx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmWidenBinaryOp(Context, Emit, Imm, false);
        }

        private static void EmitVectorShImmWidenBinaryOp(AILEmitterCtx Context, Action Emit, int Imm, bool Signed)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Elems = 8 >> Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtract(Context, Op.Rn, Part + Index, Op.Size, Signed);

                Context.EmitLdc_I4(Imm);

                Emit();

                EmitVectorInsertTmp(Context, Index, Op.Size + 1);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);
        }

        private static void EmitVectorWidenBinarySx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenBinary(Context, Emit, true);
        }

        private static void EmitVectorWidenBinaryZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorWidenBinary(Context, Emit, false);
        }

        private static void EmitVectorWidenBinary(AILEmitterCtx Context, Action Emit, bool Signed)
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

        private static void EmitVectorExtractF(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);

            if (Size == 0)
            {
                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorExtractSingle));
            }
            else if (Size == 1)
            {
                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorExtractDouble));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }
        }

        private static void EmitVectorExtractSx(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            EmitVectorExtract(Context, Reg, Index, Size, true);
        }

        private static void EmitVectorExtractZx(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            EmitVectorExtract(Context, Reg, Index, Size, false);
        }

        private static void EmitVectorExtract(AILEmitterCtx Context, int Reg, int Index, int Size, bool Signed)
        {
            if (Size < 0 || Size > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);

            ASoftFallback.EmitCall(Context, Signed
                ? nameof(ASoftFallback.ExtractSVec)
                : nameof(ASoftFallback.ExtractVec));
        }

        private static void EmitVectorZeroLower(AILEmitterCtx Context, int Rd)
        {
            EmitVectorInsert(Context, Rd, 0, 3, 0);
        }

        private static void EmitVectorZeroUpper(AILEmitterCtx Context, int Rd)
        {
            EmitVectorInsert(Context, Rd, 1, 3, 0);
        }

        private static void EmitVectorInsertF(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);

            if (Size == 0)
            {
                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorInsertSingle));
            }
            else if (Size == 1)
            {
                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorInsertDouble));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            Context.EmitStvec(Reg);
        }

        private static void EmitVectorInsertTmp(AILEmitterCtx Context, int Index, int Size)
        {
            if (Size < 0 || Size > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            Context.EmitLdvectmp();
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorInsertInt));

            Context.EmitStvectmp();
        }

        private static void EmitVectorInsert(AILEmitterCtx Context, int Reg, int Index, int Size)
        {
            if (Size < 0 || Size > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.VectorInsertInt));

            Context.EmitStvec(Reg);
        }

        private static void EmitVectorInsert(AILEmitterCtx Context, int Reg, int Index, int Size, long Value)
        {
            if (Size < 0 || Size > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);
            Context.EmitLdc_I8(Value);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.InsertVec));

            Context.EmitStvec(Reg);
        }
    }
}