namespace Ryujinx.Graphics.Shader.Decoders
{
    enum TextureTarget
    {
        Texture1DLodZero                  = 0x0,
        Texture2D                         = 0x1,
        Texture2DLodZero                  = 0x2,
        Texture2DLodLevel                 = 0x3,
        Texture2DDepthCompare             = 0x4,
        Texture2DLodLevelDepthCompare     = 0x5,
        Texture2DLodZeroDepthCompare      = 0x6,
        Texture2DArray                    = 0x7,
        Texture2DArrayLodZero             = 0x8,
        Texture2DArrayLodZeroDepthCompare = 0x9,
        Texture3D                         = 0xa,
        Texture3DLodZero                  = 0xb,
        TextureCube                       = 0xc,
        TextureCubeLodLevel               = 0xd
    }
}