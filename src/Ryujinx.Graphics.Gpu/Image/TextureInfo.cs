using Ryujinx.Common;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Texture;
using System;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture information.
    /// </summary>
    readonly struct TextureInfo
    {
        /// <summary>
        /// Address of the texture in GPU mapped memory.
        /// </summary>
        public ulong GpuAddress { get; }

        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the texture, or layers count for 1D array textures.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// The depth of the texture (for 3D textures), or layers count for array textures.
        /// </summary>
        public int DepthOrLayers { get; }

        /// <summary>
        /// The number of mipmap levels of the texture.
        /// </summary>
        public int Levels { get; }

        /// <summary>
        /// The number of samples in the X direction for multisampled textures.
        /// </summary>
        public int SamplesInX { get; }

        /// <summary>
        /// The number of samples in the Y direction for multisampled textures.
        /// </summary>
        public int SamplesInY { get; }

        /// <summary>
        /// The number of bytes per line for linear textures.
        /// </summary>
        public int Stride { get; }

        /// <summary>
        /// Indicates whenever or not the texture is a linear texture.
        /// </summary>
        public bool IsLinear { get; }

        /// <summary>
        /// GOB blocks in the Y direction, for block linear textures.
        /// </summary>
        public int GobBlocksInY { get; }

        /// <summary>
        /// GOB blocks in the Z direction, for block linear textures.
        /// </summary>
        public int GobBlocksInZ { get; }

        /// <summary>
        /// Number of GOB blocks per tile in the X direction, for block linear textures.
        /// </summary>
        public int GobBlocksInTileX { get; }

        /// <summary>
        /// Total number of samples for multisampled textures.
        /// </summary>
        public int Samples => SamplesInX * SamplesInY;

        /// <summary>
        /// Texture target type.
        /// </summary>
        public Target Target { get; }

        /// <summary>
        /// Texture format information.
        /// </summary>
        public FormatInfo FormatInfo { get; }

        /// <summary>
        /// Depth-stencil mode of the texture. This defines whenever the depth or stencil value is read from shaders,
        /// for depth-stencil texture formats.
        /// </summary>
        public DepthStencilMode DepthStencilMode { get; }

        /// <summary>
        /// Texture swizzle for the red color channel.
        /// </summary>
        public SwizzleComponent SwizzleR { get; }

        /// <summary>
        /// Texture swizzle for the green color channel.
        /// </summary>
        public SwizzleComponent SwizzleG { get; }

        /// <summary>
        /// Texture swizzle for the blue color channel.
        /// </summary>
        public SwizzleComponent SwizzleB { get; }

        /// <summary>
        /// Texture swizzle for the alpha color channel.
        /// </summary>
        public SwizzleComponent SwizzleA { get; }

        /// <summary>
        /// Constructs the texture information structure.
        /// </summary>
        /// <param name="gpuAddress">The GPU address of the texture</param>
        /// <param name="width">The width of the texture</param>
        /// <param name="height">The height or the texture</param>
        /// <param name="depthOrLayers">The depth or layers count of the texture</param>
        /// <param name="levels">The amount of mipmap levels of the texture</param>
        /// <param name="samplesInX">The number of samples in the X direction for multisample textures, should be 1 otherwise</param>
        /// <param name="samplesInY">The number of samples in the Y direction for multisample textures, should be 1 otherwise</param>
        /// <param name="stride">The stride for linear textures</param>
        /// <param name="isLinear">Whenever the texture is linear or block linear</param>
        /// <param name="gobBlocksInY">Number of GOB blocks in the Y direction</param>
        /// <param name="gobBlocksInZ">Number of GOB blocks in the Z direction</param>
        /// <param name="gobBlocksInTileX">Number of GOB blocks per tile in the X direction</param>
        /// <param name="target">Texture target type</param>
        /// <param name="formatInfo">Texture format information</param>
        /// <param name="depthStencilMode">Depth-stencil mode</param>
        /// <param name="swizzleR">Swizzle for the red color channel</param>
        /// <param name="swizzleG">Swizzle for the green color channel</param>
        /// <param name="swizzleB">Swizzle for the blue color channel</param>
        /// <param name="swizzleA">Swizzle for the alpha color channel</param>
        public TextureInfo(
            ulong gpuAddress,
            int width,
            int height,
            int depthOrLayers,
            int levels,
            int samplesInX,
            int samplesInY,
            int stride,
            bool isLinear,
            int gobBlocksInY,
            int gobBlocksInZ,
            int gobBlocksInTileX,
            Target target,
            FormatInfo formatInfo,
            DepthStencilMode depthStencilMode = DepthStencilMode.Depth,
            SwizzleComponent swizzleR = SwizzleComponent.Red,
            SwizzleComponent swizzleG = SwizzleComponent.Green,
            SwizzleComponent swizzleB = SwizzleComponent.Blue,
            SwizzleComponent swizzleA = SwizzleComponent.Alpha)
        {
            GpuAddress = gpuAddress;
            Width = width;
            Height = height;
            DepthOrLayers = depthOrLayers;
            Levels = levels;
            SamplesInX = samplesInX;
            SamplesInY = samplesInY;
            Stride = stride;
            IsLinear = isLinear;
            GobBlocksInY = gobBlocksInY;
            GobBlocksInZ = gobBlocksInZ;
            GobBlocksInTileX = gobBlocksInTileX;
            Target = target;
            FormatInfo = formatInfo;
            DepthStencilMode = depthStencilMode;
            SwizzleR = swizzleR;
            SwizzleG = swizzleG;
            SwizzleB = swizzleB;
            SwizzleA = swizzleA;
        }

        /// <summary>
        /// Gets the real texture depth.
        /// Returns 1 for any target other than 3D textures.
        /// </summary>
        /// <returns>Texture depth</returns>
        public int GetDepth()
        {
            return GetDepth(Target, DepthOrLayers);
        }

        /// <summary>
        /// Gets the real texture depth.
        /// Returns 1 for any target other than 3D textures.
        /// </summary>
        /// <param name="target">Texture target</param>
        /// <param name="depthOrLayers">Texture depth if the texture is 3D, otherwise ignored</param>
        /// <returns>Texture depth</returns>
        public static int GetDepth(Target target, int depthOrLayers)
        {
            return target == Target.Texture3D ? depthOrLayers : 1;
        }

        /// <summary>
        /// Gets the number of layers of the texture.
        /// Returns 1 for non-array textures, 6 for cubemap textures, and layer faces for cubemap array textures.
        /// </summary>
        /// <returns>The number of texture layers</returns>
        public int GetLayers()
        {
            return GetLayers(Target, DepthOrLayers);
        }

        /// <summary>
        /// Gets the number of layers of the texture.
        /// Returns 1 for non-array textures, 6 for cubemap textures, and layer faces for cubemap array textures.
        /// </summary>
        /// <param name="target">Texture target</param>
        /// <param name="depthOrLayers">Texture layers if the is a array texture, ignored otherwise</param>
        /// <returns>The number of texture layers</returns>
        public static int GetLayers(Target target, int depthOrLayers)
        {
            if (target == Target.Texture2DArray || target == Target.Texture2DMultisampleArray)
            {
                return depthOrLayers;
            }
            else if (target == Target.CubemapArray)
            {
                return depthOrLayers * 6;
            }
            else if (target == Target.Cubemap)
            {
                return 6;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Gets the number of 2D slices of the texture.
        /// Returns 6 for cubemap textures, layer faces for cubemap array textures, and DepthOrLayers for everything else.
        /// </summary>
        /// <returns>The number of texture slices</returns>
        public int GetSlices()
        {
            if (Target == Target.Texture3D || Target == Target.Texture2DArray || Target == Target.Texture2DMultisampleArray)
            {
                return DepthOrLayers;
            }
            else if (Target == Target.CubemapArray)
            {
                return DepthOrLayers * 6;
            }
            else if (Target == Target.Cubemap)
            {
                return 6;
            }
            else
            {
                return 1;
            }
        }

        /// <summary>
        /// Calculates the size information from the texture information.
        /// </summary>
        /// <param name="layerSize">Optional size of each texture layer in bytes</param>
        /// <returns>Texture size information</returns>
        public SizeInfo CalculateSizeInfo(int layerSize = 0)
        {
            if (Target == Target.TextureBuffer)
            {
                return new SizeInfo(Width * FormatInfo.BytesPerPixel);
            }
            else if (IsLinear)
            {
                return SizeCalculator.GetLinearTextureSize(
                    Stride,
                    Height,
                    FormatInfo.BlockHeight);
            }
            else
            {
                return SizeCalculator.GetBlockLinearTextureSize(
                    Width,
                    Height,
                    GetDepth(),
                    Levels,
                    GetLayers(),
                    FormatInfo.BlockWidth,
                    FormatInfo.BlockHeight,
                    FormatInfo.BytesPerPixel,
                    GobBlocksInY,
                    GobBlocksInZ,
                    GobBlocksInTileX,
                    layerSize);
            }
        }

        /// <summary>
        /// Creates texture information for a given mipmap level of the specified parent texture and this information.
        /// </summary>
        /// <param name="parent">The parent texture</param>
        /// <param name="firstLevel">The first level of the texture view</param>
        /// <param name="parentFormat">True if the parent format should be inherited</param>
        /// <returns>The adjusted texture information with the new size</returns>
        public TextureInfo CreateInfoForLevelView(Texture parent, int firstLevel, bool parentFormat)
        {
            // When the texture is used as view of another texture, we must
            // ensure that the sizes are valid, otherwise data uploads would fail
            // (and the size wouldn't match the real size used on the host API).
            // Given a parent texture from where the view is created, we have the
            // following rules:
            // - The view size must be equal to the parent size, divided by (2 ^ l),
            // where l is the first mipmap level of the view. The division result must
            // be rounded down, and the result must be clamped to 1.
            // - If the parent format is compressed, and the view format isn't, the
            // view size is calculated as above, but the width and height of the
            // view must be also divided by the compressed format block width and height.
            // - If the parent format is not compressed, and the view is, the view
            // size is calculated as described on the first point, but the width and height
            // of the view must be also multiplied by the block width and height.
            int width = Math.Max(1, parent.Info.Width >> firstLevel);
            int height = Math.Max(1, parent.Info.Height >> firstLevel);

            if (parent.Info.FormatInfo.IsCompressed && !FormatInfo.IsCompressed)
            {
                width = BitUtils.DivRoundUp(width, parent.Info.FormatInfo.BlockWidth);
                height = BitUtils.DivRoundUp(height, parent.Info.FormatInfo.BlockHeight);
            }
            else if (!parent.Info.FormatInfo.IsCompressed && FormatInfo.IsCompressed)
            {
                width *= FormatInfo.BlockWidth;
                height *= FormatInfo.BlockHeight;
            }

            int depthOrLayers;

            if (Target == Target.Texture3D)
            {
                depthOrLayers = Math.Max(1, parent.Info.DepthOrLayers >> firstLevel);
            }
            else
            {
                depthOrLayers = DepthOrLayers;
            }

            // 2D and 2D multisample textures are not considered compatible.
            // This specific case is required for copies, where the source texture might be multisample.
            // In this case, we inherit the parent texture multisample state.
            Target target = Target;
            int samplesInX = SamplesInX;
            int samplesInY = SamplesInY;

            if (target == Target.Texture2D && parent.Target == Target.Texture2DMultisample)
            {
                target = Target.Texture2DMultisample;
                samplesInX = parent.Info.SamplesInX;
                samplesInY = parent.Info.SamplesInY;
            }

            return new TextureInfo(
                GpuAddress,
                width,
                height,
                depthOrLayers,
                Levels,
                samplesInX,
                samplesInY,
                Stride,
                IsLinear,
                GobBlocksInY,
                GobBlocksInZ,
                GobBlocksInTileX,
                target,
                parentFormat ? parent.Info.FormatInfo : FormatInfo,
                DepthStencilMode,
                SwizzleR,
                SwizzleG,
                SwizzleB,
                SwizzleA);
        }

        /// <summary>
        /// Creates texture information for a given format and this information.
        /// </summary>
        /// <param name="formatInfo">Format for the new texture info</param>
        /// <returns>New info with the specified format</returns>
        public TextureInfo CreateInfoWithFormat(FormatInfo formatInfo)
        {
            return new TextureInfo(
                GpuAddress,
                Width,
                Height,
                DepthOrLayers,
                Levels,
                SamplesInX,
                SamplesInY,
                Stride,
                IsLinear,
                GobBlocksInY,
                GobBlocksInZ,
                GobBlocksInTileX,
                Target,
                formatInfo,
                DepthStencilMode,
                SwizzleR,
                SwizzleG,
                SwizzleB,
                SwizzleA);
        }
    }
}
