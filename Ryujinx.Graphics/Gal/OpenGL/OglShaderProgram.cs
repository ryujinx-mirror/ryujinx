using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    struct OglShaderProgram
    {
        public OglShaderStage Vertex;
        public OglShaderStage TessControl;
        public OglShaderStage TessEvaluation;
        public OglShaderStage Geometry;
        public OglShaderStage Fragment;
    }

    class OglShaderStage : IDisposable
    {
        public int Handle { get; private set; }

        public bool IsCompiled { get; private set; }

        public GalShaderType Type { get; private set; }

        public string Code { get; private set; }

        public IEnumerable<ShaderDeclInfo> ConstBufferUsage { get; private set; }
        public IEnumerable<ShaderDeclInfo> TextureUsage     { get; private set; }

        public OglShaderStage(
            GalShaderType               type,
            string                      code,
            IEnumerable<ShaderDeclInfo> constBufferUsage,
            IEnumerable<ShaderDeclInfo> textureUsage)
        {
            Type             = type;
            Code             = code;
            ConstBufferUsage = constBufferUsage;
            TextureUsage     = textureUsage;
        }

        public void Compile()
        {
            if (Handle == 0)
            {
                Handle = GL.CreateShader(OglEnumConverter.GetShaderType(Type));

                CompileAndCheck(Handle, Code);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && Handle != 0)
            {
                GL.DeleteShader(Handle);

                Handle = 0;
            }
        }

        public static void CompileAndCheck(int handle, string code)
        {
            GL.ShaderSource(handle, code);
            GL.CompileShader(handle);

            CheckCompilation(handle);
        }

        private static void CheckCompilation(int handle)
        {
            int status = 0;

            GL.GetShader(handle, ShaderParameter.CompileStatus, out status);

            if (status == 0)
            {
                throw new ShaderException(GL.GetShaderInfoLog(handle));
            }
        }
    }
}