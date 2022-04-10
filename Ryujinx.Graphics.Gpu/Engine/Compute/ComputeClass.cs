using Ryujinx.Graphics.Device;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.InlineToMemory;
using Ryujinx.Graphics.Gpu.Engine.Threed;
using Ryujinx.Graphics.Gpu.Engine.Types;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Gpu.Shader;
using Ryujinx.Graphics.Shader;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Gpu.Engine.Compute
{
    /// <summary>
    /// Represents a compute engine class.
    /// </summary>
    class ComputeClass : IDeviceState
    {
        private readonly GpuContext _context;
        private readonly GpuChannel _channel;
        private readonly ThreedClass _3dEngine;
        private readonly DeviceState<ComputeClassState> _state;

        private readonly InlineToMemoryClass _i2mClass;

        /// <summary>
        /// Creates a new instance of the compute engine class.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="channel">GPU channel</param>
        /// <param name="threedEngine">3D engine</param>
        public ComputeClass(GpuContext context, GpuChannel channel, ThreedClass threedEngine)
        {
            _context = context;
            _channel = channel;
            _3dEngine = threedEngine;
            _state = new DeviceState<ComputeClassState>(new Dictionary<string, RwCallback>
            {
                { nameof(ComputeClassState.LaunchDma), new RwCallback(LaunchDma, null) },
                { nameof(ComputeClassState.LoadInlineData), new RwCallback(LoadInlineData, null) },
                { nameof(ComputeClassState.SendSignalingPcasB), new RwCallback(SendSignalingPcasB, null) }
            });

            _i2mClass = new InlineToMemoryClass(context, channel, initializeState: false);
        }

        /// <summary>
        /// Reads data from the class registers.
        /// </summary>
        /// <param name="offset">Register byte offset</param>
        /// <returns>Data at the specified offset</returns>
        public int Read(int offset) => _state.Read(offset);

        /// <summary>
        /// Writes data to the class registers.
        /// </summary>
        /// <param name="offset">Register byte offset</param>
        /// <param name="data">Data to be written</param>
        public void Write(int offset, int data) => _state.Write(offset, data);

        /// <summary>
        /// Launches the Inline-to-Memory DMA copy operation.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void LaunchDma(int argument)
        {
            _i2mClass.LaunchDma(ref Unsafe.As<ComputeClassState, InlineToMemoryClassState>(ref _state.State), argument);
        }

        /// <summary>
        /// Pushes a block of data to the Inline-to-Memory engine.
        /// </summary>
        /// <param name="data">Data to push</param>
        public void LoadInlineData(ReadOnlySpan<int> data)
        {
            _i2mClass.LoadInlineData(data);
        }

        /// <summary>
        /// Pushes a word of data to the Inline-to-Memory engine.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void LoadInlineData(int argument)
        {
            _i2mClass.LoadInlineData(argument);
        }

        /// <summary>
        /// Performs the compute dispatch operation.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void SendSignalingPcasB(int argument)
        {
            var memoryManager = _channel.MemoryManager;

            _3dEngine.FlushUboDirty();

            uint qmdAddress = _state.State.SendPcasA;

            var qmd = _channel.MemoryManager.Read<ComputeQmd>((ulong)qmdAddress << 8);

            ulong shaderGpuVa = ((ulong)_state.State.SetProgramRegionAAddressUpper << 32) | _state.State.SetProgramRegionB;

            shaderGpuVa += (uint)qmd.ProgramOffset;

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

                _channel.BufferManager.SetComputeUniformBuffer(index, gpuVa, size);
            }

            ulong samplerPoolGpuVa = ((ulong)_state.State.SetTexSamplerPoolAOffsetUpper << 32) | _state.State.SetTexSamplerPoolB;
            ulong texturePoolGpuVa = ((ulong)_state.State.SetTexHeaderPoolAOffsetUpper << 32) | _state.State.SetTexHeaderPoolB;

            GpuChannelPoolState poolState = new GpuChannelPoolState(
                texturePoolGpuVa,
                _state.State.SetTexHeaderPoolCMaximumIndex,
                _state.State.SetBindlessTextureConstantBufferSlotSelect);

            GpuChannelComputeState computeState = new GpuChannelComputeState(
                qmd.CtaThreadDimension0,
                qmd.CtaThreadDimension1,
                qmd.CtaThreadDimension2,
                localMemorySize,
                sharedMemorySize);

            CachedShaderProgram cs = memoryManager.Physical.ShaderCache.GetComputeShader(_channel, poolState, computeState, shaderGpuVa);

            _context.Renderer.Pipeline.SetProgram(cs.HostProgram);

            _channel.TextureManager.SetComputeSamplerPool(samplerPoolGpuVa, _state.State.SetTexSamplerPoolCMaximumIndex, qmd.SamplerIndex);
            _channel.TextureManager.SetComputeTexturePool(texturePoolGpuVa, _state.State.SetTexHeaderPoolCMaximumIndex);
            _channel.TextureManager.SetComputeTextureBufferIndex(_state.State.SetBindlessTextureConstantBufferSlotSelect);

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

                ulong cbDescAddress = _channel.BufferManager.GetComputeUniformBufferAddress(0);

                int cbDescOffset = 0x260 + (cb.Slot - 8) * 0x10;

                cbDescAddress += (ulong)cbDescOffset;

                SbDescriptor cbDescriptor = _channel.MemoryManager.Physical.Read<SbDescriptor>(cbDescAddress);

                _channel.BufferManager.SetComputeUniformBuffer(cb.Slot, cbDescriptor.PackAddress(), (uint)cbDescriptor.Size);
            }

            for (int index = 0; index < info.SBuffers.Count; index++)
            {
                BufferDescriptor sb = info.SBuffers[index];

                ulong sbDescAddress = _channel.BufferManager.GetComputeUniformBufferAddress(0);

                int sbDescOffset = 0x310 + sb.Slot * 0x10;

                sbDescAddress += (ulong)sbDescOffset;

                SbDescriptor sbDescriptor = _channel.MemoryManager.Physical.Read<SbDescriptor>(sbDescAddress);

                _channel.BufferManager.SetComputeStorageBuffer(sb.Slot, sbDescriptor.PackAddress(), (uint)sbDescriptor.Size, sb.Flags);
            }

            _channel.BufferManager.SetComputeStorageBufferBindings(info.SBuffers);
            _channel.BufferManager.SetComputeUniformBufferBindings(info.CBuffers);

            TextureBindingInfo[] textureBindings = _channel.TextureManager.RentComputeTextureBindings(info.Textures.Count);

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

            TextureBindingInfo[] imageBindings = _channel.TextureManager.RentComputeImageBindings(info.Images.Count);

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

            _channel.TextureManager.CommitComputeBindings();
            _channel.BufferManager.CommitComputeBindings();

            _context.Renderer.Pipeline.DispatchCompute(qmd.CtaRasterWidth, qmd.CtaRasterHeight, qmd.CtaRasterDepth);

            _3dEngine.ForceShaderUpdate();
        }
    }
}
