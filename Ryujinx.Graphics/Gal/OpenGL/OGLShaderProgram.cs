using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    struct OGLShaderProgram
    {
        public OGLShaderStage Vertex;
        public OGLShaderStage TessControl;
        public OGLShaderStage TessEvaluation;
        public OGLShaderStage Geometry;
        public OGLShaderStage Fragment;
    }

    class OGLShaderStage : IDisposable
    {
        public int Handle { get; private set; }

        public bool IsCompiled { get; private set; }

        public GalShaderType Type { get; private set; }

        public string Code { get; private set; }

        public IEnumerable<ShaderDeclInfo> ConstBufferUsage { get; private set; }
        public IEnumerable<ShaderDeclInfo> TextureUsage     { get; private set; }

        public OGLShaderStage(
            GalShaderType               Type,
            string                      Code,
            IEnumerable<ShaderDeclInfo> ConstBufferUsage,
            IEnumerable<ShaderDeclInfo> TextureUsage)
        {
            this.Type             = Type;
            this.Code             = Code;
            this.ConstBufferUsage = ConstBufferUsage;
            this.TextureUsage     = TextureUsage;
        }

        public void Compile()
        {
            if (Handle == 0)
            {
                Handle = GL.CreateShader(OGLEnumConverter.GetShaderType(Type));

                CompileAndCheck(Handle, Code);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing && Handle != 0)
            {
                GL.DeleteShader(Handle);

                Handle = 0;
            }
        }

        public static void CompileAndCheck(int Handle, string Code)
        {
            GL.ShaderSource(Handle, Code);
            GL.CompileShader(Handle);

            CheckCompilation(Handle);
        }

        private static void CheckCompilation(int Handle)
        {
            int Status = 0;

            GL.GetShader(Handle, ShaderParameter.CompileStatus, out Status);

            if (Status == 0)
            {
                throw new ShaderException(GL.GetShaderInfoLog(Handle));
            }
        }
    }
}