using System;

namespace Ryujinx.Graphics.Nvdec.FFmpeg.Native
{
    struct FFCodec
    {
        public unsafe delegate int AVCodec_decode(AVCodecContext* avctx, void* outdata, int* got_frame_ptr, AVPacket* avpkt);

#pragma warning disable CS0649
        public AVCodec Base;
        public int CapsInternalOrCbType;
        public int PrivDataSize;
        public IntPtr UpdateThreadContext;
        public IntPtr UpdateThreadContextForUser;
        public IntPtr Defaults;
        public IntPtr InitStaticData;
        public IntPtr Init;
        public IntPtr CodecCallback;
#pragma warning restore CS0649

        // NOTE: There is more after, but the layout kind of changed a bit and we don't need more than this. This is safe as we only manipulate this behind a reference.
    }
}
