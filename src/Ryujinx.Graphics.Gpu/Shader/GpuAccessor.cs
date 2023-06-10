using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Represents a GPU state and memory accessor.
    /// </summary>
    class GpuAccessor : GpuAccessorBase, IGpuAccessor
    {
        private readonly GpuChannel _channel;
        private readonly GpuAccessorState _state;
        private readonly int _stageIndex;
        private readonly bool _compute;
        private readonly bool _isVulkan;

        /// <summary>
        /// Creates a new instance of the GPU state accessor for graphics shader translation.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="channel">GPU channel</param>
        /// <param name="state">Current GPU state</param>
        /// <param name="stageIndex">Graphics shader stage index (0 = Vertex, 4 = Fragment)</param>
        public GpuAccessor(
            GpuContext context,
            GpuChannel channel,
            GpuAccessorState state,
            int stageIndex) : base(context, state.ResourceCounts, stageIndex, state.TransformFeedbackDescriptors != null)
        {
            _isVulkan = context.Capabilities.Api == TargetApi.Vulkan;
            _channel = channel;
            _state = state;
            _stageIndex = stageIndex;
        }

        /// <summary>
        /// Creates a new instance of the GPU state accessor for compute shader translation.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="channel">GPU channel</param>
        /// <param name="state">Current GPU state</param>
        public GpuAccessor(GpuContext context, GpuChannel channel, GpuAccessorState state) : base(context, state.ResourceCounts, 0, false)
        {
            _channel = channel;
            _state = state;
            _compute = true;
        }

        /// <inheritdoc/>
        public uint ConstantBuffer1Read(int offset)
        {
            ulong baseAddress = _compute
                ? _channel.BufferManager.GetComputeUniformBufferAddress(1)
                : _channel.BufferManager.GetGraphicsUniformBufferAddress(_stageIndex, 1);

            return _channel.MemoryManager.Physical.Read<uint>(baseAddress + (ulong)offset);
        }

        /// <inheritdoc/>
        public void Log(string message)
        {
            Logger.Warning?.Print(LogClass.Gpu, $"Shader translator: {message}");
        }

        /// <inheritdoc/>
        public ReadOnlySpan<ulong> GetCode(ulong address, int minimumSize)
        {
            int size = Math.Max(minimumSize, 0x1000 - (int)(address & 0xfff));
            return MemoryMarshal.Cast<byte, ulong>(_channel.MemoryManager.GetSpan(address, size));
        }

        /// <inheritdoc/>
        public bool QueryAlphaToCoverageDitherEnable()
        {
            return _state.GraphicsState.AlphaToCoverageEnable && _state.GraphicsState.AlphaToCoverageDitherEnable;
        }

        /// <inheritdoc/>
        public AlphaTestOp QueryAlphaTestCompare()
        {
            if (!_isVulkan || !_state.GraphicsState.AlphaTestEnable)
            {
                return AlphaTestOp.Always;
            }

            return _state.GraphicsState.AlphaTestCompare switch
            {
                CompareOp.Never or CompareOp.NeverGl => AlphaTestOp.Never,
                CompareOp.Less or CompareOp.LessGl => AlphaTestOp.Less,
                CompareOp.Equal or CompareOp.EqualGl => AlphaTestOp.Equal,
                CompareOp.LessOrEqual or CompareOp.LessOrEqualGl => AlphaTestOp.LessOrEqual,
                CompareOp.Greater or CompareOp.GreaterGl => AlphaTestOp.Greater,
                CompareOp.NotEqual or CompareOp.NotEqualGl => AlphaTestOp.NotEqual,
                CompareOp.GreaterOrEqual or CompareOp.GreaterOrEqualGl => AlphaTestOp.GreaterOrEqual,
                _ => AlphaTestOp.Always
            };
        }

        /// <inheritdoc/>
        public float QueryAlphaTestReference()
        {
            return _state.GraphicsState.AlphaTestReference;
        }

        /// <inheritdoc/>
        public AttributeType QueryAttributeType(int location)
        {
            return _state.GraphicsState.AttributeTypes[location];
        }

        /// <inheritdoc/>
        public AttributeType QueryFragmentOutputType(int location)
        {
            return _state.GraphicsState.FragmentOutputTypes[location];
        }

        /// <inheritdoc/>
        public int QueryComputeLocalSizeX() => _state.ComputeState.LocalSizeX;

        /// <inheritdoc/>
        public int QueryComputeLocalSizeY() => _state.ComputeState.LocalSizeY;

        /// <inheritdoc/>
        public int QueryComputeLocalSizeZ() => _state.ComputeState.LocalSizeZ;

        /// <inheritdoc/>
        public int QueryComputeLocalMemorySize() => _state.ComputeState.LocalMemorySize;

        /// <inheritdoc/>
        public int QueryComputeSharedMemorySize() => _state.ComputeState.SharedMemorySize;

        /// <inheritdoc/>
        public uint QueryConstantBufferUse()
        {
            uint useMask = _compute
                ? _channel.BufferManager.GetComputeUniformBufferUseMask()
                : _channel.BufferManager.GetGraphicsUniformBufferUseMask(_stageIndex);

            _state.SpecializationState?.RecordConstantBufferUse(_stageIndex, useMask);
            return useMask;
        }

        /// <inheritdoc/>
        public bool QueryHasConstantBufferDrawParameters()
        {
            return _state.GraphicsState.HasConstantBufferDrawParameters;
        }

        /// <inheritdoc/>
        public bool QueryHasUnalignedStorageBuffer()
        {
            return _state.GraphicsState.HasUnalignedStorageBuffer || _state.ComputeState.HasUnalignedStorageBuffer;
        }

        /// <inheritdoc/>
        public bool QueryDualSourceBlendEnable()
        {
            return _state.GraphicsState.DualSourceBlendEnable;
        }

        /// <inheritdoc/>
        public InputTopology QueryPrimitiveTopology()
        {
            _state.SpecializationState?.RecordPrimitiveTopology();
            return ConvertToInputTopology(_state.GraphicsState.Topology, _state.GraphicsState.TessellationMode);
        }

        /// <inheritdoc/>
        public bool QueryProgramPointSize()
        {
            return _state.GraphicsState.ProgramPointSizeEnable;
        }

        /// <inheritdoc/>
        public float QueryPointSize()
        {
            return _state.GraphicsState.PointSize;
        }

        /// <inheritdoc/>
        public bool QueryTessCw()
        {
            return _state.GraphicsState.TessellationMode.UnpackCw();
        }

        /// <inheritdoc/>
        public TessPatchType QueryTessPatchType()
        {
            return _state.GraphicsState.TessellationMode.UnpackPatchType();
        }

        /// <inheritdoc/>
        public TessSpacing QueryTessSpacing()
        {
            return _state.GraphicsState.TessellationMode.UnpackSpacing();
        }

        //// <inheritdoc/>
        public TextureFormat QueryTextureFormat(int handle, int cbufSlot)
        {
            _state.SpecializationState?.RecordTextureFormat(_stageIndex, handle, cbufSlot);
            var descriptor = GetTextureDescriptor(handle, cbufSlot);
            return ConvertToTextureFormat(descriptor.UnpackFormat(), descriptor.UnpackSrgb());
        }

        /// <inheritdoc/>
        public SamplerType QuerySamplerType(int handle, int cbufSlot)
        {
            _state.SpecializationState?.RecordTextureSamplerType(_stageIndex, handle, cbufSlot);
            return GetTextureDescriptor(handle, cbufSlot).UnpackTextureTarget().ConvertSamplerType();
        }

        /// <inheritdoc/>
        public bool QueryTextureCoordNormalized(int handle, int cbufSlot)
        {
            _state.SpecializationState?.RecordTextureCoordNormalized(_stageIndex, handle, cbufSlot);
            return GetTextureDescriptor(handle, cbufSlot).UnpackTextureCoordNormalized();
        }

        /// <summary>
        /// Gets the texture descriptor for a given texture on the pool.
        /// </summary>
        /// <param name="handle">Index of the texture (this is the word offset of the handle in the constant buffer)</param>
        /// <param name="cbufSlot">Constant buffer slot for the texture handle</param>
        /// <returns>Texture descriptor</returns>
        private Image.TextureDescriptor GetTextureDescriptor(int handle, int cbufSlot)
        {
            if (_compute)
            {
                return _channel.TextureManager.GetComputeTextureDescriptor(
                    _state.PoolState.TexturePoolGpuVa,
                    _state.PoolState.TextureBufferIndex,
                    _state.PoolState.TexturePoolMaximumId,
                    handle,
                    cbufSlot);
            }
            else
            {
                return _channel.TextureManager.GetGraphicsTextureDescriptor(
                    _state.PoolState.TexturePoolGpuVa,
                    _state.PoolState.TextureBufferIndex,
                    _state.PoolState.TexturePoolMaximumId,
                    _stageIndex,
                    handle,
                    cbufSlot);
            }
        }

        /// <inheritdoc/>
        public bool QueryTransformDepthMinusOneToOne()
        {
            return _state.GraphicsState.DepthMode;
        }

        /// <inheritdoc/>
        public bool QueryTransformFeedbackEnabled()
        {
            return _state.TransformFeedbackDescriptors != null;
        }

        /// <inheritdoc/>
        public ReadOnlySpan<byte> QueryTransformFeedbackVaryingLocations(int bufferIndex)
        {
            return _state.TransformFeedbackDescriptors[bufferIndex].AsSpan();
        }

        /// <inheritdoc/>
        public int QueryTransformFeedbackStride(int bufferIndex)
        {
            return _state.TransformFeedbackDescriptors[bufferIndex].Stride;
        }

        /// <inheritdoc/>
        public bool QueryEarlyZForce()
        {
            _state.SpecializationState?.RecordEarlyZForce();
            return _state.GraphicsState.EarlyZForce;
        }

        /// <inheritdoc/>
        public bool QueryViewportTransformDisable()
        {
            return _state.GraphicsState.ViewportTransformDisable;
        }

        /// <inheritdoc/>
        public void RegisterTexture(int handle, int cbufSlot)
        {
            _state.SpecializationState?.RegisterTexture(_stageIndex, handle, cbufSlot, GetTextureDescriptor(handle, cbufSlot));
        }
    }
}
