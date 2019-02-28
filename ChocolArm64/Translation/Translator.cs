using ChocolArm64.Decoders;
using ChocolArm64.Events;
using ChocolArm64.Memory;
using ChocolArm64.State;
using System;
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

            if (sub.IsWorthOptimizing())
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
            Block block = Decoder.DecodeBasicBlock(_memory, position, mode);

            ILEmitterCtx context = new ILEmitterCtx(_memory, _cache, _queue, TranslationTier.Tier0, block);

            string subName = GetSubroutineName(position);

            bool isAarch64 = mode == ExecutionMode.Aarch64;

            ILMethodBuilder ilMthdBuilder = new ILMethodBuilder(context.GetILBlocks(), subName, isAarch64);

            TranslatedSub subroutine = ilMthdBuilder.GetSubroutine(TranslationTier.Tier0, isWorthOptimizing: true);

            return _cache.GetOrAdd(position, subroutine, block.OpCodes.Count);
        }

        private TranslatedSub TranslateHighCq(long position, ExecutionMode mode, bool isComplete)
        {
            Block graph = Decoder.DecodeSubroutine(_memory, position, mode);

            ILEmitterCtx context = new ILEmitterCtx(_memory, _cache, _queue, TranslationTier.Tier1, graph);

            ILBlock[] ilBlocks = context.GetILBlocks();

            string subName = GetSubroutineName(position);

            bool isAarch64 = mode == ExecutionMode.Aarch64;

            isComplete &= !context.HasIndirectJump;

            ILMethodBuilder ilMthdBuilder = new ILMethodBuilder(ilBlocks, subName, isAarch64, isComplete);

            TranslatedSub subroutine = ilMthdBuilder.GetSubroutine(TranslationTier.Tier1, context.HasSlowCall);

            int ilOpCount = 0;

            foreach (ILBlock ilBlock in ilBlocks)
            {
                ilOpCount += ilBlock.Count;
            }

            ForceAheadOfTimeCompilation(subroutine);

            _cache.AddOrUpdate(position, subroutine, ilOpCount);

            return subroutine;
        }

        private string GetSubroutineName(long position)
        {
            return $"Sub{position:x16}";
        }

        private void ForceAheadOfTimeCompilation(TranslatedSub subroutine)
        {
            subroutine.Execute(_dummyThreadState, null);
        }
    }
}