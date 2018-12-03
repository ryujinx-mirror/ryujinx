using System.IO;

namespace Ryujinx.Graphics.VDec
{
    class H264Decoder
    {
        private int    Log2MaxPicOrderCntLsbMinus4;
        private bool   DeltaPicOrderAlwaysZeroFlag;
        private bool   FrameMbsOnlyFlag;
        private int    PicWidthInMbs;
        private int    PicHeightInMapUnits;
        private bool   EntropyCodingModeFlag;
        private bool   BottomFieldPicOrderInFramePresentFlag;
        private int    NumRefIdxL0DefaultActiveMinus1;
        private int    NumRefIdxL1DefaultActiveMinus1;
        private bool   DeblockingFilterControlPresentFlag;
        private bool   RedundantPicCntPresentFlag;
        private bool   Transform8x8ModeFlag;
        private bool   MbAdaptiveFrameFieldFlag;
        private bool   Direct8x8InferenceFlag;
        private bool   WeightedPredFlag;
        private bool   ConstrainedIntraPredFlag;
        private bool   FieldPicFlag;
        private bool   BottomFieldFlag;
        private int    Log2MaxFrameNumMinus4;
        private int    ChromaFormatIdc;
        private int    PicOrderCntType;
        private int    PicInitQpMinus26;
        private int    ChromaQpIndexOffset;
        private int    ChromaQpIndexOffset2;
        private int    WeightedBipredIdc;
        private int    FrameNumber;
        private byte[] ScalingMatrix4;
        private byte[] ScalingMatrix8;

        public void Decode(H264ParameterSets Params, H264Matrices Matrices, byte[] FrameData)
        {
            Log2MaxPicOrderCntLsbMinus4           = Params.Log2MaxPicOrderCntLsbMinus4;
            DeltaPicOrderAlwaysZeroFlag           = Params.DeltaPicOrderAlwaysZeroFlag;
            FrameMbsOnlyFlag                      = Params.FrameMbsOnlyFlag;
            PicWidthInMbs                         = Params.PicWidthInMbs;
            PicHeightInMapUnits                   = Params.PicHeightInMapUnits;
            EntropyCodingModeFlag                 = Params.EntropyCodingModeFlag;
            BottomFieldPicOrderInFramePresentFlag = Params.BottomFieldPicOrderInFramePresentFlag;
            NumRefIdxL0DefaultActiveMinus1        = Params.NumRefIdxL0DefaultActiveMinus1;
            NumRefIdxL1DefaultActiveMinus1        = Params.NumRefIdxL1DefaultActiveMinus1;
            DeblockingFilterControlPresentFlag    = Params.DeblockingFilterControlPresentFlag;
            RedundantPicCntPresentFlag            = Params.RedundantPicCntPresentFlag;
            Transform8x8ModeFlag                  = Params.Transform8x8ModeFlag;

            MbAdaptiveFrameFieldFlag = ((Params.Flags >> 0) & 1) != 0;
            Direct8x8InferenceFlag   = ((Params.Flags >> 1) & 1) != 0;
            WeightedPredFlag         = ((Params.Flags >> 2) & 1) != 0;
            ConstrainedIntraPredFlag = ((Params.Flags >> 3) & 1) != 0;
            FieldPicFlag             = ((Params.Flags >> 5) & 1) != 0;
            BottomFieldFlag          = ((Params.Flags >> 6) & 1) != 0;

            Log2MaxFrameNumMinus4  = (int)(Params.Flags >> 8)  & 0xf;
            ChromaFormatIdc        = (int)(Params.Flags >> 12) & 0x3;
            PicOrderCntType        = (int)(Params.Flags >> 14) & 0x3;
            PicInitQpMinus26       = (int)(Params.Flags >> 16) & 0x3f;
            ChromaQpIndexOffset    = (int)(Params.Flags >> 22) & 0x1f;
            ChromaQpIndexOffset2   = (int)(Params.Flags >> 27) & 0x1f;
            WeightedBipredIdc      = (int)(Params.Flags >> 32) & 0x3;
            FrameNumber            = (int)(Params.Flags >> 46) & 0x1ffff;

            PicInitQpMinus26     = (PicInitQpMinus26     << 26) >> 26;
            ChromaQpIndexOffset  = (ChromaQpIndexOffset  << 27) >> 27;
            ChromaQpIndexOffset2 = (ChromaQpIndexOffset2 << 27) >> 27;

            ScalingMatrix4 = Matrices.ScalingMatrix4;
            ScalingMatrix8 = Matrices.ScalingMatrix8;

            if (FFmpegWrapper.IsInitialized)
            {
                FFmpegWrapper.DecodeFrame(FrameData);
            }
            else
            {
                FFmpegWrapper.H264Initialize();

                FFmpegWrapper.DecodeFrame(DecoderHelper.Combine(EncodeHeader(), FrameData));
            }
        }

