namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    interface IConsumerListener
    {
        void OnFrameAvailable(ref BufferItem item);
        void OnFrameReplaced(ref BufferItem item);
        void OnBuffersReleased();
    }
}
