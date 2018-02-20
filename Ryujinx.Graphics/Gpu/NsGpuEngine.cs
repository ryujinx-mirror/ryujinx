namespace Ryujinx.Graphics.Gpu
{
    enum NsGpuEngine
    {
        None    = 0,
        _2d     = 0x902d,
        _3d     = 0xb197,
        Compute = 0xb1c0,
        Kepler  = 0xa140,
        Dma     = 0xb0b5,
        GpFifo  = 0xb06f
    }
}