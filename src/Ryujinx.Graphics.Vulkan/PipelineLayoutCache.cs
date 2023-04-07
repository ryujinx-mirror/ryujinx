using Ryujinx.Graphics.GAL;
using Silk.NET.Vulkan;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Vulkan
{
    class PipelineLayoutCache
    {
        private readonly PipelineLayoutCacheEntry[] _plce;
        private readonly List<PipelineLayoutCacheEntry> _plceMinimal;

        public PipelineLayoutCache()
        {
            _plce = new PipelineLayoutCacheEntry[1 << Constants.MaxShaderStages];
            _plceMinimal = new List<PipelineLayoutCacheEntry>();
        }

        public PipelineLayoutCacheEntry Create(VulkanRenderer gd, Device device, ShaderSource[] shaders)
        {
            var plce = new PipelineLayoutCacheEntry(gd, device, shaders);
            _plceMinimal.Add(plce);
            return plce;
        }

        public PipelineLayoutCacheEntry GetOrCreate(VulkanRenderer gd, Device device, uint stages, bool usePd)
        {
            if (_plce[stages] == null)
            {
                _plce[stages] = new PipelineLayoutCacheEntry(gd, device, stages, usePd);
            }

            return _plce[stages];
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = 0; i < _plce.Length; i++)
                {
                    _plce[i]?.Dispose();
                }

                foreach (var plce in _plceMinimal)
                {
                    plce.Dispose();
                }

                _plceMinimal.Clear();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
