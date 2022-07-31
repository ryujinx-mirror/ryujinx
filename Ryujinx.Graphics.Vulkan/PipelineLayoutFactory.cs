using Ryujinx.Graphics.GAL;
using Silk.NET.Vulkan;
using System.Collections.Generic;
using System.Numerics;

namespace Ryujinx.Graphics.Vulkan
{
    static class PipelineLayoutFactory
    {
        private const ShaderStageFlags SupportBufferStages =
            ShaderStageFlags.ShaderStageVertexBit |
            ShaderStageFlags.ShaderStageFragmentBit |
            ShaderStageFlags.ShaderStageComputeBit;

        public static unsafe DescriptorSetLayout[] Create(VulkanRenderer gd, Device device, uint stages, bool usePd, out PipelineLayout layout)
        {
            int stagesCount = BitOperations.PopCount(stages);

            int uCount = Constants.MaxUniformBuffersPerStage * stagesCount + 1;
            int tCount = Constants.MaxTexturesPerStage * 2 * stagesCount;
            int iCount = Constants.MaxImagesPerStage * 2 * stagesCount;

            DescriptorSetLayoutBinding* uLayoutBindings = stackalloc DescriptorSetLayoutBinding[uCount];
            DescriptorSetLayoutBinding* sLayoutBindings = stackalloc DescriptorSetLayoutBinding[stagesCount];
            DescriptorSetLayoutBinding* tLayoutBindings = stackalloc DescriptorSetLayoutBinding[tCount];
            DescriptorSetLayoutBinding* iLayoutBindings = stackalloc DescriptorSetLayoutBinding[iCount];

            uLayoutBindings[0] = new DescriptorSetLayoutBinding
            {
                Binding = 0,
                DescriptorType = DescriptorType.UniformBuffer,
                DescriptorCount = 1,
                StageFlags = SupportBufferStages
            };

            int iter = 0;

            while (stages != 0)
            {
                int stage = BitOperations.TrailingZeroCount(stages);
                stages &= ~(1u << stage);

                var stageFlags = stage switch
                {
                    1 => ShaderStageFlags.ShaderStageFragmentBit,
                    2 => ShaderStageFlags.ShaderStageGeometryBit,
                    3 => ShaderStageFlags.ShaderStageTessellationControlBit,
                    4 => ShaderStageFlags.ShaderStageTessellationEvaluationBit,
                    _ => ShaderStageFlags.ShaderStageVertexBit | ShaderStageFlags.ShaderStageComputeBit
                };

                void Set(DescriptorSetLayoutBinding* bindings, int maxPerStage, DescriptorType type, int start, int skip)
                {
                    int totalPerStage = maxPerStage * skip;

                    for (int i = 0; i < maxPerStage; i++)
                    {
                        bindings[start + iter * totalPerStage + i] = new DescriptorSetLayoutBinding
                        {
                            Binding = (uint)(start + stage * totalPerStage + i),
                            DescriptorType = type,
                            DescriptorCount = 1,
                            StageFlags = stageFlags
                        };
                    }
                }

                void SetStorage(DescriptorSetLayoutBinding* bindings, int maxPerStage, int start = 0)
                {
                    bindings[start + iter] = new DescriptorSetLayoutBinding
                    {
                        Binding = (uint)(start + stage * maxPerStage),
                        DescriptorType = DescriptorType.StorageBuffer,
                        DescriptorCount = (uint)maxPerStage,
                        StageFlags = stageFlags
                    };
                }

                Set(uLayoutBindings, Constants.MaxUniformBuffersPerStage, DescriptorType.UniformBuffer, 1, 1);
                SetStorage(sLayoutBindings, Constants.MaxStorageBuffersPerStage);
                Set(tLayoutBindings, Constants.MaxTexturesPerStage, DescriptorType.CombinedImageSampler, 0, 2);
                Set(tLayoutBindings, Constants.MaxTexturesPerStage, DescriptorType.UniformTexelBuffer, Constants.MaxTexturesPerStage, 2);
                Set(iLayoutBindings, Constants.MaxImagesPerStage, DescriptorType.StorageImage, 0, 2);
                Set(iLayoutBindings, Constants.MaxImagesPerStage, DescriptorType.StorageTexelBuffer, Constants.MaxImagesPerStage, 2);

                iter++;
            }

            DescriptorSetLayout[] layouts = new DescriptorSetLayout[PipelineFull.DescriptorSetLayouts];

            var uDescriptorSetLayoutCreateInfo = new DescriptorSetLayoutCreateInfo()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                PBindings = uLayoutBindings,
                BindingCount = (uint)uCount,
                Flags = usePd ? DescriptorSetLayoutCreateFlags.DescriptorSetLayoutCreatePushDescriptorBitKhr : 0
            };

            var sDescriptorSetLayoutCreateInfo = new DescriptorSetLayoutCreateInfo()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                PBindings = sLayoutBindings,
                BindingCount = (uint)stagesCount
            };

