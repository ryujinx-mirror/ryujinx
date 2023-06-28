using Ryujinx.Graphics.Shader;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.GAL
{
    public class SupportBufferUpdater : IDisposable
    {
        public SupportBuffer Data;
        public BufferHandle Handle;

        private readonly IRenderer _renderer;
        private int _startOffset = -1;
        private int _endOffset = -1;

        public SupportBufferUpdater(IRenderer renderer)
        {
            _renderer = renderer;
            Handle = renderer.CreateBuffer(SupportBuffer.RequiredSize);
            renderer.Pipeline.ClearBuffer(Handle, 0, SupportBuffer.RequiredSize, 0);
        }

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

        public void UpdateFragmentRenderScaleCount(int count)
        {
            if (Data.FragmentRenderScaleCount.X != count)
            {
                Data.FragmentRenderScaleCount.X = count;

                MarkDirty(SupportBuffer.FragmentRenderScaleCountOffset, sizeof(int));
            }
        }

        private void UpdateGenericField<T>(int baseOffset, ReadOnlySpan<T> data, Span<T> target, int offset, int count) where T : unmanaged
        {
            data[..count].CopyTo(target[offset..]);

            int elemSize = Unsafe.SizeOf<T>();

            MarkDirty(baseOffset + offset * elemSize, count * elemSize);
        }

        public void UpdateRenderScale(ReadOnlySpan<Vector4<float>> data, int offset, int count)
        {
            UpdateGenericField(SupportBuffer.GraphicsRenderScaleOffset, data, Data.RenderScale.AsSpan(), offset, count);
        }

        public void UpdateFragmentIsBgra(ReadOnlySpan<Vector4<int>> data, int offset, int count)
        {
            UpdateGenericField(SupportBuffer.FragmentIsBgraOffset, data, Data.FragmentIsBgra.AsSpan(), offset, count);
        }

        public void UpdateViewportInverse(Vector4<float> data)
        {
            Data.ViewportInverse = data;

            MarkDirty(SupportBuffer.ViewportInverseOffset, SupportBuffer.FieldSize);
        }

        public void Commit()
        {
            if (_startOffset != -1)
            {
                ReadOnlySpan<byte> data = MemoryMarshal.Cast<SupportBuffer, byte>(MemoryMarshal.CreateSpan(ref Data, 1));

                _renderer.SetBufferData(Handle, _startOffset, data[_startOffset.._endOffset]);

                _startOffset = -1;
                _endOffset = -1;
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _renderer.DeleteBuffer(Handle);
        }
    }
}
