using Ryujinx.Graphics.Shader.Translation;

namespace Ryujinx.Graphics.Shader
{
    struct ShaderConfig
    {
        public ShaderStage Stage { get; }

        public ShaderCapabilities Capabilities { get; }

        public TranslationFlags Flags { get; }

        public int MaxOutputVertices { get; }

        public OutputTopology OutputTopology { get; }

        public ShaderConfig(
            ShaderStage        stage,
            ShaderCapabilities capabilities,
            TranslationFlags   flags,
            int                maxOutputVertices,
            OutputTopology     outputTopology)
        {
            Stage             = stage;
            Capabilities      = capabilities;
            Flags             = flags;
            MaxOutputVertices = maxOutputVertices;
            OutputTopology    = outputTopology;
        }
    }
}