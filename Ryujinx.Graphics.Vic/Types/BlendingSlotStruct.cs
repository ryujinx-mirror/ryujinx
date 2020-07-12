namespace Ryujinx.Graphics.Vic.Types
{
    struct BlendingSlotStruct
    {
        private long _word0;
        private long _word1;

        public int AlphaK1 => _word0.Extract(0, 10);
        public int AlphaK2 => _word0.Extract(16, 10);
        public int SrcFactCMatchSelect => _word0.Extract(32, 3);
        public int DstFactCMatchSelect => _word0.Extract(36, 3);
        public int SrcFactAMatchSelect => _word0.Extract(40, 3);
        public int DstFactAMatchSelect => _word0.Extract(44, 3);
        public int OverrideR => _word1.Extract(66, 10);
        public int OverrideG => _word1.Extract(76, 10);
        public int OverrideB => _word1.Extract(86, 10);
        public int OverrideA => _word1.Extract(96, 10);
        public bool UseOverrideR => _word1.Extract(108);
        public bool UseOverrideG => _word1.Extract(109);
        public bool UseOverrideB => _word1.Extract(110);
        public bool UseOverrideA => _word1.Extract(111);
        public bool MaskR => _word1.Extract(112);
        public bool MaskG => _word1.Extract(113);
        public bool MaskB => _word1.Extract(114);
        public bool MaskA => _word1.Extract(115);
    }
}
