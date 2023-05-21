using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Shader info structure builder.
    /// </summary>
    class ShaderInfoBuilder
    {
        private const int TotalSets = 4;

        private const int UniformSetIndex = 0;
        private const int StorageSetIndex = 1;
        private const int TextureSetIndex = 2;
        private const int ImageSetIndex = 3;

        private const ResourceStages SupportBufferStags =
            ResourceStages.Compute |
            ResourceStages.Vertex |
            ResourceStages.Fragment;

        private readonly GpuContext _context;

        private int _fragmentOutputMap;

        private readonly List<ResourceDescriptor>[] _resourceDescriptors;
        private readonly List<ResourceUsage>[] _resourceUsages;

        /// <summary>
        /// Creates a new shader info builder.
        /// </summary>
        /// <param name="context">GPU context that owns the shaders that will be added to the builder</param>
        public ShaderInfoBuilder(GpuContext context)
        {
            _context = context;

            _fragmentOutputMap = -1;

            _resourceDescriptors = new List<ResourceDescriptor>[TotalSets];
            _resourceUsages = new List<ResourceUsage>[TotalSets];

            for (int index = 0; index < TotalSets; index++)
            {
                _resourceDescriptors[index] = new();
                _resourceUsages[index] = new();
            }

            AddDescriptor(SupportBufferStags, ResourceType.UniformBuffer, UniformSetIndex, 0, 1);
        }

        /// <summary>
        /// Adds information from a given shader stage.
        /// </summary>
        /// <param name="info">Shader stage information</param>
        public void AddStageInfo(ShaderProgramInfo info)
        {
            if (info.Stage == ShaderStage.Fragment)
            {
                _fragmentOutputMap = info.FragmentOutputMap;
            }

            int stageIndex = GpuAccessorBase.GetStageIndex(info.Stage switch
            {
                ShaderStage.TessellationControl => 1,
                ShaderStage.TessellationEvaluation => 2,
                ShaderStage.Geometry => 3,
                ShaderStage.Fragment => 4,
                _ => 0
            });

            ResourceStages stages = info.Stage switch
            {
                ShaderStage.Compute => ResourceStages.Compute,
                ShaderStage.Vertex => ResourceStages.Vertex,
                ShaderStage.TessellationControl => ResourceStages.TessellationControl,
                ShaderStage.TessellationEvaluation => ResourceStages.TessellationEvaluation,
                ShaderStage.Geometry => ResourceStages.Geometry,
                ShaderStage.Fragment => ResourceStages.Fragment,
                _ => ResourceStages.None
            };

            int uniformsPerStage = (int)_context.Capabilities.MaximumUniformBuffersPerStage;
            int storagesPerStage = (int)_context.Capabilities.MaximumStorageBuffersPerStage;
            int texturesPerStage = (int)_context.Capabilities.MaximumTexturesPerStage;
            int imagesPerStage = (int)_context.Capabilities.MaximumImagesPerStage;

            int uniformBinding = 1 + stageIndex * uniformsPerStage;
            int storageBinding = stageIndex * storagesPerStage;
            int textureBinding = stageIndex * texturesPerStage * 2;
            int imageBinding = stageIndex * imagesPerStage * 2;

            AddDescriptor(stages, ResourceType.UniformBuffer, UniformSetIndex, uniformBinding, uniformsPerStage);
            AddArrayDescriptor(stages, ResourceType.StorageBuffer, StorageSetIndex, storageBinding, storagesPerStage);
            AddDualDescriptor(stages, ResourceType.TextureAndSampler, ResourceType.BufferTexture, TextureSetIndex, textureBinding, texturesPerStage);
            AddDualDescriptor(stages, ResourceType.Image, ResourceType.BufferImage, ImageSetIndex, imageBinding, imagesPerStage);

            AddUsage(info.CBuffers, stages, UniformSetIndex, isStorage: false);
            AddUsage(info.SBuffers, stages, StorageSetIndex, isStorage: true);
            AddUsage(info.Textures, stages, TextureSetIndex, isImage: false);
            AddUsage(info.Images, stages, ImageSetIndex, isImage: true);
        }

        /// <summary>
        /// Adds a resource descriptor to the list of descriptors.
        /// </summary>
        /// <param name="stages">Shader stages where the resource is used</param>
        /// <param name="type">Type of the resource</param>
        /// <param name="setIndex">Descriptor set number where the resource will be bound</param>
        /// <param name="binding">Binding number where the resource will be bound</param>
        /// <param name="count">Number of resources bound at the binding location</param>
        private void AddDescriptor(ResourceStages stages, ResourceType type, int setIndex, int binding, int count)
        {
            for (int index = 0; index < count; index++)
            {
                _resourceDescriptors[setIndex].Add(new ResourceDescriptor(binding + index, 1, type, stages));
            }
        }

        /// <summary>
        /// Adds two interleaved groups of resources to the list of descriptors.
        /// </summary>
        /// <param name="stages">Shader stages where the resource is used</param>
        /// <param name="type">Type of the first interleaved resource</param>
        /// <param name="type2">Type of the second interleaved resource</param>
        /// <param name="setIndex">Descriptor set number where the resource will be bound</param>
        /// <param name="binding">Binding number where the resource will be bound</param>
        /// <param name="count">Number of resources bound at the binding location</param>
        private void AddDualDescriptor(ResourceStages stages, ResourceType type, ResourceType type2, int setIndex, int binding, int count)
        {
            AddDescriptor(stages, type, setIndex, binding, count);
            AddDescriptor(stages, type2, setIndex, binding + count, count);
        }

        /// <summary>
        /// Adds an array resource to the list of descriptors.
        /// </summary>
        /// <param name="stages">Shader stages where the resource is used</param>
        /// <param name="type">Type of the resource</param>
        /// <param name="setIndex">Descriptor set number where the resource will be bound</param>
        /// <param name="binding">Binding number where the resource will be bound</param>
        /// <param name="count">Number of resources bound at the binding location</param>
        private void AddArrayDescriptor(ResourceStages stages, ResourceType type, int setIndex, int binding, int count)
        {
            _resourceDescriptors[setIndex].Add(new ResourceDescriptor(binding, count, type, stages));
        }

        /// <summary>
        /// Adds buffer usage information to the list of usages.
        /// </summary>
        /// <param name="buffers">Buffers to be added</param>
        /// <param name="stages">Stages where the buffers are used</param>
        /// <param name="setIndex">Descriptor set index where the buffers will be bound</param>
        /// <param name="isStorage">True for storage buffers, false for uniform buffers</param>
        private void AddUsage(IEnumerable<BufferDescriptor> buffers, ResourceStages stages, int setIndex, bool isStorage)
        {
            foreach (BufferDescriptor buffer in buffers)
            {
                _resourceUsages[setIndex].Add(new ResourceUsage(
                    buffer.Binding,
                    isStorage ? ResourceType.StorageBuffer : ResourceType.UniformBuffer,
                    stages,
                    buffer.Flags.HasFlag(BufferUsageFlags.Write) ? ResourceAccess.ReadWrite : ResourceAccess.Read));
            }
        }

        /// <summary>
        /// Adds texture usage information to the list of usages.
        /// </summary>
        /// <param name="textures">Textures to be added</param>
        /// <param name="stages">Stages where the textures are used</param>
        /// <param name="setIndex">Descriptor set index where the textures will be bound</param>
        /// <param name="isImage">True for images, false for textures</param>
        private void AddUsage(IEnumerable<TextureDescriptor> textures, ResourceStages stages, int setIndex, bool isImage)
        {
            foreach (TextureDescriptor texture in textures)
            {
                bool isBuffer = (texture.Type & SamplerType.Mask) == SamplerType.TextureBuffer;

                ResourceType type = isBuffer
                    ? (isImage ? ResourceType.BufferImage : ResourceType.BufferTexture)
                    : (isImage ? ResourceType.Image : ResourceType.TextureAndSampler);

                _resourceUsages[setIndex].Add(new ResourceUsage(
                    texture.Binding,
                    type,
                    stages,
                    texture.Flags.HasFlag(TextureUsageFlags.ImageStore) ? ResourceAccess.ReadWrite : ResourceAccess.Read));
            }
        }

        /// <summary>
        /// Creates a new shader information structure from the added information.
        /// </summary>
        /// <param name="pipeline">Optional pipeline state for background shader compilation</param>
        /// <param name="fromCache">Indicates if the shader comes from a disk cache</param>
        /// <returns>Shader information</returns>
        public ShaderInfo Build(ProgramPipelineState? pipeline, bool fromCache = false)
        {
            var descriptors = new ResourceDescriptorCollection[TotalSets];
            var usages = new ResourceUsageCollection[TotalSets];

            for (int index = 0; index < TotalSets; index++)
            {
                descriptors[index] = new ResourceDescriptorCollection(_resourceDescriptors[index].ToArray().AsReadOnly());
                usages[index] = new ResourceUsageCollection(_resourceUsages[index].ToArray().AsReadOnly());
            }

            ResourceLayout resourceLayout = new ResourceLayout(descriptors.AsReadOnly(), usages.AsReadOnly());

            if (pipeline.HasValue)
            {
                return new ShaderInfo(_fragmentOutputMap, resourceLayout, pipeline.Value, fromCache);
            }
            else
            {
                return new ShaderInfo(_fragmentOutputMap, resourceLayout, fromCache);
            }
        }

        /// <summary>
        /// Builds shader information for shaders from the disk cache.
        /// </summary>
        /// <param name="context">GPU context that owns the shaders</param>
        /// <param name="programs">Shaders from the disk cache</param>
        /// <param name="pipeline">Optional pipeline for background compilation</param>
        /// <returns>Shader information</returns>
        public static ShaderInfo BuildForCache(GpuContext context, IEnumerable<CachedShaderStage> programs, ProgramPipelineState? pipeline)
        {
            ShaderInfoBuilder builder = new ShaderInfoBuilder(context);

            foreach (CachedShaderStage program in programs)
            {
                if (program?.Info != null)
                {
                    builder.AddStageInfo(program.Info);
                }
            }

            return builder.Build(pipeline, fromCache: true);
        }

        /// <summary>
        /// Builds shader information for a compute shader.
        /// </summary>
        /// <param name="context">GPU context that owns the shader</param>
        /// <param name="info">Compute shader information</param>
        /// <param name="fromCache">True if the compute shader comes from a disk cache, false otherwise</param>
        /// <returns>Shader information</returns>
        public static ShaderInfo BuildForCompute(GpuContext context, ShaderProgramInfo info, bool fromCache = false)
        {
            ShaderInfoBuilder builder = new ShaderInfoBuilder(context);

            builder.AddStageInfo(info);

            return builder.Build(null, fromCache);
        }
    }
}