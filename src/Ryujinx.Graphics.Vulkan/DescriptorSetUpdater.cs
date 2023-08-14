using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Silk.NET.Vulkan;
using System;
using System.Runtime.CompilerServices;
using Buffer = Silk.NET.Vulkan.Buffer;
using CompareOp = Ryujinx.Graphics.GAL.CompareOp;
using Format = Ryujinx.Graphics.GAL.Format;
using SamplerCreateInfo = Ryujinx.Graphics.GAL.SamplerCreateInfo;

namespace Ryujinx.Graphics.Vulkan
{
    class DescriptorSetUpdater
    {
        private readonly VulkanRenderer _gd;
        private readonly PipelineBase _pipeline;

        private ShaderCollection _program;

        private readonly Auto<DisposableBuffer>[] _uniformBufferRefs;
        private readonly Auto<DisposableBuffer>[] _storageBufferRefs;
        private readonly Auto<DisposableImageView>[] _textureRefs;
        private readonly Auto<DisposableSampler>[] _samplerRefs;
        private readonly Auto<DisposableImageView>[] _imageRefs;
        private readonly TextureBuffer[] _bufferTextureRefs;
        private readonly TextureBuffer[] _bufferImageRefs;
        private readonly Format[] _bufferImageFormats;

        private readonly DescriptorBufferInfo[] _uniformBuffers;
        private readonly DescriptorBufferInfo[] _storageBuffers;
        private readonly DescriptorImageInfo[] _textures;
        private readonly DescriptorImageInfo[] _images;
        private readonly BufferView[] _bufferTextures;
        private readonly BufferView[] _bufferImages;

        private readonly bool[] _uniformSet;
        private readonly bool[] _storageSet;

        [Flags]
        private enum DirtyFlags
        {
            None = 0,
            Uniform = 1 << 0,
            Storage = 1 << 1,
            Texture = 1 << 2,
            Image = 1 << 3,
            All = Uniform | Storage | Texture | Image,
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
            _bufferImageFormats = new Format[Constants.MaxImageBindings * 2];

            _uniformBuffers = new DescriptorBufferInfo[Constants.MaxUniformBufferBindings];
            _storageBuffers = new DescriptorBufferInfo[Constants.MaxStorageBufferBindings];
            _textures = new DescriptorImageInfo[Constants.MaxTexturesPerStage];
            _images = new DescriptorImageInfo[Constants.MaxImagesPerStage];
            _bufferTextures = new BufferView[Constants.MaxTexturesPerStage];
            _bufferImages = new BufferView[Constants.MaxImagesPerStage];

            var initialImageInfo = new DescriptorImageInfo
            {
                ImageLayout = ImageLayout.General,
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
                _dummyBuffer = gd.BufferManager.Create(gd, 0x10000, forConditionalRendering: false, baseType: BufferAllocationType.DeviceLocal);
            }

            _dummyTexture = gd.CreateTextureView(new TextureCreateInfo(
                1,
                1,
                1,
                1,
                1,
                1,
                1,
                4,
                Format.R8G8B8A8Unorm,
                DepthStencilMode.Depth,
                Target.Texture2D,
                SwizzleComponent.Red,
                SwizzleComponent.Green,
                SwizzleComponent.Blue,
                SwizzleComponent.Alpha));

            _dummySampler = (SamplerHolder)gd.CreateSampler(new SamplerCreateInfo(
                MinFilter.Nearest,
                MagFilter.Nearest,
                false,
                AddressMode.Repeat,
                AddressMode.Repeat,
                AddressMode.Repeat,
                CompareMode.None,
                CompareOp.Always,
                new ColorF(0, 0, 0, 0),
                0,
                0,
                0,
                1f));
        }

        public void Initialize()
        {
            Span<byte> dummyTextureData = stackalloc byte[4];
            _dummyTexture.SetData(dummyTextureData);
        }

