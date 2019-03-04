using System;

namespace Ryujinx.Graphics.Gal.Shader
{
    static partial class ShaderDecode
    {
        private static int Read(this long opCode, int position, int mask)
        {
            return (int)(opCode >> position) & mask;
        }

        private static bool Read(this long opCode, int position)
        {
            return ((opCode >> position) & 1) != 0;
        }

        private static int Branch(this long opCode)
        {
            return ((int)(opCode >> 20) << 8) >> 8;
        }

        private static bool HasArray(this long opCode)
        {
            return opCode.Read(0x1c);
        }

        private static ShaderIrOperAbuf[] Abuf20(this long opCode)
        {
            int abuf = opCode.Read(20, 0x3ff);
            int size = opCode.Read(47, 3);

            ShaderIrOperGpr vertex = opCode.Gpr39();

            ShaderIrOperAbuf[] opers = new ShaderIrOperAbuf[size + 1];

            for (int index = 0; index <= size; index++)
            {
                opers[index] = new ShaderIrOperAbuf(abuf + index * 4, vertex);
            }

            return opers;
        }

        private static ShaderIrOperAbuf Abuf28(this long opCode)
        {
            int abuf = opCode.Read(28, 0x3ff);

            return new ShaderIrOperAbuf(abuf, opCode.Gpr39());
        }

        private static ShaderIrOperCbuf Cbuf34(this long opCode)
        {
            return new ShaderIrOperCbuf(
                opCode.Read(34, 0x1f),
                opCode.Read(20, 0x3fff));
        }

        private static ShaderIrOperGpr Gpr8(this long opCode)
        {
            return new ShaderIrOperGpr(opCode.Read(8, 0xff));
        }

        private static ShaderIrOperGpr Gpr20(this long opCode)
        {
            return new ShaderIrOperGpr(opCode.Read(20, 0xff));
        }

        private static ShaderIrOperGpr Gpr39(this long opCode)
        {
            return new ShaderIrOperGpr(opCode.Read(39, 0xff));
        }

        private static ShaderIrOperGpr Gpr0(this long opCode)
        {
            return new ShaderIrOperGpr(opCode.Read(0, 0xff));
        }

        private static ShaderIrOperGpr Gpr28(this long opCode)
        {
            return new ShaderIrOperGpr(opCode.Read(28, 0xff));
        }

        private static ShaderIrOperGpr[] GprHalfVec8(this long opCode)
        {
            return GetGprHalfVec2(opCode.Read(8, 0xff), opCode.Read(47, 3));
        }

        private static ShaderIrOperGpr[] GprHalfVec20(this long opCode)
        {
            return GetGprHalfVec2(opCode.Read(20, 0xff), opCode.Read(28, 3));
        }

        private static ShaderIrOperGpr[] GetGprHalfVec2(int gpr, int mask)
        {
            if (mask == 1)
            {
                //This value is used for FP32, the whole 32-bits register
                //is used as each element on the vector.
                return new ShaderIrOperGpr[]
                {
                    new ShaderIrOperGpr(gpr),
                    new ShaderIrOperGpr(gpr)
                };
            }

            ShaderIrOperGpr low  = new ShaderIrOperGpr(gpr, 0);
            ShaderIrOperGpr high = new ShaderIrOperGpr(gpr, 1);

            return new ShaderIrOperGpr[]
            {
                (mask & 1) != 0 ? high : low,
                (mask & 2) != 0 ? high : low
            };
        }

        private static ShaderIrOperGpr GprHalf0(this long opCode, int halfPart)
        {
            return new ShaderIrOperGpr(opCode.Read(0, 0xff), halfPart);
        }

        private static ShaderIrOperGpr GprHalf28(this long opCode, int halfPart)
        {
            return new ShaderIrOperGpr(opCode.Read(28, 0xff), halfPart);
        }

        private static ShaderIrOperImm Imm5_39(this long opCode)
        {
            return new ShaderIrOperImm(opCode.Read(39, 0x1f));
        }

        private static ShaderIrOperImm Imm13_36(this long opCode)
        {
            return new ShaderIrOperImm(opCode.Read(36, 0x1fff));
        }

