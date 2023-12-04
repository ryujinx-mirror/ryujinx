using Ryujinx.Common.Utilities;

namespace Ryujinx.Graphics.Vic.Types
{
    readonly struct SlotConfig
    {
        private readonly long _word0;
        private readonly long _word1;
        private readonly long _word2;
        private readonly long _word3;
        private readonly long _word4;
        private readonly long _word5;
        private readonly long _word6;
#pragma warning disable IDE0051 // Remove unused private member
        private readonly long _word7;
#pragma warning restore IDE0051

        public bool SlotEnable => _word0.Extract(0);
        public bool DeNoise => _word0.Extract(1);
        public bool AdvancedDenoise => _word0.Extract(2);
        public bool CadenceDetect => _word0.Extract(3);
        public bool MotionMap => _word0.Extract(4);
        public bool MMapCombine => _word0.Extract(5);
        public bool IsEven => _word0.Extract(6);
        public bool ChromaEven => _word0.Extract(7);
        public bool CurrentFieldEnable => _word0.Extract(8);
        public bool PrevFieldEnable => _word0.Extract(9);
        public bool NextFieldEnable => _word0.Extract(10);
        public bool NextNrFieldEnable => _word0.Extract(11);
        public bool CurMotionFieldEnable => _word0.Extract(12);
        public bool PrevMotionFieldEnable => _word0.Extract(13);
        public bool PpMotionFieldEnable => _word0.Extract(14);
        public bool CombMotionFieldEnable => _word0.Extract(15);
        public FrameFormat FrameFormat => (FrameFormat)_word0.Extract(16, 4);
        public int FilterLengthY => (int)_word0.Extract(20, 2);
        public int FilterLengthX => (int)_word0.Extract(22, 2);
        public int Panoramic => (int)_word0.Extract(24, 12);
        public int DetailFltClamp => (int)_word0.Extract(58, 6);
        public int FilterNoise => (int)_word1.Extract(64, 10);
        public int FilterDetail => (int)_word1.Extract(74, 10);
        public int ChromaNoise => (int)_word1.Extract(84, 10);
        public int ChromaDetail => (int)_word1.Extract(94, 10);
        public DeinterlaceMode DeinterlaceMode => (DeinterlaceMode)_word1.Extract(104, 4);
        public int MotionAccumWeight => (int)_word1.Extract(108, 3);
        public int NoiseIir => (int)_word1.Extract(111, 11);
        public int LightLevel => (int)_word1.Extract(122, 4);
        public int SoftClampLow => (int)_word2.Extract(128, 10);
        public int SoftClampHigh => (int)_word2.Extract(138, 10);
        public int PlanarAlpha => (int)_word2.Extract(160, 10);
        public bool ConstantAlpha => _word2.Extract(170);
        public int StereoInterleave => (int)_word2.Extract(171, 3);
        public bool ClipEnabled => _word2.Extract(174);
        public int ClearRectMask => (int)_word2.Extract(175, 8);
        public int DegammaMode => (int)_word2.Extract(183, 2);
        public bool DecompressEnable => _word2.Extract(186);
        public int DecompressCtbCount => (int)_word3.Extract(192, 8);
        public int DecompressZbcColor => (int)_word3.Extract(200, 32);
        public int SourceRectLeft => (int)_word4.Extract(256, 30);
        public int SourceRectRight => (int)_word4.Extract(288, 30);
        public int SourceRectTop => (int)_word5.Extract(320, 30);
        public int SourceRectBottom => (int)_word5.Extract(352, 30);
        public int DstRectLeft => (int)_word6.Extract(384, 14);
        public int DstRectRight => (int)_word6.Extract(400, 14);
        public int DstRectTop => (int)_word6.Extract(416, 14);
        public int DstRectBottom => (int)_word6.Extract(432, 14);
    }
}
