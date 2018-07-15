using System;
using System.IO;

namespace Ryujinx.Graphics.Gal
{
    static class ShaderDumper
    {
        private static string RuntimeDir;

        private static int DumpIndex = 1;

        public static void Dump(IGalMemory Memory, long Position, GalShaderType Type, string ExtSuffix = "")
        {
            if (string.IsNullOrWhiteSpace(GraphicsConfig.ShadersDumpPath))
            {
                return;
            }

            string FileName = "Shader" + DumpIndex.ToString("d4") + "." + ShaderExtension(Type) + ExtSuffix + ".bin";

            string FilePath = Path.Combine(DumpDir(), FileName);

            DumpIndex++;

            using (FileStream Output = File.Create(FilePath))
            using (BinaryWriter Writer = new BinaryWriter(Output))
            {
                long Offset = 0;

                ulong Instruction = 0;

                //Dump until a NOP instruction is found
                while ((Instruction >> 52 & 0xfff8) != 0x50b0)
                {
                    uint Word0 = (uint)Memory.ReadInt32(Position + Offset + 0);
                    uint Word1 = (uint)Memory.ReadInt32(Position + Offset + 4);

                    Instruction = Word0 | (ulong)Word1 << 32;

                    //Zero instructions (other kind of NOP) stop immediatly,
                    //this is to avoid two rows of zeroes
                    if (Instruction == 0)
                    {
                        break;
                    }

                    Writer.Write(Instruction);

                    Offset += 8;
                }

                //Align to meet nvdisasm requeriments
                while (Offset % 0x20 != 0)
                {
                    Writer.Write(0);

                    Offset += 4;
                }
            }
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