using System.IO;

namespace Ryujinx.Graphics.VDec
{
    class H264Decoder
    {
        private int    _log2MaxPicOrderCntLsbMinus4;
        private bool   _deltaPicOrderAlwaysZeroFlag;
        private bool   _frameMbsOnlyFlag;
        private int    _picWidthInMbs;
        private int    _picHeightInMapUnits;
        private bool   _entropyCodingModeFlag;
        private bool   _bottomFieldPicOrderInFramePresentFlag;
        private int    _numRefIdxL0DefaultActiveMinus1;
        private int    _numRefIdxL1DefaultActiveMinus1;
        private bool   _deblockingFilterControlPresentFlag;
        private bool   _redundantPicCntPresentFlag;
        private bool   _transform8x8ModeFlag;
        private bool   _mbAdaptiveFrameFieldFlag;
        private bool   _direct8x8InferenceFlag;
        private bool   _weightedPredFlag;
        private bool   _constrainedIntraPredFlag;
        private bool   _fieldPicFlag;
        private bool   _bottomFieldFlag;
        private int    _log2MaxFrameNumMinus4;
        private int    _chromaFormatIdc;
        private int    _picOrderCntType;
        private int    _picInitQpMinus26;
        private int    _chromaQpIndexOffset;
        private int    _chromaQpIndexOffset2;
        private int    _weightedBipredIdc;
        private int    _frameNumber;
        private byte[] _scalingMatrix4;
        private byte[] _scalingMatrix8;

        public void Decode(H264ParameterSets Params, H264Matrices matrices, byte[] frameData)
        {
            _log2MaxPicOrderCntLsbMinus4           = Params.Log2MaxPicOrderCntLsbMinus4;
            _deltaPicOrderAlwaysZeroFlag           = Params.DeltaPicOrderAlwaysZeroFlag;
            _frameMbsOnlyFlag                      = Params.FrameMbsOnlyFlag;
            _picWidthInMbs                         = Params.PicWidthInMbs;
            _picHeightInMapUnits                   = Params.PicHeightInMapUnits;
            _entropyCodingModeFlag                 = Params.EntropyCodingModeFlag;
            _bottomFieldPicOrderInFramePresentFlag = Params.BottomFieldPicOrderInFramePresentFlag;
            _numRefIdxL0DefaultActiveMinus1        = Params.NumRefIdxL0DefaultActiveMinus1;
            _numRefIdxL1DefaultActiveMinus1        = Params.NumRefIdxL1DefaultActiveMinus1;
            _deblockingFilterControlPresentFlag    = Params.DeblockingFilterControlPresentFlag;
            _redundantPicCntPresentFlag            = Params.RedundantPicCntPresentFlag;
            _transform8x8ModeFlag                  = Params.Transform8x8ModeFlag;

            _mbAdaptiveFrameFieldFlag = ((Params.Flags >> 0) & 1) != 0;
            _direct8x8InferenceFlag   = ((Params.Flags >> 1) & 1) != 0;
            _weightedPredFlag         = ((Params.Flags >> 2) & 1) != 0;
            _constrainedIntraPredFlag = ((Params.Flags >> 3) & 1) != 0;
            _fieldPicFlag             = ((Params.Flags >> 5) & 1) != 0;
            _bottomFieldFlag          = ((Params.Flags >> 6) & 1) != 0;

            _log2MaxFrameNumMinus4  = (int)(Params.Flags >> 8)  & 0xf;
            _chromaFormatIdc        = (int)(Params.Flags >> 12) & 0x3;
            _picOrderCntType        = (int)(Params.Flags >> 14) & 0x3;
            _picInitQpMinus26       = (int)(Params.Flags >> 16) & 0x3f;
            _chromaQpIndexOffset    = (int)(Params.Flags >> 22) & 0x1f;
            _chromaQpIndexOffset2   = (int)(Params.Flags >> 27) & 0x1f;
            _weightedBipredIdc      = (int)(Params.Flags >> 32) & 0x3;
            _frameNumber            = (int)(Params.Flags >> 46) & 0x1ffff;

            _picInitQpMinus26     = (_picInitQpMinus26     << 26) >> 26;
            _chromaQpIndexOffset  = (_chromaQpIndexOffset  << 27) >> 27;
            _chromaQpIndexOffset2 = (_chromaQpIndexOffset2 << 27) >> 27;

            _scalingMatrix4 = matrices.ScalingMatrix4;
            _scalingMatrix8 = matrices.ScalingMatrix8;

            if (FFmpegWrapper.IsInitialized)
            {
                FFmpegWrapper.DecodeFrame(frameData);
            }
            else
            {
                FFmpegWrapper.H264Initialize();

                FFmpegWrapper.DecodeFrame(DecoderHelper.Combine(EncodeHeader(), frameData));
            }
        }

