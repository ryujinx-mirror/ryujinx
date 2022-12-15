using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using System;
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

            writer.Write(sources.Length);

            for (int i = 0; i < sources.Length; i++)
            {
                writer.Write((int)sources[i].Stage);
                writer.Write(sources[i].BinaryCode.Length);
                writer.Write(sources[i].BinaryCode);
            }

            return output.ToArray();
        }

        public static ShaderSource[] Unpack(CachedShaderStage[] stages, byte[] code)
        {
            using MemoryStream input = new MemoryStream(code);
            using BinaryReader reader = new BinaryReader(input);

            List<ShaderSource> output = new List<ShaderSource>();

            int count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                ShaderStage stage = (ShaderStage)reader.ReadInt32();
                int binaryCodeLength = reader.ReadInt32();
                byte[] binaryCode = reader.ReadBytes(binaryCodeLength);

                output.Add(new ShaderSource(binaryCode, GetBindings(stages, stage), stage, TargetLanguage.Spirv));
            }

            return output.ToArray();
        }

        private static ShaderBindings GetBindings(CachedShaderStage[] stages, ShaderStage stage)
        {
            for (int i = 0; i < stages.Length; i++)
            {
                CachedShaderStage currentStage = stages[i];

                if (currentStage?.Info != null && currentStage.Info.Stage == stage)
                {
                    return ShaderCache.GetBindings(currentStage.Info);
                }
            }

            return new ShaderBindings(Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>(), Array.Empty<int>());
        }
    }
}