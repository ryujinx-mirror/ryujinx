using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static class Lop3Expression
    {
        public static Operand GetFromTruthTable(
            EmitterContext context,
            Operand        srcA,
            Operand        srcB,
            Operand        srcC,
            int            imm)
        {
            Operand expr = null;

            // Handle some simple cases, or cases where
            // the KMap would yield poor results (like XORs).
            if (imm == 0x96 || imm == 0x69)
            {
                // XOR (0x96) and XNOR (0x69).
                if (imm == 0x69)
                {
                    srcA = context.BitwiseNot(srcA);
                }

                expr = context.BitwiseExclusiveOr(srcA, srcB);
                expr = context.BitwiseExclusiveOr(expr, srcC);

                return expr;
            }
            else if (imm == 0)
            {
                // Always false.
                return Const(IrConsts.False);
            }
            else if (imm == 0xff)
            {
                // Always true.
                return Const(IrConsts.True);
            }

            int map;

            // Encode into gray code.
            map  = ((imm >> 0) & 1) << 0;
            map |= ((imm >> 1) & 1) << 4;
            map |= ((imm >> 2) & 1) << 1;
            map |= ((imm >> 3) & 1) << 5;
            map |= ((imm >> 4) & 1) << 3;
            map |= ((imm >> 5) & 1) << 7;
            map |= ((imm >> 6) & 1) << 2;
            map |= ((imm >> 7) & 1) << 6;

            // Solve KMap, get sum of products.
            int visited = 0;

            for (int index = 0; index < 8 && visited != 0xff; index++)
            {
                if ((map & (1 << index)) == 0)
                {
                    continue;
                }

                int mask = 0;

                for (int mSize = 4; mSize != 0; mSize >>= 1)
                {
                    mask = RotateLeft4((1 << mSize) - 1, index & 3) << (index & 4);

                    if ((map & mask) == mask)
                    {
                        break;
                    }
                }

                // The mask should wrap, if we are on the high row, shift to low etc.
                int mask2 = (index & 4) != 0 ? mask >> 4 : mask << 4;

                if ((map & mask2) == mask2)
                {
                    mask |= mask2;
                }

                if ((mask & visited) == mask)
                {
                    continue;
                }

                bool notA = (mask & 0x33) != 0;
                bool notB = (mask & 0x99) != 0;
                bool notC = (mask & 0x0f) != 0;

                bool aChanges = (mask & 0xcc) != 0 && notA;
                bool bChanges = (mask & 0x66) != 0 && notB;
                bool cChanges = (mask & 0xf0) != 0 && notC;

                Operand localExpr = null;

                void And(Operand source)
                {
                    if (localExpr != null)
                    {
                        localExpr = context.BitwiseAnd(localExpr, source);
                    }
                    else
                    {
                        localExpr = source;
                    }
                }

                if (!aChanges)
                {
                    And(context.BitwiseNot(srcA, notA));
                }

                if (!bChanges)
                {
                    And(context.BitwiseNot(srcB, notB));
                }

                if (!cChanges)
                {
                    And(context.BitwiseNot(srcC, notC));
                }

                if (expr != null)
                {
                    expr = context.BitwiseOr(expr, localExpr);
                }
                else
                {
                    expr = localExpr;
                }

                visited |= mask;
            }

            return expr;
        }

        private static int RotateLeft4(int value, int shift)
        {
            return ((value << shift) | (value >> (4 - shift))) & 0xf;
        }
    }
}