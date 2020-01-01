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
            if (args.Length == 1 || args.Length == 2)
            {
                TranslationFlags flags = TranslationFlags.DebugMode;

                if (args.Length == 2 && args[0] == "--compute")
                {
                    flags |= TranslationFlags.Compute;
                }

                byte[] data = File.ReadAllBytes(args[^1]);

                string code = Translator.Translate(data, new TranslatorCallbacks(null, null), flags).Code;

                Console.WriteLine(code);
            }
            else
            {
                Console.WriteLine("Usage: Ryujinx.ShaderTools [--compute] shader.bin");
            }
        }
    }
}