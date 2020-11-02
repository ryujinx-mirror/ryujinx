namespace Ryujinx.Graphics.Shader
{
    public struct TextureDescriptor
    {
        public string Name { get; }

        public SamplerType Type { get; }

        public TextureFormat Format { get; }

        public int HandleIndex { get; }

        public bool IsBindless { get; }

        public int CbufSlot   { get; }
        public int CbufOffset { get; }

        public TextureUsageFlags Flags { get; set; }

        public TextureDescriptor(string name, SamplerType type, TextureFormat format, int handleIndex)
        {
            Name        = name;
            Type        = type;
            Format      = format;
            HandleIndex = handleIndex;

            IsBindless = false;

            CbufSlot   = 0;
            CbufOffset = 0;

            Flags = TextureUsageFlags.None;
        }

        public TextureDescriptor(string name, SamplerType type, int cbufSlot, int cbufOffset)
        {
            Name        = name;
            Type        = type;
            Format      = TextureFormat.Unknown;
            HandleIndex = 0;

            IsBindless = true;

            CbufSlot   = cbufSlot;
            CbufOffset = cbufOffset;

            Flags = TextureUsageFlags.None;
        }

        public TextureDescriptor SetFlag(TextureUsageFlags flag)
        {
            Flags |= flag;

            return this;
        }
    }
}