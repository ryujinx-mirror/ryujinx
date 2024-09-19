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
        /// True if the sampler is disposed, false otherwise.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// True if the sampler has sRGB conversion enabled, false otherwise.
        /// </summary>
        public bool IsSrgb { get; }

        /// <summary>
        /// Host sampler object.
        /// </summary>
        private readonly ISampler _hostSampler;

        /// <summary>
        /// Host sampler object, with anisotropy forced.
        /// </summary>
        private readonly ISampler _anisoSampler;

        /// <summary>
        /// Creates a new instance of the cached sampler.
        /// </summary>
        /// <param name="context">The GPU context the sampler belongs to</param>
        /// <param name="descriptor">The Maxwell sampler descriptor</param>
        public Sampler(GpuContext context, SamplerDescriptor descriptor)
        {
            IsSrgb = descriptor.UnpackSrgb();

            MinFilter minFilter = descriptor.UnpackMinFilter();
            MagFilter magFilter = descriptor.UnpackMagFilter();

            bool seamlessCubemap = descriptor.UnpackSeamlessCubemap();

            AddressMode addressU = descriptor.UnpackAddressU();
            AddressMode addressV = descriptor.UnpackAddressV();
            AddressMode addressP = descriptor.UnpackAddressP();

            CompareMode compareMode = descriptor.UnpackCompareMode();
            CompareOp compareOp = descriptor.UnpackCompareOp();

            ColorF color = new(
                descriptor.BorderColorR,
                descriptor.BorderColorG,
                descriptor.BorderColorB,
                descriptor.BorderColorA);

            float minLod = descriptor.UnpackMinLod();
            float maxLod = descriptor.UnpackMaxLod();
            float mipLodBias = descriptor.UnpackMipLodBias();

            float maxRequestedAnisotropy = descriptor.UnpackMaxAnisotropy();
            float maxSupportedAnisotropy = context.Capabilities.MaximumSupportedAnisotropy;

            _hostSampler = context.Renderer.CreateSampler(new SamplerCreateInfo(
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
                Math.Min(maxRequestedAnisotropy, maxSupportedAnisotropy)));

            if (GraphicsConfig.MaxAnisotropy >= 0 && GraphicsConfig.MaxAnisotropy <= 16 && (minFilter == MinFilter.LinearMipmapNearest || minFilter == MinFilter.LinearMipmapLinear))
            {
                maxRequestedAnisotropy = GraphicsConfig.MaxAnisotropy;

                _anisoSampler = context.Renderer.CreateSampler(new SamplerCreateInfo(
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
                    Math.Min(maxRequestedAnisotropy, maxSupportedAnisotropy)));
            }
        }

        /// <summary>
        /// Gets a host sampler for the given texture.
        /// </summary>
        /// <param name="texture">Texture to be sampled</param>
        /// <returns>A host sampler</returns>
        public ISampler GetHostSampler(Texture texture)
        {
            return _anisoSampler != null && texture?.CanForceAnisotropy == true ? _anisoSampler : _hostSampler;
        }

        /// <summary>
        /// Disposes the host sampler object.
        /// </summary>
        public void Dispose()
        {
            IsDisposed = true;

            _hostSampler.Dispose();
            _anisoSampler?.Dispose();
        }
    }
}
