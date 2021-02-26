using System;
using System.Runtime.InteropServices;

namespace SoundIOSharp
{
    public struct SoundIOChannelAreas
    {
        static readonly int native_size = Marshal.SizeOf<SoundIoChannelArea>();

        internal SoundIOChannelAreas(IntPtr head, int channelCount, int frameCount)
        {
            this.head          = head;
            this.channel_count = channelCount;
            this.frame_count   = frameCount;
        }

        IntPtr head;
        int    channel_count;
        int    frame_count;

        public bool IsEmpty
        {
            get { return head == IntPtr.Zero; }
        }

        public SoundIOChannelArea GetArea(int channel)
        {
            return new SoundIOChannelArea(head + native_size * channel);
        }

        public int ChannelCount => channel_count;
        public int FrameCount => frame_count;
    }
}