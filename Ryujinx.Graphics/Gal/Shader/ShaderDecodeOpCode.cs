using System;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        private static int Read(this long OpCode, int Position, int Mask)
        {
            return (int)(OpCode >> Position) & Mask;
        }

        private static bool Read(this long OpCode, int Position)
        {
            return ((OpCode >> Position) & 1) != 0;
        }

        private static int Branch(this long OpCode)
        {
            return ((int)(OpCode >> 20) << 8) >> 8;
        }

        private static ShaderIrOperAbuf[] Abuf20(this long OpCode)
        {
            int Abuf = OpCode.Read(20, 0x3ff);
            int Size = OpCode.Read(47, 3);

            ShaderIrOperGpr Vertex = OpCode.Gpr39();

            ShaderIrOperAbuf[] Opers = new ShaderIrOperAbuf[Size + 1];

            for (int Index = 0; Index <= Size; Index++)
            {
                Opers[Index] = new ShaderIrOperAbuf(Abuf + Index * 4, Vertex);
            }

            return Opers;
        }

        private static ShaderIrOperAbuf Abuf28(this long OpCode)
        {
            int Abuf = OpCode.Read(28, 0x3ff);

            return new ShaderIrOperAbuf(Abuf, OpCode.Gpr39());
        }

        private static ShaderIrOperCbuf Cbuf34(this long OpCode)
        {
            return new ShaderIrOperCbuf(
                OpCode.Read(34, 0x1f),
                OpCode.Read(20, 0x3fff));
        }

        private static ShaderIrOperGpr Gpr8(this long OpCode)
        {
            return new ShaderIrOperGpr(OpCode.Read(8, 0xff));
        }

        private static ShaderIrOperGpr Gpr20(this long OpCode)
        {
            return new ShaderIrOperGpr(OpCode.Read(20, 0xff));
        }

        private static ShaderIrOperGpr Gpr39(this long OpCode)
        {
            return new ShaderIrOperGpr(OpCode.Read(39, 0xff));
        }

        private static ShaderIrOperGpr Gpr0(this long OpCode)
        {
            return new ShaderIrOperGpr(OpCode.Read(0, 0xff));
        }

        private static ShaderIrOperGpr Gpr28(this long OpCode)
        {
            return new ShaderIrOperGpr(OpCode.Read(28, 0xff));
        }

        private static ShaderIrOperGpr[] GprHalfVec8(this long OpCode)
        {
            return GetGprHalfVec2(OpCode.Read(8, 0xff), OpCode.Read(47, 3));
        }

        private static ShaderIrOperGpr[] GprHalfVec20(this long OpCode)
        {
            return GetGprHalfVec2(OpCode.Read(20, 0xff), OpCode.Read(28, 3));
        }

        private static ShaderIrOperGpr[] GetGprHalfVec2(int Gpr, int Mask)
        {
            if (Mask == 1)
            {
                //This value is used for FP32, the whole 32-bits register
                //is used as each element on the vector.
                return new ShaderIrOperGpr[]
                {
                    new ShaderIrOperGpr(Gpr),
                    new ShaderIrOperGpr(Gpr)
                };
            }

            ShaderIrOperGpr Low  = new ShaderIrOperGpr(Gpr, 0);
            ShaderIrOperGpr High = new ShaderIrOperGpr(Gpr, 1);

            return new ShaderIrOperGpr[]
            {
                (Mask & 1) != 0 ? High : Low,
                (Mask & 2) != 0 ? High : Low
            };
        }

        private static ShaderIrOperGpr GprHalf0(this long OpCode, int HalfPart)
        {
            return new ShaderIrOperGpr(OpCode.Read(0, 0xff), HalfPart);
        }

        private static ShaderIrOperGpr GprHalf28(this long OpCode, int HalfPart)
        {
            return new ShaderIrOperGpr(OpCode.Read(28, 0xff), HalfPart);
        }

        private static ShaderIrOperImm Imm5_39(this long OpCode)
        {
            return new ShaderIrOperImm(OpCode.Read(39, 0x1f));
        }

        private static ShaderIrOperImm Imm13_36(this long OpCode)
        {
            return new ShaderIrOperImm(OpCode.Read(36, 0x1fff));
        }

        private static ShaderIrOperImm Imm32_20(this long OpCode)
        {
            return new ShaderIrOperImm((int)(OpCode >> 20));
        }

        private static ShaderIrOperImmf Immf32_20(this long OpCode)
        {
            return new ShaderIrOperImmf(BitConverter.Int32BitsToSingle((int)(OpCode >> 20)));
        }

        private static ShaderIrOperImm Imm19_20(this long OpCode)
        {
            int Value = OpCode.Read(20, 0x7ffff);

            bool Neg = OpCode.Read(56);

            if (Neg)
            {
                Value = -Value;
            }

            return new ShaderIrOperImm(Value);
        }

        private static ShaderIrOperImmf Immf19_20(this long OpCode)
        {
            uint Imm = (uint)(OpCode >> 20) & 0x7ffff;

            bool Neg = OpCode.Read(56);

            Imm <<= 12;

            if (Neg)
            {
                Imm |= 0x80000000;
            }

            float Value = BitConverter.Int32BitsToSingle((int)Imm);

            return new ShaderIrOperImmf(Value);
        }

        private static ShaderIrOperPred Pred0(this long OpCode)
        {
            return new ShaderIrOperPred(OpCode.Read(0, 7));
        }

        private static ShaderIrOperPred Pred3(this long OpCode)
        {
            return new ShaderIrOperPred(OpCode.Read(3, 7));
        }

        private static ShaderIrOperPred Pred12(this long OpCode)
        {
            return new ShaderIrOperPred(OpCode.Read(12, 7));
        }

        private static ShaderIrOperPred Pred29(this long OpCode)
        {
            return new ShaderIrOperPred(OpCode.Read(29, 7));
        }

        private static ShaderIrNode Pred39N(this long OpCode)
        {
            ShaderIrNode Node = OpCode.Pred39();

            if (OpCode.Read(42))
            {
                Node = new ShaderIrOp(ShaderIrInst.Bnot, Node);
            }

            return Node;
        }

        private static ShaderIrOperPred Pred39(this long OpCode)
        {
            return new ShaderIrOperPred(OpCode.Read(39, 7));
        }

        private static ShaderIrOperPred Pred48(this long OpCode)
        {
            return new ShaderIrOperPred(OpCode.Read(48, 7));
        }

        private static ShaderIrInst Cmp(this long OpCode)
        {
            switch (OpCode.Read(49, 7))
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

        private static ShaderIrInst CmpF(this long OpCode)
        {
            switch (OpCode.Read(48, 0xf))
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

        private static ShaderIrInst BLop45(this long OpCode)
        {
            switch (OpCode.Read(45, 3))
            {
                case 0: return ShaderIrInst.Band;
                case 1: return ShaderIrInst.Bor;
                case 2: return ShaderIrInst.Bxor;
            }

            throw new ArgumentException(nameof(OpCode));
        }

        private static ShaderIrInst BLop24(this long OpCode)
        {
            switch (OpCode.Read(24, 3))
            {
                case 0: return ShaderIrInst.Band;
                case 1: return ShaderIrInst.Bor;
                case 2: return ShaderIrInst.Bxor;
            }

            throw new ArgumentException(nameof(OpCode));
        }

        private static ShaderIrNode PredNode(this long OpCode, ShaderIrNode Node)
        {
            ShaderIrOperPred Pred = OpCode.PredNode();

            if (Pred.Index != ShaderIrOperPred.UnusedIndex)
            {
                bool Inv = OpCode.Read(19);

                Node = new ShaderIrCond(Pred, Node, Inv);
            }

            return Node;
        }

        private static ShaderIrOperPred PredNode(this long OpCode)
        {
            int Pred = OpCode.Read(16, 0xf);

            if (Pred != 0xf)
            {
                Pred &= 7;
            }

            return new ShaderIrOperPred(Pred);
        }
    }
}