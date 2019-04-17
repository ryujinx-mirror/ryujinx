using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.IO;

namespace Ryujinx.ShaderTools
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                GalShaderType type = GalShaderType.Vertex;

                switch (args[0].ToLower())
                {
                    case "v":  type = GalShaderType.Vertex;         break;
                    case "tc": type = GalShaderType.TessControl;    break;
                    case "te": type = GalShaderType.TessEvaluation; break;
                    case "g":  type = GalShaderType.Geometry;       break;
                    case "f":  type = GalShaderType.Fragment;       break;
                }

                using (FileStream fs = new FileStream(args[1], FileMode.Open, FileAccess.Read))
                {
                    Memory mem = new Memory(fs);

                    ShaderConfig config = new ShaderConfig(type, 65536);

                    string code = Translator.Translate(mem, 0, config).Code;

                    Console.WriteLine(code);
                }
            }
            else
            {
                Console.WriteLine("Usage: Ryujinx.ShaderTools [v|tc|te|g|f] shader.bin");
            }
        }
    }
}