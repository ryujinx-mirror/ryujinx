using Ryujinx.Cpu.LightningJit.CodeGen;
using Ryujinx.Cpu.LightningJit.CodeGen.Arm64;
using System.Diagnostics;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitSaturate
    {
        public static void Qadd(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAddSubSaturate(context, rd, rn, rm, doubling: false, add: true);
        }

        public static void Qadd16(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            InstEmitCommon.EmitSigned16BitPair(context, rd, rn, rm, (d, n, m) =>
            {
                context.Arm64Assembler.Add(d, n, m);
                EmitSaturateRange(context, d, d, 16, unsigned: false, setQ: false);
            });
        }

        public static void Qadd8(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            InstEmitCommon.EmitSigned8BitPair(context, rd, rn, rm, (d, n, m) =>
            {
                context.Arm64Assembler.Add(d, n, m);
                EmitSaturateRange(context, d, d, 8, unsigned: false, setQ: false);
            });
        }

        public static void Qasx(CodeGenContext context, uint rd, uint rn, uint rm)
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

                EmitSaturateRange(context, d, d, 16, unsigned: false, setQ: false);
            });
        }

        public static void Qdadd(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAddSubSaturate(context, rd, rn, rm, doubling: true, add: true);
        }

        public static void Qdsub(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAddSubSaturate(context, rd, rn, rm, doubling: true, add: false);
        }

        public static void Qsax(CodeGenContext context, uint rd, uint rn, uint rm)
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

                EmitSaturateRange(context, d, d, 16, unsigned: false, setQ: false);
            });
        }

        public static void Qsub(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            EmitAddSubSaturate(context, rd, rn, rm, doubling: false, add: false);
        }

        public static void Qsub16(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            InstEmitCommon.EmitSigned16BitPair(context, rd, rn, rm, (d, n, m) =>
            {
                context.Arm64Assembler.Sub(d, n, m);
                EmitSaturateRange(context, d, d, 16, unsigned: false, setQ: false);
            });
        }

        public static void Qsub8(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            InstEmitCommon.EmitSigned8BitPair(context, rd, rn, rm, (d, n, m) =>
            {
                context.Arm64Assembler.Sub(d, n, m);
                EmitSaturateRange(context, d, d, 8, unsigned: false, setQ: false);
            });
        }

        public static void Ssat(CodeGenContext context, uint rd, uint imm, uint rn, bool sh, uint shift)
        {
            EmitSaturate(context, rd, imm + 1, rn, sh, shift, unsigned: false);
        }

        public static void Ssat16(CodeGenContext context, uint rd, uint imm, uint rn)
        {
            InstEmitCommon.EmitSigned16BitPair(context, rd, rn, (d, n) =>
            {
                EmitSaturateRange(context, d, n, imm + 1, unsigned: false);
            });
        }

        public static void Uqadd16(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            InstEmitCommon.EmitUnsigned16BitPair(context, rd, rn, rm, (d, n, m) =>
            {
                context.Arm64Assembler.Add(d, n, m);
                EmitSaturateUqadd(context, d, 16);
            });
        }

        public static void Uqadd8(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            InstEmitCommon.EmitUnsigned8BitPair(context, rd, rn, rm, (d, n, m) =>
            {
                context.Arm64Assembler.Add(d, n, m);
                EmitSaturateUqadd(context, d, 8);
            });
        }

        public static void Uqasx(CodeGenContext context, uint rd, uint rn, uint rm)
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

                EmitSaturateUq(context, d, 16, e == 0);
            });
        }

        public static void Uqsax(CodeGenContext context, uint rd, uint rn, uint rm)
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

                EmitSaturateUq(context, d, 16, e != 0);
            });
        }

        public static void Uqsub16(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            InstEmitCommon.EmitUnsigned16BitPair(context, rd, rn, rm, (d, n, m) =>
            {
                context.Arm64Assembler.Sub(d, n, m);
                EmitSaturateUqsub(context, d, 16);
            });
        }

        public static void Uqsub8(CodeGenContext context, uint rd, uint rn, uint rm)
        {
            InstEmitCommon.EmitUnsigned8BitPair(context, rd, rn, rm, (d, n, m) =>
            {
                context.Arm64Assembler.Sub(d, n, m);
                EmitSaturateUqsub(context, d, 8);
            });
        }

        public static void Usat(CodeGenContext context, uint rd, uint imm, uint rn, bool sh, uint shift)
        {
            EmitSaturate(context, rd, imm, rn, sh, shift, unsigned: true);
        }

        public static void Usat16(CodeGenContext context, uint rd, uint imm, uint rn)
        {
            InstEmitCommon.EmitSigned16BitPair(context, rd, rn, (d, n) =>
            {
                EmitSaturateRange(context, d, n, imm, unsigned: true);
            });
        }

        private static void EmitAddSubSaturate(CodeGenContext context, uint rd, uint rn, uint rm, bool doubling, bool add)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand tempN64 = new(OperandKind.Register, OperandType.I64, tempN.Operand.Value);
            Operand tempM64 = new(OperandKind.Register, OperandType.I64, tempM.Operand.Value);

            context.Arm64Assembler.Sxtw(tempN64, rnOperand);
            context.Arm64Assembler.Sxtw(tempM64, rmOperand);

            if (doubling)
            {
                context.Arm64Assembler.Lsl(tempN64, tempN64, InstEmitCommon.Const(1));

                EmitSaturateLongToInt(context, tempN64, tempN64);
            }

            if (add)
            {
                context.Arm64Assembler.Add(tempN64, tempN64, tempM64);
            }
            else
            {
                context.Arm64Assembler.Sub(tempN64, tempN64, tempM64);
            }

            EmitSaturateLongToInt(context, rdOperand, tempN64);
        }

        private static void EmitSaturate(CodeGenContext context, uint rd, uint imm, uint rn, bool sh, uint shift, bool unsigned)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            if (sh && shift == 0)
            {
                shift = 31;
            }

            if (shift != 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                if (sh)
                {
                    context.Arm64Assembler.Asr(tempRegister.Operand, rnOperand, InstEmitCommon.Const((int)shift));
                }
                else
                {
                    context.Arm64Assembler.Lsl(tempRegister.Operand, rnOperand, InstEmitCommon.Const((int)shift));
                }

                EmitSaturateRange(context, rdOperand, tempRegister.Operand, imm, unsigned);
            }
            else
            {
                EmitSaturateRange(context, rdOperand, rnOperand, imm, unsigned);
            }
        }

        private static void EmitSaturateRange(CodeGenContext context, Operand result, Operand value, uint saturateTo, bool unsigned, bool setQ = true)
        {
            Debug.Assert(saturateTo <= 32);
            Debug.Assert(!unsigned || saturateTo < 32);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            ScopedRegister tempValue = default;

            bool resultValueOverlap = result.Value == value.Value;

            if (!unsigned && saturateTo == 32)
            {
                // No saturation possible for this case.

                if (!resultValueOverlap)
                {
                    context.Arm64Assembler.Mov(result, value);
                }

                return;
            }
            else if (saturateTo == 0)
            {
                // Result is always zero if we saturate 0 bits.

                context.Arm64Assembler.Mov(result, 0u);

                return;
            }

            if (resultValueOverlap)
            {
                tempValue = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                context.Arm64Assembler.Mov(tempValue.Operand, value);
                value = tempValue.Operand;
            }

            if (unsigned)
            {
                // Negative values always saturate (to zero).
                // So we must always ignore the sign bit when masking, so that the truncated value will differ from the original one.

                context.Arm64Assembler.And(result, value, InstEmitCommon.Const((int)(uint.MaxValue >> (32 - (int)saturateTo))));
            }
            else
            {
                context.Arm64Assembler.Sbfx(result, value, 0, (int)saturateTo);
            }

            context.Arm64Assembler.Sub(tempRegister.Operand, value, result);

            int branchIndex = context.CodeWriter.InstructionPointer;

            // If the result is 0, the values are equal and we don't need saturation.
            context.Arm64Assembler.Cbz(tempRegister.Operand, 0);

            // Saturate and set Q flag.
            if (unsigned)
            {
                if (saturateTo == 31)
                {
                    // Only saturation case possible when going from 32 bits signed to 32 or 31 bits unsigned
                    // is when the signed input is negative, as all positive values are representable on a 31 bits range.

                    context.Arm64Assembler.Mov(result, 0u);
                }
                else
                {
                    context.Arm64Assembler.Asr(result, value, InstEmitCommon.Const(31));
                    context.Arm64Assembler.Mvn(result, result);
                    context.Arm64Assembler.Lsr(result, result, InstEmitCommon.Const(32 - (int)saturateTo));
                }
            }
            else
            {
                if (saturateTo == 1)
                {
                    context.Arm64Assembler.Asr(result, value, InstEmitCommon.Const(31));
                }
                else
                {
                    context.Arm64Assembler.Mov(result, uint.MaxValue >> (33 - (int)saturateTo));
                    context.Arm64Assembler.Eor(result, result, value, ArmShiftType.Asr, 31);
                }
            }

            if (setQ)
            {
                SetQFlag(context);
            }

            int delta = context.CodeWriter.InstructionPointer - branchIndex;
            context.CodeWriter.WriteInstructionAt(branchIndex, context.CodeWriter.ReadInstructionAt(branchIndex) | (uint)((delta & 0x7ffff) << 5));

            if (resultValueOverlap)
            {
                tempValue.Dispose();
            }
        }

        private static void EmitSaturateUqadd(CodeGenContext context, Operand value, uint saturateTo)
        {
            EmitSaturateUq(context, value, saturateTo, isSub: false);
        }

        private static void EmitSaturateUqsub(CodeGenContext context, Operand value, uint saturateTo)
        {
            EmitSaturateUq(context, value, saturateTo, isSub: true);
        }

        private static void EmitSaturateUq(CodeGenContext context, Operand value, uint saturateTo, bool isSub)
        {
            Debug.Assert(saturateTo <= 32);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            if (saturateTo == 32)
            {
                // No saturation possible for this case.

                return;
            }
            else if (saturateTo == 0)
            {
                // Result is always zero if we saturate 0 bits.

                context.Arm64Assembler.Mov(value, 0u);

                return;
            }

            context.Arm64Assembler.Lsr(tempRegister.Operand, value, InstEmitCommon.Const((int)saturateTo));

            int branchIndex = context.CodeWriter.InstructionPointer;

            // If the result is 0, the values are equal and we don't need saturation.
            context.Arm64Assembler.Cbz(tempRegister.Operand, 0);

            // Saturate.
            context.Arm64Assembler.Mov(value, isSub ? 0u : uint.MaxValue >> (32 - (int)saturateTo));

            int delta = context.CodeWriter.InstructionPointer - branchIndex;
            context.CodeWriter.WriteInstructionAt(branchIndex, context.CodeWriter.ReadInstructionAt(branchIndex) | (uint)((delta & 0x7ffff) << 5));
        }

        private static void EmitSaturateLongToInt(CodeGenContext context, Operand result, Operand value)
        {
            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            ScopedRegister tempValue = default;

            bool resultValueOverlap = result.Value == value.Value;

            if (resultValueOverlap)
            {
                tempValue = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                Operand tempValue64 = new(OperandKind.Register, OperandType.I64, tempValue.Operand.Value);

                context.Arm64Assembler.Mov(tempValue64, value);
                value = tempValue64;
            }

            Operand temp64 = new(OperandKind.Register, OperandType.I64, tempRegister.Operand.Value);
            Operand result64 = new(OperandKind.Register, OperandType.I64, result.Value);

            context.Arm64Assembler.Sxtw(result64, value);
            context.Arm64Assembler.Sub(temp64, value, result64);

            int branchIndex = context.CodeWriter.InstructionPointer;

            // If the result is 0, the values are equal and we don't need saturation.
            context.Arm64Assembler.Cbz(temp64, 0);

            // Saturate and set Q flag.
            context.Arm64Assembler.Mov(result, uint.MaxValue >> 1);
            context.Arm64Assembler.Eor(result64, result64, value, ArmShiftType.Asr, 63);

            SetQFlag(context);

            int delta = context.CodeWriter.InstructionPointer - branchIndex;
            context.CodeWriter.WriteInstructionAt(branchIndex, context.CodeWriter.ReadInstructionAt(branchIndex) | (uint)((delta & 0x7ffff) << 5));

            context.Arm64Assembler.Mov(result, result); // Zero-extend.

            if (resultValueOverlap)
            {
                tempValue.Dispose();
            }
        }

        public static void SetQFlag(CodeGenContext context)
        {
            Operand ctx = InstEmitSystem.Register(context.RegisterAllocator.FixedContextRegister);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            context.Arm64Assembler.LdrRiUn(tempRegister.Operand, ctx, NativeContextOffsets.FlagsBaseOffset);
            context.Arm64Assembler.Orr(tempRegister.Operand, tempRegister.Operand, InstEmitCommon.Const(1 << 27));
            context.Arm64Assembler.StrRiUn(tempRegister.Operand, ctx, NativeContextOffsets.FlagsBaseOffset);
        }
    }
}
