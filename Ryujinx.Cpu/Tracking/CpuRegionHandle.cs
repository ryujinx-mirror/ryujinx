using Ryujinx.Memory.Tracking;
using System;

namespace Ryujinx.Cpu.Tracking
{
    public class CpuRegionHandle : IRegionHandle
    {
        private readonly RegionHandle _impl;

        public bool Dirty => _impl.Dirty;
        public bool Unmapped => _impl.Unmapped;
        public ulong Address => _impl.Address;
        public ulong Size => _impl.Size;
        public ulong EndAddress => _impl.EndAddress;

        internal CpuRegionHandle(RegionHandle impl)
        {
            _impl = impl;
        }

        public void Dispose() => _impl.Dispose();
        public void RegisterAction(RegionSignal action) => _impl.RegisterAction(action);
        public void RegisterDirtyEvent(Action action) => _impl.RegisterDirtyEvent(action);
        public void Reprotect(bool asDirty = false) => _impl.Reprotect(asDirty);

        public bool OverlapsWith(ulong address, ulong size) => _impl.OverlapsWith(address, size);
    }
}
