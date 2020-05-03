using ARMeilleure.Decoders;
using ARMeilleure.Diagnostics;
using ARMeilleure.Instructions;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Memory;
using ARMeilleure.State;
using System;
using System.Collections.Concurrent;
using System.Threading;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;
using static ARMeilleure.IntermediateRepresentation.OperationHelper;

namespace ARMeilleure.Translation
{
    public class Translator
    {
        private const ulong CallFlag = InstEmitFlowHelper.CallFlag;

        private const bool AlwaysTranslateFunctions = true; // If false, only translates a single block for lowCq.

        private readonly IMemoryManager _memory;

        private readonly ConcurrentDictionary<ulong, TranslatedFunction> _funcs;

        private readonly JumpTable _jumpTable;

        private readonly PriorityQueue<RejitRequest> _backgroundQueue;

        private readonly AutoResetEvent _backgroundTranslatorEvent;

        private volatile int _threadCount;

        public Translator(IJitMemoryAllocator allocator, IMemoryManager memory)
        {
            _memory = memory;

            _funcs = new ConcurrentDictionary<ulong, TranslatedFunction>();

            _jumpTable = new JumpTable(allocator);

            _backgroundQueue = new PriorityQueue<RejitRequest>(2);

            _backgroundTranslatorEvent = new AutoResetEvent(false);

            JitCache.Initialize(allocator);
            DirectCallStubs.InitializeStubs();
        }

        private void TranslateQueuedSubs()
        {
            while (_threadCount != 0)
            {
                if (_backgroundQueue.TryDequeue(out RejitRequest request))
                {
                    TranslatedFunction func = Translate(request.Address, request.Mode, highCq: true);

                    _funcs.AddOrUpdate(request.Address, func, (key, oldFunc) => func);
                    _jumpTable.RegisterFunction(request.Address, func);
                }
                else
                {
                    _backgroundTranslatorEvent.WaitOne();
                }
            }
            _backgroundTranslatorEvent.Set(); // Wake up any other background translator threads, to encourage them to exit.
        }

        public void Execute(State.ExecutionContext context, ulong address)
        {
            if (Interlocked.Increment(ref _threadCount) == 1)
            {
                // Simple heuristic, should be user configurable in future. (1 for 4 core/ht or less, 2 for 6 core+ht etc).
                // All threads are normal priority except from the last, which just fills as much of the last core as the os lets it with a low priority.
                // If we only have one rejit thread, it should be normal priority as highCq code is performance critical.
                // TODO: Use physical cores rather than logical. This only really makes sense for processors with hyperthreading. Requires OS specific code.
                int unboundedThreadCount = Math.Max(1, (Environment.ProcessorCount - 6) / 3);
                int threadCount = Math.Min(4, unboundedThreadCount);
                for (int i = 0; i < threadCount; i++)
                {
                    bool last = i != 0 && i == unboundedThreadCount - 1;
                    Thread backgroundTranslatorThread = new Thread(TranslateQueuedSubs)
                    {
                        Name = "CPU.BackgroundTranslatorThread." + i,
                        Priority = last ? ThreadPriority.Lowest : ThreadPriority.Normal
                    };

                    backgroundTranslatorThread.Start();
                }
            }

            Statistics.InitializeTimer();

            NativeInterface.RegisterThread(context, _memory, this);

            do
            {
                address = ExecuteSingle(context, address);
            }
            while (context.Running && (address & ~1UL) != 0);

            NativeInterface.UnregisterThread();

            if (Interlocked.Decrement(ref _threadCount) == 0)
            {
                _backgroundTranslatorEvent.Set();
            }
        }

        public ulong ExecuteSingle(State.ExecutionContext context, ulong address)
        {
            TranslatedFunction func = GetOrTranslate(address, context.ExecutionMode);

            Statistics.StartTimer();

            ulong nextAddr = func.Execute(context);

            Statistics.StopTimer(address);

            return nextAddr;
        }

        internal TranslatedFunction GetOrTranslate(ulong address, ExecutionMode mode)
        {
            // TODO: Investigate how we should handle code at unaligned addresses.
            // Currently, those low bits are used to store special flags.
            bool isCallTarget = (address & CallFlag) != 0;

            address &= ~CallFlag;

            if (!_funcs.TryGetValue(address, out TranslatedFunction func))
            {
                func = Translate(address, mode, highCq: false);

                _funcs.TryAdd(address, func);
            }
            else if (isCallTarget && func.ShouldRejit())
            {
                _backgroundQueue.Enqueue(0, new RejitRequest(address, mode));

                _backgroundTranslatorEvent.Set();
            }

            return func;
        }

