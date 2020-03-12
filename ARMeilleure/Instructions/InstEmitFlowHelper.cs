using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static class InstEmitFlowHelper
    {
        public const ulong CallFlag = 1;

        public static void EmitCondBranch(ArmEmitterContext context, Operand target, Condition cond)
        {
            if (cond != Condition.Al)
            {
                context.BranchIfTrue(target, GetCondTrue(context, cond));
            }
            else
            {
                context.Branch(target);
            }
        }

        public static Operand GetCondTrue(ArmEmitterContext context, Condition condition)
        {
            Operand cmpResult = context.TryGetComparisonResult(condition);

            if (cmpResult != null)
            {
                return cmpResult;
            }

            Operand value = Const(1);

            Operand Inverse(Operand val)
            {
                return context.BitwiseExclusiveOr(val, Const(1));
            }

            switch (condition)
            {
                case Condition.Eq:
                    value = GetFlag(PState.ZFlag);
                    break;

                case Condition.Ne:
                    value = Inverse(GetFlag(PState.ZFlag));
                    break;

                case Condition.GeUn:
                    value = GetFlag(PState.CFlag);
                    break;

                case Condition.LtUn:
                    value = Inverse(GetFlag(PState.CFlag));
                    break;

                case Condition.Mi:
                    value = GetFlag(PState.NFlag);
                    break;

                case Condition.Pl:
                    value = Inverse(GetFlag(PState.NFlag));
                    break;

                case Condition.Vs:
                    value = GetFlag(PState.VFlag);
                    break;

                case Condition.Vc:
                    value = Inverse(GetFlag(PState.VFlag));
                    break;

                case Condition.GtUn:
                {
                    Operand c = GetFlag(PState.CFlag);
                    Operand z = GetFlag(PState.ZFlag);

                    value = context.BitwiseAnd(c, Inverse(z));

                    break;
                }

                case Condition.LeUn:
                {
                    Operand c = GetFlag(PState.CFlag);
                    Operand z = GetFlag(PState.ZFlag);

                    value = context.BitwiseOr(Inverse(c), z);

                    break;
                }

                case Condition.Ge:
                {
                    Operand n = GetFlag(PState.NFlag);
                    Operand v = GetFlag(PState.VFlag);

                    value = context.ICompareEqual(n, v);

                    break;
                }

                case Condition.Lt:
                {
                    Operand n = GetFlag(PState.NFlag);
                    Operand v = GetFlag(PState.VFlag);

                    value = context.ICompareNotEqual(n, v);

                    break;
                }

                case Condition.Gt:
                {
                    Operand n = GetFlag(PState.NFlag);
                    Operand z = GetFlag(PState.ZFlag);
                    Operand v = GetFlag(PState.VFlag);

                    value = context.BitwiseAnd(Inverse(z), context.ICompareEqual(n, v));

                    break;
                }

                case Condition.Le:
                {
                    Operand n = GetFlag(PState.NFlag);
                    Operand z = GetFlag(PState.ZFlag);
                    Operand v = GetFlag(PState.VFlag);

                    value = context.BitwiseOr(z, context.ICompareNotEqual(n, v));

                    break;
                }
            }

            return value;
        }

        public static void EmitCall(ArmEmitterContext context, ulong immediate)
        {
            EmitJumpTableBranch(context, Const(immediate));
        }

        private static void EmitNativeCall(ArmEmitterContext context, Operand nativeContextPtr, Operand funcAddr, bool isJump = false)
        {
            context.StoreToContext();
            Operand returnAddress;
            if (isJump)
            {
                context.Tailcall(funcAddr, nativeContextPtr);
            }
            else
            {
                returnAddress = context.Call(funcAddr, OperandType.I64, nativeContextPtr);
                context.LoadFromContext();

                EmitContinueOrReturnCheck(context, returnAddress);
            }
        }

        private static void EmitNativeCall(ArmEmitterContext context, Operand funcAddr, bool isJump = false)
        {
            EmitNativeCall(context, context.LoadArgument(OperandType.I64, 0), funcAddr, isJump);
        }

        public static void EmitVirtualCall(ArmEmitterContext context, Operand target)
        {
            EmitVirtualCallOrJump(context, target, isJump: false);
        }

        public static void EmitVirtualJump(ArmEmitterContext context, Operand target, bool isReturn)
        {
            EmitVirtualCallOrJump(context, target, isJump: true, isReturn: isReturn);
        }

        private static void EmitVirtualCallOrJump(ArmEmitterContext context, Operand target, bool isJump, bool isReturn = false)
        {
            if (isReturn)
            {
                context.Return(target);
            }
            else
            {
                EmitJumpTableBranch(context, target, isJump);
            }
        }

        private static void EmitContinueOrReturnCheck(ArmEmitterContext context, Operand returnAddress)
        {
            // Note: The return value of a translated function is always an Int64 with the
            // address execution has returned to. We expect this address to be immediately after the
            // current instruction, if it isn't we keep returning until we reach the dispatcher.
            Operand nextAddr = Const(GetNextOpAddress(context.CurrOp));

            // Try to continue within this block.
            // If the return address isn't to our next instruction, we need to return so the JIT can figure out what to do.
            Operand lblContinue = Label();

            // We need to clear out the call flag for the return address before comparing it.
            context.BranchIfTrue(lblContinue, context.ICompareEqual(context.BitwiseAnd(returnAddress, Const(~CallFlag)), nextAddr));

            context.Return(returnAddress);

            context.MarkLabel(lblContinue);

            if (context.CurrBlock.Next == null)
            {
                // No code following this instruction, try and find the next block and jump to it.
                EmitTailContinue(context, nextAddr);
            }
        }

        private static ulong GetNextOpAddress(OpCode op)
        {
            return op.Address + (ulong)op.OpCodeSizeInBytes;
        }

        public static void EmitTailContinue(ArmEmitterContext context, Operand address, bool allowRejit = false)
        {
            bool useTailContinue = true; // Left option here as it may be useful if we need to return to managed rather than tail call in future. (eg. for debug)
            if (useTailContinue)
            {
                if (allowRejit)
                {
                    address = context.BitwiseOr(address, Const(1L));
                }

                Operand fallbackAddr = context.Call(new _U64_U64(NativeInterface.GetFunctionAddress), address);

                EmitNativeCall(context, fallbackAddr, true);
            } 
            else
            {
                context.Return(address);
            }
        }

        private static void EmitNativeCallWithGuestAddress(ArmEmitterContext context, Operand funcAddr, Operand guestAddress, bool isJump)
        {
            Operand nativeContextPtr = context.LoadArgument(OperandType.I64, 0);
            context.Store(context.Add(nativeContextPtr, Const(NativeContext.GetCallAddressOffset())), guestAddress);

            EmitNativeCall(context, nativeContextPtr, funcAddr, isJump);
        }

        private static void EmitBranchFallback(ArmEmitterContext context, Operand address, bool isJump)
        {
            address = context.BitwiseOr(address, Const(address.Type, (long)CallFlag)); // Set call flag.
            Operand fallbackAddr = context.Call(new _U64_U64(NativeInterface.GetFunctionAddress), address);
            EmitNativeCall(context, fallbackAddr, isJump);
        }

        public static void EmitDynamicTableCall(ArmEmitterContext context, Operand tableAddress, Operand address, bool isJump)
        {
            // Loop over elements of the dynamic table. Unrolled loop.

            Operand endLabel = Label();
            Operand fallbackLabel = Label();

            Action<Operand> emitTableEntry = (Operand entrySkipLabel) =>
            {
                // Try to take this entry in the table if its guest address equals 0.
                Operand gotResult = context.CompareAndSwap(tableAddress, Const(0L), address);

                // Is the address ours? (either taken via CompareAndSwap (0), or what was already here)
                context.BranchIfFalse(entrySkipLabel, context.BitwiseOr(context.ICompareEqual(gotResult, address), context.ICompareEqual(gotResult, Const(0L))));

                // It's ours, so what function is it pointing to?
                Operand targetFunctionPtr = context.Add(tableAddress, Const(8L));
                Operand targetFunction = context.Load(OperandType.I64, targetFunctionPtr);

                // Call the function.
                // We pass in the entry address as the guest address, as the entry may need to be updated by the indirect call stub.
                EmitNativeCallWithGuestAddress(context, targetFunction, tableAddress, isJump);
                context.Branch(endLabel);
            };

            // Currently this uses a size of 1, as higher values inflate code size for no real benefit.
            for (int i = 0; i < JumpTable.DynamicTableElems; i++) 
            {
                if (i == JumpTable.DynamicTableElems - 1)
                {
                    emitTableEntry(fallbackLabel); // If this is the last entry, avoid emitting the additional label and add.
                } 
                else
                {
                    Operand nextLabel = Label();

                    emitTableEntry(nextLabel);

                    context.MarkLabel(nextLabel);
                    tableAddress = context.Add(tableAddress, Const((long)JumpTable.JumpTableStride)); // Move to the next table entry.
                }
            }

            context.MarkLabel(fallbackLabel);

            EmitBranchFallback(context, address, isJump);

            context.MarkLabel(endLabel);
        }

        public static void EmitJumpTableBranch(ArmEmitterContext context, Operand address, bool isJump = false)
        {
            if (address.Type == OperandType.I32)
            {
                address = context.ZeroExtend32(OperandType.I64, address);
            }

            // TODO: Constant folding. Indirect calls are slower in the best case and emit more code so we want to avoid them when possible.
            bool isConst = address.Kind == OperandKind.Constant;
            long constAddr = (long)address.Value;

            if (!context.HighCq)
            {
                // Don't emit indirect calls or jumps if we're compiling in lowCq mode.
                // This avoids wasting space on the jump and indirect tables.
                // Just ask the translator for the function address.

                EmitBranchFallback(context, address, isJump);
            }
            else if (!isConst)
            {
                // Virtual branch/call - store first used addresses on a small table for fast lookup.
                int entry = context.JumpTable.ReserveDynamicEntry(isJump);

                int jumpOffset = entry * JumpTable.JumpTableStride * JumpTable.DynamicTableElems;
                Operand dynTablePtr = Const(context.JumpTable.DynamicPointer.ToInt64() + jumpOffset);

                EmitDynamicTableCall(context, dynTablePtr, address, isJump);
            }
            else
            {
                int entry = context.JumpTable.ReserveTableEntry(context.BaseAddress & (~3L), constAddr, isJump);

                int jumpOffset = entry * JumpTable.JumpTableStride + 8; // Offset directly to the host address.

                // TODO: Relocatable jump table ptr for AOT. Would prefer a solution to patch this constant into functions as they are loaded rather than calculate at runtime.
                Operand tableEntryPtr = Const(context.JumpTable.JumpPointer.ToInt64() + jumpOffset);

                Operand funcAddr = context.Load(OperandType.I64, tableEntryPtr);

                EmitNativeCallWithGuestAddress(context, funcAddr, address, isJump); // Call the function directly. If it's not present yet, this will call the direct call stub.
            }
        }
    }
}
