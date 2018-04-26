namespace Ryujinx.Graphics.Gpu
{
    enum NvGpuEngine2dReg
    {
        DstFormat          = 0x80,
        DstLinear          = 0x81,
        DstBlockDimensions = 0x82,
        DstDepth           = 0x83,
        DstLayer           = 0x84,
        DstPitch           = 0x85,
        DstWidth           = 0x86,
        DstHeight          = 0x87,
        DstAddress         = 0x88,
        SrcFormat          = 0x8c,
        SrcLinear          = 0x8d,
        SrcBlockDimensions = 0x8e,
        SrcDepth           = 0x8f,
        SrcLayer           = 0x90,
        SrcPitch           = 0x91,
        SrcWidth           = 0x92,
        SrcHeight          = 0x93,
        SrcAddress         = 0x94,
        CopyOperation      = 0xab
    }
}