        private static ShaderIrOperImm Imm32_20(this long opCode)
        {
            return new ShaderIrOperImm((int)(opCode >> 20));
        }

        private static ShaderIrOperImmf Immf32_20(this long opCode)
        {
            return new ShaderIrOperImmf(BitConverter.Int32BitsToSingle((int)(opCode >> 20)));
        }

        private static ShaderIrOperImm ImmU16_20(this long opCode)
        {
            return new ShaderIrOperImm(opCode.Read(20, 0xffff));
        }

        private static ShaderIrOperImm Imm19_20(this long opCode)
        {
            int value = opCode.Read(20, 0x7ffff);

            bool neg = opCode.Read(56);

            if (neg)
            {
                value = -value;
            }

            return new ShaderIrOperImm(value);
        }

        private static ShaderIrOperImmf Immf19_20(this long opCode)
        {
            uint imm = (uint)(opCode >> 20) & 0x7ffff;

            bool neg = opCode.Read(56);

            imm <<= 12;

            if (neg)
            {
                imm |= 0x80000000;
            }

            float value = BitConverter.Int32BitsToSingle((int)imm);

            return new ShaderIrOperImmf(value);
        }

        private static ShaderIrOperPred Pred0(this long opCode)
        {
            return new ShaderIrOperPred(opCode.Read(0, 7));
        }

        private static ShaderIrOperPred Pred3(this long opCode)
        {
            return new ShaderIrOperPred(opCode.Read(3, 7));
        }

        private static ShaderIrOperPred Pred12(this long opCode)
        {
            return new ShaderIrOperPred(opCode.Read(12, 7));
        }

        private static ShaderIrOperPred Pred29(this long opCode)
        {
            return new ShaderIrOperPred(opCode.Read(29, 7));
        }

        private static ShaderIrNode Pred39N(this long opCode)
        {
            ShaderIrNode node = opCode.Pred39();

            if (opCode.Read(42))
            {
                node = new ShaderIrOp(ShaderIrInst.Bnot, node);
            }

            return node;
        }

        private static ShaderIrOperPred Pred39(this long opCode)
        {
            return new ShaderIrOperPred(opCode.Read(39, 7));
        }

        private static ShaderIrOperPred Pred48(this long opCode)
        {
            return new ShaderIrOperPred(opCode.Read(48, 7));
        }

        private static ShaderIrInst Cmp(this long opCode)
        {
            switch (opCode.Read(49, 7))
            {
                case 1: return ShaderIrInst.Clt;
                case 2: return ShaderIrInst.Ceq;
                case 3: return ShaderIrInst.Cle;
                case 4: return ShaderIrInst.Cgt;
                case 5: return ShaderIrInst.Cne;
                case 6: return ShaderIrInst.Cge;
            }

            throw new ArgumentException(nameof(opCode));
        }

        private static ShaderIrInst CmpF(this long opCode)
        {
            switch (opCode.Read(48, 0xf))
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

            throw new ArgumentException(nameof(opCode));
        }

        private static ShaderIrInst BLop45(this long opCode)
        {
            switch (opCode.Read(45, 3))
            {
                case 0: return ShaderIrInst.Band;
                case 1: return ShaderIrInst.Bor;
                case 2: return ShaderIrInst.Bxor;
            }

            throw new ArgumentException(nameof(opCode));
        }

        private static ShaderIrInst BLop24(this long opCode)
        {
            switch (opCode.Read(24, 3))
            {
                case 0: return ShaderIrInst.Band;
                case 1: return ShaderIrInst.Bor;
                case 2: return ShaderIrInst.Bxor;
            }

            throw new ArgumentException(nameof(opCode));
        }

        private static ShaderIrNode PredNode(this long opCode, ShaderIrNode node)
        {
            ShaderIrOperPred pred = opCode.PredNode();

            if (pred.Index != ShaderIrOperPred.UnusedIndex)
            {
                bool inv = opCode.Read(19);

                node = new ShaderIrCond(pred, node, inv);
            }

            return node;
        }

        private static ShaderIrOperPred PredNode(this long opCode)
        {
            int pred = opCode.Read(16, 0xf);

            if (pred != 0xf)
            {
                pred &= 7;
            }

            return new ShaderIrOperPred(pred);
        }
    }
}