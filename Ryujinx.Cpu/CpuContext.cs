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
        }

        public static ExecutionContext CreateExecutionContext() => new ExecutionContext(new JitMemoryAllocator());

        public void Execute(ExecutionContext context, ulong address) => _translator.Execute(context, address);
    }
}
