using ARMeilleure.Decoders;
using ARMeilleure.Diagnostics;
using ARMeilleure.Instructions;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Memory;
using ARMeilleure.State;
using ARMeilleure.Translation.Cache;
using ARMeilleure.Translation.PTC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Threading;

using static ARMeilleure.Common.BitMapPool;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;
using static ARMeilleure.IntermediateRepresentation.OperationHelper;

namespace ARMeilleure.Translation
{
    public class Translator
    {
        private readonly IJitMemoryAllocator _allocator;
        private readonly IMemoryManager _memory;

        private readonly ConcurrentDictionary<ulong, TranslatedFunction> _funcs;
        private readonly ConcurrentQueue<KeyValuePair<ulong, IntPtr>> _oldFuncs;

        private readonly ConcurrentStack<RejitRequest> _backgroundStack;
        private readonly AutoResetEvent _backgroundTranslatorEvent;
        private readonly ReaderWriterLock _backgroundTranslatorLock;

        private JumpTable _jumpTable;
        internal JumpTable JumpTable => _jumpTable;

        private volatile int _threadCount;

        // FIXME: Remove this once the init logic of the emulator will be redone.
        public static ManualResetEvent IsReadyForTranslation = new ManualResetEvent(false);

        public Translator(IJitMemoryAllocator allocator, IMemoryManager memory)
        {
            _allocator = allocator;
            _memory = memory;

            _funcs = new ConcurrentDictionary<ulong, TranslatedFunction>();
            _oldFuncs = new ConcurrentQueue<KeyValuePair<ulong, IntPtr>>();

            _backgroundStack = new ConcurrentStack<RejitRequest>();
            _backgroundTranslatorEvent = new AutoResetEvent(false);
            _backgroundTranslatorLock = new ReaderWriterLock();

            JitCache.Initialize(allocator);

            DirectCallStubs.InitializeStubs();
        }

        private void TranslateStackedSubs()
        {
            while (_threadCount != 0)
            {
                _backgroundTranslatorLock.AcquireReaderLock(Timeout.Infinite);

                if (_backgroundStack.TryPop(out RejitRequest request))
                {
                    TranslatedFunction func = Translate(_memory, _jumpTable, request.Address, request.Mode, highCq: true);

                    _funcs.AddOrUpdate(request.Address, func, (key, oldFunc) =>
                    {
                        EnqueueForDeletion(key, oldFunc);
                        return func;
                    });

                    _jumpTable.RegisterFunction(request.Address, func);

                    if (PtcProfiler.Enabled)
                    {
                        PtcProfiler.UpdateEntry(request.Address, request.Mode, highCq: true);
                    }

                    _backgroundTranslatorLock.ReleaseReaderLock();
                }
                else
                {
                    _backgroundTranslatorLock.ReleaseReaderLock();
                    _backgroundTranslatorEvent.WaitOne();
                }
            }

            _backgroundTranslatorEvent.Set(); // Wake up any other background translator threads, to encourage them to exit.
        }

