using Ryujinx.Common.Utilities;

namespace Ryujinx.Graphics.Vic.Types
{
    readonly struct MatrixStruct
    {
        private readonly long _word0;
        private readonly long _word1;
        private readonly long _word2;
        private readonly long _word3;

        public int MatrixCoeff00 => (int)_word0.ExtractSx(0, 20);
        public int MatrixCoeff10 => (int)_word0.ExtractSx(20, 20);
        public int MatrixCoeff20 => (int)_word0.ExtractSx(40, 20);
        public int MatrixRShift => (int)_word0.Extract(60, 4);
        public int MatrixCoeff01 => (int)_word1.ExtractSx(64, 20);
        public int MatrixCoeff11 => (int)_word1.ExtractSx(84, 20);
        public int MatrixCoeff21 => (int)_word1.ExtractSx(104, 20);
        public bool MatrixEnable => _word1.Extract(127);
        public int MatrixCoeff02 => (int)_word2.ExtractSx(128, 20);
        public int MatrixCoeff12 => (int)_word2.ExtractSx(148, 20);
        public int MatrixCoeff22 => (int)_word2.ExtractSx(168, 20);
        public int MatrixCoeff03 => (int)_word3.ExtractSx(192, 20);
        public int MatrixCoeff13 => (int)_word3.ExtractSx(212, 20);
        public int MatrixCoeff23 => (int)_word3.ExtractSx(232, 20);
    }
}
