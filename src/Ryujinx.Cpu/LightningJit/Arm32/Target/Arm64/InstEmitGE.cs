using Ryujinx.Cpu.LightningJit.CodeGen;
using Ryujinx.Cpu.LightningJit.CodeGen.Arm64;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitGE
    {
        public static void Sadd16(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAddSub(context, rd, rn, rm, is16Bit: true, add: true, unsigned: false);
        }

        public static void Sadd8(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAddSub(context, rd, rn, rm, is16Bit: false, add: true, unsigned: false);
        }

        public static void Sasx(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAsxSax(context, rd, rn, rm, isAsx: true, unsigned: false);
        }

        public static void Sel(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            using ScopedRegister geFlags = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            ExtractGEFlags(context, geFlags.Operand);

            // Broadcast compact GE flags (one bit to one byte, 0b1111 -> 0x1010101).
            context.Arm64Assembler.Mov(tempRegister.Operand, 0x204081u);
            context.Arm64Assembler.Mul(geFlags.Operand, geFlags.Operand, tempRegister.Operand);
            context.Arm64Assembler.And(geFlags.Operand, geFlags.Operand, InstEmitCommon.Const(0x1010101));

            // Build mask from expanded flags (0x1010101 -> 0xFFFFFFFF).
            context.Arm64Assembler.Lsl(tempRegister.Operand, geFlags.Operand, InstEmitCommon.Const(8));
            context.Arm64Assembler.Sub(geFlags.Operand, tempRegister.Operand, geFlags.Operand);

            // Result = (n & mask) | (m & ~mask).
            context.Arm64Assembler.And(tempRegister.Operand, geFlags.Operand, rnOperand);
            context.Arm64Assembler.Bic(rdOperand, rmOperand, geFlags.Operand);
            context.Arm64Assembler.Orr(rdOperand, rdOperand, tempRegister.Operand);
        }

        public static void Ssax(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAsxSax(context, rd, rn, rm, isAsx: false, unsigned: false);
        }

        public static void Ssub16(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAddSub(context, rd, rn, rm, is16Bit: true, add: false, unsigned: false);
        }

        public static void Ssub8(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAddSub(context, rd, rn, rm, is16Bit: false, add: false, unsigned: false);
        }

        public static void Uadd16(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAddSub(context, rd, rn, rm, is16Bit: true, add: true, unsigned: true);
        }

        public static void Uadd8(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAddSub(context, rd, rn, rm, is16Bit: false, add: true, unsigned: true);
        }

        public static void Uasx(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAsxSax(context, rd, rn, rm, isAsx: true, unsigned: true);
        }

        public static void Usax(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAsxSax(context, rd, rn, rm, isAsx: false, unsigned: true);
        }

        public static void Usub16(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAddSub(context, rd, rn, rm, is16Bit: true, add: false, unsigned: true);
        }

        public static void Usub8(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAddSub(context, rd, rn, rm, is16Bit: false, add: false, unsigned: true);
        }

        private static void EmitAddSub(CodeGenContext context, uint rd, uint rn, uint rm, bool is16Bit, bool add, bool unsigned)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            using ScopedRegister geFlags = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            int e = 0;

            void Emit(Operand d, Operand n, Operand m)
            {
                if (add)
                {
                    context.Arm64Assembler.Add(d, n, m);
                }
                else
                {
                    context.Arm64Assembler.Sub(d, n, m);
                }

                if (unsigned && add)
                {
                    if (e == 0)
                    {
                        context.Arm64Assembler.Lsr(geFlags.Operand, d, InstEmitCommon.Const(is16Bit ? 16 : 8));
                    }
                    else
                    {
                        using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                        context.Arm64Assembler.Lsr(tempRegister.Operand, d, InstEmitCommon.Const(is16Bit ? 16 : 8));
                        context.Arm64Assembler.Orr(geFlags.Operand, geFlags.Operand, tempRegister.Operand, ArmShiftType.Lsl, e);
                    }
                }
                else
                {
                    using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                    context.Arm64Assembler.Mvn(tempRegister.Operand, d);

                    if (e == 0)
                    {
                        context.Arm64Assembler.Lsr(geFlags.Operand, tempRegister.Operand, InstEmitCommon.Const(31));
                    }
                    else
                    {
                        context.Arm64Assembler.Lsr(tempRegister.Operand, tempRegister.Operand, InstEmitCommon.Const(31));
                        context.Arm64Assembler.Orr(geFlags.Operand, geFlags.Operand, tempRegister.Operand, ArmShiftType.Lsl, e);
                    }
                }

                e += is16Bit ? 2 : 1;
            }

            if (is16Bit)
            {
                if (unsigned)
                {
                    InstEmitCommon.EmitUnsigned16BitPair(context, rd, rn, rm, Emit);
                }
                else
                {
                    InstEmitCommon.EmitSigned16BitPair(context, rd, rn, rm, Emit);
                }

                // Duplicate bits.
                context.Arm64Assembler.Orr(geFlags.Operand, geFlags.Operand, geFlags.Operand, ArmShiftType.Lsl, 1);
            }
            else
            {
                if (unsigned)
                {
                    InstEmitCommon.EmitUnsigned8BitPair(context, rd, rn, rm, Emit);
                }
                else
                {
                    InstEmitCommon.EmitSigned8BitPair(context, rd, rn, rm, Emit);
                }
            }

            UpdateGEFlags(context, geFlags.Operand);
        }

        private static void EmitAsxSax(CodeGenContext context, uint rd, uint rn, uint rm, bool isAsx, bool unsigned)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            using ScopedRegister geFlags = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            void Emit(Operand d, Operand n, Operand m, int e)
            {
                bool add = e == (isAsx ? 1 : 0);

                if (add)
                {
                    context.Arm64Assembler.Add(d, n, m);
                }
                else
                {
                    context.Arm64Assembler.Sub(d, n, m);
                }

                if (unsigned && add)
                {
                    if (e == 0)
                    {
                        context.Arm64Assembler.Lsr(geFlags.Operand, d, InstEmitCommon.Const(16));
                    }
                    else
                    {
                        using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                        context.Arm64Assembler.Lsr(tempRegister.Operand, d, InstEmitCommon.Const(16));
                        context.Arm64Assembler.Orr(geFlags.Operand, geFlags.Operand, tempRegister.Operand, ArmShiftType.Lsl, e * 2);
                    }
                }
                else
                {
                    using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                    context.Arm64Assembler.Mvn(tempRegister.Operand, d);

                    if (e == 0)
                    {
                        context.Arm64Assembler.Lsr(geFlags.Operand, tempRegister.Operand, InstEmitCommon.Const(31));
                    }
                    else
                    {
                        context.Arm64Assembler.Lsr(tempRegister.Operand, tempRegister.Operand, InstEmitCommon.Const(31));
                        context.Arm64Assembler.Orr(geFlags.Operand, geFlags.Operand, tempRegister.Operand, ArmShiftType.Lsl, e * 2);
                    }
                }
            }

            if (unsigned)
            {
                InstEmitCommon.EmitUnsigned16BitXPair(context, rd, rn, rm, Emit);
            }
            else
            {
                InstEmitCommon.EmitSigned16BitXPair(context, rd, rn, rm, Emit);
            }

            // Duplicate bits.
            context.Arm64Assembler.Orr(geFlags.Operand, geFlags.Operand, geFlags.Operand, ArmShiftType.Lsl, 1);

            UpdateGEFlags(context, geFlags.Operand);
        }

        public static void UpdateGEFlags(CodeGenContext context, Operand flags)
        {
            Operand ctx = InstEmitSystem.Register(context.RegisterAllocator.FixedContextRegister);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            context.Arm64Assembler.LdrRiUn(tempRegister.Operand, ctx, NativeContextOffsets.FlagsBaseOffset);
            context.Arm64Assembler.Bfi(tempRegister.Operand, flags, 16, 4);
            context.Arm64Assembler.StrRiUn(tempRegister.Operand, ctx, NativeContextOffsets.FlagsBaseOffset);
        }

        public static void ExtractGEFlags(CodeGenContext context, Operand flags)
        {
            Operand ctx = InstEmitSystem.Register(context.RegisterAllocator.FixedContextRegister);

            context.Arm64Assembler.LdrRiUn(flags, ctx, NativeContextOffsets.FlagsBaseOffset);
            context.Arm64Assembler.Ubfx(flags, flags, 16, 4);
        }
    }
}
