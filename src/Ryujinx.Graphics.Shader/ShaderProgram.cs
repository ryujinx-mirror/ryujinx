using Ryujinx.Graphics.Shader.Translation;
using System;

namespace Ryujinx.Graphics.Shader
{
    public class ShaderProgram
    {
        public ShaderProgramInfo Info { get; }
        public TargetLanguage Language { get; }

        public string Code { get; private set; }
        public byte[] BinaryCode { get; }

        private ShaderProgram(ShaderProgramInfo info, TargetLanguage language)
        {
            Info = info;
            Language = language;
        }

        public ShaderProgram(ShaderProgramInfo info, TargetLanguage language, string code) : this(info, language)
        {
            Code = code;
        }

        public ShaderProgram(ShaderProgramInfo info, TargetLanguage language, byte[] binaryCode) : this(info, language)
        {
            BinaryCode = binaryCode;
        }

        public void Prepend(string line)
        {
            Code = line + Environment.NewLine + Code;
        }
    }
}
