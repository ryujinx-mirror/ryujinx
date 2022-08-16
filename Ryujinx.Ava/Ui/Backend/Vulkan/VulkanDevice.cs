using System;
using Silk.NET.Vulkan;

namespace Ryujinx.Ava.Ui.Vulkan
{
    internal class VulkanDevice : IDisposable
    {
        private static object _lock = new object();

        public VulkanDevice(Device apiHandle, VulkanPhysicalDevice physicalDevice, Vk api)
        {
            InternalHandle = apiHandle;
            Api = api;

            api.GetDeviceQueue(apiHandle, physicalDevice.QueueFamilyIndex, 0, out var queue);

            Queue = new VulkanQueue(this, queue);

            PresentQueue = Queue;
        }

        public IntPtr Handle => InternalHandle.Handle;

        internal Device InternalHandle { get; }
        public Vk Api { get; }

        public VulkanQueue Queue { get; private set; }
        public VulkanQueue PresentQueue { get; }

        public void Dispose()
        {
            WaitIdle();
            Queue = null;
            Api.DestroyDevice(InternalHandle, Span<AllocationCallbacks>.Empty);
        }

        internal void Submit(SubmitInfo submitInfo, Fence fence = default)
        {
            lock (_lock)
            {
                Api.QueueSubmit(Queue.InternalHandle, 1, submitInfo, fence).ThrowOnError();
            }
        }

        public void WaitIdle()
        {
            lock (_lock)
            {
                Api.DeviceWaitIdle(InternalHandle);
            }
        }

        public void QueueWaitIdle()
        {
            lock (_lock)
            {
                Api.QueueWaitIdle(Queue.InternalHandle);
            }
        }

        public object Lock => _lock;
    }
}
