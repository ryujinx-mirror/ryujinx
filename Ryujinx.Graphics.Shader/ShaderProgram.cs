using System;

namespace Ryujinx.Graphics.Shader
{
    public class ShaderProgram
    {
        public ShaderProgramInfo Info { get; }

        public ShaderStage Stage { get; }

        public string Code { get; private set; }

        public int Size { get; }

        internal ShaderProgram(ShaderProgramInfo info, ShaderStage stage, string code, int size)
        {
            Info  = info;
            Stage = stage;
            Code  = code;
            Size  = size;
        }

        public void Prepend(string line)
        {
            Code = line + Environment.NewLine + Code;
        }
    }
}