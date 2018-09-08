namespace Ryujinx.Graphics
{
    enum NvGpuEngineDmaReg
    {
        SrcAddress = 0x100,
        DstAddress = 0x102,
        SrcPitch   = 0x104,
        DstPitch   = 0x105,
        DstBlkDim  = 0x1c3,
        DstSizeX   = 0x1c4,
        DstSizeY   = 0x1c5,
        DstSizeZ   = 0x1c6,
        DstPosZ    = 0x1c7,
        DstPosXY   = 0x1c8,
        SrcBlkDim  = 0x1ca,
        SrcSizeX   = 0x1cb,
        SrcSizeY   = 0x1cc,
        SrcSizeZ   = 0x1cd,
        SrcPosZ    = 0x1ce,
        SrcPosXY   = 0x1cf
    }
}