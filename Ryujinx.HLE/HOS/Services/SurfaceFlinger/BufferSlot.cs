using Ryujinx.HLE.HOS.Services.SurfaceFlinger.Types;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    class BufferSlot
    {
        public AndroidStrongPointer<GraphicBuffer> GraphicBuffer;
        public BufferState                         BufferState;
        public bool                                RequestBufferCalled;
        public ulong                               FrameNumber;
        public AndroidFence                        Fence;
        public bool                                AcquireCalled;
        public bool                                NeedsCleanupOnRelease;
        public bool                                AttachedByConsumer;

        public BufferSlot()
        {
            GraphicBuffer = new AndroidStrongPointer<GraphicBuffer>();
            BufferState   = BufferState.Free;
        }
    }
}
