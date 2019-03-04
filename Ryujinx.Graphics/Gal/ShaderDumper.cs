using System;
using System.IO;

namespace Ryujinx.Graphics.Gal
{
    static class ShaderDumper
    {
        private static string _runtimeDir;

        public static int DumpIndex { get; private set; } = 1;

        public static void Dump(IGalMemory memory, long position, GalShaderType type, string extSuffix = "")
        {
            if (!IsDumpEnabled())
            {
                return;
            }

            string fileName = "Shader" + DumpIndex.ToString("d4") + "." + ShaderExtension(type) + extSuffix + ".bin";

            string fullPath = Path.Combine(FullDir(), fileName);
            string codePath = Path.Combine(CodeDir(), fileName);

            DumpIndex++;

            using (FileStream fullFile = File.Create(fullPath))
            using (FileStream codeFile = File.Create(codePath))
            {
                BinaryWriter fullWriter = new BinaryWriter(fullFile);
                BinaryWriter codeWriter = new BinaryWriter(codeFile);

                for (long i = 0; i < 0x50; i += 4)
                {
                    fullWriter.Write(memory.ReadInt32(position + i));
                }

                long offset = 0;

                ulong instruction = 0;

                //Dump until a NOP instruction is found
                while ((instruction >> 48 & 0xfff8) != 0x50b0)
                {
                    uint word0 = (uint)memory.ReadInt32(position + 0x50 + offset + 0);
                    uint word1 = (uint)memory.ReadInt32(position + 0x50 + offset + 4);

                    instruction = word0 | (ulong)word1 << 32;

                    //Zero instructions (other kind of NOP) stop immediatly,
                    //this is to avoid two rows of zeroes
                    if (instruction == 0)
                    {
                        break;
                    }

                    fullWriter.Write(instruction);
                    codeWriter.Write(instruction);

                    offset += 8;
                }

                //Align to meet nvdisasm requeriments
                while (offset % 0x20 != 0)
                {
                    fullWriter.Write(0);
                    codeWriter.Write(0);

                    offset += 4;
                }
            }
        }

        public static bool IsDumpEnabled()
        {
            return !string.IsNullOrWhiteSpace(GraphicsConfig.ShadersDumpPath);
        }

        private static string FullDir()
        {
            return CreateAndReturn(Path.Combine(DumpDir(), "Full"));
        }

        private static string CodeDir()
        {
            return CreateAndReturn(Path.Combine(DumpDir(), "Code"));
        }

        private static string DumpDir()
        {
            if (string.IsNullOrEmpty(_runtimeDir))
            {
                int index = 1;

                do
                {
                    _runtimeDir = Path.Combine(GraphicsConfig.ShadersDumpPath, "Dumps" + index.ToString("d2"));

                    index++;
                }
                while (Directory.Exists(_runtimeDir));

                Directory.CreateDirectory(_runtimeDir);
            }

            return _runtimeDir;
        }

        private static string CreateAndReturn(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return dir;
        }

        private static string ShaderExtension(GalShaderType type)
        {
            switch (type)
            {
                case GalShaderType.Vertex:         return "vert";
                case GalShaderType.TessControl:    return "tesc";
                case GalShaderType.TessEvaluation: return "tese";
                case GalShaderType.Geometry:       return "geom";
                case GalShaderType.Fragment:       return "frag";

                default: throw new ArgumentException(nameof(type));
            }
        }
    }
}