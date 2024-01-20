using Ryujinx.Cpu.LightningJit.CodeGen;
using System;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitMultiply
    {
        public static void Mla(CodeGenContext context, uint rd, uint rn, uint rm, uint ra)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);
            Operand raOperand = InstEmitCommon.GetInputGpr(context, ra);

            context.Arm64Assembler.Madd(rdOperand, rnOperand, rmOperand, raOperand);
        }

        public static void Mls(CodeGenContext context, uint rd, uint rn, uint rm, uint ra)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);
            Operand raOperand = InstEmitCommon.GetInputGpr(context, ra);

            context.Arm64Assembler.Msub(rdOperand, rnOperand, rmOperand, raOperand);
        }

        public static void Mul(CodeGenContext context, uint rd, uint rn, uint rm, bool s)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            if (s)
            {
                using ScopedRegister flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

                context.Arm64Assembler.Mul(rdOperand, rnOperand, rmOperand);
                context.Arm64Assembler.Tst(rdOperand, rdOperand);

                InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

                context.SetNzcvModified();
            }
            else
            {
                context.Arm64Assembler.Mul(rdOperand, rnOperand, rmOperand);
            }
        }

        public static void Smlabb(CodeGenContext context, uint rd, uint rn, uint rm, uint ra, bool nHigh, bool mHigh)
        {
            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempA = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand tempM64 = new(OperandKind.Register, OperandType.I64, tempM.Operand.Value);
            Operand tempA64 = new(OperandKind.Register, OperandType.I64, tempA.Operand.Value);

            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);
            Operand raOperand = InstEmitCommon.GetInputGpr(context, ra);

            SelectSignedHalfword(context, tempN.Operand, rnOperand, nHigh);
            SelectSignedHalfword(context, tempM.Operand, rmOperand, mHigh);

            context.Arm64Assembler.Sxtw(tempA64, raOperand);
            context.Arm64Assembler.Smaddl(tempN.Operand, tempN.Operand, tempM.Operand, tempA64);

            CheckResultOverflow(context, tempM64, tempN.Operand);

            context.Arm64Assembler.Mov(rdOperand, tempN.Operand);
        }

        public static void Smlad(CodeGenContext context, uint rd, uint rn, uint rm, uint ra, bool x)
        {
            EmitSmladSmlsd(context, rd, rn, rm, ra, x, add: true);
        }

        public static void Smlal(CodeGenContext context, uint rdLo, uint rdHi, uint rn, uint rm, bool s)
        {
            EmitMultiplyAddLong(context, context.Arm64Assembler.Smaddl, rdLo, rdHi, rn, rm, s);
        }

        public static void Smlalbb(CodeGenContext context, uint rdLo, uint rdHi, uint rn, uint rm, bool nHigh, bool mHigh)
        {
            Operand rdLoOperand = InstEmitCommon.GetOutputGpr(context, rdLo);
            Operand rdHiOperand = InstEmitCommon.GetOutputGpr(context, rdHi);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            Operand rdLoOperand64 = new(OperandKind.Register, OperandType.I64, rdLoOperand.Value);
            Operand rdHiOperand64 = new(OperandKind.Register, OperandType.I64, rdHiOperand.Value);

            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempA = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            SelectSignedHalfword(context, tempN.Operand, rnOperand, nHigh);
            SelectSignedHalfword(context, tempM.Operand, rmOperand, mHigh);

            Operand tempA64 = new(OperandKind.Register, OperandType.I64, tempA.Operand.Value);

            context.Arm64Assembler.Lsl(tempA64, rdHiOperand64, InstEmitCommon.Const(32));
            context.Arm64Assembler.Orr(tempA64, tempA64, rdLoOperand);

            context.Arm64Assembler.Smaddl(rdLoOperand64, tempN.Operand, tempM.Operand, tempA64);

            if (rdLo != rdHi)
            {
                context.Arm64Assembler.Lsr(rdHiOperand64, rdLoOperand64, InstEmitCommon.Const(32));
            }

            context.Arm64Assembler.Mov(rdLoOperand, rdLoOperand); // Zero-extend.
        }

        public static void Smlald(CodeGenContext context, uint rdLo, uint rdHi, uint rn, uint rm, bool x)
        {
            EmitSmlaldSmlsld(context, rdLo, rdHi, rn, rm, x, add: true);
        }

        public static void Smlawb(CodeGenContext context, uint rd, uint rn, uint rm, uint ra, bool mHigh)
        {
            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempA = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand tempN64 = new(OperandKind.Register, OperandType.I64, tempN.Operand.Value);
            Operand tempM64 = new(OperandKind.Register, OperandType.I64, tempM.Operand.Value);
            Operand tempA64 = new(OperandKind.Register, OperandType.I64, tempA.Operand.Value);

            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);
            Operand raOperand = InstEmitCommon.GetInputGpr(context, ra);

            SelectSignedHalfword(context, tempM.Operand, rmOperand, mHigh);

            context.Arm64Assembler.Sxtw(tempA64, raOperand);
            context.Arm64Assembler.Lsl(tempA64, tempA64, InstEmitCommon.Const(16));
            context.Arm64Assembler.Smaddl(tempN.Operand, rnOperand, tempM.Operand, tempA64);
            context.Arm64Assembler.Asr(tempN64, tempN64, InstEmitCommon.Const(16));

            CheckResultOverflow(context, tempM64, tempN.Operand);

            context.Arm64Assembler.Mov(rdOperand, tempN.Operand);
        }

        public static void Smlsd(CodeGenContext context, uint rd, uint rn, uint rm, uint ra, bool x)
        {
            EmitSmladSmlsd(context, rd, rn, rm, ra, x, add: false);
        }

        public static void Smlsld(CodeGenContext context, uint rdLo, uint rdHi, uint rn, uint rm, bool x)
        {
            EmitSmlaldSmlsld(context, rdLo, rdHi, rn, rm, x, add: false);
        }

        public static void Smmla(CodeGenContext context, uint rd, uint rn, uint rm, uint ra, bool r)
        {
            EmitSmmlaSmmls(context, rd, rn, rm, ra, r, add: true);
        }

        public static void Smmls(CodeGenContext context, uint rd, uint rn, uint rm, uint ra, bool r)
        {
            EmitSmmlaSmmls(context, rd, rn, rm, ra, r, add: false);
        }

        public static void Smmul(CodeGenContext context, uint rd, uint rn, uint rm, bool r)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            Operand rdOperand64 = new(OperandKind.Register, OperandType.I64, rdOperand.Value);

            context.Arm64Assembler.Smull(rdOperand64, rnOperand, rmOperand);

            if (r)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                context.Arm64Assembler.Mov(tempRegister.Operand, 0x80000000u);
                context.Arm64Assembler.Add(rdOperand64, rdOperand64, tempRegister.Operand);
            }

            context.Arm64Assembler.Lsr(rdOperand64, rdOperand64, InstEmitCommon.Const(32));
        }

        public static void Smuad(CodeGenContext context, uint rd, uint rn, uint rm, bool x)
        {
            EmitSmuadSmusd(context, rd, rn, rm, x, add: true);
        }

        public static void Smulbb(CodeGenContext context, uint rd, uint rn, uint rm, bool nHigh, bool mHigh)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            Operand rdOperand64 = new(OperandKind.Register, OperandType.I64, rdOperand.Value);

            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            SelectSignedHalfword(context, tempN.Operand, rnOperand, nHigh);
            SelectSignedHalfword(context, tempM.Operand, rmOperand, mHigh);

            context.Arm64Assembler.Smull(rdOperand64, tempN.Operand, tempM.Operand);

            context.Arm64Assembler.Mov(rdOperand, rdOperand); // Zero-extend.
        }

        public static void Smull(CodeGenContext context, uint rdLo, uint rdHi, uint rn, uint rm, bool s)
        {
            EmitMultiplyLong(context, context.Arm64Assembler.Smull, rdLo, rdHi, rn, rm, s);
        }

        public static void Smulwb(CodeGenContext context, uint rd, uint rn, uint rm, bool mHigh)
        {
            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand tempN64 = new(OperandKind.Register, OperandType.I64, tempN.Operand.Value);
            Operand tempM64 = new(OperandKind.Register, OperandType.I64, tempM.Operand.Value);

            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            SelectSignedHalfword(context, tempM.Operand, rmOperand, mHigh);

            context.Arm64Assembler.Smull(tempN.Operand, rnOperand, tempM.Operand);
            context.Arm64Assembler.Asr(tempN64, tempN64, InstEmitCommon.Const(16));

            CheckResultOverflow(context, tempM64, tempN.Operand);

            context.Arm64Assembler.Mov(rdOperand, tempN.Operand);
        }

        public static void Smusd(CodeGenContext context, uint rd, uint rn, uint rm, bool x)
        {
            EmitSmuadSmusd(context, rd, rn, rm, x, add: false);
        }

        public static void Umaal(CodeGenContext context, uint rdLo, uint rdHi, uint rn, uint rm)
        {
            Operand rdLoOperand = InstEmitCommon.GetOutputGpr(context, rdLo);
            Operand rdHiOperand = InstEmitCommon.GetOutputGpr(context, rdHi);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            Operand rdLoOperand64 = new(OperandKind.Register, OperandType.I64, rdLoOperand.Value);
            Operand rdHiOperand64 = new(OperandKind.Register, OperandType.I64, rdHiOperand.Value);

            if (rdLo == rdHi)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                Operand tempRegister64 = new(OperandKind.Register, OperandType.I64, tempRegister.Operand.Value);

                context.Arm64Assembler.Umaddl(tempRegister64, rnOperand, rmOperand, rdLoOperand64);
                context.Arm64Assembler.Add(rdLoOperand64, tempRegister64, rdHiOperand64);
            }
            else
            {
                context.Arm64Assembler.Umaddl(rdLoOperand64, rnOperand, rmOperand, rdLoOperand64);
                context.Arm64Assembler.Add(rdLoOperand64, rdLoOperand64, rdHiOperand64);
            }

            if (rdLo != rdHi)
            {
                context.Arm64Assembler.Lsr(rdHiOperand64, rdLoOperand64, InstEmitCommon.Const(32));
            }

            context.Arm64Assembler.Mov(rdLoOperand, rdLoOperand); // Zero-extend.
        }

        public static void Umlal(CodeGenContext context, uint rdLo, uint rdHi, uint rn, uint rm, bool s)
        {
            EmitMultiplyAddLong(context, context.Arm64Assembler.Umaddl, rdLo, rdHi, rn, rm, s);
        }

        public static void Umull(CodeGenContext context, uint rdLo, uint rdHi, uint rn, uint rm, bool s)
        {
            EmitMultiplyLong(context, context.Arm64Assembler.Umull, rdLo, rdHi, rn, rm, s);
        }

        private static void EmitMultiplyLong(CodeGenContext context, Action<Operand, Operand, Operand> action, uint rdLo, uint rdHi, uint rn, uint rm, bool s)
        {
            Operand rdLoOperand = InstEmitCommon.GetOutputGpr(context, rdLo);
            Operand rdHiOperand = InstEmitCommon.GetOutputGpr(context, rdHi);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            Operand rdLoOperand64 = new(OperandKind.Register, OperandType.I64, rdLoOperand.Value);
            Operand rdHiOperand64 = new(OperandKind.Register, OperandType.I64, rdHiOperand.Value);

            if (s)
            {
                using ScopedRegister flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

                action(rdLoOperand64, rnOperand, rmOperand);
                context.Arm64Assembler.Tst(rdLoOperand64, rdLoOperand64);

                InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);
            }
            else
            {
                action(rdLoOperand64, rnOperand, rmOperand);
            }

            if (rdLo != rdHi)
            {
                context.Arm64Assembler.Lsr(rdHiOperand64, rdLoOperand64, InstEmitCommon.Const(32));
            }

            context.Arm64Assembler.Mov(rdLoOperand, rdLoOperand); // Zero-extend.
        }

        private static void EmitMultiplyAddLong(CodeGenContext context, Action<Operand, Operand, Operand, Operand> action, uint rdLo, uint rdHi, uint rn, uint rm, bool s)
        {
            Operand rdLoOperand = InstEmitCommon.GetOutputGpr(context, rdLo);
            Operand rdHiOperand = InstEmitCommon.GetOutputGpr(context, rdHi);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            Operand rdLoOperand64 = new(OperandKind.Register, OperandType.I64, rdLoOperand.Value);
            Operand rdHiOperand64 = new(OperandKind.Register, OperandType.I64, rdHiOperand.Value);

            using ScopedRegister raRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand raOperand64 = new(OperandKind.Register, OperandType.I64, raRegister.Operand.Value);

            context.Arm64Assembler.Lsl(raOperand64, rdHiOperand64, InstEmitCommon.Const(32));
            context.Arm64Assembler.Orr(raOperand64, raOperand64, rdLoOperand);

            if (s)
            {
                using ScopedRegister flagsRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                InstEmitCommon.GetCurrentFlags(context, flagsRegister.Operand);

                action(rdLoOperand64, rnOperand, rmOperand, raOperand64);
                context.Arm64Assembler.Tst(rdLoOperand64, rdLoOperand64);

                InstEmitCommon.RestoreCvFlags(context, flagsRegister.Operand);

                context.SetNzcvModified();
            }
            else
            {
                action(rdLoOperand64, rnOperand, rmOperand, raOperand64);
            }

            if (rdLo != rdHi)
            {
                context.Arm64Assembler.Lsr(rdHiOperand64, rdLoOperand64, InstEmitCommon.Const(32));
            }

            context.Arm64Assembler.Mov(rdLoOperand, rdLoOperand); // Zero-extend.
        }

        private static void EmitSmladSmlsd(CodeGenContext context, uint rd, uint rn, uint rm, uint ra, bool x, bool add)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);
            Operand raOperand = InstEmitCommon.GetInputGpr(context, ra);

            Operand rdOperand64 = new(OperandKind.Register, OperandType.I64, rdOperand.Value);

            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempA = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand tempN64 = new(OperandKind.Register, OperandType.I64, tempN.Operand.Value);
            Operand tempM64 = new(OperandKind.Register, OperandType.I64, tempM.Operand.Value);
            Operand tempA64 = new(OperandKind.Register, OperandType.I64, tempA.Operand.Value);

            ScopedRegister swapTemp = default;

            if (x)
            {
                swapTemp = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                context.Arm64Assembler.Ror(swapTemp.Operand, rmOperand, InstEmitCommon.Const(16));

                rmOperand = swapTemp.Operand;
            }

            context.Arm64Assembler.Sxth(tempN64, rnOperand);
            context.Arm64Assembler.Sxth(tempM64, rmOperand);
            context.Arm64Assembler.Sxtw(tempA64, raOperand);

            context.Arm64Assembler.Mul(rdOperand64, tempN64, tempM64);

            context.Arm64Assembler.Asr(tempN.Operand, rnOperand, InstEmitCommon.Const(16));
            context.Arm64Assembler.Asr(tempM.Operand, rmOperand, InstEmitCommon.Const(16));

            if (add)
            {
                context.Arm64Assembler.Smaddl(rdOperand64, tempN.Operand, tempM.Operand, rdOperand64);
            }
            else
            {
                context.Arm64Assembler.Smsubl(rdOperand64, tempN.Operand, tempM.Operand, rdOperand64);
            }

            context.Arm64Assembler.Add(rdOperand64, rdOperand64, tempA64);

            CheckResultOverflow(context, tempM64, rdOperand64);

            context.Arm64Assembler.Mov(rdOperand, rdOperand); // Zero-extend.

            if (x)
            {
                swapTemp.Dispose();
            }
        }

        private static void EmitSmlaldSmlsld(CodeGenContext context, uint rdLo, uint rdHi, uint rn, uint rm, bool x, bool add)
        {
            Operand rdLoOperand = InstEmitCommon.GetOutputGpr(context, rdLo);
            Operand rdHiOperand = InstEmitCommon.GetOutputGpr(context, rdHi);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            Operand rdLoOperand64 = new(OperandKind.Register, OperandType.I64, rdLoOperand.Value);
            Operand rdHiOperand64 = new(OperandKind.Register, OperandType.I64, rdHiOperand.Value);

            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempA = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand tempN64 = new(OperandKind.Register, OperandType.I64, tempN.Operand.Value);
            Operand tempM64 = new(OperandKind.Register, OperandType.I64, tempM.Operand.Value);
            Operand tempA64 = new(OperandKind.Register, OperandType.I64, tempA.Operand.Value);

            ScopedRegister swapTemp = default;

            if (x)
            {
                swapTemp = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                context.Arm64Assembler.Ror(swapTemp.Operand, rmOperand, InstEmitCommon.Const(16));

                rmOperand = swapTemp.Operand;
            }

            context.Arm64Assembler.Sxth(tempN64, rnOperand);
            context.Arm64Assembler.Sxth(tempM64, rmOperand);

            context.Arm64Assembler.Mul(rdLoOperand64, tempN64, tempM64);

            context.Arm64Assembler.Asr(tempN.Operand, rnOperand, InstEmitCommon.Const(16));
            context.Arm64Assembler.Asr(tempM.Operand, rmOperand, InstEmitCommon.Const(16));

            if (add)
            {
                context.Arm64Assembler.Smaddl(rdLoOperand64, tempN.Operand, tempM.Operand, rdLoOperand64);
            }
            else
            {
                context.Arm64Assembler.Smsubl(rdLoOperand64, tempN.Operand, tempM.Operand, rdLoOperand64);
            }

            context.Arm64Assembler.Lsl(tempA64, rdHiOperand64, InstEmitCommon.Const(32));
            context.Arm64Assembler.Orr(tempA64, tempA64, rdLoOperand);

            context.Arm64Assembler.Add(rdLoOperand64, rdLoOperand64, tempA64);

            if (rdLo != rdHi)
            {
                context.Arm64Assembler.Lsr(rdHiOperand64, rdLoOperand64, InstEmitCommon.Const(32));
            }

            context.Arm64Assembler.Mov(rdLoOperand, rdLoOperand); // Zero-extend.

            if (x)
            {
                swapTemp.Dispose();
            }
        }

        private static void EmitSmmlaSmmls(CodeGenContext context, uint rd, uint rn, uint rm, uint ra, bool r, bool add)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);
            Operand raOperand = InstEmitCommon.GetInputGpr(context, ra);

            Operand rdOperand64 = new(OperandKind.Register, OperandType.I64, rdOperand.Value);
            Operand raOperand64 = new(OperandKind.Register, OperandType.I64, raOperand.Value);

            using ScopedRegister tempA = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand tempA64 = new(OperandKind.Register, OperandType.I64, tempA.Operand.Value);

            context.Arm64Assembler.Lsl(tempA64, raOperand64, InstEmitCommon.Const(32));

            if (add)
            {
                context.Arm64Assembler.Smaddl(rdOperand64, rnOperand, rmOperand, tempA64);
            }
            else
            {
                context.Arm64Assembler.Smsubl(rdOperand64, rnOperand, rmOperand, tempA64);
            }

            if (r)
            {
                context.Arm64Assembler.Mov(tempA.Operand, 0x80000000u);
                context.Arm64Assembler.Add(rdOperand64, rdOperand64, tempA64);
            }

            context.Arm64Assembler.Lsr(rdOperand64, rdOperand64, InstEmitCommon.Const(32));
        }

        private static void EmitSmuadSmusd(CodeGenContext context, uint rd, uint rn, uint rm, bool x, bool add)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            Operand rdOperand64 = new(OperandKind.Register, OperandType.I64, rdOperand.Value);

            using ScopedRegister tempN = context.RegisterAllocator.AllocateTempGprRegisterScoped();
            using ScopedRegister tempM = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand tempN64 = new(OperandKind.Register, OperandType.I64, tempN.Operand.Value);
            Operand tempM64 = new(OperandKind.Register, OperandType.I64, tempM.Operand.Value);

            ScopedRegister swapTemp = default;

            if (x)
            {
                swapTemp = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                context.Arm64Assembler.Ror(swapTemp.Operand, rmOperand, InstEmitCommon.Const(16));

                rmOperand = swapTemp.Operand;
            }

            context.Arm64Assembler.Sxth(tempN64, rnOperand);
            context.Arm64Assembler.Sxth(tempM64, rmOperand);

            context.Arm64Assembler.Mul(rdOperand64, tempN64, tempM64);

            context.Arm64Assembler.Asr(tempN.Operand, rnOperand, InstEmitCommon.Const(16));
            context.Arm64Assembler.Asr(tempM.Operand, rmOperand, InstEmitCommon.Const(16));

            if (add)
            {
                context.Arm64Assembler.Smaddl(rdOperand64, tempN.Operand, tempM.Operand, rdOperand64);
            }
            else
            {
                context.Arm64Assembler.Smsubl(rdOperand64, tempN.Operand, tempM.Operand, rdOperand64);
            }

            context.Arm64Assembler.Mov(rdOperand, rdOperand); // Zero-extend.

            if (x)
            {
                swapTemp.Dispose();
            }
        }

        private static void SelectSignedHalfword(CodeGenContext context, Operand dest, Operand source, bool high)
        {
            if (high)
            {
                context.Arm64Assembler.Asr(dest, source, InstEmitCommon.Const(16));
            }
            else
            {
                context.Arm64Assembler.Sxth(dest, source);
            }
        }

        private static void CheckResultOverflow(CodeGenContext context, Operand temp64, Operand result)
        {
            context.Arm64Assembler.Sxtw(temp64, result);
            context.Arm64Assembler.Sub(temp64, temp64, result);

            int branchIndex = context.CodeWriter.InstructionPointer;

            context.Arm64Assembler.Cbz(temp64, 0);

            // Set Q flag if we had an overflow.
            InstEmitSaturate.SetQFlag(context);

            int delta = context.CodeWriter.InstructionPointer - branchIndex;
            context.CodeWriter.WriteInstructionAt(branchIndex, context.CodeWriter.ReadInstructionAt(branchIndex) | (uint)((delta & 0x7ffff) << 5));
        }
    }
}
