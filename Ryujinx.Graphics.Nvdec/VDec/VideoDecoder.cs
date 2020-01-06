using Ryujinx.Graphics.Gpu;
using Ryujinx.Graphics.Vic;
using System;

namespace Ryujinx.Graphics.VDec
{
    unsafe class VideoDecoder
    {
        private H264Decoder _h264Decoder;
        private Vp9Decoder  _vp9Decoder;

        private VideoCodec _currentVideoCodec;

        private ulong _decoderContextAddress;
        private ulong _frameDataAddress;
        private ulong _vpxCurrLumaAddress;
        private ulong _vpxRef0LumaAddress;
        private ulong _vpxRef1LumaAddress;
        private ulong _vpxRef2LumaAddress;
        private ulong _vpxCurrChromaAddress;
        private ulong _vpxRef0ChromaAddress;
        private ulong _vpxRef1ChromaAddress;
        private ulong _vpxRef2ChromaAddress;
        private ulong _vpxProbTablesAddress;

        public VideoDecoder()
        {
            _h264Decoder = new H264Decoder();
            _vp9Decoder  = new Vp9Decoder();
        }

        public void Process(GpuContext gpu, int methodOffset, int[] arguments)
        {
            VideoDecoderMeth method = (VideoDecoderMeth)methodOffset;

            switch (method)
            {
                case VideoDecoderMeth.SetVideoCodec:        SetVideoCodec(arguments);        break;
                case VideoDecoderMeth.Execute:              Execute(gpu);                    break;
                case VideoDecoderMeth.SetDecoderCtxAddr:    SetDecoderCtxAddr(arguments);    break;
                case VideoDecoderMeth.SetFrameDataAddr:     SetFrameDataAddr(arguments);     break;
                case VideoDecoderMeth.SetVpxCurrLumaAddr:   SetVpxCurrLumaAddr(arguments);   break;
                case VideoDecoderMeth.SetVpxRef0LumaAddr:   SetVpxRef0LumaAddr(arguments);   break;
                case VideoDecoderMeth.SetVpxRef1LumaAddr:   SetVpxRef1LumaAddr(arguments);   break;
                case VideoDecoderMeth.SetVpxRef2LumaAddr:   SetVpxRef2LumaAddr(arguments);   break;
                case VideoDecoderMeth.SetVpxCurrChromaAddr: SetVpxCurrChromaAddr(arguments); break;
                case VideoDecoderMeth.SetVpxRef0ChromaAddr: SetVpxRef0ChromaAddr(arguments); break;
                case VideoDecoderMeth.SetVpxRef1ChromaAddr: SetVpxRef1ChromaAddr(arguments); break;
                case VideoDecoderMeth.SetVpxRef2ChromaAddr: SetVpxRef2ChromaAddr(arguments); break;
                case VideoDecoderMeth.SetVpxProbTablesAddr: SetVpxProbTablesAddr(arguments); break;
            }
        }

        private void SetVideoCodec(int[] arguments)
        {
            _currentVideoCodec = (VideoCodec)arguments[0];
        }

