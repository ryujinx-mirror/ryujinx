using Ryujinx.Memory.Tracking;

namespace Ryujinx.Cpu.Tracking
{
    public class CpuRegionHandle : IRegionHandle
    {
        private readonly RegionHandle _impl;

        public bool Dirty => _impl.Dirty;
        public ulong Address => _impl.Address;
        public ulong Size => _impl.Size;
        public ulong EndAddress => _impl.EndAddress;

        internal CpuRegionHandle(RegionHandle impl)
        {
            _impl = impl;
        }

        public void Dispose() => _impl.Dispose();
        public void RegisterAction(RegionSignal action) => _impl.RegisterAction(action);
        public void Reprotect() => _impl.Reprotect();
    }
}