        private byte[] EncodeHeader()
        {
            using (MemoryStream data = new MemoryStream())
            {
                H264BitStreamWriter writer = new H264BitStreamWriter(data);

                //Sequence Parameter Set.
                writer.WriteU(1, 24);
                writer.WriteU(0, 1);
                writer.WriteU(3, 2);
                writer.WriteU(7, 5);
                writer.WriteU(100, 8);
                writer.WriteU(0, 8);
                writer.WriteU(31, 8);
                writer.WriteUe(0);
                writer.WriteUe(_chromaFormatIdc);

                if (_chromaFormatIdc == 3)
                {
                    writer.WriteBit(false);
                }

                writer.WriteUe(0);
                writer.WriteUe(0);
                writer.WriteBit(false);
                writer.WriteBit(false); //Scaling matrix present flag

                writer.WriteUe(_log2MaxFrameNumMinus4);
                writer.WriteUe(_picOrderCntType);

                if (_picOrderCntType == 0)
                {
                    writer.WriteUe(_log2MaxPicOrderCntLsbMinus4);
                }
                else if (_picOrderCntType == 1)
                {
                    writer.WriteBit(_deltaPicOrderAlwaysZeroFlag);

                    writer.WriteSe(0);
                    writer.WriteSe(0);
                    writer.WriteUe(0);
                }

                int picHeightInMbs = _picHeightInMapUnits / (_frameMbsOnlyFlag ? 1 : 2);

                writer.WriteUe(16);
                writer.WriteBit(false);
                writer.WriteUe(_picWidthInMbs - 1);
                writer.WriteUe(picHeightInMbs - 1);
                writer.WriteBit(_frameMbsOnlyFlag);

                if (!_frameMbsOnlyFlag)
                {
                    writer.WriteBit(_mbAdaptiveFrameFieldFlag);
                }

                writer.WriteBit(_direct8x8InferenceFlag);
                writer.WriteBit(false); //Frame cropping flag
                writer.WriteBit(false); //VUI parameter present flag

                writer.End();

                //Picture Parameter Set.
                writer.WriteU(1, 24);
                writer.WriteU(0, 1);
                writer.WriteU(3, 2);
                writer.WriteU(8, 5);

                writer.WriteUe(0);
                writer.WriteUe(0);

                writer.WriteBit(_entropyCodingModeFlag);
                writer.WriteBit(false);
                writer.WriteUe(0);
                writer.WriteUe(_numRefIdxL0DefaultActiveMinus1);
                writer.WriteUe(_numRefIdxL1DefaultActiveMinus1);
                writer.WriteBit(_weightedPredFlag);
                writer.WriteU(_weightedBipredIdc, 2);
                writer.WriteSe(_picInitQpMinus26);
                writer.WriteSe(0);
                writer.WriteSe(_chromaQpIndexOffset);
                writer.WriteBit(_deblockingFilterControlPresentFlag);
                writer.WriteBit(_constrainedIntraPredFlag);
                writer.WriteBit(_redundantPicCntPresentFlag);
                writer.WriteBit(_transform8x8ModeFlag);

                writer.WriteBit(true);

                for (int index = 0; index < 6; index++)
                {
                    writer.WriteBit(true);

                    WriteScalingList(writer, _scalingMatrix4, index * 16, 16);
                }

                if (_transform8x8ModeFlag)
                {
                    for (int index = 0; index < 2; index++)
                    {
                        writer.WriteBit(true);

                        WriteScalingList(writer, _scalingMatrix8, index * 64, 64);
                    }
                }

                writer.WriteSe(_chromaQpIndexOffset2);

                writer.End();

                return data.ToArray();
            }
        }

        //ZigZag LUTs from libavcodec.
        private static readonly byte[] ZigZagDirect = new byte[]
        {
            0,   1,  8, 16,  9,  2,  3, 10,
            17, 24, 32, 25, 18, 11,  4,  5,
            12, 19, 26, 33, 40, 48, 41, 34,
            27, 20, 13,  6,  7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36,
            29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46,
            53, 60, 61, 54, 47, 55, 62, 63
        };

        private static readonly byte[] ZigZagScan = new byte[]
        {
            0 + 0 * 4, 1 + 0 * 4, 0 + 1 * 4, 0 + 2 * 4,
            1 + 1 * 4, 2 + 0 * 4, 3 + 0 * 4, 2 + 1 * 4,
            1 + 2 * 4, 0 + 3 * 4, 1 + 3 * 4, 2 + 2 * 4,
            3 + 1 * 4, 3 + 2 * 4, 2 + 3 * 4, 3 + 3 * 4
        };

        private static void WriteScalingList(H264BitStreamWriter writer, byte[] list, int start, int count)
        {
            byte[] scan = count == 16 ? ZigZagScan : ZigZagDirect;

            int lastScale = 8;

            for (int index = 0; index < count; index++)
            {
                byte value = list[start + scan[index]];

                int deltaScale = value - lastScale;

                writer.WriteSe(deltaScale);

                lastScale = value;
            }
        }
    }
}