namespace Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvGpuGpu
{
    struct NvGpuGpuGetTpcMasks
    {
        public int  MaskBufferSize;
        public int  Reserved;
        public long MaskBufferAddress;
        public int  TpcMask;
        public int  Padding;
    }
}
