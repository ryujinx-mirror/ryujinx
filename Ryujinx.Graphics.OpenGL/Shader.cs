using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.OpenGL
{
    class Shader : IShader
    {
        public int Handle { get; private set; }

        public Shader(ShaderStage stage, string code)
        {
            ShaderType type = stage switch
            {
                ShaderStage.Compute => ShaderType.ComputeShader,
                ShaderStage.Vertex => ShaderType.VertexShader,
                ShaderStage.TessellationControl => ShaderType.TessControlShader,
                ShaderStage.TessellationEvaluation => ShaderType.TessEvaluationShader,
                ShaderStage.Geometry => ShaderType.GeometryShader,
                ShaderStage.Fragment => ShaderType.FragmentShader,
                _ => ShaderType.VertexShader
            };

            Handle = GL.CreateShader(type);

            GL.ShaderSource(Handle, code);
            GL.CompileShader(Handle);
        }

        public void Dispose()
        {
            if (Handle != 0)
            {
                GL.DeleteShader(Handle);

                Handle = 0;
            }
        }
    }
}
