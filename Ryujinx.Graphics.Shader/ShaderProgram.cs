using System;

namespace Ryujinx.Graphics.Shader
{
    public class ShaderProgram
    {
        public ShaderProgramInfo Info { get; }

        public ShaderStage Stage { get; }

        public string Code { get; private set; }

        public int SizeA { get; }
        public int Size { get; }

        internal ShaderProgram(ShaderProgramInfo info, ShaderStage stage, string code, int size, int sizeA)
        {
            Info  = info;
            Stage = stage;
            Code  = code;
            SizeA = sizeA;
            Size  = size;
        }

        public void Prepend(string line)
        {
            Code = line + Environment.NewLine + Code;
        }
    }
}