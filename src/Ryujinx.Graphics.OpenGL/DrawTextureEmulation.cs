using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.OpenGL.Image;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    class DrawTextureEmulation
    {
        private const string VertexShader = @"#version 430 core

uniform float srcX0;
uniform float srcY0;
uniform float srcX1;
uniform float srcY1;

layout (location = 0) out vec2 texcoord;

void main()
{
    bool x1 = (gl_VertexID & 1) != 0;
    bool y1 = (gl_VertexID & 2) != 0;
    gl_Position = vec4(x1 ? 1 : -1, y1 ? -1 : 1, 0, 1);
    texcoord = vec2(x1 ? srcX1 : srcX0, y1 ? srcY1 : srcY0);
}";

        private const string FragmentShader = @"#version 430 core

layout (location = 0) uniform sampler2D tex;

layout (location = 0) in vec2 texcoord;
layout (location = 0) out vec4 colour;

void main()
{
    colour = texture(tex, texcoord);
}";

        private int _vsHandle;
        private int _fsHandle;
        private int _programHandle;
        private int _uniformSrcX0Location;
        private int _uniformSrcY0Location;
        private int _uniformSrcX1Location;
        private int _uniformSrcY1Location;
        private bool _initialized;

        public void Draw(
            TextureView texture,
            Sampler sampler,
            float x0,
            float y0,
            float x1,
            float y1,
            float s0,
            float t0,
            float s1,
            float t1)
        {
            EnsureInitialized();

            GL.UseProgram(_programHandle);

            texture.Bind(0);
            sampler.Bind(0);

            if (x0 > x1)
            {
                (s1, s0) = (s0, s1);
            }

            if (y0 > y1)
            {
                (t1, t0) = (t0, t1);
            }

            GL.Uniform1(_uniformSrcX0Location, s0);
            GL.Uniform1(_uniformSrcY0Location, t0);
            GL.Uniform1(_uniformSrcX1Location, s1);
            GL.Uniform1(_uniformSrcY1Location, t1);

            GL.ViewportIndexed(0, MathF.Min(x0, x1), MathF.Min(y0, y1), MathF.Abs(x1 - x0), MathF.Abs(y1 - y0));

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            _vsHandle = GL.CreateShader(ShaderType.VertexShader);
            _fsHandle = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(_vsHandle, VertexShader);
            GL.ShaderSource(_fsHandle, FragmentShader);

            GL.CompileShader(_vsHandle);
            GL.CompileShader(_fsHandle);

            _programHandle = GL.CreateProgram();

            GL.AttachShader(_programHandle, _vsHandle);
            GL.AttachShader(_programHandle, _fsHandle);

            GL.LinkProgram(_programHandle);

            GL.DetachShader(_programHandle, _vsHandle);
            GL.DetachShader(_programHandle, _fsHandle);

            _uniformSrcX0Location = GL.GetUniformLocation(_programHandle, "srcX0");
            _uniformSrcY0Location = GL.GetUniformLocation(_programHandle, "srcY0");
            _uniformSrcX1Location = GL.GetUniformLocation(_programHandle, "srcX1");
            _uniformSrcY1Location = GL.GetUniformLocation(_programHandle, "srcY1");
        }

        public void Dispose()
        {
            if (!_initialized)
            {
                return;
            }

            GL.DeleteShader(_vsHandle);
            GL.DeleteShader(_fsHandle);
            GL.DeleteProgram(_programHandle);

            _initialized = false;
        }
    }
}
