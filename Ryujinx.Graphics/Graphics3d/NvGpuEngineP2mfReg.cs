namespace Ryujinx.Graphics.Graphics3d
{
    enum NvGpuEngineP2mfReg
    {
        LineLengthIn = 0x60,
        LineCount    = 0x61,
        DstAddress   = 0x62,
        DstPitch     = 0x64,
        DstBlockDim  = 0x65,
        DstWidth     = 0x66,
        DstHeight    = 0x67,
        DstDepth     = 0x68,
        DstZ         = 0x69,
        DstX         = 0x6a,
        DstY         = 0x6b
    }
}