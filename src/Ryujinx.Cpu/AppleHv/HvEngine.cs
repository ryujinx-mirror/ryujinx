using ARMeilleure.Memory;

namespace Ryujinx.Cpu.AppleHv
{
    public class HvEngine : ICpuEngine
    {
        private readonly ITickSource _tickSource;

        public HvEngine(ITickSource tickSource)
        {
            _tickSource = tickSource;
        }

        /// <inheritdoc/>
        public ICpuContext CreateCpuContext(IMemoryManager memoryManager, bool for64Bit)
        {
            return new HvCpuContext(_tickSource, memoryManager, for64Bit);
        }
    }
}