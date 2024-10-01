using OpenTK.Graphics.OpenGL;

namespace Ryujinx.Graphics.OpenGL.Effects
{
    internal static class ShaderHelper
    {
        public static int CompileProgram(string shaderCode, ShaderType shaderType)
        {
            var shader = GL.CreateShader(shaderType);
            GL.ShaderSource(shader, shaderCode);
            GL.CompileShader(shader);

            var program = GL.CreateProgram();
            GL.AttachShader(program, shader);
            GL.LinkProgram(program);

            GL.DetachShader(program, shader);
            GL.DeleteShader(shader);

            return program;
        }

        public static int CompileProgram(string[] shaders, ShaderType shaderType)
        {
            var shader = GL.CreateShader(shaderType);
            GL.ShaderSource(shader, shaders.Length, shaders, (int[])null);
            GL.CompileShader(shader);

            var program = GL.CreateProgram();
            GL.AttachShader(program, shader);
            GL.LinkProgram(program);

            GL.DetachShader(program, shader);
            GL.DeleteShader(shader);

            return program;
        }
    }
}
