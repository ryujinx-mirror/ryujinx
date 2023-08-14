using Ryujinx.Graphics.Shader.IntermediateRepresentation;

namespace Ryujinx.Graphics.Shader.Translation
{
    readonly ref struct TransformContext
    {
        public readonly HelperFunctionManager Hfm;
        public readonly BasicBlock[] Blocks;
        public readonly ResourceManager ResourceManager;
        public readonly IGpuAccessor GpuAccessor;
        public readonly TargetLanguage TargetLanguage;
        public readonly ShaderStage Stage;
        public readonly ref FeatureFlags UsedFeatures;

        public TransformContext(
            HelperFunctionManager hfm,
            BasicBlock[] blocks,
            ResourceManager resourceManager,
            IGpuAccessor gpuAccessor,
            TargetLanguage targetLanguage,
            ShaderStage stage,
            ref FeatureFlags usedFeatures)
        {
            Hfm = hfm;
            Blocks = blocks;
            ResourceManager = resourceManager;
            GpuAccessor = gpuAccessor;
            TargetLanguage = targetLanguage;
            Stage = stage;
            UsedFeatures = ref usedFeatures;
        }
    }
}
