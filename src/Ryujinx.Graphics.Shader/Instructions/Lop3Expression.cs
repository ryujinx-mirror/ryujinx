using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static class Lop3Expression
    {
        private enum TruthTable : byte
        {
            False         = 0x00, // false
            True          = 0xff, // true
            In            = 0xf0, // a
            And2          = 0xc0, // a & b
            Or2           = 0xfc, // a | b
            Xor2          = 0x3c, // a ^ b
            And3          = 0x80, // a & b & c
            Or3           = 0xfe, // a | b | c
            XorAnd        = 0x60, // a & (b ^ c)
            XorOr         = 0xf6, // a | (b ^ c)
            OrAnd         = 0xe0, // a & (b | c)
            AndOr         = 0xf8, // a | (b & c)
            Onehot        = 0x16, // (a & !b & !c) | (!a & b & !c) | (!a & !b & c) - Only one value is true.
            Majority      = 0xe8, // Popcount(a, b, c) >= 2
            Gamble        = 0x81, // (a & b & c) | (!a & !b & !c) - All on or all off
            InverseGamble = 0x7e, // Inverse of Gamble
            Dot           = 0x1a, // a ^ (c | (a & b))
            Mux           = 0xca, // a ? b : c
            AndXor        = 0x78, // a ^ (b & c)
            OrXor         = 0x1e, // a ^ (b | c)
            Xor3          = 0x96, // a ^ b ^ c
        }

        public static Operand GetFromTruthTable(EmitterContext context, Operand srcA, Operand srcB, Operand srcC, int imm)
        {
            for (int i = 0; i < 0x40; i++)
            {
                TruthTable currImm = (TruthTable)imm;

                Operand x = srcA;
                Operand y = srcB;
                Operand z = srcC;
                
                if ((i & 0x01) != 0)
                {
                    (x, y) = (y, x);
                    currImm = PermuteTable(currImm, 7, 6, 3, 2, 5, 4, 1, 0);
                }

                if ((i & 0x02) != 0)
                {
                    (x, z) = (z, x);
                    currImm = PermuteTable(currImm, 7, 3, 5, 1, 6, 2, 4, 0);
                }

                if ((i & 0x04) != 0)
                {
                    (y, z) = (z, y);
                    currImm = PermuteTable(currImm, 7, 5, 6, 4, 3, 1, 2, 0);
                }

                if ((i & 0x08) != 0)
                {
                    x = context.BitwiseNot(x);
                    currImm = PermuteTable(currImm, 3, 2, 1, 0, 7, 6, 5, 4);
                }

                if ((i & 0x10) != 0)
                {
                    y = context.BitwiseNot(y);
                    currImm = PermuteTable(currImm, 5, 4, 7, 6, 1, 0, 3, 2);
                }

                if ((i & 0x20) != 0)
                {
                    z = context.BitwiseNot(z);
                    currImm = PermuteTable(currImm, 6, 7, 4, 5, 2, 3, 0, 1);
                }

                Operand result = GetExpr(currImm, context, x, y, z);
                if (result != null)
                {
                    return result;
                }

                Operand notResult = GetExpr((TruthTable)((~(int)currImm) & 0xff), context, x, y, z);
                if (notResult != null)
                {
                    return context.BitwiseNot(notResult);
                }
            }

            return null;
        }

        private static Operand GetExpr(TruthTable imm, EmitterContext context, Operand x, Operand y, Operand z)
        {
            return imm switch
            {
                TruthTable.False         => Const(0),
                TruthTable.True          => Const(-1),
                TruthTable.In            => x,
                TruthTable.And2          => context.BitwiseAnd(x, y),
                TruthTable.Or2           => context.BitwiseOr(x, y),
                TruthTable.Xor2          => context.BitwiseExclusiveOr(x, y),
                TruthTable.And3          => context.BitwiseAnd(x, context.BitwiseAnd(y, z)),
                TruthTable.Or3           => context.BitwiseOr(x, context.BitwiseOr(y, z)),
                TruthTable.XorAnd        => context.BitwiseAnd(x, context.BitwiseExclusiveOr(y, z)),
                TruthTable.XorOr         => context.BitwiseOr(x, context.BitwiseExclusiveOr(y, z)),
                TruthTable.OrAnd         => context.BitwiseAnd(x, context.BitwiseOr(y, z)),
                TruthTable.AndOr         => context.BitwiseOr(x, context.BitwiseAnd(y, z)),
                TruthTable.Onehot        => context.BitwiseExclusiveOr(context.BitwiseOr(x, y), context.BitwiseOr(z, context.BitwiseAnd(x, y))),
                TruthTable.Majority      => context.BitwiseAnd(context.BitwiseOr(x, y), context.BitwiseOr(z, context.BitwiseAnd(x, y))),
                TruthTable.InverseGamble => context.BitwiseOr(context.BitwiseExclusiveOr(x, y), context.BitwiseExclusiveOr(x, z)),
                TruthTable.Dot           => context.BitwiseAnd(context.BitwiseExclusiveOr(x, z), context.BitwiseOr(context.BitwiseNot(y), z)),
                TruthTable.Mux           => context.BitwiseOr(context.BitwiseAnd(x, y), context.BitwiseAnd(context.BitwiseNot(x), z)),
                TruthTable.AndXor        => context.BitwiseExclusiveOr(x, context.BitwiseAnd(y, z)),
                TruthTable.OrXor         => context.BitwiseExclusiveOr(x, context.BitwiseOr(y, z)),
                TruthTable.Xor3          => context.BitwiseExclusiveOr(x, context.BitwiseExclusiveOr(y, z)),
                _                        => null
            };
        }

        private static TruthTable PermuteTable(TruthTable imm, int bit7, int bit6, int bit5, int bit4, int bit3, int bit2, int bit1, int bit0)
        {
            int result = 0;

            result |= (((int)imm >> 0) & 1) << bit0;
            result |= (((int)imm >> 1) & 1) << bit1;
            result |= (((int)imm >> 2) & 1) << bit2;
            result |= (((int)imm >> 3) & 1) << bit3;
            result |= (((int)imm >> 4) & 1) << bit4;
            result |= (((int)imm >> 5) & 1) << bit5;
            result |= (((int)imm >> 6) & 1) << bit6;
            result |= (((int)imm >> 7) & 1) << bit7;

            return (TruthTable)result;
        }
    }
}