using ARMeilleure.Common;
using ARMeilleure.Decoders;
using ARMeilleure.Diagnostics;
using ARMeilleure.Instructions;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Memory;
using ARMeilleure.State;
using ARMeilleure.Translation.Cache;
using ARMeilleure.Translation.PTC;
using Ryujinx.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Threading;

using static ARMeilleure.Common.BitMapPool;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;
using static ARMeilleure.IntermediateRepresentation.OperationHelper;

namespace ARMeilleure.Translation
{
    public class Translator
    {
        private const int CountTableCapacity = 4 * 1024 * 1024;

        private readonly IJitMemoryAllocator _allocator;
        private readonly IMemoryManager _memory;

        private readonly ConcurrentDictionary<ulong, TranslatedFunction> _funcs;
        private readonly ConcurrentQueue<KeyValuePair<ulong, TranslatedFunction>> _oldFuncs;

        private readonly ConcurrentDictionary<ulong, object> _backgroundSet;
        private readonly ConcurrentStack<RejitRequest> _backgroundStack;
        private readonly AutoResetEvent _backgroundTranslatorEvent;
        private readonly ReaderWriterLock _backgroundTranslatorLock;

        private JumpTable _jumpTable;
        internal JumpTable JumpTable => _jumpTable;
        internal EntryTable<uint> CountTable { get; }

        private volatile int _threadCount;

        // FIXME: Remove this once the init logic of the emulator will be redone.
        public static readonly ManualResetEvent IsReadyForTranslation = new(false);

        public Translator(IJitMemoryAllocator allocator, IMemoryManager memory)
        {
            _allocator = allocator;
            _memory = memory;

            _funcs = new ConcurrentDictionary<ulong, TranslatedFunction>();
            _oldFuncs = new ConcurrentQueue<KeyValuePair<ulong, TranslatedFunction>>();

            _backgroundSet = new ConcurrentDictionary<ulong, object>();
            _backgroundStack = new ConcurrentStack<RejitRequest>();
            _backgroundTranslatorEvent = new AutoResetEvent(false);
            _backgroundTranslatorLock = new ReaderWriterLock();

            CountTable = new EntryTable<uint>();

            JitCache.Initialize(allocator);

            DirectCallStubs.InitializeStubs();
        }

