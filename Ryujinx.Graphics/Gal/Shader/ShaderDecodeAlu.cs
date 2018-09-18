using System;

using static Ryujinx.Graphics.Gal.Shader.ShaderDecodeHelper;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        public static void Bfe_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitBfe(Block, OpCode, ShaderOper.CR);
        }

        public static void Bfe_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitBfe(Block, OpCode, ShaderOper.Imm);
        }

        public static void Bfe_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitBfe(Block, OpCode, ShaderOper.RR);
        }

        public static void Fadd_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFadd(Block, OpCode, ShaderOper.CR);
        }

        public static void Fadd_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFadd(Block, OpCode, ShaderOper.Immf);
        }

        public static void Fadd_I32(ShaderIrBlock Block, long OpCode, int Position)
        {
            ShaderIrNode OperA = OpCode.Gpr8();
            ShaderIrNode OperB = OpCode.Immf32_20();

            bool NegB = OpCode.Read(53);
            bool AbsA = OpCode.Read(54);
            bool NegA = OpCode.Read(56);
            bool AbsB = OpCode.Read(57);

            OperA = GetAluFabsFneg(OperA, AbsA, NegA);
            OperB = GetAluFabsFneg(OperB, AbsB, NegB);

            ShaderIrOp Op = new ShaderIrOp(ShaderIrInst.Fadd, OperA, OperB);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
        }

        public static void Fadd_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFadd(Block, OpCode, ShaderOper.RR);
        }

        public static void Ffma_CR(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFfma(Block, OpCode, ShaderOper.CR);
        }

        public static void Ffma_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFfma(Block, OpCode, ShaderOper.Immf);
        }

        public static void Ffma_RC(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFfma(Block, OpCode, ShaderOper.RC);
        }

        public static void Ffma_RR(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFfma(Block, OpCode, ShaderOper.RR);
        }

        public static void Fmnmx_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFmnmx(Block, OpCode, ShaderOper.CR);
        }

        public static void Fmnmx_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFmnmx(Block, OpCode, ShaderOper.Immf);
        }

        public static void Fmnmx_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFmnmx(Block, OpCode, ShaderOper.RR);
        }

        public static void Fmul_I32(ShaderIrBlock Block, long OpCode, int Position)
        {
            ShaderIrNode OperA = OpCode.Gpr8();
            ShaderIrNode OperB = OpCode.Immf32_20();

            ShaderIrOp Op = new ShaderIrOp(ShaderIrInst.Fmul, OperA, OperB);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
        }

        public static void Fmul_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFmul(Block, OpCode, ShaderOper.CR);
        }

        public static void Fmul_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFmul(Block, OpCode, ShaderOper.Immf);
        }

        public static void Fmul_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFmul(Block, OpCode, ShaderOper.RR);
        }

        public static void Fset_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFset(Block, OpCode, ShaderOper.CR);
        }

        public static void Fset_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFset(Block, OpCode, ShaderOper.Immf);
        }

        public static void Fset_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFset(Block, OpCode, ShaderOper.RR);
        }

        public static void Fsetp_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFsetp(Block, OpCode, ShaderOper.CR);
        }

        public static void Fsetp_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFsetp(Block, OpCode, ShaderOper.Immf);
        }

        public static void Fsetp_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitFsetp(Block, OpCode, ShaderOper.RR);
        }

        public static void Iadd_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIadd(Block, OpCode, ShaderOper.CR);
        }

        public static void Iadd_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIadd(Block, OpCode, ShaderOper.Imm);
        }

        public static void Iadd_I32(ShaderIrBlock Block, long OpCode, int Position)
        {
            ShaderIrNode OperA = OpCode.Gpr8();
            ShaderIrNode OperB = OpCode.Imm32_20();

            bool NegA = OpCode.Read(56);

            OperA = GetAluIneg(OperA, NegA);

            ShaderIrOp Op = new ShaderIrOp(ShaderIrInst.Add, OperA, OperB);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
        }

        public static void Iadd_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIadd(Block, OpCode, ShaderOper.RR);
        }

        public static void Iadd3_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIadd3(Block, OpCode, ShaderOper.CR);
        }

        public static void Iadd3_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIadd3(Block, OpCode, ShaderOper.Imm);
        }

        public static void Iadd3_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIadd3(Block, OpCode, ShaderOper.RR);
        }

        public static void Imnmx_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitImnmx(Block, OpCode, ShaderOper.CR);
        }

        public static void Imnmx_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitImnmx(Block, OpCode, ShaderOper.Imm);
        }

        public static void Imnmx_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitImnmx(Block, OpCode, ShaderOper.RR);
        }

        public static void Ipa(ShaderIrBlock Block, long OpCode, int Position)
        {
            ShaderIrNode OperA = OpCode.Abuf28();
            ShaderIrNode OperB = OpCode.Gpr20();

            ShaderIpaMode Mode = (ShaderIpaMode)(OpCode.Read(54, 3));

            ShaderIrMetaIpa Meta = new ShaderIrMetaIpa(Mode);

            ShaderIrOp Op = new ShaderIrOp(ShaderIrInst.Ipa, OperA, OperB, null, Meta);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
        }

        public static void Iscadd_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIscadd(Block, OpCode, ShaderOper.CR);
        }

        public static void Iscadd_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIscadd(Block, OpCode, ShaderOper.Imm);
        }

        public static void Iscadd_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIscadd(Block, OpCode, ShaderOper.RR);
        }

        public static void Iset_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIset(Block, OpCode, ShaderOper.CR);
        }

        public static void Iset_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIset(Block, OpCode, ShaderOper.Imm);
        }

        public static void Iset_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIset(Block, OpCode, ShaderOper.RR);
        }

        public static void Isetp_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIsetp(Block, OpCode, ShaderOper.CR);
        }

        public static void Isetp_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIsetp(Block, OpCode, ShaderOper.Imm);
        }

        public static void Isetp_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitIsetp(Block, OpCode, ShaderOper.RR);
        }

        public static void Lop_I32(ShaderIrBlock Block, long OpCode, int Position)
        {
            int SubOp = OpCode.Read(53, 3);

            bool InvA = OpCode.Read(55);
            bool InvB = OpCode.Read(56);

            ShaderIrInst Inst = 0;

            switch (SubOp)
            {
                case 0: Inst = ShaderIrInst.And; break;
                case 1: Inst = ShaderIrInst.Or;  break;
                case 2: Inst = ShaderIrInst.Xor; break;
            }

            ShaderIrNode OperB = GetAluNot(OpCode.Imm32_20(), InvB);

            //SubOp == 3 is pass, used by the not instruction
            //which just moves the inverted register value.
            if (SubOp < 3)
            {
                ShaderIrNode OperA = GetAluNot(OpCode.Gpr8(), InvA);

                ShaderIrOp Op = new ShaderIrOp(Inst, OperA, OperB);

                Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
            }
            else
            {
                Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), OperB)));
            }
        }

        public static void Lop_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitLop(Block, OpCode, ShaderOper.CR);
        }

        public static void Lop_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitLop(Block, OpCode, ShaderOper.Imm);
        }

        public static void Lop_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitLop(Block, OpCode, ShaderOper.RR);
        }

        public static void Mufu(ShaderIrBlock Block, long OpCode, int Position)
        {
            int SubOp = OpCode.Read(20, 0xf);

            bool AbsA = OpCode.Read(46);
            bool NegA = OpCode.Read(48);

            ShaderIrInst Inst = 0;

            switch (SubOp)
            {
                case 0: Inst = ShaderIrInst.Fcos;  break;
                case 1: Inst = ShaderIrInst.Fsin;  break;
                case 2: Inst = ShaderIrInst.Fex2;  break;
                case 3: Inst = ShaderIrInst.Flg2;  break;
                case 4: Inst = ShaderIrInst.Frcp;  break;
                case 5: Inst = ShaderIrInst.Frsq;  break;
                case 8: Inst = ShaderIrInst.Fsqrt; break;

                default: throw new NotImplementedException(SubOp.ToString());
            }

            ShaderIrNode OperA = OpCode.Gpr8();

            ShaderIrOp Op = new ShaderIrOp(Inst, GetAluFabsFneg(OperA, AbsA, NegA));

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
        }

        public static void Psetp(ShaderIrBlock Block, long OpCode, int Position)
        {
            bool NegA = OpCode.Read(15);
            bool NegB = OpCode.Read(32);
            bool NegP = OpCode.Read(42);

            ShaderIrInst LopInst = OpCode.BLop24();

            ShaderIrNode OperA = OpCode.Pred12();
            ShaderIrNode OperB = OpCode.Pred29();

            if (NegA)
            {
                OperA = new ShaderIrOp(ShaderIrInst.Bnot, OperA);
            }

            if (NegB)
            {
                OperB = new ShaderIrOp(ShaderIrInst.Bnot, OperB);
            }

            ShaderIrOp Op = new ShaderIrOp(LopInst, OperA, OperB);

            ShaderIrOperPred P0Node = OpCode.Pred3();
            ShaderIrOperPred P1Node = OpCode.Pred0();
            ShaderIrOperPred P2Node = OpCode.Pred39();

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(P0Node, Op)));

            LopInst = OpCode.BLop45();

            if (LopInst == ShaderIrInst.Band && P1Node.IsConst && P2Node.IsConst)
            {
                return;
            }

            ShaderIrNode P2NNode = P2Node;

            if (NegP)
            {
                P2NNode = new ShaderIrOp(ShaderIrInst.Bnot, P2NNode);
            }

            Op = new ShaderIrOp(ShaderIrInst.Bnot, P0Node);

            Op = new ShaderIrOp(LopInst, Op, P2NNode);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(P1Node, Op)));

            Op = new ShaderIrOp(LopInst, P0Node, P2NNode);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(P0Node, Op)));
        }

        public static void Rro_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitRro(Block, OpCode, ShaderOper.CR);
        }

        public static void Rro_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitRro(Block, OpCode, ShaderOper.Immf);
        }

        public static void Rro_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitRro(Block, OpCode, ShaderOper.RR);
        }

        public static void Shl_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitAluBinary(Block, OpCode, ShaderOper.CR, ShaderIrInst.Lsl);
        }

        public static void Shl_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitAluBinary(Block, OpCode, ShaderOper.Imm, ShaderIrInst.Lsl);
        }

        public static void Shl_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitAluBinary(Block, OpCode, ShaderOper.RR, ShaderIrInst.Lsl);
        }

        public static void Shr_C(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitAluBinary(Block, OpCode, ShaderOper.CR, GetShrInst(OpCode));
        }

        public static void Shr_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitAluBinary(Block, OpCode, ShaderOper.Imm, GetShrInst(OpCode));
        }

        public static void Shr_R(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitAluBinary(Block, OpCode, ShaderOper.RR, GetShrInst(OpCode));
        }

        private static ShaderIrInst GetShrInst(long OpCode)
        {
            bool Signed = OpCode.Read(48);

            return Signed ? ShaderIrInst.Asr : ShaderIrInst.Lsr;
        }

        public static void Vmad(ShaderIrBlock Block, long OpCode, int Position)
        {
            ShaderIrNode OperA = OpCode.Gpr8();

            ShaderIrNode OperB;

            if (OpCode.Read(50))
            {
                OperB = OpCode.Gpr20();
            }
            else
            {
                OperB = OpCode.Imm19_20();
            }

            ShaderIrOperGpr OperC = OpCode.Gpr39();

            ShaderIrNode Tmp = new ShaderIrOp(ShaderIrInst.Mul, OperA, OperB);

            ShaderIrNode Final = new ShaderIrOp(ShaderIrInst.Add, Tmp, OperC);

            int Shr = OpCode.Read(51, 3);

            if (Shr != 0)
            {
                int Shift = (Shr == 2) ? 15 : 7;

                Final = new ShaderIrOp(ShaderIrInst.Lsr, Final, new ShaderIrOperImm(Shift));
            }

            Block.AddNode(new ShaderIrCmnt("Stubbed. Instruction is reduced to a * b + c"));

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Final)));
        }

        public static void Xmad_CR(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitXmad(Block, OpCode, ShaderOper.CR);
        }

        public static void Xmad_I(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitXmad(Block, OpCode, ShaderOper.Imm);
        }

        public static void Xmad_RC(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitXmad(Block, OpCode, ShaderOper.RC);
        }

        public static void Xmad_RR(ShaderIrBlock Block, long OpCode, int Position)
        {
            EmitXmad(Block, OpCode, ShaderOper.RR);
        }

        private static void EmitAluBinary(
            ShaderIrBlock Block,
            long          OpCode,
            ShaderOper    Oper,
            ShaderIrInst  Inst)
        {
            ShaderIrNode OperA = OpCode.Gpr8(), OperB;

            switch (Oper)
            {
                case ShaderOper.CR:  OperB = OpCode.Cbuf34();   break;
                case ShaderOper.Imm: OperB = OpCode.Imm19_20(); break;
                case ShaderOper.RR:  OperB = OpCode.Gpr20();    break;

                default: throw new ArgumentException(nameof(Oper));
            }

            ShaderIrNode Op = new ShaderIrOp(Inst, OperA, OperB);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
        }

        private static void EmitBfe(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            //TODO: Handle the case where position + length
            //is greater than the word size, in this case the sign bit
            //needs to be replicated to fill the remaining space.
            bool NegB = OpCode.Read(48);
            bool NegA = OpCode.Read(49);

            ShaderIrNode OperA = OpCode.Gpr8(), OperB;

            switch (Oper)
            {
                case ShaderOper.CR:  OperB = OpCode.Cbuf34();   break;
                case ShaderOper.Imm: OperB = OpCode.Imm19_20(); break;
                case ShaderOper.RR:  OperB = OpCode.Gpr20();    break;

                default: throw new ArgumentException(nameof(Oper));
            }

            ShaderIrNode Op;

            bool Signed = OpCode.Read(48); //?

            if (OperB is ShaderIrOperImm PosLen)
            {
                int Position = (PosLen.Value >> 0) & 0xff;
                int Length   = (PosLen.Value >> 8) & 0xff;

                int LSh = 32 - (Position + Length);

                ShaderIrInst RightShift = Signed
                    ? ShaderIrInst.Asr
                    : ShaderIrInst.Lsr;

                Op = new ShaderIrOp(ShaderIrInst.Lsl, OperA, new ShaderIrOperImm(LSh));
                Op = new ShaderIrOp(RightShift,       Op,    new ShaderIrOperImm(LSh + Position));
            }
            else
            {
                ShaderIrOperImm Shift = new ShaderIrOperImm(8);
                ShaderIrOperImm Mask  = new ShaderIrOperImm(0xff);

                ShaderIrNode OpPos, OpLen;

                OpPos = new ShaderIrOp(ShaderIrInst.And, OperB, Mask);
                OpLen = new ShaderIrOp(ShaderIrInst.Lsr, OperB, Shift);
                OpLen = new ShaderIrOp(ShaderIrInst.And, OpLen, Mask);

                Op = new ShaderIrOp(ShaderIrInst.Lsr, OperA, OpPos);

                Op = ExtendTo32(Op, Signed, OpLen);
            }

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
        }

        private static void EmitFadd(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            bool NegB = OpCode.Read(45);
            bool AbsA = OpCode.Read(46);
            bool NegA = OpCode.Read(48);
            bool AbsB = OpCode.Read(49);

            ShaderIrNode OperA = OpCode.Gpr8(), OperB;

            OperA = GetAluFabsFneg(OperA, AbsA, NegA);

            switch (Oper)
            {
                case ShaderOper.CR:   OperB = OpCode.Cbuf34();    break;
                case ShaderOper.Immf: OperB = OpCode.Immf19_20(); break;
                case ShaderOper.RR:   OperB = OpCode.Gpr20();     break;

                default: throw new ArgumentException(nameof(Oper));
            }

            OperB = GetAluFabsFneg(OperB, AbsB, NegB);

            ShaderIrNode Op = new ShaderIrOp(ShaderIrInst.Fadd, OperA, OperB);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
        }

        private static void EmitFmul(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            bool NegB = OpCode.Read(48);

            ShaderIrNode OperA = OpCode.Gpr8(), OperB;

            switch (Oper)
            {
                case ShaderOper.CR:   OperB = OpCode.Cbuf34();    break;
                case ShaderOper.Immf: OperB = OpCode.Immf19_20(); break;
                case ShaderOper.RR:   OperB = OpCode.Gpr20();     break;

                default: throw new ArgumentException(nameof(Oper));
            }

            OperB = GetAluFneg(OperB, NegB);

            ShaderIrNode Op = new ShaderIrOp(ShaderIrInst.Fmul, OperA, OperB);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
        }

        private static void EmitFfma(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            bool NegB = OpCode.Read(48);
            bool NegC = OpCode.Read(49);

            ShaderIrNode OperA = OpCode.Gpr8(), OperB, OperC;

            switch (Oper)
            {
                case ShaderOper.CR:   OperB = OpCode.Cbuf34();    break;
                case ShaderOper.Immf: OperB = OpCode.Immf19_20(); break;
                case ShaderOper.RC:   OperB = OpCode.Gpr39();     break;
                case ShaderOper.RR:   OperB = OpCode.Gpr20();     break;

                default: throw new ArgumentException(nameof(Oper));
            }

            OperB = GetAluFneg(OperB, NegB);

            if (Oper == ShaderOper.RC)
            {
                OperC = GetAluFneg(OpCode.Cbuf34(), NegC);
            }
            else
            {
                OperC = GetAluFneg(OpCode.Gpr39(), NegC);
            }

            ShaderIrOp Op = new ShaderIrOp(ShaderIrInst.Ffma, OperA, OperB, OperC);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
        }

        private static void EmitIadd(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            ShaderIrNode OperA = OpCode.Gpr8();
            ShaderIrNode OperB;

            switch (Oper)
            {
                case ShaderOper.CR:  OperB = OpCode.Cbuf34();   break;
                case ShaderOper.Imm: OperB = OpCode.Imm19_20(); break;
                case ShaderOper.RR:  OperB = OpCode.Gpr20();    break;

                default: throw new ArgumentException(nameof(Oper));
            }

            bool NegA = OpCode.Read(49);
            bool NegB = OpCode.Read(48);

            OperA = GetAluIneg(OperA, NegA);
            OperB = GetAluIneg(OperB, NegB);

            ShaderIrOp Op = new ShaderIrOp(ShaderIrInst.Add, OperA, OperB);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
        }

        private static void EmitIadd3(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            int Mode = OpCode.Read(37, 3);

            bool Neg1 = OpCode.Read(51);
            bool Neg2 = OpCode.Read(50);
            bool Neg3 = OpCode.Read(49);

            int Height1 = OpCode.Read(35, 3);
            int Height2 = OpCode.Read(33, 3);
            int Height3 = OpCode.Read(31, 3);

            ShaderIrNode OperB;

            switch (Oper)
            {
                case ShaderOper.CR:  OperB = OpCode.Cbuf34();   break;
                case ShaderOper.Imm: OperB = OpCode.Imm19_20(); break;
                case ShaderOper.RR:  OperB = OpCode.Gpr20();    break;

                default: throw new ArgumentException(nameof(Oper));
            }

            ShaderIrNode ApplyHeight(ShaderIrNode Src, int Height)
            {
                if (Oper != ShaderOper.RR)
                {
                    return Src;
                }

                switch (Height)
                {
                    case 0: return Src;
                    case 1: return new ShaderIrOp(ShaderIrInst.And, Src, new ShaderIrOperImm(0xffff));
                    case 2: return new ShaderIrOp(ShaderIrInst.Lsr, Src, new ShaderIrOperImm(16));

                    default: throw new InvalidOperationException();
                }
            }

            ShaderIrNode Src1 = GetAluIneg(ApplyHeight(OpCode.Gpr8(),  Height1), Neg1);
            ShaderIrNode Src2 = GetAluIneg(ApplyHeight(OperB,                Height2), Neg2);
            ShaderIrNode Src3 = GetAluIneg(ApplyHeight(OpCode.Gpr39(), Height3), Neg3);

            ShaderIrOp Sum = new ShaderIrOp(ShaderIrInst.Add, Src1, Src2);

            if (Oper == ShaderOper.RR)
            {
                switch (Mode)
                {
                    case 1: Sum = new ShaderIrOp(ShaderIrInst.Lsr, Sum, new ShaderIrOperImm(16)); break;
                    case 2: Sum = new ShaderIrOp(ShaderIrInst.Lsl, Sum, new ShaderIrOperImm(16)); break;
                }
            }

            //Note: Here there should be a "+ 1" when carry flag is set
            //but since carry is mostly ignored by other instructions, it's excluded for now

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), new ShaderIrOp(ShaderIrInst.Add, Sum, Src3))));
        }

        private static void EmitIscadd(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            bool NegB = OpCode.Read(48);
            bool NegA = OpCode.Read(49);

            ShaderIrNode OperA = OpCode.Gpr8(), OperB;

            ShaderIrOperImm Scale = OpCode.Imm5_39();

            switch (Oper)
            {
                case ShaderOper.CR:  OperB = OpCode.Cbuf34();   break;
                case ShaderOper.Imm: OperB = OpCode.Imm19_20(); break;
                case ShaderOper.RR:  OperB = OpCode.Gpr20();    break;

                default: throw new ArgumentException(nameof(Oper));
            }

            OperA = GetAluIneg(OperA, NegA);
            OperB = GetAluIneg(OperB, NegB);

            ShaderIrOp ScaleOp = new ShaderIrOp(ShaderIrInst.Lsl, OperA, Scale);
            ShaderIrOp AddOp   = new ShaderIrOp(ShaderIrInst.Add, OperB, ScaleOp);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), AddOp)));
        }

        private static void EmitFmnmx(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            EmitMnmx(Block, OpCode, true, Oper);
        }

        private static void EmitImnmx(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            EmitMnmx(Block, OpCode, false, Oper);
        }

        private static void EmitMnmx(ShaderIrBlock Block, long OpCode, bool IsFloat, ShaderOper Oper)
        {
            bool NegB = OpCode.Read(45);
            bool AbsA = OpCode.Read(46);
            bool NegA = OpCode.Read(48);
            bool AbsB = OpCode.Read(49);

            ShaderIrNode OperA = OpCode.Gpr8(), OperB;

            if (IsFloat)
            {
                OperA = GetAluFabsFneg(OperA, AbsA, NegA);
            }
            else
            {
                OperA = GetAluIabsIneg(OperA, AbsA, NegA);
            }

            switch (Oper)
            {
                case ShaderOper.CR:   OperB = OpCode.Cbuf34();    break;
                case ShaderOper.Imm:  OperB = OpCode.Imm19_20();  break;
                case ShaderOper.Immf: OperB = OpCode.Immf19_20(); break;
                case ShaderOper.RR:   OperB = OpCode.Gpr20();     break;

                default: throw new ArgumentException(nameof(Oper));
            }

            if (IsFloat)
            {
                OperB = GetAluFabsFneg(OperB, AbsB, NegB);
            }
            else
            {
                OperB = GetAluIabsIneg(OperB, AbsB, NegB);
            }

            ShaderIrOperPred Pred = OpCode.Pred39();

            ShaderIrOp Op;

            ShaderIrInst MaxInst = IsFloat ? ShaderIrInst.Fmax : ShaderIrInst.Max;
            ShaderIrInst MinInst = IsFloat ? ShaderIrInst.Fmin : ShaderIrInst.Min;

            if (Pred.IsConst)
            {
                bool IsMax = OpCode.Read(42);

                Op = new ShaderIrOp(IsMax
                    ? MaxInst
                    : MinInst, OperA, OperB);

                Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
            }
            else
            {
                ShaderIrNode PredN = OpCode.Pred39N();

                ShaderIrOp OpMax = new ShaderIrOp(MaxInst, OperA, OperB);
                ShaderIrOp OpMin = new ShaderIrOp(MinInst, OperA, OperB);

                ShaderIrAsg AsgMax = new ShaderIrAsg(OpCode.Gpr0(), OpMax);
                ShaderIrAsg AsgMin = new ShaderIrAsg(OpCode.Gpr0(), OpMin);

                Block.AddNode(OpCode.PredNode(new ShaderIrCond(PredN, AsgMax, Not: true)));
                Block.AddNode(OpCode.PredNode(new ShaderIrCond(PredN, AsgMin, Not: false)));
            }
        }

        private static void EmitRro(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            //Note: this is a range reduction instruction and is supposed to
            //be used with Mufu, here it just moves the value and ignores the operation.
            bool NegA = OpCode.Read(45);
            bool AbsA = OpCode.Read(49);

            ShaderIrNode OperA;

            switch (Oper)
            {
                case ShaderOper.CR:   OperA = OpCode.Cbuf34();    break;
                case ShaderOper.Immf: OperA = OpCode.Immf19_20(); break;
                case ShaderOper.RR:   OperA = OpCode.Gpr20();     break;

                default: throw new ArgumentException(nameof(Oper));
            }

            OperA = GetAluFabsFneg(OperA, AbsA, NegA);

            Block.AddNode(new ShaderIrCmnt("Stubbed."));

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), OperA)));
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
            bool NegA = OpCode.Read(43);
            bool AbsB = OpCode.Read(44);
            bool NegB = OpCode.Read(53);
            bool AbsA = OpCode.Read(54);

            bool BoolFloat = OpCode.Read(IsFloat ? 52 : 44);

            ShaderIrNode OperA = OpCode.Gpr8(), OperB;

            switch (Oper)
            {
                case ShaderOper.CR:   OperB = OpCode.Cbuf34();    break;
                case ShaderOper.Imm:  OperB = OpCode.Imm19_20();  break;
                case ShaderOper.Immf: OperB = OpCode.Immf19_20(); break;
                case ShaderOper.RR:   OperB = OpCode.Gpr20();     break;

                default: throw new ArgumentException(nameof(Oper));
            }

            ShaderIrInst CmpInst;

            if (IsFloat)
            {
                OperA = GetAluFabsFneg(OperA, AbsA, NegA);
                OperB = GetAluFabsFneg(OperB, AbsB, NegB);

                CmpInst = OpCode.CmpF();
            }
            else
            {
                CmpInst = OpCode.Cmp();
            }

            ShaderIrOp Op = new ShaderIrOp(CmpInst, OperA, OperB);

            ShaderIrInst LopInst = OpCode.BLop45();

            ShaderIrOperPred PNode = OpCode.Pred39();

            ShaderIrNode Imm0, Imm1;

            if (BoolFloat)
            {
                Imm0 = new ShaderIrOperImmf(0);
                Imm1 = new ShaderIrOperImmf(1);
            }
            else
            {
                Imm0 = new ShaderIrOperImm(0);
                Imm1 = new ShaderIrOperImm(-1);
            }

            ShaderIrNode Asg0 = new ShaderIrAsg(OpCode.Gpr0(), Imm0);
            ShaderIrNode Asg1 = new ShaderIrAsg(OpCode.Gpr0(), Imm1);

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

            Block.AddNode(OpCode.PredNode(Asg0));
            Block.AddNode(OpCode.PredNode(Asg1));
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
            bool AbsA = OpCode.Read(7);
            bool NegP = OpCode.Read(42);
            bool NegA = OpCode.Read(43);
            bool AbsB = OpCode.Read(44);

            ShaderIrNode OperA = OpCode.Gpr8(), OperB;

            switch (Oper)
            {
                case ShaderOper.CR:   OperB = OpCode.Cbuf34();    break;
                case ShaderOper.Imm:  OperB = OpCode.Imm19_20();  break;
                case ShaderOper.Immf: OperB = OpCode.Immf19_20(); break;
                case ShaderOper.RR:   OperB = OpCode.Gpr20();     break;

                default: throw new ArgumentException(nameof(Oper));
            }

            ShaderIrInst CmpInst;

            if (IsFloat)
            {
                OperA = GetAluFabsFneg(OperA, AbsA, NegA);
                OperB = GetAluFabs    (OperB, AbsB);

                CmpInst = OpCode.CmpF();
            }
            else
            {
                CmpInst = OpCode.Cmp();
            }

            ShaderIrOp Op = new ShaderIrOp(CmpInst, OperA, OperB);

            ShaderIrOperPred P0Node = OpCode.Pred3();
            ShaderIrOperPred P1Node = OpCode.Pred0();
            ShaderIrOperPred P2Node = OpCode.Pred39();

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(P0Node, Op)));

            ShaderIrInst LopInst = OpCode.BLop45();

            if (LopInst == ShaderIrInst.Band && P1Node.IsConst && P2Node.IsConst)
            {
                return;
            }

            ShaderIrNode P2NNode = P2Node;

            if (NegP)
            {
                P2NNode = new ShaderIrOp(ShaderIrInst.Bnot, P2NNode);
            }

            Op = new ShaderIrOp(ShaderIrInst.Bnot, P0Node);

            Op = new ShaderIrOp(LopInst, Op, P2NNode);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(P1Node, Op)));

            Op = new ShaderIrOp(LopInst, P0Node, P2NNode);

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(P0Node, Op)));
        }

        private static void EmitLop(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            int SubOp = OpCode.Read(41, 3);

            bool InvA = OpCode.Read(39);
            bool InvB = OpCode.Read(40);

            ShaderIrInst Inst = 0;

            switch (SubOp)
            {
                case 0: Inst = ShaderIrInst.And; break;
                case 1: Inst = ShaderIrInst.Or;  break;
                case 2: Inst = ShaderIrInst.Xor; break;
            }

            ShaderIrNode OperA = GetAluNot(OpCode.Gpr8(), InvA);
            ShaderIrNode OperB;

            switch (Oper)
            {
                case ShaderOper.CR:  OperB = OpCode.Cbuf34();   break;
                case ShaderOper.Imm: OperB = OpCode.Imm19_20(); break;
                case ShaderOper.RR:  OperB = OpCode.Gpr20();    break;

                default: throw new ArgumentException(nameof(Oper));
            }

            OperB = GetAluNot(OperB, InvB);

            ShaderIrNode Op;

            if (SubOp < 3)
            {
                Op = new ShaderIrOp(Inst, OperA, OperB);
            }
            else
            {
                Op = OperB;
            }

            ShaderIrNode Compare = new ShaderIrOp(ShaderIrInst.Cne, Op, new ShaderIrOperImm(0));

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Pred48(), Compare)));

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), Op)));
        }

        private static void EmitXmad(ShaderIrBlock Block, long OpCode, ShaderOper Oper)
        {
            //TODO: Confirm SignAB/C, it is just a guess.
            //TODO: Implement Mode 3 (CSFU), what it does?
            bool SignAB = OpCode.Read(48);
            bool SignC  = OpCode.Read(49);
            bool HighB  = OpCode.Read(52);
            bool HighA  = OpCode.Read(53);

            int Mode = OpCode.Read(50, 7);

            ShaderIrNode OperA = OpCode.Gpr8(), OperB, OperC;

            ShaderIrOperImm Imm16  = new ShaderIrOperImm(16);
            ShaderIrOperImm ImmMsk = new ShaderIrOperImm(0xffff);

            ShaderIrInst ShiftAB = SignAB ? ShaderIrInst.Asr : ShaderIrInst.Lsr;
            ShaderIrInst ShiftC  = SignC  ? ShaderIrInst.Asr : ShaderIrInst.Lsr;

            if (HighA)
            {
                OperA = new ShaderIrOp(ShiftAB, OperA, Imm16);
            }

            switch (Oper)
            {
                case ShaderOper.CR:  OperB = OpCode.Cbuf34();   break;
                case ShaderOper.Imm: OperB = OpCode.Imm19_20(); break;
                case ShaderOper.RC:  OperB = OpCode.Gpr39();    break;
                case ShaderOper.RR:  OperB = OpCode.Gpr20();    break;

                default: throw new ArgumentException(nameof(Oper));
            }

            bool ProductShiftLeft = false, Merge = false;

            if (Oper == ShaderOper.RC)
            {
                OperC = OpCode.Cbuf34();
            }
            else
            {
                OperC = OpCode.Gpr39();

                ProductShiftLeft = OpCode.Read(36);
                Merge            = OpCode.Read(37);
            }

            switch (Mode)
            {
                //CLO.
                case 1: OperC = ExtendTo32(OperC, SignC, 16); break;

                //CHI.
                case 2: OperC = new ShaderIrOp(ShiftC, OperC, Imm16); break;
            }

            ShaderIrNode OperBH = OperB;

            if (HighB)
            {
                OperBH = new ShaderIrOp(ShiftAB, OperBH, Imm16);
            }

            ShaderIrOp MulOp = new ShaderIrOp(ShaderIrInst.Mul, OperA, OperBH);

            if (ProductShiftLeft)
            {
                MulOp = new ShaderIrOp(ShaderIrInst.Lsl, MulOp, Imm16);
            }

            ShaderIrOp AddOp = new ShaderIrOp(ShaderIrInst.Add, MulOp, OperC);

            if (Merge)
            {
                AddOp = new ShaderIrOp(ShaderIrInst.And, AddOp, ImmMsk);
                OperB = new ShaderIrOp(ShaderIrInst.Lsl, OperB, Imm16);
                AddOp = new ShaderIrOp(ShaderIrInst.Or,  AddOp, OperB);
            }

            if (Mode == 4)
            {
                OperB = new ShaderIrOp(ShaderIrInst.Lsl, OperB, Imm16);
                AddOp = new ShaderIrOp(ShaderIrInst.Or,  AddOp, OperB);
            }

            Block.AddNode(OpCode.PredNode(new ShaderIrAsg(OpCode.Gpr0(), AddOp)));
        }
    }
}