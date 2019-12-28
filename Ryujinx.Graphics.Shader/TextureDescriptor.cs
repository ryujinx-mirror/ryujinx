namespace Ryujinx.Graphics.Shader
{
    public struct TextureDescriptor
    {
        public string Name { get; }

        public SamplerType Type { get; }

        public int HandleIndex { get; }

        public bool IsBindless { get; }

        public int CbufSlot   { get; }
        public int CbufOffset { get; }

        public TextureDescriptor(string name, SamplerType type, int handleIndex)
        {
            Name        = name;
            Type        = type;
            HandleIndex = handleIndex;

            IsBindless = false;

            CbufSlot   = 0;
            CbufOffset = 0;
        }

        public TextureDescriptor(string name, SamplerType type, int cbufSlot, int cbufOffset)
        {
            Name        = name;
            Type        = type;
            HandleIndex = 0;

            IsBindless = true;

            CbufSlot   = cbufSlot;
            CbufOffset = cbufOffset;
        }
    }
}