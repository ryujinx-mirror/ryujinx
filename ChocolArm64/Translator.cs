using ChocolArm64.Decoders;
using ChocolArm64.Events;
using ChocolArm64.Memory;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

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
            //TODO: Both the execute A32/A64 methods should be merged on the future,
            //when both ISAs are implemented with the interpreter and JIT.
            //As of now, A32 only has a interpreter and A64 a JIT.
            CpuThreadState state  = thread.ThreadState;
            MemoryManager  memory = thread.Memory;

            if (state.ExecutionMode == ExecutionMode.AArch32)
            {
                ExecuteSubroutineA32(state, memory);
            }
            else
            {
                ExecuteSubroutineA64(state, memory, position);
            }
        }

        private void ExecuteSubroutineA32(CpuThreadState state, MemoryManager memory)
        {
            do
            {
                OpCode64 opCode = Decoder.DecodeOpCode(state, memory, state.R15);

                opCode.Interpreter(state, memory, opCode);
            }
            while (state.R15 != 0 && state.Running);
        }

        private void ExecuteSubroutineA64(CpuThreadState state, MemoryManager memory, long position)
        {
            do
            {
                if (EnableCpuTrace)
                {
                    CpuTrace?.Invoke(this, new CpuTraceEventArgs(position));
                }

                if (!_cache.TryGetSubroutine(position, out TranslatedSub sub))
                {
                    sub = TranslateTier0(state, memory, position);
                }

                if (sub.ShouldReJit())
                {
                    TranslateTier1(state, memory, position);
                }

                position = sub.Execute(state, memory);
            }
            while (position != 0 && state.Running);
        }

        internal bool HasCachedSub(long position)
        {
            return _cache.HasSubroutine(position);
        }

        private TranslatedSub TranslateTier0(CpuThreadState state, MemoryManager memory, long position)
        {
            Block block = Decoder.DecodeBasicBlock(state, memory, position);

            ILEmitterCtx context = new ILEmitterCtx(_cache, block);

            string subName = GetSubroutineName(position);

            ILMethodBuilder ilMthdBuilder = new ILMethodBuilder(context.GetILBlocks(), subName);

            TranslatedSub subroutine = ilMthdBuilder.GetSubroutine();

            subroutine.SetType(TranslatedSubType.SubTier0);

            _cache.AddOrUpdate(position, subroutine, block.OpCodes.Count);

            return subroutine;
        }

        private void TranslateTier1(CpuThreadState state, MemoryManager memory, long position)
        {
            Block graph = Decoder.DecodeSubroutine(_cache, state, memory, position);

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