using System;

namespace Ryujinx.Graphics.Shader.CodeGen.Spirv
{
    struct TextureMeta : IEquatable<TextureMeta>
    {
        public int CbufSlot { get; }
        public int Handle { get; }
        public TextureFormat Format { get; }

        public TextureMeta(int cbufSlot, int handle, TextureFormat format)
        {
            CbufSlot = cbufSlot;
            Handle = handle;
            Format = format;
        }

        public override bool Equals(object obj)
        {
            return obj is TextureMeta other && Equals(other);
        }

        public bool Equals(TextureMeta other)
        {
            return CbufSlot == other.CbufSlot && Handle == other.Handle && Format == other.Format;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CbufSlot, Handle, Format);
        }
    }
}