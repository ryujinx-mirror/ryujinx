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
            MemoryPropertyFlags flags = 0)
        {
            return AllocateDeviceMemory(requirements, flags, flags);
        }

        public MemoryAllocation AllocateDeviceMemory(
            MemoryRequirements requirements,
            MemoryPropertyFlags flags,
            MemoryPropertyFlags alternativeFlags)
        {
            int memoryTypeIndex = FindSuitableMemoryTypeIndex(requirements.MemoryTypeBits, flags, alternativeFlags);
            if (memoryTypeIndex < 0)
            {
                return default;
            }

            bool map = flags.HasFlag(MemoryPropertyFlags.HostVisibleBit);
            return Allocate(memoryTypeIndex, requirements.Size, requirements.Alignment, map);
        }

        private MemoryAllocation Allocate(int memoryTypeIndex, ulong size, ulong alignment, bool map)
        {
            for (int i = 0; i < _blockLists.Count; i++)
            {
                var bl = _blockLists[i];
                if (bl.MemoryTypeIndex == memoryTypeIndex)
                {
                    lock (bl)
                    {
                        return bl.Allocate(size, alignment, map);
                    }
                }
            }

            var newBl = new MemoryAllocatorBlockList(_api, _device, memoryTypeIndex, _blockAlignment);
            _blockLists.Add(newBl);
            return newBl.Allocate(size, alignment, map);
        }

        private int FindSuitableMemoryTypeIndex(
            uint memoryTypeBits,
            MemoryPropertyFlags flags,
            MemoryPropertyFlags alternativeFlags)
        {
            int bestCandidateIndex = -1;

            for (int i = 0; i < _physicalDeviceMemoryProperties.MemoryTypeCount; i++)
            {
                var type = _physicalDeviceMemoryProperties.MemoryTypes[i];

                if ((memoryTypeBits & (1 << i)) != 0)
                {
                    if (type.PropertyFlags.HasFlag(flags))
                    {
                        return i;
                    }
                    else if (type.PropertyFlags.HasFlag(alternativeFlags))
                    {
                        bestCandidateIndex = i;
                    }
                }
            }

            return bestCandidateIndex;
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