            var tDescriptorSetLayoutCreateInfo = new DescriptorSetLayoutCreateInfo()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                PBindings = tLayoutBindings,
                BindingCount = (uint)tCount
            };

            var iDescriptorSetLayoutCreateInfo = new DescriptorSetLayoutCreateInfo()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                PBindings = iLayoutBindings,
                BindingCount = (uint)iCount
            };

            gd.Api.CreateDescriptorSetLayout(device, uDescriptorSetLayoutCreateInfo, null, out layouts[PipelineFull.UniformSetIndex]).ThrowOnError();
            gd.Api.CreateDescriptorSetLayout(device, sDescriptorSetLayoutCreateInfo, null, out layouts[PipelineFull.StorageSetIndex]).ThrowOnError();
            gd.Api.CreateDescriptorSetLayout(device, tDescriptorSetLayoutCreateInfo, null, out layouts[PipelineFull.TextureSetIndex]).ThrowOnError();
            gd.Api.CreateDescriptorSetLayout(device, iDescriptorSetLayoutCreateInfo, null, out layouts[PipelineFull.ImageSetIndex]).ThrowOnError();

            fixed (DescriptorSetLayout* pLayouts = layouts)
            {
                var pipelineLayoutCreateInfo = new PipelineLayoutCreateInfo()
                {
                    SType = StructureType.PipelineLayoutCreateInfo,
                    PSetLayouts = pLayouts,
                    SetLayoutCount = PipelineFull.DescriptorSetLayouts
                };

                gd.Api.CreatePipelineLayout(device, &pipelineLayoutCreateInfo, null, out layout).ThrowOnError();
            }

            return layouts;
        }

        public static unsafe DescriptorSetLayout[] CreateMinimal(VulkanRenderer gd, Device device, ShaderSource[] shaders, out PipelineLayout layout)
        {
            int stagesCount = shaders.Length;

            int uCount = 0;
            int tCount = 0;
            int iCount = 0;

            foreach (var shader in shaders)
            {
                uCount += shader.Bindings.UniformBufferBindings.Count;
                tCount += shader.Bindings.TextureBindings.Count;
                iCount += shader.Bindings.ImageBindings.Count;
            }

            DescriptorSetLayoutBinding* uLayoutBindings = stackalloc DescriptorSetLayoutBinding[uCount];
            DescriptorSetLayoutBinding* sLayoutBindings = stackalloc DescriptorSetLayoutBinding[stagesCount];
            DescriptorSetLayoutBinding* tLayoutBindings = stackalloc DescriptorSetLayoutBinding[tCount];
            DescriptorSetLayoutBinding* iLayoutBindings = stackalloc DescriptorSetLayoutBinding[iCount];

            int uIndex = 0;
            int sIndex = 0;
            int tIndex = 0;
            int iIndex = 0;

            foreach (var shader in shaders)
            {
                var stageFlags = shader.Stage.Convert();

                void Set(DescriptorSetLayoutBinding* bindings, DescriptorType type, ref int start, IEnumerable<int> bds)
                {
                    foreach (var b in bds)
                    {
                        bindings[start++] = new DescriptorSetLayoutBinding
                        {
                            Binding = (uint)b,
                            DescriptorType = type,
                            DescriptorCount = 1,
                            StageFlags = stageFlags
                        };
                    }
                }

                void SetStorage(DescriptorSetLayoutBinding* bindings, ref int start, int count)
                {
                    bindings[start++] = new DescriptorSetLayoutBinding
                    {
                        Binding = (uint)start,
                        DescriptorType = DescriptorType.StorageBuffer,
                        DescriptorCount = (uint)count,
                        StageFlags = stageFlags
                    };
                }

                // TODO: Support buffer textures and images here.
                // This is only used for the helper shaders on the backend, and we don't use buffer textures on them
                // so far, so it's not really necessary right now.
                Set(uLayoutBindings, DescriptorType.UniformBuffer, ref uIndex, shader.Bindings.UniformBufferBindings);
                SetStorage(sLayoutBindings, ref sIndex, shader.Bindings.StorageBufferBindings.Count);
                Set(tLayoutBindings, DescriptorType.CombinedImageSampler, ref tIndex, shader.Bindings.TextureBindings);
                Set(iLayoutBindings, DescriptorType.StorageImage, ref iIndex, shader.Bindings.ImageBindings);
            }

            DescriptorSetLayout[] layouts = new DescriptorSetLayout[PipelineFull.DescriptorSetLayouts];

            var uDescriptorSetLayoutCreateInfo = new DescriptorSetLayoutCreateInfo()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                PBindings = uLayoutBindings,
                BindingCount = (uint)uCount
            };

            var sDescriptorSetLayoutCreateInfo = new DescriptorSetLayoutCreateInfo()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                PBindings = sLayoutBindings,
                BindingCount = (uint)stagesCount
            };

            var tDescriptorSetLayoutCreateInfo = new DescriptorSetLayoutCreateInfo()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                PBindings = tLayoutBindings,
                BindingCount = (uint)tCount
            };

            var iDescriptorSetLayoutCreateInfo = new DescriptorSetLayoutCreateInfo()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                PBindings = iLayoutBindings,
                BindingCount = (uint)iCount
            };

            gd.Api.CreateDescriptorSetLayout(device, uDescriptorSetLayoutCreateInfo, null, out layouts[PipelineFull.UniformSetIndex]).ThrowOnError();
            gd.Api.CreateDescriptorSetLayout(device, sDescriptorSetLayoutCreateInfo, null, out layouts[PipelineFull.StorageSetIndex]).ThrowOnError();
            gd.Api.CreateDescriptorSetLayout(device, tDescriptorSetLayoutCreateInfo, null, out layouts[PipelineFull.TextureSetIndex]).ThrowOnError();
            gd.Api.CreateDescriptorSetLayout(device, iDescriptorSetLayoutCreateInfo, null, out layouts[PipelineFull.ImageSetIndex]).ThrowOnError();

            fixed (DescriptorSetLayout* pLayouts = layouts)
            {
                var pipelineLayoutCreateInfo = new PipelineLayoutCreateInfo()
                {
                    SType = StructureType.PipelineLayoutCreateInfo,
                    PSetLayouts = pLayouts,
                    SetLayoutCount = PipelineFull.DescriptorSetLayouts
                };

                gd.Api.CreatePipelineLayout(device, &pipelineLayoutCreateInfo, null, out layout).ThrowOnError();
            }

            return layouts;
        }
    }
}
