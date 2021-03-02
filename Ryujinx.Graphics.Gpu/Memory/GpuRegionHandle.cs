using Ryujinx.Cpu.Tracking;
using Ryujinx.Memory.Tracking;
using System;

namespace Ryujinx.Graphics.Gpu.Memory
{
    class GpuRegionHandle : IRegionHandle
    {
        private readonly CpuRegionHandle[] _cpuRegionHandles;

        public bool Dirty
        {
            get
            {
                foreach (var regionHandle in _cpuRegionHandles)
                {
                    if (regionHandle.Dirty)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public ulong Address => throw new NotSupportedException();
        public ulong Size => throw new NotSupportedException();
        public ulong EndAddress => throw new NotSupportedException();

        public GpuRegionHandle(CpuRegionHandle[] cpuRegionHandles)
        {
            _cpuRegionHandles = cpuRegionHandles;
        }

        public void Dispose()
        {
            foreach (var regionHandle in _cpuRegionHandles)
            {
                regionHandle.Dispose();
            }
        }

        public void RegisterAction(RegionSignal action)
        {
            foreach (var regionHandle in _cpuRegionHandles)
            {
                regionHandle.RegisterAction(action);
            }
        }

        public void Reprotect(bool asDirty = false)
        {
            foreach (var regionHandle in _cpuRegionHandles)
            {
                regionHandle.Reprotect(asDirty);
            }
        }
    }
}
