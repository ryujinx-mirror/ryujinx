using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Texture;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture pool.
    /// </summary>
    class TexturePool : Pool<Texture, TextureDescriptor>
    {
        private readonly GpuChannel _channel;
        private readonly ConcurrentQueue<Texture> _dereferenceQueue = new ConcurrentQueue<Texture>();

        /// <summary>
        /// Intrusive linked list node used on the texture pool cache.
        /// </summary>
        public LinkedListNode<TexturePool> CacheNode { get; set; }

        /// <summary>
        /// Constructs a new instance of the texture pool.
        /// </summary>
        /// <param name="context">GPU context that the texture pool belongs to</param>
        /// <param name="channel">GPU channel that the texture pool belongs to</param>
        /// <param name="address">Address of the texture pool in guest memory</param>
        /// <param name="maximumId">Maximum texture ID of the texture pool (equal to maximum textures minus one)</param>
        public TexturePool(GpuContext context, GpuChannel channel, ulong address, int maximumId) : base(context, channel.MemoryManager.Physical, address, maximumId)
        {
            _channel = channel;
        }

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

            if (SequenceNumber != Context.SequenceNumber)
            {
                SequenceNumber = Context.SequenceNumber;

                SynchronizeMemory();
            }

            Texture texture = Items[id];

            if (texture == null)
            {
                TextureDescriptor descriptor = GetDescriptor(id);

                TextureInfo info = GetInfo(descriptor, out int layerSize);

                ProcessDereferenceQueue();

                texture = PhysicalMemory.TextureCache.FindOrCreateTexture(_channel.MemoryManager, TextureSearchFlags.ForSampler, info, layerSize);

                // If this happens, then the texture address is invalid, we can't add it to the cache.
                if (texture == null)
                {
                    return null;
                }

                texture.IncrementReferenceCount(this, id);

                Items[id] = texture;

                DescriptorCache[id] = descriptor;
            }
            else
            {
                if (texture.ChangedSize)
                {
                    // Texture changed size at one point - it may be a different size than the sampler expects.
                    // This can be triggered when the size is changed by a size hint on copy or draw, but the texture has been sampled before.

                    TextureDescriptor descriptor = GetDescriptor(id);

                    int baseLevel = descriptor.UnpackBaseLevel();
                    int width = Math.Max(1, descriptor.UnpackWidth() >> baseLevel);
                    int height = Math.Max(1, descriptor.UnpackHeight() >> baseLevel);

                    if (texture.Info.Width != width || texture.Info.Height != height)
                    {
                        texture.ChangeSize(width, height, texture.Info.DepthOrLayers);
                    }
                }

                // Memory is automatically synchronized on texture creation.
                texture.SynchronizeMemory();
            }

            return texture;
        }

        /// <summary>
        /// Forcibly remove a texture from this pool's items.
        /// If deferred, the dereference will be queued to occur on the render thread.
        /// </summary>
        /// <param name="texture">The texture being removed</param>
        /// <param name="id">The ID of the texture in this pool</param>
        /// <param name="deferred">If true, queue the dereference to happen on the render thread, otherwise dereference immediately</param>
        public void ForceRemove(Texture texture, int id, bool deferred)
        {
            Items[id] = null;

            if (deferred)
            {
                _dereferenceQueue.Enqueue(texture);
            }
            else
            {
                texture.DecrementReferenceCount();
            }
        }

        /// <summary>
        /// Process the dereference queue, decrementing the reference count for each texture in it.
        /// This is used to ensure that texture disposal happens on the render thread.
        /// </summary>
        private void ProcessDereferenceQueue()
        {
            while (_dereferenceQueue.TryDequeue(out Texture toRemove))
            {
                toRemove.DecrementReferenceCount();
            }
        }

        /// <summary>
        /// Implementation of the texture pool range invalidation.
        /// </summary>
        /// <param name="address">Start address of the range of the texture pool</param>
        /// <param name="size">Size of the range being invalidated</param>
        protected override void InvalidateRangeImpl(ulong address, ulong size)
        {
            ProcessDereferenceQueue();

            ulong endAddress = address + size;

            for (; address < endAddress; address += DescriptorSize)
            {
                int id = (int)((address - Address) / DescriptorSize);

                Texture texture = Items[id];

                if (texture != null)
                {
                    TextureDescriptor descriptor = PhysicalMemory.Read<TextureDescriptor>(address);

                    // If the descriptors are the same, the texture is the same,
                    // we don't need to remove as it was not modified. Just continue.
                    if (descriptor.Equals(ref DescriptorCache[id]))
                    {
                        continue;
                    }

                    texture.DecrementReferenceCount(this, id);

                    Items[id] = null;
                }
            }
        }

        /// <summary>
        /// Gets texture information from a texture descriptor.
        /// </summary>
        /// <param name="descriptor">The texture descriptor</param>
        /// <param name="layerSize">Layer size for textures using a sub-range of mipmap levels, otherwise 0</param>
        /// <returns>The texture information</returns>
        private TextureInfo GetInfo(TextureDescriptor descriptor, out int layerSize)
        {
            int depthOrLayers = descriptor.UnpackDepth();
            int levels        = descriptor.UnpackLevels();

            TextureMsaaMode msaaMode = descriptor.UnpackTextureMsaaMode();

            int samplesInX = msaaMode.SamplesInX();
            int samplesInY = msaaMode.SamplesInY();

            int stride = descriptor.UnpackStride();

            TextureDescriptorType descriptorType = descriptor.UnpackTextureDescriptorType();

            bool isLinear = descriptorType == TextureDescriptorType.Linear;

            Target target = descriptor.UnpackTextureTarget().Convert((samplesInX | samplesInY) != 1);

            int width = target == Target.TextureBuffer ? descriptor.UnpackBufferTextureWidth() : descriptor.UnpackWidth();
            int height = descriptor.UnpackHeight();

            if (target == Target.Texture2DMultisample || target == Target.Texture2DMultisampleArray)
            {
                // This is divided back before the backend texture is created.
                width *= samplesInX;
                height *= samplesInY;
            }

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

            ulong gpuVa = descriptor.UnpackAddress();

            if (!FormatTable.TryGetTextureFormat(format, srgb, out FormatInfo formatInfo))
            {
                if (gpuVa != 0 && (int)format > 0)
                {
                    Logger.Error?.Print(LogClass.Gpu, $"Invalid texture format 0x{format:X} (sRGB: {srgb}).");
                }

                formatInfo = FormatInfo.Default;
            }

            int gobBlocksInY = descriptor.UnpackGobBlocksInY();
            int gobBlocksInZ = descriptor.UnpackGobBlocksInZ();

            int gobBlocksInTileX = descriptor.UnpackGobBlocksInTileX();

            layerSize = 0;

            int minLod = descriptor.UnpackBaseLevel();
            int maxLod = descriptor.UnpackMaxLevelInclusive();

            // Linear textures don't support mipmaps, so we don't handle this case here.
            if ((minLod != 0 || maxLod + 1 != levels) && target != Target.TextureBuffer && !isLinear)
            {
                int depth  = TextureInfo.GetDepth(target, depthOrLayers);
                int layers = TextureInfo.GetLayers(target, depthOrLayers);

                SizeInfo sizeInfo = SizeCalculator.GetBlockLinearTextureSize(
                    width,
                    height,
                    depth,
                    levels,
                    layers,
                    formatInfo.BlockWidth,
                    formatInfo.BlockHeight,
                    formatInfo.BytesPerPixel,
                    gobBlocksInY,
                    gobBlocksInZ,
                    gobBlocksInTileX);

                layerSize = sizeInfo.LayerSize;

                if (minLod != 0 && minLod < levels)
                {
                    // If the base level is not zero, we additionally add the mip level offset
                    // to the address, this allows the texture manager to find the base level from the
                    // address if there is a overlapping texture on the cache that can contain the new texture.
                    gpuVa += (ulong)sizeInfo.GetMipOffset(minLod);

                    width  = Math.Max(1, width  >> minLod);
                    height = Math.Max(1, height >> minLod);

                    if (target == Target.Texture3D)
                    {
                        depthOrLayers = Math.Max(1, depthOrLayers >> minLod);
                    }

                    (gobBlocksInY, gobBlocksInZ) = SizeCalculator.GetMipGobBlockSizes(height, depth, formatInfo.BlockHeight, gobBlocksInY, gobBlocksInZ);
                }

                levels = (maxLod - minLod) + 1;
            }

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
                gpuVa,
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

            if (format == Format.D24UnormS8Uint)
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
            item?.DecrementReferenceCount(this);
        }

        public override void Dispose()
        {
            ProcessDereferenceQueue();

            base.Dispose();
        }
    }
}