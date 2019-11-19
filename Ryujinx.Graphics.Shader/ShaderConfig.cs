using Ryujinx.Graphics.Shader.Translation;

namespace Ryujinx.Graphics.Shader
{
    struct ShaderConfig
    {
        public ShaderStage Stage { get; }

        public TranslationFlags Flags { get; }

        public int MaxOutputVertices { get; }

        public OutputTopology OutputTopology { get; }

        public ShaderConfig(
            ShaderStage      stage,
            TranslationFlags flags,
            int              maxOutputVertices,
            OutputTopology   outputTopology)
        {
            Stage             = stage;
            Flags             = flags;
            MaxOutputVertices = maxOutputVertices;
            OutputTopology    = outputTopology;
        }
    }
}