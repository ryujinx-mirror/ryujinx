using Ryujinx.Graphics.GAL;
using Silk.NET.Vulkan;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Vulkan
{
    class PipelineLayoutCacheEntry
    {
        private readonly VulkanRenderer _gd;
        private readonly Device _device;

        public DescriptorSetLayout[] DescriptorSetLayouts { get; }
        public PipelineLayout PipelineLayout { get; }

        private readonly List<Auto<DescriptorSetCollection>>[][] _dsCache;
        private readonly int[] _dsCacheCursor;
        private int _dsLastCbIndex;

        private PipelineLayoutCacheEntry(VulkanRenderer gd, Device device)
        {
            _gd = gd;
            _device = device;

            _dsCache = new List<Auto<DescriptorSetCollection>>[CommandBufferPool.MaxCommandBuffers][];

            for (int i = 0; i < CommandBufferPool.MaxCommandBuffers; i++)
            {
                _dsCache[i] = new List<Auto<DescriptorSetCollection>>[PipelineBase.DescriptorSetLayouts];

                for (int j = 0; j < PipelineBase.DescriptorSetLayouts; j++)
                {
                    _dsCache[i][j] = new List<Auto<DescriptorSetCollection>>();
                }
            }

            _dsCacheCursor = new int[PipelineBase.DescriptorSetLayouts];
        }

        public PipelineLayoutCacheEntry(VulkanRenderer gd, Device device, uint stages, bool usePd) : this(gd, device)
        {
            DescriptorSetLayouts = PipelineLayoutFactory.Create(gd, device, stages, usePd, out var pipelineLayout);
            PipelineLayout = pipelineLayout;
        }

        public PipelineLayoutCacheEntry(VulkanRenderer gd, Device device, ShaderSource[] shaders) : this(gd, device)
        {
            DescriptorSetLayouts = PipelineLayoutFactory.CreateMinimal(gd, device, shaders, out var pipelineLayout);
            PipelineLayout = pipelineLayout;
        }

        public Auto<DescriptorSetCollection> GetNewDescriptorSetCollection(
            VulkanRenderer gd,
            int commandBufferIndex,
            int setIndex,
            out bool isNew)
        {
            if (_dsLastCbIndex != commandBufferIndex)
            {
                _dsLastCbIndex = commandBufferIndex;

                for (int i = 0; i < PipelineBase.DescriptorSetLayouts; i++)
                {
                    _dsCacheCursor[i] = 0;
                }
            }

            var list = _dsCache[commandBufferIndex][setIndex];
            int index = _dsCacheCursor[setIndex]++;
            if (index == list.Count)
            {
                var dsc = gd.DescriptorSetManager.AllocateDescriptorSet(gd.Api, DescriptorSetLayouts[setIndex]);
                list.Add(dsc);
                isNew = true;
                return dsc;
            }

            isNew = false;
            return list[index];
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = 0; i < _dsCache.Length; i++)
                {
                    for (int j = 0; j < _dsCache[i].Length; j++)
                    {
                        for (int k = 0; k < _dsCache[i][j].Count; k++)
                        {
                            _dsCache[i][j][k].Dispose();
                        }

                        _dsCache[i][j].Clear();
                    }
                }

                _gd.Api.DestroyPipelineLayout(_device, PipelineLayout, null);

                for (int i = 0; i < DescriptorSetLayouts.Length; i++)
                {
                    _gd.Api.DestroyDescriptorSetLayout(_device, DescriptorSetLayouts[i], null);
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
