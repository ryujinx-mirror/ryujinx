using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Silk.NET.Vulkan;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.Vulkan
{
    class DescriptorSetUpdater
    {
        private readonly VulkanRenderer _gd;
        private readonly PipelineBase _pipeline;

        private ShaderCollection _program;

        private Auto<DisposableBuffer>[] _uniformBufferRefs;
        private Auto<DisposableBuffer>[] _storageBufferRefs;
        private Auto<DisposableImageView>[] _textureRefs;
        private Auto<DisposableSampler>[] _samplerRefs;
        private Auto<DisposableImageView>[] _imageRefs;
        private TextureBuffer[] _bufferTextureRefs;
        private TextureBuffer[] _bufferImageRefs;
        private GAL.Format[] _bufferImageFormats;

        private DescriptorBufferInfo[] _uniformBuffers;
        private DescriptorBufferInfo[] _storageBuffers;
        private DescriptorImageInfo[] _textures;
        private DescriptorImageInfo[] _images;
        private BufferView[] _bufferTextures;
        private BufferView[] _bufferImages;

        private bool[] _uniformSet;
        private bool[] _storageSet;
        private Silk.NET.Vulkan.Buffer _cachedSupportBuffer;

        [Flags]
        private enum DirtyFlags
        {
            None = 0,
            Uniform = 1 << 0,
            Storage = 1 << 1,
            Texture = 1 << 2,
            Image = 1 << 3,
            All = Uniform | Storage | Texture | Image
        }

        private DirtyFlags _dirty;

        private readonly BufferHolder _dummyBuffer;
        private readonly TextureView _dummyTexture;
        private readonly SamplerHolder _dummySampler;

        public DescriptorSetUpdater(VulkanRenderer gd, PipelineBase pipeline)
        {
            _gd = gd;
            _pipeline = pipeline;

            // Some of the bindings counts needs to be multiplied by 2 because we have buffer and
            // regular textures/images interleaved on the same descriptor set.

            _uniformBufferRefs = new Auto<DisposableBuffer>[Constants.MaxUniformBufferBindings];
            _storageBufferRefs = new Auto<DisposableBuffer>[Constants.MaxStorageBufferBindings];
            _textureRefs = new Auto<DisposableImageView>[Constants.MaxTextureBindings * 2];
            _samplerRefs = new Auto<DisposableSampler>[Constants.MaxTextureBindings * 2];
            _imageRefs = new Auto<DisposableImageView>[Constants.MaxImageBindings * 2];
            _bufferTextureRefs = new TextureBuffer[Constants.MaxTextureBindings * 2];
            _bufferImageRefs = new TextureBuffer[Constants.MaxImageBindings * 2];
            _bufferImageFormats = new GAL.Format[Constants.MaxImageBindings * 2];

            _uniformBuffers = new DescriptorBufferInfo[Constants.MaxUniformBufferBindings];
            _storageBuffers = new DescriptorBufferInfo[Constants.MaxStorageBufferBindings];
            _textures = new DescriptorImageInfo[Constants.MaxTexturesPerStage];
            _images = new DescriptorImageInfo[Constants.MaxImagesPerStage];
            _bufferTextures = new BufferView[Constants.MaxTexturesPerStage];
            _bufferImages = new BufferView[Constants.MaxImagesPerStage];

            var initialImageInfo = new DescriptorImageInfo()
            {
                ImageLayout = ImageLayout.General
            };

            _textures.AsSpan().Fill(initialImageInfo);
            _images.AsSpan().Fill(initialImageInfo);

            _uniformSet = new bool[Constants.MaxUniformBufferBindings];
            _storageSet = new bool[Constants.MaxStorageBufferBindings];

            if (gd.Capabilities.SupportsNullDescriptors)
            {
                // If null descriptors are supported, we can pass null as the handle.
                _dummyBuffer = null;
            }
            else
            {
                // If null descriptors are not supported, we need to pass the handle of a dummy buffer on unused bindings.
                _dummyBuffer = gd.BufferManager.Create(gd, 0x10000, forConditionalRendering: false, deviceLocal: true);
            }

            _dummyTexture = gd.CreateTextureView(new GAL.TextureCreateInfo(
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                4,
                GAL.Format.R8G8B8A8Unorm,
                DepthStencilMode.Depth,
                Target.Texture2D,
                SwizzleComponent.Red,
                SwizzleComponent.Green,
                SwizzleComponent.Blue,
                SwizzleComponent.Alpha), 1f);

            _dummySampler = (SamplerHolder)gd.CreateSampler(new GAL.SamplerCreateInfo(
                MinFilter.Nearest,
                MagFilter.Nearest,
                false,
                AddressMode.Repeat,
                AddressMode.Repeat,
                AddressMode.Repeat,
                CompareMode.None,
                GAL.CompareOp.Always,
                new ColorF(0, 0, 0, 0),
                0,
                0,
                0,
                1f));
        }

        public void SetProgram(ShaderCollection program)
        {
            _program = program;
            _dirty = DirtyFlags.All;
        }

        public void SetImage(int binding, ITexture image, GAL.Format imageFormat)
        {
            if (image == null)
            {
                return;
            }

            if (image is TextureBuffer imageBuffer)
            {
                _bufferImageRefs[binding] = imageBuffer;
                _bufferImageFormats[binding] = imageFormat;
            }
            else if (image is TextureView view)
            {
                _imageRefs[binding] = view.GetView(imageFormat).GetIdentityImageView();
            }

            SignalDirty(DirtyFlags.Image);
        }

        public void SetStorageBuffers(CommandBuffer commandBuffer, int first, ReadOnlySpan<BufferRange> buffers)
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                var buffer = buffers[i];
                int index = first + i;

                Auto<DisposableBuffer> vkBuffer = _gd.BufferManager.GetBuffer(commandBuffer, buffer.Handle, false);
                ref Auto<DisposableBuffer> currentVkBuffer = ref _storageBufferRefs[index];

                DescriptorBufferInfo info = new DescriptorBufferInfo()
                {
                    Offset = (ulong)buffer.Offset,
                    Range = (ulong)buffer.Size
                };
                ref DescriptorBufferInfo currentInfo = ref _storageBuffers[index];

                if (vkBuffer != currentVkBuffer || currentInfo.Offset != info.Offset || currentInfo.Range != info.Range)
                {
                    _storageSet[index] = false;

                    currentInfo = info;
                    currentVkBuffer = vkBuffer;
                }
            }

            SignalDirty(DirtyFlags.Storage);
        }

        public void SetStorageBuffers(CommandBuffer commandBuffer, int first, ReadOnlySpan<Auto<DisposableBuffer>> buffers)
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                var vkBuffer = buffers[i];
                int index = first + i;

                ref Auto<DisposableBuffer> currentVkBuffer = ref _storageBufferRefs[index];

                DescriptorBufferInfo info = new DescriptorBufferInfo()
                {
                    Offset = 0,
                    Range = Vk.WholeSize
                };
                ref DescriptorBufferInfo currentInfo = ref _storageBuffers[index];

                if (vkBuffer != currentVkBuffer || currentInfo.Offset != info.Offset || currentInfo.Range != info.Range)
                {
                    _storageSet[index] = false;

                    currentInfo = info;
                    currentVkBuffer = vkBuffer;
                }
            }

            SignalDirty(DirtyFlags.Storage);
        }

        public void SetTextureAndSampler(CommandBufferScoped cbs, ShaderStage stage, int binding, ITexture texture, ISampler sampler)
        {
            if (texture == null)
            {
                return;
            }

            if (texture is TextureBuffer textureBuffer)
            {
                _bufferTextureRefs[binding] = textureBuffer;
            }
            else
            {
                TextureView view = (TextureView)texture;

                view.Storage.InsertBarrier(cbs, AccessFlags.AccessShaderReadBit, stage.ConvertToPipelineStageFlags());

                _textureRefs[binding] = view.GetImageView();
                _samplerRefs[binding] = ((SamplerHolder)sampler)?.GetSampler();
            }

            SignalDirty(DirtyFlags.Texture);
        }

        public void SetUniformBuffers(CommandBuffer commandBuffer, int first, ReadOnlySpan<BufferRange> buffers)
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                var buffer = buffers[i];
                int index = first + i;

                Auto<DisposableBuffer> vkBuffer = _gd.BufferManager.GetBuffer(commandBuffer, buffer.Handle, false);
                ref Auto<DisposableBuffer> currentVkBuffer = ref _uniformBufferRefs[index];

                DescriptorBufferInfo info = new DescriptorBufferInfo()
                {
                    Offset = (ulong)buffer.Offset,
                    Range = (ulong)buffer.Size
                };
                ref DescriptorBufferInfo currentInfo = ref _uniformBuffers[index];

                if (vkBuffer != currentVkBuffer || currentInfo.Offset != info.Offset || currentInfo.Range != info.Range)
                {
                    _uniformSet[index] = false;

                    currentInfo = info;
                    currentVkBuffer = vkBuffer;
                }
            }

            SignalDirty(DirtyFlags.Uniform);
        }

        private void SignalDirty(DirtyFlags flag)
        {
            _dirty |= flag;
        }

        public void UpdateAndBindDescriptorSets(CommandBufferScoped cbs, PipelineBindPoint pbp)
        {
            if ((_dirty & DirtyFlags.All) == 0)
            {
                return;
            }

            if (_dirty.HasFlag(DirtyFlags.Uniform))
            {
                if (_program.UsePushDescriptors)
                {
                    UpdateAndBindUniformBufferPd(cbs, pbp);
                }
                else
                {
                    UpdateAndBind(cbs, PipelineBase.UniformSetIndex, pbp);
                }
            }

            if (_dirty.HasFlag(DirtyFlags.Storage))
            {
                UpdateAndBind(cbs, PipelineBase.StorageSetIndex, pbp);
            }

            if (_dirty.HasFlag(DirtyFlags.Texture))
            {
                UpdateAndBind(cbs, PipelineBase.TextureSetIndex, pbp);
            }

            if (_dirty.HasFlag(DirtyFlags.Image))
            {
                UpdateAndBind(cbs, PipelineBase.ImageSetIndex, pbp);
            }

            _dirty = DirtyFlags.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void UpdateBuffer(
            CommandBufferScoped cbs,
            ref DescriptorBufferInfo info,
            Auto<DisposableBuffer> buffer,
            Auto<DisposableBuffer> dummyBuffer)
        {
            info.Buffer = buffer?.Get(cbs, (int)info.Offset, (int)info.Range).Value ?? default;

            // The spec requires that buffers with null handle have offset as 0 and range as VK_WHOLE_SIZE.
            if (info.Buffer.Handle == 0)
            {
                info.Buffer = dummyBuffer?.Get(cbs).Value ?? default;
                info.Offset = 0;
                info.Range = Vk.WholeSize;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateAndBind(CommandBufferScoped cbs, int setIndex, PipelineBindPoint pbp)
        {
            var program = _program;
            int stagesCount = program.Bindings[setIndex].Length;
            if (stagesCount == 0 && setIndex != PipelineBase.UniformSetIndex)
            {
                return;
            }

            var dummyBuffer = _dummyBuffer?.GetBuffer();

            var dsc = program.GetNewDescriptorSetCollection(_gd, cbs.CommandBufferIndex, setIndex, out var isNew).Get(cbs);

            if (!program.HasMinimalLayout)
            {
                if (isNew)
                {
                    Initialize(cbs, setIndex, dsc);
                }

                if (setIndex == PipelineBase.UniformSetIndex)
                {
                    Span<DescriptorBufferInfo> uniformBuffer = stackalloc DescriptorBufferInfo[1];

                    if (!_uniformSet[0])
                    {
                        _cachedSupportBuffer = _gd.BufferManager.GetBuffer(cbs.CommandBuffer, _pipeline.SupportBufferUpdater.Handle, false).Get(cbs, 0, SupportBuffer.RequiredSize).Value;
                        _uniformSet[0] = true;
                    }

                    uniformBuffer[0] = new DescriptorBufferInfo()
                    {
                        Offset = 0,
                        Range = (ulong)SupportBuffer.RequiredSize,
                        Buffer = _cachedSupportBuffer
                    };

                    dsc.UpdateBuffers(0, 0, uniformBuffer, DescriptorType.UniformBuffer);
                }
            }

            for (int stageIndex = 0; stageIndex < stagesCount; stageIndex++)
            {
                var stageBindings = program.Bindings[setIndex][stageIndex];
                int bindingsCount = stageBindings.Length;
                int count;

                for (int bindingIndex = 0; bindingIndex < bindingsCount; bindingIndex += count)
                {
                    int binding = stageBindings[bindingIndex];
                    count = 1;

                    while (bindingIndex + count < bindingsCount && stageBindings[bindingIndex + count] == binding + count)
                    {
                        count++;
                    }

                    if (setIndex == PipelineBase.UniformSetIndex)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int index = binding + i;

                            if (!_uniformSet[index])
                            {
                                UpdateBuffer(cbs, ref _uniformBuffers[index], _uniformBufferRefs[index], dummyBuffer);

                                _uniformSet[index] = true;
                            }
                        }

                        ReadOnlySpan<DescriptorBufferInfo> uniformBuffers = _uniformBuffers;
                        dsc.UpdateBuffers(0, binding, uniformBuffers.Slice(binding, count), DescriptorType.UniformBuffer);
                    }
                    else if (setIndex == PipelineBase.StorageSetIndex)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            int index = binding + i;

                            if (!_storageSet[index])
                            {
                                UpdateBuffer(cbs, ref _storageBuffers[index], _storageBufferRefs[index], dummyBuffer);

                                _storageSet[index] = true;
                            }
                        }

                        ReadOnlySpan<DescriptorBufferInfo> storageBuffers = _storageBuffers;
                        if (program.HasMinimalLayout)
                        {
                            dsc.UpdateBuffers(0, binding, storageBuffers.Slice(binding, count), DescriptorType.StorageBuffer);
                        }
                        else
                        {
                            dsc.UpdateStorageBuffers(0, binding, storageBuffers.Slice(binding, count));
                        }
                    }
                    else if (setIndex == PipelineBase.TextureSetIndex)
                    {
                        if (((uint)binding % (Constants.MaxTexturesPerStage * 2)) < Constants.MaxTexturesPerStage || program.HasMinimalLayout)
                        {
                            Span<DescriptorImageInfo> textures = _textures;

                            for (int i = 0; i < count; i++)
                            {
                                ref var texture = ref textures[i];

                                texture.ImageView = _textureRefs[binding + i]?.Get(cbs).Value ?? default;
                                texture.Sampler = _samplerRefs[binding + i]?.Get(cbs).Value ?? default;

                                if (texture.ImageView.Handle == 0)
                                {
                                    texture.ImageView = _dummyTexture.GetImageView().Get(cbs).Value;
                                }

                                if (texture.Sampler.Handle == 0)
                                {
                                    texture.Sampler = _dummySampler.GetSampler().Get(cbs).Value;
                                }
                            }

                            dsc.UpdateImages(0, binding, textures.Slice(0, count), DescriptorType.CombinedImageSampler);
                        }
                        else
                        {
                            Span<BufferView> bufferTextures = _bufferTextures;

                            for (int i = 0; i < count; i++)
                            {
                                bufferTextures[i] = _bufferTextureRefs[binding + i]?.GetBufferView(cbs) ?? default;
                            }

                            dsc.UpdateBufferImages(0, binding, bufferTextures.Slice(0, count), DescriptorType.UniformTexelBuffer);
                        }
                    }
                    else if (setIndex == PipelineBase.ImageSetIndex)
                    {
                        if (((uint)binding % (Constants.MaxImagesPerStage * 2)) < Constants.MaxImagesPerStage || program.HasMinimalLayout)
                        {
                            Span<DescriptorImageInfo> images = _images;

                            for (int i = 0; i < count; i++)
                            {
                                images[i].ImageView = _imageRefs[binding + i]?.Get(cbs).Value ?? default;
                            }

                            dsc.UpdateImages(0, binding, images.Slice(0, count), DescriptorType.StorageImage);
                        }
                        else
                        {
                            Span<BufferView> bufferImages = _bufferImages;

                            for (int i = 0; i < count; i++)
                            {
                                bufferImages[i] = _bufferImageRefs[binding + i]?.GetBufferView(cbs, _bufferImageFormats[binding + i]) ?? default;
                            }

                            dsc.UpdateBufferImages(0, binding, bufferImages.Slice(0, count), DescriptorType.StorageTexelBuffer);
                        }
                    }
                }
            }

            var sets = dsc.GetSets();

            _gd.Api.CmdBindDescriptorSets(cbs.CommandBuffer, pbp, _program.PipelineLayout, (uint)setIndex, 1, sets, 0, ReadOnlySpan<uint>.Empty);
        }

        private unsafe void UpdateBuffers(
            CommandBufferScoped cbs,
            PipelineBindPoint pbp,
            int baseBinding,
            ReadOnlySpan<DescriptorBufferInfo> bufferInfo,
            DescriptorType type)
        {
            if (bufferInfo.Length == 0)
            {
                return;
            }

            fixed (DescriptorBufferInfo* pBufferInfo = bufferInfo)
            {
                var writeDescriptorSet = new WriteDescriptorSet
                {
                    SType = StructureType.WriteDescriptorSet,
                    DstBinding = (uint)baseBinding,
                    DescriptorType = type,
                    DescriptorCount = (uint)bufferInfo.Length,
                    PBufferInfo = pBufferInfo
                };

                _gd.PushDescriptorApi.CmdPushDescriptorSet(cbs.CommandBuffer, pbp, _program.PipelineLayout, 0, 1, &writeDescriptorSet);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateAndBindUniformBufferPd(CommandBufferScoped cbs, PipelineBindPoint pbp)
        {
            var dummyBuffer = _dummyBuffer?.GetBuffer();
            int stagesCount = _program.Bindings[PipelineBase.UniformSetIndex].Length;

            if (!_uniformSet[0])
            {
                Span<DescriptorBufferInfo> uniformBuffer = stackalloc DescriptorBufferInfo[1];

                uniformBuffer[0] = new DescriptorBufferInfo()
                {
                    Offset = 0,
                    Range = (ulong)SupportBuffer.RequiredSize,
                    Buffer = _gd.BufferManager.GetBuffer(cbs.CommandBuffer, _pipeline.SupportBufferUpdater.Handle, false).Get(cbs, 0, SupportBuffer.RequiredSize).Value
                };

                _uniformSet[0] = true;

                UpdateBuffers(cbs, pbp, 0, uniformBuffer, DescriptorType.UniformBuffer);
            }

            for (int stageIndex = 0; stageIndex < stagesCount; stageIndex++)
            {
                var stageBindings = _program.Bindings[PipelineBase.UniformSetIndex][stageIndex];
                int bindingsCount = stageBindings.Length;
                int count;

                for (int bindingIndex = 0; bindingIndex < bindingsCount; bindingIndex += count)
                {
                    int binding = stageBindings[bindingIndex];
                    count = 1;

                    while (bindingIndex + count < bindingsCount && stageBindings[bindingIndex + count] == binding + count)
                    {
                        count++;
                    }

                    bool doUpdate = false;

                    for (int i = 0; i < count; i++)
                    {
                        int index = binding + i;

                        if (!_uniformSet[index])
                        {
                            UpdateBuffer(cbs, ref _uniformBuffers[index], _uniformBufferRefs[index], dummyBuffer);
                            _uniformSet[index] = true;
                            doUpdate = true;
                        }
                    }

                    if (doUpdate)
                    {
                        ReadOnlySpan<DescriptorBufferInfo> uniformBuffers = _uniformBuffers;
                        UpdateBuffers(cbs, pbp, binding, uniformBuffers.Slice(binding, count), DescriptorType.UniformBuffer);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Initialize(CommandBufferScoped cbs, int setIndex, DescriptorSetCollection dsc)
        {
            var dummyBuffer = _dummyBuffer?.GetBuffer().Get(cbs).Value ?? default;

            uint stages = _program.Stages;

            while (stages != 0)
            {
                int stage = BitOperations.TrailingZeroCount(stages);
                stages &= ~(1u << stage);

                if (setIndex == PipelineBase.UniformSetIndex)
                {
                    dsc.InitializeBuffers(
                        0,
                        1 + stage * Constants.MaxUniformBuffersPerStage,
                        Constants.MaxUniformBuffersPerStage,
                        DescriptorType.UniformBuffer,
                        dummyBuffer);
                }
                else if (setIndex == PipelineBase.StorageSetIndex)
                {
                    dsc.InitializeBuffers(
                        0,
                        stage * Constants.MaxStorageBuffersPerStage,
                        Constants.MaxStorageBuffersPerStage,
                        DescriptorType.StorageBuffer,
                        dummyBuffer);
                }
            }
        }

        public void SignalCommandBufferChange()
        {
            _dirty = DirtyFlags.All;

            Array.Clear(_uniformSet);
            Array.Clear(_storageSet);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dummyTexture.Dispose();
                _dummySampler.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
