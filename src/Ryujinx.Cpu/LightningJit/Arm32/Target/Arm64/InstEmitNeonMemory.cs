using Ryujinx.Cpu.LightningJit.CodeGen;
using Ryujinx.Cpu.LightningJit.CodeGen.Arm64;
using System;
using System.Diagnostics;

namespace Ryujinx.Cpu.LightningJit.Arm32.Target.Arm64
{
    static class InstEmitNeonMemory
    {
        public static void Vld11(CodeGenContext context, uint rd, uint rn, uint rm, uint indexAlign, uint size)
        {
            uint index = indexAlign >> ((int)size + 1);

            EmitMemory1234InstructionCore(context, rn, rm, 1 << (int)size, (address) =>
            {
                EmitMemoryLoad1234SingleInstruction(context, address, rd, index, size, 1, 1, context.Arm64Assembler.Ld1SnglAsNoPostIndex);
            });
        }

        public static void Vld1A(CodeGenContext context, uint rd, uint rn, uint rm, uint a, uint t, uint size)
        {
            EmitMemory1234InstructionCore(context, rn, rm, 1 << (int)size, (address) =>
            {
                EmitMemoryLoad1SingleReplicateInstruction(context, address, rd, size, t + 1, 1, context.Arm64Assembler.Ld1rAsNoPostIndex);
            });
        }

        public static void Vld1M(CodeGenContext context, uint rd, uint rn, uint rm, uint registersCount, uint align, uint size)
        {
            EmitMemory1234InstructionCore(context, rn, rm, 8 * (int)registersCount, (address) =>
            {
                EmitMemoryLoad1234MultipleInstruction(context, address, rd, size, registersCount, 1, context.Arm64Assembler.Ld1MultAsNoPostIndex);
            });
        }

        public static void Vld21(CodeGenContext context, uint rd, uint rn, uint rm, uint indexAlign, uint size)
        {
            uint index = indexAlign >> ((int)size + 1);
            uint step = size > 0 && (indexAlign & (1u << (int)size)) != 0 ? 2u : 1u;

            EmitMemory1234InstructionCore(context, rn, rm, 2 * (1 << (int)size), (address) =>
            {
                EmitMemoryLoad1234SingleInstruction(context, address, rd, index, size, 2, step, context.Arm64Assembler.Ld2SnglAsNoPostIndex);
            });
        }

        public static void Vld2A(CodeGenContext context, uint rd, uint rn, uint rm, uint a, uint t, uint size)
        {
            EmitMemory1234InstructionCore(context, rn, rm, 2 * (1 << (int)size), (address) =>
            {
                EmitMemoryLoad234SingleReplicateInstruction(context, address, rd, size, 2, t + 1, context.Arm64Assembler.Ld2rAsNoPostIndex);
            });
        }

        public static void Vld2M(CodeGenContext context, uint rd, uint rn, uint rm, uint type, uint align, uint size)
        {
            uint step = (type & 1) + 1;

            EmitMemory1234InstructionCore(context, rn, rm, 16, (address) =>
            {
                EmitMemoryLoad1234MultipleInstruction(context, address, rd, size, 2, step, context.Arm64Assembler.Ld2MultAsNoPostIndex);
            });
        }

        public static void Vld2M(CodeGenContext context, uint rd, uint rn, uint rm, uint align, uint size)
        {
            EmitMemory1234InstructionCore(context, rn, rm, 32, (address) =>
            {
                EmitMemoryLoad1234Multiple2x2Instruction(context, address, rd, size, context.Arm64Assembler.Ld2MultAsNoPostIndex);
            });
        }

        public static void Vld31(CodeGenContext context, uint rd, uint rn, uint rm, uint indexAlign, uint size)
        {
            uint index = indexAlign >> ((int)size + 1);
            uint step = size > 0 && (indexAlign & (1u << (int)size)) != 0 ? 2u : 1u;

            EmitMemory1234InstructionCore(context, rn, rm, 3 * (1 << (int)size), (address) =>
            {
                EmitMemoryLoad1234SingleInstruction(context, address, rd, index, size, 3, step, context.Arm64Assembler.Ld3SnglAsNoPostIndex);
            });
        }

