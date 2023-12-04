using AVBufferRef = System.IntPtr;

namespace Ryujinx.Graphics.Nvdec.FFmpeg.Native
{
    struct AVPacket
    {
#pragma warning disable CS0649 // Field is never assigned to
        public unsafe AVBufferRef* Buf;
        public long Pts;
        public long Dts;
        public unsafe byte* Data;
        public int Size;
        public int StreamIndex;
        public int Flags;
        public AVBufferRef SizeData;
        public int SizeDataElems;
        public long Duration;
        public long Position;
        public AVBufferRef Opaque;
        public unsafe AVBufferRef* OpaqueRef;
        public AVRational TimeBase;
#pragma warning restore CS0649
    }
}
