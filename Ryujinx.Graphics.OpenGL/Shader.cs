using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.OpenGL
{
    class Shader : IShader
    {
        public int Handle { get; private set; }

        private ShaderProgram _program;

        public ShaderProgramInfo Info => _program.Info;

        public ShaderStage Stage => _program.Stage;

        public Shader(ShaderProgram program)
        {
            _program = program;

            ShaderType type = ShaderType.VertexShader;

            switch (program.Stage)
            {
                case ShaderStage.Compute:                type = ShaderType.ComputeShader;        break;
                case ShaderStage.Vertex:                 type = ShaderType.VertexShader;         break;
                case ShaderStage.TessellationControl:    type = ShaderType.TessControlShader;    break;
                case ShaderStage.TessellationEvaluation: type = ShaderType.TessEvaluationShader; break;
                case ShaderStage.Geometry:               type = ShaderType.GeometryShader;       break;
                case ShaderStage.Fragment:               type = ShaderType.FragmentShader;       break;
            }

            Handle = GL.CreateShader(type);

            GL.ShaderSource(Handle, program.Code);
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
