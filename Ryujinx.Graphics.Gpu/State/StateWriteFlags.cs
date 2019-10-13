namespace Ryujinx.Graphics.Gpu.State
{
    enum StateWriteFlags
    {
        InputAssemblerGroup =
            VertexAttribState     |
            PrimitiveRestartState |
            IndexBufferState      |
            VertexBufferState,

        RenderTargetGroup =
            RtColorState |
            RtDepthStencilState,

        RtColorState          = 1 << 0,
        ViewportTransform     = 1 << 1,
        DepthBiasState        = 1 << 2,
        RtDepthStencilState   = 1 << 3,
        DepthTestState        = 1 << 4,
        VertexAttribState     = 1 << 5,
        StencilTestState      = 1 << 6,
        SamplerPoolState      = 1 << 7,
        TexturePoolState      = 1 << 8,
        PrimitiveRestartState = 1 << 9,
        IndexBufferState      = 1 << 10,
        FaceState             = 1 << 11,
        RtColorMask           = 1 << 12,
        VertexBufferState     = 1 << 13,
        BlendState            = 1 << 14,
        ShaderState           = 1 << 15,

        Any = -1
    }
}