        private void Execute(GpuContext gpu)
        {
            if (_currentVideoCodec == VideoCodec.H264)
            {
                int frameDataSize = gpu.MemoryAccessor.ReadInt32(_decoderContextAddress + 0x48);

                H264ParameterSets Params = gpu.MemoryAccessor.Read<H264ParameterSets>(_decoderContextAddress + 0x58);

                H264Matrices matrices = new H264Matrices()
                {
                    ScalingMatrix4 = gpu.MemoryAccessor.ReadBytes(_decoderContextAddress + 0x1c0, 6 * 16),
                    ScalingMatrix8 = gpu.MemoryAccessor.ReadBytes(_decoderContextAddress + 0x220, 2 * 64)
                };

                byte[] frameData = gpu.MemoryAccessor.ReadBytes(_frameDataAddress, (ulong)frameDataSize);

                _h264Decoder.Decode(Params, matrices, frameData);
            }
            else if (_currentVideoCodec == VideoCodec.Vp9)
            {
                int frameDataSize = gpu.MemoryAccessor.ReadInt32(_decoderContextAddress + 0x30);

                Vp9FrameKeys keys = new Vp9FrameKeys()
                {
                    CurrKey = (long)gpu.MemoryManager.Translate(_vpxCurrLumaAddress),
                    Ref0Key = (long)gpu.MemoryManager.Translate(_vpxRef0LumaAddress),
                    Ref1Key = (long)gpu.MemoryManager.Translate(_vpxRef1LumaAddress),
                    Ref2Key = (long)gpu.MemoryManager.Translate(_vpxRef2LumaAddress)
                };

                Vp9FrameHeader header = gpu.MemoryAccessor.Read<Vp9FrameHeader>(_decoderContextAddress + 0x48);

                Vp9ProbabilityTables probs = new Vp9ProbabilityTables()
                {
                    SegmentationTreeProbs = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x387, 0x7),
                    SegmentationPredProbs = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x38e, 0x3),
                    Tx8x8Probs            = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x470, 0x2),
                    Tx16x16Probs          = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x472, 0x4),
                    Tx32x32Probs          = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x476, 0x6),
                    CoefProbs             = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x5a0, 0x900),
                    SkipProbs             = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x537, 0x3),
                    InterModeProbs        = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x400, 0x1c),
                    InterpFilterProbs     = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x52a, 0x8),
                    IsInterProbs          = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x41c, 0x4),
                    CompModeProbs         = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x532, 0x5),
                    SingleRefProbs        = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x580, 0xa),
                    CompRefProbs          = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x58a, 0x5),
                    YModeProbs0           = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x480, 0x20),
                    YModeProbs1           = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x47c, 0x4),
                    PartitionProbs        = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x4e0, 0x40),
                    MvJointProbs          = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x53b, 0x3),
                    MvSignProbs           = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x53e, 0x3),
                    MvClassProbs          = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x54c, 0x14),
                    MvClass0BitProbs      = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x540, 0x3),
                    MvBitsProbs           = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x56c, 0x14),
                    MvClass0FrProbs       = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x560, 0xc),
                    MvFrProbs             = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x542, 0x6),
                    MvClass0HpProbs       = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x548, 0x2),
                    MvHpProbs             = gpu.MemoryAccessor.ReadBytes(_vpxProbTablesAddress + 0x54a, 0x2)
                };

                byte[] frameData = gpu.MemoryAccessor.ReadBytes(_frameDataAddress, (ulong)frameDataSize);

                _vp9Decoder.Decode(keys, header, probs, frameData);
            }
            else
            {
                ThrowUnimplementedCodec();
            }
        }

        private void SetDecoderCtxAddr(int[] arguments)
        {
            _decoderContextAddress = GetAddress(arguments);
        }

        private void SetFrameDataAddr(int[] arguments)
        {
            _frameDataAddress = GetAddress(arguments);
        }

        private void SetVpxCurrLumaAddr(int[] arguments)
        {
            _vpxCurrLumaAddress = GetAddress(arguments);
        }

        private void SetVpxRef0LumaAddr(int[] arguments)
        {
            _vpxRef0LumaAddress = GetAddress(arguments);
        }

        private void SetVpxRef1LumaAddr(int[] arguments)
        {
            _vpxRef1LumaAddress = GetAddress(arguments);
        }

        private void SetVpxRef2LumaAddr(int[] arguments)
        {
            _vpxRef2LumaAddress = GetAddress(arguments);
        }

        private void SetVpxCurrChromaAddr(int[] arguments)
        {
            _vpxCurrChromaAddress = GetAddress(arguments);
        }

        private void SetVpxRef0ChromaAddr(int[] arguments)
        {
            _vpxRef0ChromaAddress = GetAddress(arguments);
        }

        private void SetVpxRef1ChromaAddr(int[] arguments)
        {
            _vpxRef1ChromaAddress = GetAddress(arguments);
        }

        private void SetVpxRef2ChromaAddr(int[] arguments)
        {
            _vpxRef2ChromaAddress = GetAddress(arguments);
        }

        private void SetVpxProbTablesAddr(int[] arguments)
        {
            _vpxProbTablesAddress = GetAddress(arguments);
        }

        private static ulong GetAddress(int[] arguments)
        {
            return (ulong)(uint)arguments[0] << 8;
        }

        internal void CopyPlanes(GpuContext gpu, SurfaceOutputConfig outputConfig)
        {
            switch (outputConfig.PixelFormat)
            {
                case SurfacePixelFormat.Rgba8:   CopyPlanesRgba8  (gpu, outputConfig); break;
                case SurfacePixelFormat.Yuv420P: CopyPlanesYuv420P(gpu, outputConfig); break;

                default: ThrowUnimplementedPixelFormat(outputConfig.PixelFormat); break;
            }
        }

        private void CopyPlanesRgba8(GpuContext gpu, SurfaceOutputConfig outputConfig)
        {
            FFmpegFrame frame = FFmpegWrapper.GetFrameRgba();

            if ((frame.Width | frame.Height) == 0)
            {
                return;
            }

            throw new NotImplementedException();
        }

        private void CopyPlanesYuv420P(GpuContext gpu, SurfaceOutputConfig outputConfig)
        {
            FFmpegFrame frame = FFmpegWrapper.GetFrame();

            if ((frame.Width | frame.Height) == 0)
            {
                return;
            }

            int halfSrcWidth = frame.Width / 2;

            int halfWidth  = frame.Width  / 2;
            int halfHeight = frame.Height / 2;

            int alignedWidth = (outputConfig.SurfaceWidth + 0xff) & ~0xff;

            for (int y = 0; y < frame.Height; y++)
            {
                int src = y * frame.Width;
                int dst = y * alignedWidth;

                int size = frame.Width;

                for (int offset = 0; offset < size; offset++)
                {
                    gpu.MemoryAccessor.WriteByte(outputConfig.SurfaceLumaAddress + (ulong)dst + (ulong)offset, *(frame.LumaPtr + src + offset));
                }
            }

            // Copy chroma data from both channels with interleaving.
            for (int y = 0; y < halfHeight; y++)
            {
                int src = y * halfSrcWidth;
                int dst = y * alignedWidth;

                for (int x = 0; x < halfWidth; x++)
                {
                    gpu.MemoryAccessor.WriteByte(outputConfig.SurfaceChromaUAddress + (ulong)dst + (ulong)x * 2 + 0, *(frame.ChromaBPtr + src + x));
                    gpu.MemoryAccessor.WriteByte(outputConfig.SurfaceChromaUAddress + (ulong)dst + (ulong)x * 2 + 1, *(frame.ChromaRPtr + src + x));
                }
            }
        }

        private void ThrowUnimplementedCodec()
        {
            throw new NotImplementedException($"Codec \"{_currentVideoCodec}\" is not supported!");
        }

        private void ThrowUnimplementedPixelFormat(SurfacePixelFormat pixelFormat)
        {
            throw new NotImplementedException($"Pixel format \"{pixelFormat}\" is not supported!");
        }
    }
}