        public void SetProgram(ShaderCollection program)
        {
            _program = program;
            _dirty = DirtyFlags.All;
        }

        public void SetImage(int binding, ITexture image, Format imageFormat)
        {
            if (image is TextureBuffer imageBuffer)
            {
                _bufferImageRefs[binding] = imageBuffer;
                _bufferImageFormats[binding] = imageFormat;
            }
            else if (image is TextureView view)
            {
                _imageRefs[binding] = view.GetView(imageFormat).GetIdentityImageView();
            }
            else
            {
                _imageRefs[binding] = null;
                _bufferImageRefs[binding] = null;
                _bufferImageFormats[binding] = default;
            }

            SignalDirty(DirtyFlags.Image);
        }

        public void SetImage(int binding, Auto<DisposableImageView> image)
        {
            _imageRefs[binding] = image;

            SignalDirty(DirtyFlags.Image);
        }

        public void SetStorageBuffers(CommandBuffer commandBuffer, ReadOnlySpan<BufferAssignment> buffers)
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                var assignment = buffers[i];
                var buffer = assignment.Range;
                int index = assignment.Binding;

                Auto<DisposableBuffer> vkBuffer = _gd.BufferManager.GetBuffer(commandBuffer, buffer.Handle, false, isSSBO: true);
                ref Auto<DisposableBuffer> currentVkBuffer = ref _storageBufferRefs[index];