        public static void Vld3A(CodeGenContext context, uint rd, uint rn, uint rm, uint a, uint t, uint size)
        {
            EmitMemory1234InstructionCore(context, rn, rm, 3 * (1 << (int)size), (address) =>
            {
                EmitMemoryLoad234SingleReplicateInstruction(context, address, rd, size, 3, t + 1, context.Arm64Assembler.Ld3rAsNoPostIndex);
            });
        }

        public static void Vld3M(CodeGenContext context, uint rd, uint rn, uint rm, uint type, uint align, uint size)
        {
            uint step = (type & 1) + 1;

            EmitMemory1234InstructionCore(context, rn, rm, 24, (address) =>
            {
                EmitMemoryLoad1234MultipleInstruction(context, address, rd, size, 3, step, context.Arm64Assembler.Ld3MultAsNoPostIndex);
            });
        }

        public static void Vld41(CodeGenContext context, uint rd, uint rn, uint rm, uint indexAlign, uint size)
        {
            uint index = indexAlign >> ((int)size + 1);
            uint step = size > 0 && (indexAlign & (1u << (int)size)) != 0 ? 2u : 1u;

            EmitMemory1234InstructionCore(context, rn, rm, 4 * (1 << (int)size), (address) =>
            {
                EmitMemoryLoad1234SingleInstruction(context, address, rd, index, size, 4, step, context.Arm64Assembler.Ld4SnglAsNoPostIndex);
            });
        }

        public static void Vld4A(CodeGenContext context, uint rd, uint rn, uint rm, uint a, uint t, uint size)
        {
            EmitMemory1234InstructionCore(context, rn, rm, 4 * (1 << (int)size), (address) =>
            {
                EmitMemoryLoad234SingleReplicateInstruction(context, address, rd, size, 4, t + 1, context.Arm64Assembler.Ld4rAsNoPostIndex);
            });
        }

        public static void Vld4M(CodeGenContext context, uint rd, uint rn, uint rm, uint type, uint align, uint size)
        {
            uint step = (type & 1) + 1;

            EmitMemory1234InstructionCore(context, rn, rm, 32, (address) =>
            {
                EmitMemoryLoad1234MultipleInstruction(context, address, rd, size, 4, step, context.Arm64Assembler.Ld4MultAsNoPostIndex);
            });
        }

        public static void Vldm(CodeGenContext context, uint rd, uint rn, uint registerCount, bool u, bool w, bool singleRegs)
        {
            EmitMemoryMultipleInstruction(context, rd, rn, registerCount, u, w, singleRegs, isStore: false);
        }

        public static void Vldr(CodeGenContext context, uint rd, uint rn, uint imm8, bool u, uint size)
        {
            EmitMemoryInstruction(context, rd, rn, imm8, u, size, isStore: false);
        }

        public static void Vst11(CodeGenContext context, uint rd, uint rn, uint rm, uint indexAlign, uint size)
        {
            uint index = indexAlign >> ((int)size + 1);

            EmitMemory1234InstructionCore(context, rn, rm, 1 << (int)size, (address) =>
            {
                EmitMemoryStore1234SingleInstruction(context, address, rd, index, size, 1, 1, context.Arm64Assembler.St1SnglAsNoPostIndex);
            });
        }

        public static void Vst1M(CodeGenContext context, uint rd, uint rn, uint rm, uint registersCount, uint align, uint size)
        {
            EmitMemory1234InstructionCore(context, rn, rm, 8 * (int)registersCount, (address) =>
            {
                EmitMemoryStore1234MultipleInstruction(context, address, rd, size, registersCount, 1, context.Arm64Assembler.St1MultAsNoPostIndex);
            });
        }

        public static void Vst21(CodeGenContext context, uint rd, uint rn, uint rm, uint indexAlign, uint size)
        {
            uint index = indexAlign >> ((int)size + 1);
            uint step = size > 0 && (indexAlign & (1u << (int)size)) != 0 ? 2u : 1u;

            EmitMemory1234InstructionCore(context, rn, rm, 2 * (1 << (int)size), (address) =>
            {
                EmitMemoryStore1234SingleInstruction(context, address, rd, index, size, 2, step, context.Arm64Assembler.St2SnglAsNoPostIndex);
            });
        }

        public static void Vst2M(CodeGenContext context, uint rd, uint rn, uint rm, uint type, uint align, uint size)
        {
            uint step = (type & 1) + 1;

            EmitMemory1234InstructionCore(context, rn, rm, 16, (address) =>
            {
                EmitMemoryStore1234MultipleInstruction(context, address, rd, size, 2, step, context.Arm64Assembler.St2MultAsNoPostIndex);
            });
        }

