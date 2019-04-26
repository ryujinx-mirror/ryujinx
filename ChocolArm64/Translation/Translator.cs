using ChocolArm64.Decoders;
using ChocolArm64.Events;
using ChocolArm64.IntermediateRepresentation;
using ChocolArm64.Memory;
using ChocolArm64.State;
using System;
using System.Reflection.Emit;
using System.Threading;

namespace ChocolArm64.Translation
{
    public class Translator
    {
        private MemoryManager _memory;

        private CpuThreadState _dummyThreadState;

        private TranslatorCache _cache;
        private TranslatorQueue _queue;

        private Thread _backgroundTranslator;

        public event EventHandler<CpuTraceEventArgs> CpuTrace;

        public bool EnableCpuTrace { get; set; }

        private volatile int _threadCount;

        public Translator(MemoryManager memory)
        {
            _memory = memory;

            _dummyThreadState = new CpuThreadState();

            _dummyThreadState.Running = false;

            _cache = new TranslatorCache();
            _queue = new TranslatorQueue();
        }

        internal void ExecuteSubroutine(CpuThread thread, long position)
        {
            if (Interlocked.Increment(ref _threadCount) == 1)
            {
                _backgroundTranslator = new Thread(TranslateQueuedSubs);
                _backgroundTranslator.Start();
            }

            ExecuteSubroutine(thread.ThreadState, position);

            if (Interlocked.Decrement(ref _threadCount) == 0)
            {
                _queue.ForceSignal();
            }
        }

        private void ExecuteSubroutine(CpuThreadState state, long position)
        {
            state.CurrentTranslator = this;

            do
            {
                if (EnableCpuTrace)
                {
                    CpuTrace?.Invoke(this, new CpuTraceEventArgs(position));
                }

                if (!_cache.TryGetSubroutine(position, out TranslatedSub sub))
                {
                    sub = TranslateLowCq(position, state.GetExecutionMode());
                }

                position = sub.Execute(state, _memory);
            }
            while (position != 0 && state.Running);

            state.CurrentTranslator = null;
        }

        internal ArmSubroutine GetOrTranslateSubroutine(CpuThreadState state, long position, CallType cs)
        {
            if (!_cache.TryGetSubroutine(position, out TranslatedSub sub))
            {
                sub = TranslateLowCq(position, state.GetExecutionMode());
            }

            if (sub.Rejit())
            {
                bool isComplete = cs == CallType.Call ||
                                  cs == CallType.VirtualCall;

                _queue.Enqueue(position, state.GetExecutionMode(), TranslationTier.Tier1, isComplete);
            }

            return sub.Delegate;
        }

        private void TranslateQueuedSubs()
        {
            while (_threadCount != 0)
            {
                if (_queue.TryDequeue(out TranslatorQueueItem item))
                {
                    bool isCached = _cache.TryGetSubroutine(item.Position, out TranslatedSub sub);

                    if (isCached && item.Tier <= sub.Tier)
                    {
                        continue;
                    }

                    if (item.Tier == TranslationTier.Tier0)
                    {
                        TranslateLowCq(item.Position, item.Mode);
                    }
                    else
                    {
                        TranslateHighCq(item.Position, item.Mode, item.IsComplete);
                    }
                }
                else
                {
                    _queue.WaitForItems();
                }
            }
        }

        private TranslatedSub TranslateLowCq(long position, ExecutionMode mode)
        {
            Block[] blocks = Decoder.DecodeBasicBlock(_memory, (ulong)position, mode);

            ILEmitterCtx context = new ILEmitterCtx(_memory, _cache, _queue, TranslationTier.Tier0);

            BasicBlock[] bbs = EmitAndGetBlocks(context, blocks);

            TranslatedSubBuilder builder = new TranslatedSubBuilder(mode);

            string name = GetSubroutineName(position);

            TranslatedSub subroutine = builder.Build(bbs, name, TranslationTier.Tier0);

            return _cache.GetOrAdd(position, subroutine, GetOpsCount(bbs));
        }

        private TranslatedSub TranslateHighCq(long position, ExecutionMode mode, bool isComplete)
        {
            Block[] blocks = Decoder.DecodeSubroutine(_memory, (ulong)position, mode);

            ILEmitterCtx context = new ILEmitterCtx(_memory, _cache, _queue, TranslationTier.Tier1);

            if (blocks[0].Address != (ulong)position)
            {
                context.Emit(OpCodes.Br, context.GetLabel(position));
            }

            BasicBlock[] bbs = EmitAndGetBlocks(context, blocks);

            isComplete &= !context.HasIndirectJump;

            TranslatedSubBuilder builder = new TranslatedSubBuilder(mode, isComplete);

            string name = GetSubroutineName(position);

            TranslatedSub subroutine = builder.Build(bbs, name, TranslationTier.Tier1, context.HasSlowCall);

            ForceAheadOfTimeCompilation(subroutine);

            _cache.AddOrUpdate(position, subroutine, GetOpsCount(bbs));

            return subroutine;
        }

        private static BasicBlock[] EmitAndGetBlocks(ILEmitterCtx context, Block[] blocks)
        {
            for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
            {
                Block block = blocks[blkIndex];

                context.CurrBlock = block;

                context.MarkLabel(context.GetLabel((long)block.Address));

                for (int opcIndex = 0; opcIndex < block.OpCodes.Count; opcIndex++)
                {
                    OpCode64 opCode = block.OpCodes[opcIndex];

                    context.CurrOp = opCode;

                    bool isLastOp = opcIndex == block.OpCodes.Count - 1;

                    if (isLastOp && block.Branch != null && block.Branch.Address <= block.Address)
                    {
                        context.EmitSynchronization();
                    }

                    ILLabel lblPredicateSkip = null;

                    if (opCode is OpCode32 op && op.Cond < Condition.Al)
                    {
                        lblPredicateSkip = new ILLabel();

                        context.EmitCondBranch(lblPredicateSkip, op.Cond.Invert());
                    }

                    opCode.Emitter(context);

                    if (lblPredicateSkip != null)
                    {
                        context.MarkLabel(lblPredicateSkip);

                        context.ResetBlockStateForPredicatedOp();

                        //If this is the last op on the block, and there's no "next" block
                        //after this one, then we have to return right now, with the address
                        //of the next instruction to be executed (in the case that the condition
                        //is false, and the branch was not taken, as all basic blocks should end
                        //with some kind of branch).
                        if (isLastOp && block.Next == null)
                        {
                            context.EmitStoreContext();
                            context.EmitLdc_I8(opCode.Position + opCode.OpCodeSizeInBytes);

                            context.Emit(OpCodes.Ret);
                        }
                    }
                }
            }

            return context.GetBlocks();
        }

        private static string GetSubroutineName(long position)
        {
            return $"Sub{position:x16}";
        }

        private static int GetOpsCount(BasicBlock[] blocks)
        {
            int opCount = 0;

            foreach (BasicBlock block in blocks)
            {
                opCount += block.Count;
            }

            return opCount;
        }

        private void ForceAheadOfTimeCompilation(TranslatedSub subroutine)
        {
            subroutine.Execute(_dummyThreadState, null);
        }
    }
}