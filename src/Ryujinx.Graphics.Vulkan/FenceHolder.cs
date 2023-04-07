using Silk.NET.Vulkan;
using System;
using System.Threading;

namespace Ryujinx.Graphics.Vulkan
{
    class FenceHolder : IDisposable
    {
        private readonly Vk _api;
        private readonly Device _device;
        private Fence _fence;
        private int _referenceCount;
        private bool _disposed;

        public unsafe FenceHolder(Vk api, Device device)
        {
            _api = api;
            _device = device;

            var fenceCreateInfo = new FenceCreateInfo()
            {
                SType = StructureType.FenceCreateInfo
            };

            api.CreateFence(device, in fenceCreateInfo, null, out _fence).ThrowOnError();

            _referenceCount = 1;
        }

        public Fence GetUnsafe()
        {
            return _fence;
        }

        public Fence Get()
        {
            Interlocked.Increment(ref _referenceCount);
            return _fence;
        }

        public void Put()
        {
            if (Interlocked.Decrement(ref _referenceCount) == 0)
            {
                _api.DestroyFence(_device, _fence, Span<AllocationCallbacks>.Empty);
                _fence = default;
            }
        }

        public void Wait()
        {
            Span<Fence> fences = stackalloc Fence[]
            {
                _fence
            };

            FenceHelper.WaitAllIndefinitely(_api, _device, fences);
        }

        public bool IsSignaled()
        {
            Span<Fence> fences = stackalloc Fence[]
            {
                _fence
            };

            return FenceHelper.AllSignaled(_api, _device, fences);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Put();
                _disposed = true;
            }
        }
    }
}
