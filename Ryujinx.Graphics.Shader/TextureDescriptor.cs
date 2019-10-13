namespace Ryujinx.Graphics.Shader
{
    public struct TextureDescriptor
    {
        public string Name { get; }

        public TextureTarget Target { get; }

        public int HandleIndex { get; }

        public bool IsBindless { get; }

        public int CbufSlot   { get; }
        public int CbufOffset { get; }

        public TextureDescriptor(string name, TextureTarget target, int hIndex)
        {
            Name        = name;
            Target      = target;
            HandleIndex = hIndex;

            IsBindless = false;

            CbufSlot   = 0;
            CbufOffset = 0;
        }

        public TextureDescriptor(string name, TextureTarget target, int cbufSlot, int cbufOffset)
        {
            Name        = name;
            Target      = target;
            HandleIndex = 0;

            IsBindless = true;

            CbufSlot   = cbufSlot;
            CbufOffset = cbufOffset;
        }
    }
}