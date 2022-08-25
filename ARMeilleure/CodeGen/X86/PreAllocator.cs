using ARMeilleure.CodeGen.RegisterAllocators;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;
using static ARMeilleure.IntermediateRepresentation.Operation.Factory;

namespace ARMeilleure.CodeGen.X86
{
    static class PreAllocator
    {
        public static void RunPass(CompilerContext cctx, StackAllocator stackAlloc, out int maxCallArgs)
        {
            maxCallArgs = -1;

            CallConvName callConv = CallingConvention.GetCurrentCallConv();

            Operand[] preservedArgs = new Operand[CallingConvention.GetArgumentsOnRegsCount()];

            for (BasicBlock block = cctx.Cfg.Blocks.First; block != null; block = block.ListNext)
            {
                Operation nextNode;

                for (Operation node = block.Operations.First; node != default; node = nextNode)
                {
                    nextNode = node.ListNext;

                    if (node.Instruction == Instruction.Phi)
                    {
                        continue;
                    }

                    HandleConstantRegCopy(block.Operations, node);
                    HandleDestructiveRegCopy(block.Operations, node);
                    HandleConstrainedRegCopy(block.Operations, node);

                    switch (node.Instruction)
                    {
                        case Instruction.Call:
                            // Get the maximum number of arguments used on a call.
                            // On windows, when a struct is returned from the call,
                            // we also need to pass the pointer where the struct
                            // should be written on the first argument.
                            int argsCount = node.SourcesCount - 1;

                            if (node.Destination != default && node.Destination.Type == OperandType.V128)
                            {
                                argsCount++;
                            }

                            if (maxCallArgs < argsCount)
                            {
                                maxCallArgs = argsCount;
                            }

                            // Copy values to registers expected by the function
                            // being called, as mandated by the ABI.
                            if (callConv == CallConvName.Windows)
                            {
                                HandleCallWindowsAbi(block.Operations, stackAlloc, node);
                            }
                            else /* if (callConv == CallConvName.SystemV) */
                            {
                                HandleCallSystemVAbi(block.Operations, node);
                            }
                            break;

                        case Instruction.ConvertToFPUI:
                            HandleConvertToFPUI(block.Operations, node);
                            break;

                        case Instruction.LoadArgument:
                            if (callConv == CallConvName.Windows)
                            {
                                nextNode = HandleLoadArgumentWindowsAbi(cctx, block.Operations, preservedArgs, node);
                            }
                            else /* if (callConv == CallConvName.SystemV) */
                            {
                                nextNode = HandleLoadArgumentSystemVAbi(cctx, block.Operations, preservedArgs, node);
                            }
                            break;

                        case Instruction.Negate:
                            if (!node.GetSource(0).Type.IsInteger())
                            {
                                HandleNegate(block.Operations, node);
                            }
                            break;

                        case Instruction.Return:
                            if (callConv == CallConvName.Windows)
                            {
                                HandleReturnWindowsAbi(cctx, block.Operations, preservedArgs, node);
                            }
                            else /* if (callConv == CallConvName.SystemV) */
                            {
                                HandleReturnSystemVAbi(block.Operations, node);
                            }
                            break;

                        case Instruction.Tailcall:
                            if (callConv == CallConvName.Windows)
                            {
                                HandleTailcallWindowsAbi(block.Operations, stackAlloc, node);
                            }
                            else
                            {
                                HandleTailcallSystemVAbi(block.Operations, stackAlloc, node);
                            }
                            break;

                        case Instruction.VectorInsert8:
                            if (!HardwareCapabilities.SupportsSse41)
                            {
                                HandleVectorInsert8(block.Operations, node);
                            }
                            break;

                        case Instruction.Extended:
                            if (node.Intrinsic == Intrinsic.X86Mxcsrmb || node.Intrinsic == Intrinsic.X86Mxcsrub)
                            {
                                int stackOffset = stackAlloc.Allocate(OperandType.I32);

                                node.SetSources(new Operand[] { Const(stackOffset), node.GetSource(0) });
                            }
                            break;
                    }
                }
            }
        }

        private static void HandleConstantRegCopy(IntrusiveList<Operation> nodes, Operation node)
        {
            if (node.SourcesCount == 0 || IsXmmIntrinsic(node))
            {
                return;
            }

            Instruction inst = node.Instruction;

            Operand src1 = node.GetSource(0);
            Operand src2;

            if (src1.Kind == OperandKind.Constant)
            {
                if (!src1.Type.IsInteger())
                {
                    // Handle non-integer types (FP32, FP64 and V128).
                    // For instructions without an immediate operand, we do the following:
                    // - Insert a copy with the constant value (as integer) to a GPR.
                    // - Insert a copy from the GPR to a XMM register.
                    // - Replace the constant use with the XMM register.
                    src1 = AddXmmCopy(nodes, node, src1);

                    node.SetSource(0, src1);
                }
                else if (!HasConstSrc1(inst))
                {
                    // Handle integer types.
                    // Most ALU instructions accepts a 32-bits immediate on the second operand.
                    // We need to ensure the following:
                    // - If the constant is on operand 1, we need to move it.
                    // -- But first, we try to swap operand 1 and 2 if the instruction is commutative.
                    // -- Doing so may allow us to encode the constant as operand 2 and avoid a copy.
                    // - If the constant is on operand 2, we check if the instruction supports it,
                    // if not, we also add a copy. 64-bits constants are usually not supported.
                    if (IsCommutative(node))
                    {
                        src2 = node.GetSource(1);

                        Operand temp = src1;

                        src1 = src2;
                        src2 = temp;

                        node.SetSource(0, src1);
                        node.SetSource(1, src2);
                    }

                    if (src1.Kind == OperandKind.Constant)
                    {
                        src1 = AddCopy(nodes, node, src1);

                        node.SetSource(0, src1);
                    }
                }
            }

            if (node.SourcesCount < 2)
            {
                return;
            }

            src2 = node.GetSource(1);

            if (src2.Kind == OperandKind.Constant)
            {
                if (!src2.Type.IsInteger())
                {
                    src2 = AddXmmCopy(nodes, node, src2);

                    node.SetSource(1, src2);
                }
                else if (!HasConstSrc2(inst) || CodeGenCommon.IsLongConst(src2))
                {
                    src2 = AddCopy(nodes, node, src2);

                    node.SetSource(1, src2);
                }
            }
        }

