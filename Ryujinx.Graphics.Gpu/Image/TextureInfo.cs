using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Texture;

namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// Texture information.
    /// </summary>
    struct TextureInfo
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
            ulong            gpuAddress,
            int              width,
            int              height,
            int              depthOrLayers,
            int              levels,
            int              samplesInX,
            int              samplesInY,
            int              stride,
            bool             isLinear,
            int              gobBlocksInY,
            int              gobBlocksInZ,
            int              gobBlocksInTileX,
            Target           target,
            FormatInfo       formatInfo,
            DepthStencilMode depthStencilMode = DepthStencilMode.Depth,
            SwizzleComponent swizzleR         = SwizzleComponent.Red,
            SwizzleComponent swizzleG         = SwizzleComponent.Green,
            SwizzleComponent swizzleB         = SwizzleComponent.Blue,
            SwizzleComponent swizzleA         = SwizzleComponent.Alpha)
        {
            GpuAddress       = gpuAddress;
            Width            = width;
            Height           = height;
            DepthOrLayers    = depthOrLayers;
            Levels           = levels;
            SamplesInX       = samplesInX;
            SamplesInY       = samplesInY;
            Stride           = stride;
            IsLinear         = isLinear;
            GobBlocksInY     = gobBlocksInY;
            GobBlocksInZ     = gobBlocksInZ;
            GobBlocksInTileX = gobBlocksInTileX;
            Target           = target;
            FormatInfo       = formatInfo;
            DepthStencilMode = depthStencilMode;
            SwizzleR         = swizzleR;
            SwizzleG         = swizzleG;
            SwizzleB         = swizzleB;
            SwizzleA         = swizzleA;
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
    }
}