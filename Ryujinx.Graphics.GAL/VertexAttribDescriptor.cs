namespace Ryujinx.Graphics.GAL
{
    public struct VertexAttribDescriptor
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
    }
}
