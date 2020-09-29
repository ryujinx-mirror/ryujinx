using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Memory;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture pool.
    /// </summary>
    class TexturePool : Pool<Texture>
    {
        private int _sequenceNumber;

        /// <summary>
        /// Intrusive linked list node used on the texture pool cache.
        /// </summary>
        public LinkedListNode<TexturePool> CacheNode { get; set; }

        /// <summary>
        /// Constructs a new instance of the texture pool.
        /// </summary>
        /// <param name="context">GPU context that the texture pool belongs to</param>
        /// <param name="address">Address of the texture pool in guest memory</param>
        /// <param name="maximumId">Maximum texture ID of the texture pool (equal to maximum textures minus one)</param>
        public TexturePool(GpuContext context, ulong address, int maximumId) : base(context, address, maximumId) { }

        /// <summary>
        /// Gets the texture with the given ID.
        /// </summary>
        /// <param name="id">ID of the texture. This is effectively a zero-based index</param>
        /// <returns>The texture with the given ID</returns>
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
                TextureDescriptor descriptor = GetDescriptor(id);

                TextureInfo info = GetInfo(descriptor);

                // Bad address. We can't add a texture with a invalid address
                // to the cache.
                if (info.Address == MemoryManager.BadAddress)
                {
                    return null;
                }

                texture = Context.Methods.TextureManager.FindOrCreateTexture(info, TextureSearchFlags.ForSampler);

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

        /// <summary>
        /// Gets the texture descriptor from a given texture ID.
        /// </summary>
        /// <param name="id">ID of the texture. This is effectively a zero-based index</param>
        /// <returns>The texture descriptor</returns>
        public TextureDescriptor GetDescriptor(int id)
        {
            return Context.PhysicalMemory.Read<TextureDescriptor>(Address + (ulong)id * DescriptorSize);
        }

        /// <summary>
        /// Implementation of the texture pool range invalidation.
        /// </summary>
        /// <param name="address">Start address of the range of the texture pool</param>
        /// <param name="size">Size of the range being invalidated</param>
        protected override void InvalidateRangeImpl(ulong address, ulong size)
        {
            ulong endAddress = address + size;

            for (; address < endAddress; address += DescriptorSize)
            {
                int id = (int)((address - Address) / DescriptorSize);

                Texture texture = Items[id];

                if (texture != null)
                {
                    TextureDescriptor descriptor = Context.PhysicalMemory.Read<TextureDescriptor>(address);

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

        /// <summary>
        /// Gets texture information from a texture descriptor.
        /// </summary>
        /// <param name="descriptor">The texture descriptor</param>
        /// <returns>The texture information</returns>
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

            // We use 2D targets for 1D textures as that makes texture cache
            // management easier. We don't know the target for render target
            // and copies, so those would normally use 2D targets, which are
            // not compatible with 1D targets. By doing that we also allow those
            // to match when looking for compatible textures on the cache.
            if (target == Target.Texture1D)
            {
                target = Target.Texture2D;
                height = 1;
            }
            else if (target == Target.Texture1DArray)
            {
                target = Target.Texture2DArray;
                height = 1;
            }

            uint format = descriptor.UnpackFormat();
            bool srgb   = descriptor.UnpackSrgb();

            if (!FormatTable.TryGetTextureFormat(format, srgb, out FormatInfo formatInfo))
            {
                if ((long)address > 0L && (int)format > 0)
                {
                    Logger.Error?.Print(LogClass.Gpu, $"Invalid texture format 0x{format:X} (sRGB: {srgb}).");
                }

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

            if (formatInfo.Format.IsDepthOrStencil())
            {
                swizzleR = SwizzleComponent.Red;
                swizzleG = SwizzleComponent.Red;
                swizzleB = SwizzleComponent.Red;

                if (depthStencilMode == DepthStencilMode.Depth)
                {
                    swizzleA = SwizzleComponent.One;
                }
                else
                {
                    swizzleA = SwizzleComponent.Red;
                }
            }

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

        /// <summary>
        /// Gets the texture depth-stencil mode, based on the swizzle components of each color channel.
        /// The depth-stencil mode is determined based on how the driver sets those parameters.
        /// </summary>
        /// <param name="format">The format of the texture</param>
        /// <param name="components">The texture swizzle components</param>
        /// <returns>The depth-stencil mode</returns>
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

        /// <summary>
        /// Checks if the swizzle component is equal to the red or green channels.
        /// </summary>
        /// <param name="component">The swizzle component to check</param>
        /// <returns>True if the swizzle component is equal to the red or green, false otherwise</returns>
        private static bool IsRG(SwizzleComponent component)
        {
            return component == SwizzleComponent.Red ||
                   component == SwizzleComponent.Green;
        }

        /// <summary>
        /// Decrements the reference count of the texture.
        /// This indicates that the texture pool is not using it anymore.
        /// </summary>
        /// <param name="item">The texture to be deleted</param>
        protected override void Delete(Texture item)
        {
            item?.DecrementReferenceCount();
        }
    }
}