using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Image
{
    class TextureBindingsManager
    {
        private GpuContext _context;

        private bool _isCompute;

        private SamplerPool _samplerPool;

        private SamplerIndex _samplerIndex;

        private ulong _texturePoolAddress;
        private int   _texturePoolMaximumId;

        private TexturePoolCache _texturePoolCache;

        private TextureBindingInfo[][] _textureBindings;
        private TextureBindingInfo[][] _imageBindings;

        private struct TextureStatePerStage
        {
            public ITexture Texture;
            public ISampler Sampler;
        }

        private TextureStatePerStage[][] _textureState;
        private TextureStatePerStage[][] _imageState;

        private int _textureBufferIndex;

        private bool _rebind;

        public TextureBindingsManager(GpuContext context, TexturePoolCache texturePoolCache, bool isCompute)
        {
            _context          = context;
            _texturePoolCache = texturePoolCache;
            _isCompute        = isCompute;

            int stages = isCompute ? 1 : Constants.TotalShaderStages;

            _textureBindings = new TextureBindingInfo[stages][];
            _imageBindings   = new TextureBindingInfo[stages][];

            _textureState = new TextureStatePerStage[stages][];
            _imageState   = new TextureStatePerStage[stages][];
        }

        public void SetTextures(int stage, TextureBindingInfo[] bindings)
        {
            _textureBindings[stage] = bindings;

            _textureState[stage] = new TextureStatePerStage[bindings.Length];
        }

        public void SetImages(int stage, TextureBindingInfo[] bindings)
        {
            _imageBindings[stage] = bindings;

            _imageState[stage] = new TextureStatePerStage[bindings.Length];
        }

        public void SetTextureBufferIndex(int index)
        {
            _textureBufferIndex = index;
        }

        public void SetSamplerPool(ulong gpuVa, int maximumId, SamplerIndex samplerIndex)
        {
            ulong address = _context.MemoryManager.Translate(gpuVa);

            if (_samplerPool != null)
            {
                if (_samplerPool.Address == address && _samplerPool.MaximumId >= maximumId)
                {
                    return;
                }

                _samplerPool.Dispose();
            }

            _samplerPool = new SamplerPool(_context, address, maximumId);

            _samplerIndex = samplerIndex;
        }

        public void SetTexturePool(ulong gpuVa, int maximumId)
        {
            ulong address = _context.MemoryManager.Translate(gpuVa);

            _texturePoolAddress   = address;
            _texturePoolMaximumId = maximumId;
        }

        public void CommitBindings()
        {
            TexturePool texturePool = _texturePoolCache.FindOrCreate(
                _texturePoolAddress,
                _texturePoolMaximumId);

            if (_isCompute)
            {
                CommitTextureBindings(texturePool, ShaderStage.Compute, 0);
                CommitImageBindings  (texturePool, ShaderStage.Compute, 0);
            }
            else
            {
                for (ShaderStage stage = ShaderStage.Vertex; stage <= ShaderStage.Fragment; stage++)
                {
                    int stageIndex = (int)stage - 1;

                    CommitTextureBindings(texturePool, stage, stageIndex);
                    CommitImageBindings  (texturePool, stage, stageIndex);
                }
            }

            _rebind = false;
        }

        private void CommitTextureBindings(TexturePool pool, ShaderStage stage, int stageIndex)
        {
            if (_textureBindings[stageIndex] == null)
            {
                return;
            }

            for (int index = 0; index < _textureBindings[stageIndex].Length; index++)
            {
                TextureBindingInfo binding = _textureBindings[stageIndex][index];

                int packedId;

                if (binding.IsBindless)
                {
                    ulong address;

                    var bufferManager = _context.Methods.BufferManager;

                    if (_isCompute)
                    {
                        address = bufferManager.GetComputeUniformBufferAddress(binding.CbufSlot);
                    }
                    else
                    {
                        address = bufferManager.GetGraphicsUniformBufferAddress(stageIndex, binding.CbufSlot);
                    }

                    packedId = MemoryMarshal.Cast<byte, int>(_context.PhysicalMemory.Read(address + (ulong)binding.CbufOffset * 4, 4))[0];
                }
                else
                {
                    packedId = ReadPackedId(stageIndex, binding.Handle);
                }

                int textureId = UnpackTextureId(packedId);
                int samplerId;

                if (_samplerIndex == SamplerIndex.ViaHeaderIndex)
                {
                    samplerId = textureId;
                }
                else
                {
                    samplerId = UnpackSamplerId(packedId);
                }

                Texture texture = pool.Get(textureId);

                ITexture hostTexture = texture?.GetTargetTexture(binding.Target);

                if (_textureState[stageIndex][index].Texture != hostTexture || _rebind)
                {
                    _textureState[stageIndex][index].Texture = hostTexture;

                    _context.Renderer.Pipeline.BindTexture(index, stage, hostTexture);
                }

                Sampler sampler = _samplerPool.Get(samplerId);

                ISampler hostSampler = sampler?.HostSampler;

                if (_textureState[stageIndex][index].Sampler != hostSampler || _rebind)
                {
                    _textureState[stageIndex][index].Sampler = hostSampler;

                    _context.Renderer.Pipeline.BindSampler(index, stage, hostSampler);
                }
            }
        }

        private void CommitImageBindings(TexturePool pool, ShaderStage stage, int stageIndex)
        {
            if (_imageBindings[stageIndex] == null)
            {
                return;
            }

            for (int index = 0; index < _imageBindings[stageIndex].Length; index++)
            {
                TextureBindingInfo binding = _imageBindings[stageIndex][index];

                int packedId = ReadPackedId(stageIndex, binding.Handle);

                int textureId = UnpackTextureId(packedId);

                Texture texture = pool.Get(textureId);

                ITexture hostTexture = texture?.GetTargetTexture(binding.Target);

                if (_imageState[stageIndex][index].Texture != hostTexture || _rebind)
                {
                    _imageState[stageIndex][index].Texture = hostTexture;

                    _context.Renderer.Pipeline.BindImage(index, stage, hostTexture);
                }
            }
        }

        public TextureDescriptor GetTextureDescriptor(GpuState state, int stageIndex, int handle)
        {
            int packedId = ReadPackedId(stageIndex, handle);

            int textureId = UnpackTextureId(packedId);

            var poolState = state.Get<PoolState>(MethodOffset.TexturePoolState);

            ulong poolAddress = _context.MemoryManager.Translate(poolState.Address.Pack());

            TexturePool texturePool = _texturePoolCache.FindOrCreate(poolAddress, poolState.MaximumId);

            return texturePool.GetDescriptor(textureId);
        }

        private int ReadPackedId(int stage, int wordOffset)
        {
            ulong address;

            var bufferManager = _context.Methods.BufferManager;

            if (_isCompute)
            {
                address = bufferManager.GetComputeUniformBufferAddress(_textureBufferIndex);
            }
            else
            {
                address = bufferManager.GetGraphicsUniformBufferAddress(stage, _textureBufferIndex);
            }

            address += (uint)wordOffset * 4;

            return BitConverter.ToInt32(_context.PhysicalMemory.Read(address, 4));
        }

        private static int UnpackTextureId(int packedId)
        {
            return (packedId >> 0) & 0xfffff;
        }

        private static int UnpackSamplerId(int packedId)
        {
            return (packedId >> 20) & 0xfff;
        }

        public void InvalidatePoolRange(ulong address, ulong size)
        {
            _samplerPool?.InvalidateRange(address, size);

            _texturePoolCache.InvalidateRange(address, size);
        }

        public void Rebind()
        {
            _rebind = true;
        }
    }
}