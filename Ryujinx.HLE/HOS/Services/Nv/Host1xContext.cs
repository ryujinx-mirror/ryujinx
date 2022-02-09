using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Host1x;
using Ryujinx.Graphics.Nvdec;
using Ryujinx.Graphics.Vic;
using System;
using GpuContext = Ryujinx.Graphics.Gpu.GpuContext;

namespace Ryujinx.HLE.HOS.Services.Nv
{
    class Host1xContext : IDisposable
    {
        public MemoryManager Smmu { get; }
        public NvMemoryAllocator MemoryAllocator { get; }
        public Host1xDevice Host1x { get;}

        public Host1xContext(GpuContext gpu, ulong pid)
        {
            MemoryAllocator = new NvMemoryAllocator();
            Host1x = new Host1xDevice(gpu.Synchronization);
            Smmu = gpu.CreateMemoryManager(pid);
            var nvdec = new NvdecDevice(Smmu);
            var vic = new VicDevice(Smmu);
            Host1x.RegisterDevice(ClassId.Nvdec, nvdec);
            Host1x.RegisterDevice(ClassId.Vic, vic);
        }

        public void Dispose()
        {
            Host1x.Dispose();
        }
    }
}