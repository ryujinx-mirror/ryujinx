namespace Ryujinx.Graphics.Shader
{
    public class ShaderProgram
    {
        public ShaderProgramInfo Info { get; }

        public string Code { get; }

        internal ShaderProgram(ShaderProgramInfo info, string code)
        {
            Info = info;
            Code = code;
        }
    }
}