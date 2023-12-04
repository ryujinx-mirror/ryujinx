using Ryujinx.Graphics.GAL;
using Silk.NET.Vulkan;
using System;
using System.Linq;
using VkFormat = Silk.NET.Vulkan.Format;

namespace Ryujinx.Graphics.Vulkan
{
    class FramebufferParams
    {
        private readonly Device _device;
        private readonly Auto<DisposableImageView>[] _attachments;
        private readonly TextureView[] _colors;
        private readonly TextureView _depthStencil;
        private readonly uint _validColorAttachments;

        public uint Width { get; }
        public uint Height { get; }
        public uint Layers { get; }

        public uint[] AttachmentSamples { get; }
        public VkFormat[] AttachmentFormats { get; }
        public int[] AttachmentIndices { get; }
        public uint AttachmentIntegerFormatMask { get; }

        public int AttachmentsCount { get; }
        public int MaxColorAttachmentIndex => AttachmentIndices.Length > 0 ? AttachmentIndices[^1] : -1;
        public bool HasDepthStencil { get; }
        public int ColorAttachmentsCount => AttachmentsCount - (HasDepthStencil ? 1 : 0);

        public FramebufferParams(
            Device device,
            Auto<DisposableImageView> view,
            uint width,
            uint height,
            uint samples,
            bool isDepthStencil,
            VkFormat format)
        {
            _device = device;
            _attachments = new[] { view };
            _validColorAttachments = isDepthStencil ? 0u : 1u;

            Width = width;
            Height = height;
            Layers = 1;

            AttachmentSamples = new[] { samples };
            AttachmentFormats = new[] { format };
            AttachmentIndices = isDepthStencil ? Array.Empty<int>() : new[] { 0 };

            AttachmentsCount = 1;

            HasDepthStencil = isDepthStencil;
        }

        public FramebufferParams(Device device, ITexture[] colors, ITexture depthStencil)
        {
            _device = device;

            int colorsCount = colors.Count(IsValidTextureView);

            int count = colorsCount + (IsValidTextureView(depthStencil) ? 1 : 0);

            _attachments = new Auto<DisposableImageView>[count];
            _colors = new TextureView[colorsCount];

            AttachmentSamples = new uint[count];
            AttachmentFormats = new VkFormat[count];
            AttachmentIndices = new int[colorsCount];

            uint width = uint.MaxValue;
            uint height = uint.MaxValue;
            uint layers = uint.MaxValue;

            int index = 0;
            int bindIndex = 0;
            uint attachmentIntegerFormatMask = 0;

            foreach (ITexture color in colors)
            {
                if (IsValidTextureView(color))
                {
                    var texture = (TextureView)color;

                    _attachments[index] = texture.GetImageViewForAttachment();
                    _colors[index] = texture;
                    _validColorAttachments |= 1u << bindIndex;

                    AttachmentSamples[index] = (uint)texture.Info.Samples;
                    AttachmentFormats[index] = texture.VkFormat;
                    AttachmentIndices[index] = bindIndex;

                    if (texture.Info.Format.IsInteger())
                    {
                        attachmentIntegerFormatMask |= 1u << bindIndex;
                    }

                    width = Math.Min(width, (uint)texture.Width);
                    height = Math.Min(height, (uint)texture.Height);
                    layers = Math.Min(layers, (uint)texture.Layers);

                    if (++index >= colorsCount)
                    {
                        break;
                    }
                }

                bindIndex++;
            }

            AttachmentIntegerFormatMask = attachmentIntegerFormatMask;

            if (depthStencil is TextureView dsTexture && dsTexture.Valid)
            {
                _attachments[count - 1] = dsTexture.GetImageViewForAttachment();
                _depthStencil = dsTexture;

                AttachmentSamples[count - 1] = (uint)dsTexture.Info.Samples;
                AttachmentFormats[count - 1] = dsTexture.VkFormat;

                width = Math.Min(width, (uint)dsTexture.Width);
                height = Math.Min(height, (uint)dsTexture.Height);
                layers = Math.Min(layers, (uint)dsTexture.Layers);

                HasDepthStencil = true;
            }

            if (count == 0)
            {
                width = height = layers = 1;
            }

            Width = width;
            Height = height;
            Layers = layers;

            AttachmentsCount = count;
        }

