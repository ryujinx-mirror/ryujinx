using Silk.NET.Vulkan;
using System;

namespace Ryujinx.Graphics.Vulkan
{
    struct DisposableBuffer : IDisposable
    {
        private readonly Vk _api;
        private readonly Device _device;

        public Silk.NET.Vulkan.Buffer Value { get; }

        public DisposableBuffer(Vk api, Device device, Silk.NET.Vulkan.Buffer buffer)
        {
            _api = api;
            _device = device;
            Value = buffer;
        }

        public void Dispose()
        {
            _api.DestroyBuffer(_device, Value, Span<AllocationCallbacks>.Empty);
        }
    }
}
