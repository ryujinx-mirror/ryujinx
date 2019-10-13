using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        public void Dispatch(int argument)
        {
            uint dispatchParamsAddress = (uint)_context.State.Get<int>(MethodOffset.DispatchParamsAddress);

            var dispatchParams = _context.MemoryAccessor.Read<ComputeParams>((ulong)dispatchParamsAddress << 8);

            GpuVa shaderBaseAddress = _context.State.Get<GpuVa>(MethodOffset.ShaderBaseAddress);

            ulong shaderGpuVa = shaderBaseAddress.Pack() + (uint)dispatchParams.ShaderOffset;

            ComputeShader cs = _shaderCache.GetComputeShader(
                shaderGpuVa,
                dispatchParams.UnpackBlockSizeX(),
                dispatchParams.UnpackBlockSizeY(),
                dispatchParams.UnpackBlockSizeZ());

            _context.Renderer.ComputePipeline.SetProgram(cs.Interface);

            ShaderProgramInfo info = cs.Shader.Info;

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

            _bufferManager.CommitComputeBindings();

            _context.Renderer.ComputePipeline.Dispatch(
                dispatchParams.UnpackGridSizeX(),
                dispatchParams.UnpackGridSizeY(),
                dispatchParams.UnpackGridSizeZ());
        }
    }
}