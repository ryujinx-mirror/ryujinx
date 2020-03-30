using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    class VertexArray : IDisposable
    {
        public int Handle { get; private set; }

        private bool _needsAttribsUpdate;

        private VertexBufferDescriptor[] _vertexBuffers;
        private VertexAttribDescriptor[] _vertexAttribs;

        public VertexArray()
        {
            Handle = GL.GenVertexArray();
        }

        public void Bind()
        {
            GL.BindVertexArray(Handle);
        }

        public void SetVertexBuffers(VertexBufferDescriptor[] vertexBuffers)
        {
            int bindingIndex = 0;

            foreach (VertexBufferDescriptor vb in vertexBuffers)
            {
                if (vb.Buffer.Buffer != null)
                {
                    int bufferHandle = ((Buffer)vb.Buffer.Buffer).Handle;

                    GL.BindVertexBuffer(bindingIndex, bufferHandle, (IntPtr)vb.Buffer.Offset, vb.Stride);

                    GL.VertexBindingDivisor(bindingIndex, vb.Divisor);
                }
                else
                {
                    GL.BindVertexBuffer(bindingIndex, 0, IntPtr.Zero, 0);
                }

                bindingIndex++;
            }

            _vertexBuffers = vertexBuffers;

            _needsAttribsUpdate = true;
        }

        public void SetVertexAttributes(VertexAttribDescriptor[] vertexAttribs)
        {
            int attribIndex = 0;

            foreach (VertexAttribDescriptor attrib in vertexAttribs)
            {
                FormatInfo fmtInfo = FormatTable.GetFormatInfo(attrib.Format);

                if (attrib.IsZero)
                {
                    // Disabling the attribute causes the shader to read a constant value.
                    // The value is configurable, but by default is a vector of (0, 0, 0, 1).
                    GL.DisableVertexAttribArray(attribIndex);
                }
                else
                {
                    GL.EnableVertexAttribArray(attribIndex);
                }
                
                int offset = attrib.Offset;
                int size   = fmtInfo.Components;

                bool isFloat = fmtInfo.PixelType == PixelType.Float ||
                               fmtInfo.PixelType == PixelType.HalfFloat;

                if (isFloat || fmtInfo.Normalized || fmtInfo.Scaled)
                {
                    VertexAttribType type = (VertexAttribType)fmtInfo.PixelType;

                    GL.VertexAttribFormat(attribIndex, size, type, fmtInfo.Normalized, offset);
                }
                else
                {
                    VertexAttribIntegerType type = (VertexAttribIntegerType)fmtInfo.PixelType;

                    GL.VertexAttribIFormat(attribIndex, size, type, offset);
                }

                GL.VertexAttribBinding(attribIndex, attrib.BufferIndex);

                attribIndex++;
            }

            for (; attribIndex < 16; attribIndex++)
            {
                GL.DisableVertexAttribArray(attribIndex);
            }

            _vertexAttribs = vertexAttribs;
        }

        public void SetIndexBuffer(Buffer indexBuffer)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffer?.Handle ?? 0);
        }

        public void Validate()
        {
            for (int attribIndex = 0; attribIndex < _vertexAttribs.Length; attribIndex++)
            {
                VertexAttribDescriptor attrib = _vertexAttribs[attribIndex];

                if ((uint)attrib.BufferIndex >= _vertexBuffers.Length)
                {
                    GL.DisableVertexAttribArray(attribIndex);

                    continue;
                }

                if (_vertexBuffers[attrib.BufferIndex].Buffer.Buffer == null)
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
