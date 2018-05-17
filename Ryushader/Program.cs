using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Gal.Shader;
using System;
using System.IO;

namespace Ryushader
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                GlslDecompiler Decompiler = new GlslDecompiler();

                GalShaderType ShaderType = GalShaderType.Vertex;

                switch (args[0].ToLower())
                {
                    case "v":  ShaderType = GalShaderType.Vertex;         break;
                    case "tc": ShaderType = GalShaderType.TessControl;    break;
                    case "te": ShaderType = GalShaderType.TessEvaluation; break;
                    case "g":  ShaderType = GalShaderType.Geometry;       break;
                    case "f":  ShaderType = GalShaderType.Fragment;       break;
                }

                byte[] Data = File.ReadAllBytes(args[1]);

                int[] Code = new int[Data.Length / 4];

                for (int Offset = 0; Offset < Data.Length; Offset += 4)
                {
                    int Value = BitConverter.ToInt32(Data, Offset);

                    Code[Offset >> 2] = Value;
                }

                GlslProgram Program = Decompiler.Decompile(Code, ShaderType);

                Console.WriteLine(Program.Code);
            }
            else
            {
                Console.WriteLine("Usage: Ryushader [v|tc|te|g|f] shader.bin");
            }
        }
    }
}
