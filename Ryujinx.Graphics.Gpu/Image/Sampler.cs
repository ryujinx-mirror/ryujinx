using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Cached sampler entry for sampler pools.
    /// </summary>
    class Sampler : IDisposable
    {
        /// <summary>
        /// Host sampler object.
        /// </summary>
        public ISampler HostSampler { get; }

        /// <summary>
        /// Creates a new instance of the cached sampler.
        /// </summary>
        /// <param name="context">The GPU context the sampler belongs to</param>
        /// <param name="descriptor">The Maxwell sampler descriptor</param>
        public Sampler(GpuContext context, SamplerDescriptor descriptor)
        {
            MinFilter minFilter = descriptor.UnpackMinFilter();
            MagFilter magFilter = descriptor.UnpackMagFilter();

            AddressMode addressU = descriptor.UnpackAddressU();
            AddressMode addressV = descriptor.UnpackAddressV();
            AddressMode addressP = descriptor.UnpackAddressP();

            CompareMode compareMode = descriptor.UnpackCompareMode();
            CompareOp   compareOp   = descriptor.UnpackCompareOp();

            ColorF color = new ColorF(0, 0, 0, 0);

            float minLod     = descriptor.UnpackMinLod();
            float maxLod     = descriptor.UnpackMaxLod();
            float mipLodBias = descriptor.UnpackMipLodBias();

            float maxAnisotropy = descriptor.UnpackMaxAnisotropy();

            HostSampler = context.Renderer.CreateSampler(new SamplerCreateInfo(
                minFilter,
                magFilter,
                addressU,
                addressV,
                addressP,
                compareMode,
                compareOp,
                color,
                minLod,
                maxLod,
                mipLodBias,
                maxAnisotropy));
        }

        /// <summary>
        /// Disposes the host sampler object.
        /// </summary>
        public void Dispose()
        {
            HostSampler.Dispose();
        }
    }
}