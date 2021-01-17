using Ryujinx.Memory.Tracking;
using System;

namespace Ryujinx.Cpu.Tracking
{
    public class CpuSmartMultiRegionHandle : IMultiRegionHandle
    {
        private readonly SmartMultiRegionHandle _impl;

        public bool Dirty => _impl.Dirty;

        internal CpuSmartMultiRegionHandle(SmartMultiRegionHandle impl)
        {
            _impl = impl;
        }

        public void Dispose() => _impl.Dispose();
        public void RegisterAction(RegionSignal action) => _impl.RegisterAction(action);
        public void QueryModified(Action<ulong, ulong> modifiedAction) => _impl.QueryModified(modifiedAction);
        public void QueryModified(ulong address, ulong size, Action<ulong, ulong> modifiedAction) => _impl.QueryModified(address, size, modifiedAction);
        public void QueryModified(ulong address, ulong size, Action<ulong, ulong> modifiedAction, int sequenceNumber) => _impl.QueryModified(address, size, modifiedAction, sequenceNumber);
        public void SignalWrite() => _impl.SignalWrite();
    }
}