        private void TranslateStackedSubs()
        {
            while (_threadCount != 0)
            {
                _backgroundTranslatorLock.AcquireReaderLock(Timeout.Infinite);

                if (_backgroundStack.TryPop(out RejitRequest request) && 
                    _backgroundSet.TryRemove(request.Address, out _))
                {
                    TranslatedFunction func = Translate(
                        _memory,
                        _jumpTable,
                        CountTable,
                        request.Address,
                        request.Mode,
                        highCq: true);

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

             // Wake up any other background translator threads, to encourage them to exit.
            _backgroundTranslatorEvent.Set();
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
                    Debug.Assert(_funcs.Count == 0);
                    Ptc.LoadTranslations(_funcs, _memory, _jumpTable, CountTable);
                    Ptc.MakeAndSaveTranslations(_funcs, _memory, _jumpTable, CountTable);
                }

                PtcProfiler.Start();

                Ptc.Disable();

                // Simple heuristic, should be user configurable in future. (1 for 4 core/ht or less, 2 for 6 core + ht
                // etc). All threads are normal priority except from the last, which just fills as much of the last core
                // as the os lets it with a low priority. If we only have one rejit thread, it should be normal priority
                // as highCq code is performance critical.
                //
                // TODO: Use physical cores rather than logical. This only really makes sense for processors with
                // hyperthreading. Requires OS specific code.
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

                CountTable.Dispose();

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

        internal TranslatedFunction GetOrTranslate(ulong address, ExecutionMode mode)
        {
            if (!_funcs.TryGetValue(address, out TranslatedFunction func))
            {
                func = Translate(_memory, _jumpTable, CountTable, address, mode, highCq: false);

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

            return func;
        }

        internal static TranslatedFunction Translate(
            IMemoryManager memory,
            JumpTable jumpTable,
            EntryTable<uint> countTable,
            ulong address,
            ExecutionMode mode,
            bool highCq)
        {
            var context = new ArmEmitterContext(memory, jumpTable, countTable, address, highCq, Aarch32Mode.User);

            Logger.StartPass(PassName.Decoding);

            Block[] blocks = Decoder.Decode(memory, address, mode, highCq, singleBlock: false);

            Logger.EndPass(PassName.Decoding);

            PreparePool(highCq ? 1 : 0);

            Logger.StartPass(PassName.Translation);

            Counter<uint> counter = null;

            if (!context.HighCq)
            {
                EmitRejitCheck(context, out counter);
            }

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

                Hash128 hash = Ptc.ComputeHash(memory, address, funcSize);

                Ptc.WriteInfoCodeRelocUnwindInfo(address, funcSize, hash, highCq, ptcInfo);
            }

            return new TranslatedFunction(func, counter, funcSize, highCq);
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
                    InstEmitFlowHelper.EmitTailContinue(context, Const(block.Address));
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

        internal static void EmitRejitCheck(ArmEmitterContext context, out Counter<uint> counter)
        {
            const int MinsCallForRejit = 100;

            counter = new Counter<uint>(context.CountTable);

            Operand lblEnd = Label();

            Operand address = Const(ref counter.Value, Ptc.CountTableIndex);
            Operand curCount = context.Load(OperandType.I32, address);
            Operand count = context.Add(curCount, Const(1));
            context.Store(address, count);
            context.BranchIf(lblEnd, curCount, Const(MinsCallForRejit), Comparison.NotEqual, BasicBlockFrequency.Cold);

            context.Call(typeof(NativeInterface).GetMethod(nameof(NativeInterface.EnqueueForRejit)), Const(context.EntryAddress));

            context.MarkLabel(lblEnd);
        }

        internal static void EmitSynchronization(EmitterContext context)
        {
            long countOffs = NativeContext.GetCounterOffset();

            Operand lblNonZero = Label();
            Operand lblExit = Label();

            Operand countAddr = context.Add(context.LoadArgument(OperandType.I64, 0), Const(countOffs));
            Operand count = context.Load(OperandType.I32, countAddr);
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

        internal void EnqueueForRejit(ulong guestAddress, ExecutionMode mode)
        {
            if (_backgroundSet.TryAdd(guestAddress, null))
            {
                _backgroundStack.Push(new RejitRequest(guestAddress, mode));
                _backgroundTranslatorEvent.Set();
            }
        }

        private void EnqueueForDeletion(ulong guestAddress, TranslatedFunction func)
        {
            _oldFuncs.Enqueue(new(guestAddress, func));
        }

        private void ClearJitCache()
        {
            // Ensure no attempt will be made to compile new functions due to rejit.
            ClearRejitQueue(allowRequeue: false);

            foreach (var func in _funcs.Values)
            {
                JitCache.Unmap(func.FuncPtr);

                func.CallCounter?.Dispose();
            }

            _funcs.Clear();

            while (_oldFuncs.TryDequeue(out var kv))
            {
                JitCache.Unmap(kv.Value.FuncPtr);

                kv.Value.CallCounter?.Dispose();
            }
        }

        private void ClearRejitQueue(bool allowRequeue)
        {
            _backgroundTranslatorLock.AcquireWriterLock(Timeout.Infinite);

            if (allowRequeue)
            {
                while (_backgroundStack.TryPop(out var request))
                {
                    if (_funcs.TryGetValue(request.Address, out var func) && func.CallCounter != null)
                    {
                        Volatile.Write(ref func.CallCounter.Value, 0);
                    }

                    _backgroundSet.TryRemove(request.Address, out _);
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
