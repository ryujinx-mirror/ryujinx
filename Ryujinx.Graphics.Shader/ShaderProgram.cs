using System;

namespace Ryujinx.Graphics.Shader
{
    public class ShaderProgram
    {
        public ShaderStage Stage { get; }

        public string Code { get; private set; }

        public int SizeA { get; }
        public int Size { get; }

        public ShaderProgram(ShaderStage stage, string code, int size, int sizeA)
        {
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