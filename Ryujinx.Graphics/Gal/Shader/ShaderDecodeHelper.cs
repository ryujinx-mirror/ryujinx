using System;

namespace Ryujinx.Graphics.Gal.Shader
{
    static class ShaderDecodeHelper
    {
        public static ShaderIrOperAbuf[] GetOperAbuf20(long OpCode)
        {
            int Abuf = (int)(OpCode >> 20) & 0x3ff;
            int Size = (int)(OpCode >> 47) & 3;

            ShaderIrOperGpr Vertex = GetOperGpr39(OpCode);

            ShaderIrOperAbuf[] Opers = new ShaderIrOperAbuf[Size + 1];

            for (int Index = 0; Index <= Size; Index++)
            {
                Opers[Index] = new ShaderIrOperAbuf(Abuf + Index * 4, Vertex);
            }

            return Opers;
        }

        public static ShaderIrOperAbuf GetOperAbuf28(long OpCode)
        {
            int Abuf = (int)(OpCode >> 28) & 0x3ff;

            return new ShaderIrOperAbuf(Abuf, GetOperGpr39(OpCode));
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

        public static ShaderIrOperImm GetOperImm5_39(long OpCode)
        {
            return new ShaderIrOperImm((int)(OpCode >> 39) & 0x1f);
        }

        public static ShaderIrOperImm GetOperImm13_36(long OpCode)
        {
            return new ShaderIrOperImm((int)(OpCode >> 36) & 0x1fff);
        }

        public static ShaderIrOperImm GetOperImm32_20(long OpCode)
        {
            return new ShaderIrOperImm((int)(OpCode >> 20));
        }

        public static ShaderIrOperImmf GetOperImmf32_20(long OpCode)
        {
            return new ShaderIrOperImmf(BitConverter.Int32BitsToSingle((int)(OpCode >> 20)));
        }

        public static ShaderIrOperImm GetOperImm19_20(long OpCode)
        {
            int Value = (int)(OpCode >> 20) & 0x7ffff;

            bool Neg = ((OpCode >> 56) & 1) != 0;

            if (Neg)
            {
                Value = -Value;
            }

            return new ShaderIrOperImm((int)Value);
        }

        public static ShaderIrOperImmf GetOperImmf19_20(long OpCode)
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

        public static ShaderIrOperPred GetOperPred0(long OpCode)
        {
            return new ShaderIrOperPred((int)(OpCode >> 0) & 7);
        }

        public static ShaderIrOperPred GetOperPred3(long OpCode)
        {
            return new ShaderIrOperPred((int)(OpCode >> 3) & 7);
        }

        public static ShaderIrOperPred GetOperPred12(long OpCode)
        {
            return new ShaderIrOperPred((int)(OpCode >> 12) & 7);
        }

        public static ShaderIrOperPred GetOperPred29(long OpCode)
        {
            return new ShaderIrOperPred((int)(OpCode >> 29) & 7);
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

        public static ShaderIrOperPred GetOperPred48(long OpCode)
        {
            return new ShaderIrOperPred((int)((OpCode >> 48) & 7));
        }

        public static ShaderIrInst GetCmp(long OpCode)
        {
            switch ((int)(OpCode >> 49) & 7)
            {
                case 1: return ShaderIrInst.Clt;
                case 2: return ShaderIrInst.Ceq;
                case 3: return ShaderIrInst.Cle;
                case 4: return ShaderIrInst.Cgt;
                case 5: return ShaderIrInst.Cne;
                case 6: return ShaderIrInst.Cge;
            }

            throw new ArgumentException(nameof(OpCode));
        }

        public static ShaderIrInst GetCmpF(long OpCode)
        {
            switch ((int)(OpCode >> 48) & 0xf)
            {
                case 0x1: return ShaderIrInst.Fclt;
                case 0x2: return ShaderIrInst.Fceq;
                case 0x3: return ShaderIrInst.Fcle;
                case 0x4: return ShaderIrInst.Fcgt;
                case 0x5: return ShaderIrInst.Fcne;
                case 0x6: return ShaderIrInst.Fcge;
                case 0x7: return ShaderIrInst.Fcnum;
                case 0x8: return ShaderIrInst.Fcnan;
                case 0x9: return ShaderIrInst.Fcltu;
                case 0xa: return ShaderIrInst.Fcequ;
                case 0xb: return ShaderIrInst.Fcleu;
                case 0xc: return ShaderIrInst.Fcgtu;
                case 0xd: return ShaderIrInst.Fcneu;
                case 0xe: return ShaderIrInst.Fcgeu;
            }

            throw new ArgumentException(nameof(OpCode));
        }

        public static ShaderIrInst GetBLop45(long OpCode)
        {
            switch ((int)(OpCode >> 45) & 3)
            {
                case 0: return ShaderIrInst.Band;
                case 1: return ShaderIrInst.Bor;
                case 2: return ShaderIrInst.Bxor;
            }

            throw new ArgumentException(nameof(OpCode));
        }

        public static ShaderIrInst GetBLop24(long OpCode)
        {
            switch ((int)(OpCode >> 24) & 3)
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
                bool Inv = ((OpCode >> 19) & 1) != 0;

                Node = new ShaderIrCond(Pred, Node, Inv);
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

        public static ShaderIrNode GetAluFabsFneg(ShaderIrNode Node, bool Abs, bool Neg)
        {
            return GetAluFneg(GetAluFabs(Node, Abs), Neg);
        }

        public static ShaderIrNode GetAluFabs(ShaderIrNode Node, bool Abs)
        {
            return Abs ? new ShaderIrOp(ShaderIrInst.Fabs, Node) : Node;
        }

        public static ShaderIrNode GetAluFneg(ShaderIrNode Node, bool Neg)
        {
            return Neg ? new ShaderIrOp(ShaderIrInst.Fneg, Node) : Node;
        }

        public static ShaderIrNode GetAluIabsIneg(ShaderIrNode Node, bool Abs, bool Neg)
        {
            return GetAluIneg(GetAluIabs(Node, Abs), Neg);
        }

        public static ShaderIrNode GetAluIabs(ShaderIrNode Node, bool Abs)
        {
            return Abs ? new ShaderIrOp(ShaderIrInst.Abs, Node) : Node;
        }

        public static ShaderIrNode GetAluIneg(ShaderIrNode Node, bool Neg)
        {
            return Neg ? new ShaderIrOp(ShaderIrInst.Neg, Node) : Node;
        }

        public static ShaderIrNode GetAluNot(ShaderIrNode Node, bool Not)
        {
            return Not ? new ShaderIrOp(ShaderIrInst.Not, Node) : Node;
        }

        public static ShaderIrNode ExtendTo32(ShaderIrNode Node, bool Signed, int Size)
        {
            int Shift = 32 - Size;

            ShaderIrInst RightShift = Signed
                ? ShaderIrInst.Asr
                : ShaderIrInst.Lsr;

            Node = new ShaderIrOp(ShaderIrInst.Lsl, Node, new ShaderIrOperImm(Shift));
            Node = new ShaderIrOp(RightShift,       Node, new ShaderIrOperImm(Shift));

            return Node;
        }

        public static ShaderIrNode ExtendTo32(ShaderIrNode Node, bool Signed, ShaderIrNode Size)
        {
            ShaderIrOperImm WordSize = new ShaderIrOperImm(32);

            ShaderIrOp Shift = new ShaderIrOp(ShaderIrInst.Sub, WordSize, Size);

            ShaderIrInst RightShift = Signed
                ? ShaderIrInst.Asr
                : ShaderIrInst.Lsr;

            Node = new ShaderIrOp(ShaderIrInst.Lsl, Node, Shift);
            Node = new ShaderIrOp(RightShift,       Node, Shift);

            return Node;
        }
    }
}