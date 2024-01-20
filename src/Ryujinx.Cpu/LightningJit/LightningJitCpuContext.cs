using ARMeilleure.Memory;
using Ryujinx.Cpu.Jit;
using Ryujinx.Cpu.LightningJit.State;

namespace Ryujinx.Cpu.LightningJit
{
    class LightningJitCpuContext : ICpuContext
    {
        private readonly ITickSource _tickSource;
        private readonly Translator _translator;

        public LightningJitCpuContext(ITickSource tickSource, IMemoryManager memory, bool for64Bit)
        {
            _tickSource = tickSource;
            _translator = new Translator(memory, for64Bit);
            memory.UnmapEvent += UnmapHandler;
        }

        private void UnmapHandler(ulong address, ulong size)
        {
            _translator.InvalidateJitCacheRegion(address, size);
        }

        /// <inheritdoc/>
        public IExecutionContext CreateExecutionContext(ExceptionCallbacks exceptionCallbacks)
        {
            return new ExecutionContext(new JitMemoryAllocator(), _tickSource, exceptionCallbacks);
        }

        /// <inheritdoc/>
        public void Execute(IExecutionContext context, ulong address)
        {
            _translator.Execute((ExecutionContext)context, address);
        }

        /// <inheritdoc/>
        public void InvalidateCacheRegion(ulong address, ulong size)
        {
            _translator.InvalidateJitCacheRegion(address, size);
        }

        /// <inheritdoc/>
        public IDiskCacheLoadState LoadDiskCache(string titleIdText, string displayVersion, bool enabled)
        {
            return new DummyDiskCacheLoadState();
        }

        /// <inheritdoc/>
        public void PrepareCodeRange(ulong address, ulong size)
        {
        }

        public void Dispose()
        {
            _translator.Dispose();
        }
    }
}