        private static void HandleConstrainedRegCopy(IntrusiveList<Operation> nodes, Operation node)
        {
            Operand dest = node.Destination;

            switch (node.Instruction)
            {
                case Instruction.CompareAndSwap:
                case Instruction.CompareAndSwap16:
                case Instruction.CompareAndSwap8:
                {
                    OperandType type = node.GetSource(1).Type;

                    if (type == OperandType.V128)
                    {
                        // Handle the many restrictions of the compare and exchange (16 bytes) instruction:
                        // - The expected value should be in RDX:RAX.
                        // - The new value to be written should be in RCX:RBX.
                        // - The value at the memory location is loaded to RDX:RAX.
                        void SplitOperand(Operand source, Operand lr, Operand hr)
                        {
                            nodes.AddBefore(node, Operation(Instruction.VectorExtract, lr, source, Const(0)));
                            nodes.AddBefore(node, Operation(Instruction.VectorExtract, hr, source, Const(1)));
                        }

                        Operand rax = Gpr(X86Register.Rax, OperandType.I64);
                        Operand rbx = Gpr(X86Register.Rbx, OperandType.I64);
                        Operand rcx = Gpr(X86Register.Rcx, OperandType.I64);
                        Operand rdx = Gpr(X86Register.Rdx, OperandType.I64);

                        SplitOperand(node.GetSource(1), rax, rdx);
                        SplitOperand(node.GetSource(2), rbx, rcx);

                        Operation operation = node;

                        node = nodes.AddAfter(node, Operation(Instruction.VectorCreateScalar, dest, rax));
                        nodes.AddAfter(node, Operation(Instruction.VectorInsert, dest, dest, rdx, Const(1)));

                        operation.SetDestinations(new Operand[] { rdx, rax });
                        operation.SetSources(new Operand[] { operation.GetSource(0), rdx, rax, rcx, rbx });
                    }
                    else
                    {
                        // Handle the many restrictions of the compare and exchange (32/64) instruction:
                        // - The expected value should be in (E/R)AX.
                        // - The value at the memory location is loaded to (E/R)AX.
                        Operand expected = node.GetSource(1);
                        Operand newValue = node.GetSource(2);

                        Operand rax = Gpr(X86Register.Rax, expected.Type);

                        nodes.AddBefore(node, Operation(Instruction.Copy, rax, expected));

                        // We need to store the new value into a temp, since it may
                        // be a constant, and this instruction does not support immediate operands.
                        Operand temp = Local(newValue.Type);

                        nodes.AddBefore(node, Operation(Instruction.Copy, temp, newValue));

                        node.SetSources(new Operand[] { node.GetSource(0), rax, temp });

                        nodes.AddAfter(node, Operation(Instruction.Copy, dest, rax));

                        node.Destination = rax;
                    }

                    break;
                }

                case Instruction.Divide:
                case Instruction.DivideUI:
                {
                    // Handle the many restrictions of the division instructions:
                    // - The dividend is always in RDX:RAX.
                    // - The result is always in RAX.
                    // - Additionally it also writes the remainder in RDX.
                    if (dest.Type.IsInteger())
                    {
                        Operand src1 = node.GetSource(0);

                        Operand rax = Gpr(X86Register.Rax, src1.Type);
                        Operand rdx = Gpr(X86Register.Rdx, src1.Type);

                        nodes.AddBefore(node, Operation(Instruction.Copy,    rax, src1));
                        nodes.AddBefore(node, Operation(Instruction.Clobber, rdx));

                        nodes.AddAfter(node, Operation(Instruction.Copy, dest, rax));

                        node.SetSources(new Operand[] { rdx, rax, node.GetSource(1) });
                        node.Destination = rax;
                    }

                    break;
                }

                case Instruction.Extended:
                {
                    bool isBlend = node.Intrinsic == Intrinsic.X86Blendvpd ||
                                   node.Intrinsic == Intrinsic.X86Blendvps ||
                                   node.Intrinsic == Intrinsic.X86Pblendvb;

                    // BLENDVPD, BLENDVPS, PBLENDVB last operand is always implied to be XMM0 when VEX is not supported.
                    // SHA256RNDS2 always has an implied XMM0 as a last operand.
                    if ((isBlend && !HardwareCapabilities.SupportsVexEncoding) || node.Intrinsic == Intrinsic.X86Sha256Rnds2)
                    {
                        Operand xmm0 = Xmm(X86Register.Xmm0, OperandType.V128);

                        nodes.AddBefore(node, Operation(Instruction.Copy, xmm0, node.GetSource(2)));

                        node.SetSource(2, xmm0);
                    }

                    break;
                }

                case Instruction.Multiply64HighSI:
                case Instruction.Multiply64HighUI:
                {
                    // Handle the many restrictions of the i64 * i64 = i128 multiply instructions:
                    // - The multiplicand is always in RAX.
                    // - The lower 64-bits of the result is always in RAX.
                    // - The higher 64-bits of the result is always in RDX.
                    Operand src1 = node.GetSource(0);

                    Operand rax = Gpr(X86Register.Rax, src1.Type);
                    Operand rdx = Gpr(X86Register.Rdx, src1.Type);

                    nodes.AddBefore(node, Operation(Instruction.Copy, rax, src1));

                    node.SetSource(0, rax);

                    nodes.AddAfter(node, Operation(Instruction.Copy, dest, rdx));

                    node.SetDestinations(new Operand[] { rdx, rax });

                    break;
                }

                case Instruction.RotateRight:
                case Instruction.ShiftLeft:
                case Instruction.ShiftRightSI:
                case Instruction.ShiftRightUI:
                {
                    // The shift register is always implied to be CL (low 8-bits of RCX or ECX).
                    if (node.GetSource(1).Kind == OperandKind.LocalVariable)
                    {
                        Operand rcx = Gpr(X86Register.Rcx, OperandType.I32);

                        nodes.AddBefore(node, Operation(Instruction.Copy, rcx, node.GetSource(1)));

                        node.SetSource(1, rcx);
                    }

                    break;
                }
            }
        }

