using System;

namespace Ryujinx.Graphics.Gal.Shader
{
    static class ShaderDecodeHelper
    {
        public static ShaderIrOperAbuf[] GetOperAbuf20(long OpCode)
        {
            int Abuf = (int)(OpCode >> 20) & 0x3ff;
            int Reg  = (int)(OpCode >> 39) & 0xff;
            int Size = (int)(OpCode >> 47) & 3;

            ShaderIrOperAbuf[] Opers = new ShaderIrOperAbuf[Size + 1];

            for (int Index = 0; Index <= Size; Index++)
            {
                Opers[Index] = new ShaderIrOperAbuf(Abuf, Reg);
            }

            return Opers;
        }

        public static ShaderIrOperAbuf GetOperAbuf28(long OpCode)
        {
            int Abuf = (int)(OpCode >> 28) & 0x3ff;
            int Reg  = (int)(OpCode >> 39) & 0xff;

            return new ShaderIrOperAbuf(Abuf, Reg);
        }

        public static ShaderIrOperCbuf GetOperCbuf34(long OpCode)
        {
            return new ShaderIrOperCbuf(
                (int)(OpCode >> 34) & 0x1f,
                (int)(OpCode >> 20) & 0x3fff);
        }

        public static ShaderIrOperGpr GetOperGpr8(long OpCode)
        {
            return new ShaderIrOperGpr((int)(OpCode >> 8) & 0xff);
        }

        public static ShaderIrOperGpr GetOperGpr20(long OpCode)
        {
            return new ShaderIrOperGpr((int)(OpCode >> 20) & 0xff);
        }

        public static ShaderIrOperGpr GetOperGpr39(long OpCode)
        {
            return new ShaderIrOperGpr((int)(OpCode >> 39) & 0xff);
        }

        public static ShaderIrOperGpr GetOperGpr0(long OpCode)
        {
            return new ShaderIrOperGpr((int)(OpCode >> 0) & 0xff);
        }

        public static ShaderIrOperGpr GetOperGpr28(long OpCode)
        {
            return new ShaderIrOperGpr((int)(OpCode >> 28) & 0xff);
        }

        public static ShaderIrNode GetOperImm19_20(long OpCode)
        {
            int Value = (int)(OpCode >> 20) & 0x7ffff;

            bool Neg = ((OpCode >> 56) & 1) != 0;

            if (Neg)
            {
                Value = -Value;
            }

            return new ShaderIrOperImm((int)Value);
        }

        public static ShaderIrNode GetOperImmf19_20(long OpCode)
        {
            uint Imm = (uint)(OpCode >> 20) & 0x7ffff;

            bool Neg = ((OpCode >> 56) & 1) != 0;

            Imm <<= 12;

            if (Neg)
            {
                Imm |= 0x80000000;
            }

            float Value = BitConverter.Int32BitsToSingle((int)Imm);

            return new ShaderIrOperImmf(Value);
        }

        public static ShaderIrOperImm GetOperImm13_36(long OpCode)
        {
            return new ShaderIrOperImm((int)(OpCode >> 36) & 0x1fff);
        }

        public static ShaderIrOperImm GetOperImm32_20(long OpCode)
        {
            return new ShaderIrOperImm((int)(OpCode >> 20));
        }

        public static ShaderIrOperPred GetOperPred3(long OpCode)
        {
            return new ShaderIrOperPred((int)(OpCode >> 3) & 7);
        }

        public static ShaderIrOperPred GetOperPred0(long OpCode)
        {
            return new ShaderIrOperPred((int)(OpCode >> 0) & 7);
        }

        public static ShaderIrNode GetOperPred39N(long OpCode)
        {
            ShaderIrNode Node = GetOperPred39(OpCode);

            if (((OpCode >> 42) & 1) != 0)
            {
                Node = new ShaderIrOp(ShaderIrInst.Bnot, Node);
            }

            return Node;
        }

        public static ShaderIrOperPred GetOperPred39(long OpCode)
        {
            return new ShaderIrOperPred((int)(OpCode >> 39) & 7);
        }

        public static ShaderIrInst GetCmp(long OpCode)
        {
            switch ((int)(OpCode >> 48) & 0xf)
            {
                case 0x1: return ShaderIrInst.Clt;
                case 0x2: return ShaderIrInst.Ceq;
                case 0x3: return ShaderIrInst.Cle;
                case 0x4: return ShaderIrInst.Cgt;
                case 0x5: return ShaderIrInst.Cne;
                case 0x6: return ShaderIrInst.Cge;
                case 0x7: return ShaderIrInst.Cnum;
                case 0x8: return ShaderIrInst.Cnan;
                case 0x9: return ShaderIrInst.Cltu;
                case 0xa: return ShaderIrInst.Cequ;
                case 0xb: return ShaderIrInst.Cleu;
                case 0xc: return ShaderIrInst.Cgtu;
                case 0xd: return ShaderIrInst.Cneu;
                case 0xe: return ShaderIrInst.Cgeu;
            }

            throw new ArgumentException(nameof(OpCode));
        }

        public static ShaderIrInst GetBLop(long OpCode)
        {
            switch ((int)(OpCode >> 45) & 3)
            {
                case 0: return ShaderIrInst.Band;
                case 1: return ShaderIrInst.Bor;
                case 2: return ShaderIrInst.Bxor;
            }

            throw new ArgumentException(nameof(OpCode));
        }

        public static ShaderIrNode GetPredNode(ShaderIrNode Node, long OpCode)
        {
            ShaderIrOperPred Pred = GetPredNode(OpCode);

            if (Pred.Index != ShaderIrOperPred.UnusedIndex)
            {
                Node = new ShaderIrCond(Pred, Node);
            }

            return Node;
        }

        private static ShaderIrOperPred GetPredNode(long OpCode)
        {
            int Pred = (int)(OpCode >> 16) & 0xf;

            if (Pred != 0xf)
            {
                Pred &= 7;
            }

            return new ShaderIrOperPred(Pred);
        }

        public static ShaderIrNode GetAluAbsNeg(ShaderIrNode Node, bool Abs, bool Neg)
        {
            return GetAluNeg(GetAluAbs(Node, Abs), Neg);
        }

        public static ShaderIrNode GetAluAbs(ShaderIrNode Node, bool Abs)
        {
            return Abs ? new ShaderIrOp(ShaderIrInst.Fabs, Node) : Node;
        }

        public static ShaderIrNode GetAluNeg(ShaderIrNode Node, bool Neg)
        {
            return Neg ? new ShaderIrOp(ShaderIrInst.Fneg, Node) : Node;
        }

        public static ShaderIrNode GetAluNot(ShaderIrNode Node, bool Not)
        {
            return Not ? new ShaderIrOp(ShaderIrInst.Not, Node) : Node;
        }
    }
}