using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Vulkan
{
    class MemoryAllocator : IDisposable
    {
        private ulong MaxDeviceMemoryUsageEstimate = 16UL * 1024 * 1024 * 1024;

        private readonly Vk _api;
        private readonly PhysicalDevice _physicalDevice;
        private readonly Device _device;
        private readonly List<MemoryAllocatorBlockList> _blockLists;
        private readonly int _blockAlignment;
        private readonly PhysicalDeviceMemoryProperties _physicalDeviceMemoryProperties;

        public MemoryAllocator(Vk api, PhysicalDevice physicalDevice, Device device, uint maxMemoryAllocationCount)
        {
            _api = api;
            _physicalDevice = physicalDevice;
            _device = device;
            _blockLists = new List<MemoryAllocatorBlockList>();
            _blockAlignment = (int)Math.Min(int.MaxValue, MaxDeviceMemoryUsageEstimate / (ulong)maxMemoryAllocationCount);

            _api.GetPhysicalDeviceMemoryProperties(_physicalDevice, out _physicalDeviceMemoryProperties);
        }

        public MemoryAllocation AllocateDeviceMemory(
            MemoryRequirements requirements,
            MemoryPropertyFlags flags = 0,
            bool isBuffer = false)
        {
            int memoryTypeIndex = FindSuitableMemoryTypeIndex(requirements.MemoryTypeBits, flags);
            if (memoryTypeIndex < 0)
            {
                return default;
            }

            bool map = flags.HasFlag(MemoryPropertyFlags.HostVisibleBit);
            return Allocate(memoryTypeIndex, requirements.Size, requirements.Alignment, map, isBuffer);
        }

        private MemoryAllocation Allocate(int memoryTypeIndex, ulong size, ulong alignment, bool map, bool isBuffer)
        {
            for (int i = 0; i < _blockLists.Count; i++)
            {
                var bl = _blockLists[i];
                if (bl.MemoryTypeIndex == memoryTypeIndex && bl.ForBuffer == isBuffer)
                {
                    lock (bl)
                    {
                        return bl.Allocate(size, alignment, map);
                    }
                }
            }

            var newBl = new MemoryAllocatorBlockList(_api, _device, memoryTypeIndex, _blockAlignment, isBuffer);
            _blockLists.Add(newBl);
            return newBl.Allocate(size, alignment, map);
        }

        private int FindSuitableMemoryTypeIndex(
            uint memoryTypeBits,
            MemoryPropertyFlags flags)
        {
            for (int i = 0; i < _physicalDeviceMemoryProperties.MemoryTypeCount; i++)
            {
                var type = _physicalDeviceMemoryProperties.MemoryTypes[i];

                if ((memoryTypeBits & (1 << i)) != 0)
                {
                    if (type.PropertyFlags.HasFlag(flags))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public static bool IsDeviceMemoryShared(Vk api, PhysicalDevice physicalDevice)
        {
            // The device is regarded as having shared memory if all heaps have the device local bit.

            api.GetPhysicalDeviceMemoryProperties(physicalDevice, out var properties);

            for (int i = 0; i < properties.MemoryHeapCount; i++)
            {
                if (!properties.MemoryHeaps[i].Flags.HasFlag(MemoryHeapFlags.DeviceLocalBit))
                {
                    return false;
                }
            }

            return true;
        }

        public void Dispose()
        {
            for (int i = 0; i < _blockLists.Count; i++)
            {
                _blockLists[i].Dispose();
            }
        }
    }
}
