using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gpu.Image;
using Ryujinx.Graphics.Shader;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.DiskCache
{
    /// <summary>
    /// Represents a GPU state and memory accessor.
    /// </summary>
    class DiskCacheGpuAccessor : GpuAccessorBase, IGpuAccessor
    {
        private readonly ReadOnlyMemory<byte> _data;
        private readonly ReadOnlyMemory<byte> _cb1Data;
        private readonly ShaderSpecializationState _oldSpecState;
        private readonly ShaderSpecializationState _newSpecState;
        private readonly int _stageIndex;
        private ResourceCounts _resourceCounts;

        /// <summary>
        /// Creates a new instance of the cached GPU state accessor for shader translation.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="data">The data of the shader</param>
        /// <param name="cb1Data">The constant buffer 1 data of the shader</param>
        /// <param name="oldSpecState">Shader specialization state of the cached shader</param>
        /// <param name="newSpecState">Shader specialization state of the recompiled shader</param>
        /// <param name="stageIndex">Shader stage index</param>
        public DiskCacheGpuAccessor(
            GpuContext context,
            ReadOnlyMemory<byte> data,
            ReadOnlyMemory<byte> cb1Data,
            ShaderSpecializationState oldSpecState,
            ShaderSpecializationState newSpecState,
            ResourceCounts counts,
            int stageIndex) : base(context)
        {
            _data = data;
            _cb1Data = cb1Data;
            _oldSpecState = oldSpecState;
            _newSpecState = newSpecState;
            _stageIndex = stageIndex;
            _resourceCounts = counts;
        }

        /// <inheritdoc/>
        public uint ConstantBuffer1Read(int offset)
        {
            if (offset + sizeof(uint) > _cb1Data.Length)
            {
                throw new DiskCacheLoadException(DiskCacheLoadResult.InvalidCb1DataLength);
            }

            return MemoryMarshal.Cast<byte, uint>(_cb1Data.Span.Slice(offset))[0];
        }

        /// <inheritdoc/>
        public void Log(string message)
        {
            Logger.Warning?.Print(LogClass.Gpu, $"Shader translator: {message}");
        }

        /// <inheritdoc/>
        public ReadOnlySpan<ulong> GetCode(ulong address, int minimumSize)
        {
            return MemoryMarshal.Cast<byte, ulong>(_data.Span.Slice((int)address));
        }

        /// <inheritdoc/>
        public int QueryBindingConstantBuffer(int index)
        {
            return _resourceCounts.UniformBuffersCount++;
        }

        /// <inheritdoc/>
        public int QueryBindingStorageBuffer(int index)
        {
            return _resourceCounts.StorageBuffersCount++;
        }

        /// <inheritdoc/>
        public int QueryBindingTexture(int index)
        {
            return _resourceCounts.TexturesCount++;
        }

        /// <inheritdoc/>
        public int QueryBindingImage(int index)
        {
            return _resourceCounts.ImagesCount++;
        }

        /// <inheritdoc/>
        public int QueryComputeLocalSizeX() => _oldSpecState.ComputeState.LocalSizeX;

        /// <inheritdoc/>
        public int QueryComputeLocalSizeY() => _oldSpecState.ComputeState.LocalSizeY;

        /// <inheritdoc/>
        public int QueryComputeLocalSizeZ() => _oldSpecState.ComputeState.LocalSizeZ;

        /// <inheritdoc/>
        public int QueryComputeLocalMemorySize() => _oldSpecState.ComputeState.LocalMemorySize;

        /// <inheritdoc/>
        public int QueryComputeSharedMemorySize() => _oldSpecState.ComputeState.SharedMemorySize;

        /// <inheritdoc/>
        public uint QueryConstantBufferUse()
        {
            _newSpecState.RecordConstantBufferUse(_stageIndex, _oldSpecState.ConstantBufferUse[_stageIndex]);
            return _oldSpecState.ConstantBufferUse[_stageIndex];
        }

        /// <inheritdoc/>
        public InputTopology QueryPrimitiveTopology()
        {
            _newSpecState.RecordPrimitiveTopology();
            return ConvertToInputTopology(_oldSpecState.GraphicsState.Topology, _oldSpecState.GraphicsState.TessellationMode);
        }

        /// <inheritdoc/>
        public bool QueryTessCw()
        {
            return _oldSpecState.GraphicsState.TessellationMode.UnpackCw();
        }

        /// <inheritdoc/>
        public TessPatchType QueryTessPatchType()
        {
            return _oldSpecState.GraphicsState.TessellationMode.UnpackPatchType();
        }

        /// <inheritdoc/>
        public TessSpacing QueryTessSpacing()
        {
            return _oldSpecState.GraphicsState.TessellationMode.UnpackSpacing();
        }

        /// <inheritdoc/>
        public TextureFormat QueryTextureFormat(int handle, int cbufSlot)
        {
            _newSpecState.RecordTextureFormat(_stageIndex, handle, cbufSlot);
            (uint format, bool formatSrgb) = _oldSpecState.GetFormat(_stageIndex, handle, cbufSlot);
            return ConvertToTextureFormat(format, formatSrgb);
        }

        /// <inheritdoc/>
        public SamplerType QuerySamplerType(int handle, int cbufSlot)
        {
            _newSpecState.RecordTextureSamplerType(_stageIndex, handle, cbufSlot);
            return _oldSpecState.GetTextureTarget(_stageIndex, handle, cbufSlot).ConvertSamplerType();
        }

        /// <inheritdoc/>
        public bool QueryTextureCoordNormalized(int handle, int cbufSlot)
        {
            _newSpecState.RecordTextureCoordNormalized(_stageIndex, handle, cbufSlot);
            return _oldSpecState.GetCoordNormalized(_stageIndex, handle, cbufSlot);
        }

        /// <inheritdoc/>
        public bool QueryTransformFeedbackEnabled()
        {
            return _oldSpecState.TransformFeedbackDescriptors != null;
        }

        /// <inheritdoc/>
        public ReadOnlySpan<byte> QueryTransformFeedbackVaryingLocations(int bufferIndex)
        {
            return _oldSpecState.TransformFeedbackDescriptors[bufferIndex].AsSpan();
        }

        /// <inheritdoc/>
        public int QueryTransformFeedbackStride(int bufferIndex)
        {
            return _oldSpecState.TransformFeedbackDescriptors[bufferIndex].Stride;
        }

        /// <inheritdoc/>
        public bool QueryEarlyZForce()
        {
            _newSpecState.RecordEarlyZForce();
            return _oldSpecState.GraphicsState.EarlyZForce;
        }

        /// <inheritdoc/>
        public void RegisterTexture(int handle, int cbufSlot)
        {
            if (!_oldSpecState.TextureRegistered(_stageIndex, handle, cbufSlot))
            {
                throw new DiskCacheLoadException(DiskCacheLoadResult.MissingTextureDescriptor);
            }

            (uint format, bool formatSrgb) = _oldSpecState.GetFormat(_stageIndex, handle, cbufSlot);
            TextureTarget target = _oldSpecState.GetTextureTarget(_stageIndex, handle, cbufSlot);
            bool coordNormalized = _oldSpecState.GetCoordNormalized(_stageIndex, handle, cbufSlot);
            _newSpecState.RegisterTexture(_stageIndex, handle, cbufSlot, format, formatSrgb, target, coordNormalized);
        }
    }
}
