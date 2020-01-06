namespace Ryujinx.Graphics.GAL
{
    public struct VertexAttribDescriptor
    {
        public int BufferIndex { get; }
        public int Offset      { get; }

        public Format Format { get; }

        public VertexAttribDescriptor(int bufferIndex, int offset, Format format)
        {
            BufferIndex = bufferIndex;
            Offset      = offset;
            Format      = format;
        }
    }
}