        private static void HandleDestructiveRegCopy(IntrusiveList<Operation> nodes, Operation node)
        {
            if (node.Destination == default || node.SourcesCount == 0)
            {
                return;
            }

            Instruction inst = node.Instruction;

            Operand dest = node.Destination;
            Operand src1 = node.GetSource(0);

            // The multiply instruction (that maps to IMUL) is somewhat special, it has
            // a three operand form where the second source is a immediate value.
            bool threeOperandForm = inst == Instruction.Multiply && node.GetSource(1).Kind == OperandKind.Constant;

            if (IsSameOperandDestSrc1(node) && src1.Kind == OperandKind.LocalVariable && !threeOperandForm)
            {
                bool useNewLocal = false;

                for (int srcIndex = 1; srcIndex < node.SourcesCount; srcIndex++)
                {
                    if (node.GetSource(srcIndex) == dest)
                    {
                        useNewLocal = true;

                        break;
                    }
                }

                if (useNewLocal)
                {
                    // Dest is being used as some source already, we need to use a new
                    // local to store the temporary value, otherwise the value on dest
                    // local would be overwritten.
                    Operand temp = Local(dest.Type);

                    nodes.AddBefore(node, Operation(Instruction.Copy, temp, src1));

                    node.SetSource(0, temp);

                    nodes.AddAfter(node, Operation(Instruction.Copy, dest, temp));

                    node.Destination = temp;
                }
                else
                {
                    nodes.AddBefore(node, Operation(Instruction.Copy, dest, src1));

                    node.SetSource(0, dest);
                }
            }
            else if (inst == Instruction.ConditionalSelect)
            {
                Operand src2 = node.GetSource(1);
                Operand src3 = node.GetSource(2);

                if (src1 == dest || src2 == dest)
                {
                    Operand temp = Local(dest.Type);

                    nodes.AddBefore(node, Operation(Instruction.Copy, temp, src3));

                    node.SetSource(2, temp);

                    nodes.AddAfter(node, Operation(Instruction.Copy, dest, temp));

                    node.Destination = temp;
                }
                else
                {
                    nodes.AddBefore(node, Operation(Instruction.Copy, dest, src3));

                    node.SetSource(2, dest);
                }
            }
        }

        private static void HandleConvertToFPUI(IntrusiveList<Operation> nodes, Operation node)
        {
            // Unsigned integer to FP conversions are not supported on X86.
            // We need to turn them into signed integer to FP conversions, and
            // adjust the final result.
            Operand dest   = node.Destination;
            Operand source = node.GetSource(0);

            Debug.Assert(source.Type.IsInteger(), $"Invalid source type \"{source.Type}\".");

            Operation currentNode = node;

            if (source.Type == OperandType.I32)
            {
                // For 32-bits integers, we can just zero-extend to 64-bits,
                // and then use the 64-bits signed conversion instructions.
                Operand zex = Local(OperandType.I64);

                node = nodes.AddAfter(node, Operation(Instruction.ZeroExtend32, zex,  source));
                node = nodes.AddAfter(node, Operation(Instruction.ConvertToFP,  dest, zex));
            }
            else /* if (source.Type == OperandType.I64) */
            {
                // For 64-bits integers, we need to do the following:
                // - Ensure that the integer has the most significant bit clear.
                // -- This can be done by shifting the value right by 1, that is, dividing by 2.
                // -- The least significant bit is lost in this case though.
                // - We can then convert the shifted value with a signed integer instruction.
                // - The result still needs to be corrected after that.
                // -- First, we need to multiply the result by 2, as we divided it by 2 before.
                // --- This can be done efficiently by adding the result to itself.
                // -- Then, we need to add the least significant bit that was shifted out.
                // --- We can convert the least significant bit to float, and add it to the result.
                Operand lsb  = Local(OperandType.I64);
                Operand half = Local(OperandType.I64);

                Operand lsbF = Local(dest.Type);

                node = nodes.AddAfter(node, Operation(Instruction.Copy, lsb,  source));
                node = nodes.AddAfter(node, Operation(Instruction.Copy, half, source));

                node = nodes.AddAfter(node, Operation(Instruction.BitwiseAnd,   lsb,  lsb,  Const(1L)));
                node = nodes.AddAfter(node, Operation(Instruction.ShiftRightUI, half, half, Const(1)));

                node = nodes.AddAfter(node, Operation(Instruction.ConvertToFP, lsbF, lsb));
                node = nodes.AddAfter(node, Operation(Instruction.ConvertToFP, dest, half));

                node = nodes.AddAfter(node, Operation(Instruction.Add, dest, dest, dest));
                nodes.AddAfter(node, Operation(Instruction.Add, dest, dest, lsbF));
            }

            Delete(nodes, currentNode);
        }

