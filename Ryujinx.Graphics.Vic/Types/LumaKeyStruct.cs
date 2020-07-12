namespace Ryujinx.Graphics.Vic.Types
{
    struct LumaKeyStruct
    {
        private long _word0;
        private long _word1;

        public int LumaCoeff0 => _word0.Extract(0, 20);
        public int LumaCoeff1 => _word0.Extract(20, 20);
        public int LumaCoeff2 => _word0.Extract(40, 20);
        public int LumaRShift => _word0.Extract(60, 4);
        public int LumaCoeff3 => _word1.Extract(64, 20);
        public int LumaKeyLower => _word1.Extract(84, 10);
        public int LumaKeyUpper => _word1.Extract(94, 10);
        public bool LumaKeyEnabled => _word1.Extract(104);
    }
}
