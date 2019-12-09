using Ryujinx.Graphics.GAL.Texture;
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
        public void Dispatch(GpuState state, int argument)
        {
            uint dispatchParamsAddress = (uint)state.Get<int>(MethodOffset.DispatchParamsAddress);

            var dispatchParams = _context.MemoryAccessor.Read<ComputeParams>((ulong)dispatchParamsAddress << 8);

            GpuVa shaderBaseAddress = state.Get<GpuVa>(MethodOffset.ShaderBaseAddress);

            ulong shaderGpuVa = shaderBaseAddress.Pack() + (uint)dispatchParams.ShaderOffset;

            // Note: A size of 0 is also invalid, the size must be at least 1.
            int sharedMemorySize = Math.Clamp(dispatchParams.SharedMemorySize & 0xffff, 1, _context.Capabilities.MaximumComputeSharedMemorySize);

            ComputeShader cs = _shaderCache.GetComputeShader(
                shaderGpuVa,
                sharedMemorySize,
                dispatchParams.UnpackBlockSizeX(),
                dispatchParams.UnpackBlockSizeY(),
                dispatchParams.UnpackBlockSizeZ());

            _context.Renderer.Pipeline.BindProgram(cs.HostProgram);

            var samplerPool = state.Get<PoolState>(MethodOffset.SamplerPoolState);

            _textureManager.SetComputeSamplerPool(samplerPool.Address.Pack(), samplerPool.MaximumId, dispatchParams.SamplerIndex);

            var texturePool = state.Get<PoolState>(MethodOffset.TexturePoolState);

            _textureManager.SetComputeTexturePool(texturePool.Address.Pack(), texturePool.MaximumId);

            _textureManager.SetComputeTextureBufferIndex(state.Get<int>(MethodOffset.TextureBufferIndex));

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

                _bufferManager.SetComputeUniformBuffer(index, gpuVa, size);
            }

            for (int index = 0; index < info.SBuffers.Count; index++)
            {
                BufferDescriptor sb = info.SBuffers[index];

                sbEnableMask |= 1u << sb.Slot;

                ulong sbDescAddress = _bufferManager.GetComputeUniformBufferAddress(0);

                int sbDescOffset = 0x310 + sb.Slot * 0x10;

                sbDescAddress += (ulong)sbDescOffset;

                Span<byte> sbDescriptorData = _context.PhysicalMemory.Read(sbDescAddress, 0x10);

                SbDescriptor sbDescriptor = MemoryMarshal.Cast<byte, SbDescriptor>(sbDescriptorData)[0];

                _bufferManager.SetComputeStorageBuffer(sb.Slot, sbDescriptor.PackAddress(), (uint)sbDescriptor.Size);
            }

            ubEnableMask = 0;

            for (int index = 0; index < info.CBuffers.Count; index++)
            {
                ubEnableMask |= 1u << info.CBuffers[index].Slot;
            }

            _bufferManager.SetComputeStorageBufferEnableMask(sbEnableMask);
            _bufferManager.SetComputeUniformBufferEnableMask(ubEnableMask);

            var textureBindings = new TextureBindingInfo[info.Textures.Count];

            for (int index = 0; index < info.Textures.Count; index++)
            {
                var descriptor = info.Textures[index];

                Target target = GetTarget(descriptor.Type);

                textureBindings[index] = new TextureBindingInfo(target, descriptor.HandleIndex);
            }

            _textureManager.SetComputeTextures(textureBindings);

            var imageBindings = new TextureBindingInfo[info.Images.Count];

            for (int index = 0; index < info.Images.Count; index++)
            {
                var descriptor = info.Images[index];

                Target target = GetTarget(descriptor.Type);

                imageBindings[index] = new TextureBindingInfo(target, descriptor.HandleIndex);
            }

            _textureManager.SetComputeImages(imageBindings);

            _bufferManager.CommitComputeBindings();
            _textureManager.CommitComputeBindings();

            _context.Renderer.Pipeline.Dispatch(
                dispatchParams.UnpackGridSizeX(),
                dispatchParams.UnpackGridSizeY(),
                dispatchParams.UnpackGridSizeZ());

            UpdateShaderState(state);
        }
    }
}