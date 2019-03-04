namespace Ryujinx.Graphics.Gal.Shader
{
    class ShaderIrOperPred : ShaderIrNode
    {
        public const int UnusedIndex  = 0x7;
        public const int NeverExecute = 0xf;

        public bool IsConst => Index >= UnusedIndex;

        public int Index { get; set; }

        public ShaderIrOperPred(int index)
        {
            Index = index;
        }
    }
}