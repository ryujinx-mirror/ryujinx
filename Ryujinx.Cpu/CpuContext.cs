using ARMeilleure.State;
using ARMeilleure.Translation;

namespace Ryujinx.Cpu
{
    public class CpuContext
    {
        private readonly Translator _translator;

        public CpuContext(MemoryManager memory)
        {
            _translator = new Translator(new JitMemoryAllocator(), memory);
            memory.UnmapEvent += UnmapHandler;
        }

        private void UnmapHandler(ulong address, ulong size)
        {
            _translator.InvalidateJitCacheRegion(address, size);
        }

        public static ExecutionContext CreateExecutionContext()
        {
            return new ExecutionContext(new JitMemoryAllocator());
        }

        public void Execute(ExecutionContext context, ulong address)
        {
            _translator.Execute(context, address);
        }
    }
}
