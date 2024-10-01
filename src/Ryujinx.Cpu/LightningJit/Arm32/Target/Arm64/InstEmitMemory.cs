using ARMeilleure.Memory;
using Ryujinx.Cpu.LightningJit.CodeGen;
using Ryujinx.Cpu.LightningJit.CodeGen.Arm64;
using System;
using System.Diagnostics;
using System.Numerics;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitMemory
    {
        private enum PrefetchType : uint
        {
            Pld = 0,
            Pli = 1,
            Pst = 2,
        }

        public static void Lda(CodeGenContext context, uint rt, uint rn)
        {
            EmitMemoryInstruction(context, rt, rn, isStore: false, context.Arm64Assembler.Ldar);
        }

        public static void Ldab(CodeGenContext context, uint rt, uint rn)
        {
            EmitMemoryInstruction(context, rt, rn, isStore: false, context.Arm64Assembler.Ldarb);
        }

        public static void Ldaex(CodeGenContext context, uint rt, uint rn)
        {
            EmitMemoryInstruction(context, rt, rn, isStore: false, context.Arm64Assembler.Ldaxr);
        }

        public static void Ldaexb(CodeGenContext context, uint rt, uint rn)
        {
            EmitMemoryInstruction(context, rt, rn, isStore: false, context.Arm64Assembler.Ldaxrb);
        }

        public static void Ldaexd(CodeGenContext context, uint rt, uint rt2, uint rn)
        {
            EmitMemoryDWordInstruction(context, rt, rt2, rn, isStore: false, context.Arm64Assembler.Ldaxp);
        }

        public static void Ldaexh(CodeGenContext context, uint rt, uint rn)
        {
            EmitMemoryInstruction(context, rt, rn, isStore: false, context.Arm64Assembler.Ldaxrh);
        }

        public static void Ldah(CodeGenContext context, uint rt, uint rn)
        {
            EmitMemoryInstruction(context, rt, rn, isStore: false, context.Arm64Assembler.Ldarh);
        }

        public static void LdcI(CodeGenContext context, uint rn, int imm, bool p, bool u, bool w)
        {
            // TODO.
        }

        public static void LdcL(CodeGenContext context, uint imm, bool p, bool u, bool w)
        {
            // TODO.
        }

        public static void Ldm(CodeGenContext context, uint rn, uint registerList, bool w)
        {
            Operand baseAddress = InstEmitCommon.GetInputGpr(context, rn);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, baseAddress);

            EmitMemoryMultipleInstructionCore(
                context,
                tempRegister.Operand,
                registerList,
                isStore: false,
                context.Arm64Assembler.LdrRiUn,
                context.Arm64Assembler.LdpRiUn);

            if (w)
            {
                Operand offset = InstEmitCommon.Const(BitOperations.PopCount(registerList) * 4);

                WriteAddShiftOffset(context.Arm64Assembler, baseAddress, baseAddress, offset, true, ArmShiftType.Lsl, 0);
            }
        }

        public static void Ldmda(CodeGenContext context, uint rn, uint registerList, bool w)
        {
            EmitMemoryMultipleDaInstruction(context, rn, registerList, w, isStore: false, context.Arm64Assembler.LdrRiUn, context.Arm64Assembler.LdpRiUn);
        }

        public static void Ldmdb(CodeGenContext context, uint rn, uint registerList, bool w)
        {
            EmitMemoryMultipleDbInstruction(context, rn, registerList, w, isStore: false, context.Arm64Assembler.LdrRiUn, context.Arm64Assembler.LdpRiUn);
        }

        public static void Ldmib(CodeGenContext context, uint rn, uint registerList, bool w)
        {
            EmitMemoryMultipleIbInstruction(context, rn, registerList, w, isStore: false, context.Arm64Assembler.LdrRiUn, context.Arm64Assembler.LdpRiUn);
        }

        public static void LdrI(CodeGenContext context, uint rt, uint rn, int imm, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 2, p, u, w, isStore: false, context.Arm64Assembler.LdrRiUn, context.Arm64Assembler.Ldur);
        }

        public static void LdrL(CodeGenContext context, uint rt, uint imm, bool p, bool u, bool w)
        {
            EmitMemoryLiteralInstruction(context, rt, imm, 2, p, u, w, context.Arm64Assembler.LdrRiUn);
        }

        public static void LdrR(CodeGenContext context, uint rt, uint rn, uint rm, uint sType, uint imm5, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, rm, sType, imm5, p, u, w, isStore: false, context.Arm64Assembler.LdrRiUn, context.Arm64Assembler.Ldur);
        }

        public static void LdrbI(CodeGenContext context, uint rt, uint rn, int imm, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 0, p, u, w, isStore: false, context.Arm64Assembler.LdrbRiUn, context.Arm64Assembler.Ldurb);
        }

        public static void LdrbL(CodeGenContext context, uint rt, uint imm, bool p, bool u, bool w)
        {
            EmitMemoryLiteralInstruction(context, rt, imm, 0, p, u, w, context.Arm64Assembler.LdrbRiUn);
        }

        public static void LdrbR(CodeGenContext context, uint rt, uint rn, uint rm, uint sType, uint imm5, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, rm, sType, imm5, p, u, w, isStore: false, context.Arm64Assembler.LdrbRiUn, context.Arm64Assembler.Ldurb);
        }

        public static void LdrbtI(CodeGenContext context, uint rt, uint rn, int imm, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 0, !postIndex, u, false, isStore: false, context.Arm64Assembler.LdrbRiUn, context.Arm64Assembler.Ldurb);
        }

        public static void LdrbtR(CodeGenContext context, uint rt, uint rn, uint rm, uint sType, uint imm5, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, rm, sType, imm5, !postIndex, u, false, isStore: false, context.Arm64Assembler.LdrbRiUn, context.Arm64Assembler.Ldurb);
        }

        public static void LdrdI(CodeGenContext context, uint rt, uint rt2, uint rn, uint imm, bool p, bool u, bool w)
        {
            EmitMemoryDWordInstructionI(context, rt, rt2, rn, imm, p, u, w, isStore: false, context.Arm64Assembler.LdpRiUn);
        }

        public static void LdrdL(CodeGenContext context, uint rt, uint rt2, uint imm, bool p, bool u, bool w)
        {
            EmitMemoryDWordLiteralInstruction(context, rt, rt2, imm, p, u, w, context.Arm64Assembler.LdpRiUn);
        }

        public static void LdrdR(CodeGenContext context, uint rt, uint rt2, uint rn, uint rm, bool p, bool u, bool w)
        {
            EmitMemoryDWordInstructionR(context, rt, rt2, rn, rm, p, u, w, isStore: false, context.Arm64Assembler.LdpRiUn);
        }

        public static void Ldrex(CodeGenContext context, uint rt, uint rn)
        {
            EmitMemoryInstruction(context, rt, rn, isStore: false, context.Arm64Assembler.Ldaxr);
        }

        public static void Ldrexb(CodeGenContext context, uint rt, uint rn)
        {
            EmitMemoryInstruction(context, rt, rn, isStore: false, context.Arm64Assembler.Ldaxrb);
        }

        public static void Ldrexd(CodeGenContext context, uint rt, uint rt2, uint rn)
        {
            EmitMemoryDWordInstruction(context, rt, rt2, rn, isStore: false, context.Arm64Assembler.Ldaxp);
        }

        public static void Ldrexh(CodeGenContext context, uint rt, uint rn)
        {
            EmitMemoryInstruction(context, rt, rn, isStore: false, context.Arm64Assembler.Ldaxrh);
        }

        public static void LdrhI(CodeGenContext context, uint rt, uint rn, int imm, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 1, p, u, w, isStore: false, context.Arm64Assembler.LdrhRiUn, context.Arm64Assembler.Ldurh);
        }

        public static void LdrhL(CodeGenContext context, uint rt, uint imm, bool p, bool u, bool w)
        {
            EmitMemoryLiteralInstruction(context, rt, imm, 1, p, u, w, context.Arm64Assembler.LdrhRiUn);
        }

        public static void LdrhR(CodeGenContext context, uint rt, uint rn, uint rm, uint sType, uint imm5, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, rm, sType, imm5, p, u, w, isStore: false, context.Arm64Assembler.LdrhRiUn, context.Arm64Assembler.Ldurh);
        }

        public static void LdrhtI(CodeGenContext context, uint rt, uint rn, int imm, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 1, !postIndex, u, false, isStore: false, context.Arm64Assembler.LdrhRiUn, context.Arm64Assembler.Ldurh);
        }

        public static void LdrhtR(CodeGenContext context, uint rt, uint rn, uint rm, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, rm, 0, 0, !postIndex, u, false, isStore: false, context.Arm64Assembler.LdrhRiUn, context.Arm64Assembler.Ldurh);
        }

        public static void LdrsbI(CodeGenContext context, uint rt, uint rn, int imm, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 0, p, u, w, isStore: false, context.Arm64Assembler.LdrsbRiUn, context.Arm64Assembler.Ldursb);
        }

        public static void LdrsbL(CodeGenContext context, uint rt, uint imm, bool p, bool u, bool w)
        {
            EmitMemoryLiteralInstruction(context, rt, imm, 0, p, u, w, context.Arm64Assembler.LdrsbRiUn);
        }

        public static void LdrsbR(CodeGenContext context, uint rt, uint rn, uint rm, uint sType, uint imm5, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, rm, sType, imm5, p, u, w, isStore: false, context.Arm64Assembler.LdrsbRiUn, context.Arm64Assembler.Ldursb);
        }

        public static void LdrsbtI(CodeGenContext context, uint rt, uint rn, int imm, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 0, !postIndex, u, false, isStore: false, context.Arm64Assembler.LdrsbRiUn, context.Arm64Assembler.Ldursb);
        }

        public static void LdrsbtR(CodeGenContext context, uint rt, uint rn, uint rm, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, rm, 0, 0, !postIndex, u, false, isStore: false, context.Arm64Assembler.LdrsbRiUn, context.Arm64Assembler.Ldursb);
        }

        public static void LdrshI(CodeGenContext context, uint rt, uint rn, int imm, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 1, p, u, w, isStore: false, context.Arm64Assembler.LdrshRiUn, context.Arm64Assembler.Ldursh);
        }

        public static void LdrshL(CodeGenContext context, uint rt, uint imm, bool p, bool u, bool w)
        {
            EmitMemoryLiteralInstruction(context, rt, imm, 1, p, u, w, context.Arm64Assembler.LdrshRiUn);
        }

        public static void LdrshR(CodeGenContext context, uint rt, uint rn, uint rm, uint sType, uint imm5, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, rm, sType, imm5, p, u, w, isStore: false, context.Arm64Assembler.LdrshRiUn, context.Arm64Assembler.Ldursh);
        }

        public static void LdrshtI(CodeGenContext context, uint rt, uint rn, int imm, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 1, !postIndex, u, false, isStore: false, context.Arm64Assembler.LdrshRiUn, context.Arm64Assembler.Ldursh);
        }

        public static void LdrshtR(CodeGenContext context, uint rt, uint rn, uint rm, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, rm, 0, 0, !postIndex, u, false, isStore: false, context.Arm64Assembler.LdrshRiUn, context.Arm64Assembler.Ldursh);
        }

        public static void LdrtI(CodeGenContext context, uint rt, uint rn, int imm, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 2, !postIndex, u, false, isStore: false, context.Arm64Assembler.LdrRiUn, context.Arm64Assembler.Ldur);
        }

        public static void LdrtR(CodeGenContext context, uint rt, uint rn, uint rm, uint sType, uint imm5, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, rm, sType, imm5, !postIndex, u, false, isStore: false, context.Arm64Assembler.LdrRiUn, context.Arm64Assembler.Ldur);
        }

        public static void PldI(CodeGenContext context, uint rn, uint imm, bool u, bool r)
        {
            EmitMemoryPrefetchInstruction(context, rn, imm, u, r ? PrefetchType.Pld : PrefetchType.Pst);
        }

        public static void PldL(CodeGenContext context, uint imm, bool u)
        {
            EmitMemoryPrefetchLiteralInstruction(context, imm, u, PrefetchType.Pld);
        }

        public static void PldR(CodeGenContext context, uint rn, uint rm, uint sType, uint imm5, bool u, bool r)
        {
            EmitMemoryPrefetchInstruction(context, rn, rm, u, sType, imm5, r ? PrefetchType.Pld : PrefetchType.Pst);
        }

        public static void PliI(CodeGenContext context, uint rn, uint imm, bool u)
        {
            EmitMemoryPrefetchInstruction(context, rn, imm, u, PrefetchType.Pli);
        }

        public static void PliL(CodeGenContext context, uint imm, bool u)
        {
            EmitMemoryPrefetchLiteralInstruction(context, imm, u, PrefetchType.Pli);
        }

        public static void PliR(CodeGenContext context, uint rn, uint rm, uint sType, uint imm5, bool u)
        {
            EmitMemoryPrefetchInstruction(context, rn, rm, u, sType, imm5, PrefetchType.Pli);
        }

        public static void Stc(CodeGenContext context, uint rn, int imm, bool p, bool u, bool w)
        {
            // TODO.
        }

        public static void Stl(CodeGenContext context, uint rt, uint rn)
        {
            EmitMemoryInstruction(context, rt, rn, isStore: true, context.Arm64Assembler.Stlr);
        }

        public static void Stlb(CodeGenContext context, uint rt, uint rn)
        {
            EmitMemoryInstruction(context, rt, rn, isStore: true, context.Arm64Assembler.Stlrb);
        }

        public static void Stlex(CodeGenContext context, uint rd, uint rt, uint rn)
        {
            EmitMemoryStrexInstruction(context, rd, rt, rn, context.Arm64Assembler.Stlxr);
        }

        public static void Stlexb(CodeGenContext context, uint rd, uint rt, uint rn)
        {
            EmitMemoryStrexInstruction(context, rd, rt, rn, context.Arm64Assembler.Stlxrb);
        }

        public static void Stlexd(CodeGenContext context, uint rd, uint rt, uint rt2, uint rn)
        {
            EmitMemoryDWordStrexInstruction(context, rd, rt, rt2, rn, context.Arm64Assembler.Stlxp);
        }

        public static void Stlexh(CodeGenContext context, uint rd, uint rt, uint rn)
        {
            EmitMemoryStrexInstruction(context, rd, rt, rn, context.Arm64Assembler.Stlxrh);
        }

        public static void Stlh(CodeGenContext context, uint rt, uint rn)
        {
            EmitMemoryInstruction(context, rt, rn, isStore: true, context.Arm64Assembler.Stlrh);
        }

        public static void Stm(CodeGenContext context, uint rn, uint registerList, bool w)
        {
            Operand baseAddress = InstEmitCommon.GetInputGpr(context, rn);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, baseAddress);

            EmitMemoryMultipleInstructionCore(
                context,
                tempRegister.Operand,
                registerList,
                isStore: true,
                context.Arm64Assembler.StrRiUn,
                context.Arm64Assembler.StpRiUn);

            if (w)
            {
                Operand offset = InstEmitCommon.Const(BitOperations.PopCount(registerList) * 4);

                WriteAddShiftOffset(context.Arm64Assembler, baseAddress, baseAddress, offset, true, ArmShiftType.Lsl, 0);
            }
        }

        public static void Stmda(CodeGenContext context, uint rn, uint registerList, bool w)
        {
            EmitMemoryMultipleDaInstruction(context, rn, registerList, w, isStore: true, context.Arm64Assembler.StrRiUn, context.Arm64Assembler.StpRiUn);
        }

        public static void Stmdb(CodeGenContext context, uint rn, uint registerList, bool w)
        {
            EmitMemoryMultipleDbInstruction(context, rn, registerList, w, isStore: true, context.Arm64Assembler.StrRiUn, context.Arm64Assembler.StpRiUn);
        }

        public static void Stmib(CodeGenContext context, uint rn, uint registerList, bool w)
        {
            EmitMemoryMultipleIbInstruction(context, rn, registerList, w, isStore: true, context.Arm64Assembler.StrRiUn, context.Arm64Assembler.StpRiUn);
        }

        public static void StrI(CodeGenContext context, uint rt, uint rn, int imm, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 2, p, u, w, isStore: true, context.Arm64Assembler.StrRiUn, context.Arm64Assembler.Stur);
        }

        public static void StrR(CodeGenContext context, uint rt, uint rn, uint rm, uint sType, uint imm5, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, rm, sType, imm5, p, u, w, isStore: true, context.Arm64Assembler.StrRiUn, context.Arm64Assembler.Stur);
        }

        public static void StrbI(CodeGenContext context, uint rt, uint rn, int imm, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 0, p, u, w, isStore: true, context.Arm64Assembler.StrbRiUn, context.Arm64Assembler.Sturb);
        }

        public static void StrbR(CodeGenContext context, uint rt, uint rn, uint rm, uint sType, uint imm5, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, rm, sType, imm5, p, u, w, isStore: true, context.Arm64Assembler.StrbRiUn, context.Arm64Assembler.Sturb);
        }

        public static void StrbtI(CodeGenContext context, uint rt, uint rn, int imm, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 0, !postIndex, u, false, isStore: true, context.Arm64Assembler.StrbRiUn, context.Arm64Assembler.Sturb);
        }

        public static void StrbtR(CodeGenContext context, uint rt, uint rn, uint rm, uint sType, uint imm5, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, rm, sType, imm5, !postIndex, u, false, isStore: true, context.Arm64Assembler.StrbRiUn, context.Arm64Assembler.Sturb);
        }

        public static void StrdI(CodeGenContext context, uint rt, uint rt2, uint rn, uint imm, bool p, bool u, bool w)
        {
            EmitMemoryDWordInstructionI(context, rt, rt2, rn, imm, p, u, w, isStore: true, context.Arm64Assembler.StpRiUn);
        }

        public static void StrdR(CodeGenContext context, uint rt, uint rt2, uint rn, uint rm, bool p, bool u, bool w)
        {
            EmitMemoryDWordInstructionR(context, rt, rt2, rn, rm, p, u, w, isStore: true, context.Arm64Assembler.StpRiUn);
        }

        public static void Strex(CodeGenContext context, uint rd, uint rt, uint rn)
        {
            EmitMemoryStrexInstruction(context, rd, rt, rn, context.Arm64Assembler.Stlxr);
        }

        public static void Strexb(CodeGenContext context, uint rd, uint rt, uint rn)
        {
            EmitMemoryStrexInstruction(context, rd, rt, rn, context.Arm64Assembler.Stlxrb);
        }

        public static void Strexd(CodeGenContext context, uint rd, uint rt, uint rt2, uint rn)
        {
            EmitMemoryDWordStrexInstruction(context, rd, rt, rt2, rn, context.Arm64Assembler.Stlxp);
        }

        public static void Strexh(CodeGenContext context, uint rd, uint rt, uint rn)
        {
            EmitMemoryStrexInstruction(context, rd, rt, rn, context.Arm64Assembler.Stlxrh);
        }

        public static void StrhI(CodeGenContext context, uint rt, uint rn, int imm, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 1, p, u, w, isStore: true, context.Arm64Assembler.StrhRiUn, context.Arm64Assembler.Sturh);
        }

        public static void StrhR(CodeGenContext context, uint rt, uint rn, uint rm, uint sType, uint imm5, bool p, bool u, bool w)
        {
            EmitMemoryInstruction(context, rt, rn, rm, sType, imm5, p, u, w, isStore: true, context.Arm64Assembler.StrhRiUn, context.Arm64Assembler.Sturh);
        }

        public static void StrhtI(CodeGenContext context, uint rt, uint rn, int imm, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 1, !postIndex, u, false, isStore: true, context.Arm64Assembler.StrhRiUn, context.Arm64Assembler.Sturh);
        }

        public static void StrhtR(CodeGenContext context, uint rt, uint rn, uint rm, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, rm, 0, 0, !postIndex, u, false, isStore: true, context.Arm64Assembler.StrhRiUn, context.Arm64Assembler.Sturh);
        }

        public static void StrtI(CodeGenContext context, uint rt, uint rn, int imm, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, imm, 2, !postIndex, u, false, isStore: true, context.Arm64Assembler.StrRiUn, context.Arm64Assembler.Stur);
        }

        public static void StrtR(CodeGenContext context, uint rt, uint rn, uint rm, uint sType, uint imm5, bool postIndex, bool u)
        {
            EmitMemoryInstruction(context, rt, rn, rm, sType, imm5, !postIndex, u, false, isStore: true, context.Arm64Assembler.StrRiUn, context.Arm64Assembler.Stur);
        }

        private static void EmitMemoryMultipleDaInstruction(
            CodeGenContext context,
            uint rn,
            uint registerList,
            bool w,
            bool isStore,
            Action<Operand, Operand, int> writeInst,
            Action<Operand, Operand, Operand, int> writeInstPair)
        {
            Operand baseAddress = InstEmitCommon.GetInputGpr(context, rn);
            Operand offset;

            if (registerList != 0)
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

                offset = InstEmitCommon.Const(BitOperations.PopCount(registerList) * 4 - 4);

                WriteAddShiftOffset(context.Arm64Assembler, tempRegister.Operand, baseAddress, offset, false, ArmShiftType.Lsl, 0);
                WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, tempRegister.Operand);

                EmitMemoryMultipleInstructionCore(
                    context,
                    tempRegister.Operand,
                    registerList,
                    isStore,
                    writeInst,
                    writeInstPair);
            }

            if (w)
            {
                offset = InstEmitCommon.Const(BitOperations.PopCount(registerList) * 4);

                WriteAddShiftOffset(context.Arm64Assembler, baseAddress, baseAddress, offset, false, ArmShiftType.Lsl, 0);
            }
        }

        private static void EmitMemoryMultipleDbInstruction(
            CodeGenContext context,
            uint rn,
            uint registerList,
            bool w,
            bool isStore,
            Action<Operand, Operand, int> writeInst,
            Action<Operand, Operand, Operand, int> writeInstPair)
        {
            Operand baseAddress = InstEmitCommon.GetInputGpr(context, rn);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand offset = InstEmitCommon.Const(BitOperations.PopCount(registerList) * 4);

            bool writesToRn = (registerList & (1u << (int)rn)) != 0;

            if (w && !writesToRn)
            {
                WriteAddShiftOffset(context.Arm64Assembler, baseAddress, baseAddress, offset, false, ArmShiftType.Lsl, 0);
                WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, baseAddress);
            }
            else
            {
                WriteAddShiftOffset(context.Arm64Assembler, tempRegister.Operand, baseAddress, offset, false, ArmShiftType.Lsl, 0);
                WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, tempRegister.Operand);
            }

            EmitMemoryMultipleInstructionCore(
                context,
                tempRegister.Operand,
                registerList,
                isStore,
                writeInst,
                writeInstPair);

            if (w && writesToRn)
            {
                WriteAddShiftOffset(context.Arm64Assembler, baseAddress, baseAddress, offset, false, ArmShiftType.Lsl, 0);
            }
        }

        private static void EmitMemoryMultipleIbInstruction(
            CodeGenContext context,
            uint rn,
            uint registerList,
            bool w,
            bool isStore,
            Action<Operand, Operand, int> writeInst,
            Action<Operand, Operand, Operand, int> writeInstPair)
        {
            Operand baseAddress = InstEmitCommon.GetInputGpr(context, rn);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand offset = InstEmitCommon.Const(4);

            WriteAddShiftOffset(context.Arm64Assembler, tempRegister.Operand, baseAddress, offset, true, ArmShiftType.Lsl, 0);
            WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, tempRegister.Operand);

            EmitMemoryMultipleInstructionCore(
                context,
                tempRegister.Operand,
                registerList,
                isStore,
                writeInst,
                writeInstPair);

            if (w)
            {
                offset = InstEmitCommon.Const(BitOperations.PopCount(registerList) * 4);

                WriteAddShiftOffset(context.Arm64Assembler, baseAddress, baseAddress, offset, true, ArmShiftType.Lsl, 0);
            }
        }

        private static void EmitMemoryMultipleInstructionCore(
            CodeGenContext context,
            Operand baseAddress,
            uint registerList,
            bool isStore,
            Action<Operand, Operand, int> writeInst,
            Action<Operand, Operand, Operand, int> writeInstPair)
        {
            uint registers = registerList;
            int offs = 0;

            while (registers != 0)
            {
                int regIndex = BitOperations.TrailingZeroCount(registers);

                registers &= ~(1u << regIndex);

                Operand rt = isStore
                    ? InstEmitCommon.GetInputGpr(context, (uint)regIndex)
                    : InstEmitCommon.GetOutputGpr(context, (uint)regIndex);

                int regIndex2 = BitOperations.TrailingZeroCount(registers);
                if (regIndex2 < 32)
                {
                    registers &= ~(1u << regIndex2);

                    Operand rt2 = isStore
                        ? InstEmitCommon.GetInputGpr(context, (uint)regIndex2)
                        : InstEmitCommon.GetOutputGpr(context, (uint)regIndex2);

                    writeInstPair(rt, rt2, baseAddress, offs);

                    offs += 8;
                }
                else
                {
                    writeInst(rt, baseAddress, offs);

                    offs += 4;
                }
            }
        }

        private static void EmitMemoryInstruction(
            CodeGenContext context,
            uint rt,
            uint rn,
            int imm,
            int scale,
            bool p,
            bool u,
            bool w,
            bool isStore,
            Action<Operand, Operand, int> writeInst,
            Action<Operand, Operand, int> writeInstUnscaled)
        {
            Operand rtOperand = isStore ? InstEmitCommon.GetInputGpr(context, rt) : InstEmitCommon.GetOutputGpr(context, rt);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand offset = InstEmitCommon.Const(imm);

            EmitMemoryInstruction(context, writeInst, writeInstUnscaled, rtOperand, rnOperand, offset, scale, p, u, w);
        }

        private static void EmitMemoryInstruction(
            CodeGenContext context,
            uint rt,
            uint rn,
            uint rm,
            uint sType,
            uint imm5,
            bool p,
            bool u,
            bool w,
            bool isStore,
            Action<Operand, Operand, int> writeInst,
            Action<Operand, Operand, int> writeInstUnscaled)
        {
            Operand rtOperand = isStore ? InstEmitCommon.GetInputGpr(context, rt) : InstEmitCommon.GetOutputGpr(context, rt);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            EmitMemoryInstruction(context, writeInst, writeInstUnscaled, rtOperand, rnOperand, rmOperand, 0, p, u, w, (ArmShiftType)sType, (int)imm5);
        }

        private static void EmitMemoryInstruction(CodeGenContext context, uint rt, uint rn, bool isStore, Action<Operand, Operand> action)
        {
            Operand rtOperand = isStore ? InstEmitCommon.GetInputGpr(context, rt) : InstEmitCommon.GetOutputGpr(context, rt);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, rnOperand);

            action(rtOperand, tempRegister.Operand);
        }

        private static void EmitMemoryDWordInstruction(CodeGenContext context, uint rt, uint rt2, uint rn, bool isStore, Action<Operand, Operand, Operand> action)
        {
            Operand rtOperand = isStore ? InstEmitCommon.GetInputGpr(context, rt) : InstEmitCommon.GetOutputGpr(context, rt);
            Operand rt2Operand = isStore ? InstEmitCommon.GetInputGpr(context, rt2) : InstEmitCommon.GetOutputGpr(context, rt2);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, rnOperand);

            action(rtOperand, rt2Operand, tempRegister.Operand);
        }

        private static void EmitMemoryDWordInstructionI(
            CodeGenContext context,
            uint rt,
            uint rt2,
            uint rn,
            uint imm,
            bool p,
            bool u,
            bool w,
            bool isStore,
            Action<Operand, Operand, Operand, int> action)
        {
            Operand rtOperand = isStore ? InstEmitCommon.GetInputGpr(context, rt) : InstEmitCommon.GetOutputGpr(context, rt);
            Operand rt2Operand = isStore ? InstEmitCommon.GetInputGpr(context, rt2) : InstEmitCommon.GetOutputGpr(context, rt2);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand offset = InstEmitCommon.Const((int)imm);

            EmitMemoryDWordInstruction(context, rtOperand, rt2Operand, rnOperand, offset, p, u, w, action);
        }

        private static void EmitMemoryDWordInstructionR(
            CodeGenContext context,
            uint rt,
            uint rt2,
            uint rn,
            uint rm,
            bool p,
            bool u,
            bool w,
            bool isStore,
            Action<Operand, Operand, Operand, int> action)
        {
            Operand rtOperand = isStore ? InstEmitCommon.GetInputGpr(context, rt) : InstEmitCommon.GetOutputGpr(context, rt);
            Operand rt2Operand = isStore ? InstEmitCommon.GetInputGpr(context, rt2) : InstEmitCommon.GetOutputGpr(context, rt2);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            EmitMemoryDWordInstruction(context, rtOperand, rt2Operand, rnOperand, rmOperand, p, u, w, action);
        }

        private static void EmitMemoryDWordInstruction(
            CodeGenContext context,
            Operand rt,
            Operand rt2,
            Operand baseAddress,
            Operand offset,
            bool index,
            bool add,
            bool wBack,
            Action<Operand, Operand, Operand, int> action)
        {
            Assembler asm = context.Arm64Assembler;
            RegisterAllocator regAlloc = context.RegisterAllocator;

            if (index && !wBack)
            {
                // Offset.

                using ScopedRegister tempRegister = regAlloc.AllocateTempGprRegisterScoped();

                int signedOffs = add ? offset.AsInt32() : -offset.AsInt32();
                int offs = 0;

                if (offset.Kind == OperandKind.Constant && offset.Value == 0)
                {
                    WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, baseAddress);
                }
                else if (offset.Kind == OperandKind.Constant && CanFoldDWordOffset(context.MemoryManagerType, signedOffs))
                {
                    WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, baseAddress);
                    offs = signedOffs;
                }
                else
                {
                    WriteAddShiftOffset(asm, tempRegister.Operand, baseAddress, offset, add, ArmShiftType.Lsl, 0);
                    WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, tempRegister.Operand);
                }

                action(rt, rt2, tempRegister.Operand, offs);
            }
            else if (context.IsThumb ? !index && wBack : !index && !wBack)
            {
                // Post-indexed.

                using ScopedRegister tempRegister = regAlloc.AllocateTempGprRegisterScoped();

                WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, baseAddress);

                action(rt, rt2, tempRegister.Operand, 0);

                WriteAddShiftOffset(asm, baseAddress, baseAddress, offset, add, ArmShiftType.Lsl, 0);
            }
            else if (index && wBack)
            {
                // Pre-indexed.

                using ScopedRegister tempRegister = regAlloc.AllocateTempGprRegisterScoped();

                if (rt.Value == baseAddress.Value)
                {
                    // If Rt and Rn are the same register, ensure we perform the write back after the read/write.

                    WriteAddShiftOffset(asm, tempRegister.Operand, baseAddress, offset, add, ArmShiftType.Lsl, 0);
                    WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, tempRegister.Operand);

                    action(rt, rt2, tempRegister.Operand, 0);

                    context.Arm64Assembler.Mov(baseAddress, tempRegister.Operand);
                }
                else
                {
                    WriteAddShiftOffset(asm, baseAddress, baseAddress, offset, add, ArmShiftType.Lsl, 0);
                    WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, baseAddress);

                    action(rt, rt2, tempRegister.Operand, 0);
                }
            }
        }

        private static void EmitMemoryStrexInstruction(CodeGenContext context, uint rd, uint rt, uint rn, Action<Operand, Operand, Operand> action)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rtOperand = InstEmitCommon.GetInputGpr(context, rt);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, rnOperand);

            action(rdOperand, rtOperand, tempRegister.Operand);
        }

        private static void EmitMemoryDWordStrexInstruction(CodeGenContext context, uint rd, uint rt, uint rt2, uint rn, Action<Operand, Operand, Operand, Operand> action)
        {
            Operand rdOperand = InstEmitCommon.GetOutputGpr(context, rd);
            Operand rtOperand = InstEmitCommon.GetInputGpr(context, rt);
            Operand rt2Operand = InstEmitCommon.GetInputGpr(context, rt2);
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, rnOperand);

            action(rdOperand, rtOperand, rt2Operand, tempRegister.Operand);
        }

        private static void EmitMemoryInstruction(
            CodeGenContext context,
            Action<Operand, Operand, int> writeInst,
            Action<Operand, Operand, int> writeInstUnscaled,
            Operand rt,
            Operand baseAddress,
            Operand offset,
            int scale,
            bool index,
            bool add,
            bool wBack,
            ArmShiftType shiftType = ArmShiftType.Lsl,
            int shift = 0)
        {
            Assembler asm = context.Arm64Assembler;
            RegisterAllocator regAlloc = context.RegisterAllocator;

            if (index && !wBack)
            {
                // Offset.

                using ScopedRegister tempRegister = regAlloc.AllocateTempGprRegisterScoped();

                int signedOffs = add ? offset.AsInt32() : -offset.AsInt32();
                int offs = 0;
                bool unscaled = false;

                if (offset.Kind == OperandKind.Constant && offset.Value == 0)
                {
                    WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, baseAddress);
                }
                else if (offset.Kind == OperandKind.Constant && shift == 0 && CanFoldOffset(context.MemoryManagerType, signedOffs, scale, writeInstUnscaled != null, out unscaled))
                {
                    WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, baseAddress);
                    offs = signedOffs;
                }
                else
                {
                    WriteAddShiftOffset(asm, tempRegister.Operand, baseAddress, offset, add, shiftType, shift);
                    WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, tempRegister.Operand);
                }

                if (unscaled)
                {
                    writeInstUnscaled(rt, tempRegister.Operand, offs);
                }
                else
                {
                    writeInst(rt, tempRegister.Operand, offs);
                }
            }
            else if (context.IsThumb ? !index && wBack : !index && !wBack)
            {
                // Post-indexed.

                if (rt.Type == offset.Type && rt.Value == offset.Value)
                {
                    // If Rt and Rm are the same register, we must ensure we add the register offset (Rm)
                    // before the value is loaded, otherwise we will be adding the wrong value.

                    if (rt.Type != baseAddress.Type || rt.Value != baseAddress.Value)
                    {
                        using ScopedRegister tempRegister = regAlloc.AllocateTempGprRegisterScoped();

                        WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, baseAddress);
                        WriteAddShiftOffset(asm, baseAddress, baseAddress, offset, add, shiftType, shift);

                        writeInst(rt, tempRegister.Operand, 0);
                    }
                    else
                    {
                        using ScopedRegister tempRegister = regAlloc.AllocateTempGprRegisterScoped();
                        using ScopedRegister tempRegister2 = regAlloc.AllocateTempGprRegisterScoped();

                        WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, baseAddress);
                        WriteAddShiftOffset(asm, tempRegister2.Operand, baseAddress, offset, add, shiftType, shift);

                        writeInst(rt, tempRegister.Operand, 0);

                        asm.Mov(baseAddress, tempRegister2.Operand);
                    }
                }
                else
                {
                    using ScopedRegister tempRegister = regAlloc.AllocateTempGprRegisterScoped();

                    WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, baseAddress);

                    writeInst(rt, tempRegister.Operand, 0);

                    WriteAddShiftOffset(asm, baseAddress, baseAddress, offset, add, shiftType, shift);
                }
            }
            else if (index && wBack)
            {
                // Pre-indexed.

                using ScopedRegister tempRegister = regAlloc.AllocateTempGprRegisterScoped();

                if (rt.Value == baseAddress.Value)
                {
                    // If Rt and Rn are the same register, ensure we perform the write back after the read/write.

                    WriteAddShiftOffset(asm, tempRegister.Operand, baseAddress, offset, add, shiftType, shift);
                    WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, tempRegister.Operand);

                    writeInst(rt, tempRegister.Operand, 0);

                    context.Arm64Assembler.Mov(baseAddress, tempRegister.Operand);
                }
                else
                {
                    WriteAddShiftOffset(asm, baseAddress, baseAddress, offset, add, shiftType, shift);
                    WriteAddressTranslation(context.MemoryManagerType, regAlloc, asm, tempRegister.Operand, baseAddress);

                    writeInst(rt, tempRegister.Operand, 0);
                }
            }
            else
            {
                Debug.Fail($"Invalid pre-index and write-back combination.");
            }
        }

        private static void EmitMemoryLiteralInstruction(CodeGenContext context, uint rt, uint imm, int scale, bool p, bool u, bool w, Action<Operand, Operand, int> action)
        {
            if (!p || w)
            {
                EmitMemoryInstruction(context, rt, RegisterUtils.PcRegister, (int)imm, scale, p, u, w, isStore: false, action, null);

                return;
            }

            Operand rtOperand = InstEmitCommon.GetOutputGpr(context, rt);
            uint targetAddress = context.Pc & ~3u;

            if (u)
            {
                targetAddress += imm;
            }
            else
            {
                targetAddress -= imm;
            }

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, targetAddress);

            action(rtOperand, tempRegister.Operand, 0);
        }

        private static void EmitMemoryDWordLiteralInstruction(CodeGenContext context, uint rt, uint rt2, uint imm, bool p, bool u, bool w, Action<Operand, Operand, Operand, int> action)
        {
            if (!p || w)
            {
                EmitMemoryDWordInstructionI(context, rt, rt2, RegisterUtils.PcRegister, imm, p, u, w, isStore: false, action);

                return;
            }

            Operand rtOperand = InstEmitCommon.GetOutputGpr(context, rt);
            Operand rt2Operand = InstEmitCommon.GetOutputGpr(context, rt2);
            uint targetAddress = context.Pc & ~3u;

            if (u)
            {
                targetAddress += imm;
            }
            else
            {
                targetAddress -= imm;
            }

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, targetAddress);

            action(rtOperand, rt2Operand, tempRegister.Operand, 0);
        }

        private static void EmitMemoryPrefetchInstruction(CodeGenContext context, uint rn, uint imm, bool u, PrefetchType type)
        {
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            int signedOffs = u ? (int)imm : -(int)imm;
            int offs = 0;
            bool unscaled = false;

            if (imm == 0)
            {
                WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, rnOperand);
            }
            else if (CanFoldOffset(context.MemoryManagerType, signedOffs, 3, true, out unscaled))
            {
                WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, rnOperand);
                offs = signedOffs;
            }
            else
            {
                WriteAddShiftOffset(context.Arm64Assembler, tempRegister.Operand, rnOperand, InstEmitCommon.Const((int)imm), u, ArmShiftType.Lsl, 0);
                WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, tempRegister.Operand);
            }

            if (unscaled)
            {
                context.Arm64Assembler.Prfum(tempRegister.Operand, offs, (uint)type, 0, 0);
            }
            else
            {
                context.Arm64Assembler.PrfmI(tempRegister.Operand, offs, (uint)type, 0, 0);
            }
        }

        private static void EmitMemoryPrefetchInstruction(CodeGenContext context, uint rn, uint rm, bool u, uint sType, uint shift, PrefetchType type)
        {
            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);
            Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            WriteAddShiftOffset(context.Arm64Assembler, tempRegister.Operand, rnOperand, rmOperand, u, (ArmShiftType)sType, (int)shift);
            WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, tempRegister.Operand);

            context.Arm64Assembler.PrfmI(tempRegister.Operand, 0, (uint)type, 0, 0);
        }

        private static void EmitMemoryPrefetchLiteralInstruction(CodeGenContext context, uint imm, bool u, PrefetchType type)
        {
            uint targetAddress = context.Pc & ~3u;

            if (u)
            {
                targetAddress += imm;
            }
            else
            {
                targetAddress -= imm;
            }

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, targetAddress);

            context.Arm64Assembler.PrfmI(tempRegister.Operand, 0, (uint)type, 0, 0);
        }

        public static bool CanFoldOffset(MemoryManagerType mmType, int offset, int scale, bool hasUnscaled, out bool unscaled)
        {
            if (mmType != MemoryManagerType.HostMappedUnsafe)
            {
                unscaled = false;

                return false;
            }

            int mask = (1 << scale) - 1;

            if ((offset & mask) == 0 && offset >= 0 && offset < 0x1000)
            {
                // We can use the unsigned, scaled encoding.

                unscaled = false;

                return true;
            }

            // Check if we can use the signed, unscaled encoding.

            unscaled = hasUnscaled && offset >= -0x100 && offset < 0x100;

            return unscaled;
        }

        private static bool CanFoldDWordOffset(MemoryManagerType mmType, int offset)
        {
            if (mmType != MemoryManagerType.HostMappedUnsafe)
            {
                return false;
            }

            return offset >= 0 && offset < 0x40 && (offset & 3) == 0;
        }

        private static void WriteAddressTranslation(MemoryManagerType mmType, RegisterAllocator regAlloc, in Assembler asm, Operand destination, uint guestAddress)
        {
            asm.Mov(destination, guestAddress);

            WriteAddressTranslation(mmType, regAlloc, asm, destination, destination);
        }

        public static void WriteAddressTranslation(MemoryManagerType mmType, RegisterAllocator regAlloc, in Assembler asm, Operand destination, Operand guestAddress)
        {
            Operand destination64 = new(destination.Kind, OperandType.I64, destination.Value);
            Operand basePointer = new(regAlloc.FixedPageTableRegister, RegisterType.Integer, OperandType.I64);

            // We don't need to mask the address for the safe mode, since it is already naturally limited to 32-bit
            // and can never reach out of the guest address space.

            if (mmType.IsHostTracked())
            {
                int tempRegister = regAlloc.AllocateTempGprRegister();

                Operand pte = new(tempRegister, RegisterType.Integer, OperandType.I64);

                asm.Lsr(pte, guestAddress, new Operand(OperandKind.Constant, OperandType.I32, 12));
                asm.LdrRr(pte, basePointer, pte, ArmExtensionType.Uxtx, true);
                asm.Add(destination64, pte, guestAddress);

                regAlloc.FreeTempGprRegister(tempRegister);
            }
            else if (mmType.IsHostMapped())
            {
                asm.Add(destination64, basePointer, guestAddress);
            }
            else
            {
                throw new NotImplementedException(mmType.ToString());
            }
        }

        public static void WriteAddShiftOffset(in Assembler asm, Operand rd, Operand rn, Operand offset, bool add, ArmShiftType shiftType, int shift)
        {
            Debug.Assert(offset.Kind != OperandKind.Constant || offset.AsInt32() >= 0);

            if (shiftType == ArmShiftType.Ror)
            {
                asm.Ror(rd, rn, InstEmitCommon.Const(shift & 31));

                if (add)
                {
                    asm.Add(rd, rd, offset, ArmShiftType.Lsl, 0);
                }
                else
                {
                    asm.Sub(rd, rd, offset, ArmShiftType.Lsl, 0);
                }
            }
            else
            {
                if (add)
                {
                    asm.Add(rd, rn, offset, shiftType, shift);
                }
                else
                {
                    asm.Sub(rd, rn, offset, shiftType, shift);
                }
            }
        }
    }
}
