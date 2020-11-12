using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ryujinx.ShaderTools
{
    class Program
    {
        private class GpuAccessor : IGpuAccessor
        {
            private readonly byte[] _data;

            public GpuAccessor(byte[] data)
            {
                _data = data;
            }

            public T MemoryRead<T>(ulong address) where T : unmanaged
            {
                return MemoryMarshal.Cast<byte, T>(new ReadOnlySpan<byte>(_data).Slice((int)address))[0];
            }
        }

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

                string code = Translator.CreateContext(0, new GpuAccessor(data), flags).Translate(out _).Code;

                Console.WriteLine(code);
            }
            else
            {
                Console.WriteLine("Usage: Ryujinx.ShaderTools [--compute] shader.bin");
            }
        }
    }
}