        private static void HandleNegate(IntrusiveList<Operation> nodes, Operation node)
        {
            // There's no SSE FP negate instruction, so we need to transform that into
            // a XOR of the value to be negated with a mask with the highest bit set.
            // This also produces -0 for a negation of the value 0.
            Operand dest   = node.Destination;
            Operand source = node.GetSource(0);

            Debug.Assert(dest.Type == OperandType.FP32 ||
                         dest.Type == OperandType.FP64, $"Invalid destination type \"{dest.Type}\".");

            Operation currentNode = node;

            Operand res = Local(dest.Type);

            node = nodes.AddAfter(node, Operation(Instruction.VectorOne, res));

            if (dest.Type == OperandType.FP32)
            {
                node = nodes.AddAfter(node, Operation(Intrinsic.X86Pslld, res, res, Const(31)));
            }
            else /* if (dest.Type == OperandType.FP64) */
            {
                node = nodes.AddAfter(node, Operation(Intrinsic.X86Psllq, res, res, Const(63)));
            }

            node = nodes.AddAfter(node, Operation(Intrinsic.X86Xorps, res, res, source));

            nodes.AddAfter(node, Operation(Instruction.Copy, dest, res));

            Delete(nodes, currentNode);
        }

        private static void HandleVectorInsert8(IntrusiveList<Operation> nodes, Operation node)
        {
            // Handle vector insertion, when SSE 4.1 is not supported.
            Operand dest = node.Destination;
            Operand src1 = node.GetSource(0); // Vector
            Operand src2 = node.GetSource(1); // Value
            Operand src3 = node.GetSource(2); // Index

            Debug.Assert(src3.Kind == OperandKind.Constant);

            byte index = src3.AsByte();

            Debug.Assert(index < 16);

            Operation currentNode = node;

            Operand temp1 = Local(OperandType.I32);
            Operand temp2 = Local(OperandType.I32);

            node = nodes.AddAfter(node, Operation(Instruction.Copy, temp2, src2));

            Operation vextOp = Operation(Instruction.VectorExtract16, temp1, src1, Const(index >> 1));

            node = nodes.AddAfter(node, vextOp);

            if ((index & 1) != 0)
            {
                node = nodes.AddAfter(node, Operation(Instruction.ZeroExtend8, temp1, temp1));
                node = nodes.AddAfter(node, Operation(Instruction.ShiftLeft,   temp2, temp2, Const(8)));
                node = nodes.AddAfter(node, Operation(Instruction.BitwiseOr,   temp1, temp1, temp2));
            }
            else
            {
                node = nodes.AddAfter(node, Operation(Instruction.ZeroExtend8, temp2, temp2));
                node = nodes.AddAfter(node, Operation(Instruction.BitwiseAnd,  temp1, temp1, Const(0xff00)));
                node = nodes.AddAfter(node, Operation(Instruction.BitwiseOr,   temp1, temp1, temp2));
            }

            Operation vinsOp = Operation(Instruction.VectorInsert16, dest, src1, temp1, Const(index >> 1));

            nodes.AddAfter(node, vinsOp);

            Delete(nodes, currentNode);
        }

        private static void HandleCallWindowsAbi(IntrusiveList<Operation> nodes, StackAllocator stackAlloc, Operation node)
        {
            Operand dest = node.Destination;

            // Handle struct arguments.
            int retArgs = 0;
            int stackAllocOffset = 0;

            int AllocateOnStack(int size)
            {
                // We assume that the stack allocator is initially empty (TotalSize = 0).
                // Taking that into account, we can reuse the space allocated for other
                // calls by keeping track of our own allocated size (stackAllocOffset).
                // If the space allocated is not big enough, then we just expand it.
                int offset = stackAllocOffset;

                if (stackAllocOffset + size > stackAlloc.TotalSize)
                {
                    stackAlloc.Allocate((stackAllocOffset + size) - stackAlloc.TotalSize);
                }

                stackAllocOffset += size;

                return offset;
            }

            Operand arg0Reg = default;

            if (dest != default && dest.Type == OperandType.V128)
            {
                int stackOffset = AllocateOnStack(dest.Type.GetSizeInBytes());

                arg0Reg = Gpr(CallingConvention.GetIntArgumentRegister(0), OperandType.I64);

                Operation allocOp = Operation(Instruction.StackAlloc, arg0Reg, Const(stackOffset));

                nodes.AddBefore(node, allocOp);

                retArgs = 1;
            }

            int argsCount = node.SourcesCount - 1;
            int maxArgs = CallingConvention.GetArgumentsOnRegsCount() - retArgs;

            if (argsCount > maxArgs)
            {
                argsCount = maxArgs;
            }

            Operand[] sources = new Operand[1 + retArgs + argsCount];

            sources[0] = node.GetSource(0);

            if (arg0Reg != default)
            {
                sources[1] = arg0Reg;
            }

            for (int index = 1; index < node.SourcesCount; index++)
            {
                Operand source = node.GetSource(index);

                if (source.Type == OperandType.V128)
                {
                    Operand stackAddr = Local(OperandType.I64);

                    int stackOffset = AllocateOnStack(source.Type.GetSizeInBytes());

                    nodes.AddBefore(node, Operation(Instruction.StackAlloc, stackAddr, Const(stackOffset)));

                    Operation storeOp = Operation(Instruction.Store, default, stackAddr, source);

                    HandleConstantRegCopy(nodes, nodes.AddBefore(node, storeOp));

                    node.SetSource(index, stackAddr);
                }
            }

            // Handle arguments passed on registers.
            for (int index = 0; index < argsCount; index++)
            {
                Operand source = node.GetSource(index + 1);
                Operand argReg;

                int argIndex = index + retArgs;

                if (source.Type.IsInteger())
                {
                    argReg = Gpr(CallingConvention.GetIntArgumentRegister(argIndex), source.Type);
                }
                else
                {
                    argReg = Xmm(CallingConvention.GetVecArgumentRegister(argIndex), source.Type);
                }

                Operation copyOp = Operation(Instruction.Copy, argReg, source);

                HandleConstantRegCopy(nodes, nodes.AddBefore(node, copyOp));

                sources[1 + retArgs + index] = argReg;
            }

            // The remaining arguments (those that are not passed on registers)
            // should be passed on the stack, we write them to the stack with "SpillArg".
            for (int index = argsCount; index < node.SourcesCount - 1; index++)
            {
                Operand source = node.GetSource(index + 1);
                Operand offset = Const((index + retArgs) * 8);

                Operation spillOp = Operation(Instruction.SpillArg, default, offset, source);

                HandleConstantRegCopy(nodes, nodes.AddBefore(node, spillOp));
            }

            if (dest != default)
            {
                if (dest.Type == OperandType.V128)
                {
                    Operand retValueAddr = Local(OperandType.I64);

                    nodes.AddBefore(node, Operation(Instruction.Copy, retValueAddr, arg0Reg));

                    Operation loadOp = Operation(Instruction.Load, dest, retValueAddr);

                    nodes.AddAfter(node, loadOp);

                    node.Destination = default;
                }
                else
                {
                    Operand retReg = dest.Type.IsInteger()
                        ? Gpr(CallingConvention.GetIntReturnRegister(), dest.Type)
                        : Xmm(CallingConvention.GetVecReturnRegister(), dest.Type);

                    Operation copyOp = Operation(Instruction.Copy, dest, retReg);

                    nodes.AddAfter(node, copyOp);

                    node.Destination = retReg;
                }
            }

            node.SetSources(sources);
        }

