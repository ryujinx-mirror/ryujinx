using System;

namespace Ryujinx.Graphics.Shader
{
    public class ShaderProgram
    {
        public ShaderStage Stage { get; }

        public string Code { get; private set; }

        public ShaderProgram(ShaderStage stage, string code)
        {
            Stage = stage;
            Code  = code;
        }

        public void Prepend(string line)
        {
            Code = line + Environment.NewLine + Code;
        }
    }
}