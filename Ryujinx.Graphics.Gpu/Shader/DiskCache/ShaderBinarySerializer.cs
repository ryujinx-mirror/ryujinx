using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader.Translation;
using System.Collections.Generic;
using System.IO;

namespace Ryujinx.Graphics.Gpu.Shader.DiskCache
{
    static class ShaderBinarySerializer
    {
        public static byte[] Pack(ShaderSource[] sources)
        {
            using MemoryStream output = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(output);

            for (int i = 0; i < sources.Length; i++)
            {
                writer.Write(sources[i].BinaryCode.Length);
                writer.Write(sources[i].BinaryCode);
            }

            return output.ToArray();
        }

        public static ShaderSource[] Unpack(CachedShaderStage[] stages, byte[] code, bool compute)
        {
            using MemoryStream input = new MemoryStream(code);
            using BinaryReader reader = new BinaryReader(input);

            List<ShaderSource> output = new List<ShaderSource>();

            for (int i = compute ? 0 : 1; i < stages.Length; i++)
            {
                CachedShaderStage stage = stages[i];

                if (stage == null)
                {
                    continue;
                }

                int binaryCodeLength = reader.ReadInt32();
                byte[] binaryCode = reader.ReadBytes(binaryCodeLength);

                output.Add(new ShaderSource(binaryCode, ShaderCache.GetBindings(stage.Info), stage.Info.Stage, TargetLanguage.Spirv));
            }

            return output.ToArray();
        }
    }
}