        private static void HandleCallSystemVAbi(IntrusiveList<Operation> nodes, Operation node)
        {
            Operand dest = node.Destination;

            List<Operand> sources = new List<Operand>
            {
                node.GetSource(0)
            };

            int argsCount = node.SourcesCount - 1;

            int intMax = CallingConvention.GetIntArgumentsOnRegsCount();
            int vecMax = CallingConvention.GetVecArgumentsOnRegsCount();

            int intCount = 0;
            int vecCount = 0;

            int stackOffset = 0;

            for (int index = 0; index < argsCount; index++)
            {
                Operand source = node.GetSource(index + 1);

                bool passOnReg;

                if (source.Type.IsInteger())
                {
                    passOnReg = intCount < intMax;
                }
                else if (source.Type == OperandType.V128)
                {
                    passOnReg = intCount + 1 < intMax;
                }
                else
                {
                    passOnReg = vecCount < vecMax;
                }

                if (source.Type == OperandType.V128 && passOnReg)
                {
                    // V128 is a struct, we pass each half on a GPR if possible.
                    Operand argReg  = Gpr(CallingConvention.GetIntArgumentRegister(intCount++), OperandType.I64);
                    Operand argReg2 = Gpr(CallingConvention.GetIntArgumentRegister(intCount++), OperandType.I64);

                    nodes.AddBefore(node, Operation(Instruction.VectorExtract, argReg,  source, Const(0)));
                    nodes.AddBefore(node, Operation(Instruction.VectorExtract, argReg2, source, Const(1)));

                    continue;
                }

                if (passOnReg)
                {
                    Operand argReg = source.Type.IsInteger()
                        ? Gpr(CallingConvention.GetIntArgumentRegister(intCount++), source.Type)
                        : Xmm(CallingConvention.GetVecArgumentRegister(vecCount++), source.Type);

                    Operation copyOp = Operation(Instruction.Copy, argReg, source);

                    HandleConstantRegCopy(nodes, nodes.AddBefore(node, copyOp));

                    sources.Add(argReg);
                }
                else
                {
                    Operand offset = Const(stackOffset);

                    Operation spillOp = Operation(Instruction.SpillArg, default, offset, source);

                    HandleConstantRegCopy(nodes, nodes.AddBefore(node, spillOp));

                    stackOffset += source.Type.GetSizeInBytes();
                }
            }

            node.SetSources(sources.ToArray());

            if (dest != default)
            {
                if (dest.Type == OperandType.V128)
                {
                    Operand retLReg = Gpr(CallingConvention.GetIntReturnRegister(),     OperandType.I64);
                    Operand retHReg = Gpr(CallingConvention.GetIntReturnRegisterHigh(), OperandType.I64);

                    Operation operation = node;

                    node = nodes.AddAfter(node, Operation(Instruction.VectorCreateScalar, dest, retLReg));
                    nodes.AddAfter(node, Operation(Instruction.VectorInsert, dest, dest, retHReg, Const(1)));

                    operation.Destination = default;
                }
                else
                {
                    Operand retReg = dest.Type.IsInteger()
                        ? Gpr(CallingConvention.GetIntReturnRegister(), dest.Type)
                        : Xmm(CallingConvention.GetVecReturnRegister(), dest.Type);

                    Operation copyOp = Operation(Instruction.Copy, dest, retReg);

                    nodes.AddAfter(node, copyOp);

                    node.Destination = retReg;
                }
            }
        }