        public static void Vst2M(CodeGenContext context, uint rd, uint rn, uint rm, uint align, uint size)
        {
            EmitMemory1234InstructionCore(context, rn, rm, 32, (address) =>
            {
                EmitMemoryStore1234Multiple2x2Instruction(context, address, rd, size, context.Arm64Assembler.St2MultAsNoPostIndex);
            });
        }

        public static void Vst31(CodeGenContext context, uint rd, uint rn, uint rm, uint indexAlign, uint size)
        {
            uint index = indexAlign >> ((int)size + 1);
            uint step = size > 0 && (indexAlign & (1u << (int)size)) != 0 ? 2u : 1u;

            EmitMemory1234InstructionCore(context, rn, rm, 3 * (1 << (int)size), (address) =>
            {
                EmitMemoryStore1234SingleInstruction(context, address, rd, index, size, 3, step, context.Arm64Assembler.St3SnglAsNoPostIndex);
            });
        }

        public static void Vst3M(CodeGenContext context, uint rd, uint rn, uint rm, uint type, uint align, uint size)
        {
            uint step = (type & 1) + 1;

            EmitMemory1234InstructionCore(context, rn, rm, 24, (address) =>
            {
                EmitMemoryStore1234MultipleInstruction(context, address, rd, size, 3, step, context.Arm64Assembler.St3MultAsNoPostIndex);
            });
        }

        public static void Vst41(CodeGenContext context, uint rd, uint rn, uint rm, uint indexAlign, uint size)
        {
            uint index = indexAlign >> ((int)size + 1);
            uint step = size > 0 && (indexAlign & (1u << (int)size)) != 0 ? 2u : 1u;

            EmitMemory1234InstructionCore(context, rn, rm, 4 * (1 << (int)size), (address) =>
            {
                EmitMemoryStore1234SingleInstruction(context, address, rd, index, size, 4, step, context.Arm64Assembler.St4SnglAsNoPostIndex);
            });
        }

        public static void Vst4M(CodeGenContext context, uint rd, uint rn, uint rm, uint type, uint align, uint size)
        {
            uint step = (type & 1) + 1;

            EmitMemory1234InstructionCore(context, rn, rm, 32, (address) =>
            {
                EmitMemoryStore1234MultipleInstruction(context, address, rd, size, 4, step, context.Arm64Assembler.St4MultAsNoPostIndex);
            });
        }

        public static void Vstm(CodeGenContext context, uint rd, uint rn, uint registerCount, bool u, bool w, bool singleRegs)
        {
            EmitMemoryMultipleInstruction(context, rd, rn, registerCount, u, w, singleRegs, isStore: true);
        }

        public static void Vstr(CodeGenContext context, uint rd, uint rn, uint imm8, bool u, uint size)
        {
            EmitMemoryInstruction(context, rd, rn, imm8, u, size, isStore: true);
        }

