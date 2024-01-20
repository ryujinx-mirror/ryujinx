using Ryujinx.Cpu.LightningJit.CodeGen;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitHalve
    {
        public static void Shadd16(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitHadd(context, rd, rn, rm, 0x7fff7fff, unsigned: false);
        }

        public static void Shadd8(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitHadd(context, rd, rn, rm, 0x7f7f7f7f, unsigned: false);
        }

        public static void Shsub16(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitHsub(context, rd, rn, rm, 0x7fff7fff, unsigned: false);
        }

        public static void Shsub8(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitHsub(context, rd, rn, rm, 0x7f7f7f7f, unsigned: false);
        }

        public static void Shasx(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            InstEmitCommon.EmitSigned16BitXPair(context, rd, rn, rm, (d, n, m, e) =>
            {
                if (e == 0)
                {
                    context.Arm64Assembler.Sub(d, n, m);
                }
                else
                {
                    context.Arm64Assembler.Add(d, n, m);
                }

                context.Arm64Assembler.Lsr(d, d, InstEmitCommon.Const(1));
            });
        }

        public static void Shsax(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            InstEmitCommon.EmitSigned16BitXPair(context, rd, rn, rm, (d, n, m, e) =>
            {
                if (e == 0)
                {
                    context.Arm64Assembler.Add(d, n, m);
                }
                else
                {
                    context.Arm64Assembler.Sub(d, n, m);
                }

                context.Arm64Assembler.Lsr(d, d, InstEmitCommon.Const(1));
            });
        }

        public static void Uhadd16(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitHadd(context, rd, rn, rm, 0x7fff7fff, unsigned: true);
        }

        public static void Uhadd8(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitHadd(context, rd, rn, rm, 0x7f7f7f7f, unsigned: true);
        }

        public static void Uhasx(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            InstEmitCommon.EmitUnsigned16BitXPair(context, rd, rn, rm, (d, n, m, e) =>
            {
                if (e == 0)
                {
                    context.Arm64Assembler.Sub(d, n, m);
                }
                else
                {
                    context.Arm64Assembler.Add(d, n, m);
                }

                context.Arm64Assembler.Lsr(d, d, InstEmitCommon.Const(1));
            });
        }

        public static void Uhsax(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            InstEmitCommon.EmitUnsigned16BitXPair(context, rd, rn, rm, (d, n, m, e) =>
            {
                if (e == 0)
                {
                    context.Arm64Assembler.Add(d, n, m);
                }
                else
                {
                    context.Arm64Assembler.Sub(d, n, m);
                }

                context.Arm64Assembler.Lsr(d, d, InstEmitCommon.Const(1));
            });
        }

        public static void Uhsub16(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitHsub(context, rd, rn, rm, 0x7fff7fff, unsigned: true);
        }

        public static void Uhsub8(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitHsub(context, rd, rn, rm, 0x7f7f7f7f, unsigned: true);
        }

        private static void EmitHadd(CodeGenContext context, uint rd, uint rn, uint rm, int mask, bool unsigned)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            using ScopedRegister res = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister carry = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            // This relies on the equality x+y == ((x&y) << 1) + (x^y).
            // Note that x^y always contains the LSB of the result.
            // Since we want to calculate (x+y)/2, we can instead calculate (x&y) + ((x^y)>>1).
            // We mask by 0x7F/0x7FFF to remove the LSB so that it doesn't leak into the field below.

            context.Arm64Assembler.And(res.Operand, rmOperand, rnOperand);
            context.Arm64Assembler.Eor(carry.Operand, rmOperand, rnOperand);
            context.Arm64Assembler.Lsr(rdOperand, carry.Operand, InstEmitCommon.Const(1));
            context.Arm64Assembler.And(rdOperand, rdOperand, InstEmitCommon.Const(mask));
            context.Arm64Assembler.Add(rdOperand, rdOperand, res.Operand);

            if (!unsigned)
            {
                // Propagates the sign bit from (x^y)>>1 upwards by one.
                context.Arm64Assembler.And(carry.Operand, carry.Operand, InstEmitCommon.Const(~mask));
                context.Arm64Assembler.Eor(rdOperand, rdOperand, carry.Operand);
            }
        }

        private static void EmitHsub(CodeGenContext context, uint rd, uint rn, uint rm, int mask, bool unsigned)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            using ScopedRegister carry = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister left = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister right = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            // This relies on the equality x-y == (x^y) - (((x^y)&y) << 1).
            // Note that x^y always contains the LSB of the result.
            // Since we want to calculate (x+y)/2, we can instead calculate ((x^y)>>1) - ((x^y)&y).

            context.Arm64Assembler.Eor(carry.Operand, rmOperand, rnOperand);
            context.Arm64Assembler.Lsr(left.Operand, carry.Operand, InstEmitCommon.Const(1));
            context.Arm64Assembler.And(right.Operand, carry.Operand, rmOperand);

            // We must now perform a partitioned subtraction.
            // We can do this because minuend contains 7/15 bit fields.
            // We use the extra bit in minuend as a bit to borrow from; we set this bit.
            // We invert this bit at the end as this tells us if that bit was borrowed from.

            context.Arm64Assembler.Orr(rdOperand, left.Operand, InstEmitCommon.Const(~mask));
            context.Arm64Assembler.Sub(rdOperand, rdOperand, right.Operand);
            context.Arm64Assembler.Eor(rdOperand, rdOperand, InstEmitCommon.Const(~mask));

            if (!unsigned)
            {
                // We then sign extend the result into this bit.
                context.Arm64Assembler.And(carry.Operand, carry.Operand, InstEmitCommon.Const(~mask));
                context.Arm64Assembler.Eor(rdOperand, rdOperand, carry.Operand);
            }
        }
    }
}
