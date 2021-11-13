using Ryujinx.Graphics.GAL;
using System;
using System.Numerics;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Cached sampler entry for sampler pools.
    /// </summary>
    class Sampler : IDisposable
    {
        private const int MinLevelsForAnisotropic = 5;

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
            return _anisoSampler != null && AllowForceAnisotropy(texture) ? _anisoSampler : _hostSampler;
        }

        /// <summary>
        /// Determine if the given texture can have anisotropic filtering forced.
        /// Filtered textures that we might want to force anisotropy on should have a lot of mip levels.
        /// </summary>
        /// <param name="texture">The texture</param>
        /// <returns>True if anisotropic filtering can be forced, false otherwise</returns>
        private static bool AllowForceAnisotropy(Texture texture)
        {
            if (texture == null || !(texture.Target == Target.Texture2D || texture.Target == Target.Texture2DArray))
            {
                return false;
            }

            int maxSize = Math.Max(texture.Info.Width, texture.Info.Height);
            int maxLevels = BitOperations.Log2((uint)maxSize) + 1;

            return texture.Info.Levels >= Math.Min(MinLevelsForAnisotropic, maxLevels);
        }

        /// <summary>
        /// Disposes the host sampler object.
        /// </summary>
        public void Dispose()
        {
            _hostSampler.Dispose();
            _anisoSampler?.Dispose();
        }
    }
}