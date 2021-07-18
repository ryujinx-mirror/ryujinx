using System;

namespace Ryujinx.Graphics.Shader
{
    public class ShaderProgram
    {
        public ShaderStage Stage { get; }

        public string Code { get; private set; }
        public byte[] BinaryCode { get; }

        private ShaderProgram(ShaderStage stage)
        {
            Stage = stage;
        }

        public ShaderProgram(ShaderStage stage, string code) : this(stage)
        {
            Code = code;
        }

        public ShaderProgram(ShaderStage stage, byte[] binaryCode) : this(stage)
        {
            BinaryCode = binaryCode;
        }

        public void Prepend(string line)
        {
            Code = line + Environment.NewLine + Code;
        }
    }
}