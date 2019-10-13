using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Color;
using Ryujinx.Graphics.GAL.Sampler;
using System;

namespace Ryujinx.Graphics.Gpu.Image
{
    class Sampler : IDisposable
    {
        public ISampler HostSampler { get; }

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

        public void Dispose()
        {
            HostSampler.Dispose();
        }
    }
}