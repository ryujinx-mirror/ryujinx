using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Texture;
using Ryujinx.Graphics.Gpu.Memory;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Image
{
    class TexturePool : Pool<Texture>
    {
        public LinkedListNode<TexturePool> CacheNode { get; set; }

        private int _sequenceNumber;

        public TexturePool(
            GpuContext context,
            ulong      address,
            int        maximumId) : base(context, address, maximumId) { }

        public override Texture Get(int id)
        {
            if ((uint)id >= Items.Length)
            {
                return null;
            }

            if (_sequenceNumber != Context.SequenceNumber)
            {
                _sequenceNumber = Context.SequenceNumber;

                SynchronizeMemory();
            }

            Texture texture = Items[id];

            if (texture == null)
            {
                ulong address = Address + (ulong)(uint)id * DescriptorSize;

                Span<byte> data = Context.PhysicalMemory.Read(address, DescriptorSize);

                TextureDescriptor descriptor = MemoryMarshal.Cast<byte, TextureDescriptor>(data)[0];

                TextureInfo info = GetInfo(descriptor);

                // Bad address. We can't add a texture with a invalid address
                // to the cache.
                if (info.Address == MemoryManager.BadAddress)
                {
                    return null;
                }

                texture = Context.Methods.TextureManager.FindOrCreateTexture(info, TextureSearchFlags.Sampler);

                texture.IncrementReferenceCount();

                Items[id] = texture;
            }
            else
            {
                // Memory is automatically synchronized on texture creation.
                texture.SynchronizeMemory();
            }

            return texture;
        }

        protected override void InvalidateRangeImpl(ulong address, ulong size)
        {
            ulong endAddress = address + size;

            for (; address < endAddress; address += DescriptorSize)
            {
                int id = (int)((address - Address) / DescriptorSize);

                Texture texture = Items[id];

                if (texture != null)
                {
                    Span<byte> data = Context.PhysicalMemory.Read(address, DescriptorSize);

                    TextureDescriptor descriptor = MemoryMarshal.Cast<byte, TextureDescriptor>(data)[0];

                    // If the descriptors are the same, the texture is the same,
                    // we don't need to remove as it was not modified. Just continue.
                    if (texture.IsPerfectMatch(GetInfo(descriptor), TextureSearchFlags.Strict))
                    {
                        continue;
                    }

                    texture.DecrementReferenceCount();

                    Items[id] = null;
                }
            }
        }

        private TextureInfo GetInfo(TextureDescriptor descriptor)
        {
            ulong address = Context.MemoryManager.Translate(descriptor.UnpackAddress());

            int width         = descriptor.UnpackWidth();
            int height        = descriptor.UnpackHeight();
            int depthOrLayers = descriptor.UnpackDepth();
            int levels        = descriptor.UnpackLevels();

            TextureMsaaMode msaaMode = descriptor.UnpackTextureMsaaMode();

            int samplesInX = msaaMode.SamplesInX();
            int samplesInY = msaaMode.SamplesInY();

            int stride = descriptor.UnpackStride();

            TextureDescriptorType descriptorType = descriptor.UnpackTextureDescriptorType();

            bool isLinear = descriptorType == TextureDescriptorType.Linear;

            Target target = descriptor.UnpackTextureTarget().Convert((samplesInX | samplesInY) != 1);

            uint format = descriptor.UnpackFormat();
            bool srgb   = descriptor.UnpackSrgb();

            if (!FormatTable.TryGetTextureFormat(format, srgb, out FormatInfo formatInfo))
            {
                // TODO: Warning.

                formatInfo = FormatInfo.Default;
            }

            int gobBlocksInY = descriptor.UnpackGobBlocksInY();
            int gobBlocksInZ = descriptor.UnpackGobBlocksInZ();

            int gobBlocksInTileX = descriptor.UnpackGobBlocksInTileX();

            SwizzleComponent swizzleR = descriptor.UnpackSwizzleR().Convert();
            SwizzleComponent swizzleG = descriptor.UnpackSwizzleG().Convert();
            SwizzleComponent swizzleB = descriptor.UnpackSwizzleB().Convert();
            SwizzleComponent swizzleA = descriptor.UnpackSwizzleA().Convert();

            DepthStencilMode depthStencilMode = GetDepthStencilMode(
                formatInfo.Format,
                swizzleR,
                swizzleG,
                swizzleB,
                swizzleA);

            return new TextureInfo(
                address,
                width,
                height,
                depthOrLayers,
                levels,
                samplesInX,
                samplesInY,
                stride,
                isLinear,
                gobBlocksInY,
                gobBlocksInZ,
                gobBlocksInTileX,
                target,
                formatInfo,
                depthStencilMode,
                swizzleR,
                swizzleG,
                swizzleB,
                swizzleA);
        }

        private static DepthStencilMode GetDepthStencilMode(Format format, params SwizzleComponent[] components)
        {
            // R = Depth, G = Stencil.
            // On 24-bits depth formats, this is inverted (Stencil is R etc).
            // NVN setup:
            // For depth, A is set to 1.0f, the other components are set to Depth.
            // For stencil, all components are set to Stencil.
            SwizzleComponent component = components[0];

            for (int index = 1; index < 4 && !IsRG(component); index++)
            {
                component = components[index];
            }

            if (!IsRG(component))
            {
                return DepthStencilMode.Depth;
            }

            if (format == Format.D24X8Unorm || format == Format.D24UnormS8Uint)
            {
                return component == SwizzleComponent.Red
                    ? DepthStencilMode.Stencil
                    : DepthStencilMode.Depth;
            }
            else
            {
                return component == SwizzleComponent.Red
                    ? DepthStencilMode.Depth
                    : DepthStencilMode.Stencil;
            }
        }

        private static bool IsRG(SwizzleComponent component)
        {
            return component == SwizzleComponent.Red ||
                   component == SwizzleComponent.Green;
        }

        protected override void Delete(Texture item)
        {
            item?.DecrementReferenceCount();
        }
    }
}