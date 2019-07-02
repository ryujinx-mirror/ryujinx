using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Memory;
using Ryujinx.Graphics.Texture;
using Ryujinx.Graphics.Vic;
using System;

namespace Ryujinx.Graphics.VDec
{
    unsafe class VideoDecoder
    {
        private NvGpu _gpu;

        private H264Decoder _h264Decoder;
        private Vp9Decoder  _vp9Decoder;

        private VideoCodec _currentVideoCodec;

        private long _decoderContextAddress;
        private long _frameDataAddress;
        private long _vpxCurrLumaAddress;
        private long _vpxRef0LumaAddress;
        private long _vpxRef1LumaAddress;
        private long _vpxRef2LumaAddress;
        private long _vpxCurrChromaAddress;
        private long _vpxRef0ChromaAddress;
        private long _vpxRef1ChromaAddress;
        private long _vpxRef2ChromaAddress;
        private long _vpxProbTablesAddress;

        public VideoDecoder(NvGpu gpu)
        {
            _gpu = gpu;

            _h264Decoder = new H264Decoder();
            _vp9Decoder  = new Vp9Decoder();
        }

        public void Process(NvGpuVmm vmm, int methodOffset, int[] arguments)
        {
            VideoDecoderMeth method = (VideoDecoderMeth)methodOffset;

            switch (method)
            {
                case VideoDecoderMeth.SetVideoCodec:        SetVideoCodec       (vmm, arguments); break;
                case VideoDecoderMeth.Execute:              Execute             (vmm, arguments); break;
                case VideoDecoderMeth.SetDecoderCtxAddr:    SetDecoderCtxAddr   (vmm, arguments); break;
                case VideoDecoderMeth.SetFrameDataAddr:     SetFrameDataAddr    (vmm, arguments); break;
                case VideoDecoderMeth.SetVpxCurrLumaAddr:   SetVpxCurrLumaAddr  (vmm, arguments); break;
                case VideoDecoderMeth.SetVpxRef0LumaAddr:   SetVpxRef0LumaAddr  (vmm, arguments); break;
                case VideoDecoderMeth.SetVpxRef1LumaAddr:   SetVpxRef1LumaAddr  (vmm, arguments); break;
                case VideoDecoderMeth.SetVpxRef2LumaAddr:   SetVpxRef2LumaAddr  (vmm, arguments); break;
                case VideoDecoderMeth.SetVpxCurrChromaAddr: SetVpxCurrChromaAddr(vmm, arguments); break;
                case VideoDecoderMeth.SetVpxRef0ChromaAddr: SetVpxRef0ChromaAddr(vmm, arguments); break;
                case VideoDecoderMeth.SetVpxRef1ChromaAddr: SetVpxRef1ChromaAddr(vmm, arguments); break;
                case VideoDecoderMeth.SetVpxRef2ChromaAddr: SetVpxRef2ChromaAddr(vmm, arguments); break;
                case VideoDecoderMeth.SetVpxProbTablesAddr: SetVpxProbTablesAddr(vmm, arguments); break;
            }
        }

        private void SetVideoCodec(NvGpuVmm vmm, int[] arguments)
        {
            _currentVideoCodec = (VideoCodec)arguments[0];
        }

