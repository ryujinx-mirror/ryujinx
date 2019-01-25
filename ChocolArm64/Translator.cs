using ChocolArm64.Decoders;
using ChocolArm64.Events;
using ChocolArm64.Memory;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;

namespace ChocolArm64
{
    public class Translator
    {
        private TranslatorCache _cache;

        public event EventHandler<CpuTraceEventArgs> CpuTrace;

        public bool EnableCpuTrace { get; set; }

        public Translator()
        {
            _cache = new TranslatorCache();
        }

        internal void ExecuteSubroutine(CpuThread thread, long position)
        {
            ExecuteSubroutine(thread.ThreadState, thread.Memory, position);
        }

        private void ExecuteSubroutine(CpuThreadState state, MemoryManager memory, long position)
        {
            do
            {
                if (EnableCpuTrace)
                {
                    CpuTrace?.Invoke(this, new CpuTraceEventArgs(position));
                }

                if (!_cache.TryGetSubroutine(position, out TranslatedSub sub))
                {
                    sub = TranslateTier0(memory, position, state.GetExecutionMode());
                }

                if (sub.ShouldReJit())
                {
                    TranslateTier1(memory, position, state.GetExecutionMode());
                }

                position = sub.Execute(state, memory);
            }
            while (position != 0 && state.Running);
        }

        internal bool HasCachedSub(long position)
        {
            return _cache.HasSubroutine(position);
        }

        private TranslatedSub TranslateTier0(MemoryManager memory, long position, ExecutionMode mode)
        {
            Block block = Decoder.DecodeBasicBlock(memory, position, mode);

            ILEmitterCtx context = new ILEmitterCtx(_cache, block);

            string subName = GetSubroutineName(position);

            ILMethodBuilder ilMthdBuilder = new ILMethodBuilder(context.GetILBlocks(), subName);

            TranslatedSub subroutine = ilMthdBuilder.GetSubroutine();

            subroutine.SetType(TranslatedSubType.SubTier0);

            _cache.AddOrUpdate(position, subroutine, block.OpCodes.Count);

            return subroutine;
        }

        private void TranslateTier1(MemoryManager memory, long position, ExecutionMode mode)
        {
            Block graph = Decoder.DecodeSubroutine(_cache, memory, position, mode);

            ILEmitterCtx context = new ILEmitterCtx(_cache, graph);

            ILBlock[] ilBlocks = context.GetILBlocks();

            string subName = GetSubroutineName(position);

            ILMethodBuilder ilMthdBuilder = new ILMethodBuilder(ilBlocks, subName);

            TranslatedSub subroutine = ilMthdBuilder.GetSubroutine();

            subroutine.SetType(TranslatedSubType.SubTier1);

            int ilOpCount = 0;

            foreach (ILBlock ilBlock in ilBlocks)
            {
                ilOpCount += ilBlock.Count;
            }

            _cache.AddOrUpdate(position, subroutine, ilOpCount);

            //Mark all methods that calls this method for ReJiting,
            //since we can now call it directly which is faster.
            if (_cache.TryGetSubroutine(position, out TranslatedSub oldSub))
            {
                foreach (long callerPos in oldSub.GetCallerPositions())
                {
                    if (_cache.TryGetSubroutine(position, out TranslatedSub callerSub))
                    {
                        callerSub.MarkForReJit();
                    }
                }
            }
        }

        private string GetSubroutineName(long position)
        {
            return $"Sub{position:x16}";
        }
    }
}