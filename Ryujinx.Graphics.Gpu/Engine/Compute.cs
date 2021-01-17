using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Shader;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using System;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        /// <summary>
        /// Dispatches compute work.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        public void Dispatch(GpuState state, int argument)
        {
            uint qmdAddress = (uint)state.Get<int>(MethodOffset.DispatchParamsAddress);

            var qmd = _context.MemoryManager.Read<ComputeQmd>((ulong)qmdAddress << 8);

            GpuVa shaderBaseAddress = state.Get<GpuVa>(MethodOffset.ShaderBaseAddress);

            ulong shaderGpuVa = shaderBaseAddress.Pack() + (uint)qmd.ProgramOffset;

            int localMemorySize = qmd.ShaderLocalMemoryLowSize + qmd.ShaderLocalMemoryHighSize;

            int sharedMemorySize = Math.Min(qmd.SharedMemorySize, _context.Capabilities.MaximumComputeSharedMemorySize);

            for (int index = 0; index < Constants.TotalCpUniformBuffers; index++)
            {
                if (!qmd.ConstantBufferValid(index))
                {
                    continue;
                }

                ulong gpuVa = (uint)qmd.ConstantBufferAddrLower(index) | (ulong)qmd.ConstantBufferAddrUpper(index) << 32;
                ulong size = (ulong)qmd.ConstantBufferSize(index);

                BufferManager.SetComputeUniformBuffer(index, gpuVa, size);
            }

            ShaderBundle cs = ShaderCache.GetComputeShader(
                state,
                shaderGpuVa,
                qmd.CtaThreadDimension0,
                qmd.CtaThreadDimension1,
                qmd.CtaThreadDimension2,
                localMemorySize,
                sharedMemorySize);

            _context.Renderer.Pipeline.SetProgram(cs.HostProgram);

            var samplerPool = state.Get<PoolState>(MethodOffset.SamplerPoolState);
            var texturePool = state.Get<PoolState>(MethodOffset.TexturePoolState);

            TextureManager.SetComputeSamplerPool(samplerPool.Address.Pack(), samplerPool.MaximumId, qmd.SamplerIndex);
            TextureManager.SetComputeTexturePool(texturePool.Address.Pack(), texturePool.MaximumId);
            TextureManager.SetComputeTextureBufferIndex(state.Get<int>(MethodOffset.TextureBufferIndex));

            ShaderProgramInfo info = cs.Shaders[0].Info;

            for (int index = 0; index < info.CBuffers.Count; index++)
            {
                BufferDescriptor cb = info.CBuffers[index];

                // NVN uses the "hardware" constant buffer for anything that is less than 8,
                // and those are already bound above.
                // Anything greater than or equal to 8 uses the emulated constant buffers.
                // They are emulated using global memory loads.
                if (cb.Slot < 8)
                {
                    continue;
                }

                ulong cbDescAddress = BufferManager.GetComputeUniformBufferAddress(0);

                int cbDescOffset = 0x260 + (cb.Slot - 8) * 0x10;

                cbDescAddress += (ulong)cbDescOffset;

                SbDescriptor cbDescriptor = _context.PhysicalMemory.Read<SbDescriptor>(cbDescAddress);

                BufferManager.SetComputeUniformBuffer(cb.Slot, cbDescriptor.PackAddress(), (uint)cbDescriptor.Size);
            }

            for (int index = 0; index < info.SBuffers.Count; index++)
            {
                BufferDescriptor sb = info.SBuffers[index];

                ulong sbDescAddress = BufferManager.GetComputeUniformBufferAddress(0);

                int sbDescOffset = 0x310 + sb.Slot * 0x10;

                sbDescAddress += (ulong)sbDescOffset;

                SbDescriptor sbDescriptor = _context.PhysicalMemory.Read<SbDescriptor>(sbDescAddress);

                BufferManager.SetComputeStorageBuffer(sb.Slot, sbDescriptor.PackAddress(), (uint)sbDescriptor.Size, sb.Flags);
            }

            BufferManager.SetComputeStorageBufferBindings(info.SBuffers);
            BufferManager.SetComputeUniformBufferBindings(info.CBuffers);

            var textureBindings = new TextureBindingInfo[info.Textures.Count];

            for (int index = 0; index < info.Textures.Count; index++)
            {
                var descriptor = info.Textures[index];

                Target target = ShaderTexture.GetTarget(descriptor.Type);

                textureBindings[index] = new TextureBindingInfo(
                    target,
                    descriptor.Binding,
                    descriptor.CbufSlot,
                    descriptor.HandleIndex,
                    descriptor.Flags);
            }

            TextureManager.SetComputeTextures(textureBindings);

            var imageBindings = new TextureBindingInfo[info.Images.Count];

            for (int index = 0; index < info.Images.Count; index++)
            {
                var descriptor = info.Images[index];

                Target target = ShaderTexture.GetTarget(descriptor.Type);
                Format format = ShaderTexture.GetFormat(descriptor.Format);

                imageBindings[index] = new TextureBindingInfo(
                    target,
                    format,
                    descriptor.Binding,
                    descriptor.CbufSlot,
                    descriptor.HandleIndex,
                    descriptor.Flags);
            }

            TextureManager.SetComputeImages(imageBindings);

            BufferManager.CommitComputeBindings();
            TextureManager.CommitComputeBindings();

            _context.Renderer.Pipeline.DispatchCompute(
                qmd.CtaRasterWidth,
                qmd.CtaRasterHeight,
                qmd.CtaRasterDepth);

            _forceShaderUpdate = true;
        }
    }
}