        public void Execute(State.ExecutionContext context, ulong address)
        {
            if (Interlocked.Increment(ref _threadCount) == 1)
            {
                IsReadyForTranslation.WaitOne();

                Debug.Assert(_jumpTable == null);
                _jumpTable = new JumpTable(_allocator);

                if (Ptc.State == PtcState.Enabled)
                {
                    Ptc.LoadTranslations(_funcs, _memory, _jumpTable);
                    Ptc.MakeAndSaveTranslations(_funcs, _memory, _jumpTable);
                }

                PtcProfiler.Start();

                Ptc.Disable();

                // Simple heuristic, should be user configurable in future. (1 for 4 core/ht or less, 2 for 6 core+ht etc).
                // All threads are normal priority except from the last, which just fills as much of the last core as the os lets it with a low priority.
                // If we only have one rejit thread, it should be normal priority as highCq code is performance critical.
                // TODO: Use physical cores rather than logical. This only really makes sense for processors with hyperthreading. Requires OS specific code.
                int unboundedThreadCount = Math.Max(1, (Environment.ProcessorCount - 6) / 3);
                int threadCount          = Math.Min(4, unboundedThreadCount);

                for (int i = 0; i < threadCount; i++)
                {
                    bool last = i != 0 && i == unboundedThreadCount - 1;

                    Thread backgroundTranslatorThread = new Thread(TranslateStackedSubs)
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
            while (context.Running && address != 0);

            NativeInterface.UnregisterThread();

            if (Interlocked.Decrement(ref _threadCount) == 0)
            {
                _backgroundTranslatorEvent.Set();

                ClearJitCache();

                DisposePools();

                _jumpTable.Dispose();
                _jumpTable = null;

                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
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

        internal TranslatedFunction GetOrTranslate(ulong address, ExecutionMode mode, bool hintRejit = false)
        {
            if (!_funcs.TryGetValue(address, out TranslatedFunction func))
            {
                func = Translate(_memory, _jumpTable, address, mode, highCq: false);

                TranslatedFunction getFunc = _funcs.GetOrAdd(address, func);

                if (getFunc != func)
                {
                    JitCache.Unmap(func.FuncPtr);
                    func = getFunc;
                }

                if (PtcProfiler.Enabled)
                {
                    PtcProfiler.AddEntry(address, mode, highCq: false);
                }
            }

            if (hintRejit && func.ShouldRejit())
            {
                _backgroundStack.Push(new RejitRequest(address, mode));
                _backgroundTranslatorEvent.Set();
            }

            return func;
        }

        internal static TranslatedFunction Translate(IMemoryManager memory, JumpTable jumpTable, ulong address, ExecutionMode mode, bool highCq)
        {
            ArmEmitterContext context = new ArmEmitterContext(memory, jumpTable, address, highCq, Aarch32Mode.User);

            Logger.StartPass(PassName.Decoding);

            Block[] blocks = Decoder.Decode(memory, address, mode, highCq, singleBlock: false);

            Logger.EndPass(PassName.Decoding);

            PreparePool(highCq ? 1 : 0);

            Logger.StartPass(PassName.Translation);

            EmitSynchronization(context);

            if (blocks[0].Address != address)
            {
                context.Branch(context.GetLabel(address));
            }

            ControlFlowGraph cfg = EmitAndGetCFG(context, blocks, out Range funcRange);

            ulong funcSize = funcRange.End - funcRange.Start;

            Logger.EndPass(PassName.Translation);

            Logger.StartPass(PassName.RegisterUsage);

            RegisterUsage.RunPass(cfg, mode);

            Logger.EndPass(PassName.RegisterUsage);

            OperandType[] argTypes = new OperandType[] { OperandType.I64 };

            CompilerOptions options = highCq ? CompilerOptions.HighCq : CompilerOptions.None;

            GuestFunction func;

            if (Ptc.State == PtcState.Disabled)
            {
                func = Compiler.Compile<GuestFunction>(cfg, argTypes, OperandType.I64, options);

                ResetPool(highCq ? 1 : 0);
            }
            else
            {
                using PtcInfo ptcInfo = new PtcInfo();

                func = Compiler.Compile<GuestFunction>(cfg, argTypes, OperandType.I64, options, ptcInfo);

                ResetPool(highCq ? 1 : 0);

                Ptc.WriteInfoCodeRelocUnwindInfo(address, funcSize, highCq, ptcInfo);
            }

            return new TranslatedFunction(func, funcSize, highCq);
        }

        internal static void PreparePool(int groupId = 0)
        {
            PrepareOperandPool(groupId);
            PrepareOperationPool(groupId);
        }

        internal static void ResetPool(int groupId = 0)
        {
            ResetOperationPool(groupId);
            ResetOperandPool(groupId);
        }

        internal static void DisposePools()
        {
            DisposeOperandPools();
            DisposeOperationPools();
            DisposeBitMapPools();
        }

        private struct Range
        {
            public ulong Start { get; }
            public ulong End { get; }

            public Range(ulong start, ulong end)
            {
                Start = start;
                End = end;
            }
        }

        private static ControlFlowGraph EmitAndGetCFG(ArmEmitterContext context, Block[] blocks, out Range range)
        {
            ulong rangeStart = ulong.MaxValue;
            ulong rangeEnd = 0;

            for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
            {
                Block block = blocks[blkIndex];

                if (!block.Exit)
                {
                    if (rangeStart > block.Address)
                    {
                        rangeStart = block.Address;
                    }

                    if (rangeEnd < block.EndAddress)
                    {
                        rangeEnd = block.EndAddress;
                    }
                }

                context.CurrBlock = block;

                context.MarkLabel(context.GetLabel(block.Address));

                if (block.Exit)
                {
                    InstEmitFlowHelper.EmitTailContinue(context, Const(block.Address), block.TailCall);
                }
                else
                {
                    for (int opcIndex = 0; opcIndex < block.OpCodes.Count; opcIndex++)
                    {
                        OpCode opCode = block.OpCodes[opcIndex];

                        context.CurrOp = opCode;

                        bool isLastOp = opcIndex == block.OpCodes.Count - 1;

                        if (isLastOp && block.Branch != null && !block.Branch.Exit && block.Branch.Address <= block.Address)
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
                        }
                    }
                }
            }

            range = new Range(rangeStart, rangeEnd);

            return context.GetControlFlowGraph();
        }

        internal static void EmitSynchronization(EmitterContext context)
        {
            long countOffs = NativeContext.GetCounterOffset();

            Operand countAddr = context.Add(context.LoadArgument(OperandType.I64, 0), Const(countOffs));

            Operand count = context.Load(OperandType.I32, countAddr);

            Operand lblNonZero = Label();
            Operand lblExit    = Label();

            context.BranchIfTrue(lblNonZero, count, BasicBlockFrequency.Cold);

            Operand running = context.Call(typeof(NativeInterface).GetMethod(nameof(NativeInterface.CheckSynchronization)));

            context.BranchIfTrue(lblExit, running, BasicBlockFrequency.Cold);

            context.Return(Const(0L));

            context.MarkLabel(lblNonZero);

            count = context.Subtract(count, Const(1));

            context.Store(countAddr, count);

            context.MarkLabel(lblExit);
        }

        public void InvalidateJitCacheRegion(ulong address, ulong size)
        {
            // If rejit is running, stop it as it may be trying to rejit a function on the invalidated region.
            ClearRejitQueue(allowRequeue: true);

            // TODO: Completely remove functions overlapping the specified range from the cache.
        }

        private void EnqueueForDeletion(ulong guestAddress, TranslatedFunction func)
        {
            _oldFuncs.Enqueue(new KeyValuePair<ulong, IntPtr>(guestAddress, func.FuncPtr));
        }

        private void ClearJitCache()
        {
            // Ensure no attempt will be made to compile new functions due to rejit.
            ClearRejitQueue(allowRequeue: false);

            foreach (var kv in _funcs)
            {
                JitCache.Unmap(kv.Value.FuncPtr);
            }

            _funcs.Clear();

            while (_oldFuncs.TryDequeue(out var kv))
            {
                JitCache.Unmap(kv.Value);
            }
        }

        private void ClearRejitQueue(bool allowRequeue)
        {
            _backgroundTranslatorLock.AcquireWriterLock(Timeout.Infinite);

            if (allowRequeue)
            {
                while (_backgroundStack.TryPop(out var request))
                {
                    if (_funcs.TryGetValue(request.Address, out var func))
                    {
                        func.ResetCallCount();
                    }
                }
            }
            else
            {
                _backgroundStack.Clear();
            }

            _backgroundTranslatorLock.ReleaseWriterLock();
        }
    }
}