        private void Execute(NvGpuVmm vmm, int[] arguments)
        {
            if (_currentVideoCodec == VideoCodec.H264)
            {
                int frameDataSize = vmm.ReadInt32(_decoderContextAddress + 0x48);

                H264ParameterSets Params = MemoryHelper.Read<H264ParameterSets>(vmm.Memory, vmm.GetPhysicalAddress(_decoderContextAddress + 0x58));

                H264Matrices matrices = new H264Matrices()
                {
                    ScalingMatrix4 = vmm.ReadBytes(_decoderContextAddress + 0x1c0, 6 * 16),
                    ScalingMatrix8 = vmm.ReadBytes(_decoderContextAddress + 0x220, 2 * 64)
                };

                byte[] frameData = vmm.ReadBytes(_frameDataAddress, frameDataSize);

                _h264Decoder.Decode(Params, matrices, frameData);
            }
            else if (_currentVideoCodec == VideoCodec.Vp9)
            {
                int frameDataSize = vmm.ReadInt32(_decoderContextAddress + 0x30);

                Vp9FrameKeys keys = new Vp9FrameKeys()
                {
                    CurrKey = vmm.GetPhysicalAddress(_vpxCurrLumaAddress),
                    Ref0Key = vmm.GetPhysicalAddress(_vpxRef0LumaAddress),
                    Ref1Key = vmm.GetPhysicalAddress(_vpxRef1LumaAddress),
                    Ref2Key = vmm.GetPhysicalAddress(_vpxRef2LumaAddress)
                };

                Vp9FrameHeader header = MemoryHelper.Read<Vp9FrameHeader>(vmm.Memory, vmm.GetPhysicalAddress(_decoderContextAddress + 0x48));

                Vp9ProbabilityTables probs = new Vp9ProbabilityTables()
                {
                    SegmentationTreeProbs = vmm.ReadBytes(_vpxProbTablesAddress + 0x387, 0x7),
                    SegmentationPredProbs = vmm.ReadBytes(_vpxProbTablesAddress + 0x38e, 0x3),
                    Tx8x8Probs            = vmm.ReadBytes(_vpxProbTablesAddress + 0x470, 0x2),
                    Tx16x16Probs          = vmm.ReadBytes(_vpxProbTablesAddress + 0x472, 0x4),
                    Tx32x32Probs          = vmm.ReadBytes(_vpxProbTablesAddress + 0x476, 0x6),
                    CoefProbs             = vmm.ReadBytes(_vpxProbTablesAddress + 0x5a0, 0x900),
                    SkipProbs             = vmm.ReadBytes(_vpxProbTablesAddress + 0x537, 0x3),
                    InterModeProbs        = vmm.ReadBytes(_vpxProbTablesAddress + 0x400, 0x1c),
                    InterpFilterProbs     = vmm.ReadBytes(_vpxProbTablesAddress + 0x52a, 0x8),
                    IsInterProbs          = vmm.ReadBytes(_vpxProbTablesAddress + 0x41c, 0x4),
                    CompModeProbs         = vmm.ReadBytes(_vpxProbTablesAddress + 0x532, 0x5),
                    SingleRefProbs        = vmm.ReadBytes(_vpxProbTablesAddress + 0x580, 0xa),
                    CompRefProbs          = vmm.ReadBytes(_vpxProbTablesAddress + 0x58a, 0x5),
                    YModeProbs0           = vmm.ReadBytes(_vpxProbTablesAddress + 0x480, 0x20),
                    YModeProbs1           = vmm.ReadBytes(_vpxProbTablesAddress + 0x47c, 0x4),
                    PartitionProbs        = vmm.ReadBytes(_vpxProbTablesAddress + 0x4e0, 0x40),
                    MvJointProbs          = vmm.ReadBytes(_vpxProbTablesAddress + 0x53b, 0x3),
                    MvSignProbs           = vmm.ReadBytes(_vpxProbTablesAddress + 0x53e, 0x3),
                    MvClassProbs          = vmm.ReadBytes(_vpxProbTablesAddress + 0x54c, 0x14),
                    MvClass0BitProbs      = vmm.ReadBytes(_vpxProbTablesAddress + 0x540, 0x3),
                    MvBitsProbs           = vmm.ReadBytes(_vpxProbTablesAddress + 0x56c, 0x14),
                    MvClass0FrProbs       = vmm.ReadBytes(_vpxProbTablesAddress + 0x560, 0xc),
                    MvFrProbs             = vmm.ReadBytes(_vpxProbTablesAddress + 0x542, 0x6),
                    MvClass0HpProbs       = vmm.ReadBytes(_vpxProbTablesAddress + 0x548, 0x2),
                    MvHpProbs             = vmm.ReadBytes(_vpxProbTablesAddress + 0x54a, 0x2)
                };

                byte[] frameData = vmm.ReadBytes(_frameDataAddress, frameDataSize);

                _vp9Decoder.Decode(keys, header, probs, frameData);
            }
            else
            {
                ThrowUnimplementedCodec();
            }
        }

        private void SetDecoderCtxAddr(NvGpuVmm vmm, int[] arguments)
        {
            _decoderContextAddress = GetAddress(arguments);
        }

        private void SetFrameDataAddr(NvGpuVmm vmm, int[] arguments)
        {
            _frameDataAddress = GetAddress(arguments);
        }

