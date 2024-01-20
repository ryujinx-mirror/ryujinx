using Ryujinx.Graphics.GAL;
using Silk.NET.Vulkan;
using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Vulkan
{
    class DescriptorSetTemplate : IDisposable
    {
        private readonly VulkanRenderer _gd;
        private readonly Device _device;

        public readonly DescriptorUpdateTemplate Template;
        public readonly int Size;

        public unsafe DescriptorSetTemplate(VulkanRenderer gd, Device device, ResourceBindingSegment[] segments, PipelineLayoutCacheEntry plce, PipelineBindPoint pbp, int setIndex)
        {
            _gd = gd;
            _device = device;

            // Create a template from the set usages. Assumes the descriptor set is updated in segment order then binding order.

            DescriptorUpdateTemplateEntry* entries = stackalloc DescriptorUpdateTemplateEntry[segments.Length];
            nuint structureOffset = 0;

            for (int seg = 0; seg < segments.Length; seg++)
            {
                ResourceBindingSegment segment = segments[seg];

                int binding = segment.Binding;
                int count = segment.Count;

                if (setIndex == PipelineBase.UniformSetIndex)
                {
                    entries[seg] = new DescriptorUpdateTemplateEntry()
                    {
                        DescriptorType = DescriptorType.UniformBuffer,
                        DstBinding = (uint)binding,
                        DescriptorCount = (uint)count,
                        Offset = structureOffset,
                        Stride = (nuint)Unsafe.SizeOf<DescriptorBufferInfo>()
                    };

                    structureOffset += (nuint)(Unsafe.SizeOf<DescriptorBufferInfo>() * count);
                }
                else if (setIndex == PipelineBase.StorageSetIndex)
                {
                    entries[seg] = new DescriptorUpdateTemplateEntry()
                    {
                        DescriptorType = DescriptorType.StorageBuffer,
                        DstBinding = (uint)binding,
                        DescriptorCount = (uint)count,
                        Offset = structureOffset,
                        Stride = (nuint)Unsafe.SizeOf<DescriptorBufferInfo>()
                    };

                    structureOffset += (nuint)(Unsafe.SizeOf<DescriptorBufferInfo>() * count);
                }
                else if (setIndex == PipelineBase.TextureSetIndex)
                {
                    if (segment.Type != ResourceType.BufferTexture)
                    {
                        entries[seg] = new DescriptorUpdateTemplateEntry()
                        {
                            DescriptorType = DescriptorType.CombinedImageSampler,
                            DstBinding = (uint)binding,
                            DescriptorCount = (uint)count,
                            Offset = structureOffset,
                            Stride = (nuint)Unsafe.SizeOf<DescriptorImageInfo>()
                        };

                        structureOffset += (nuint)(Unsafe.SizeOf<DescriptorImageInfo>() * count);
                    }
                    else
                    {
                        entries[seg] = new DescriptorUpdateTemplateEntry()
                        {
                            DescriptorType = DescriptorType.UniformTexelBuffer,
                            DstBinding = (uint)binding,
                            DescriptorCount = (uint)count,
                            Offset = structureOffset,
                            Stride = (nuint)Unsafe.SizeOf<BufferView>()
                        };

                        structureOffset += (nuint)(Unsafe.SizeOf<BufferView>() * count);
                    }
                }
                else if (setIndex == PipelineBase.ImageSetIndex)
                {
                    if (segment.Type != ResourceType.BufferImage)
                    {
                        entries[seg] = new DescriptorUpdateTemplateEntry()
                        {
                            DescriptorType = DescriptorType.StorageImage,
                            DstBinding = (uint)binding,
                            DescriptorCount = (uint)count,
                            Offset = structureOffset,
                            Stride = (nuint)Unsafe.SizeOf<DescriptorImageInfo>()
                        };

                        structureOffset += (nuint)(Unsafe.SizeOf<DescriptorImageInfo>() * count);
                    }
                    else
                    {
                        entries[seg] = new DescriptorUpdateTemplateEntry()
                        {
                            DescriptorType = DescriptorType.StorageTexelBuffer,
                            DstBinding = (uint)binding,
                            DescriptorCount = (uint)count,
                            Offset = structureOffset,
                            Stride = (nuint)Unsafe.SizeOf<BufferView>()
                        };

                        structureOffset += (nuint)(Unsafe.SizeOf<BufferView>() * count);
                    }
                }
            }

            Size = (int)structureOffset;

            var info = new DescriptorUpdateTemplateCreateInfo()
            {
                SType = StructureType.DescriptorUpdateTemplateCreateInfo,
                DescriptorUpdateEntryCount = (uint)segments.Length,
                PDescriptorUpdateEntries = entries,

                TemplateType = DescriptorUpdateTemplateType.DescriptorSet,
                DescriptorSetLayout = plce.DescriptorSetLayouts[setIndex],
                PipelineBindPoint = pbp,
                PipelineLayout = plce.PipelineLayout,
                Set = (uint)setIndex,
            };

            DescriptorUpdateTemplate result;
            gd.Api.CreateDescriptorUpdateTemplate(device, &info, null, &result).ThrowOnError();

            Template = result;
        }

        public unsafe void Dispose()
        {
            _gd.Api.DestroyDescriptorUpdateTemplate(_device, Template, null);
        }
    }
}
