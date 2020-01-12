using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Shader;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using System;
using System.Runtime.InteropServices;

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
            uint dispatchParamsAddress = (uint)state.Get<int>(MethodOffset.DispatchParamsAddress);

            var dispatchParams = _context.MemoryAccessor.Read<ComputeParams>((ulong)dispatchParamsAddress << 8);

            GpuVa shaderBaseAddress = state.Get<GpuVa>(MethodOffset.ShaderBaseAddress);

            ulong shaderGpuVa = shaderBaseAddress.Pack() + (uint)dispatchParams.ShaderOffset;

            // Note: A size of 0 is also invalid, the size must be at least 1.
            int sharedMemorySize = Math.Clamp(dispatchParams.SharedMemorySize & 0xffff, 1, _context.Capabilities.MaximumComputeSharedMemorySize);

            ComputeShader cs = ShaderCache.GetComputeShader(
                shaderGpuVa,
                sharedMemorySize,
                dispatchParams.UnpackBlockSizeX(),
                dispatchParams.UnpackBlockSizeY(),
                dispatchParams.UnpackBlockSizeZ());

            _context.Renderer.Pipeline.SetProgram(cs.HostProgram);

            var samplerPool = state.Get<PoolState>(MethodOffset.SamplerPoolState);

            TextureManager.SetComputeSamplerPool(samplerPool.Address.Pack(), samplerPool.MaximumId, dispatchParams.SamplerIndex);

            var texturePool = state.Get<PoolState>(MethodOffset.TexturePoolState);

            TextureManager.SetComputeTexturePool(texturePool.Address.Pack(), texturePool.MaximumId);

            TextureManager.SetComputeTextureBufferIndex(state.Get<int>(MethodOffset.TextureBufferIndex));

            ShaderProgramInfo info = cs.Shader.Program.Info;

            uint sbEnableMask = 0;
            uint ubEnableMask = dispatchParams.UnpackUniformBuffersEnableMask();

            for (int index = 0; index < dispatchParams.UniformBuffers.Length; index++)
            {
                if ((ubEnableMask & (1 << index)) == 0)
                {
                    continue;
                }

                ulong gpuVa = dispatchParams.UniformBuffers[index].PackAddress();
                ulong size  = dispatchParams.UniformBuffers[index].UnpackSize();

                BufferManager.SetComputeUniformBuffer(index, gpuVa, size);
            }

            for (int index = 0; index < info.SBuffers.Count; index++)
            {
                BufferDescriptor sb = info.SBuffers[index];

                sbEnableMask |= 1u << sb.Slot;

                ulong sbDescAddress = BufferManager.GetComputeUniformBufferAddress(0);

                int sbDescOffset = 0x310 + sb.Slot * 0x10;

                sbDescAddress += (ulong)sbDescOffset;

                ReadOnlySpan<byte> sbDescriptorData = _context.PhysicalMemory.GetSpan(sbDescAddress, 0x10);

                SbDescriptor sbDescriptor = MemoryMarshal.Cast<byte, SbDescriptor>(sbDescriptorData)[0];

                BufferManager.SetComputeStorageBuffer(sb.Slot, sbDescriptor.PackAddress(), (uint)sbDescriptor.Size);
            }

            ubEnableMask = 0;

            for (int index = 0; index < info.CBuffers.Count; index++)
            {
                ubEnableMask |= 1u << info.CBuffers[index].Slot;
            }

            BufferManager.SetComputeStorageBufferEnableMask(sbEnableMask);
            BufferManager.SetComputeUniformBufferEnableMask(ubEnableMask);

            var textureBindings = new TextureBindingInfo[info.Textures.Count];

            for (int index = 0; index < info.Textures.Count; index++)
            {
                var descriptor = info.Textures[index];

                Target target = GetTarget(descriptor.Type);

                if (descriptor.IsBindless)
                {
                    textureBindings[index] = new TextureBindingInfo(target, descriptor.CbufOffset, descriptor.CbufSlot);
                }
                else
                {
                    textureBindings[index] = new TextureBindingInfo(target, descriptor.HandleIndex);
                }
            }

            TextureManager.SetComputeTextures(textureBindings);

            var imageBindings = new TextureBindingInfo[info.Images.Count];

            for (int index = 0; index < info.Images.Count; index++)
            {
                var descriptor = info.Images[index];

                Target target = GetTarget(descriptor.Type);

                imageBindings[index] = new TextureBindingInfo(target, descriptor.HandleIndex);
            }

            TextureManager.SetComputeImages(imageBindings);

            BufferManager.CommitComputeBindings();
            TextureManager.CommitComputeBindings();

            _context.Renderer.Pipeline.DispatchCompute(
                dispatchParams.UnpackGridSizeX(),
                dispatchParams.UnpackGridSizeY(),
                dispatchParams.UnpackGridSizeZ());

            UpdateShaderState(state);
        }
    }
}