        private byte[] EncodeHeader()
        {
            using (MemoryStream Data = new MemoryStream())
            {
                H264BitStreamWriter Writer = new H264BitStreamWriter(Data);

                //Sequence Parameter Set.
                Writer.WriteU(1, 24);
                Writer.WriteU(0, 1);
                Writer.WriteU(3, 2);
                Writer.WriteU(7, 5);
                Writer.WriteU(100, 8);
                Writer.WriteU(0, 8);
                Writer.WriteU(31, 8);
                Writer.WriteUe(0);
                Writer.WriteUe(ChromaFormatIdc);

                if (ChromaFormatIdc == 3)
                {
                    Writer.WriteBit(false);
                }

                Writer.WriteUe(0);
                Writer.WriteUe(0);
                Writer.WriteBit(false);
                Writer.WriteBit(false); //Scaling matrix present flag

                Writer.WriteUe(Log2MaxFrameNumMinus4);
                Writer.WriteUe(PicOrderCntType);

                if (PicOrderCntType == 0)
                {
                    Writer.WriteUe(Log2MaxPicOrderCntLsbMinus4);
                }
                else if (PicOrderCntType == 1)
                {
                    Writer.WriteBit(DeltaPicOrderAlwaysZeroFlag);

                    Writer.WriteSe(0);
                    Writer.WriteSe(0);
                    Writer.WriteUe(0);
                }

                int PicHeightInMbs = PicHeightInMapUnits / (FrameMbsOnlyFlag ? 1 : 2);

                Writer.WriteUe(16);
                Writer.WriteBit(false);
                Writer.WriteUe(PicWidthInMbs - 1);
                Writer.WriteUe(PicHeightInMbs - 1);
                Writer.WriteBit(FrameMbsOnlyFlag);

                if (!FrameMbsOnlyFlag)
                {
                    Writer.WriteBit(MbAdaptiveFrameFieldFlag);
                }

                Writer.WriteBit(Direct8x8InferenceFlag);
                Writer.WriteBit(false); //Frame cropping flag
                Writer.WriteBit(false); //VUI parameter present flag

                Writer.End();

                //Picture Parameter Set.
                Writer.WriteU(1, 24);
                Writer.WriteU(0, 1);
                Writer.WriteU(3, 2);
                Writer.WriteU(8, 5);

                Writer.WriteUe(0);
                Writer.WriteUe(0);

                Writer.WriteBit(EntropyCodingModeFlag);
                Writer.WriteBit(false);
                Writer.WriteUe(0);
                Writer.WriteUe(NumRefIdxL0DefaultActiveMinus1);
                Writer.WriteUe(NumRefIdxL1DefaultActiveMinus1);
                Writer.WriteBit(WeightedPredFlag);
                Writer.WriteU(WeightedBipredIdc, 2);
                Writer.WriteSe(PicInitQpMinus26);
                Writer.WriteSe(0);
                Writer.WriteSe(ChromaQpIndexOffset);
                Writer.WriteBit(DeblockingFilterControlPresentFlag);
                Writer.WriteBit(ConstrainedIntraPredFlag);
                Writer.WriteBit(RedundantPicCntPresentFlag);
                Writer.WriteBit(Transform8x8ModeFlag);

                Writer.WriteBit(true);

                for (int Index = 0; Index < 6; Index++)
                {
                    Writer.WriteBit(true);

                    WriteScalingList(Writer, ScalingMatrix4, Index * 16, 16);
                }

                if (Transform8x8ModeFlag)
                {
                    for (int Index = 0; Index < 2; Index++)
                    {
                        Writer.WriteBit(true);

                        WriteScalingList(Writer, ScalingMatrix8, Index * 64, 64);
                    }
                }

                Writer.WriteSe(ChromaQpIndexOffset2);

                Writer.End();

                return Data.ToArray();
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

        private static void WriteScalingList(H264BitStreamWriter Writer, byte[] List, int Start, int Count)
        {
            byte[] Scan = Count == 16 ? ZigZagScan : ZigZagDirect;

            int LastScale = 8;

            for (int Index = 0; Index < Count; Index++)
            {
                byte Value = List[Start + Scan[Index]];

                int DeltaScale = Value - LastScale;

                Writer.WriteSe(DeltaScale);

                LastScale = Value;
            }
        }
    }
}