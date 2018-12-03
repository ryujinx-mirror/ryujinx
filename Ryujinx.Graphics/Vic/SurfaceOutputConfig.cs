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
            SurfacePixelFormat PixelFormat,
            int                SurfaceWidth,
            int                SurfaceHeight,
            int                GobBlockHeight,
            long               OutputSurfaceLumaAddress,
            long               OutputSurfaceChromaUAddress,
            long               OutputSurfaceChromaVAddress)
        {
            this.PixelFormat           = PixelFormat;
            this.SurfaceWidth          = SurfaceWidth;
            this.SurfaceHeight         = SurfaceHeight;
            this.GobBlockHeight        = GobBlockHeight;
            this.SurfaceLumaAddress    = OutputSurfaceLumaAddress;
            this.SurfaceChromaUAddress = OutputSurfaceChromaUAddress;
            this.SurfaceChromaVAddress = OutputSurfaceChromaVAddress;
        }
    }
}