        private static void HandleTailcallSystemVAbi(IntrusiveList<Operation> nodes, StackAllocator stackAlloc, Operation node)
        {
            List<Operand> sources = new List<Operand>
            {
                node.GetSource(0)
            };

            int argsCount = node.SourcesCount - 1;

            int intMax = CallingConvention.GetIntArgumentsOnRegsCount();
            int vecMax = CallingConvention.GetVecArgumentsOnRegsCount();

            int intCount = 0;
            int vecCount = 0;

            // Handle arguments passed on registers.
            for (int index = 0; index < argsCount; index++)
            {
                Operand source = node.GetSource(1 + index);

                bool passOnReg;

                if (source.Type.IsInteger())
                {
                    passOnReg = intCount + 1 < intMax;
                }
                else
                {
                    passOnReg = vecCount < vecMax;
                }

                if (source.Type == OperandType.V128 && passOnReg)
                {
                    // V128 is a struct, we pass each half on a GPR if possible.
                    Operand argReg = Gpr(CallingConvention.GetIntArgumentRegister(intCount++), OperandType.I64);
                    Operand argReg2 = Gpr(CallingConvention.GetIntArgumentRegister(intCount++), OperandType.I64);

                    nodes.AddBefore(node, Operation(Instruction.VectorExtract, argReg, source, Const(0)));
                    nodes.AddBefore(node, Operation(Instruction.VectorExtract, argReg2, source, Const(1)));

                    continue;
                }

                if (passOnReg)
                {
                    Operand argReg = source.Type.IsInteger()
                        ? Gpr(CallingConvention.GetIntArgumentRegister(intCount++), source.Type)
                        : Xmm(CallingConvention.GetVecArgumentRegister(vecCount++), source.Type);

                    Operation copyOp = Operation(Instruction.Copy, argReg, source);

                    HandleConstantRegCopy(nodes, nodes.AddBefore(node, copyOp));

                    sources.Add(argReg);
                }
                else
                {
                    throw new NotImplementedException("Spilling is not currently supported for tail calls. (too many arguments)");
                }
            }

            // The target address must be on the return registers, since we
            // don't return anything and it is guaranteed to not be a
            // callee saved register (which would be trashed on the epilogue).
            Operand retReg = Gpr(CallingConvention.GetIntReturnRegister(), OperandType.I64);

            Operation addrCopyOp = Operation(Instruction.Copy, retReg, node.GetSource(0));

            nodes.AddBefore(node, addrCopyOp);

            sources[0] = retReg;

            node.SetSources(sources.ToArray());
        }

        private static void HandleTailcallWindowsAbi(IntrusiveList<Operation> nodes, StackAllocator stackAlloc, Operation node)
        {
            int argsCount = node.SourcesCount - 1;
            int maxArgs = CallingConvention.GetArgumentsOnRegsCount();

            if (argsCount > maxArgs)
            {
                throw new NotImplementedException("Spilling is not currently supported for tail calls. (too many arguments)");
            }

            Operand[] sources = new Operand[1 + argsCount];

            // Handle arguments passed on registers.
            for (int index = 0; index < argsCount; index++)
            {
                Operand source = node.GetSource(1 + index);
                Operand argReg = source.Type.IsInteger()
                    ? Gpr(CallingConvention.GetIntArgumentRegister(index), source.Type)
                    : Xmm(CallingConvention.GetVecArgumentRegister(index), source.Type);

                Operation copyOp = Operation(Instruction.Copy, argReg, source);

                HandleConstantRegCopy(nodes, nodes.AddBefore(node, copyOp));

                sources[1 + index] = argReg;
            }

            // The target address must be on the return registers, since we
            // don't return anything and it is guaranteed to not be a
            // callee saved register (which would be trashed on the epilogue).
            Operand retReg = Gpr(CallingConvention.GetIntReturnRegister(), OperandType.I64);

            Operation addrCopyOp = Operation(Instruction.Copy, retReg, node.GetSource(0));

            nodes.AddBefore(node, addrCopyOp);

            sources[0] = retReg;

            node.SetSources(sources);
        }

        private static Operation HandleLoadArgumentWindowsAbi(
            CompilerContext cctx,
            IntrusiveList<Operation> nodes,
            Operand[] preservedArgs,
            Operation node)
        {
            Operand source = node.GetSource(0);

            Debug.Assert(source.Kind == OperandKind.Constant, "Non-constant LoadArgument source kind.");

            int retArgs = cctx.FuncReturnType == OperandType.V128 ? 1 : 0;

            int index = source.AsInt32() + retArgs;

            if (index < CallingConvention.GetArgumentsOnRegsCount())
            {
                Operand dest = node.Destination;

                if (preservedArgs[index] == default)
                {
                    Operand argReg, pArg;

                    if (dest.Type.IsInteger())
                    {
                        argReg = Gpr(CallingConvention.GetIntArgumentRegister(index), dest.Type);
                        pArg = Local(dest.Type);
                    }
                    else if (dest.Type == OperandType.V128)
                    {
                        argReg = Gpr(CallingConvention.GetIntArgumentRegister(index), OperandType.I64);
                        pArg = Local(OperandType.I64);
                    }
                    else
                    {
                        argReg = Xmm(CallingConvention.GetVecArgumentRegister(index), dest.Type);
                        pArg = Local(dest.Type);
                    }

                    Operation copyOp = Operation(Instruction.Copy, pArg, argReg);

                    cctx.Cfg.Entry.Operations.AddFirst(copyOp);

                    preservedArgs[index] = pArg;
                }

                Operation argCopyOp = Operation(dest.Type == OperandType.V128
                    ? Instruction.Load
                    : Instruction.Copy, dest, preservedArgs[index]);

                Operation newNode = nodes.AddBefore(node, argCopyOp);

                Delete(nodes, node);

                return newNode;
            }
            else
            {
                // TODO: Pass on stack.
                return node;
            }
        }

