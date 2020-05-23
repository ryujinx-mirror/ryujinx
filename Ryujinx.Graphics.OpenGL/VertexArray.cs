using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

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
            int bindingIndex = 0;

            for (int index = 0; index < vertexBuffers.Length; index++)
            {
                VertexBufferDescriptor vb = vertexBuffers[index];

                if (vb.Buffer.Handle != null)
                {
                    GL.BindVertexBuffer(bindingIndex, vb.Buffer.Handle.ToInt32(), (IntPtr)vb.Buffer.Offset, vb.Stride);

                    GL.VertexBindingDivisor(bindingIndex, vb.Divisor);
                }
                else
                {
                    GL.BindVertexBuffer(bindingIndex, 0, IntPtr.Zero, 0);
                }

                _vertexBuffers[index] = vb;

                bindingIndex++;
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

                FormatInfo fmtInfo = FormatTable.GetFormatInfo(attrib.Format);

                if (attrib.IsZero)
                {
                    // Disabling the attribute causes the shader to read a constant value.
                    // The value is configurable, but by default is a vector of (0, 0, 0, 1).
                    GL.DisableVertexAttribArray(index);
                }
                else
                {
                    GL.EnableVertexAttribArray(index);
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
                GL.DisableVertexAttribArray(index);
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

                if ((uint)attrib.BufferIndex >= _vertexBuffersCount)
                {
                    GL.DisableVertexAttribArray(attribIndex);

                    continue;
                }

                if (_vertexBuffers[attrib.BufferIndex].Buffer.Handle == null)
                {
                    GL.DisableVertexAttribArray(attribIndex);

                    continue;
                }

                if (_needsAttribsUpdate && !attrib.IsZero)
                {
                    GL.EnableVertexAttribArray(attribIndex);
                }
            }

            _needsAttribsUpdate = false;
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
