using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Support buffer data updater.
    /// </summary>
    class SupportBufferUpdater : IDisposable
    {
        private SupportBuffer _data;
        private BufferHandle _handle;

        private readonly IRenderer _renderer;
        private int _startOffset = -1;
        private int _endOffset = -1;

        /// <summary>
        /// Creates a new instance of the support buffer updater.
        /// </summary>
        /// <param name="renderer">Renderer that the support buffer will be used with</param>
        public SupportBufferUpdater(IRenderer renderer)
        {
            _renderer = renderer;

            var defaultScale = new Vector4<float> { X = 1f, Y = 0f, Z = 0f, W = 0f };
            _data.RenderScale.AsSpan().Fill(defaultScale);
            DirtyRenderScale(0, SupportBuffer.RenderScaleMaxCount);
        }

        /// <summary>
        /// Mark a region of the support buffer as modified and needing to be sent to the GPU.
        /// </summary>
        /// <param name="startOffset">Start offset of the region in bytes</param>
        /// <param name="byteSize">Size of the region in bytes</param>
        private void MarkDirty(int startOffset, int byteSize)
        {
            int endOffset = startOffset + byteSize;

            if (_startOffset == -1)
            {
                _startOffset = startOffset;
                _endOffset = endOffset;
            }
            else
            {
                if (startOffset < _startOffset)
                {
                    _startOffset = startOffset;
                }

                if (endOffset > _endOffset)
                {
                    _endOffset = endOffset;
                }
            }
        }

        /// <summary>
        /// Marks the fragment render scale count as being modified.
        /// </summary>
        private void DirtyFragmentRenderScaleCount()
        {
            MarkDirty(SupportBuffer.FragmentRenderScaleCountOffset, sizeof(int));
        }

        /// <summary>
        /// Marks data of a given type as being modified.
        /// </summary>
        /// <typeparam name="T">Type of the data</typeparam>
        /// <param name="baseOffset">Base offset of the data in bytes</param>
        /// <param name="offset">Index of the data, in elements</param>
        /// <param name="count">Number of elements</param>
        private void DirtyGenericField<T>(int baseOffset, int offset, int count) where T : unmanaged
        {
            int elemSize = Unsafe.SizeOf<T>();

            MarkDirty(baseOffset + offset * elemSize, count * elemSize);
        }

        /// <summary>
        /// Marks render scales as being modified.
        /// </summary>
        /// <param name="offset">Index of the first scale that was modified</param>
        /// <param name="count">Number of modified scales</param>
        private void DirtyRenderScale(int offset, int count)
        {
            DirtyGenericField<Vector4<float>>(SupportBuffer.GraphicsRenderScaleOffset, offset, count);
        }

        /// <summary>
        /// Marks render target BGRA format state as modified.
        /// </summary>
        /// <param name="offset">Index of the first render target that had its BGRA format modified</param>
        /// <param name="count">Number of render targets</param>
        private void DirtyFragmentIsBgra(int offset, int count)
        {
            DirtyGenericField<Vector4<int>>(SupportBuffer.FragmentIsBgraOffset, offset, count);
        }

        /// <summary>
        /// Updates the inverse viewport vector.
        /// </summary>
        /// <param name="data">Inverse viewport vector</param>
        private void UpdateViewportInverse(Vector4<float> data)
        {
            _data.ViewportInverse = data;

            MarkDirty(SupportBuffer.ViewportInverseOffset, SupportBuffer.FieldSize);
        }

        /// <summary>
        /// Sets the scale of all output render targets (they should all have the same scale).
        /// </summary>
        /// <param name="scale">Scale value</param>
        public void SetRenderTargetScale(float scale)
        {
            _data.RenderScale[0].X = scale;
            DirtyRenderScale(0, 1); // Just the first element.
        }

        /// <summary>
        /// Updates the render scales for shader input textures or images.
        /// </summary>
        /// <param name="scales">Scale values</param>
        /// <param name="totalCount">Total number of scales across all stages</param>
        /// <param name="fragmentCount">Total number of scales on the fragment shader stage</param>
        public void UpdateRenderScale(ReadOnlySpan<float> scales, int totalCount, int fragmentCount)
        {
            bool changed = false;

            for (int index = 0; index < totalCount; index++)
            {
                if (_data.RenderScale[1 + index].X != scales[index])
                {
                    _data.RenderScale[1 + index].X = scales[index];
                    changed = true;
                }
            }

            // Only update fragment count if there are scales after it for the vertex stage.
            if (fragmentCount != totalCount && fragmentCount != _data.FragmentRenderScaleCount.X)
            {
                _data.FragmentRenderScaleCount.X = fragmentCount;
                DirtyFragmentRenderScaleCount();
            }

            if (changed)
            {
                DirtyRenderScale(0, 1 + totalCount);
            }
        }

        /// <summary>
        /// Sets whether the format of a given render target is a BGRA format.
        /// </summary>
        /// <param name="index">Render target index</param>
        /// <param name="isBgra">True if the format is BGRA< false otherwise</param>
        public void SetRenderTargetIsBgra(int index, bool isBgra)
        {
            bool isBgraChanged = (_data.FragmentIsBgra[index].X != 0) != isBgra;

            if (isBgraChanged)
            {
                _data.FragmentIsBgra[index].X = isBgra ? 1 : 0;
                DirtyFragmentIsBgra(index, 1);
            }
        }

        /// <summary>
        /// Sets whether a viewport has transform disabled.
        /// </summary>
        /// <param name="viewportWidth">Value used as viewport width</param>
        /// <param name="viewportHeight">Value used as viewport height</param>
        /// <param name="scale">Render target scale</param>
        /// <param name="disableTransform">True if transform is disabled, false otherwise</param>
        public void SetViewportTransformDisable(float viewportWidth, float viewportHeight, float scale, bool disableTransform)
        {
            float disableTransformF = disableTransform ? 1.0f : 0.0f;
            if (_data.ViewportInverse.W != disableTransformF || disableTransform)
            {
                UpdateViewportInverse(new Vector4<float>
                {
                    X = scale * 2f / viewportWidth,
                    Y = scale * 2f / viewportHeight,
                    Z = 1,
                    W = disableTransformF,
                });
            }
        }

        /// <summary>
        /// Submits all pending buffer updates to the GPU.
        /// </summary>
        public void Commit()
        {
            if (_startOffset != -1)
            {
                if (_handle == BufferHandle.Null)
                {
                    _handle = _renderer.CreateBuffer(SupportBuffer.RequiredSize);
                    _renderer.Pipeline.ClearBuffer(_handle, 0, SupportBuffer.RequiredSize, 0);

                    var range = new BufferRange(_handle, 0, SupportBuffer.RequiredSize);
                    _renderer.Pipeline.SetUniformBuffers(stackalloc[] { new BufferAssignment(0, range) });
                }

                ReadOnlySpan<byte> data = MemoryMarshal.Cast<SupportBuffer, byte>(MemoryMarshal.CreateSpan(ref _data, 1));

                _renderer.SetBufferData(_handle, _startOffset, data[_startOffset.._endOffset]);

                _startOffset = -1;
                _endOffset = -1;
            }
        }

        /// <summary>
        /// Destroys the support buffer.
        /// </summary>
        public void Dispose()
        {
            if (_handle != BufferHandle.Null)
            {
                _renderer.DeleteBuffer(_handle);
                _handle = BufferHandle.Null;
            }
        }
    }
}
