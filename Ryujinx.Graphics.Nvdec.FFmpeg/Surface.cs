using FFmpeg.AutoGen;
using Ryujinx.Graphics.Video;
using System;

namespace Ryujinx.Graphics.Nvdec.FFmpeg
{
    unsafe class Surface : ISurface
    {
        public AVFrame* Frame { get; }

        public int RequestedWidth { get; }
        public int RequestedHeight { get; }

        public Plane YPlane => new Plane((IntPtr)Frame->data[0], Stride * Height);
        public Plane UPlane => new Plane((IntPtr)Frame->data[1], UvStride * UvHeight);
        public Plane VPlane => new Plane((IntPtr)Frame->data[2], UvStride * UvHeight);

        public FrameField Field => Frame->interlaced_frame != 0 ? FrameField.Interlaced : FrameField.Progressive;

        public int Width => Frame->width;
        public int Height => Frame->height;
        public int Stride => Frame->linesize[0];
        public int UvWidth => (Width + 1) >> 1;
        public int UvHeight => (Height + 1) >> 1;
        public int UvStride => Frame->linesize[1];

        public Surface(int width, int height)
        {
            RequestedWidth = width;
            RequestedHeight = height;

            Frame = ffmpeg.av_frame_alloc();
        }

        public void Dispose()
        {
            ffmpeg.av_frame_unref(Frame);
            ffmpeg.av_free(Frame);
        }
    }
}