                DescriptorBufferInfo info = new()
                {
                    Offset = (ulong)buffer.Offset,
                    Range = (ulong)buffer.Size,
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

                DescriptorBufferInfo info = new()
                {
                    Offset = 0,
                    Range = Vk.WholeSize,
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

        public void SetTextureAndSampler(
            CommandBufferScoped cbs,
            ShaderStage stage,
            int binding,
            ITexture texture,
            ISampler sampler)
        {
            if (texture is TextureBuffer textureBuffer)
            {
                _bufferTextureRefs[binding] = textureBuffer;
            }
            else if (texture is TextureView view)
            {
                view.Storage.InsertWriteToReadBarrier(cbs, AccessFlags.ShaderReadBit, stage.ConvertToPipelineStageFlags());

                _textureRefs[binding] = view.GetImageView();
                _samplerRefs[binding] = ((SamplerHolder)sampler)?.GetSampler();
            }
            else
            {
                _textureRefs[binding] = null;
                _samplerRefs[binding] = null;
                _bufferTextureRefs[binding] = null;
            }

            SignalDirty(DirtyFlags.Texture);
        }

        public void SetTextureAndSamplerIdentitySwizzle(
            CommandBufferScoped cbs,
            ShaderStage stage,
            int binding,
            ITexture texture,
            ISampler sampler)
        {
            if (texture is TextureView view)
            {
                view.Storage.InsertWriteToReadBarrier(cbs, AccessFlags.ShaderReadBit, stage.ConvertToPipelineStageFlags());

                _textureRefs[binding] = view.GetIdentityImageView();
                _samplerRefs[binding] = ((SamplerHolder)sampler)?.GetSampler();

                SignalDirty(DirtyFlags.Texture);
            }
            else
            {
                SetTextureAndSampler(cbs, stage, binding, texture, sampler);
            }
        }

        public void SetUniformBuffers(CommandBuffer commandBuffer, ReadOnlySpan<BufferAssignment> buffers)
        {
            for (int i = 0; i < buffers.Length; i++)
            {
                var assignment = buffers[i];
                var buffer = assignment.Range;
                int index = assignment.Binding;

                Auto<DisposableBuffer> vkBuffer = _gd.BufferManager.GetBuffer(commandBuffer, buffer.Handle, false);
                ref Auto<DisposableBuffer> currentVkBuffer = ref _uniformBufferRefs[index];

                DescriptorBufferInfo info = new()
                {
                    Offset = (ulong)buffer.Offset,
                    Range = (ulong)buffer.Size,
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
            var bindingSegments = program.BindingSegments[setIndex];

            if (bindingSegments.Length == 0)
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
            }

            foreach (ResourceBindingSegment segment in bindingSegments)
            {
                int binding = segment.Binding;
                int count = segment.Count;

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
                    dsc.UpdateBuffers(0, binding, storageBuffers.Slice(binding, count), DescriptorType.StorageBuffer);
                }
                else if (setIndex == PipelineBase.TextureSetIndex)
                {
                    if (segment.Type != ResourceType.BufferTexture)
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

                        dsc.UpdateImages(0, binding, textures[..count], DescriptorType.CombinedImageSampler);
                    }
                    else
                    {
                        Span<BufferView> bufferTextures = _bufferTextures;

                        for (int i = 0; i < count; i++)
                        {
                            bufferTextures[i] = _bufferTextureRefs[binding + i]?.GetBufferView(cbs) ?? default;
                        }

                        dsc.UpdateBufferImages(0, binding, bufferTextures[..count], DescriptorType.UniformTexelBuffer);
                    }
                }
                else if (setIndex == PipelineBase.ImageSetIndex)
                {
                    if (segment.Type != ResourceType.BufferImage)
                    {
                        Span<DescriptorImageInfo> images = _images;

                        for (int i = 0; i < count; i++)
                        {
                            images[i].ImageView = _imageRefs[binding + i]?.Get(cbs).Value ?? default;
                        }

                        dsc.UpdateImages(0, binding, images[..count], DescriptorType.StorageImage);
                    }
                    else
                    {
                        Span<BufferView> bufferImages = _bufferImages;

                        for (int i = 0; i < count; i++)
                        {
                            bufferImages[i] = _bufferImageRefs[binding + i]?.GetBufferView(cbs, _bufferImageFormats[binding + i]) ?? default;
                        }

                        dsc.UpdateBufferImages(0, binding, bufferImages[..count], DescriptorType.StorageTexelBuffer);
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
                    PBufferInfo = pBufferInfo,
                };

                _gd.PushDescriptorApi.CmdPushDescriptorSet(cbs.CommandBuffer, pbp, _program.PipelineLayout, 0, 1, &writeDescriptorSet);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateAndBindUniformBufferPd(CommandBufferScoped cbs, PipelineBindPoint pbp)
        {
            var bindingSegments = _program.BindingSegments[PipelineBase.UniformSetIndex];
            var dummyBuffer = _dummyBuffer?.GetBuffer();

            foreach (ResourceBindingSegment segment in bindingSegments)
            {
                int binding = segment.Binding;
                int count = segment.Count;

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

        private void Initialize(CommandBufferScoped cbs, int setIndex, DescriptorSetCollection dsc)
        {
            // We don't support clearing texture descriptors currently.
            if (setIndex != PipelineBase.UniformSetIndex && setIndex != PipelineBase.StorageSetIndex)
            {
                return;
            }

            var dummyBuffer = _dummyBuffer?.GetBuffer().Get(cbs).Value ?? default;

            foreach (ResourceBindingSegment segment in _program.ClearSegments[setIndex])
            {
                dsc.InitializeBuffers(0, segment.Binding, segment.Count, segment.Type.Convert(), dummyBuffer);
            }
        }

        public void SignalCommandBufferChange()
        {
            _dirty = DirtyFlags.All;

            Array.Clear(_uniformSet);
            Array.Clear(_storageSet);
        }

        private static void SwapBuffer(Auto<DisposableBuffer>[] list, Auto<DisposableBuffer> from, Auto<DisposableBuffer> to)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == from)
                {
                    list[i] = to;
                }
            }
        }

        public void SwapBuffer(Auto<DisposableBuffer> from, Auto<DisposableBuffer> to)
        {
            SwapBuffer(_uniformBufferRefs, from, to);
            SwapBuffer(_storageBufferRefs, from, to);
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
