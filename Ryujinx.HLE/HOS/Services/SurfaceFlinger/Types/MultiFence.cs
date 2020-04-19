using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.HOS.Services.Nv.Types;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x24)]
    struct MultiFence
    {
        public int FenceCount;

        private byte _fenceStorageStart;

        private Span<byte> _storage => MemoryMarshal.CreateSpan(ref _fenceStorageStart, Unsafe.SizeOf<NvFence>() * 4);

        private Span<NvFence> _nvFences => MemoryMarshal.Cast<byte, NvFence>(_storage);

        public static MultiFence NoFence
        {
            get
            {
                MultiFence fence = new MultiFence
                {
                    FenceCount = 0
                };

                fence._nvFences[0].Id = NvFence.InvalidSyncPointId;

                return fence;
            }
        }

        public void WaitForever(GpuContext gpuContext)
        {
            Wait(gpuContext, Timeout.InfiniteTimeSpan);
        }

        public void Wait(GpuContext gpuContext, TimeSpan timeout)
        {
            for (int i = 0; i < FenceCount; i++)
            {
                _nvFences[i].Wait(gpuContext, timeout);
            }
        }
    }
}