using System;

namespace Ryujinx.Graphics.Shader
{
    public class ShaderProgram
    {
        public ShaderProgramInfo Info { get; }

        public ShaderStage Stage { get; }

        public string Code { get; private set; }

        internal ShaderProgram(ShaderProgramInfo info, ShaderStage stage, string code)
        {
            Info  = info;
            Stage = stage;
            Code  = code;
        }

        public void Prepend(string line)
        {
            Code = line + Environment.NewLine + Code;
        }

        public void Replace(string name, string value)
        {
            Code = Code.Replace(name, value);
        }
    }
}