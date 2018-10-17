using System;
using System.IO;

namespace Ryujinx.Graphics.Gal
{
    static class ShaderDumper
    {
        private static string RuntimeDir;

        public static int DumpIndex { get; private set; } = 1;

        public static void Dump(IGalMemory Memory, long Position, GalShaderType Type, string ExtSuffix = "")
        {
            if (!IsDumpEnabled())
            {
                return;
            }

            string FileName = "Shader" + DumpIndex.ToString("d4") + "." + ShaderExtension(Type) + ExtSuffix + ".bin";

            string FullPath = Path.Combine(FullDir(), FileName);
            string CodePath = Path.Combine(CodeDir(), FileName);

            DumpIndex++;

            using (FileStream FullFile = File.Create(FullPath))
            using (FileStream CodeFile = File.Create(CodePath))
            {
                BinaryWriter FullWriter = new BinaryWriter(FullFile);
                BinaryWriter CodeWriter = new BinaryWriter(CodeFile);

                for (long i = 0; i < 0x50; i += 4)
                {
                    FullWriter.Write(Memory.ReadInt32(Position + i));
                }

                long Offset = 0;

                ulong Instruction = 0;

                //Dump until a NOP instruction is found
                while ((Instruction >> 52 & 0xfff8) != 0x50b0)
                {
                    uint Word0 = (uint)Memory.ReadInt32(Position + 0x50 + Offset + 0);
                    uint Word1 = (uint)Memory.ReadInt32(Position + 0x50 + Offset + 4);

                    Instruction = Word0 | (ulong)Word1 << 32;

                    //Zero instructions (other kind of NOP) stop immediatly,
                    //this is to avoid two rows of zeroes
                    if (Instruction == 0)
                    {
                        break;
                    }

                    FullWriter.Write(Instruction);
                    CodeWriter.Write(Instruction);

                    Offset += 8;
                }

                //Align to meet nvdisasm requeriments
                while (Offset % 0x20 != 0)
                {
                    FullWriter.Write(0);
                    CodeWriter.Write(0);

                    Offset += 4;
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
            if (string.IsNullOrEmpty(RuntimeDir))
            {
                int Index = 1;

                do
                {
                    RuntimeDir = Path.Combine(GraphicsConfig.ShadersDumpPath, "Dumps" + Index.ToString("d2"));

                    Index++;
                }
                while (Directory.Exists(RuntimeDir));

                Directory.CreateDirectory(RuntimeDir);
            }

            return RuntimeDir;
        }

        private static string CreateAndReturn(string Dir)
        {
            if (!Directory.Exists(Dir))
            {
                Directory.CreateDirectory(Dir);
            }

            return Dir;
        }

        private static string ShaderExtension(GalShaderType Type)
        {
            switch (Type)
            {
                case GalShaderType.Vertex:         return "vert";
                case GalShaderType.TessControl:    return "tesc";
                case GalShaderType.TessEvaluation: return "tese";
                case GalShaderType.Geometry:       return "geom";
                case GalShaderType.Fragment:       return "frag";

                default: throw new ArgumentException(nameof(Type));
            }
        }
    }
}