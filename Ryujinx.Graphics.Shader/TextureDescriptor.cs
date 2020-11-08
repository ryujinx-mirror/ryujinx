namespace Ryujinx.Graphics.Shader
{
    public struct TextureDescriptor
    {
        public int Binding { get; }

        public SamplerType Type { get; }

        public TextureFormat Format { get; }

        public int HandleIndex { get; }

        public bool IsBindless { get; }

        public int CbufSlot   { get; }
        public int CbufOffset { get; }

        public TextureUsageFlags Flags { get; set; }

        public TextureDescriptor(int binding, SamplerType type, TextureFormat format, int handleIndex)
        {
            Binding     = binding;
            Type        = type;
            Format      = format;
            HandleIndex = handleIndex;

            IsBindless = false;

            CbufSlot   = 0;
            CbufOffset = 0;

            Flags = TextureUsageFlags.None;
        }

        public TextureDescriptor(int binding, SamplerType type, int cbufSlot, int cbufOffset)
        {
            Binding     = binding;
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