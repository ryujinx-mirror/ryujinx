using System;

using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        public static void Fadd_C(ShaderIrBlock Block, long OpCode)
        {
            EmitAluBinaryF(Block, OpCode, ShaderOper.CR, ShaderIrInst.Fadd);
        }

        public static void Fadd_I(ShaderIrBlock Block, long OpCode)
        {
            EmitAluBinaryF(Block, OpCode, ShaderOper.Immf, ShaderIrInst.Fadd);
        }

        public static void Fadd_R(ShaderIrBlock Block, long OpCode)
        {
            EmitAluBinaryF(Block, OpCode, ShaderOper.RR, ShaderIrInst.Fadd);
        }

        public static void Ffma_CR(ShaderIrBlock Block, long OpCode)
        {
            EmitAluFfma(Block, OpCode, ShaderOper.CR);
        }

        public static void Ffma_I(ShaderIrBlock Block, long OpCode)
        {
            EmitAluFfma(Block, OpCode, ShaderOper.Immf);
        }

        public static void Ffma_RC(ShaderIrBlock Block, long OpCode)
        {
            EmitAluFfma(Block, OpCode, ShaderOper.RC);
        }

        public static void Ffma_RR(ShaderIrBlock Block, long OpCode)
        {
            EmitAluFfma(Block, OpCode, ShaderOper.RR);
        }

        public static void Fmul32i(ShaderIrBlock Block, long OpCode)
        {
            ShaderIrNode OperA = GetOperGpr8     (OpCode);
            ShaderIrNode OperB = GetOperImmf32_20(OpCode);

            ShaderIrOp Op = new ShaderIrOp(ShaderIrInst.Fmul, OperA, OperB);

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Op), OpCode));
        }

        public static void Fmul_C(ShaderIrBlock Block, long OpCode)
        {
            EmitAluBinaryF(Block, OpCode, ShaderOper.CR, ShaderIrInst.Fmul);
        }

        public static void Fmul_I(ShaderIrBlock Block, long OpCode)
        {
            EmitAluBinaryF(Block, OpCode, ShaderOper.Immf, ShaderIrInst.Fmul);
        }

        public static void Fmul_R(ShaderIrBlock Block, long OpCode)
        {
            EmitAluBinaryF(Block, OpCode, ShaderOper.RR, ShaderIrInst.Fmul);
        }

        public static void Fset_C(ShaderIrBlock Block, long OpCode)
        {
            EmitFset(Block, OpCode, ShaderOper.CR);
        }

        public static void Fset_I(ShaderIrBlock Block, long OpCode)
        {
            EmitFset(Block, OpCode, ShaderOper.Immf);
        }

        public static void Fset_R(ShaderIrBlock Block, long OpCode)
        {
            EmitFset(Block, OpCode, ShaderOper.RR);
        }

        public static void Fsetp_C(ShaderIrBlock Block, long OpCode)
        {
            EmitFsetp(Block, OpCode, ShaderOper.CR);
        }

        public static void Fsetp_I(ShaderIrBlock Block, long OpCode)
        {
            EmitFsetp(Block, OpCode, ShaderOper.Immf);
        }

        public static void Fsetp_R(ShaderIrBlock Block, long OpCode)
        {
            EmitFsetp(Block, OpCode, ShaderOper.RR);
        }

        public static void Ipa(ShaderIrBlock Block, long OpCode)
        {
            ShaderIrNode OperA = GetOperAbuf28(OpCode);
            ShaderIrNode OperB = GetOperGpr20 (OpCode);

            ShaderIrOp Op = new ShaderIrOp(ShaderIrInst.Ipa, OperA, OperB);

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Op), OpCode));
        }

        public static void Isetp_C(ShaderIrBlock Block, long OpCode)
        {
            EmitIsetp(Block, OpCode, ShaderOper.CR);
        }

        public static void Isetp_I(ShaderIrBlock Block, long OpCode)
        {
            EmitIsetp(Block, OpCode, ShaderOper.Imm);
        }

        public static void Isetp_R(ShaderIrBlock Block, long OpCode)
        {
            EmitIsetp(Block, OpCode, ShaderOper.RR);
        }

        public static void Lop32i(ShaderIrBlock Block, long OpCode)
        {
            int SubOp = (int)(OpCode >> 53) & 3;

            bool Ia = ((OpCode >> 55) & 1) != 0;
            bool Ib = ((OpCode >> 56) & 1) != 0;

            ShaderIrInst Inst = 0;

            switch (SubOp)
            {
                case 0: Inst = ShaderIrInst.And; break;
                case 1: Inst = ShaderIrInst.Or;  break;
                case 2: Inst = ShaderIrInst.Xor; break;
            }

            ShaderIrNode OperA = GetAluNot(GetOperGpr8(OpCode), Ia);

            //SubOp == 3 is pass, used by the not instruction
            //which just moves the inverted register value.
            if (SubOp < 3)
            {
                ShaderIrNode OperB = GetAluNot(GetOperImm32_20(OpCode), Ib);

                ShaderIrOp Op = new ShaderIrOp(Inst, OperA, OperB);

                Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Op), OpCode));
            }
            else
            {
                Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), OperA), OpCode));
            }
        }

        public static void Mufu(ShaderIrBlock Block, long OpCode)
        {
            int SubOp = (int)(OpCode >> 20) & 7;

            bool Aa = ((OpCode >> 46) & 1) != 0;
            bool Na = ((OpCode >> 48) & 1) != 0;

            ShaderIrInst Inst = 0;

            switch (SubOp)
            {
                case 0: Inst = ShaderIrInst.Fcos; break;
                case 1: Inst = ShaderIrInst.Fsin; break;
                case 2: Inst = ShaderIrInst.Fex2; break;
                case 3: Inst = ShaderIrInst.Flg2; break;
                case 4: Inst = ShaderIrInst.Frcp; break;
                case 5: Inst = ShaderIrInst.Frsq; break;

                default: throw new NotImplementedException(SubOp.ToString());
            }

            ShaderIrNode OperA = GetOperGpr8(OpCode);

            ShaderIrOp Op = new ShaderIrOp(Inst, GetAluAbsNeg(OperA, Aa, Na));

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Op), OpCode));
        }

        public static void Shr_C(ShaderIrBlock Block, long OpCode)
        {
            EmitAluBinary(Block, OpCode, ShaderOper.CR, GetShrInst(OpCode));
        }

        public static void Shr_I(ShaderIrBlock Block, long OpCode)
        {
            EmitAluBinary(Block, OpCode, ShaderOper.Imm, GetShrInst(OpCode));
        }

        public static void Shr_R(ShaderIrBlock Block, long OpCode)
        {
            EmitAluBinary(Block, OpCode, ShaderOper.RR, GetShrInst(OpCode));
        }

        private static ShaderIrInst GetShrInst(long OpCode)
        {
            bool Signed = ((OpCode >> 48) & 1) != 0;

            return Signed ? ShaderIrInst.Asr : ShaderIrInst.Lsr;
        }

        private static void EmitAluBinary(
            ShaderIrBlock Block,
            long          OpCode,
            ShaderOper    Oper,
            ShaderIrInst  Inst)
        {
            ShaderIrNode OperA = GetOperGpr8(OpCode), OperB;

            switch (Oper)
            {
                case ShaderOper.CR:  OperB = GetOperCbuf34  (OpCode); break;
                case ShaderOper.Imm: OperB = GetOperImm19_20(OpCode); break;
                case ShaderOper.RR:  OperB = GetOperGpr20   (OpCode); break;

                default: throw new ArgumentException(nameof(Oper));
            }

            ShaderIrNode Op = new ShaderIrOp(Inst, OperA, OperB);

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Op), OpCode));
        }

        private static void EmitAluBinaryF(
            ShaderIrBlock Block,
            long          OpCode,
            ShaderOper    Oper,
            ShaderIrInst  Inst)
        {
            bool Nb = ((OpCode >> 45) & 1) != 0;
            bool Aa = ((OpCode >> 46) & 1) != 0;
            bool Na = ((OpCode >> 48) & 1) != 0;
            bool Ab = ((OpCode >> 49) & 1) != 0;

            ShaderIrNode OperA = GetOperGpr8(OpCode), OperB;

            if (Inst == ShaderIrInst.Fadd)
            {
                OperA = GetAluAbsNeg(OperA, Aa, Na);
            }

            switch (Oper)
            {
                case ShaderOper.CR:   OperB = GetOperCbuf34   (OpCode); break;
                case ShaderOper.Immf: OperB = GetOperImmf19_20(OpCode); break;
                case ShaderOper.RR:   OperB = GetOperGpr20    (OpCode); break;

                default: throw new ArgumentException(nameof(Oper));
            }

            OperB = GetAluAbsNeg(OperB, Ab, Nb);

            ShaderIrNode Op = new ShaderIrOp(Inst, OperA, OperB);

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Op), OpCode));
        }

        private static void EmitAluFfma(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            bool Nb = ((OpCode >> 48) & 1) != 0;
            bool Nc = ((OpCode >> 49) & 1) != 0;

            ShaderIrNode OperA = GetOperGpr8(OpCode), OperB, OperC;

            switch (Oper)
            {
                case ShaderOper.CR:   OperB = GetOperCbuf34   (OpCode); break;
                case ShaderOper.Immf: OperB = GetOperImmf19_20(OpCode); break;
                case ShaderOper.RC:   OperB = GetOperGpr39    (OpCode); break;
                case ShaderOper.RR:   OperB = GetOperGpr20    (OpCode); break;

                default: throw new ArgumentException(nameof(Oper));
            }

            OperB = GetAluNeg(OperB, Nb);

            if (Oper == ShaderOper.RC)
            {
                OperC = GetAluNeg(GetOperCbuf34(OpCode), Nc);
            }
            else
            {
                OperC = GetAluNeg(GetOperGpr39(OpCode), Nc);
            }

            ShaderIrOp Op = new ShaderIrOp(ShaderIrInst.Ffma, OperA, OperB, OperC);

            Block.AddNode(GetPredNode(new ShaderIrAsg(GetOperGpr0(OpCode), Op), OpCode));
        }

        private static void EmitFset(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            EmitSet(Block, OpCode, true, Oper);
        }

        private static void EmitIset(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            EmitSet(Block, OpCode, false, Oper);
        }

        private static void EmitSet(ShaderIrBlock Block, long OpCode, bool IsFloat, ShaderOper Oper)
        {
            bool Na = ((OpCode >> 43) & 1) != 0;
            bool Ab = ((OpCode >> 44) & 1) != 0;
            bool Nb = ((OpCode >> 53) & 1) != 0;
            bool Aa = ((OpCode >> 54) & 1) != 0;

            ShaderIrNode OperA = GetOperGpr8(OpCode), OperB;

            switch (Oper)
            {
                case ShaderOper.CR:   OperB = GetOperCbuf34   (OpCode); break;
                case ShaderOper.Imm:  OperB = GetOperImm19_20 (OpCode); break;
                case ShaderOper.Immf: OperB = GetOperImmf19_20(OpCode); break;
                case ShaderOper.RR:   OperB = GetOperGpr20    (OpCode); break;

                default: throw new ArgumentException(nameof(Oper));
            }

            ShaderIrInst CmpInst;

            if (IsFloat)
            {
                OperA = GetAluAbsNeg(OperA, Aa, Na);
                OperB = GetAluAbsNeg(OperB, Ab, Nb);

                CmpInst = GetCmpF(OpCode);
            }
            else
            {
                CmpInst = GetCmp(OpCode);
            }

            ShaderIrOp Op = new ShaderIrOp(CmpInst, OperA, OperB);

            ShaderIrInst LopInst = GetBLop(OpCode);

            ShaderIrOperPred PNode = GetOperPred39(OpCode);

            ShaderIrOperImmf Imm0 = new ShaderIrOperImmf(0);
            ShaderIrOperImmf Imm1 = new ShaderIrOperImmf(1);

            ShaderIrNode Asg0 = new ShaderIrAsg(GetOperGpr0(OpCode), Imm0);
            ShaderIrNode Asg1 = new ShaderIrAsg(GetOperGpr0(OpCode), Imm1);

            if (LopInst != ShaderIrInst.Band || !PNode.IsConst)
            {
                ShaderIrOp Op2 = new ShaderIrOp(LopInst, Op, PNode);

                Asg0 = new ShaderIrCond(Op2, Asg0, Not: true);
                Asg1 = new ShaderIrCond(Op2, Asg1, Not: false);
            }
            else
            {
                Asg0 = new ShaderIrCond(Op, Asg0, Not: true);
                Asg1 = new ShaderIrCond(Op, Asg1, Not: false);
            }

            Block.AddNode(GetPredNode(Asg0, OpCode));
            Block.AddNode(GetPredNode(Asg1, OpCode));
        }

        private static void EmitFsetp(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            EmitSetp(Block, OpCode, true, Oper);
        }

        private static void EmitIsetp(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            EmitSetp(Block, OpCode, false, Oper);
        }

        private static void EmitSetp(ShaderIrBlock Block, long OpCode, bool IsFloat, ShaderOper Oper)
        {
            bool Aa = ((OpCode >>  7) & 1) != 0;
            bool Np = ((OpCode >> 42) & 1) != 0;
            bool Na = ((OpCode >> 43) & 1) != 0;
            bool Ab = ((OpCode >> 44) & 1) != 0;

            ShaderIrNode OperA = GetOperGpr8(OpCode), OperB;

            switch (Oper)
            {
                case ShaderOper.CR:   OperB = GetOperCbuf34   (OpCode); break;
                case ShaderOper.Imm:  OperB = GetOperImm19_20 (OpCode); break;
                case ShaderOper.Immf: OperB = GetOperImmf19_20(OpCode); break;
                case ShaderOper.RR:   OperB = GetOperGpr20    (OpCode); break;

                default: throw new ArgumentException(nameof(Oper));
            }

            ShaderIrInst CmpInst;

            if (IsFloat)
            {
                OperA = GetAluAbsNeg(OperA, Aa, Na);
                OperB = GetAluAbs   (OperB, Ab);

                CmpInst = GetCmpF(OpCode);
            }
            else
            {
                CmpInst = GetCmp(OpCode);
            }

            ShaderIrOp Op = new ShaderIrOp(CmpInst, OperA, OperB);

            ShaderIrOperPred P0Node = GetOperPred3 (OpCode);
            ShaderIrOperPred P1Node = GetOperPred0 (OpCode);
            ShaderIrOperPred P2Node = GetOperPred39(OpCode);

            Block.AddNode(GetPredNode(new ShaderIrAsg(P0Node, Op), OpCode));

            ShaderIrInst LopInst = GetBLop(OpCode);

            if (LopInst == ShaderIrInst.Band && P1Node.IsConst && P2Node.IsConst)
            {
                return;
            }

            ShaderIrNode P2NNode = P2Node;

            if (Np)
            {
                P2NNode = new ShaderIrOp(ShaderIrInst.Bnot, P2NNode);
            }

            Op = new ShaderIrOp(ShaderIrInst.Bnot, P0Node);

            Op = new ShaderIrOp(LopInst, Op, P2NNode);

            Block.AddNode(GetPredNode(new ShaderIrAsg(P1Node, Op), OpCode));

            Op = new ShaderIrOp(LopInst, P0Node, P2NNode);

            Block.AddNode(GetPredNode(new ShaderIrAsg(P0Node, Op), OpCode));
        }
    }
}