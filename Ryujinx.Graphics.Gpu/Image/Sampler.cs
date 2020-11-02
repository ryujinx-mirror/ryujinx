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

            bool seamlessCubemap = descriptor.UnpackSeamlessCubemap();

            AddressMode addressU = descriptor.UnpackAddressU();
            AddressMode addressV = descriptor.UnpackAddressV();
            AddressMode addressP = descriptor.UnpackAddressP();

            CompareMode compareMode = descriptor.UnpackCompareMode();
            CompareOp   compareOp   = descriptor.UnpackCompareOp();

            ColorF color = new ColorF(
                descriptor.BorderColorR,
                descriptor.BorderColorG,
                descriptor.BorderColorB,
                descriptor.BorderColorA);

            float minLod     = descriptor.UnpackMinLod();
            float maxLod     = descriptor.UnpackMaxLod();
            float mipLodBias = descriptor.UnpackMipLodBias();

            float maxRequestedAnisotropy = GraphicsConfig.MaxAnisotropy >= 0 && GraphicsConfig.MaxAnisotropy <= 16 ? GraphicsConfig.MaxAnisotropy : descriptor.UnpackMaxAnisotropy();
            float maxSupportedAnisotropy = context.Capabilities.MaximumSupportedAnisotropy;

            if (maxRequestedAnisotropy > maxSupportedAnisotropy)
                maxRequestedAnisotropy = maxSupportedAnisotropy;

            HostSampler = context.Renderer.CreateSampler(new SamplerCreateInfo(
                minFilter,
                magFilter,
                seamlessCubemap,
                addressU,
                addressV,
                addressP,
                compareMode,
                compareOp,
                color,
                minLod,
                maxLod,
                mipLodBias,
                maxRequestedAnisotropy));
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