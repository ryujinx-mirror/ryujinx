using FFmpeg.AutoGen;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.VDec
{
    static unsafe class FFmpegWrapper
    {
        private static AVCodec*        _codec;
        private static AVCodecContext* _context;
        private static AVFrame*        _frame;
        private static SwsContext*     _scalerCtx;

        private static int _scalerWidth;
        private static int _scalerHeight;

        public static bool IsInitialized { get; private set; }

        public static void H264Initialize()
        {
            EnsureCodecInitialized(AVCodecID.AV_CODEC_ID_H264);
        }

        public static void Vp9Initialize()
        {
            EnsureCodecInitialized(AVCodecID.AV_CODEC_ID_VP9);
        }

        private static void EnsureCodecInitialized(AVCodecID codecId)
        {
            if (IsInitialized)
            {
                Uninitialize();
            }

            _codec   = ffmpeg.avcodec_find_decoder(codecId);
            _context = ffmpeg.avcodec_alloc_context3(_codec);
            _frame   = ffmpeg.av_frame_alloc();

            ffmpeg.avcodec_open2(_context, _codec, null);

            IsInitialized = true;
        }

        public static int DecodeFrame(byte[] data)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Tried to use uninitialized codec!");
            }

            AVPacket packet;

            ffmpeg.av_init_packet(&packet);

            fixed (byte* ptr = data)
            {
                packet.data = ptr;
                packet.size = data.Length;

                ffmpeg.avcodec_send_packet(_context, &packet);
            }

            return ffmpeg.avcodec_receive_frame(_context, _frame);
        }

        public static FFmpegFrame GetFrame()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Tried to use uninitialized codec!");
            }

            AVFrame managedFrame = Marshal.PtrToStructure<AVFrame>((IntPtr)_frame);

            byte*[] data = managedFrame.data.ToArray();

            return new FFmpegFrame()
            {
                Width  = managedFrame.width,
                Height = managedFrame.height,

                LumaPtr    = data[0],
                ChromaBPtr = data[1],
                ChromaRPtr = data[2]
            };
        }

        public static FFmpegFrame GetFrameRgba()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Tried to use uninitialized codec!");
            }

            AVFrame managedFrame = Marshal.PtrToStructure<AVFrame>((IntPtr)_frame);

            EnsureScalerSetup(managedFrame.width, managedFrame.height);

            byte*[] data = managedFrame.data.ToArray();

            int[] lineSizes = managedFrame.linesize.ToArray();

            byte[] dst = new byte[managedFrame.width * managedFrame.height * 4];

            fixed (byte* ptr = dst)
            {
                byte*[] dstData = new byte*[] { ptr };

                int[] dstLineSizes = new int[] { managedFrame.width * 4 };

                ffmpeg.sws_scale(_scalerCtx, data, lineSizes, 0, managedFrame.height, dstData, dstLineSizes);
            }

            return new FFmpegFrame()
            {
                Width  = managedFrame.width,
                Height = managedFrame.height,

                Data = dst
            };
        }

        private static void EnsureScalerSetup(int width, int height)
        {
            if (width == 0 || height == 0)
            {
                return;
            }

            if (_scalerCtx == null || _scalerWidth != width || _scalerHeight != height)
            {
                FreeScaler();

                _scalerCtx = ffmpeg.sws_getContext(
                    width, height, AVPixelFormat.AV_PIX_FMT_YUV420P,
                    width, height, AVPixelFormat.AV_PIX_FMT_RGBA, 0, null, null, null);

                _scalerWidth  = width;
                _scalerHeight = height;
            }
        }

        public static void Uninitialize()
        {
            if (IsInitialized)
            {
                ffmpeg.av_frame_unref(_frame);
                ffmpeg.av_free(_frame);
                ffmpeg.avcodec_close(_context);

                FreeScaler();

                IsInitialized = false;
            }
        }

        private static void FreeScaler()
        {
            if (_scalerCtx != null)
            {
                ffmpeg.sws_freeContext(_scalerCtx);

                _scalerCtx = null;
            }
        }
    }
}