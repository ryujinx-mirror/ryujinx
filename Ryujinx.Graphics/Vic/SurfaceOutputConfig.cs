namespace Ryujinx.Graphics.Vic
{
    struct SurfaceOutputConfig
    {
        public SurfacePixelFormat PixelFormat;

        public int SurfaceWidth;
        public int SurfaceHeight;
        public int GobBlockHeight;

        public long SurfaceLumaAddress;
        public long SurfaceChromaUAddress;
        public long SurfaceChromaVAddress;

        public SurfaceOutputConfig(
            SurfacePixelFormat pixelFormat,
            int                surfaceWidth,
            int                surfaceHeight,
            int                gobBlockHeight,
            long               outputSurfaceLumaAddress,
            long               outputSurfaceChromaUAddress,
            long               outputSurfaceChromaVAddress)
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