        private static Operation HandleLoadArgumentSystemVAbi(
            CompilerContext cctx,
            IntrusiveList<Operation> nodes,
            Operand[] preservedArgs,
            Operation node)
        {
            Operand source = node.GetSource(0);

            Debug.Assert(source.Kind == OperandKind.Constant, "Non-constant LoadArgument source kind.");

            int index = source.AsInt32();

            int intCount = 0;
            int vecCount = 0;

            for (int cIndex = 0; cIndex < index; cIndex++)
            {
                OperandType argType = cctx.FuncArgTypes[cIndex];

                if (argType.IsInteger())
                {
                    intCount++;
                }
                else if (argType == OperandType.V128)
                {
                    intCount += 2;
                }
                else
                {
                    vecCount++;
                }
            }

            bool passOnReg;

            if (source.Type.IsInteger())
            {
                passOnReg = intCount < CallingConvention.GetIntArgumentsOnRegsCount();
            }
            else if (source.Type == OperandType.V128)
            {
                passOnReg = intCount + 1 < CallingConvention.GetIntArgumentsOnRegsCount();
            }
            else
            {
                passOnReg = vecCount < CallingConvention.GetVecArgumentsOnRegsCount();
            }

            if (passOnReg)
            {
                Operand dest = node.Destination;

                if (preservedArgs[index] == default)
                {
                    if (dest.Type == OperandType.V128)
                    {
                        // V128 is a struct, we pass each half on a GPR if possible.
                        Operand pArg = Local(OperandType.V128);

                        Operand argLReg = Gpr(CallingConvention.GetIntArgumentRegister(intCount),     OperandType.I64);
                        Operand argHReg = Gpr(CallingConvention.GetIntArgumentRegister(intCount + 1), OperandType.I64);

                        Operation copyL = Operation(Instruction.VectorCreateScalar, pArg, argLReg);
                        Operation copyH = Operation(Instruction.VectorInsert,       pArg, pArg, argHReg, Const(1));

                        cctx.Cfg.Entry.Operations.AddFirst(copyH);
                        cctx.Cfg.Entry.Operations.AddFirst(copyL);

                        preservedArgs[index] = pArg;
                    }
                    else
                    {
                        Operand pArg = Local(dest.Type);

                        Operand argReg = dest.Type.IsInteger()
                            ? Gpr(CallingConvention.GetIntArgumentRegister(intCount), dest.Type)
                            : Xmm(CallingConvention.GetVecArgumentRegister(vecCount), dest.Type);

                        Operation copyOp = Operation(Instruction.Copy, pArg, argReg);

                        cctx.Cfg.Entry.Operations.AddFirst(copyOp);

                        preservedArgs[index] = pArg;
                    }
                }

                Operation argCopyOp = Operation(Instruction.Copy, dest, preservedArgs[index]);

                Operation newNode = nodes.AddBefore(node, argCopyOp);

                Delete(nodes, node);

                return newNode;
            }
            else
            {
                // TODO: Pass on stack.
                return node;
            }
        }

        private static void HandleReturnWindowsAbi(
            CompilerContext cctx,
            IntrusiveList<Operation> nodes,
            Operand[] preservedArgs,
            Operation node)
        {
            if (node.SourcesCount == 0)
            {
                return;
            }

            Operand source = node.GetSource(0);
            Operand retReg;

            if (source.Type.IsInteger())
            {
                retReg = Gpr(CallingConvention.GetIntReturnRegister(), source.Type);
            }
            else if (source.Type == OperandType.V128)
            {
                if (preservedArgs[0] == default)
                {
                    Operand preservedArg = Local(OperandType.I64);
                    Operand arg0 = Gpr(CallingConvention.GetIntArgumentRegister(0), OperandType.I64);

                    Operation copyOp = Operation(Instruction.Copy, preservedArg, arg0);

                    cctx.Cfg.Entry.Operations.AddFirst(copyOp);

                    preservedArgs[0] = preservedArg;
                }

                retReg = preservedArgs[0];
            }
            else
            {
                retReg = Xmm(CallingConvention.GetVecReturnRegister(), source.Type);
            }

            if (source.Type == OperandType.V128)
            {
                Operation retStoreOp = Operation(Instruction.Store, default, retReg, source);

                nodes.AddBefore(node, retStoreOp);
            }
            else
            {
                Operation retCopyOp = Operation(Instruction.Copy, retReg, source);

                nodes.AddBefore(node, retCopyOp);
            }

            node.SetSources(Array.Empty<Operand>());
        }

        private static void HandleReturnSystemVAbi(IntrusiveList<Operation> nodes, Operation node)
        {
            if (node.SourcesCount == 0)
            {
                return;
            }

            Operand source = node.GetSource(0);

            if (source.Type == OperandType.V128)
            {
                Operand retLReg = Gpr(CallingConvention.GetIntReturnRegister(),     OperandType.I64);
                Operand retHReg = Gpr(CallingConvention.GetIntReturnRegisterHigh(), OperandType.I64);

                nodes.AddBefore(node, Operation(Instruction.VectorExtract, retLReg, source, Const(0)));
                nodes.AddBefore(node, Operation(Instruction.VectorExtract, retHReg, source, Const(1)));
            }
            else
            {
                Operand retReg = source.Type.IsInteger()
                    ? Gpr(CallingConvention.GetIntReturnRegister(), source.Type)
                    : Xmm(CallingConvention.GetVecReturnRegister(), source.Type);

                Operation retCopyOp = Operation(Instruction.Copy, retReg, source);

                nodes.AddBefore(node, retCopyOp);
            }
        }

