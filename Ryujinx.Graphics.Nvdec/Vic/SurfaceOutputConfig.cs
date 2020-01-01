namespace Ryujinx.Graphics.Vic
{
    struct SurfaceOutputConfig
    {
        public SurfacePixelFormat PixelFormat;

        public int SurfaceWidth;
        public int SurfaceHeight;
        public int GobBlockHeight;

        public ulong SurfaceLumaAddress;
        public ulong SurfaceChromaUAddress;
        public ulong SurfaceChromaVAddress;

        public SurfaceOutputConfig(
            SurfacePixelFormat pixelFormat,
            int                surfaceWidth,
            int                surfaceHeight,
            int                gobBlockHeight,
            ulong              outputSurfaceLumaAddress,
            ulong              outputSurfaceChromaUAddress,
            ulong              outputSurfaceChromaVAddress)
        {
            PixelFormat           = pixelFormat;
            SurfaceWidth          = surfaceWidth;
            SurfaceHeight         = surfaceHeight;
            GobBlockHeight        = gobBlockHeight;
            SurfaceLumaAddress    = outputSurfaceLumaAddress;
            SurfaceChromaUAddress = outputSurfaceChromaUAddress;
            SurfaceChromaVAddress = outputSurfaceChromaVAddress;
        }
    }
}