        private void SetVpxCurrLumaAddr(NvGpuVmm vmm, int[] arguments)
        {
            _vpxCurrLumaAddress = GetAddress(arguments);
        }

        private void SetVpxRef0LumaAddr(NvGpuVmm vmm, int[] arguments)
        {
            _vpxRef0LumaAddress = GetAddress(arguments);
        }

        private void SetVpxRef1LumaAddr(NvGpuVmm vmm, int[] arguments)
        {
            _vpxRef1LumaAddress = GetAddress(arguments);
        }

        private void SetVpxRef2LumaAddr(NvGpuVmm vmm, int[] arguments)
        {
            _vpxRef2LumaAddress = GetAddress(arguments);
        }

        private void SetVpxCurrChromaAddr(NvGpuVmm vmm, int[] arguments)
        {
            _vpxCurrChromaAddress = GetAddress(arguments);
        }

        private void SetVpxRef0ChromaAddr(NvGpuVmm vmm, int[] arguments)
        {
            _vpxRef0ChromaAddress = GetAddress(arguments);
        }

        private void SetVpxRef1ChromaAddr(NvGpuVmm vmm, int[] arguments)
        {
            _vpxRef1ChromaAddress = GetAddress(arguments);
        }

        private void SetVpxRef2ChromaAddr(NvGpuVmm vmm, int[] arguments)
        {
            _vpxRef2ChromaAddress = GetAddress(arguments);
        }

        private void SetVpxProbTablesAddr(NvGpuVmm vmm, int[] arguments)
        {
            _vpxProbTablesAddress = GetAddress(arguments);
        }

        private static long GetAddress(int[] arguments)
        {
            return (long)(uint)arguments[0] << 8;
        }

        internal void CopyPlanes(NvGpuVmm vmm, SurfaceOutputConfig outputConfig)
        {
            switch (outputConfig.PixelFormat)
            {
                case SurfacePixelFormat.Rgba8:   CopyPlanesRgba8  (vmm, outputConfig); break;
                case SurfacePixelFormat.Yuv420P: CopyPlanesYuv420P(vmm, outputConfig); break;

                default: ThrowUnimplementedPixelFormat(outputConfig.PixelFormat); break;
            }
        }

        private void CopyPlanesRgba8(NvGpuVmm vmm, SurfaceOutputConfig outputConfig)
        {
            FFmpegFrame frame = FFmpegWrapper.GetFrameRgba();

            if ((frame.Width | frame.Height) == 0)
            {
                return;
            }

            GalImage image = new GalImage(
                outputConfig.SurfaceWidth,
                outputConfig.SurfaceHeight, 1, 1, 1,
                outputConfig.GobBlockHeight, 1,
                GalMemoryLayout.BlockLinear,
                GalImageFormat.Rgba8 | GalImageFormat.Unorm,
                GalTextureTarget.TwoD);

            ImageUtils.WriteTexture(vmm, image, vmm.GetPhysicalAddress(outputConfig.SurfaceLumaAddress), frame.Data);
        }

        private void CopyPlanesYuv420P(NvGpuVmm vmm, SurfaceOutputConfig outputConfig)
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
                    vmm.WriteByte(outputConfig.SurfaceLumaAddress + dst + offset, *(frame.LumaPtr + src + offset));
                }
            }

            // Copy chroma data from both channels with interleaving.
            for (int y = 0; y < halfHeight; y++)
            {
                int src = y * halfSrcWidth;
                int dst = y * alignedWidth;

                for (int x = 0; x < halfWidth; x++)
                {
                    vmm.WriteByte(outputConfig.SurfaceChromaUAddress + dst + x * 2 + 0, *(frame.ChromaBPtr + src + x));
                    vmm.WriteByte(outputConfig.SurfaceChromaUAddress + dst + x * 2 + 1, *(frame.ChromaRPtr + src + x));
                }
            }
        }

        private void ThrowUnimplementedCodec()
        {
            throw new NotImplementedException("Codec \"" + _currentVideoCodec + "\" is not supported!");
        }

        private void ThrowUnimplementedPixelFormat(SurfacePixelFormat pixelFormat)
        {
            throw new NotImplementedException("Pixel format \"" + pixelFormat + "\" is not supported!");
        }
    }
}