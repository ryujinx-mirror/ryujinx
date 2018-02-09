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
        public static void Add_V(AILEmitterCtx Context) => EmitVectorBinaryZx(Context, OpCodes.Add);

        public static void Addp_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Addp64),
                nameof(ASoftFallback.Addp128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Addv_V(AILEmitterCtx Context) => EmitVectorAddv(Context);

        public static void And_V(AILEmitterCtx Context) => EmitVectorBinaryZx(Context, OpCodes.And);

        public static void Bic_V(AILEmitterCtx Context) => EmitVectorBic(Context);
        public static void Bic_Vi(AILEmitterCtx Context)
        {
            AOpCodeSimdImm Op = (AOpCodeSimdImm)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdc_I8(Op.Imm);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Bic_Vi64),
                nameof(ASoftFallback.Bic_Vi128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Bsl_V(AILEmitterCtx Context) => EmitVectorBsl(Context);

        public static void Cmeq_V(AILEmitterCtx Context) => EmitVectorCmp(Context, OpCodes.Beq_S);
        public static void Cmge_V(AILEmitterCtx Context) => EmitVectorCmp(Context, OpCodes.Bge_S);
        public static void Cmgt_V(AILEmitterCtx Context) => EmitVectorCmp(Context, OpCodes.Bgt_S);
        public static void Cmhi_V(AILEmitterCtx Context) => EmitVectorCmp(Context, OpCodes.Bgt_Un_S);
        public static void Cmhs_V(AILEmitterCtx Context) => EmitVectorCmp(Context, OpCodes.Bge_Un_S);
        public static void Cmle_V(AILEmitterCtx Context) => EmitVectorCmp(Context, OpCodes.Ble_S);
        public static void Cmlt_V(AILEmitterCtx Context) => EmitVectorCmp(Context, OpCodes.Blt_S);

        public static void Cnt_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Cnt64),
                nameof(ASoftFallback.Cnt128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Dup_Gp(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Dup_Gp64),
                nameof(ASoftFallback.Dup_Gp128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Dup_V(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4(Op.DstIndex);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Dup_V64),
                nameof(ASoftFallback.Dup_V128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Eor_V(AILEmitterCtx Context) => EmitVectorBinaryZx(Context, OpCodes.Xor);

        public static void Fadd_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);
            Context.EmitLdc_I4(Op.SizeF);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Fadd64),
                nameof(ASoftFallback.Fadd128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Fcvtzs_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4(0);
            Context.EmitLdc_I4(Op.SizeF);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Fcvtzs_V64),
                nameof(ASoftFallback.Fcvtzs_V128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Fcvtzs_V_Fix(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4((8 << (Op.Size + 1)) - Op.Imm);
            Context.EmitLdc_I4(Op.Size - 2);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Fcvtzs_V64),
                nameof(ASoftFallback.Fcvtzs_V128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Fcvtzu_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4(0);
            Context.EmitLdc_I4(Op.SizeF);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Fcvtzu_V_64),
                nameof(ASoftFallback.Fcvtzu_V_128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Fcvtzu_V_Fix(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4((8 << (Op.Size + 1)) - Op.Imm);
            Context.EmitLdc_I4(Op.Size - 2);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Fcvtzu_V_64),
                nameof(ASoftFallback.Fcvtzu_V_128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Fmla_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);
            Context.EmitLdc_I4(Op.SizeF);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Fmla64),
                nameof(ASoftFallback.Fmla128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Fmla_Vs(AILEmitterCtx Context)
        {
            AOpCodeSimdRegElem Op = (AOpCodeSimdRegElem)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);
            Context.EmitLdc_I4(Op.Index);
            Context.EmitLdc_I4(Op.SizeF);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Fmla_Ve64),
                nameof(ASoftFallback.Fmla_Ve128));

            Context.EmitStvec(Op.Rd);
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
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);
            Context.EmitLdc_I4(Op.SizeF);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Fmul64),
                nameof(ASoftFallback.Fmul128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Fmul_Vs(AILEmitterCtx Context)
        {
            AOpCodeSimdRegElem Op = (AOpCodeSimdRegElem)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);
            Context.EmitLdc_I4(Op.Index);
            Context.EmitLdc_I4(Op.SizeF);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Fmul_Ve64),
                nameof(ASoftFallback.Fmul_Ve128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Fsub_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);
            Context.EmitLdc_I4(Op.SizeF);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Fsub64),
                nameof(ASoftFallback.Fsub128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Ins_Gp(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdintzr(Op.Rn);
            Context.EmitLdc_I4(Op.DstIndex);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Ins_Gp));

            Context.EmitStvec(Op.Rd);
        }

        public static void Ins_V(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4(Op.SrcIndex);
            Context.EmitLdc_I4(Op.DstIndex);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Ins_V));

            Context.EmitStvec(Op.Rd);
        }

        public static void Ld__Vms(AILEmitterCtx Context) => EmitSimdMemMs(Context, IsLoad: true);
        public static void Ld__Vss(AILEmitterCtx Context) => EmitSimdMemSs(Context, IsLoad: true);

        public static void Mla_V(AILEmitterCtx Context) => EmitVectorMla(Context);

        public static void Movi_V(AILEmitterCtx Context) => EmitMovi_V(Context, false);

        public static void Mul_V(AILEmitterCtx Context) => EmitVectorBinaryZx(Context, OpCodes.Mul);

        public static void Mvni_V(AILEmitterCtx Context) => EmitMovi_V(Context, true);

        private static void EmitMovi_V(AILEmitterCtx Context, bool Not)
        {
            AOpCodeSimdImm Op = (AOpCodeSimdImm)Context.CurrOp;

            Context.EmitLdc_I8(Not ? ~Op.Imm : Op.Imm);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Dup_Gp64),
                nameof(ASoftFallback.Dup_Gp128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Neg_V(AILEmitterCtx Context) => EmitVectorUnarySx(Context, OpCodes.Neg);

        public static void Not_V(AILEmitterCtx Context) => EmitVectorUnaryZx(Context, OpCodes.Not);

        public static void Orr_V(AILEmitterCtx Context) => EmitVectorBinaryZx(Context, OpCodes.Or);

        public static void Orr_Vi(AILEmitterCtx Context)
        {
            AOpCodeSimdImm Op = (AOpCodeSimdImm)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdc_I8(Op.Imm);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Orr_Vi64),
                nameof(ASoftFallback.Orr_Vi128));

            Context.EmitStvec(Op.Rd);
        }       

        public static void Saddw_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Saddw),
                nameof(ASoftFallback.Saddw2));

            Context.EmitStvec(Op.Rd);
        }

        public static void Scvtf_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4(Op.SizeF);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Scvtf_V64),
                nameof(ASoftFallback.Scvtf_V128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Shl_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            EmitVectorImmBinaryZx(Context, OpCodes.Shl, Op.Imm - (8 << Op.Size));
        }

        public static void Smax_V(AILEmitterCtx Context) => EmitVectorSmax(Context);
        public static void Smin_V(AILEmitterCtx Context) => EmitVectorSmin(Context);

        public static void Sshl_V(AILEmitterCtx Context) => EmitVectorSshl(Context);

        public static void Sshll_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4(Op.Imm - (8 << Op.Size));
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Sshll),
                nameof(ASoftFallback.Sshll2));

            Context.EmitStvec(Op.Rd);
        }

        public static void Sshr_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            EmitVectorImmBinarySx(Context, OpCodes.Shr, (8 << (Op.Size + 1)) - Op.Imm);
        }

        public static void St__V(AILEmitterCtx Context) => EmitSimdMemMs(Context, IsLoad: false);

        public static void Sub_V(AILEmitterCtx Context) => EmitVectorBinaryZx(Context, OpCodes.Sub);

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

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Uaddlv64),
                nameof(ASoftFallback.Uaddlv128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Uaddw_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Uaddw),
                nameof(ASoftFallback.Uaddw2));

            Context.EmitStvec(Op.Rd);
        }

        public static void Ucvtf_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);

            if (Op.Size == 0)
            {
                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Ucvtf_V_F));
            }
            else if (Op.Size == 1)
            {
                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.Ucvtf_V_D));
            }
            else
            {
                throw new InvalidOperationException();
            }

            Context.EmitStvec(Op.Rd);
        }

        public static void Umov_S(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4(Op.DstIndex);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.ExtractVec));

            Context.EmitStintzr(Op.Rd);
        }

        public static void Ushl_V(AILEmitterCtx Context) => EmitVectorUshl(Context);

        public static void Ushll_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4(Op.Imm - (8 << Op.Size));
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Ushll),
                nameof(ASoftFallback.Ushll2));

            Context.EmitStvec(Op.Rd);
        }

        public static void Ushr_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4((8 << (Op.Size + 1)) - Op.Imm);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Ushr64),
                nameof(ASoftFallback.Ushr128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Usra_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4((8 << (Op.Size + 1)) - Op.Imm);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Usra64),
                nameof(ASoftFallback.Usra128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Uzp1_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdvec(Op.Rm);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Uzp1_V64),
                nameof(ASoftFallback.Uzp1_V128));

            Context.EmitStvec(Op.Rd);
        }

        public static void Xtn_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Op.Rn);
            Context.EmitLdc_I4(Op.Size);

            ASoftFallback.EmitCall(Context,
                nameof(ASoftFallback.Xtn),
                nameof(ASoftFallback.Xtn2));

            Context.EmitStvec(Op.Rd);
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
                    Context.EmitLdvec(Rtt);
                    Context.EmitLdc_I4(Elem);
                    Context.EmitLdc_I4(Op.Size);
                    Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
                    Context.EmitLdint(Op.Rn);
                    Context.EmitLdc_I8(Offset);

                    Context.Emit(OpCodes.Add);

                    EmitReadZxCall(Context, Op.Size);

                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.InsertVec));

                    Context.EmitStvec(Rtt);

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

                    Context.EmitLdvec(Rtt);
                    Context.EmitLdc_I4(Elem);
                    Context.EmitLdc_I4(Op.Size);

                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.ExtractVec));

                    EmitWriteCall(Context, Op.Size);
                }

                Offset += 1 << Op.Size;
            }

            if (Op.WBack)
            {
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
                    Context.EmitLdvec(Rt);
                    Context.EmitLdc_I4(Op.Index);
                    Context.EmitLdc_I4(Op.Size);
                    Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
                    Context.EmitLdint(Op.Rn);
                    Context.EmitLdc_I8(Offset);

                    Context.Emit(OpCodes.Add);

                    EmitReadZxCall(Context, Op.Size);

                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.InsertVec));

                    Context.EmitStvec(Rt);

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

                    Context.EmitLdvec(Rt);
                    Context.EmitLdc_I4(Op.Index);
                    Context.EmitLdc_I4(Op.Size);

                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.ExtractVec));

                    EmitWriteCall(Context, Op.Size);
                }

                Offset += 1 << Op.Size;
            }

            if (Op.WBack)
            {
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
        }

        private static void EmitVectorAddv(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            EmitVectorZeroLower(Context, Op.Rd);
            EmitVectorZeroUpper(Context, Op.Rd);

            Context.EmitLdvec(Op.Rd);
            Context.EmitLdc_I4(0);
            Context.EmitLdc_I4(Op.Size);

            EmitVectorExtractZx(Context, Op.Rn, 0);

            for (int Index = 1; Index < (Bytes >> Op.Size); Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index);

                Context.Emit(OpCodes.Add);
            }

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.InsertVec));

            Context.EmitStvec(Op.Rd);
        }

        private static void EmitVectorBic(AILEmitterCtx Context)
        {
            EmitVectorBinaryZx(Context, () =>
            {
                Context.Emit(OpCodes.Not);
                Context.Emit(OpCodes.And);
            });
        }

        private static void EmitVectorBsl(AILEmitterCtx Context)
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

        private static void EmitVectorMla(AILEmitterCtx Context)
        {
            EmitVectorTernaryZx(Context, () =>
            {
                Context.Emit(OpCodes.Mul);
                Context.Emit(OpCodes.Add);
            });
        }

        private static void EmitVectorSmax(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(long), typeof(long) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Max), Types);

            EmitVectorBinarySx(Context, () => Context.EmitCall(MthdInfo));
        }

        private static void EmitVectorSmin(AILEmitterCtx Context)
        {
            Type[] Types = new Type[] { typeof(long), typeof(long) };

            MethodInfo MthdInfo = typeof(Math).GetMethod(nameof(Math.Min), Types);

            EmitVectorBinarySx(Context, () => Context.EmitCall(MthdInfo));
        }

        private static void EmitVectorSshl(AILEmitterCtx Context) => EmitVectorShl(Context, true);
        private static void EmitVectorUshl(AILEmitterCtx Context) => EmitVectorShl(Context, false);

        private static void EmitVectorShl(AILEmitterCtx Context, bool Signed)
        {
            //This instruction shifts the value on vector A by the number of bits
            //specified on the signed, lower 8 bits of vector B. If the shift value
            //is greater or equal to the data size of each lane, then the result is zero.
            //Additionally, negative shifts produces right shifts by the negated shift value.
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int MaxShift = 8 << Op.Size;

            EmitVectorBinaryZx(Context, () =>
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
            });
        }

        private static void EmitVectorUnarySx(AILEmitterCtx Context, OpCode ILOp)
        {
            EmitVectorUnarySx(Context, () => Context.Emit(ILOp));
        }

        private static void EmitVectorUnaryZx(AILEmitterCtx Context, OpCode ILOp)
        {
            EmitVectorUnaryZx(Context, () => Context.Emit(ILOp));
        }

        private static void EmitVectorBinaryZx(AILEmitterCtx Context, OpCode ILOp)
        {
            EmitVectorBinaryZx(Context, () => Context.Emit(ILOp));
        }

        private static void EmitVectorUnarySx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, 1, true);
        }

        private static void EmitVectorBinarySx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, 2, true);
        }

        private static void EmitVectorUnaryZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, 1, false);
        }

        private static void EmitVectorBinaryZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, 2, false);
        }

        private static void EmitVectorTernaryZx(AILEmitterCtx Context, Action Emit)
        {
            EmitVectorOp(Context, Emit, 3, false);
        }

        private static void EmitVectorOp(AILEmitterCtx Context, Action Emit, int Opers, bool Signed)
        {
            if (Opers < 1 || Opers > 3)
            {
                throw new ArgumentOutOfRangeException(nameof(Opers));
            }

            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                Context.EmitLdvec(Op.Rd);
                Context.EmitLdc_I4(Index);
                Context.EmitLdc_I4(Op.Size);

                if (Opers == 3)
                {
                    EmitVectorExtract(Context, Op.Rd, Index, Signed);
                }

                if (Opers >= 1)
                {
                    EmitVectorExtract(Context, Op.Rn, Index, Signed);
                }

                if (Opers >= 2)
                {
                    EmitVectorExtract(Context, ((AOpCodeSimdReg)Op).Rm, Index, Signed);
                }

                Emit();

                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.InsertVec));

                Context.EmitStvec(Op.Rd);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitVectorImmBinarySx(AILEmitterCtx Context, OpCode ILOp, long Imm)
        {
            EmitVectorImmBinarySx(Context, () => Context.Emit(ILOp), Imm);
        }

        private static void EmitVectorImmBinaryZx(AILEmitterCtx Context, OpCode ILOp, long Imm)
        {
            EmitVectorImmBinaryZx(Context, () => Context.Emit(ILOp), Imm);
        }

        private static void EmitVectorImmBinarySx(AILEmitterCtx Context, Action Emit, long Imm)
        {
            EmitVectorImmBinaryOp(Context, Emit, Imm, true);
        }

        private static void EmitVectorImmBinaryZx(AILEmitterCtx Context, Action Emit, long Imm)
        {
            EmitVectorImmBinaryOp(Context, Emit, Imm, false);
        }

        private static void EmitVectorImmBinaryOp(AILEmitterCtx Context, Action Emit, long Imm, bool Signed)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                Context.EmitLdvec(Op.Rd);
                Context.EmitLdc_I4(Index);
                Context.EmitLdc_I4(Op.Size);

                EmitVectorExtract(Context, Op.Rn, Index, Signed);

                Context.EmitLdc_I8(Imm);

                Emit();

                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.InsertVec));

                Context.EmitStvec(Op.Rd);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitVectorCmp(AILEmitterCtx Context, OpCode ILOp)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            ulong SzMask = ulong.MaxValue >> (64 - (8 << Op.Size));

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                EmitVectorExtractSx(Context, Op.Rn, Index);

                if (Op is AOpCodeSimdReg BinOp)
                {
                    EmitVectorExtractSx(Context, BinOp.Rm, Index);
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

        private static void EmitVectorExtractSx(AILEmitterCtx Context, int Reg, int Index)
        {
            EmitVectorExtract(Context, Reg, Index, true);
        }

        private static void EmitVectorExtractZx(AILEmitterCtx Context, int Reg, int Index)
        {
            EmitVectorExtract(Context, Reg, Index, false);
        }

        private static void EmitVectorExtract(AILEmitterCtx Context, int Reg, int Index, bool Signed)
        {
            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Op.Size);

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

        private static void EmitVectorInsert(AILEmitterCtx Context, int Reg, int Index, int Size, long Value)
        {
            Context.EmitLdvec(Reg);
            Context.EmitLdc_I4(Index);
            Context.EmitLdc_I4(Size);
            Context.EmitLdc_I8(Value);

            ASoftFallback.EmitCall(Context, nameof(ASoftFallback.InsertVec));

            Context.EmitStvec(Reg);
        }
    }
}