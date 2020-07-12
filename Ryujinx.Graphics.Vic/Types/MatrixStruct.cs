namespace Ryujinx.Graphics.Vic.Types
{
    struct MatrixStruct
    {
        private long _word0;
        private long _word1;
        private long _word2;
        private long _word3;

        public int MatrixCoeff00 => _word0.ExtractSx(0, 20);
        public int MatrixCoeff10 => _word0.ExtractSx(20, 20);
        public int MatrixCoeff20 => _word0.ExtractSx(40, 20);
        public int MatrixRShift => _word0.Extract(60, 4);
        public int MatrixCoeff01 => _word1.ExtractSx(64, 20);
        public int MatrixCoeff11 => _word1.ExtractSx(84, 20);
        public int MatrixCoeff21 => _word1.ExtractSx(104, 20);
        public bool MatrixEnable => _word1.Extract(127);
        public int MatrixCoeff02 => _word2.ExtractSx(128, 20);
        public int MatrixCoeff12 => _word2.ExtractSx(148, 20);
        public int MatrixCoeff22 => _word2.ExtractSx(168, 20);
        public int MatrixCoeff03 => _word3.ExtractSx(192, 20);
        public int MatrixCoeff13 => _word3.ExtractSx(212, 20);
        public int MatrixCoeff23 => _word3.ExtractSx(232, 20);
    }
}
