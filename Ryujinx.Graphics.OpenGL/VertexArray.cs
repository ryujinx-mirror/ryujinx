using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.OpenGL
{
    class VertexArray : IDisposable
    {
        public int Handle { get; private set; }

        private bool _needsAttribsUpdate;

        private readonly VertexAttribDescriptor[] _vertexAttribs;
        private readonly VertexBufferDescriptor[] _vertexBuffers;

        private int _vertexAttribsCount;
        private int _vertexBuffersCount;

        private uint _vertexAttribsInUse;
        private uint _vertexBuffersInUse;

        public VertexArray()
        {
            Handle = GL.GenVertexArray();

            _vertexAttribs = new VertexAttribDescriptor[Constants.MaxVertexAttribs];
            _vertexBuffers = new VertexBufferDescriptor[Constants.MaxVertexBuffers];
        }

        public void Bind()
        {
            GL.BindVertexArray(Handle);
        }

        public void SetVertexBuffers(ReadOnlySpan<VertexBufferDescriptor> vertexBuffers)
        {
            int bindingIndex;
            for (bindingIndex = 0; bindingIndex < vertexBuffers.Length; bindingIndex++)
            {
                VertexBufferDescriptor vb = vertexBuffers[bindingIndex];

                if (vb.Buffer.Handle != BufferHandle.Null)
                {
                    GL.BindVertexBuffer(bindingIndex, vb.Buffer.Handle.ToInt32(), (IntPtr)vb.Buffer.Offset, vb.Stride);
                    GL.VertexBindingDivisor(bindingIndex, vb.Divisor);
                    _vertexBuffersInUse |= 1u << bindingIndex;
                }
                else
                {
                    if ((_vertexBuffersInUse & (1u << bindingIndex)) != 0)
                    {
                        GL.BindVertexBuffer(bindingIndex, 0, IntPtr.Zero, 0);
                        _vertexBuffersInUse &= ~(1u << bindingIndex);
                    }
                }

                _vertexBuffers[bindingIndex] = vb;
            }

            _vertexBuffersCount = bindingIndex;
            _needsAttribsUpdate = true;
        }

        public void SetVertexAttributes(ReadOnlySpan<VertexAttribDescriptor> vertexAttribs)
        {
            int index = 0;

            for (; index < vertexAttribs.Length; index++)
            {
                VertexAttribDescriptor attrib = vertexAttribs[index];

                if (attrib.Equals(_vertexAttribs[index]))
                {
                    continue;
                }

                FormatInfo fmtInfo = FormatTable.GetFormatInfo(attrib.Format);

                if (attrib.IsZero)
                {
                    // Disabling the attribute causes the shader to read a constant value.
                    // The value is configurable, but by default is a vector of (0, 0, 0, 1).
                    DisableVertexAttrib(index);
                }
                else
                {
                    EnableVertexAttrib(index);
                }

                int offset = attrib.Offset;
                int size   = fmtInfo.Components;

                bool isFloat = fmtInfo.PixelType == PixelType.Float ||
                               fmtInfo.PixelType == PixelType.HalfFloat;

                if (isFloat || fmtInfo.Normalized || fmtInfo.Scaled)
                {
                    VertexAttribType type = (VertexAttribType)fmtInfo.PixelType;

                    GL.VertexAttribFormat(index, size, type, fmtInfo.Normalized, offset);
                }
                else
                {
                    VertexAttribIntegerType type = (VertexAttribIntegerType)fmtInfo.PixelType;

                    GL.VertexAttribIFormat(index, size, type, offset);
                }

                GL.VertexAttribBinding(index, attrib.BufferIndex);

                _vertexAttribs[index] = attrib;
            }

            _vertexAttribsCount = index;

            for (; index < Constants.MaxVertexAttribs; index++)
            {
                DisableVertexAttrib(index);
            }
        }

        public void SetIndexBuffer(BufferHandle buffer)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffer.ToInt32());
        }

        public void Validate()
        {
            for (int attribIndex = 0; attribIndex < _vertexAttribsCount; attribIndex++)
            {
                VertexAttribDescriptor attrib = _vertexAttribs[attribIndex];

                if (!attrib.IsZero)
                {
                    if ((uint)attrib.BufferIndex >= _vertexBuffersCount)
                    {
                        DisableVertexAttrib(attribIndex);
                        continue;
                    }

                    if (_vertexBuffers[attrib.BufferIndex].Buffer.Handle == BufferHandle.Null)
                    {
                        DisableVertexAttrib(attribIndex);
                        continue;
                    }

                    if (_needsAttribsUpdate)
                    {
                        EnableVertexAttrib(attribIndex);
                    }
                }
            }

            _needsAttribsUpdate = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnableVertexAttrib(int index)
        {
            uint mask = 1u << index;

            if ((_vertexAttribsInUse & mask) == 0)
            {
                _vertexAttribsInUse |= mask;
                GL.EnableVertexAttribArray(index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DisableVertexAttrib(int index)
        {
            uint mask = 1u << index;

            if ((_vertexAttribsInUse & mask) != 0)
            {
                _vertexAttribsInUse &= ~mask;
                GL.DisableVertexAttribArray(index);
            }
        }

        public void Dispose()
        {
            if (Handle != 0)
            {
                GL.DeleteVertexArray(Handle);

                Handle = 0;
            }
        }
    }
}
