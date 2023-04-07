using Ryujinx.Memory.Tracking;
using System;
using System.Collections.Generic;

namespace Ryujinx.Cpu.Tracking
{
    public class CpuMultiRegionHandle : IMultiRegionHandle
    {
        private readonly MultiRegionHandle _impl;

        public bool Dirty => _impl.Dirty;

        internal CpuMultiRegionHandle(MultiRegionHandle impl)
        {
            _impl = impl;
        }

        public void Dispose() => _impl.Dispose();
        public void ForceDirty(ulong address, ulong size) => _impl.ForceDirty(address, size);
        public IEnumerable<IRegionHandle> GetHandles() => _impl.GetHandles();
        public void QueryModified(Action<ulong, ulong> modifiedAction) => _impl.QueryModified(modifiedAction);
        public void QueryModified(ulong address, ulong size, Action<ulong, ulong> modifiedAction) => _impl.QueryModified(address, size, modifiedAction);
        public void QueryModified(ulong address, ulong size, Action<ulong, ulong> modifiedAction, int sequenceNumber) => _impl.QueryModified(address, size, modifiedAction, sequenceNumber);
        public void RegisterAction(ulong address, ulong size, RegionSignal action) => _impl.RegisterAction(address, size, action);
        public void RegisterPreciseAction(ulong address, ulong size, PreciseRegionSignal action) => _impl.RegisterPreciseAction(address, size, action);
        public void SignalWrite() => _impl.SignalWrite();
    }
}
