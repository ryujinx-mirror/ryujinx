namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    enum NativeWindowAttribute : uint
    {
        Width                 = 0,
        Height                = 1,
        Format                = 2,
        MinUnqueuedBuffers    = 3,
        ConsumerRunningBehind = 9,
        ConsumerUsageBits     = 10,
        MaxBufferCountAsync   = 12
    }
}