        private static Operand AddXmmCopy(IntrusiveList<Operation> nodes, Operation node, Operand source)
        {
            Operand temp = Local(source.Type);
            Operand intConst = AddCopy(nodes, node, GetIntConst(source));

            Operation copyOp = Operation(Instruction.VectorCreateScalar, temp, intConst);

            nodes.AddBefore(node, copyOp);

            return temp;
        }

        private static Operand AddCopy(IntrusiveList<Operation> nodes, Operation node, Operand source)
        {
            Operand temp = Local(source.Type);

            Operation copyOp = Operation(Instruction.Copy, temp, source);

            nodes.AddBefore(node, copyOp);

            return temp;
        }

        private static Operand GetIntConst(Operand value)
        {
            if (value.Type == OperandType.FP32)
            {
                return Const(value.AsInt32());
            }
            else if (value.Type == OperandType.FP64)
            {
                return Const(value.AsInt64());
            }

            return value;
        }

        private static void Delete(IntrusiveList<Operation> nodes, Operation node)
        {
            node.Destination = default;

            for (int index = 0; index < node.SourcesCount; index++)
            {
                node.SetSource(index, default);
            }

            nodes.Remove(node);
        }

        private static Operand Gpr(X86Register register, OperandType type)
        {
            return Register((int)register, RegisterType.Integer, type);
        }

        private static Operand Xmm(X86Register register, OperandType type)
        {
            return Register((int)register, RegisterType.Vector, type);
        }

        private static bool IsSameOperandDestSrc1(Operation operation)
        {
            switch (operation.Instruction)
            {
                case Instruction.Add:
                    return !HardwareCapabilities.SupportsVexEncoding && !operation.Destination.Type.IsInteger();
                case Instruction.Multiply:
                case Instruction.Subtract:
                    return !HardwareCapabilities.SupportsVexEncoding || operation.Destination.Type.IsInteger();

                case Instruction.BitwiseAnd:
                case Instruction.BitwiseExclusiveOr:
                case Instruction.BitwiseNot:
                case Instruction.BitwiseOr:
                case Instruction.ByteSwap:
                case Instruction.Negate:
                case Instruction.RotateRight:
                case Instruction.ShiftLeft:
                case Instruction.ShiftRightSI:
                case Instruction.ShiftRightUI:
                    return true;

                case Instruction.Divide:
                    return !HardwareCapabilities.SupportsVexEncoding && !operation.Destination.Type.IsInteger();

                case Instruction.VectorInsert:
                case Instruction.VectorInsert16:
                case Instruction.VectorInsert8:
                    return !HardwareCapabilities.SupportsVexEncoding;

                case Instruction.Extended:
                    return IsIntrinsicSameOperandDestSrc1(operation);
            }

            return IsVexSameOperandDestSrc1(operation);
        }

        private static bool IsIntrinsicSameOperandDestSrc1(Operation operation)
        {
            IntrinsicInfo info = IntrinsicTable.GetInfo(operation.Intrinsic);

            return info.Type == IntrinsicType.Crc32 || info.Type == IntrinsicType.Fma || IsVexSameOperandDestSrc1(operation);
        }

        private static bool IsVexSameOperandDestSrc1(Operation operation)
        {
            if (IsIntrinsic(operation.Instruction))
            {
                IntrinsicInfo info = IntrinsicTable.GetInfo(operation.Intrinsic);

                bool hasVex = HardwareCapabilities.SupportsVexEncoding && Assembler.SupportsVexPrefix(info.Inst);

                bool isUnary = operation.SourcesCount < 2;

                bool hasVecDest = operation.Destination != default && operation.Destination.Type == OperandType.V128;

                return !hasVex && !isUnary && hasVecDest;
            }

            return false;
        }

        private static bool HasConstSrc1(Instruction inst)
        {
            switch (inst)
            {
                case Instruction.Copy:
                case Instruction.LoadArgument:
                case Instruction.Spill:
                case Instruction.SpillArg:
                    return true;
            }

            return false;
        }

        private static bool HasConstSrc2(Instruction inst)
        {
            switch (inst)
            {
                case Instruction.Add:
                case Instruction.BitwiseAnd:
                case Instruction.BitwiseExclusiveOr:
                case Instruction.BitwiseOr:
                case Instruction.BranchIf:
                case Instruction.Compare:
                case Instruction.Multiply:
                case Instruction.RotateRight:
                case Instruction.ShiftLeft:
                case Instruction.ShiftRightSI:
                case Instruction.ShiftRightUI:
                case Instruction.Store:
                case Instruction.Store16:
                case Instruction.Store8:
                case Instruction.Subtract:
                case Instruction.VectorExtract:
                case Instruction.VectorExtract16:
                case Instruction.VectorExtract8:
                    return true;
            }

            return false;
        }

        private static bool IsCommutative(Operation operation)
        {
            switch (operation.Instruction)
            {
                case Instruction.Add:
                case Instruction.BitwiseAnd:
                case Instruction.BitwiseExclusiveOr:
                case Instruction.BitwiseOr:
                case Instruction.Multiply:
                    return true;

                case Instruction.BranchIf:
                case Instruction.Compare:
                {
                    Operand comp = operation.GetSource(2);

                    Debug.Assert(comp.Kind == OperandKind.Constant);

                    var compType = (Comparison)comp.AsInt32();

                    return compType == Comparison.Equal || compType == Comparison.NotEqual;
                }
            }

            return false;
        }

        private static bool IsIntrinsic(Instruction inst)
        {
            return inst == Instruction.Extended;
        }

        private static bool IsXmmIntrinsic(Operation operation)
        {
            if (operation.Instruction != Instruction.Extended)
            {
                return false;
            }

            IntrinsicInfo info = IntrinsicTable.GetInfo(operation.Intrinsic);

            return info.Type != IntrinsicType.Crc32;
        }
    }
}