namespace Ryujinx.Graphics.Shader
{
    public struct TextureDescriptor
    {
        public string Name { get; }

        public int HandleIndex { get; }

        public bool IsBindless { get; }

        public int CbufSlot   { get; }
        public int CbufOffset { get; }

        public TextureDescriptor(string name, int hIndex)
        {
            Name        = name;
            HandleIndex = hIndex;

            IsBindless = false;

            CbufSlot   = 0;
            CbufOffset = 0;
        }

        public TextureDescriptor(string name, int cbufSlot, int cbufOffset)
        {
            Name        = name;
            HandleIndex = 0;

            IsBindless = true;

            CbufSlot   = cbufSlot;
            CbufOffset = cbufOffset;
        }
    }
}