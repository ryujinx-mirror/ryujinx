using Ryujinx.Graphics.GAL.Texture;

namespace Ryujinx.Graphics.Gpu.Image
{
    struct TextureInfo
    {
        public ulong Address { get; }

        public int  Width            { get; }
        public int  Height           { get; }
        public int  DepthOrLayers    { get; }
        public int  Levels           { get; }
        public int  SamplesInX       { get; }
        public int  SamplesInY       { get; }
        public int  Stride           { get; }
        public bool IsLinear         { get; }
        public int  GobBlocksInY     { get; }
        public int  GobBlocksInZ     { get; }
        public int  GobBlocksInTileX { get; }

        public int Samples => SamplesInX * SamplesInY;

        public Target Target { get; }

        public FormatInfo FormatInfo { get; }

        public DepthStencilMode DepthStencilMode { get; }

        public SwizzleComponent SwizzleR { get; }
        public SwizzleComponent SwizzleG { get; }
        public SwizzleComponent SwizzleB { get; }
        public SwizzleComponent SwizzleA { get; }

        public TextureInfo(
            ulong            address,
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
            Address          = address;
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

        public int GetDepth()
        {
            return Target == Target.Texture3D ? DepthOrLayers : 1;
        }

        public int GetLayers()
        {
            if (Target == Target.Texture2DArray || Target == Target.Texture2DMultisampleArray)
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
    }
}