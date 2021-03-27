using Ryujinx.Common.Logging;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Tamper.CodeEmitters;
using Ryujinx.HLE.HOS.Tamper.Operations;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Tamper
{
    class AtmosphereCompiler
    {
        public ITamperProgram Compile(IEnumerable<string> rawInstructions, ulong exeAddress, ulong heapAddress, ITamperedProcess process)
        {
            Logger.Debug?.Print(LogClass.TamperMachine, $"Executable address: {exeAddress:X16}");
            Logger.Debug?.Print(LogClass.TamperMachine, $"Heap address: {heapAddress:X16}");

            try
            {
                return CompileImpl(rawInstructions, exeAddress, heapAddress, process);
            }
            catch(TamperCompilationException exception)
            {
                // Just print the message without the stack trace.
                Logger.Error?.Print(LogClass.TamperMachine, exception.Message);
            }
            catch (Exception exception)
            {
                Logger.Error?.Print(LogClass.TamperMachine, exception.ToString());
            }

            Logger.Error?.Print(LogClass.TamperMachine, "There was a problem while compiling the Atmosphere cheat");

            return null;
        }

        private ITamperProgram CompileImpl(IEnumerable<string> rawInstructions, ulong exeAddress, ulong heapAddress, ITamperedProcess process)
        {
            CompilationContext context = new CompilationContext(exeAddress, heapAddress, process);
            context.BlockStack.Push(new OperationBlock(null));

            // Parse the instructions.

            foreach (string rawInstruction in rawInstructions)
            {
                Logger.Debug?.Print(LogClass.TamperMachine, $"Compiling instruction {rawInstruction}");

                byte[] instruction = InstructionHelper.ParseRawInstruction(rawInstruction);
                CodeType codeType = InstructionHelper.GetCodeType(instruction);

                switch (codeType)
                {
                    case CodeType.StoreConstantToAddress:
                        StoreConstantToAddress.Emit(instruction, context);
                        break;
                    case CodeType.BeginMemoryConditionalBlock:
                        BeginConditionalBlock.Emit(instruction, context);
                        break;
                    case CodeType.EndConditionalBlock:
                        EndConditionalBlock.Emit(instruction, context);
                        break;
                    case CodeType.StartEndLoop:
                        StartEndLoop.Emit(instruction, context);
                        break;
                    case CodeType.LoadRegisterWithContant:
                        LoadRegisterWithConstant.Emit(instruction, context);
                        break;
                    case CodeType.LoadRegisterWithMemory:
                        LoadRegisterWithMemory.Emit(instruction, context);
                        break;
                    case CodeType.StoreConstantToMemory:
                        StoreConstantToMemory.Emit(instruction, context);
                        break;
                    case CodeType.LegacyArithmetic:
                        LegacyArithmetic.Emit(instruction, context);
                        break;
                    case CodeType.BeginKeypressConditionalBlock:
                        BeginConditionalBlock.Emit(instruction, context);
                        break;
                    case CodeType.Arithmetic:
                        Arithmetic.Emit(instruction, context);
                        break;
                    case CodeType.StoreRegisterToMemory:
                        StoreRegisterToMemory.Emit(instruction, context);
                        break;
                    case CodeType.BeginRegisterConditionalBlock:
                        BeginConditionalBlock.Emit(instruction, context);
                        break;
                    case CodeType.SaveOrRestoreRegister:
                        SaveOrRestoreRegister.Emit(instruction, context);
                        break;
                    case CodeType.SaveOrRestoreRegisterWithMask:
                        SaveOrRestoreRegisterWithMask.Emit(instruction, context);
                        break;
                    case CodeType.ReadOrWriteStaticRegister:
                        ReadOrWriteStaticRegister.Emit(instruction, context);
                        break;
                    case CodeType.PauseProcess:
                        PauseProcess.Emit(instruction, context);
                        break;
                    case CodeType.ResumeProcess:
                        ResumeProcess.Emit(instruction, context);
                        break;
                    case CodeType.DebugLog:
                        DebugLog.Emit(instruction, context);
                        break;
                    default:
                        throw new TamperCompilationException($"Code type {codeType} not implemented in Atmosphere cheat");
                }
            }

            // Initialize only the registers used.

            Value<ulong> zero = new Value<ulong>(0UL);
            int position = 0;

            foreach (Register register in context.Registers.Values)
            {
                context.CurrentOperations.Insert(position, new OpMov<ulong>(register, zero));
                position++;
            }

            if (context.BlockStack.Count != 1)
            {
                throw new TamperCompilationException($"Reached end of compilation with unmatched conditional(s) or loop(s)");
            }

            return new AtmosphereProgram(process, context.PressedKeys, new Block(context.CurrentOperations));
        }
    }
}
