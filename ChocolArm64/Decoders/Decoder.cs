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

        public static Block DecodeBasicBlock(CpuThreadState state, MemoryManager memory, long start)
        {
            Block block = new Block(start);

            FillBlock(state, memory, block);

            return block;
        }

        public static (Block[] Graph, Block Root) DecodeSubroutine(
            TranslatorCache  cache,
            CpuThreadState   state,
            MemoryManager    memory,
            long             start)
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

            Block root = Enqueue(start);

            while (blocks.Count > 0)
            {
                Block current = blocks.Dequeue();

                FillBlock(state, memory, current);

                //Set child blocks. "Branch" is the block the branch instruction
                //points to (when taken), "Next" is the block at the next address,
                //executed when the branch is not taken. For Unconditional Branches
                //(except BL/BLR that are sub calls) or end of executable, Next is null.
                if (current.OpCodes.Count > 0)
                {
                    bool hasCachedSub = false;

                    OpCode64 lastOp = current.GetLastOp();

                    if (lastOp is OpCodeBImm64 op)
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

                    if (!((lastOp is OpCodeBImmAl64) ||
                          (lastOp is OpCodeBReg64)) || hasCachedSub)
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

            //Make and sort Graph blocks array by position.
            Block[] graph = new Block[visited.Count];

            while (visited.Count > 0)
            {
                ulong firstPos = ulong.MaxValue;

                foreach (Block block in visited.Values)
                {
                    if (firstPos > (ulong)block.Position)
                        firstPos = (ulong)block.Position;
                }

                Block current = visited[(long)firstPos];

                do
                {
                    graph[graph.Length - visited.Count] = current;

                    visited.Remove(current.Position);

                    current = current.Next;
                }
                while (current != null);
            }

            return (graph, root);
        }

        private static void FillBlock(CpuThreadState state, MemoryManager memory, Block block)
        {
            long position = block.Position;

            OpCode64 opCode;

            do
            {
                //TODO: This needs to be changed to support both AArch32 and AArch64,
                //once JIT support is introduced on AArch32 aswell.
                opCode = DecodeOpCode(state, memory, position);

                block.OpCodes.Add(opCode);

                position += 4;
            }
            while (!(IsBranch(opCode) || IsException(opCode)));

            block.EndPosition = position;
        }

        private static bool IsBranch(OpCode64 opCode)
        {
            return opCode is OpCodeBImm64 ||
                   opCode is OpCodeBReg64;
        }

        private static bool IsException(OpCode64 opCode)
        {
            return opCode.Emitter == InstEmit.Brk ||
                   opCode.Emitter == InstEmit.Svc ||
                   opCode.Emitter == InstEmit.Und;
        }

        public static OpCode64 DecodeOpCode(CpuThreadState state, MemoryManager memory, long position)
        {
            int opCode = memory.ReadInt32(position);

            Inst inst;

            if (state.ExecutionMode == ExecutionMode.AArch64)
            {
                inst = OpCodeTable.GetInstA64(opCode);
            }
            else
            {
                //TODO: Thumb support.
                inst = OpCodeTable.GetInstA32(opCode);
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