        private TranslatedFunction Translate(ulong address, ExecutionMode mode, bool highCq)
        {
            ArmEmitterContext context = new ArmEmitterContext(_memory, _jumpTable, (long)address, highCq, Aarch32Mode.User);

            PrepareOperandPool(highCq);
            PrepareOperationPool(highCq);

            Logger.StartPass(PassName.Decoding);

            Block[] blocks = AlwaysTranslateFunctions
                ? Decoder.DecodeFunction  (_memory, address, mode, highCq)
                : Decoder.DecodeBasicBlock(_memory, address, mode);

            Logger.EndPass(PassName.Decoding);

            Logger.StartPass(PassName.Translation);

            EmitSynchronization(context);

            if (blocks[0].Address != address)
            {
                context.Branch(context.GetLabel(address));
            }

            ControlFlowGraph cfg = EmitAndGetCFG(context, blocks);

            Logger.EndPass(PassName.Translation);

            Logger.StartPass(PassName.RegisterUsage);

            RegisterUsage.RunPass(cfg, mode, isCompleteFunction: false);

            Logger.EndPass(PassName.RegisterUsage);

            OperandType[] argTypes = new OperandType[] { OperandType.I64 };

            CompilerOptions options = highCq ? CompilerOptions.HighCq : CompilerOptions.None;

            GuestFunction func = Compiler.Compile<GuestFunction>(cfg, argTypes, OperandType.I64, options);

            ResetOperandPool(highCq);
            ResetOperationPool(highCq);

            return new TranslatedFunction(func, rejit: !highCq);
        }

        private static ControlFlowGraph EmitAndGetCFG(ArmEmitterContext context, Block[] blocks)
        {
            for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
            {
                Block block = blocks[blkIndex];

                context.CurrBlock = block;

                context.MarkLabel(context.GetLabel(block.Address));

                for (int opcIndex = 0; opcIndex < block.OpCodes.Count; opcIndex++)
                {
                    OpCode opCode = block.OpCodes[opcIndex];

                    context.CurrOp = opCode;

                    bool isLastOp = opcIndex == block.OpCodes.Count - 1;

                    if (isLastOp && block.Branch != null && block.Branch.Address <= block.Address)
                    {
                        EmitSynchronization(context);
                    }

                    Operand lblPredicateSkip = null;

                    if (opCode is OpCode32 op && op.Cond < Condition.Al)
                    {
                        lblPredicateSkip = Label();

                        InstEmitFlowHelper.EmitCondBranch(context, lblPredicateSkip, op.Cond.Invert());
                    }

                    if (opCode.Instruction.Emitter != null)
                    {
                        opCode.Instruction.Emitter(context);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Invalid instruction \"{opCode.Instruction.Name}\".");
                    }

                    if (lblPredicateSkip != null)
                    {
                        context.MarkLabel(lblPredicateSkip);

                        // If this is the last op on the block, and there's no "next" block
                        // after this one, then we have to return right now, with the address
                        // of the next instruction to be executed (in the case that the condition
                        // is false, and the branch was not taken, as all basic blocks should end
                        // with some kind of branch).
                        if (isLastOp && block.Next == null)
                        {
                            InstEmitFlowHelper.EmitTailContinue(context, Const(opCode.Address + (ulong)opCode.OpCodeSizeInBytes));
                        }
                    }
                }
            }

            return context.GetControlFlowGraph();
        }

        private static void EmitSynchronization(EmitterContext context)
        {
            long countOffs = NativeContext.GetCounterOffset();

            Operand countAddr = context.Add(context.LoadArgument(OperandType.I64, 0), Const(countOffs));

            Operand count = context.Load(OperandType.I32, countAddr);

            Operand lblNonZero = Label();
            Operand lblExit    = Label();

            context.BranchIfTrue(lblNonZero, count);

            Operand running = context.Call(new _Bool(NativeInterface.CheckSynchronization));

            context.BranchIfTrue(lblExit, running);

            context.Return(Const(0L));

            context.Branch(lblExit);

            context.MarkLabel(lblNonZero);

            count = context.Subtract(count, Const(1));

            context.Store(countAddr, count);

            context.MarkLabel(lblExit);
        }
    }
}