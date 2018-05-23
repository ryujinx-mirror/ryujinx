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

                using (FileStream FS = new FileStream(args[1], FileMode.Open, FileAccess.Read))
                {
                    Memory Mem = new Memory(FS);

                    GlslProgram Program = Decompiler.Decompile(Mem, 0, ShaderType);

                    Console.WriteLine(Program.Code);
                }
            }
            else
            {
                Console.WriteLine("Usage: Ryushader [v|tc|te|g|f] shader.bin");
            }
        }
    }
}
