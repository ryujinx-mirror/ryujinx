using ChocolArm64.Instructions;
using ChocolArm64.Memory;
using ChocolArm64.State;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ChocolArm64.Decoders
{
    static class Decoder
    {
        private delegate object OpActivator(Inst inst, long position, int opCode);

        private static ConcurrentDictionary<Type, OpActivator> _opActivators;

        static Decoder()
        {
            _opActivators = new ConcurrentDictionary<Type, OpActivator>();
        }

        public static Block DecodeBasicBlock(MemoryManager memory, long start, ExecutionMode mode)
        {
            Block block = new Block(start);

            FillBlock(memory, mode, block);

            return block;
        }

        public static Block DecodeSubroutine(
            TranslatorCache cache,
            MemoryManager   memory,
            long            start,
            ExecutionMode   mode)
        {
            Dictionary<long, Block> visited    = new Dictionary<long, Block>();
            Dictionary<long, Block> visitedEnd = new Dictionary<long, Block>();

            Queue<Block> blocks = new Queue<Block>();

            Block Enqueue(long position)
            {
                if (!visited.TryGetValue(position, out Block output))
                {
                    output = new Block(position);

                    blocks.Enqueue(output);

                    visited.Add(position, output);
                }

                return output;
            }

            Block entry = Enqueue(start);

            while (blocks.Count > 0)
            {
                Block current = blocks.Dequeue();

                FillBlock(memory, mode, current);

                //Set child blocks. "Branch" is the block the branch instruction
                //points to (when taken), "Next" is the block at the next address,
                //executed when the branch is not taken. For Unconditional Branches
                //(except BL/BLR that are sub calls) or end of executable, Next is null.
                if (current.OpCodes.Count > 0)
                {
                    bool hasCachedSub = false;

                    OpCode64 lastOp = current.GetLastOp();

                    if (lastOp is IOpCodeBImm op)
                    {
                        if (op.Emitter == InstEmit.Bl)
                        {
                            hasCachedSub = cache.HasSubroutine(op.Imm);
                        }
                        else
                        {
                            current.Branch = Enqueue(op.Imm);
                        }
                    }

                    if (!IsUnconditionalBranch(lastOp) || hasCachedSub)
                    {
                        current.Next = Enqueue(current.EndPosition);
                    }
                }

                //If we have on the graph two blocks with the same end position,
                //then we need to split the bigger block and have two small blocks,
                //the end position of the bigger "Current" block should then be == to
                //the position of the "Smaller" block.
                while (visitedEnd.TryGetValue(current.EndPosition, out Block smaller))
                {
                    if (current.Position > smaller.Position)
                    {
                        Block temp = smaller;

                        smaller = current;
                        current = temp;
                    }

                    current.EndPosition = smaller.Position;
                    current.Next        = smaller;
                    current.Branch      = null;

                    current.OpCodes.RemoveRange(
                        current.OpCodes.Count - smaller.OpCodes.Count,
                        smaller.OpCodes.Count);

                    visitedEnd[smaller.EndPosition] = smaller;
                }

                visitedEnd.Add(current.EndPosition, current);
            }

            return entry;
        }

        private static void FillBlock(MemoryManager memory, ExecutionMode mode, Block block)
        {
            long position = block.Position;

            OpCode64 opCode;

            do
            {
                opCode = DecodeOpCode(memory, position, mode);

                block.OpCodes.Add(opCode);

                position += opCode.OpCodeSizeInBytes;
            }
            while (!(IsBranch(opCode) || IsException(opCode)));

            block.EndPosition = position;
        }

        private static bool IsBranch(OpCode64 opCode)
        {
            return opCode is OpCodeBImm64 ||
                   opCode is OpCodeBReg64 || IsAarch32Branch(opCode);
        }

        private static bool IsUnconditionalBranch(OpCode64 opCode)
        {
            return opCode is OpCodeBImmAl64 ||
                   opCode is OpCodeBReg64   || IsAarch32UnconditionalBranch(opCode);
        }

        private static bool IsAarch32UnconditionalBranch(OpCode64 opCode)
        {
            if (!(opCode is OpCode32 op))
            {
                return false;
            }

            //Note: On ARM32, most instructions have conditional execution,
            //so there's no "Always" (unconditional) branch like on ARM64.
            //We need to check if the condition is "Always" instead.
            return IsAarch32Branch(op) && op.Cond >= Condition.Al;
        }

        private static bool IsAarch32Branch(OpCode64 opCode)
        {
            //Note: On ARM32, most ALU operations can write to R15 (PC),
            //so we must consider such operations as a branch in potential aswell.
            if (opCode is IOpCode32Alu opAlu && opAlu.Rd == RegisterAlias.Aarch32Pc)
            {
                return true;
            }

            //Same thing for memory operations. We have the cases where PC is a target
            //register (Rt == 15 or (mask & (1 << 15)) != 0), and cases where there is
            //a write back to PC (wback == true && Rn == 15), however the later may
            //be "undefined" depending on the CPU, so compilers should not produce that.
            if (opCode is IOpCode32Mem || opCode is IOpCode32MemMult)
            {
                int rt, rn;

                bool wBack, isLoad;

                if (opCode is IOpCode32Mem opMem)
                {
                    rt     = opMem.Rt;
                    rn     = opMem.Rn;
                    wBack  = opMem.WBack;
                    isLoad = opMem.IsLoad;

                    //For the dual load, we also need to take into account the
                    //case were Rt2 == 15 (PC).
                    if (rt == 14 && opMem.Emitter == InstEmit32.Ldrd)
                    {
                        rt = RegisterAlias.Aarch32Pc;
                    }
                }
                else if (opCode is IOpCode32MemMult opMemMult)
                {
                    const int pcMask = 1 << RegisterAlias.Aarch32Pc;

                    rt     = (opMemMult.RegisterMask & pcMask) != 0 ? RegisterAlias.Aarch32Pc : 0;
                    rn     =  opMemMult.Rn;
                    wBack  =  opMemMult.PostOffset != 0;
                    isLoad =  opMemMult.IsLoad;
                }
                else
                {
                    throw new NotImplementedException($"The type \"{opCode.GetType().Name}\" is not implemented on the decoder.");
                }

                if ((rt == RegisterAlias.Aarch32Pc && isLoad) ||
                    (rn == RegisterAlias.Aarch32Pc && wBack))
                {
                    return true;
                }
            }

            //Explicit branch instructions.
            return opCode is IOpCode32BImm ||
                   opCode is IOpCode32BReg;
        }

        private static bool IsException(OpCode64 opCode)
        {
            return opCode.Emitter == InstEmit.Brk ||
                   opCode.Emitter == InstEmit.Svc ||
                   opCode.Emitter == InstEmit.Und;
        }

        public static OpCode64 DecodeOpCode(MemoryManager memory, long position, ExecutionMode mode)
        {
            int opCode = memory.ReadInt32(position);

            Inst inst;

            if (mode == ExecutionMode.Aarch64)
            {
                inst = OpCodeTable.GetInstA64(opCode);
            }
            else
            {
                if (mode == ExecutionMode.Aarch32Arm)
                {
                    inst = OpCodeTable.GetInstA32(opCode);
                }
                else /* if (mode == ExecutionMode.Aarch32Thumb) */
                {
                    inst = OpCodeTable.GetInstT32(opCode);
                }
            }

            OpCode64 decodedOpCode = new OpCode64(Inst.Undefined, position, opCode);

            if (inst.Type != null)
            {
                decodedOpCode = MakeOpCode(inst.Type, inst, position, opCode);
            }

            return decodedOpCode;
        }

        private static OpCode64 MakeOpCode(Type type, Inst inst, long position, int opCode)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            OpActivator createInstance = _opActivators.GetOrAdd(type, CacheOpActivator);

            return (OpCode64)createInstance(inst, position, opCode);
        }

        private static OpActivator CacheOpActivator(Type type)
        {
            Type[] argTypes = new Type[] { typeof(Inst), typeof(long), typeof(int) };

            DynamicMethod mthd = new DynamicMethod($"Make{type.Name}", type, argTypes);

            ILGenerator generator = mthd.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Newobj, type.GetConstructor(argTypes));
            generator.Emit(OpCodes.Ret);

            return (OpActivator)mthd.CreateDelegate(typeof(OpActivator));
        }
    }
}