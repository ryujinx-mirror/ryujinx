using System;

namespace Ryujinx.Graphics.GAL
{
    public struct VertexAttribDescriptor : IEquatable<VertexAttribDescriptor>
    {
        public int BufferIndex { get; }
        public int Offset      { get; }

        public bool IsZero { get; }

        public Format Format { get; }

        public VertexAttribDescriptor(int bufferIndex, int offset, bool isZero, Format format)
        {
            BufferIndex = bufferIndex;
            Offset      = offset;
            IsZero      = isZero;
            Format      = format;
        }

        public override bool Equals(object obj)
        {
            return obj is VertexAttribDescriptor other && Equals(other);
        }

        public bool Equals(VertexAttribDescriptor other)
        {
            return BufferIndex == other.BufferIndex &&
                   Offset      == other.Offset &&
                   IsZero      == other.IsZero &&
                   Format      == other.Format;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BufferIndex, Offset, IsZero, Format);
        }
    }
}