        public Auto<DisposableImageView> GetAttachment(int index)
        {
            if ((uint)index >= _attachments.Length)
            {
                return null;
            }

            return _attachments[index];
        }

        public Auto<DisposableImageView> GetDepthStencilAttachment()
        {
            if (!HasDepthStencil)
            {
                return null;
            }

            return _attachments[AttachmentsCount - 1];
        }

        public ComponentType GetAttachmentComponentType(int index)
        {
            if (_colors != null && (uint)index < _colors.Length)
            {
                var format = _colors[index].Info.Format;

                if (format.IsSint())
                {
                    return ComponentType.SignedInteger;
                }

                if (format.IsUint())
                {
                    return ComponentType.UnsignedInteger;
                }
            }

            return ComponentType.Float;
        }

        public ImageAspectFlags GetDepthStencilAspectFlags()
        {
            if (_depthStencil == null)
            {
                return ImageAspectFlags.None;
            }

            return _depthStencil.Info.Format.ConvertAspectFlags();
        }

        public bool IsValidColorAttachment(int bindIndex)
        {
            return (uint)bindIndex < Constants.MaxRenderTargets && (_validColorAttachments & (1u << bindIndex)) != 0;
        }

        private static bool IsValidTextureView(ITexture texture)
        {
            return texture is TextureView view && view.Valid;
        }

        public ClearRect GetClearRect(Rectangle<int> scissor, int layer, int layerCount)
        {
            int x = scissor.X;
            int y = scissor.Y;
            int width = Math.Min((int)Width - scissor.X, scissor.Width);
            int height = Math.Min((int)Height - scissor.Y, scissor.Height);

            return new ClearRect(new Rect2D(new Offset2D(x, y), new Extent2D((uint)width, (uint)height)), (uint)layer, (uint)layerCount);
        }

        public unsafe Auto<DisposableFramebuffer> Create(Vk api, CommandBufferScoped cbs, Auto<DisposableRenderPass> renderPass)
        {
            ImageView* attachments = stackalloc ImageView[_attachments.Length];

            for (int i = 0; i < _attachments.Length; i++)
            {
                attachments[i] = _attachments[i].Get(cbs).Value;
            }

            var framebufferCreateInfo = new FramebufferCreateInfo
            {
                SType = StructureType.FramebufferCreateInfo,
                RenderPass = renderPass.Get(cbs).Value,
                AttachmentCount = (uint)_attachments.Length,
                PAttachments = attachments,
                Width = Width,
                Height = Height,
                Layers = Layers,
            };

            api.CreateFramebuffer(_device, framebufferCreateInfo, null, out var framebuffer).ThrowOnError();
            return new Auto<DisposableFramebuffer>(new DisposableFramebuffer(api, _device, framebuffer), null, _attachments);
        }

        public void UpdateModifications()
        {
            if (_colors != null)
            {
                for (int index = 0; index < _colors.Length; index++)
                {
                    _colors[index].Storage.SetModification(
                        AccessFlags.ColorAttachmentWriteBit,
                        PipelineStageFlags.ColorAttachmentOutputBit);
                }
            }

            _depthStencil?.Storage.SetModification(
                AccessFlags.DepthStencilAttachmentWriteBit,
                PipelineStageFlags.LateFragmentTestsBit);
        }

        public void InsertClearBarrier(CommandBufferScoped cbs, int index)
        {
            if (_colors != null)
            {
                int realIndex = Array.IndexOf(AttachmentIndices, index);

                if (realIndex != -1)
                {
                    _colors[realIndex].Storage?.InsertReadToWriteBarrier(
                        cbs,
                        AccessFlags.ColorAttachmentWriteBit,
                        PipelineStageFlags.ColorAttachmentOutputBit,
                        insideRenderPass: true);
                }
            }
        }

        public void InsertClearBarrierDS(CommandBufferScoped cbs)
        {
            _depthStencil?.Storage?.InsertReadToWriteBarrier(
                cbs,
                AccessFlags.DepthStencilAttachmentWriteBit,
                PipelineStageFlags.LateFragmentTestsBit,
                insideRenderPass: true);
        }
    }
}