        private static void EmitMemoryMultipleInstruction(
            CodeGenContext context,
            uint rd,
            uint rn,
            uint registerCount,
            bool add,
            bool wBack,
            bool singleRegs,
            bool isStore)
        {
            Operand baseAddress = InstEmitCommon.GetInputGpr(context, rn);

            using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            Operand offset = InstEmitCommon.Const((int)registerCount * (singleRegs ? 4 : 8));

            if (!add)
            {
                if (wBack)
                {
                    InstEmitMemory.WriteAddShiftOffset(context.Arm64Assembler, baseAddress, baseAddress, offset, false, ArmShiftType.Lsl, 0);
                    InstEmitMemory.WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, baseAddress);
                }
                else
                {
                    InstEmitMemory.WriteAddShiftOffset(context.Arm64Assembler, tempRegister.Operand, baseAddress, offset, false, ArmShiftType.Lsl, 0);
                    InstEmitMemory.WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, tempRegister.Operand);
                }
            }
            else
            {
                InstEmitMemory.WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, tempRegister.Operand, baseAddress);
            }

            EmitMemoryMultipleInstructionCore(context, tempRegister.Operand, rd, registerCount, singleRegs, isStore);

            if (add && wBack)
            {
                context.Arm64Assembler.Add(baseAddress, baseAddress, offset);
            }
        }

        private static void EmitMemoryMultipleInstructionCore(CodeGenContext context, Operand baseAddress, uint rd, uint registerCount, bool singleRegs, bool isStore)
        {
            int offs = 0;
            uint r = rd;
            uint upperBound = Math.Min(rd + registerCount, 32u);
            uint regMask = singleRegs ? 3u : 1u;

            // Read/write misaligned elements first.

            for (; (r & regMask) != 0 && r < upperBound; r++)
            {
                EmitMemoryInstruction(context, baseAddress, r, offs, singleRegs, isStore);

                offs += singleRegs ? 4 : 8;
            }

            // Read/write aligned, full vectors.

            while (upperBound - r >= (singleRegs ? 4 : 2))
            {
                int qIndex = (int)(r >> (singleRegs ? 2 : 1));

                Operand rtOperand = context.RegisterAllocator.RemapSimdRegister(qIndex);

                if (upperBound - r >= (singleRegs ? 8 : 4) && (offs & 0xf) == 0)
                {
                    Operand rt2Operand = context.RegisterAllocator.RemapSimdRegister(qIndex + 1);

                    if (isStore)
                    {
                        context.Arm64Assembler.StpRiUn(rtOperand, rt2Operand, baseAddress, offs);
                    }
                    else
                    {
                        context.Arm64Assembler.LdpRiUn(rtOperand, rt2Operand, baseAddress, offs);
                    }

                    r += singleRegs ? 8u : 4u;
                    offs += 32;
                }
                else
                {
                    if ((offs & 0xf) == 0)
                    {
                        if (isStore)
                        {
                            context.Arm64Assembler.StrRiUn(rtOperand, baseAddress, offs);
                        }
                        else
                        {
                            context.Arm64Assembler.LdrRiUn(rtOperand, baseAddress, offs);
                        }
                    }
                    else
                    {
                        if (isStore)
                        {
                            context.Arm64Assembler.Stur(rtOperand, baseAddress, offs);
                        }
                        else
                        {
                            context.Arm64Assembler.Ldur(rtOperand, baseAddress, offs);
                        }
                    }

                    r += singleRegs ? 4u : 2u;
                    offs += 16;
                }
            }

            // Read/write last misaligned elements.

            for (; r < upperBound; r++)
            {
                EmitMemoryInstruction(context, baseAddress, r, offs, singleRegs, isStore);

                offs += singleRegs ? 4 : 8;
            }
        }

        private static void EmitMemoryInstruction(CodeGenContext context, Operand baseAddress, uint r, int offs, bool singleRegs, bool isStore)
        {
            if (isStore)
            {
                using ScopedRegister tempRegister = InstEmitNeonCommon.MoveScalarToSide(context, r, singleRegs);

                context.Arm64Assembler.StrRiUn(tempRegister.Operand, baseAddress, offs);
            }
            else
            {
                using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempFpRegisterScoped(singleRegs);

                context.Arm64Assembler.LdrRiUn(tempRegister.Operand, baseAddress, offs);

                InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, r, singleRegs);
            }
        }

        private static void EmitMemoryInstruction(CodeGenContext context, uint rd, uint rn, uint imm8, bool add, uint size, bool isStore)
        {
            bool singleRegs = size != 3;
            int offs = (int)imm8;

            if (size == 1)
            {
                offs <<= 1;
            }
            else
            {
                offs <<= 2;
            }

            using ScopedRegister address = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            if (rn == RegisterUtils.PcRegister)
            {
                if (!add)
                {
                    offs = -offs;
                }

                context.Arm64Assembler.Mov(address.Operand, (context.Pc & ~3u) + (uint)offs);

                InstEmitMemory.WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, address.Operand, address.Operand);

                offs = 0;
            }
            else
            {
                Operand rnOperand = context.RegisterAllocator.RemapGprRegister((int)rn);

                if (InstEmitMemory.CanFoldOffset(context.MemoryManagerType, add ? offs : -offs, (int)size, true, out _))
                {
                    InstEmitMemory.WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, address.Operand, rnOperand);

                    if (!add)
                    {
                        offs = -offs;
                    }
                }
                else
                {
                    InstEmitMemory.WriteAddShiftOffset(context.Arm64Assembler, address.Operand, rnOperand, InstEmitCommon.Const(offs), add, ArmShiftType.Lsl, 0);
                    InstEmitMemory.WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, address.Operand, address.Operand);

                    offs = 0;
                }
            }

            if ((size == 3 && (offs & 7) != 0) || offs < 0)
            {
                if (isStore)
                {
                    using ScopedRegister tempRegister = InstEmitNeonCommon.MoveScalarToSide(context, rd, singleRegs);

                    context.Arm64Assembler.Stur(tempRegister.Operand, address.Operand, offs, size);
                }
                else
                {
                    using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempFpRegisterScoped(singleRegs);

                    context.Arm64Assembler.Ldur(tempRegister.Operand, address.Operand, offs, size);

                    InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, singleRegs);
                }
            }
            else
            {
                if (isStore)
                {
                    using ScopedRegister tempRegister = InstEmitNeonCommon.MoveScalarToSide(context, rd, singleRegs);

                    context.Arm64Assembler.StrRiUn(tempRegister.Operand, address.Operand, offs, size);
                }
                else
                {
                    using ScopedRegister tempRegister = context.RegisterAllocator.AllocateTempFpRegisterScoped(singleRegs);

                    context.Arm64Assembler.LdrRiUn(tempRegister.Operand, address.Operand, offs, size);

                    InstEmitNeonCommon.InsertResult(context, tempRegister.Operand, rd, singleRegs);
                }
            }
        }

        private static void EmitMemory1234InstructionCore(CodeGenContext context, uint rn, uint rm, int bytes, Action<Operand> callback)
        {
            bool wBack = rm != RegisterUtils.PcRegister;
            bool registerIndex = rm != RegisterUtils.PcRegister && rm != RegisterUtils.SpRegister;

            Operand rnOperand = InstEmitCommon.GetInputGpr(context, rn);

            using ScopedRegister address = context.RegisterAllocator.AllocateTempGprRegisterScoped();

            InstEmitMemory.WriteAddressTranslation(context.MemoryManagerType, context.RegisterAllocator, context.Arm64Assembler, address.Operand, rnOperand);

            callback(address.Operand);

            if (wBack)
            {
                if (registerIndex)
                {
                    Operand rmOperand = InstEmitCommon.GetInputGpr(context, rm);

                    context.Arm64Assembler.Add(rnOperand, rnOperand, rmOperand);
                }
                else
                {
                    context.Arm64Assembler.Add(rnOperand, rnOperand, InstEmitCommon.Const(bytes));
                }
            }
        }

        private static void EmitMemoryLoad1234SingleInstruction(
            CodeGenContext context,
            Operand baseAddress,
            uint rd,
            uint index,
            uint size,
            uint registerCount,
            uint step,
            Action<Operand, Operand, uint, uint> action)
        {
            ScopedRegister[] tempRegisters = AllocateSequentialRegisters(context, (int)registerCount);

            MoveDoublewordsToQuadwordsLower(context, rd, registerCount, step, tempRegisters);

            action(tempRegisters[0].Operand, baseAddress, index, size);

            MoveQuadwordsLowerToDoublewords(context, rd, registerCount, step, tempRegisters);

            FreeSequentialRegisters(tempRegisters);
        }

        private static void EmitMemoryLoad1SingleReplicateInstruction(
            CodeGenContext context,
            Operand baseAddress,
            uint rd,
            uint size,
            uint registerCount,
            uint step,
            Action<Operand, Operand, uint, uint> action)
        {
            if ((rd & 1) == 0 && registerCount == 2)
            {
                action(context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1)), baseAddress, size, 1);
            }
            else
            {
                uint vecsCount = (registerCount + 1) >> 1;

                ScopedRegister[] tempRegisters = AllocateSequentialRegisters(context, (int)vecsCount);

                action(tempRegisters[0].Operand, baseAddress, size, registerCount > 1 ? 1u : 0u);

                MoveQuadwordsToDoublewords(context, rd, registerCount, step, tempRegisters);

                FreeSequentialRegisters(tempRegisters);
            }
        }

        private static void EmitMemoryLoad234SingleReplicateInstruction(
            CodeGenContext context,
            Operand baseAddress,
            uint rd,
            uint size,
            uint registerCount,
            uint step,
            Action<Operand, Operand, uint, uint> action)
        {
            ScopedRegister[] tempRegisters = AllocateSequentialRegisters(context, (int)registerCount);

            action(tempRegisters[0].Operand, baseAddress, size, 0u);

            MoveQuadwordsLowerToDoublewords(context, rd, registerCount, step, tempRegisters);

            FreeSequentialRegisters(tempRegisters);
        }

        private static void EmitMemoryLoad1234MultipleInstruction(
            CodeGenContext context,
            Operand baseAddress,
            uint rd,
            uint size,
            uint registerCount,
            uint step,
            Action<Operand, Operand, uint, uint> action)
        {
            ScopedRegister[] tempRegisters = AllocateSequentialRegisters(context, (int)registerCount);

            action(tempRegisters[0].Operand, baseAddress, size, 0);

            MoveQuadwordsLowerToDoublewords(context, rd, registerCount, step, tempRegisters);

            FreeSequentialRegisters(tempRegisters);
        }

        private static void EmitMemoryLoad1234MultipleInstruction(
            CodeGenContext context,
            Operand baseAddress,
            uint rd,
            uint size,
            uint registerCount,
            uint step,
            Action<Operand, Operand, uint, uint, uint> action)
        {
            ScopedRegister[] tempRegisters = AllocateSequentialRegisters(context, (int)registerCount);

            action(tempRegisters[0].Operand, baseAddress, registerCount, size, 0);

            MoveQuadwordsLowerToDoublewords(context, rd, registerCount, step, tempRegisters);

            FreeSequentialRegisters(tempRegisters);
        }

        private static void EmitMemoryLoad1234Multiple2x2Instruction(
            CodeGenContext context,
            Operand baseAddress,
            uint rd,
            uint size,
            Action<Operand, Operand, uint, uint> action)
        {
            if ((rd & 1) == 0)
            {
                action(context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1), 2), baseAddress, size, 1);
            }
            else
            {
                ScopedRegister[] tempRegisters = AllocateSequentialRegisters(context, 2);

                action(tempRegisters[0].Operand, baseAddress, size, 1);

                MoveQuadwordsToDoublewords2x2(context, rd, tempRegisters);

                FreeSequentialRegisters(tempRegisters);
            }
        }

        private static void EmitMemoryStore1234SingleInstruction(
            CodeGenContext context,
            Operand baseAddress,
            uint rd,
            uint index,
            uint size,
            uint registerCount,
            uint step,
            Action<Operand, Operand, uint, uint> action)
        {
            ScopedRegister[] tempRegisters = AllocateSequentialRegisters(context, (int)registerCount);

            MoveDoublewordsToQuadwordsLower(context, rd, registerCount, step, tempRegisters);

            action(tempRegisters[0].Operand, baseAddress, index, size);

            FreeSequentialRegisters(tempRegisters);
        }

        private static void EmitMemoryStore1234MultipleInstruction(
            CodeGenContext context,
            Operand baseAddress,
            uint rd,
            uint size,
            uint registerCount,
            uint step,
            Action<Operand, Operand, uint, uint> action)
        {
            ScopedRegister[] tempRegisters = AllocateSequentialRegisters(context, (int)registerCount);

            MoveDoublewordsToQuadwordsLower(context, rd, registerCount, step, tempRegisters);

            action(tempRegisters[0].Operand, baseAddress, size, 0);

            FreeSequentialRegisters(tempRegisters);
        }

        private static void EmitMemoryStore1234MultipleInstruction(
            CodeGenContext context,
            Operand baseAddress,
            uint rd,
            uint size,
            uint registerCount,
            uint step,
            Action<Operand, Operand, uint, uint, uint> action)
        {
            ScopedRegister[] tempRegisters = AllocateSequentialRegisters(context, (int)registerCount);

            MoveDoublewordsToQuadwordsLower(context, rd, registerCount, step, tempRegisters);

            action(tempRegisters[0].Operand, baseAddress, registerCount, size, 0);

            FreeSequentialRegisters(tempRegisters);
        }

        private static void EmitMemoryStore1234Multiple2x2Instruction(
            CodeGenContext context,
            Operand baseAddress,
            uint rd,
            uint size,
            Action<Operand, Operand, uint, uint> action)
        {
            if ((rd & 1) == 0)
            {
                action(context.RegisterAllocator.RemapSimdRegister((int)(rd >> 1), 2), baseAddress, size, 1);
            }
            else
            {
                ScopedRegister[] tempRegisters = AllocateSequentialRegisters(context, 2);

                MoveDoublewordsToQuadwords2x2(context, rd, tempRegisters);

                action(tempRegisters[0].Operand, baseAddress, size, 1);

                FreeSequentialRegisters(tempRegisters);
            }
        }

        private static ScopedRegister[] AllocateSequentialRegisters(CodeGenContext context, int count)
        {
            ScopedRegister[] registers = new ScopedRegister[count];

            for (int index = 0; index < count; index++)
            {
                registers[index] = context.RegisterAllocator.AllocateTempSimdRegisterScoped();
            }

            AssertSequentialRegisters(registers);

            return registers;
        }

        private static void FreeSequentialRegisters(ReadOnlySpan<ScopedRegister> registers)
        {
            for (int index = 0; index < registers.Length; index++)
            {
                registers[index].Dispose();
            }
        }

        [Conditional("DEBUG")]
        private static void AssertSequentialRegisters(ReadOnlySpan<ScopedRegister> registers)
        {
            for (int index = 1; index < registers.Length; index++)
            {
                Debug.Assert(registers[index].Operand.GetRegister().Index == registers[0].Operand.GetRegister().Index + index);
            }
        }

        private static void MoveQuadwordsLowerToDoublewords(CodeGenContext context, uint rd, uint registerCount, uint step, ReadOnlySpan<ScopedRegister> registers)
        {
            for (int index = 0; index < registerCount; index++)
            {
                uint r = rd + (uint)index * step;

                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(r >> 1));
                uint imm5 = InstEmitNeonCommon.GetImm5ForElementIndex(r & 1u, false);
                context.Arm64Assembler.InsElt(rdOperand, registers[index].Operand, 0, imm5);
            }
        }

        private static void MoveDoublewordsToQuadwordsLower(CodeGenContext context, uint rd, uint registerCount, uint step, ReadOnlySpan<ScopedRegister> registers)
        {
            for (int index = 0; index < registerCount; index++)
            {
                uint r = rd + (uint)index * step;

                InstEmitNeonCommon.MoveScalarToSide(context, registers[index].Operand, r, false);
            }
        }

        private static void MoveDoublewordsToQuadwords2x2(CodeGenContext context, uint rd, ReadOnlySpan<ScopedRegister> registers)
        {
            for (int index = 0; index < 2; index++)
            {
                uint r = rd + (uint)index * 2;
                uint r2 = r + 1;

                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(r >> 1));
                uint imm5 = InstEmitNeonCommon.GetImm5ForElementIndex(0, false);
                context.Arm64Assembler.InsElt(registers[index].Operand, rdOperand, (r & 1u) << 3, imm5);

                rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(r2 >> 1));
                imm5 = InstEmitNeonCommon.GetImm5ForElementIndex(1, false);
                context.Arm64Assembler.InsElt(registers[index].Operand, rdOperand, (r2 & 1u) << 3, imm5);
            }
        }

        private static void MoveQuadwordsToDoublewords(CodeGenContext context, uint rd, uint registerCount, uint step, ReadOnlySpan<ScopedRegister> registers)
        {
            for (int index = 0; index < registerCount; index++)
            {
                uint r = rd + (uint)index * step;

                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(r >> 1));
                uint imm5 = InstEmitNeonCommon.GetImm5ForElementIndex(r & 1u, false);
                context.Arm64Assembler.InsElt(rdOperand, registers[index >> 1].Operand, ((uint)index & 1u) << 3, imm5);
            }
        }

        private static void MoveQuadwordsToDoublewords2x2(CodeGenContext context, uint rd, ReadOnlySpan<ScopedRegister> registers)
        {
            for (int index = 0; index < 2; index++)
            {
                uint r = rd + (uint)index * 2;
                uint r2 = r + 1;

                Operand rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(r >> 1));
                uint imm5 = InstEmitNeonCommon.GetImm5ForElementIndex(r & 1u, false);
                context.Arm64Assembler.InsElt(rdOperand, registers[index].Operand, 0, imm5);

                rdOperand = context.RegisterAllocator.RemapSimdRegister((int)(r2 >> 1));
                imm5 = InstEmitNeonCommon.GetImm5ForElementIndex(r2 & 1u, false);
                context.Arm64Assembler.InsElt(rdOperand, registers[index].Operand, 1u << 3, imm5);
            }
        }
    }
}
