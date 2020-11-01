using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture bindings manager.
    /// </summary>
    class TextureBindingsManager
    {
        private const int HandleHigh = 16;
        private const int HandleMask = (1 << HandleHigh) - 1;

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

        /// <summary>
        /// Constructs a new instance of the texture bindings manager.
        /// </summary>
        /// <param name="context">The GPU context that the texture bindings manager belongs to</param>
        /// <param name="texturePoolCache">Texture pools cache used to get texture pools from</param>
        /// <param name="isCompute">True if the bindings manager is used for the compute engine</param>
        public TextureBindingsManager(GpuContext context, TexturePoolCache texturePoolCache, bool isCompute)
        {
            _context          = context;
            _texturePoolCache = texturePoolCache;
            _isCompute        = isCompute;

            int stages = isCompute ? 1 : Constants.ShaderStages;

            _textureBindings = new TextureBindingInfo[stages][];
            _imageBindings   = new TextureBindingInfo[stages][];

            _textureState = new TextureStatePerStage[stages][];
            _imageState   = new TextureStatePerStage[stages][];
        }

        /// <summary>
        /// Binds textures for a given shader stage.
        /// </summary>
        /// <param name="stage">Shader stage number, or 0 for compute shaders</param>
        /// <param name="bindings">Texture bindings</param>
        public void SetTextures(int stage, TextureBindingInfo[] bindings)
        {
            _textureBindings[stage] = bindings;
            _textureState[stage] = new TextureStatePerStage[bindings.Length];
        }

        /// <summary>
        /// Binds images for a given shader stage.
        /// </summary>
        /// <param name="stage">Shader stage number, or 0 for compute shaders</param>
        /// <param name="bindings">Image bindings</param>
        public void SetImages(int stage, TextureBindingInfo[] bindings)
        {
            _imageBindings[stage] = bindings;
            _imageState[stage] = new TextureStatePerStage[bindings.Length];
        }

        /// <summary>
        /// Sets the textures constant buffer index.
        /// The constant buffer specified holds the texture handles.
        /// </summary>
        /// <param name="index">Constant buffer index</param>
        public void SetTextureBufferIndex(int index)
        {
            _textureBufferIndex = index;
        }

        /// <summary>
        /// Sets the current texture sampler pool to be used.
        /// </summary>
        /// <param name="gpuVa">Start GPU virtual address of the pool</param>
        /// <param name="maximumId">Maximum ID of the pool (total count minus one)</param>
        /// <param name="samplerIndex">Type of the sampler pool indexing used for bound samplers</param>
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

        /// <summary>
        /// Sets the current texture pool to be used.
        /// </summary>
        /// <param name="gpuVa">Start GPU virtual address of the pool</param>
        /// <param name="maximumId">Maximum ID of the pool (total count minus one)</param>
        public void SetTexturePool(ulong gpuVa, int maximumId)
        {
            ulong address = _context.MemoryManager.Translate(gpuVa);

            _texturePoolAddress   = address;
            _texturePoolMaximumId = maximumId;
        }

        /// <summary>
        /// Ensures that the bindings are visible to the host GPU.
        /// Note: this actually performs the binding using the host graphics API.
        /// </summary>
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

        /// <summary>
        /// Ensures that the texture bindings are visible to the host GPU.
        /// Note: this actually performs the binding using the host graphics API.
        /// </summary>
        /// <param name="pool">The current texture pool</param>
        /// <param name="stage">The shader stage using the textures to be bound</param>
        /// <param name="stageIndex">The stage number of the specified shader stage</param>
        private void CommitTextureBindings(TexturePool pool, ShaderStage stage, int stageIndex)
        {
            if (_textureBindings[stageIndex] == null)
            {
                return;
            }

            bool changed = false;

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

                    packedId = _context.PhysicalMemory.Read<int>(address + (ulong)binding.CbufOffset * 4);
                }
                else
                {
                    packedId = ReadPackedId(stageIndex, binding.Handle, _textureBufferIndex);
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

                if ((binding.Flags & TextureUsageFlags.ResScaleUnsupported) != 0)
                {
                    texture?.BlacklistScale();
                }

                ITexture hostTexture = texture?.GetTargetTexture(binding.Target);

                if (_textureState[stageIndex][index].Texture != hostTexture || _rebind)
                {
                    _textureState[stageIndex][index].Texture = hostTexture;

                    _context.Renderer.Pipeline.SetTexture(index, stage, hostTexture);

                    changed = true;
                }

                if (hostTexture != null && texture.Info.Target == Target.TextureBuffer)
                {
                    // Ensure that the buffer texture is using the correct buffer as storage.
                    // Buffers are frequently re-created to accomodate larger data, so we need to re-bind
                    // to ensure we're not using a old buffer that was already deleted.
                    _context.Methods.BufferManager.SetBufferTextureStorage(hostTexture, texture.Address, texture.Size, _isCompute);
                }

                Sampler sampler = _samplerPool.Get(samplerId);

                ISampler hostSampler = sampler?.HostSampler;

                if (_textureState[stageIndex][index].Sampler != hostSampler || _rebind)
                {
                    _textureState[stageIndex][index].Sampler = hostSampler;

                    _context.Renderer.Pipeline.SetSampler(index, stage, hostSampler);
                }
            }

            if (changed)
            {
                _context.Renderer.Pipeline.UpdateRenderScale(stage, _textureBindings[stageIndex].Length);
            }
        }

        /// <summary>
        /// Ensures that the image bindings are visible to the host GPU.
        /// Note: this actually performs the binding using the host graphics API.
        /// </summary>
        /// <param name="pool">The current texture pool</param>
        /// <param name="stage">The shader stage using the textures to be bound</param>
        /// <param name="stageIndex">The stage number of the specified shader stage</param>
        private void CommitImageBindings(TexturePool pool, ShaderStage stage, int stageIndex)
        {
            if (_imageBindings[stageIndex] == null)
            {
                return;
            }

            for (int index = 0; index < _imageBindings[stageIndex].Length; index++)
            {
                TextureBindingInfo binding = _imageBindings[stageIndex][index];

                int packedId = ReadPackedId(stageIndex, binding.Handle, _textureBufferIndex);
                int textureId = UnpackTextureId(packedId);

                Texture texture = pool.Get(textureId);

                if ((binding.Flags & TextureUsageFlags.ResScaleUnsupported) != 0)
                {
                    texture?.BlacklistScale();
                }

                ITexture hostTexture = texture?.GetTargetTexture(binding.Target);

                if (hostTexture != null && texture.Info.Target == Target.TextureBuffer)
                {
                    // Ensure that the buffer texture is using the correct buffer as storage.
                    // Buffers are frequently re-created to accomodate larger data, so we need to re-bind
                    // to ensure we're not using a old buffer that was already deleted.
                    _context.Methods.BufferManager.SetBufferTextureStorage(hostTexture, texture.Address, texture.Size, _isCompute);
                }

                if (_imageState[stageIndex][index].Texture != hostTexture || _rebind)
                {
                    _imageState[stageIndex][index].Texture = hostTexture;

                    Format format = binding.Format;

                    if (format == 0 && texture != null)
                    {
                        format = texture.Format;
                    }

                    _context.Renderer.Pipeline.SetImage(index, stage, hostTexture, format);
                }
            }
        }

        /// <summary>
        /// Gets the texture descriptor for a given texture handle.
        /// </summary>
        /// <param name="state">The current GPU state</param>
        /// <param name="stageIndex">The stage number where the texture is bound</param>
        /// <param name="handle">The texture handle</param>
        /// <returns>The texture descriptor for the specified texture</returns>
        public TextureDescriptor GetTextureDescriptor(GpuState state, int stageIndex, int handle)
        {
            int packedId = ReadPackedId(stageIndex, handle, state.Get<int>(MethodOffset.TextureBufferIndex));
            int textureId = UnpackTextureId(packedId);

            var poolState = state.Get<PoolState>(MethodOffset.TexturePoolState);

            ulong poolAddress = _context.MemoryManager.Translate(poolState.Address.Pack());

            TexturePool texturePool = _texturePoolCache.FindOrCreate(poolAddress, poolState.MaximumId);

            return texturePool.GetDescriptor(textureId);
        }

        /// <summary>
        /// Reads a packed texture and sampler ID (basically, the real texture handle)
        /// from the texture constant buffer.
        /// </summary>
        /// <param name="stageIndex">The number of the shader stage where the texture is bound</param>
        /// <param name="wordOffset">A word offset of the handle on the buffer (the "fake" shader handle)</param>
        /// <param name="textureBufferIndex">Index of the constant buffer holding the texture handles</param>
        /// <returns>The packed texture and sampler ID (the real texture handle)</returns>
        private int ReadPackedId(int stageIndex, int wordOffset, int textureBufferIndex)
        {
            ulong address;

            var bufferManager = _context.Methods.BufferManager;

            if (_isCompute)
            {
                address = bufferManager.GetComputeUniformBufferAddress(textureBufferIndex);
            }
            else
            {
                address = bufferManager.GetGraphicsUniformBufferAddress(stageIndex, textureBufferIndex);
            }

            int handle = _context.PhysicalMemory.Read<int>(address + (ulong)(wordOffset & HandleMask) * 4);

            // The "wordOffset" (which is really the immediate value used on texture instructions on the shader)
            // is a 13-bit value. However, in order to also support separate samplers and textures (which uses
            // bindless textures on the shader), we extend it with another value on the higher 16 bits with
            // another offset for the sampler.
            // The shader translator has code to detect separate texture and sampler uses with a bindless texture,
            // turn that into a regular texture access and produce those special handles with values on the higher 16 bits.
            if (wordOffset >> HandleHigh != 0)
            {
                handle |= _context.PhysicalMemory.Read<int>(address + (ulong)(wordOffset >> HandleHigh) * 4);
            }

            return handle;
        }

        /// <summary>
        /// Unpacks the texture ID from the real texture handle.
        /// </summary>
        /// <param name="packedId">The real texture handle</param>
        /// <returns>The texture ID</returns>
        private static int UnpackTextureId(int packedId)
        {
            return (packedId >> 0) & 0xfffff;
        }

        /// <summary>
        /// Unpacks the sampler ID from the real texture handle.
        /// </summary>
        /// <param name="packedId">The real texture handle</param>
        /// <returns>The sampler ID</returns>
        private static int UnpackSamplerId(int packedId)
        {
            return (packedId >> 20) & 0xfff;
        }

        /// <summary>
        /// Force all bound textures and images to be rebound the next time CommitBindings is called.
        /// </summary>
        public void Rebind()
        {
            _rebind = true;
        }
    }
}